// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Tracing;
using System.Globalization;

namespace Microsoft.Practices.IoTJourney.ScenarioSimulator
{
    public class SimulatorConfiguration
    {
        public string Scenario { get; set; }

        public int NumberOfDevices { get; set; }

        public int EventHubTokenLifetimeDays { get; set; }

        public string EventHubName { get; set; }

        public string IotHubOwnerConnectionString { get; set; }

        public TimeSpan WarmUpDuration { get; set; }

        private EventLevel _eventLevel;
        public EventLevel GetLogLevel()
        {
            const string Key = "IOT_LOGLEVEL";
            object logLevel = Environment.GetEnvironmentVariable(Key);
            if (logLevel != null)
            {
                _eventLevel = ConfigurationHelper.ConvertValue<EventLevel>(Key, logLevel);
            }

            return _eventLevel;
        }

        public static SimulatorConfiguration GetCurrentConfiguration()
        {
            return new SimulatorConfiguration
            {
                Scenario = ConfigurationHelper.GetConfigValue<string>("Simulator.Scenario", String.Empty),
                NumberOfDevices = ConfigurationHelper.GetConfigValue<int>("Simulator.NumberOfDevices"),
                EventHubName = ConfigurationHelper.GetConfigValue<string>("Simulator.EventHubName"),
                EventHubTokenLifetimeDays = ConfigurationHelper.GetConfigValue<int>("Simulator.EventHubTokenLifetimeDays", 7),
                WarmUpDuration = ConfigurationHelper.GetConfigValue("Simulator.WarmUpDuration", TimeSpan.FromSeconds(30)),
                _eventLevel = ConfigurationHelper.GetConfigValue<EventLevel>("Simulator.LogLevel", EventLevel.Informational),
                IotHubOwnerConnectionString = ConfigurationHelper.GetConfigValue<string>("Simulator.IotHubOwnerConnectionString")
            };
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                "Simulation SimulatorConfiguration; device count = {0} event hub name = {1}",
                NumberOfDevices,
                EventHubName);
        }
    }
}