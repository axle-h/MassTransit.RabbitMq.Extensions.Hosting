using System;
using MassTransit.RabbitMqTransport;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    /// <summary>
    /// MassTransit config builder.
    /// </summary>
    public interface IMassTransitRabbitMqHostingBuilder
    {
        /// <summary>
        /// Configures a consumer of the specified type.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        IMassTransitRabbitMqHostingBuilder Consume<TConsumer, TMessage>(string queueName)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class;

        /// <summary>
        /// Registers the specified action to be run on the RabbitMQ MassTransit bus factory.
        /// </summary>
        /// <param name="configure">The configure.</param>
        /// <returns></returns>
        IMassTransitRabbitMqHostingBuilder Configure(Action<IRabbitMqBusFactoryConfigurator> configure);

        /// <summary>
        /// Configures the specified send endpoint.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        IMassTransitRabbitMqHostingBuilder WithSendEndpoint<TMessage>(string queueName);
    }
}