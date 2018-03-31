using System;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    /// <summary>
    /// A repository of endpoints and their configuration.
    /// </summary>
    public interface IMassTransitRabbitMqEndpointRepository
    {
        /// <summary>
        /// Gets the send endpoint path for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns></returns>
        string GetSendEndpointPath<TMessage>();

        /// <summary>
        /// Gets the receive receive timeout for the specified message type.
        /// </summary>
        /// <typeparam name="TResponseMessage">The type of the response message.</typeparam>
        /// <returns></returns>
        TimeSpan GetConfiguredRequestTimeout<TResponseMessage>();
    }
}