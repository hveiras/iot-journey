// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.IoTJourney.Logging;
using Microsoft.Azure.Devices;

namespace Microsoft.Practices.IoTJourney.ScenarioSimulator
{
    public class SimulatedDevice
    {
        private static readonly TimeSpan LoopFrequency = TimeSpan.FromSeconds(0.33);

        private Device _device;

        public string Id
        {
            get { return _device.Id; }
        }

        public AuthenticationMechanism Authentication
        {
            get { return _device.Authentication; }
        }

        public int StartupOrder { get; private set; }

        public float? CurrentTemperature { get; set; }

        public ISubject<int> ObservableEventCount { get; private set; }

        public SimulatedDevice(Device device, int startupOrder)
        {
            _device = device;
            StartupOrder = startupOrder;
            ObservableEventCount = new Subject<int>();
        }

        public async Task RunSimulationAsync(
            IEnumerable<EventEntry> eventEntries,
            Func<object, Task<bool>> sendEventAsync,
            CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew();

            ScenarioSimulatorEventSource.Log.DeviceStarting(Id);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var elaspedTime = stopwatch.Elapsed;
                    stopwatch.Restart();

                    foreach (var entry in eventEntries)
                    {
                        entry.UpdateElapsedTime(elaspedTime);
                        if (!entry.ShouldSendEvent())
                        {
                            continue;
                        }
                        entry.ResetElapsedTime();

                        var evt = entry.CreateNewEvent(this);
                        var wasEventSent = await sendEventAsync(evt).ConfigureAwait(false);

                        if (wasEventSent)
                        {
                            ObservableEventCount.OnNext(1);
                        }
                        else
                        {
                            // If the event was not sent, it is likely that Event Hub
                            // is throttling our requests. So we will cause the simulation
                            // for this particular device to delay and reduce the load.
                            // Note that in some cases you will want resend the event,
                            // however we are merely pausing before trying to send
                            // the next one.
                            try
                            {
                                await Task.Delay(TimeSpan.FromSeconds(10), token);
                            }
                            catch (TaskCanceledException) { /* cancelling Task.Delay will throw */ }
                        }
                    }

                    try
                    {
                        await Task.Delay(LoopFrequency, token);
                    }
                    catch (TaskCanceledException) { /* cancelling Task.Delay will throw */ }
                }
            }
            catch (Exception e)
            {
                ObservableEventCount.OnError(e);
                ScenarioSimulatorEventSource.Log.DeviceUnexpectedFailure(e, Id);
                return;
            }

            ObservableEventCount.OnCompleted();

            ScenarioSimulatorEventSource.Log.DeviceStopping(Id);
        }
    }
}