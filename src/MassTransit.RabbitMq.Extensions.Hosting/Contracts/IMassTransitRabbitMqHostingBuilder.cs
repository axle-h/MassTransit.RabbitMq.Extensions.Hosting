using System;
using GreenPipes.Configurators;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    /// <summary>
    /// MassTransit config builder.
    /// </summary>
    public interface IMassTransitRabbitMqHostingBuilder
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        string ApplicationName { get; }

        /// <summary>
        /// Configures a consumer of the specified type.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="retry">The optional retry configurator action.</param>
        /// <returns></returns>
        IMassTransitRabbitMqHostingBuilder Consume<TConsumer, TMessage>(string queueName, Action<IRetryConfigurator> retry = null)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class;

        /// <summary>
        /// Registers the specified action to be run on the RabbitMQ MassTransit bus factory.
        /// </summary>
        /// <param name="configure">The configure.</param>
        /// <returns></returns>
        IMassTransitRabbitMqHostingBuilder Configure(Action<IRabbitMqBusFactoryConfigurator> configure);

        /// <summary>
        /// Configures the specified send endpoint as fire and forget i.e. no response possible.
        /// To use this endpoint inject <see cref="IConfiguredSendEndpointProvider" /> and call <see cref="IConfiguredSendEndpointProvider.GetSendEndpoint{TMessage}" />.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        IMassTransitRabbitMqHostingBuilder WithFireAndForgetSendEndpoint<TMessage>(string queueName);

        /// <summary>
        /// Configures the specified send endpoint with response topology i.e. queues setup to receive responses.
        /// To use this endpoint inject <see cref="IConfiguredSendEndpointProvider" /> and call <see cref="IConfiguredSendEndpointProvider.GetRequestClient{TRequest, TResponse}" />.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="requestQueueName">Name of the request queue.</param>
        /// <param name="defaultTimeout">The optional default timeout. If not provided a global default will be used.</param>
        /// <returns></returns>
        IMassTransitRabbitMqHostingBuilder WithRequestResponseSendEndpoint<TRequest, TResponse>(string requestQueueName, TimeSpan? defaultTimeout = null);
    }
}