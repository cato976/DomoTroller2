using System;
using FirebaseSharp.Portable;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace NestSharp
{
    public class NestApi
    {
        public NestApi (string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;

            http = new HttpClient ();
        }

        const string BASE_URL = "https://developer-api.nest.com/";
        const string AUTHORIZATION_URL = "https://home.nest.com/login/oauth2?client_id={0}&state={1}";
        const string ACCESS_TOKEN_URL  = "https://api.home.nest.com/oauth2/access_token";

        HttpClient http;

        public string ClientId { get; private set; }

        public string ClientSecret { get; private set; }

        public string AccessToken { get; set; }

        public DateTime ExpiresAt { get; set; }


        public string GetAuthorizationUrl ()
        {
            var state = Guid.NewGuid ().ToString ();

            return string.Format (
                AUTHORIZATION_URL,
                ClientId,
                state);            
        }

        public async Task<string> GetAccessToken (string authorizationToken)
        {
            var url = string.Format (ACCESS_TOKEN_URL,
                          ClientId,
                          authorizationToken,
                          ClientSecret);

            var v = new Dictionary<string, string> ();
            v.Add ("client_id", ClientId);
            v.Add ("code", authorizationToken);
            v.Add ("client_secret", ClientSecret);
            v.Add ("grant_type", "authorization_code");

            var r = await http.PostAsync (url, new FormUrlEncodedContent (v));
            var data = await r.Content.ReadAsStringAsync ();

            var json = JObject.Parse (data);

            if (json != null) {

                if (json ["access_token"] != null)
                    AccessToken = json ["access_token"].ToString ();

                if (json ["expires_in"] != null) {
                    var expiresIn = json.Value<long> ("expires_in");

                    ExpiresAt = DateTime.UtcNow.AddSeconds (expiresIn);
                }
            }   

            return AccessToken;
        }

        void CheckAuth ()
        {
            if (string.IsNullOrEmpty (AccessToken)
                || ExpiresAt < DateTime.UtcNow) {
                throw new UnauthorizedAccessException ("Invalid Acess Token");
            }
        }

        //TODO: Get the right data enclosure for this call
//        public async Task<Devices> GetStructuresAndDevicesAsync ()
//        {
//            CheckAuth ();
//
//            var url = "https://developer-api.nest.com/?auth={0}";
//
//            var data = await http.GetStringAsync (string.Format (url, AccessToken));
//
//            return JsonConvert.DeserializeObject<Devices> (data);
//        }

        public async Task<Devices> GetDevicesAsync ()
        {
            string data = string.Empty;
            CheckAuth ();

            var url = "https://developer-api.nest.com/devices.json?auth={0}";
            try
            {
                data = await http.GetStringAsync(string.Format(url, AccessToken));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }

            return JsonConvert.DeserializeObject<Devices> (data);
        }

        public void SubscribeToNestDeviceDataUpdates()
        {
            var accessToken = string.Empty;
            string file;

            file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "accesstoken.txt");
            if (File.Exists(file))
            {
                accessToken = File.ReadAllText(file);
            }

            var firebaseClient = new Firebase("https://developer-api.nest.com/", accessToken);
            var response = firebaseClient.GetStreaming("devices",
                    changed: (s, e) => {
                        if (e.Path.Contains("ambient_temperature_f"))
                            //Console.WriteLine("Current temperature has been updated to: {0}.", e.Data);
                            Trace.TraceInformation("Current temperature of Nest Thermostat has been updated to: {0}.", e.Data);
                    });

            //Console.WriteLine("Change the current temperature of the Nest Thermostat in the Nest Developer Chrome Extension to see the real-time updates.");
            Trace.TraceInformation("Change the current temperature of the Nest Thermostat in the Nest Developer Chrome Extension to see the real-time updates.");
        }

        public async Task<Dictionary<string, Structure>> GetStructuresAsync ()
        {
            CheckAuth ();

            var url = "https://developer-api.nest.com/structures.json?auth={0}";

            var data = await http.GetStringAsync (string.Format (url, AccessToken));

            return JsonConvert.DeserializeObject<Dictionary<string, Structure>> (data);
        }
            
        public async Task<Thermostat> GetThermostatAsync (string deviceId)
        {
            CheckAuth ();

            var url = BASE_URL + "devices/thermostats/.json{0}?auth={1}";
                       
            var data = await http.GetStringAsync (string.Format (url, deviceId, AccessToken));

            var thermostats = JsonConvert.DeserializeObject<Dictionary<string, Thermostat>> (data);

            return thermostats.FirstOrDefault ().Value;
        }

        public async Task<JObject> AdjustModeAsync(string deviceId, HvacMode mode)
        {
            var url = BASE_URL + "devices/thermostats/{0}?auth={1}";

            var field = "hvac_mode";

            var thermostat = await GetThermostatAsync(deviceId);

            var formattedUrl = string.Format(
                                  url,
                                  thermostat.DeviceId,
                                  AccessToken);

            var formattedField = string.Format(
                                   field);

            string modeString;
            if(mode == HvacMode.HeatCool)
            {
                modeString = "heat-cool";
            }
            else
            {
                modeString = mode.ToString().ToLower();
            }

            var json = @"{""" + formattedField + @""": """ + modeString + @"""}";

            var r = await http.PutAsync(formattedUrl, new StringContent(
                       json,
                       Encoding.UTF8,
                       "application/json"));

            try
            {
                r.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Exception in AdjustModeAsync: {0}", e.Message);
            }

            var data = await r.Content.ReadAsStringAsync();
            return JObject.Parse(data);
        }

        public async Task<JObject> AdjustTemperatureAsync (string deviceId, float degrees, TemperatureScale scale, TemperatureSettingType type = TemperatureSettingType.None)
        {
            var url = BASE_URL + "devices/thermostats/{0}?auth={1}";

            var field = "target_temperature_{0}{1}";

            var tempScale = scale.ToString ().ToLower ();

            var tempType = string.Empty;
            if (type != TemperatureSettingType.None)
                tempType = type.ToString ().ToLower () + "_";
            
            var thermostat = await GetThermostatAsync (deviceId);

            var exMsg = string.Empty;

            if (thermostat.IsUsingEmergencyHeat) {
                exMsg = "Can't adjust target temperature while using emergency heat";
            }
            else if (thermostat.HvacMode == HvacMode.HeatCool && type == TemperatureSettingType.None) {
                exMsg = "Can't adjust target temperature while in Heat + Cool Mode, use High/Low TemperatureSettingTypes instead";
            } else if (thermostat.HvacMode != HvacMode.HeatCool && type != TemperatureSettingType.None) {
                exMsg = "Can't adjust targeet temperature type.ToString () while in Heat or Cool mode.  Use None for TemperatureSettingType instead";
            }
            //TODO: Get the structure instead and check for away
//            else if (1 == 2) { // Check for 'away'
//                exMsg = "Can't adjust target temperature while in Away or Auto-Away mode";
//            }

            if (!string.IsNullOrEmpty (exMsg))
                throw new ArgumentException (exMsg);

            var formattedUrl = string.Format (
                                   url, 
                                   thermostat.DeviceId,
                                   AccessToken);

            var formattedField = string.Format (
                                     field, 
                                     tempType,
                                     tempScale);
                          
            var json = @"{""" + formattedField + @""": " + degrees + "}";


            var r = await http.PutAsync (formattedUrl, new StringContent (
                        json,
                        Encoding.UTF8,
                        "application/json"));

            try
            {
                r.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
            }

            var data = await r.Content.ReadAsStringAsync ();

            return JObject.Parse (data);
        }            
    }
}

