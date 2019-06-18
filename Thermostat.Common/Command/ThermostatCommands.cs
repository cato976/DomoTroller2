using DomoTroller2.Common;
using System;

namespace Thermostat.Common.Command
{
    public class ConnectThermostat : ICommand
    {
        public ConnectThermostat(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
