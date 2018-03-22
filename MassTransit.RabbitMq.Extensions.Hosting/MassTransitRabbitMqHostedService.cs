using System.Threading;
using System.Threading.Tasks;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;

namespace MassTransit.RabbitMq.Extensions.Hosting
{
    public class MassTransitRabbitMqHostedService : Microsoft.Extensions.Hosting.IHostedService
    {
        private readonly IMassTransitRabbitMqContext _context;

        public MassTransitRabbitMqHostedService(IMassTransitRabbitMqContext context)
        {
            _context = context;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // This starts the bus.
            await _context.GetBusControlAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var bus = await _context.GetBusControlAsync(cancellationToken);
            await bus.StopAsync(cancellationToken);
        }
    }
}
