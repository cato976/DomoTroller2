using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using HAI_Shared;
using DomoTroller2.Common;
using DomoTroller2.Email;
using System.Reflection;
using System.IO;
using System.Net.Mail;
using static DomoTrollerShare2.Enumerations.ClimateControl;
using DomoTrollerShare2.Item;
using System.Collections.Generic;
using System.Linq;
using NestSharp;
using Controller.Common.Command;
using DomoTroller2.ESFramework.Common.Interfaces;
using DomoTroller2.ESFramework.Common.Base;
using DomoTrollerShare2.Interfaces;
using DomoTrollerShare2.EventArguments;

namespace DomoTrollerShare2
{
    [ComVisible(true)]
    public class DomoShare : IHAC
    {
        public delegate void ControllerConnectedHandler(Object sender, ControllerConnectedEventArgs e);
        public event ControllerConnectedHandler ControllerConnected;

        public delegate void ControllerDisconnectedHandler(Object sender, ControllerDisconnectedEventArgs e);
        public event ControllerDisconnectedHandler ControllerDisconnected;

        public delegate void ThermostatConnectedHandler(Object sender, ThermostatConnectedEventArgs e);
        public event ThermostatConnectedHandler ThermostatConnected;

        public delegate void SendEvent(Object sender, ThermostatHeatSetpointChangedEventArgs e);
        public event SendEvent ThermostatHeatSetpointChanged;

        public delegate void CoolSetpointSendEvent(Object sender, ThermostatCoolSetpointChangedEventArgs e);
        public event CoolSetpointSendEvent ThermostatCoolSetpointChanged;

        public delegate void AmbientTemperatureSendEvent(Object sender, ThermostatAmbientTemperatureChangedEventArgs e);
        public event AmbientTemperatureSendEvent ThermostatAmbientTemperatureChanged;

        private static clsHAC HAC = null;
        private static Guid ControllerId = Guid.Empty;
        private static IConfigurationRoot Configuration { get; set; }
        public clsHAC HACPublic { get => HAC; set => throw new NotImplementedException(); }

        private object unitLock = new object();
        private object roomLock = new object();

        private static bool UserDisconnected = false;

        private System.Timers.Timer timerThermostat1 = new System.Timers.Timer(90000);

        private static string LastSecurityMode;
        private int[] tempValue = new int[25];

        private static DateTime LastCalled = DateTime.Now;

        private MailAddress ma;
        private MailMessage mm;

        private bool alarmSent = false;

        int numOfTrouble = 0;

        public DomoShare()
        {
            if (HAC == null)
            {
                Startup();
                ControllerId = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerId").Value);
                HAC = new clsHAC();
                try
                {
                    var t = new Task(() =>
                            {
                                Trace.TraceInformation("Task {0} running on thread {1}", Task.CurrentId, Thread.CurrentThread.ManagedThreadId);
                                Connect();
                            });
                    t.Start();

                    var emailSettings = Configuration.GetSection("AppSettings").GetSection("EmailSettings");
                    var emailAddresses = emailSettings.GetSection("SendEmailsTo")
                        .GetChildren()
                        .Select(x => x.Value)
                        .ToArray();
                    ma = new MailAddress(emailSettings.GetSection("SenderEmail").Value, emailSettings.GetSection("SenderName").Value);
                    mm = new MailMessage(ma, ma);
                    foreach (var email in emailAddresses)
                    {
                        mm.To.Add(email);
                    }
                    mm.Subject = "Monitoring";
                    mm.Body = "Monitoring Started";
                    SendMessage();
                    Initialize();
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error occured: {ex}");
                    throw;
                }
                Trace.TraceInformation("Home Automation monitor has been started");
            }
        }

