using System;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    /// <summary>
    /// A singleton context for managing the MassTransit bus.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IMassTransitRabbitMqContext : IDisposable
    {
        /// <summary>
        /// Gets the configured MassTransit bus control.
        /// </summary>
        /// <remarks>This will never complete if RabbitMQ is not up.</remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IBusControl> GetBusControlAsync(CancellationToken cancellationToken = default);
    }
}