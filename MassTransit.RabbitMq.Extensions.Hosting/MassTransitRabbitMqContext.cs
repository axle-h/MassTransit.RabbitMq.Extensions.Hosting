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
    /// <summary>
    /// A singleton context for managing the MassTransit bus.
    /// </summary>
    /// <seealso cref="MassTransit.RabbitMq.Extensions.Hosting.Contracts.IMassTransitRabbitMqContext" />
    public class MassTransitRabbitMqContext : IMassTransitRabbitMqContext
    {
        private readonly MassTransitRabbitMqHostingOptions _options;
        private readonly IMassTransitRabbitMqHostingConfigurator _configurator;
        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<Task<IBusControl>> _bus;
        private CancellationToken _cancellationToken;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitRabbitMqContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="configurator">The configurator.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public MassTransitRabbitMqContext(IOptions<MassTransitRabbitMqHostingOptions> options,
                                          IMassTransitRabbitMqHostingConfigurator configurator,
                                          IServiceProvider serviceProvider,
                                          ILoggerFactory loggerFactory)
        {
            _configurator = configurator;
            _serviceProvider = serviceProvider;
            _logger = loggerFactory.CreateLogger<MassTransitRabbitMqContext>();
            _loggerFactory = loggerFactory;
            _options = options.Value;
            _bus = new Lazy<Task<IBusControl>>(GetBusAsync, LazyThreadSafetyMode.ExecutionAndPublication);

            foreach (var s in configurator.GetConfigurationStrings())
            {
                _logger.LogInformation(s);
            }
        }

        /// <summary>
        /// Gets the configured MassTransit bus control.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <remarks>
        /// This will never complete if RabbitMQ is not up.
        /// </remarks>
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
                                                             c.Username(_options.RabbitMqUsername);
                                                             c.Password(_options.RabbitMqPassword);
                                                         });

            _configurator.Configure(config);
            _configurator.CreateReceiveEndpoints(host, _serviceProvider, config);

            config.UseExtensionsLogging(_loggerFactory);
            config.UseBsonSerializer(); // BSON by default. Because why not.
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