        protected virtual void OnControllerConnected(ControllerConnectedEventArgs e)
        {
            ControllerConnectedHandler handler = ControllerConnected;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnControllerDisconnected(ControllerDisconnectedEventArgs e)
        {
            ControllerDisconnectedHandler handler = ControllerDisconnected;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnThermostatConnected(ThermostatConnectedEventArgs e)
        {
            ThermostatConnectedHandler handler = ThermostatConnected;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnThermostatHeatSetpointChanged(ThermostatHeatSetpointChangedEventArgs e)
        {
            SendEvent handler = ThermostatHeatSetpointChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnThermostatCoolSetpointChanged(ThermostatCoolSetpointChangedEventArgs e)
        {
            CoolSetpointSendEvent handler = ThermostatCoolSetpointChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnThermostatAmbientTemperatureChanged(ThermostatAmbientTemperatureChangedEventArgs e)
        {
            AmbientTemperatureSendEvent handler = ThermostatAmbientTemperatureChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Connect()
        {
            if (HAC != null)
            {
                if (HAC.Connection.ConnectionState == enuOmniLinkConnectionState.Offline)
                {
                    HAC.Connection.NetworkAddress = Configuration.GetSection("AppSettings").GetSection("ControllerUrl").Value;
                    HAC.Connection.NetworkPort = ushort.Parse(Configuration.GetSection("AppSettings").GetSection("ControllerPort").Value);

                    Trace.TraceInformation($"Connecting to: {HAC.Connection.NetworkAddress} Port {HAC.Connection.NetworkPort}");

                    HAC.Connection.ControllerKey = clsUtil.HexString2ByteArray(string.Concat(Configuration.GetSection("AppSettings").GetSection("ControllerKey1").Value, Configuration.GetSection("AppSettings").GetSection("ControllerKey2").Value));
                    HAC.PreferredNetworkProtocol = (clsHAC.enuPreferredNetworkProtocol)clsHAC.enuPreferredNetworkProtocol.TCP;

                    if (HAC.PreferredNetworkProtocol == clsHAC.enuPreferredNetworkProtocol.TCP)
                    {
                        HAC.Connection.ConnectionType = enuOmniLinkConnectionType.Network_TCP;
                    }

                    try
                    {
                        while (HAC.Connection.ConnectionState != enuOmniLinkConnectionState.OnlineSecure)
                        {
                            Thread.Sleep(2000);
                            HAC.Connection.Connect(HandleConnectStatus, HandleUnsolicitedPackets);

                            if (HAC.Connection.ConnectionState == enuOmniLinkConnectionState.OnlineSecure)
                            {
                                StartPolling(enuOmniLinkCommStatus.Connecting);

                                Trace.TraceInformation("SENDING AREA REQUEST");
                                clsOL2MsgRequestExtendedStatus status = null;

                                status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
                                status.ObjectType = enuObjectType.Area;
                                status.StartingNumber = 1;
                                status.EndingNumber = HAC.NumAreasUsed;

                                HAC.Connection.Send(status, null, HandleExtendedStatus);

                                Trace.TraceInformation("SENDING THERMOSTAT REQUEST");

                                status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
                                status.ObjectType = enuObjectType.Thermostat;
                                status.StartingNumber = 1;
                                status.EndingNumber = HAC.NumThermostats;

                                HAC.Connection.Send(status, HandleRequestExtentedThermostatStatus);


                                Trace.TraceInformation("SENDING ZONE REQUEST");

                                status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
                                status.ObjectType = enuObjectType.Zone;
                                status.StartingNumber = 1;
                                status.EndingNumber = 1;

                                status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
                                status.ObjectType = enuObjectType.Zone;
                                status.StartingNumber = 1;
                                status.EndingNumber = (ushort)HAC.Zones.Count;
                                HAC.Connection.Send(status, HandleExtendedStatus);

                                Trace.TraceInformation("SENDING UNIT REQUEST");
                                status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
                                status.ObjectType = enuObjectType.Unit;
                                status.StartingNumber = 1;
                                status.EndingNumber = (ushort)HAC.Units.Count;
                                HAC.Connection.Send(status, HandleExtendedStatus);

                                Trace.TraceInformation("SENDING BUTTON REQUEST");
                                status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
                                status.ObjectType = enuObjectType.Button;
                                status.StartingNumber = 1;
                                status.EndingNumber = (ushort)HAC.Buttons.Count;
                                HAC.Connection.Send(status, HandleExtendedStatus);

                                Trace.TraceInformation("SENDING USER SETTINGS REQUEST");
                                status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
                                status.ObjectType = enuObjectType.UserSetting;
                                status.StartingNumber = 1;
                                status.EndingNumber = (ushort)HAC.UserSettings.Count;
                                HAC.Connection.Send(status, HandleExtendedStatus);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Exception: {ex}");
                    }
                }
            }
        }

        private void StartPolling(enuOmniLinkCommStatus connecting)
        {
            Trace.TraceInformation("Reading Config.");
            HAC.CommReadConfig(clsHAC.enuConfigItems.All, DisplayStatus);


            while (HAC.CommReadConfigInProgress)
            {
                Thread.Sleep(500);
            }

            int halfZones = HAC.Zones.Count / 2;

            new Thread(() =>
            {
                    //ready |= Ready.Thread1;
                }).Start();

            new Thread(() =>
            {
                    //ready |= Ready.Thread2;
                }).Start();

            //timerThermostat1_Elapsed(null, null);
            //timerThermostat1_Elapsed(null, null);
            timerThermostat1.Elapsed += timerThermostat1_Elapsed;

            timerThermostat1.Start();

            clsOmniLink2Message msg = new clsOmniLink2Message(HAC.Connection);
            msg.MessageType = enuOmniLink2MessageType.EnableNotifications;
            HAC.Connection.Send(new clsOmniLink2Message(HAC.Connection, enuOmniLink2MessageType.EnableNotifications), HandleExtendedStatus);
        }

        private void DisplayStatus(clsHAC.enuConfigPhase Phase, int ix, ref bool Cancel)
        {
            // Display the status
            if (ix > 0)
            {
                Trace.TraceInformation(String.Format("Reading {0} ({1})",
                    Phase.ToString(),
                    ix));
            }
            else
            {
                Trace.TraceInformation("Reading " + Phase.ToString());
                if (Phase == clsHAC.enuConfigPhase.Done)
                {
                    //ready |= Ready.Done;
                }
            }
        }

        private void Initialize()
        {
            Task.Run(() =>
            {
                GetAreas();
                GetThermostats();
                GetZones();
                GetRooms();
            });
        }

        private List<Item.Room> GetRooms()
        {
            Status status = new Status();
            var rr = new Room()
            {
                ID = 0,
                Name = "Unassigned",
                Number = 0
            };
            status.Rooms.Add(rr);

            for (int u = 1; u < HAC.Units.Count; u++)
            {
                if (HAC.Units[u].Type == enuOL2UnitType.HLCRoom && HAC.Units[u].Number % 8 == 1)
                {
                    rr = new Room()
                    {
                        ID = u,
                        Name = HAC.Units[u].Name,
                        Number = HAC.Zones[u].Number
                    };
                    status.Rooms.Add(rr);
                }

                //var ro = status.Rooms.Find(room => room.ID == u);
                if (rr != null && rr.Number == HAC.Units[u].Number)
                {
                    SetupUnit(rr, u);
                }
                else if (rr != null && rr.Number != HAC.Units[u].Number)
                {
                    Unit unit = new Unit();
                    SetupUnit(unit, u);
                    rr.Units.Add(unit);
                }
            }

            return status.Rooms;
        }

        private void SetupUnit(Room rr, int u)
        {
            lock (roomLock)
            {
                rr.Name = HAC.Units[u].Name;
                rr.ID = u;
                if ((HAC.Units[u].Status) != 0)
                {
                    rr.Status = "ON";
                }
                else
                {
                    if (rr != null)
                    {
                        rr.Status = "OFF";
                    }
                }
                if (HAC.Units[u].Status > 100)
                {
                    rr.Level = HAC.Units[u].Status - 100;
                    rr.Status = String.Format("Light Level {0}%", rr.Level);
                }
                else
                {
                    if (rr != null)
                    {
                        rr.Level = HAC.Units[u].Status;
                    }
                }
                rr.Time = HAC.Units[u].StatusTime;
            }
        }

        private void SetupUnit(Unit rr, int u)
        {
            lock (roomLock)
            {
                rr.Name = HAC.Units[u].Name;
                rr.ID = u;
                if ((HAC.Units[u].Status) != 0)
                {
                    rr.Status = "ON";
                }
                else
                {
                    if (rr != null)
                    {
                        rr.Status = "OFF";
                    }
                }
                if (HAC.Units[u].Status > 100)
                {
                    rr.Level = HAC.Units[u].Status - 100;
                    rr.Status = String.Format("Light Level {0}%", rr.Level);
                }
                else
                {
                    if (rr != null)
                    {
                        rr.Level = HAC.Units[u].Status;
                    }
                }
                rr.Time = HAC.Units[u].StatusTime;
            }
        }
        private static List<Item.Zone> GetZones()
        {
            Status status = new Status();

            for (int z = 1; z < HAC.Zones.Count; z++)
            {
                status.Zones.Add(new Zone()
                {
                    ID = z,
                    Name = HAC.Zones[z].Name,
                    Number = HAC.Zones[z].Number
                });

                var zn = status.Zones.Find(zone => zone.ID == z);
                switch ((((HAC.Zones[z].Status) >> 2) & 0x3))
                {
                    case 0:
                        zn.CurrentStatus = "SECURE";
                        break;
                    case 1:
                        zn.CurrentStatus = "TRIPPED";
                        break;
                    case 2:
                        zn.CurrentStatus = "RESET, BUT PREVIOUSLY TRIPPED";
                        break;
                    default:
                        break;
                }

                switch ((((HAC.Zones[z].Status) >> 4) & 0x3))
                {
                    case 0:
                        zn.ArmingStatus = "DISARMED";
                        break;
                    case 1:
                        zn.ArmingStatus = "ARMED";
                        break;
                    case 2:
                        zn.ArmingStatus = "BYPASSED BY USER";
                        break;
                    case 3:
                        zn.ArmingStatus = "BYPASSED BY SYSTEM";
                        break;
                    default:
                        break;
                }
            }

            return status.Zones;
        }

        private static List<Item.Area> GetAreas()
        {
            Status status = new Status();

            for (int a = 0; a < HAC.Areas.Count; a++)
            {
                status.Areas.Add(new Area()
                {
                    ID = a,
                    Name = HAC.Areas[a].Name,
                    SecurityMode = HAC.Areas[a].AreaMode.ToString()
                });
            }

            return status.Areas;
        }

        private bool HandleUnsolicitedPackets(byte[] B)
        {
            if ((B.Length > 3) && (B[0] == 0x21))
            {
                switch ((enuOmniLink2MessageType)B[2])
                {
                    case enuOmniLink2MessageType.ClearNames:
                        break;
                    case enuOmniLink2MessageType.DownloadNames:
                        break;
                    case enuOmniLink2MessageType.UploadNames:
                        break;
                    case enuOmniLink2MessageType.NameData:
                        break;
                    case enuOmniLink2MessageType.ClearVoices:
                        break;
                    case enuOmniLink2MessageType.DownloadVoices:
                        break;
                    case enuOmniLink2MessageType.UploadVoices:
                        break;
                    case enuOmniLink2MessageType.VoiceData:
                        break;
                    case enuOmniLink2MessageType.Command:
                        break;
                    case enuOmniLink2MessageType.EnableNotifications:
                        break;
                    case enuOmniLink2MessageType.SystemInformation:
                        break;
                    case enuOmniLink2MessageType.SystemStatus:
                        break;
                    case enuOmniLink2MessageType.SystemTroubles:
                        break;
                    case enuOmniLink2MessageType.SystemFeatures:
                        break;
                    case enuOmniLink2MessageType.Capacities:
                        break;
                    case enuOmniLink2MessageType.Properties:
                        break;
                    case enuOmniLink2MessageType.Status:
                        break;
                    case enuOmniLink2MessageType.EventLogItem:
                        break;
                    case enuOmniLink2MessageType.ValidateCode:
                        break;
                    case enuOmniLink2MessageType.SystemFormats:
                        break;
                    case enuOmniLink2MessageType.Login:
                        break;
                    case enuOmniLink2MessageType.Logout:
                        break;
                    case enuOmniLink2MessageType.ActivateKeypadEmg:
                        break;
                    case enuOmniLink2MessageType.ExtSecurityStatus:
                        break;
                    case enuOmniLink2MessageType.CmdExtSecurity:
                        break;
                    case enuOmniLink2MessageType.AudioSourceStatus:
                        break;
                    case enuOmniLink2MessageType.SystemEvents:
                        HandleSystemEvents(new clsOmniLinkMessageQueueItem(), B, false);
                        break;
                    case enuOmniLink2MessageType.ZoneReadyStatus:
                        break;
                    case enuOmniLink2MessageType.ExtendedStatus:
                        //case enuOmniLink2MessageType.SystemEvents:
                        HandleExtendedStatus(new clsOmniLinkMessageQueueItem(), B, false);
                        break;
                    default:
                        Trace.TraceInformation("UNSOLICITED: " +
                            ((enuOmniLink2MessageType)B[2]).ToString());
                        break;
                }
            }
            return true;
        }

        private void HandleExtendedStatus(clsOmniLinkMessageQueueItem clsOmniLinkMessageQueueItem, byte[] bytes, bool timeout)
        {
            if (timeout)
            {
                return;  // just use the default timeout retry sequence
            }


            clsOL2MsgExtendedStatus MSG = null;

            // turn the byte array into a message for ease of handling

            MSG = new clsOL2MsgExtendedStatus(HAC.Connection, bytes);

            // if there is an alarm, send an email
            if (MSG != null)
            {
                int unitNumber;

                if (MSG.MessageType != enuOmniLink2MessageType.Ack) // && alarmSent == false)
                {
                    switch (MSG.ObjectType)
                    {
                        case enuObjectType.Unit:

                            for (int i = 0; i < MSG.UnitStatusCount(); i++)
                            {

                                unitNumber = MSG.Data[3 + (5 * i)] << 8;
                                unitNumber += MSG.Data[4 + (5 * i)];

                                HAC.Units[unitNumber].Status = MSG.UnitStatus((byte)i);

                                int level;
                                string status;
                                int time;
                                if (MSG.UnitStatus((byte)i) != 0)
                                {
                                    status = "ON";
                                }
                                else
                                {
                                    status = "OFF";
                                }
                                if (MSG.UnitStatus((byte)i) > 100)
                                {
                                    level = MSG.UnitStatus((byte)i) - 100;
                                    status = String.Format("Light Level {0}%", level);
                                }
                                else
                                {
                                    level = MSG.UnitStatus((byte)i);
                                }
                                time = MSG.Data[6 + (5 * i)] << 8;
                                time += MSG.Data[7 + (5 * i)];

                                if ((unitNumber > 10) && (unitNumber < 17))
                                {
                                    break;
                                }
                                if (unitNumber < 393)
                                {
                                    Trace.TraceInformation(String.Format("{0}: Unit change unit {1}, {2}, status {3}, time {4}", DateTime.Now.ToShortTimeString(), unitNumber, HAC.Units[unitNumber].Name, status, time));

                                    // create a writer and open the file
                                    //using (TextWriter tw = new StreamWriter("data.txt", true))
                                    //{
                                    //    // write a line of text to the file
                                    //    tw.WriteLine(String.Format("{0}: Unit change unit {1}, {2}, status {3}, time {4}", DateTime.Now.ToShortTimeString(), unitNumber, HAC.Units[unitNumber].Name, status, time));

                                    //    // close the stream
                                    //    tw.Close();
                                    //}
                                }
                                else if (unitNumber > 392)
                                {
                                    if (unitNumber == 399 && MSG.UnitStatus((byte)i) == 1)
                                    {
                                        //Get7DayForcast();
                                    }
                                    else if (unitNumber == 400 && MSG.UnitStatus((byte)i) == 1)
                                    {
                                        //GetCurrentCoditions();
                                    }
                                }

                            }
                            break;
                        case enuObjectType.Thermostat:
                            for (int i = 0; i < MSG.ThermostatStatusCount(); i++)
                            {
                                unitNumber = MSG.Data[3 + (14 * i)] << 8;
                                unitNumber += MSG.Data[4 + (14 * i)];
                                string fanStatus = String.Empty;
                                timerThermostat1.Start();
                                switch (MSG.Data[16 + (14 * i)] & 0x0f)
                                {
                                    case 0:
                                        fanStatus = "OFF";
                                        break;
                                    case 1:
                                        fanStatus = "HEATING";
                                        break;
                                    case 2:
                                        fanStatus = "COOLING";
                                        break;
                                    case 10:
                                        fanStatus = "COOLING, DEHUMIDIFYING";
                                        break;
                                }
                                timerThermostat1_Elapsed(this, null);

                                Trace.TraceInformation(String.Format("{0}: Thermostat change thermostat {1}, {2}, Mode {3}, Fan {4}, Heat setpoint {5}°, Cool setpoint {6}°, Hold {7}, Current Temp. {8}°, ",
                                    DateTime.Now.ToShortTimeString(), unitNumber, "TSTAT", MSG.SystemMode((byte)i).ToString(),
                                    MSG.FanMode((byte)i).ToString(), clsText.DecodeTemp(MSG.HeatSetpoint((byte)i), HAC.TempFormat, HAC), clsText.DecodeTemp(MSG.CoolSetpoint((byte)i), HAC.TempFormat, HAC),
                                    MSG.HoldStatus((byte)i).ToString(), clsText.DecodeTemp(MSG.CurrentTemp((byte)i), HAC.TempFormat, HAC)));
                                Trace.TraceInformation(String.Format("Outdoor Temp. {0}°, {1}", clsText.DecodeTemp(MSG.OutdoorTemp((byte)i), HAC.TempFormat, HAC), fanStatus));


                                // create a writer and open the file
                                //using (TextWriter tw = new StreamWriter("data.txt", true))
                                //{
                                //    // write a line of text to the file
                                //    tw.WriteLine(String.Format("{0}: Thermostat change thermostat {1}, {2}, Mode {3}, Fan {4}, Heat setpoint {5}°, Cool setpoint {6}°, Hold {7}, Current Temp. {8}°, Outdoor Temp. {9}°, {10}",
                                //    DateTime.Now.ToShortTimeString(), unitNumber, "TSTAT", MSG.SystemMode((byte)i).ToString(),
                                //    MSG.FanMode((byte)i).ToString(), clsText.DecodeTemp(MSG.HeatSetpoint((byte)i), HAC.TempFormat, HAC), clsText.DecodeTemp(MSG.CoolSetpoint((byte)i), HAC.TempFormat, HAC),
                                //    MSG.HoldStatus((byte)i).ToString(), clsText.DecodeTemp(MSG.CurrentTemp((byte)i), HAC.TempFormat, HAC), clsText.DecodeTemp(MSG.OutdoorTemp((byte)i), HAC.TempFormat, HAC), fanStatus));

                                //    // close the stream
                                //    tw.Close();
                                //}
                            }
                            break;
                        case enuObjectType.Zone:
                            for (int i = 0; i < MSG.ZoneStatusCount(); i++)
                            {
                                unitNumber = MSG.Data[3 + (4 * i)] << 8;
                                unitNumber += MSG.Data[4 + (4 * i)];
                                string zoneStatus;
                                string latchAlarmStatus = String.Empty;
                                string armingStatus = String.Empty;
                                string troubleStatus = String.Empty;

                                HAC.Zones[unitNumber].Status = MSG.ZoneStatus((byte)i);

                                switch (MSG.ZoneStatus((byte)i) & 0x3)
                                {
                                    case 0:
                                        zoneStatus = "SECURE";
                                        break;
                                    case 1:
                                        zoneStatus = "NOT READY";
                                        break;
                                    case 2:
                                        zoneStatus = "TROUBLE";
                                        HAC.Connection.Send(new clsOL2MsgRequestSystemTroubles(HAC.Connection), HandleRequestSystemTrouble);
                                        break;
                                    default:
                                        zoneStatus = "UNKNOW";
                                        break;
                                }

                                switch ((MSG.ZoneStatus((byte)i) >> 2) & 0x3)
                                {
                                    case 0:
                                        latchAlarmStatus = "SECURE";
                                        break;
                                    case 1:
                                        latchAlarmStatus = "TRIPPED";
                                        break;
                                    case 2:
                                        latchAlarmStatus = "RESET, BUT PREVIOUSLY TRIPPED";
                                        break;
                                    default:
                                        latchAlarmStatus = "UNKNOW";
                                        break;
                                }

                                switch ((MSG.ZoneStatus((byte)i) >> 4) & 0x3)
                                {
                                    case 0:
                                        armingStatus = "DISARMED";
                                        break;
                                    case 1:
                                        armingStatus = "ARMED";
                                        break;
                                    case 2:
                                        armingStatus = "BYPASSED BY USER";
                                        break;
                                    case 3:
                                        armingStatus = "BYPASSED BY SYSTEM";
                                        break;
                                    default:
                                        armingStatus = "UNKNOW";
                                        break;
                                }

                                switch ((MSG.ZoneStatus((byte)i) >> 6) & 0x3)
                                {
                                    case 0:
                                        troubleStatus = String.Empty;
                                        break;
                                    case 1:
                                        troubleStatus = "TROUBLE NOT ACKNOWLEDGED";
                                        HAC.Connection.Send(new clsOL2MsgRequestSystemTroubles(HAC.Connection), HandleRequestSystemTrouble);
                                        break;
                                    default:
                                        troubleStatus = "UNKNOWN";
                                        break;
                                }
                                string name = HAC.Zones[unitNumber].Name;

                                Trace.TraceInformation(String.Format("{0}: Zone change zone {1}, {2}, status {3}, latch alarm status {4}, arming status {5}, trouble status {6}",
                                    DateTime.Now.ToShortTimeString(), unitNumber, name, zoneStatus, latchAlarmStatus, armingStatus, troubleStatus));
                            }
                            break;
                        case enuObjectType.Area:
                            for (byte i = 0; i < MSG.AreaCount(); i++)
                            {
                                unitNumber = MSG.Data[3 + (6 * i)] << 8;
                                unitNumber += MSG.Data[4 + (6 * i)];

                                HAC.Areas[unitNumber].AreaMode = MSG.Mode((byte)i);
                                HAC.Areas[unitNumber].AreaAlarms = MSG.AreaAlarms((byte)i);
                                string burglaryAlarm, fireAlarm, gasAlarm, auxilaryAlarm, freezeAlarm, waterAlarm, duressAlarm, TempAlarm, totalAlarm;

                                if (MSG.AreaAlarms((byte)i) >= 1)
                                {
                                    HAC.Connection.Send(new clsOL2MsgRequestSystemStatus(HAC.Connection), HandleRequestAlarmStatus);
                                    Trace.TraceInformation(MSG.Mode(i).ToString());
                                }
                                else
                                {
                                    if (LastSecurityMode == MSG.Mode((byte)i).ToString())
                                    {
                                        HAC.Connection.Send(new clsOL2MsgRequestSystemStatus(HAC.Connection), HandleRequestAlarmStatus2);
                                        //Trace.TraceInformation(MSG.Mode(i).ToString());
                                    }
                                }

                                switch ((MSG.AreaAlarms((byte)i)) & 0x1)
                                {
                                    case 0:
                                        burglaryAlarm = String.Empty;
                                        break;
                                    case 1:
                                        burglaryAlarm = "Burglary alarm";
                                        break;
                                    default:
                                        burglaryAlarm = "UNKNOWN";
                                        break;
                                }
                                switch ((MSG.AreaAlarms((byte)i)) >> 1 & 0x1)
                                {
                                    case 0:
                                        fireAlarm = String.Empty;
                                        break;
                                    case 1:
                                        fireAlarm = "Fire alarm";
                                        break;
                                    default:
                                        fireAlarm = "UNKNOWN";
                                        break;
                                }
                                switch ((MSG.AreaAlarms((byte)i)) >> 2 & 0x1)
                                {
                                    case 0:
                                        gasAlarm = String.Empty;
                                        break;
                                    case 1:
                                        gasAlarm = "Gas alarm";
                                        break;
                                    default:
                                        gasAlarm = "UNKNOWN";
                                        break;
                                }
                                switch ((MSG.AreaAlarms((byte)i)) >> 3 & 0x1)
                                {
                                    case 0:
                                        auxilaryAlarm = String.Empty;
                                        break;
                                    case 1:
                                        auxilaryAlarm = "Auxiliary alarm";
                                        break;
                                    default:
                                        auxilaryAlarm = "UNKNOWN";
                                        break;
                                }
                                switch ((MSG.AreaAlarms((byte)i)) >> 4 & 0x1)
                                {
                                    case 0:
                                        freezeAlarm = String.Empty;
                                        break;
                                    case 1:
                                        freezeAlarm = "Freeze alarm";
                                        break;
                                    default:
                                        freezeAlarm = "UNKNOWN";
                                        break;
                                }
                                switch ((MSG.AreaAlarms((byte)i)) >> 5 & 0x1)
                                {
                                    case 0:
                                        waterAlarm = String.Empty;
                                        break;
                                    case 1:
                                        waterAlarm = "Water alarm";
                                        break;
                                    default:
                                        waterAlarm = "UNKNOWN";
                                        break;
                                }
                                switch ((MSG.AreaAlarms((byte)i)) >> 6 & 0x1)
                                {
                                    case 0:
                                        duressAlarm = String.Empty;
                                        break;
                                    case 1:
                                        duressAlarm = "Duress alarm";
                                        break;
                                    default:
                                        duressAlarm = "UNKNOWN";
                                        break;
                                }
                                switch ((MSG.AreaAlarms((byte)i)) >> 7 & 0x1)
                                {
                                    case 0:
                                        TempAlarm = String.Empty;
                                        break;
                                    case 1:
                                        TempAlarm = "Temperature alarm";
                                        break;
                                    default:
                                        TempAlarm = "UNKNOWN";
                                        break;
                                }

                                totalAlarm = burglaryAlarm + " " + fireAlarm + " " + gasAlarm + " " + auxilaryAlarm + " " + freezeAlarm + " " + waterAlarm + " " + duressAlarm + " " + TempAlarm;

                                if (MSG.AreaAlarms((byte)i) >= 1)
                                {
                                    Trace.TraceInformation(String.Format("{0}: Area change area {1}, security mode {2}, Alarm {3}",
                                        DateTime.Now.ToShortTimeString(), unitNumber, MSG.Mode((byte)i).ToString(), totalAlarm));
                                }
                                else
                                {
                                    if (LastSecurityMode != MSG.Mode((byte)i).ToString())
                                    {
                                        LastSecurityMode = MSG.Mode((byte)i).ToString();
                                        Trace.TraceInformation(String.Format("{0}: Area change area {1}, security mode {2}, Alarm {3}",
                                        DateTime.Now.ToShortTimeString(), unitNumber, MSG.Mode((byte)i).ToString(), totalAlarm));
                                    }
                                }
                            }
                            break;
                        case enuObjectType.UserSetting:
                            for (int i = 0; i < MSG.UserSettingStatusCount(); i++)
                            {
                                unitNumber = MSG.Data[3 + (5 * i)] << 8;
                                unitNumber += MSG.Data[4 + (5 * i)];
                                string settingType = String.Empty;
                                string value = String.Empty;
                                string name = String.Empty;
                                switch (MSG.UserSettingType((byte)(i)))
                                {
                                    case 3:
                                        settingType = "Temperature";
                                        value = clsText.DecodeTemp((byte)MSG.UserSettingStatus((byte)i), HAC.TempFormat, HAC);

                                        tempValue[i] = int.Parse(value);
                                        HAC.UserSettings[i + 1].Status = MSG.UserSettingStatus((byte)i);
                                        break;
                                    case 6:
                                        settingType = "Time";
                                        value = MSG.UserSettingStatus((byte)(i)).ToString();
                                        break;
                                    case 7:
                                        settingType = "Day Of the Week";
                                        value = MSG.UserSettingStatus((byte)(i)).ToString();
                                        break;
                                    case 8:
                                        settingType = "Level";
                                        value = MSG.UserSettingStatus((byte)(i)).ToString();
                                        break;
                                }
                                name = HAC.UserSettings[i + 1].Name;

                                Trace.TraceInformation(String.Format("{0}: User setting change user setting {1}, setting type {2}, value {3}", DateTime.Now.ToShortTimeString(), unitNumber, settingType, value));

                                // create a writer and open the file
                                //using (TextWriter tw = new StreamWriter("data.txt", true))
                                //{
                                //    // write a line of text to the file
                                //    tw.WriteLine(String.Format("{0}: User setting change user setting {1}, setting type {2}, value {3}", DateTime.Now.ToShortTimeString(), unitNumber, settingType, value));

                                //    // close the stream
                                //    tw.Close();
                                //}
                            }
                            break;

                        case enuObjectType.Auxillary:
                            for (int i = 0; i < MSG.AuxStatusCount(); i++)
                            {
                                unitNumber = MSG.Data[3 + (6 * i)] << 8;
                                unitNumber += MSG.Data[4 + (6 * i)];
                                if (HAC.Zones[unitNumber].ZoneType == enuZoneType.Temperature)
                                {
                                    Trace.TraceInformation(String.Format("{0}: Auxillary change zone {1}, {2}, temperture. {3}°", DateTime.Now.ToShortTimeString(), unitNumber, HAC.Zones[unitNumber].Name /*"Sensor"*/, clsText.DecodeTemp(MSG.Aux_Temp((byte)i), HAC.TempFormat, HAC)));
                                }
                                else if (HAC.Zones[unitNumber].ZoneType == enuZoneType.Humidity)
                                {
                                    Trace.TraceInformation(String.Format("{0}: Auxillary change zone {1}, {2}, humidity. {3}%", DateTime.Now.ToShortTimeString(), unitNumber, HAC.Zones[unitNumber].Name /*"Sensor"*/, clsText.DecodeTemp(MSG.Aux_Temp((byte)i), HAC.TempFormat, HAC)));
                                }

                                // create a writer and open the file
                                //using (TextWriter tw = new StreamWriter("data.txt", true))
                                //{
                                //    // write a line of text to the file
                                //    tw.WriteLine(String.Format("{0}: Auxillary change zone {1}, {2}, temp. {3}°", DateTime.Now.ToShortTimeString(), unitNumber, HAC.Zones[unitNumber].Name /*"Sensor"*/, clsText.DecodeTemp(MSG.Aux_Temp((byte)i), HAC.TempFormat, HAC)));

                                //    // close the stream
                                //    tw.Close();
                                //}
                            }
                            break;
                        case enuObjectType.Button:
                            Trace.TraceInformation("button pushed");
                            break;
                        case enuObjectType.Message:
                            Trace.TraceInformation("Message");
                            break;
                        case enuObjectType.Invalid:
                            Trace.TraceInformation("Invalid");
                            break;
                        default:
                            Trace.TraceInformation("Unknown");
                            break;
                    }
                }
            }
        }

        private void timerThermostat1_Elapsed(Object domoShare, System.Timers.ElapsedEventArgs p)
        {
            Trace.TraceInformation("Requesting Thermostat 1 status.");

            clsOL2MsgRequestExtendedStatus status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
            GetStatus(enuOmniLinkCommStatus.Connecting);
            status.ObjectType = enuObjectType.Thermostat;
            status.StartingNumber = 1;
            status.EndingNumber = 1;
            HAC.Connection.Send(status, HandleRequestExtentedThermostatStatus);

            status.ObjectType = enuObjectType.UserSetting;
            status.StartingNumber = 1;
            status.EndingNumber = (ushort)HAC.UserSettings.Count;


            try
            {
                Status stat = new Status();
                var nests = GetNestThermostats(stat);
                Task.WaitAll(nests);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error getting Nest Thermostats: {ex}");
                timerThermostat1.Start();
                return;
            }

            timerThermostat1.Stop();
        }

        private void HandleRequestExtentedThermostatStatus(clsOmniLinkMessageQueueItem M, byte[] B, bool Timeout)
        {
            if (Timeout)
            {
                return;  // just use the default timeout retry sequence
            }

            // turn the byte array into a message for ease of handling
            clsOL2MsgExtendedStatus MSG = new clsOL2MsgExtendedStatus(HAC.Connection, B);
            HAC.Thermostats[MSG.ObjectNumber(0)].Temp = MSG.CurrentTemp(0);
            HAC.Thermostats[MSG.ObjectNumber(0)].Mode = MSG.SystemMode(0);
            HAC.Thermostats[MSG.ObjectNumber(0)].CoolSetpoint = MSG.CoolSetpoint(0);
            HAC.Thermostats[MSG.ObjectNumber(0)].HeatSetpoint = MSG.HeatSetpoint(0);
            HAC.Thermostats[MSG.ObjectNumber(0)].FanMode = MSG.FanMode(0);
            HAC.Thermostats[MSG.ObjectNumber(0)].HoldStatus = MSG.HoldStatus(0);
            HAC.Thermostats[MSG.ObjectNumber(0)].OutdoorTemp = MSG.OutdoorTemp(0);
            HAC.Thermostats[MSG.ObjectNumber(0)].HorC_Status = MSG.HorC_Status(0);

            // display in log
            Trace.TraceInformation(String.Format("{0}: {1} Current Temperature Is {2}°: Outdoor Temperature is {3}°: Current Heatpoint setting is {4}° : Current Coolpoint setting is {5}°",
                DateTime.Now.ToShortTimeString(),
                HAC.Thermostats[MSG.ObjectNumber(0)].Name,
                clsText.DecodeTemp(HAC.Thermostats[MSG.ObjectNumber(0)].Temp, HAC.TempFormat, HAC), clsText.DecodeTemp(HAC.Thermostats[MSG.ObjectNumber(0)].OutdoorTemp, HAC.TempFormat, HAC), clsText.DecodeTemp(HAC.Thermostats[MSG.ObjectNumber(0)].HeatSetpoint, HAC.TempFormat, HAC), clsText.DecodeTemp(HAC.Thermostats[MSG.ObjectNumber(0)].CoolSetpoint, HAC.TempFormat, HAC)));

            GetNextThermostat(MSG.ObjectNumber(0));
        }

        private void GetNextThermostat(ushort index)
        {
            clsOL2MsgRequestProperties MSG = new clsOL2MsgRequestProperties(HAC.Connection);
            MSG.ObjectType = enuObjectType.Thermostat;
            MSG.IndexNumber = (UInt16)index;
            MSG.RelativeDirection = 1;  // next object after IndexNumber
            MSG.Filter1 = 1;  // (0=Named or Unnamed, 1=Named, 2=Unnamed).
            MSG.Filter2 = 0;  // Any Area
            MSG.Filter3 = 0;  // Any Room
            HAC.Connection.Send(MSG, HandleThermostatPropertiesResponse);
        }

        private void HandleThermostatPropertiesResponse(clsOmniLinkMessageQueueItem M, byte[] B, bool Timeout)
        {
            if (Timeout)
            {
                return;
            }

            // does it look like a valid response
            if ((B.Length > 3) && (B[0] == 0x21))
            {
                switch ((enuOmniLink2MessageType)B[2])
                {
                    case enuOmniLink2MessageType.EOD:
                        // End of data, we are done...
                        break;
                    case enuOmniLink2MessageType.Properties:
                        // create a new message from byte array for ease of processing
                        clsOL2MsgProperties MSG = new clsOL2MsgProperties(
                            HAC.Connection, B);
                        // copy the zone properties into the zone list
                        HAC.Thermostats.CopyProperties(MSG);
                        GetNextThermostat(MSG.ObjectNumber);
                        break;
                    default:
                        break;
                }
            }
        }

        private void GetStatus(enuOmniLinkCommStatus connecting)
        {
            clsOL2MsgRequestExtendedStatus status = null;

            status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
            status.ObjectType = enuObjectType.Area;
            status.StartingNumber = 1;
            status.EndingNumber = 1;

            HAC.Connection.Send(status, null, HandleExtendedStatus);
        }

        private void HandleRequestAlarmStatus2(clsOmniLinkMessageQueueItem M, byte[] B, bool Timeout)
        {
            if (Timeout)
            {
                return;  // just use the default timeout retry sequence
            }

            clsOL2MsgSystemStatus MSG = null;

            // turn the byte array into a message for ease of handling

            MSG = new clsOL2MsgSystemStatus(HAC.Connection, B);

            // if there is an alarm, send an email
            if (MSG != null)
            {

                //lblTime.Text = string.Format("{0}:{1}:{2}", MSG.Hour, String.Format("{0:00}", (MSG.Minute)), String.Format("{0:00}", MSG.Second));
                try
                {
                    DateTime d = new DateTime(MSG.Year + 2000, MSG.Month, MSG.Day);
                    //lblDate.Text = string.Format("{0} {1}", d.DayOfWeek, d.ToShortDateString());
                    //lblSunrise.Text = string.Format("{0} {1}:{2:00}", DateTime.Now.DayOfWeek, MSG.SunriseHour, MSG.SunriseMinute);
                    //lblSunset.Text = string.Format("{0} {1}:{2:00}", DateTime.Now.DayOfWeek, MSG.SunsetHour, MSG.SunsetMinute);
                    clsOL2MsgRequestExtendedStatus status = null;
                    status = new clsOL2MsgRequestExtendedStatus(HAC.Connection);
                    status.ObjectType = enuObjectType.Area;
                    status.StartingNumber = 1;
                    status.EndingNumber = HAC.NumAreasUsed;

                    TimeSpan ts = DateTime.Now - LastCalled;
                    if (ts.Milliseconds > 50)
                    {
                        HAC.Connection.Send(status, null, HandleExtendedStatus);
                        LastCalled = DateTime.Now;
                    }

                }
                catch
                {
                }
            }
        }

        private void HandleRequestAlarmStatus(clsOmniLinkMessageQueueItem M, byte[] B, bool Timeout)
        {
            if (Timeout)
            {
                return;  // just use the default timeout retry sequence
            }

            clsOL2MsgSystemStatus MSG = null;

            // turn the byte array into a message for ease of handling

            MSG = new clsOL2MsgSystemStatus(HAC.Connection, B);

            // if there is an alarm, send an email
            if (MSG != null)
            {

                if (MSG.AreaAlarms != null /*&& alarmSent == false*/)
                {
                    var emailAddresses = Configuration.GetSection("AppSettings").GetSection("SendEmailsTo");
                    ma = new MailAddress("andrecato@comcast.net", "House");
                    mm = new MailMessage(ma, ma);
                    mm.To.Add("5612813999@messaging.sprintpcs.com");
                    mm.To.Add("7545812745@messaging.sprintpcs.com");
                    mm.To.Add("agentivy08@gmail.com");
                    mm.Subject = "ALARM";
                    mm.Body = "There was an alarm:";
                    mm.Body += Environment.NewLine;
                    for (byte i = 0; i < MSG.NumberOfAreaAlarms; i++)
                    {
                        enuAlarmType alarm = enuAlarmType.Any;
                        switch (MSG.AreaAlarms[1 + (i * 2)])
                        {
                            case 0:
                                alarm = enuAlarmType.Any;
                                break;
                            case 1:
                                alarm = enuAlarmType.Burglary;
                                break;
                            case 2:
                                alarm = enuAlarmType.Fire;
                                break;
                            case 3:
                                alarm = enuAlarmType.Gas;
                                break;
                            case 4:
                                alarm = enuAlarmType.Aux;
                                break;
                            case 5:
                                alarm = enuAlarmType.Freeze;
                                break;
                            case 6:
                                alarm = enuAlarmType.Water;
                                break;
                            case 7:
                                alarm = enuAlarmType.Duress;
                                break;
                            case 8:
                                alarm = enuAlarmType.Temperature;
                                break;

                        }
                        mm.Body += Environment.NewLine + "Area " + MSG.AreaAlarms[0 + (i * 2)];
                        mm.Body += Environment.NewLine + "Alarm " + alarm;
                        GetNextZone(0);
                    }
                    alarmSent = false;
                }
            }
        }

        private void GetNextZone(int index)
        {
            clsOL2MsgRequestProperties MSG = new clsOL2MsgRequestProperties(HAC.Connection);
            MSG.ObjectType = enuObjectType.Zone;
            MSG.IndexNumber = (UInt16)index;
            MSG.RelativeDirection = 1;  // next object after IndexNumber
            MSG.Filter1 = 1;  // (0=Named or Unnamed, 1=Named, 2=Unnamed).
            MSG.Filter2 = 0;  // Any Area
            MSG.Filter3 = 0;  // Any Room
            HAC.Connection.Send(MSG, HandleZoneAlarmPropertiesResponse);
        }

        private void HandleZoneAlarmPropertiesResponse(clsOmniLinkMessageQueueItem M, byte[] B, bool Timeout)
        {
            if (Timeout)
            {
                return;
            }

            // does it look like a valid response
            if ((B.Length > 3) && (B[0] == 0x21))
            {
                switch ((enuOmniLink2MessageType)B[2])
                {
                    case enuOmniLink2MessageType.EOD:
                        //Close();  // End of data, we are done...
                        if (HAC.Areas[1].AreaAlarms > 0)
                        {
                            if (alarmSent == false)
                            {
                                SendMessage();
                            }
                        }
                        break;
                    case enuOmniLink2MessageType.Properties:
                        // create a new message from byte array for ease of processing
                        clsOL2MsgProperties MSG = new clsOL2MsgProperties(
                            HAC.Connection, B);
                        // copy the zone properties into the zone list
                        HAC.Zones.CopyProperties(MSG);
                        // get a pointer the the zone that was just updated
                        clsZone Zon = HAC.Zones[MSG.ObjectNumber];
                        // build email
                        switch ((((Zon.Status) >> 2) & 0x3))
                        {
                            case 1:
                                mm.Body += Environment.NewLine + Zon.Name + " TRIPPED";
                                break;
                            case 2:
                                mm.Body += Environment.NewLine + Zon.Name + " RESET, BUT PREVIOUSLY TRIPPED";
                                break;
                            default:
                                break;
                        }
                        GetNextZone(MSG.ObjectNumber);
                        break;
                }
            }
            // else handle unexpected response
        }

        private void SendMessage()
        {
            try
            {
                var smtpServer = Configuration.GetSection("AppSettings").GetSection("EmailSettings").GetSection("SMTPServer").Value;
                var smtpPort = Configuration.GetSection("AppSettings").GetSection("EmailSettings").GetSection("SMTPPort").Value;
                var senderEmail = Configuration.GetSection("AppSettings").GetSection("EmailSettings").GetSection("SenderEmail").Value;
                var senderPassword = Configuration.GetSection("AppSettings").GetSection("EmailSettings").GetSection("SenderPassword").Value;
                if (SendEmail.SendEmailToUser(ma, mm, smtpServer, int.Parse(smtpPort), senderEmail, senderPassword))
                {
                    alarmSent = true;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Exceptiion: {0} Stack Trace: {1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        private void HandleRequestSystemTrouble(clsOmniLinkMessageQueueItem M, byte[] B, bool Timeout)
        {
            if (Timeout)
            {
                return;  // just use the default timeout retry sequence
            }

            clsOL2MsgSystemTroubles MSG = null;


            // turn the byte array into a message for ease of handling
            MSG = new clsOL2MsgSystemTroubles(HAC.Connection, B);


            // if there is trouble, send an email
            if (MSG != null)
            {
                if (MSG.NumberOfTroubles() > 0 && MSG.NumberOfTroubles() != numOfTrouble)
                {
                    MailAddress ma = new MailAddress("andrecato@comcast.net", "House");
                    MailMessage mm = new MailMessage(ma, ma);
                    mm.To.Add("5612813999@messaging.sprintpcs.com");
                    mm.To.Add("7545812745@messaging.sprintpcs.com");
                    mm.Subject = "TROUBLE";
                    mm.Body = "There was is trouble:";
                    mm.Body += Environment.NewLine;
                    for (byte i = 0; i < MSG.NumberOfTroubles(); i++)
                    {
                        enuTroubles trouble;
                        trouble = MSG.Troubles(i);
                        mm.Body += Environment.NewLine + "Trouble Type " + trouble;
                    }
                    numOfTrouble = MSG.NumberOfTroubles();

                    try
                    {
                        SendMessage();
                    }
                    catch
                    {
                        HandleRequestSystemTrouble(M, B, Timeout);
                    }
                }
                else if (MSG.NumberOfTroubles() == 0 && numOfTrouble > 0)
                {
                    // reset trouble

                    MailAddress ma = new MailAddress("andrecato@comcast.net", "House");
                    MailMessage mm = new MailMessage(ma, ma);
                    mm.To.Add("5612813999@messaging.sprintpcs.com");
                    mm.To.Add("7545812745@messaging.sprintpcs.com");
                    mm.Subject = "TROUBLE";
                    mm.Body = "Trouble Clear:";
                    mm.Body += Environment.NewLine;

                    try
                    {
                        SendMessage();
                        numOfTrouble = 0;
                    }
                    catch
                    {
                        HandleRequestSystemTrouble(M, B, Timeout);
                    }
                }
            }
        }

        private void HandleSystemEvents(clsOmniLinkMessageQueueItem clsOmniLinkMessageQueueItem, byte[] bytes, bool timeout)
        {
            if (timeout)
            {
                return;  // just use the default timeout retry sequence
            }


            clsOL2MsgExtendedStatus MSG = null;

            // turn the byte array into a message for ease of handling

            MSG = new clsOL2MsgExtendedStatus(HAC.Connection, bytes);

            if (MSG != null)
            {
                if (MSG.MessageType != enuOmniLink2MessageType.Ack) // && alarmSent == false)
                {
                    int messageLength = (MSG.MessageLength - 1) / 2;

                    //Status stat = new Status();
                    //var nests = GetNestThermostats(stat);
                    //Task.WaitAll(nests);
                    int coolSetting, heatSetting;

                    for (int m = 0; m < messageLength; m++)
                    {
                        byte high, low;
                        high = MSG.Data[(m + 1) * sizeof(byte)];
                        low = MSG.Data[(m + 1) * (sizeof(byte) + 1)];

                        switch (high)
                        {
                            case 0: //button

                                Trace.TraceInformation(String.Format("Button Number {0} ({1}) Pressed", low, HAC.Buttons[low].Name));
                                switch (HAC.Buttons[low].Name)
                                {
                                    case "3rd Pty Leav":
                                        // if leaving home set the Nest to away

                                        coolSetting = 0;
                                        heatSetting = 0;
                                        for (int u = 1; u < HAC.UserSettings.Count; u++)
                                        {
                                            if (coolSetting != 0 && heatSetting != 0)
                                            {
                                                break;
                                            }
                                            if (HAC.UserSettings[u].Name.Equals("Away Cool Point"))
                                            {
                                                coolSetting = int.Parse(HAC.UserSettings[u].StatusText());
                                            }
                                            else if (HAC.UserSettings[u].Name.Equals("Away Heat Point"))
                                            {
                                                heatSetting = int.Parse(HAC.UserSettings[u].StatusText());
                                            }
                                        }
                                        if (coolSetting != 0 && heatSetting != 0)
                                        {
                                            Item.Thermostat t = new Item.Thermostat()
                                            {
                                                //ID = nests.Result.Values.FirstOrDefault().DeviceId,
                                                ThermostatType = ThermostatType.Nest,
                                                CoolSetting = coolSetting,
                                                HeatSetting = heatSetting,
                                                Mode = ThermostatMode.HeatCool // (ThermostatMode)Enum.Parse(typeof(ThermostatMode), Enum.GetName(typeof(HvacMode), nests.Result.Values.FirstOrDefault().HvacMode))
                                            };

                                            UpdateThermostat(t);
                                        }
                                        break;
                                    case "3rd Pty Home":

                                        coolSetting = 0;
                                        heatSetting = 0;

                                        for (int u = 1; u < HAC.UserSettings.Count; u++)
                                        {
                                            if (coolSetting != 0 && heatSetting != 0)
                                            {
                                                break;
                                            }
                                            if (HAC.UserSettings[u].Name.Equals("Cool Point"))
                                            {
                                                coolSetting = int.Parse(HAC.UserSettings[u].StatusText());
                                            }
                                            else if (HAC.UserSettings[u].Name.Equals("Heat Point"))
                                            {
                                                heatSetting = int.Parse(HAC.UserSettings[u].StatusText());
                                            }
                                        }
                                        if (coolSetting != 0 && heatSetting != 0)
                                        {
                                            Item.Thermostat t = new Item.Thermostat()
                                            {
                                                //ID = nests.Result.Values.FirstOrDefault().DeviceId,
                                                ThermostatType = ThermostatType.Nest,
                                                CoolSetting = coolSetting,
                                                HeatSetting = heatSetting,
                                                Mode = ThermostatMode.HeatCool // (ThermostatMode)Enum.Parse(typeof(ThermostatMode), Enum.GetName(typeof(HvacMode), nests.Result.Values.FirstOrDefault().HvacMode))
                                            };

                                            UpdateThermostat(t);
                                        }
                                        break;
                                    case "Windows Open":
                                        coolSetting = 0;
                                        heatSetting = 0;

                                        for (int u = 1; u < HAC.UserSettings.Count; u++)
                                        {
                                            if (coolSetting != 0 && heatSetting != 0)
                                            {
                                                break;
                                            }
                                            if (HAC.UserSettings[u].Name.Equals("Cool Point"))
                                            {
                                                coolSetting = int.Parse(HAC.UserSettings[u].StatusText());
                                            }
                                            else if (HAC.UserSettings[u].Name.Equals("Heat Point"))
                                            {
                                                heatSetting = int.Parse(HAC.UserSettings[u].StatusText());
                                            }
                                        }
                                        if (coolSetting != 0 && heatSetting != 0)
                                        {
                                            Item.Thermostat t = new Item.Thermostat()
                                            {
                                                //ID = nests.Result.Values.FirstOrDefault().DeviceId,
                                                ThermostatType = ThermostatType.Nest,
                                                CoolSetting = coolSetting,
                                                HeatSetting = heatSetting,
                                                Mode = ThermostatMode.Off
                                            };

                                            UpdateThermostat(t);
                                        }
                                        break;
                                    case "Window Close":
                                        coolSetting = 0;
                                        heatSetting = 0;

                                        for (int u = 1; u < HAC.UserSettings.Count; u++)
                                        {
                                            if (coolSetting != 0 && heatSetting != 0)
                                            {
                                                break;
                                            }
                                            if (HAC.UserSettings[u].Name.Equals("Cool Point"))
                                            {
                                                coolSetting = int.Parse(HAC.UserSettings[u].StatusText());
                                            }
                                            else if (HAC.UserSettings[u].Name.Equals("Heat Point"))
                                            {
                                                heatSetting = int.Parse(HAC.UserSettings[u].StatusText());
                                            }
                                        }
                                        if (coolSetting != 0 && heatSetting != 0)
                                        {
                                            Item.Thermostat t = new Item.Thermostat()
                                            {
                                                //ID = nests.Result.Values.FirstOrDefault().DeviceId,
                                                ThermostatType = ThermostatType.Nest,
                                                CoolSetting = coolSetting,
                                                HeatSetting = heatSetting,
                                                Mode = ThermostatMode.HeatCool
                                            };

                                            UpdateThermostat(t);
                                        }
                                        break;
                                }



                                break;
                        }
                    }
                }
            }
        }

        private void UpdateThermostat(Item.Thermostat thermo)
        {
            var thermos = GetThermostats();
            var theThermo = thermos.Find(t => t.ID == thermo.ID);

            switch (theThermo.ThermostatType)
            {
                case ThermostatType.HAI:

                    UpdateHAIThermostat(thermo);

                    break;
                case ThermostatType.Nest:

                    UpdateNestThermostat(thermo);

                    break;
            }
        }

        private async void UpdateNestThermostat(Item.Thermostat thermo)
        {
            string file;
            NestApi nest;
            GetNetObject(out file, out nest);

            Status status = new Status();

            try
            {
                var nestThermostats = GetNestThermostats(status);

                Task.WaitAll(nestThermostats);
                var nesttt = nestThermostats.Result.Values.Where<NestSharp.Thermostat>(n => n.DeviceId == thermo.ID).FirstOrDefault();

                float temp = nesttt.AmbientTemperatureFarenheight > thermo.CoolSetting ? thermo.CoolSetting : thermo.HeatSetting;

                if (nesttt.HvacMode.ToString() != thermo.Mode.ToString())
                {
                    await nest.AdjustModeAsync(thermo.ID, (HvacMode)Enum.Parse(typeof(HvacMode), Enum.GetName(thermo.Mode.GetType(), thermo.Mode)));
                    nesttt.HvacMode = (HvacMode)Enum.Parse(typeof(HvacMode), thermo.Mode.ToString());
                }
                if (nesttt.HvacMode != HvacMode.HeatCool)
                {
                    await nest.AdjustTemperatureAsync(
                        thermo.ID,
                        temp,
                        TemperatureScale.F);
                }
                else
                {
                    await nest.AdjustTemperatureAsync(
                       thermo.ID,
                       thermo.CoolSetting,
                       TemperatureScale.F,
                       TemperatureSettingType.High);
                    await nest.AdjustTemperatureAsync(
                     thermo.ID,
                     thermo.HeatSetting,
                     TemperatureScale.F,
                     TemperatureSettingType.Low);
                }
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Exception setting Nest thermostat data: {0}", e.Message);
            }
        }

        private static void GetNetObject(out string file, out NestApi nest)
        {
            var accessToken = string.Empty;

            file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "accesstoken.txt");
            if (File.Exists(file))
            {
                accessToken = File.ReadAllText(file);
            }

            nest = new NestApi(Configuration.GetSection("AppSettings").GetSection("NEST_CLIENT_ID").Value, Configuration.GetSection("AppSettings").GetSection("NEST_CLIENT_SECRET").Value)
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.UtcNow.AddYears(1)
            };
        }

        private void UpdateHAIThermostat(Item.Thermostat thermo)
        {
            HAC.SendCommand(enuUnitCommand.SetHighSetPt, (byte)(clsText.EncodeTemp(thermo.CoolSetting.ToString(), enuTempFormat.Fahrenheit)), (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
            HAC.SendCommand(enuUnitCommand.SetLowSetPt, (byte)(clsText.EncodeTemp(thermo.HeatSetting.ToString(), enuTempFormat.Fahrenheit)), (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
            HAC.SendCommand(enuUnitCommand.SetHumidifySetPt, (byte)(clsText.EncodeTemp(thermo.Humidify.ToString(), enuTempFormat.Fahrenheit)), (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
            HAC.SendCommand(enuUnitCommand.SetDeHumidifySetPt, (byte)(clsText.EncodeTemp(thermo.Dehumidify.ToString(), enuTempFormat.Fahrenheit)), (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
            switch (thermo.Mode)
            {
                case ThermostatMode.Auto:
                    HAC.SendCommand(enuUnitCommand.Mode, (byte)enuThermostatMode.Auto, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
                case ThermostatMode.Cool:
                    HAC.SendCommand(enuUnitCommand.Mode, (byte)enuThermostatMode.Cool, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
                case ThermostatMode.Heat:
                    HAC.SendCommand(enuUnitCommand.Mode, (byte)enuThermostatMode.Heat, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
                case ThermostatMode.EmergencyHeat:
                    HAC.SendCommand(enuUnitCommand.Mode, (byte)enuThermostatMode.E_Heat, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
                case ThermostatMode.Off:
                    HAC.SendCommand(enuUnitCommand.Mode, (byte)enuThermostatMode.Off, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
            }

            switch (thermo.FanMode)
            {
                case ThermostatFanMode.Auto:
                    HAC.SendCommand(enuUnitCommand.Fan, (byte)enuThermostatFanMode.Auto, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
                case ThermostatFanMode.Cycle:
                    HAC.SendCommand(enuUnitCommand.Fan, (byte)enuThermostatFanMode.Cycle, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
                case ThermostatFanMode.On:
                    HAC.SendCommand(enuUnitCommand.Fan, (byte)enuThermostatFanMode.On, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
            }
            switch (thermo.HoldMode)
            {
                case ThermostatHoldMode.Off:
                    HAC.SendCommand(enuUnitCommand.Hold, (byte)enuThermostatHoldMode.Off, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
                case ThermostatHoldMode.On:
                    HAC.SendCommand(enuUnitCommand.Hold, (byte)enuThermostatHoldMode.On, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
                case ThermostatHoldMode.Vacation:
                    HAC.SendCommand(enuUnitCommand.Hold, (byte)enuThermostatHoldMode.Vacation, (ushort)HAC.Thermostats[int.Parse(thermo.ID)].Number);
                    break;
            }
        }

        private List<Item.Thermostat> GetThermostats()
        {
            Status status = new Status();

            try
            {
                var nestThermostats = GetNestThermostats(status);

                Task.WaitAll(nestThermostats);
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception getting Nest thermostat data: {0}", e.Message);
            }

            for (int t = 0; t < HAC.NumThermostats; t++)
            {
                status.Thermostats.Add(new Item.Thermostat()
                {
                    ID = t.ToString(),
                    ThermostatType = ThermostatType.HAI,
                    Number = HAC.Thermostats[t].Number,
                    Name = HAC.Thermostats[t].Name,
                    FanMode = (ThermostatFanMode)Enum.Parse(typeof(ThermostatFanMode), Enum.GetName(typeof(enuThermostatFanMode), HAC.Thermostats[t].FanMode)),
                    InsideTemp = clsText.DecodeTemp(HAC.Thermostats[t].Temp, HAC.TempFormat, HAC),
                    InsideTempCel = clsText.DecodeTemp(HAC.Thermostats[t].Temp, enuTempFormat.Celsius, HAC),
                    OutsideTemp = clsText.DecodeTemp(HAC.Thermostats[t].OutdoorTemp, HAC.TempFormat, HAC),
                    OutsideTempCel = clsText.DecodeTemp(HAC.Thermostats[t].OutdoorTemp, enuTempFormat.Celsius, HAC),
                    HeatSetting = float.Parse(clsText.DecodeTemp(HAC.Thermostats[t].HeatSetpoint, HAC.TempFormat, HAC)),
                    HeatSettingCel = float.Parse(clsText.DecodeTemp(HAC.Thermostats[t].HeatSetpoint, enuTempFormat.Celsius, HAC)),
                    CoolSetting = float.Parse(clsText.DecodeTemp(HAC.Thermostats[t].CoolSetpoint, HAC.TempFormat, HAC)),
                    CoolSettingCel = float.Parse(clsText.DecodeTemp(HAC.Thermostats[t].CoolSetpoint, enuTempFormat.Celsius, HAC)),
                    Humidity = float.Parse(clsText.DecodeHumidity(HAC.Thermostats[t].Humidity, HAC)),
                    Humidify = float.Parse(clsText.DecodeTemp(HAC.Thermostats[t].HumidifySetpoint, HAC.TempFormat, HAC)),
                    Dehumidify = float.Parse(clsText.DecodeTemp(HAC.Thermostats[t].DehumidifySetpoint, HAC.TempFormat, HAC)),
                    Mode = (ThermostatMode)Enum.Parse(typeof(ThermostatMode), Enum.GetName(typeof(enuThermostatMode), HAC.Thermostats[t].Mode)),
                    HoldMode = (ThermostatHoldMode)Enum.Parse(typeof(ThermostatHoldMode), Enum.GetName(typeof(enuThermostatHoldMode), HAC.Thermostats[t].HoldStatus)),
                    SystemStatus = HAC.Thermostats[t].HorC_StatusText()
                });
            }

            return status.Thermostats;
        }

        private async Task<Dictionary<string, NestSharp.Thermostat>> GetNestThermostats(Status status)
        {
            Devices devices = await GetNestDevices();

            // Loop through the devices
            if (devices == null)
            {
                return null;
            }

            var thermostatList = new Dictionary<string, Guid>();
            var thermostatGuid = Guid.Parse(Configuration.GetSection("AppSettings").GetSection("Nest_Thermostat_ID").Value);

            foreach (var t in devices?.Thermostats)
            {
                ThermostatConnectedEventArgs thermostatArgs = new ThermostatConnectedEventArgs(t.Key, 
                    Guid.Parse(Configuration.GetSection("AppSettings").GetSection("Nest_Thermostat_ID").Value), 
                    t.Value.AmbientTemperatureFarenheight, 
                    t.Value.TargetTemperatureLowFarenheit,
                    t.Value.TargetTemperatureHighFarenheit,
                    Enum.GetName(typeof(HvacMode), t.Value.HvacMode),
                    Enum.GetName(typeof(HvacState), t.Value.HvacState),
                    t.Value.Humidity);
                OnThermostatConnected(thermostatArgs);

                var thermostatId = t.Value.DeviceId;
                thermostatList.Add(thermostatId, thermostatGuid);

                Trace.TraceInformation(String.Format("{0}: {1} Current Temperature Is {2}°: Outdoor Temperature is {3}°: Current Heatpoint setting is {4}° : Current Coolpoint setting is {5}°",
                DateTime.Now.ToShortTimeString(),
                t.Value.Name,
                t.Value.AmbientTemperatureFarenheight, "Unknown", t.Value.TargetTemperatureLowFarenheit, t.Value.TargetTemperatureHighFarenheit));

                status.Thermostats.Add(new Item.Thermostat()
                {
                    ID = t.Value.DeviceId,
                    ThermostatType = ThermostatType.Nest,
                    Name = t.Value.Name,
                    InsideTemp = t.Value.AmbientTemperatureFarenheight.ToString(),
                    InsideTempCel = t.Value.AmbientTemperatureCelsius.ToString(),
                    OutsideTemp = "Unknown",
                    HeatSetting = t.Value.AmbientTemperatureFarenheight < t.Value.TargetTemperatureFarenheit ? t.Value.TargetTemperatureFarenheit : t.Value.TargetTemperatureLowFarenheit,
                    HeatSettingCel = t.Value.AmbientTemperatureCelsius < t.Value.TargetTemperatureCelsius ? t.Value.TargetTemperatureCelsius : t.Value.TargetTemperatureLowCelsius,
                    CoolSetting = t.Value.AmbientTemperatureFarenheight > t.Value.TargetTemperatureFarenheit ? t.Value.TargetTemperatureFarenheit : t.Value.TargetTemperatureHighFarenheit,
                    CoolSettingCel = t.Value.AmbientTemperatureCelsius > t.Value.TargetTemperatureCelsius ? t.Value.TargetTemperatureCelsius : t.Value.TargetTemperatureHighCelsius,
                    Humidity = t.Value.Humidity,
                    Mode = (ThermostatMode)Enum.Parse(typeof(ThermostatMode), Enum.GetName(typeof(HvacMode), t.Value.HvacMode)),
                    SystemStatus = Enum.GetName(typeof(HvacState), t.Value.HvacState)

                });
            }


            SubscribeToNestDeviceDataUpdates(thermostatList);

            return devices.Thermostats;
        }

        private void SubscribeToNestDeviceDataUpdates(Dictionary<string, Guid> thermostatList)
        {
            string file;
            NestApi nest;
            GetNetObject(out file, out nest);

            var t = new Task(() =>
            {
                nest.ThermostatHeatSetpointChanged += SendEventForHeatSetpointChanged;
                nest.ThermostatCoolSetpointChanged += SendEventForCoolSetpointChanged;
                nest.ThermostatAmbientTemperatureChanged += SendEventForAmbientTemperatureChanged;

                nest.SubscribeToNestDeviceDataUpdates(ControllerId, thermostatList);
            });

            t.Start();
        }

        private void SendEventForHeatSetpointChanged(Object sender, NestSharp2.EventArguments.ThermostatHeatSetpointChangedEventArgs e)
        {
            ThermostatHeatSetpointChangedEventArgs heatSetpointChangedArgs = new ThermostatHeatSetpointChangedEventArgs(e.TenantId, e.ThermostatId, e.ThermostatGuid, e.NewHeatSetpoint);
            OnThermostatHeatSetpointChanged(heatSetpointChangedArgs);
        }

        private void SendEventForCoolSetpointChanged(Object sender, NestSharp2.EventArguments.ThermostatCoolSetpointChangedEventArgs e)
        {
            ThermostatCoolSetpointChangedEventArgs coolSetpointChangedArgs = new ThermostatCoolSetpointChangedEventArgs(e.TenantId, e.ThermostatId, e.ThermostatGuid, e.NewCoolSetpoint);
            OnThermostatCoolSetpointChanged(coolSetpointChangedArgs);
        }

        private void SendEventForAmbientTemperatureChanged(Object sender, NestSharp2.EventArguments.ThermostatAmbientTemperatureChangedEventArgs e)
        {
            ThermostatAmbientTemperatureChangedEventArgs ambientTemperatureChangedArgs 
                = new ThermostatAmbientTemperatureChangedEventArgs(e.TenantId, e.ThermostatId, e.ThermostatGuid, e.NewAmbientTemperature);
            OnThermostatAmbientTemperatureChanged(ambientTemperatureChangedArgs);
        }

        private static async Task<Devices> GetNestDevices()
        {
            string file;
            NestApi nest;
            GetNetObject(out file, out nest);

            if (string.IsNullOrEmpty(nest.AccessToken))
            {

                Console.WriteLine("Login to the Web Browser and authorize...");

                // Get the URL to load in a browser for the PIN
                var authUrl = nest.GetAuthorizationUrl();

                System.Diagnostics.Process.Start(authUrl);

                Console.WriteLine("Enter your PIN:");

                // Read back in the PIN
                var authToken = Console.ReadLine();

                // Get an access token to use with the API
                await nest.GetAccessToken(authToken);

                File.WriteAllText(file, nest.AccessToken);
            }

            var devices = await nest.GetDevicesAsync();
            return devices;
        }

        private void HandleConnectStatus(enuOmniLinkCommStatus commStatus)
        {
            switch (commStatus)
            {
                case enuOmniLinkCommStatus.NoReply:
                    Trace.TraceInformation("CONNECTION STATUS: No Reply");
                    break;

                case enuOmniLinkCommStatus.Disconnected:
                    Trace.TraceInformation("CONNECTION STATUS: Disconnected");

                    // Send Disconnected Event
                    ControllerDisconnectedEventArgs disconnectArgs = new ControllerDisconnectedEventArgs();
                    OnControllerDisconnected(disconnectArgs);
                    if (UserDisconnected == true)
                    {
                        UserDisconnected = false;
                    }
                    else
                    {
                        Trace.TraceInformation("This was not a user disconnect. RECONNECTING.");
                        Connect();
                    }
                    break;

                case enuOmniLinkCommStatus.Connected:
                    // Send Connected Event
                    ControllerConnectedEventArgs args = new ControllerConnectedEventArgs();
                    OnControllerConnected(args);

                    IdentifyController();
                    break;
                case enuOmniLinkCommStatus.Connecting:
                    Trace.TraceInformation("CONNECTION STATUS: Connecting");
                    break;

                default:
                    break;
            }
        }

        private void IdentifyController()
        {
            Trace.TraceInformation("Identifying Controller...");
            if (HAC.Connection.ConnectionState == enuOmniLinkConnectionState.Online ||
                HAC.Connection.ConnectionState == enuOmniLinkConnectionState.OnlineSecure)
            {
                if (HAC.Connection.ConnectionProtocol() == enuOmniLinkProtocol.V1)  // user OmniLink
                {
                    HAC.Connection.Send(new clsOLMsgRequestSystemInformation(HAC.Connection), HandleIdentifyController);
                }
                else  // use OmniLink 2
                {
                    HAC.Connection.Send(new clsOL2MsgRequestSystemInformation(HAC.Connection), HandleIdentifyController);
                }
            }
            else
            {
                // TODO: display error message.
                Trace.TraceError("ERROR: Not On Line.");
            }
        }

        private void HandleIdentifyController(clsOmniLinkMessageQueueItem M, byte[] B, bool Timeout)
        {
            if (Timeout)
            {
                return;
            }


            // decode the message based on the current protocol
            if (HAC.Connection.ConnectionProtocol() == enuOmniLinkProtocol.V1)
            {
                if ((B.Length > 3) &&
                ((enuOmniLinkMessageType)B[2] == enuOmniLinkMessageType.SystemInformation))
                {
                    clsOLMsgSystemInformation MSG = new clsOLMsgSystemInformation(HAC.Connection, B);
                    if (HAC.Model == MSG.ModelNumber)
                    {
                        HAC.CopySystemInformation(MSG);
                        Trace.TraceInformation("CONTROLLER IS: " + HAC.GetModelText() + " (" + HAC.GetVersionText() + ")");
                        return;
                    }
                    Trace.TraceError("Model does not match file");
                    HAC.Connection.Disconnect();
                }
                Trace.TraceError("Unexpected Response");
                HAC.Connection.Disconnect();
            }
            else
            {
                if ((B.Length > 3) &&
                (B[2] == (byte)enuOmniLink2MessageType.SystemInformation))
                {
                    clsOL2MsgSystemInformation MSG = new clsOL2MsgSystemInformation(HAC.Connection, B);
                    if (HAC.Model == MSG.ModelNumber)
                    {
                        HAC.CopySystemInformation(MSG);
                        Trace.TraceInformation("CONTROLLER IS: " + HAC.GetModelText() + " (" + HAC.GetVersionText() + ")");
                        return;
                    }
                    Trace.TraceError("Model does not match file");
                    HAC.Connection.Disconnect();
                }
                Trace.TraceError("Unexpected Response");
                HAC.Connection.Disconnect();
            }
        }

        private static void Startup()
        {
            string path = GetPath();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(path))
                .AddJsonFile("appsettings.json", false, true);
            Configuration = builder.Build();
        }

        private static string GetPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return path;
        }
    }
}
