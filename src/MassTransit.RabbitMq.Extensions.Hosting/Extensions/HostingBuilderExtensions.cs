using System;
using System.Text.RegularExpressions;
using GreenPipes.Configurators;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IMassTransitRabbitMqHostingBuilder"/>.
    /// </summary>
    public static class HostingBuilderExtensions
    {
        private const string FaultQueuePostfix = "fault";
        private const string ErrorQueuePostfix = "error";

        /// <summary>
        /// Configures a send endpoint via convention.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="remoteApplicationName">Name of the remote application.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder WithSendEndpointByConvention<TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                                string remoteApplicationName)
            => builder.WithFireAndForgetSendEndpoint<TMessage>(GetQueueName<TMessage>(remoteApplicationName));

        /// <summary>
        /// Configures a consumer of the specified type via convention.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="retry">The optional retry configurator action.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeByConvention<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                                  Action<IRetryConfigurator> retry = null)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
            => builder.Consume<TConsumer, TMessage>(builder.GetQueueName<TMessage>(), retry);

        /// <summary>
        /// Configures a fault consumer of the specified type.
        /// This is for subscribing to <see cref="Fault{T}"/> events when using fire-and-forget messages.
        /// Note that send-receive messages do not publish faults.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="retry">The optional retry configurator action.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeFault<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                           string queueName,
                                                                                           Action<IRetryConfigurator> retry = null)
            where TConsumer : class, IConsumer<Fault<TMessage>>
            where TMessage : class
            => builder.Consume<TConsumer, Fault<TMessage>>($"{queueName}_{FaultQueuePostfix}", retry);

        /// <summary>
        /// Configures a fault consumer of the specified type via convention.
        /// This is for subscribing to <see cref="Fault{T}"/> events when using fire-and-forget messages.
        /// Note that send-receive messages do not publish faults.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="retry">The optional retry configurator action.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeFaultByConvention<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                                       Action<IRetryConfigurator> retry = null)
            where TConsumer : class, IConsumer<Fault<TMessage>>
            where TMessage : class
            => builder.Consume<TConsumer, Fault<TMessage>>($"{builder.GetQueueName<TMessage>()}_{FaultQueuePostfix}", retry);

        /// <summary>
        /// Configures an error consumer of the specified type.
        /// This is MassTransit's standard for hard failed messages.
        /// I.e. all messages that fail are routed to queues post-fixed with _error. Consume them again with this.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="retry">The optional retry configurator action.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeError<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                           string queueName,
                                                                                           Action<IRetryConfigurator> retry = null)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
            => builder.Consume<TConsumer, TMessage>($"{queueName}_{ErrorQueuePostfix}", retry);

        /// <summary>
        /// Configures an error consumer of the specified type via convention.
        /// This is MassTransit's standard for hard failed messages.
        /// I.e. all messages that fail are routed to queues post-fixed with _error. Consume them again with this.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="remoteApplicationName">Name of the remote application.</param>
        /// <param name="retry">The optional retry configurator action.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeErrorByConvention<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                                       string remoteApplicationName,
                                                                                                       Action<IRetryConfigurator> retry = null)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
            => builder.Consume<TConsumer, TMessage>($"{GetQueueName<TMessage>(remoteApplicationName)}_{ErrorQueuePostfix}", retry);

        /// <summary>
        /// Configures a send endpoint and a timeout for responses via convention.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="remoteApplicationName">Name of the remote application.</param>
        /// <param name="defaultTimeout">The default timeout.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">remoteApplicationName</exception>
        public static IMassTransitRabbitMqHostingBuilder WithRequestResponseSendEndpointByConvention<TRequest, TResponse>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                                                          string remoteApplicationName,
                                                                                                                          TimeSpan? defaultTimeout = null)
            => builder.WithRequestResponseSendEndpoint<TRequest, TResponse>(GetQueueName<TRequest>(remoteApplicationName), defaultTimeout);


        private static string GetQueueName<TMessage>(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException(nameof(applicationName));
            }

            var type = typeof(TMessage);
            var name = type.IsInterface && Regex.IsMatch(type.Name, "^I[A-Z]")
                           ? type.Name.Substring(1) // type is interface and looks like ISomeInterface
                           : type.Name;
            return $"{applicationName}_{name.ToSnailCase()}";
        }

        private static string GetQueueName<TMessage>(this IMassTransitRabbitMqHostingBuilder builder) => GetQueueName<TMessage>(builder.ApplicationName);
    }
}
