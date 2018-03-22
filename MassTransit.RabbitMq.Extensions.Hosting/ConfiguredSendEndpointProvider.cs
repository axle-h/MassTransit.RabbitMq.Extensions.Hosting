using System;
using System.Threading.Tasks;
using MassTransit.Pipeline;
using MassTransit.RabbitMq.Extensions.Hosting.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using Microsoft.Extensions.Options;

namespace MassTransit.RabbitMq.Extensions.Hosting
{
    /// <summary>
    /// Provider for creating send endpoints from configured URI's.
    /// <see cref="IMassTransitRabbitMqHostingBuilder"/>.
    /// </summary>
    /// <seealso cref="MassTransit.RabbitMq.Extensions.Hosting.Contracts.IConfiguredSendEndpointProvider" />
    public class ConfiguredSendEndpointProvider : IConfiguredSendEndpointProvider
    {
        private readonly IMassTransitRabbitMqEndpointRepository _configurator;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IBusControl _bus;
        private readonly string _rabbitMqUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredSendEndpointProvider" /> class.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        /// <param name="options">The options.</param>
        /// <param name="sendEndpointProvider">The send endpoint provider.</param>
        /// <param name="bus">The request pipe connector.</param>
        public ConfiguredSendEndpointProvider(IMassTransitRabbitMqEndpointRepository configurator,
                                              IOptions<MassTransitRabbitMqHostingOptions> options,
                                              ISendEndpointProvider sendEndpointProvider,
                                              IBusControl bus)
        {
            _configurator = configurator;
            _sendEndpointProvider = sendEndpointProvider;
            _bus = bus;
            _rabbitMqUri = options.Value.RabbitMqUri.ToString().TrimEnd('/');
        }

        /// <summary>
        /// Gets a send endpoint for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns></returns>
        public Task<ISendEndpoint> GetSendEndpoint<TMessage>()
        {
            var uri = GetSendEndpointUri<TMessage>();
            return _sendEndpointProvider.GetSendEndpoint(uri);
        }

        /// <summary>
        /// Gets a request client for the specified request and response message types.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <returns></returns>
        public IRequestClient<TRequest, TResponse> GetRequestClient<TRequest, TResponse>()
            where TRequest : class
            where TResponse : class
        {
            return _bus.CreateRequestClient<TRequest, TResponse>(GetSendEndpointUri<TRequest>(), _configurator.GetConfiguredRequestTimeout<TResponse>());
        }

        private Uri GetSendEndpointUri<TMessage>()
        {
            return new Uri($"{_rabbitMqUri}/{_configurator.GetSendEndpointPath<TMessage>()}");
        }
    }
}