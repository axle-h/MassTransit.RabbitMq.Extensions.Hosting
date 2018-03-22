using System;
using System.Threading.Tasks;
using MassTransit.RabbitMq.Extensions.Hosting.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using Microsoft.Extensions.Options;

namespace MassTransit.RabbitMq.Extensions.Hosting
{
    public class TypedSendEndpointProvider : ITypedSendEndpointProvider
    {
        private readonly IMassTransitRabbitMqHostingConfigurator _configurator;
        private readonly MassTransitRabbitMqHostingOptions _options;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public TypedSendEndpointProvider(IMassTransitRabbitMqHostingConfigurator configurator,
                                         IOptions<MassTransitRabbitMqHostingOptions> options,
                                         ISendEndpointProvider sendEndpointProvider)
        {
            _configurator = configurator;
            _sendEndpointProvider = sendEndpointProvider;
            _options = options.Value;
        }

        public Task<ISendEndpoint> GetSendEndpoint<TMessage>()
        {
            var path = _configurator.GetSendEndpointPath<TMessage>();
            var uri = new Uri($"{_options.RabbitMqUri.ToString().TrimEnd('/')}/{path}");
            return _sendEndpointProvider.GetSendEndpoint(uri);
        }
    }
}