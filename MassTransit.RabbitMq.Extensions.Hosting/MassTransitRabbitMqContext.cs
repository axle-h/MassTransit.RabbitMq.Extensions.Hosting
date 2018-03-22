using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.RabbitMq.Extensions.Hosting.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransit.RabbitMq.Extensions.Hosting
{
    public class MassTransitRabbitMqContext : IMassTransitRabbitMqContext
    {
        private readonly MassTransitRabbitMqHostingOptions _options;
        private readonly IMassTransitRabbitMqHostingConfigurator _configurator;
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<Task<IBusControl>> _bus;
        private CancellationToken _cancellationToken;
        private readonly ILogger<MassTransitRabbitMqContext> _logger;

        public MassTransitRabbitMqContext(IOptions<MassTransitRabbitMqHostingOptions> options,
                                          IMassTransitRabbitMqHostingConfigurator configurator,
                                          IServiceProvider serviceProvider,
                                          ILogger<MassTransitRabbitMqContext> logger)
        {
            _configurator = configurator;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
            _bus = new Lazy<Task<IBusControl>>(GetBusAsync, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public Task<IBusControl> GetBusControlAsync(CancellationToken cancellationToken = default)
        {
            if (!_bus.IsValueCreated && cancellationToken != default && _cancellationToken == default)
            {
                // Only the first call to this with a non-default cancellation token gets their cancellation token used.
                // Hopefully it'll be the IHostedService.
                _cancellationToken = cancellationToken;
            }

            return _bus.Value;
        }

        private async Task<IBusControl> GetBusAsync()
        {
            while (true)
            {
                try
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    var bus = Bus.Factory.CreateUsingRabbitMq(Configure);
                    await bus.StartAsync(_cancellationToken);
                    return bus;
                }
                catch (RabbitMqConnectionException)
                {
                    _logger.LogError("Failed to connect to RabbitMQ, retrying");
                    await Task.Delay(1000, _cancellationToken);
                }
            }
        }

        private void Configure(IRabbitMqBusFactoryConfigurator config)
        {
            var host = config.Host(_options.RabbitMqUri, c =>
                                                         {
                                                             c.Username(_options.Username);
                                                             c.Password(_options.Password);
                                                         });

            _configurator.Configure(config);
            _configurator.CreateReceiveEndpoints(host, _serviceProvider, config);

        }

        public void Dispose()
        {
            if (!_bus.IsValueCreated || !_bus.Value.IsCompleted)
            {
                return;
            }

            var bus = _bus.Value.Result;
            bus.Stop();
        }
    }
}
