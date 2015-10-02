// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Practices.IoTJourney.Devices.Events;

namespace Microsoft.Practices.IoTJourney.ScenarioSimulator
{
    public static class EventFactory
    {
        public static UpdateTemperatureEvent TemperatureEventFactory(Random random, SimulatedDevice simulatedDevice)
        {
            if (!simulatedDevice.CurrentTemperature.HasValue)
            {
                simulatedDevice.CurrentTemperature = random.Next(25);
            }
            else
            {
                var temperatureChange = random.Next(-2,3);
                simulatedDevice.CurrentTemperature += temperatureChange;
            }

            return new UpdateTemperatureEvent
            {
                DeviceId = simulatedDevice.Id,
                TimeObserved = DateTime.UtcNow,
                Temperature = simulatedDevice.CurrentTemperature.Value,
            };
        }

        public static UpdateTemperatureEvent ThirtyDegreeTemperatureEventFactory(Random random, SimulatedDevice simulatedDevice)
        {
            return new UpdateTemperatureEvent
            {
                DeviceId = simulatedDevice.Id,
                TimeObserved = DateTime.UtcNow,
                Temperature = 30,
            };
        }
    }
}