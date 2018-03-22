using System;
using MassTransit.RabbitMqTransport;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    public interface IMassTransitRabbitMqHostingBuilder
    {
        IMassTransitRabbitMqHostingBuilder Consume<TConsumer, TMessage>(string queueName)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class;

        IMassTransitRabbitMqHostingBuilder Configure(Action<IRabbitMqBusFactoryConfigurator> configure);

        IMassTransitRabbitMqHostingBuilder WithSendEndpoint<TMessage>(string path);
    }
}