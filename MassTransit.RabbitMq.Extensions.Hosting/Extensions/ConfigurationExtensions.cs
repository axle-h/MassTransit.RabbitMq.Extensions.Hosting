using System;
using MassTransit.RabbitMq.Extensions.Hosting.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    public static class ConfigurationExtensions
    {
        public static MassTransitRabbitMqHostingOptions GetMassTransitOptionsSection(this IConfiguration configuration,
                                                                                     string section = "MassTransit")
        {
            return configuration.GetSection(section).Get<MassTransitRabbitMqHostingOptions>();
        }

        public static MassTransitRabbitMqHostingOptions GetMassTransitOptionsConnectionString(this IConfiguration configuration,
                                                                                              string connectionStringName = "rabbitmq")
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string does not exist: " + connectionString, nameof(connectionStringName));
            }

            if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Connection string is not a URI: " + connectionString, nameof(connectionStringName));
            }

            // We use the RabbitMQ connection factory to parse the connection string.
            ConnectionFactory factory;
            try
            {
                factory = new ConnectionFactory { Uri = uri };
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Connection string is not a RabbitMQ URI: " + connectionString, nameof(connectionStringName), e);
            }

            // MassTransit expects the uri to have a rabbitmq scheme without username, or password or any other gubbins.
            var rabbitMqUri = new UriBuilder($"rabbitmq://{factory.HostName}")
                              {
                                  Path = factory.VirtualHost
                              };

            return new MassTransitRabbitMqHostingOptions
                   {
                       RabbitMqUri = rabbitMqUri.Uri,
                       RabbitMqUsername = factory.UserName,
                       RabbitMqPassword = factory.Password
                   };
        }

        public static IMassTransitRabbitMqHostingBuilder WithTypeConventionSendEndpoint<TMessage>(this IMassTransitRabbitMqHostingBuilder builder)
            => builder.WithSendEndpoint<TMessage>(GetQueueName<TMessage>());

        public static IMassTransitRabbitMqHostingBuilder ConsumeWithTypeConvention<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
            => builder.Consume<TConsumer, TMessage>(GetQueueName<TMessage>());

        public static IMassTransitRabbitMqHostingBuilder ConsumeFault<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder,
                                                                                           string queueName)
            where TConsumer : class, IConsumer<Fault<TMessage>>
            where TMessage : class
            => builder.Consume<TConsumer, Fault<TMessage>>(queueName + "_error");

        private static string GetQueueName<TMessage>() => $"Queue.{typeof(TMessage)}";
    }
}
