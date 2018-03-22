using System;
using System.Collections.Generic;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;

namespace MassTransit.RabbitMq.Extensions.Hosting.Configuration
{
    public class MassTransitRabbitMqEndpointRepository : IMassTransitRabbitMqEndpointRepository
    {
        private readonly IDictionary<Type, string> _sendEndpoints;
        private readonly IDictionary<Type, TimeSpan> _responseTimeouts;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitRabbitMqEndpointRepository"/> class.
        /// </summary>
        /// <param name="sendEndpoints">The send endpoints.</param>
        /// <param name="responseTimeouts">The receive endpoints.</param>
        public MassTransitRabbitMqEndpointRepository(IDictionary<Type, string> sendEndpoints,
                                                     IDictionary<Type, TimeSpan> responseTimeouts)
        {
            _sendEndpoints = sendEndpoints;
            _responseTimeouts = responseTimeouts;
        }

        /// <summary>
        /// Gets the send endpoint path for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns></returns>
        public string GetSendEndpointPath<TMessage>()
        {
            if (!_sendEndpoints.TryGetValue(typeof(TMessage), out var path))
            {
                throw new ArgumentException($"Type not configured: {typeof(TMessage)}", nameof(TMessage));
            }

            return path;
        }

        /// <summary>
        /// Gets the receive receive timeout for the specified message type.
        /// </summary>
        /// <typeparam name="TResponseMessage">The type of the response message.</typeparam>
        /// <returns></returns>
        public TimeSpan GetConfiguredRequestTimeout<TResponseMessage>()
        {
            if (!_responseTimeouts.TryGetValue(typeof(TResponseMessage), out var timeout))
            {
                throw new ArgumentException($"Type not configured: {typeof(TResponseMessage)}", nameof(TResponseMessage));
            }
            
            return timeout;
        }
    }
}