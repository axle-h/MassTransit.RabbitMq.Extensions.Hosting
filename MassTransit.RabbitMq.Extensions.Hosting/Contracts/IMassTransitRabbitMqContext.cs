using System;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    public interface IMassTransitRabbitMqContext : IDisposable
    {
        Task<IBusControl> GetBusControlAsync(CancellationToken cancellationToken = default);
    }
}