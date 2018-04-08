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
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _disposing;
        private IBusControl _bus;
        private bool _disposed;

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
            _semaphore = new SemaphoreSlim(1, 1);
            _disposing = new CancellationTokenSource();

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
        public async Task<IBusControl> GetBusControlAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MassTransitRabbitMqContext));
            }

            if (_bus == null)
            {
                var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_disposing.Token, cancellationToken);
                await CreateBusAsync(linkedToken.Token);
            }

            return _bus;
        }
        
        private async Task CreateBusAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                while (_bus == null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var bus = Bus.Factory.CreateUsingRabbitMq(Configure);
                        await bus.StartAsync(cancellationToken);
                        _bus = bus; // only assign once started.
                    }
                    catch (RabbitMqConnectionException)
                    {
                        _logger.LogError("Failed to connect to RabbitMQ, retrying");
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }
            finally
            {
                _semaphore.Release();
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
            lock (_disposing)
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    _bus.Stop();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to stop MassTransit bus");
                }

                _disposing.Cancel();
                _disposing.Dispose();
                _semaphore.Dispose();

                _disposed = true;
            }
        }
    }
}
