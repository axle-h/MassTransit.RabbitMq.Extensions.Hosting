using System;
using MassTransit.RabbitMqTransport;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    public interface IMassTransitRabbitMqHostingConfigurator
    {
        void CreateReceiveEndpoints(IRabbitMqHost host, IServiceProvider provider, IRabbitMqBusFactoryConfigurator configure);

        void Configure(IRabbitMqBusFactoryConfigurator configurator);
        string GetSendEndpointPath<TMessage>();
    }
}