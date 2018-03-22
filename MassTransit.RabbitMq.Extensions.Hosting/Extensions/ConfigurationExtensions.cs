using System;
using MassTransit.RabbitMq.Extensions.Hosting.Configuration;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Reads <see cref="MassTransitRabbitMqHostingOptions"/> from the specified configuration with section binding.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="section">The section.</param>
        /// <returns></returns>
        public static MassTransitRabbitMqHostingOptions GetMassTransitOptionsSection(this IConfiguration configuration,
                                                                                     string section = "MassTransit")
        {
            return configuration.GetSection(section).Get<MassTransitRabbitMqHostingOptions>();
        }

        /// <summary>
        /// Reads <see cref="MassTransitRabbitMqHostingOptions"/> from the specified configuration with a RabbitMQ (amqp://) connection string.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <returns></returns>
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
    }
}
