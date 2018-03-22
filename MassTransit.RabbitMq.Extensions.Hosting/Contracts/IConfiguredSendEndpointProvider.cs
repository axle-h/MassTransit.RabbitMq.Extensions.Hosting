using System.Threading.Tasks;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    /// <summary>
    /// Provider for creating send endpoints from configured URI's.
    /// <see cref="IMassTransitRabbitMqHostingBuilder"/>.
    /// </summary>
    public interface IConfiguredSendEndpointProvider
    {
        /// <summary>
        /// Gets a send endpoint for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns></returns>
        Task<ISendEndpoint> GetSendEndpoint<TMessage>();
    }
}
