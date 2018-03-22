using System;
using System.Text.RegularExpressions;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IMassTransitRabbitMqHostingBuilder"/>.
    /// </summary>
    public static class HostingBuilderExtensions
    {
        private const string FaultQueuePostfix = "error";
        private const string ResponseQueuePostfix = "response";

        /// <summary>
        /// Configures a send endpoint via convention.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="remoteApplicationName">Name of the remote application.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder WithSendEndpointByConvention<TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                                string remoteApplicationName)
        {
            if (string.IsNullOrEmpty(remoteApplicationName))
            {
                throw new ArgumentNullException(nameof(remoteApplicationName));
            }

            return builder.WithSendEndpoint<TMessage>(GetQueueName<TMessage>(remoteApplicationName));
        }

        /// <summary>
        /// Configures a consumer of the specified type via convention.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeByConvention<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
            => builder.Consume<TConsumer, TMessage>(GetQueueName<TMessage>());

        /// <summary>
        /// Configures a fault consumer of the specified type.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeFault<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                           string queueName)
            where TConsumer : class, IConsumer<Fault<TMessage>>
            where TMessage : class
            => builder.Consume<TConsumer, Fault<TMessage>>($"{queueName}_{FaultQueuePostfix}");

        /// <summary>
        /// Configures a fault consumer of the specified type via convention.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeFaultByConvention<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder)
            where TConsumer : class, IConsumer<Fault<TMessage>>
            where TMessage : class
            => builder.Consume<TConsumer, Fault<TMessage>>($"{GetQueueName<TMessage>()}_{FaultQueuePostfix}");

        /// <summary>
        /// Configures a response consumer of the specified type.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TResponseMessage">The type of the response message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeResponse<TConsumer, TResponseMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                                      string queueName)
            where TConsumer : class, IConsumer<TResponseMessage>
            where TResponseMessage : class
            => builder.Consume<TConsumer, TResponseMessage>($"{queueName}_{ResponseQueuePostfix}");

        /// <summary>
        /// Configures a response consumer of the specified type via convention.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TOriginalMessage">The type of the original message.</typeparam>
        /// <typeparam name="TResponseMessage">The type of the response message.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder ConsumeResponseByConvention<TConsumer, TOriginalMessage, TResponseMessage>(this IMassTransitRabbitMqHostingBuilder builder)
            where TConsumer : class, IConsumer<TResponseMessage>
            where TResponseMessage : class
            => builder.Consume<TConsumer, TResponseMessage>($"{GetQueueName<TOriginalMessage>()}_{ResponseQueuePostfix}");

        private static string GetQueueName<TMessage>(string applicationName = null)
        {
            var type = typeof(TMessage);
            var name = type.IsInterface && Regex.IsMatch(type.Name, "^I[A-Z]")
                           ? type.Name.Substring(1) // type is interface and looks like ISomeInterface
                           : type.Name;
            return $"{applicationName ?? ApplicationConstants.Name ?? throw new InvalidOperationException("Must set the application name")}_{name.ToSnailCase()}";
        }
    }
}
