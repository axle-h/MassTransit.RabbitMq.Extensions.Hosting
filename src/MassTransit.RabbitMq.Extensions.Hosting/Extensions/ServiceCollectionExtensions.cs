using System;
using Humanizer;
using MassTransit.RabbitMq.Extensions.Hosting.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the MassTransit backed by RabbitMQ as a hosted service.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="configure">The configure action.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder AddMassTransitRabbitMqHostedService(this IServiceCollection services,
                                                                                             string applicationName,
                                                                                             Action<MassTransitRabbitMqHostingOptions> configure)
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException(nameof(applicationName));
            }

            services.Configure(configure);

            var builder = new MassTransitRabbitMqHostingBuilder(services, applicationName.Underscore());
            services.AddSingleton(c => builder.BuildConfigurator());
            services.AddSingleton(c => builder.BuildSendEndpointRepository());

            // This is the hosted service, i.e. the thing that makes sure the bus is started and stopped with the application.
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, MassTransitRabbitMqHostedService>();

            // We have to manage the bus via a context just in cast this service starts before RabbitMQ.
            services.AddSingleton<IMassTransitRabbitMqContext, MassTransitRabbitMqContext>();

            // I don't known where we'd consume this but I'm registering it for convenience as below.
            services.AddTransient(c => c.GetRequiredService<IMassTransitRabbitMqContext>().GetBusControlAsync().ConfigureAwait(false).GetAwaiter().GetResult());

            // Consume this to publish events.
            services.AddTransient<IPublishEndpoint>(c => c.GetRequiredService<IBusControl>());

            // Consume this to send commands to specific endpoints.
            services.AddTransient<ISendEndpointProvider>(c => c.GetRequiredService<IBusControl>());

            // Consume this to send commands to endpoints by the type.
            // You must have called IMassTransitRabbitMqHostingBuilder.WithSendEndpoint<TMessage> in order to use this for type TMessage.
            services.AddTransient<IConfiguredSendEndpointProvider, ConfiguredSendEndpointProvider>();

            return builder;
        }

        /// <summary>
        /// Adds the MassTransit backed by RabbitMQ as a hosted service.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="options">The mass transit options.</param>
        /// <returns></returns>
        public static IMassTransitRabbitMqHostingBuilder AddMassTransitRabbitMqHostedService(this IServiceCollection services,
                                                                                             string applicationName,
                                                                                             MassTransitRabbitMqHostingOptions options)
        {
            return services.AddMassTransitRabbitMqHostedService(applicationName, o =>
                                                                {
                                                                    o.RabbitMqUri = options.RabbitMqUri;
                                                                    o.RabbitMqUsername = options.RabbitMqUsername;
                                                                    o.RabbitMqPassword = options.RabbitMqPassword;
                                                                });
        }
    }
}
