﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Practices.IoTJourney.WarmStorage.EventProcessor.ElasticSearchWriter;
using Microsoft.Practices.IoTJourney.WarmStorage.EventProcessor.Logging;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Microsoft.Practices.IoTJourney.WarmStorage.EventProcessor
{
    public class WarmStorageCoordinator : IDisposable
    {
        private EventProcessorHost _host;

        private WarmStorageCoordinator(EventProcessorHost host)
        {
            _host = host;
        }

        public static async Task<WarmStorageCoordinator> CreateAsync(string hostName, Configuration configuration)
        {
            WarmStorageEventSource.Log.InitializingEventHubListener(configuration.EventHubName, configuration.ConsumerGroupName);

            Func<string, IElasticSearchWriter> elasticSearchWriterFactory = partitionId => 
                new ElasticSearchWriter.ElasticSearchWriter(
                    configuration.ElasticSearchUrl,
                    configuration.ElasticSearchIndexPrefix,
                    configuration.ElasticSearchIndexType,
                    configuration.RetryCount
                );

            var ns = NamespaceManager.CreateFromConnectionString(configuration.EventHubConnectionString);
            try
            {
                await ns.GetConsumerGroupAsync(configuration.EventHubName, configuration.ConsumerGroupName);
            }
            catch (Exception e)
            {
                WarmStorageEventSource.Log.InvalidEventHubConsumerGroupName(e, configuration.EventHubName, configuration.ConsumerGroupName);
                throw;
            }

            WarmStorageEventSource.Log.ConsumerGroupFound(configuration.EventHubName, configuration.ConsumerGroupName);

            var eventHubId = ConfigurationHelper.GetEventHubName(ns.Address, configuration.EventHubName);

            var buildingLookupService = new BuildingLookupService();
            await buildingLookupService.InitializeAsync();

            var factory = new WarmStorageEventProcessorFactory(elasticSearchWriterFactory, eventHubId, buildingLookupService);

            var options = new EventProcessorOptions()
            {
                MaxBatchSize = configuration.MaxBatchSize,
                PrefetchCount = configuration.PreFetchCount,
                ReceiveTimeOut = configuration.ReceiveTimeout
            };

            options.ExceptionReceived += 
                (s, e) => WarmStorageEventSource.Log.ErrorProcessingMessage(e.Exception, e.Action);

            var host = new EventProcessorHost(
                hostName,
                consumerGroupName: configuration.ConsumerGroupName,
                eventHubPath: configuration.EventHubName,
                eventHubConnectionString: configuration.EventHubConnectionString,
                storageConnectionString: configuration.CheckpointStorageAccount,
                leaseContainerName: configuration.EventHubName.ToLower());

            await host.RegisterEventProcessorFactoryAsync(factory, options);

            return new WarmStorageCoordinator(host);
        }

        public async Task TearDownAsync()
        {
            if (_host != null)
            {
                await _host.UnregisterEventProcessorAsync();
                _host = null;
            }
        }

        public void Dispose()
        {
            TearDownAsync().Wait();
        }
    }
}
