using System;
using System.Threading.Tasks;
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
        private readonly IMassTransitRabbitMqHostingConfigurator _configurator;
        private readonly MassTransitRabbitMqHostingOptions _options;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredSendEndpointProvider"/> class.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        /// <param name="options">The options.</param>
        /// <param name="sendEndpointProvider">The send endpoint provider.</param>
        public ConfiguredSendEndpointProvider(IMassTransitRabbitMqHostingConfigurator configurator,
                                         IOptions<MassTransitRabbitMqHostingOptions> options,
                                         ISendEndpointProvider sendEndpointProvider)
        {
            _configurator = configurator;
            _sendEndpointProvider = sendEndpointProvider;
            _options = options.Value;
        }

        /// <summary>
        /// Gets a send endpoint for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns></returns>
        public Task<ISendEndpoint> GetSendEndpoint<TMessage>()
        {
            var path = _configurator.GetSendEndpointPath<TMessage>();
            var uri = new Uri($"{_options.RabbitMqUri.ToString().TrimEnd('/')}/{path}");
            return _sendEndpointProvider.GetSendEndpoint(uri);
        }
    }
}