using System.Threading;
using System.Threading.Tasks;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;

namespace MassTransit.RabbitMq.Extensions.Hosting
{
    /// <summary>
    /// A hosted service for ensuring that the MassTransit bus is started and stopped.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.IHostedService" />
    public class MassTransitRabbitMqHostedService : Microsoft.Extensions.Hosting.IHostedService
    {
        private readonly IMassTransitRabbitMqContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitRabbitMqHostedService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public MassTransitRabbitMqHostedService(IMassTransitRabbitMqContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // This starts the bus.
            await _context.GetBusControlAsync(cancellationToken);
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var bus = await _context.GetBusControlAsync(cancellationToken);
            await bus.StopAsync(cancellationToken);
        }
    }
}
