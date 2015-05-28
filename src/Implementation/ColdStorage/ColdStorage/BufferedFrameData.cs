﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Practices.IoTJourney.ColdStorage.RollingBlobWriter;
using Microsoft.ServiceBus.Messaging;

namespace Microsoft.Practices.IoTJourney.ColdStorage
{
    public class BufferedFrameData : BlockData
    {
        private readonly EventData _lastEventDataInFrame;

        public BufferedFrameData(byte[] frame, int actualFrameLength, EventData lastEventDataInFrame)
            : base(frame, actualFrameLength)
        {
            Guard.ArgumentNotNull(lastEventDataInFrame, "lastEventDataInFrame");

            _lastEventDataInFrame = lastEventDataInFrame;
        }

        public EventData LastEventDataInFrame
        {
            get { return _lastEventDataInFrame; }
        }
    }
}