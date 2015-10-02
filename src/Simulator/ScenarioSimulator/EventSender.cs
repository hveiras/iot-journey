// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Practices.IoTJourney.Logging;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Client;

namespace Microsoft.Practices.IoTJourney.ScenarioSimulator
{
    public class EventSender : IEventSender
    {
        private readonly DeviceClient _deviceClient;
        static string iotHubUri = "IoTHubTestHav.azure-devices.net";

        private readonly Func<object, byte[]> _serializer;

        public EventSender(
            SimulatedDevice simulatedDevice,
            SimulatorConfiguration config,
            Func<object, byte[]> serializer)
        {
            this._serializer = serializer;

            _deviceClient = DeviceClient.Create(iotHubUri, 
                                                new DeviceAuthenticationWithRegistrySymmetricKey
                                                (
                                                    simulatedDevice.Id,
                                                    simulatedDevice.Authentication.SymmetricKey.PrimaryKey
                                                ));
        }

        public static string DetermineTypeFromEvent(object evt)
        {
            return evt.GetType().Name;
        }

        public async Task<bool> SendAsync(object evt)
        {
            try
            {
                var bytes = this._serializer(evt);

                using (var message = new Azure.Devices.Client.Message(bytes))
                {
                    var stopwatch = Stopwatch.StartNew();

                    await _deviceClient.SendEventAsync(message).ConfigureAwait(false);
                    stopwatch.Stop();

                    ScenarioSimulatorEventSource.Log.EventSent(stopwatch.ElapsedTicks);

                    return true;
                }
            }
            catch (ServerBusyException e)
            {
                ScenarioSimulatorEventSource.Log.ServiceThrottled(e);
            }
            catch (Exception e)
            {
                ScenarioSimulatorEventSource.Log.UnableToSend(e, evt.ToString());
            }

            return false;
        }
    }
}