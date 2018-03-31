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

        /// <summary>
        /// Gets a request client for the specified request and response message types.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <returns></returns>
        IRequestClient<TRequest, TResponse> GetRequestClient<TRequest, TResponse>()
            where TRequest : class
            where TResponse : class;
    }
}
