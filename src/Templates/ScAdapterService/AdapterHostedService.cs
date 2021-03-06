using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Logging;
using ServiceControl.TransportAdapter;

namespace ScAdapterService
{
    class AdapterHostedService : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var adapterConfig = new TransportAdapterConfig<LearningTransport, LearningTransport>("TransportAdapter.ScAdapterService");

                adapterConfig.CustomizeEndpointTransport(t =>
                {
                    //TODO: Customize the endpoint-facing side of the adapter
                    //Use exactly the same settings as in regular endpoints
                });

                adapterConfig.CustomizeServiceControlTransport(t =>
                {
                    //TODO: Customize the ServiceControl-facing side of the adapter
                    //e.g. specify the same connection string as ServiceControl uses.
                });

                adapter = TransportAdapter.Create(adapterConfig);

                await adapter.Start();
            }
            catch (Exception exception)
            {
                FailFast("Failed to start", exception);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                // TODO: perform any further shutdown operations before or after stopping the adapter
                await adapter.Stop();
            }
            catch (Exception ex)
            {
                FailFast("Failed to stop correctly.", ex);
            }
        }

        static void FailFast(string message, Exception exception)
        {
            try
            {
                log.Fatal(message, exception);

                // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
                // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }

        ITransportAdapter adapter;

        // TODO: optionally choose a custom logging library
        // https://docs.particular.net/nservicebus/logging/#custom-logging
        // LogManager.Use<TheLoggingFactory>();
        static readonly ILog log = LogManager.GetLogger<AdapterHostedService>();
    }
}