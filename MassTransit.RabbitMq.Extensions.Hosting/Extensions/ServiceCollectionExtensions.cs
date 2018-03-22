using System;
using MassTransit.RabbitMq.Extensions.Hosting.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IMassTransitRabbitMqHostingBuilder AddMassTransitRabbitMqHostedService(this IServiceCollection services,
                                                                                             Action<MassTransitRabbitMqHostingOptions> configure)
        {
            services.Configure(configure);

            var builder = new MassTransitRabbitMqHostingBuilder(services);
            services.AddSingleton<IMassTransitRabbitMqHostingConfigurator>(builder);

            // This is the hosted service, i.e. the thing that makes sure the bus is started and stopped with the application.
            services.AddScoped<Microsoft.Extensions.Hosting.IHostedService, MassTransitRabbitMqHostedService>();

            // We have to manage the bus via a context just in cast this service starts before RabbitMQ.
            services.AddSingleton<IMassTransitRabbitMqContext, MassTransitRabbitMqContext>();

            // I don't known where we'd consume this but I'm registering it for convenience as below.
            services.AddSingleton(c => c.GetRequiredService<IMassTransitRabbitMqContext>().GetBusControlAsync().ConfigureAwait(false).GetAwaiter().GetResult());

            // Consume this to publish events.
            services.AddSingleton<IPublishEndpoint>(c => c.GetRequiredService<IBusControl>());

            // Consume this to send commands to specific endpoints.
            services.AddSingleton<ISendEndpointProvider>(c => c.GetRequiredService<IBusControl>());

            // Consume this to send commands to endpoints by the type.
            // You must have called IMassTransitRabbitMqHostingBuilder.WithSendEndpoint<TMessage> in order to use this for type TMessage.
            services.AddTransient<ITypedSendEndpointProvider, TypedSendEndpointProvider>();

            return builder;
        }

        public static IMassTransitRabbitMqHostingBuilder AddMassTransitRabbitMqHostedService(this IServiceCollection services,
                                                                                             MassTransitRabbitMqHostingOptions options)
        {
            return services.AddMassTransitRabbitMqHostedService(o =>
                                                                {
                                                                    o.RabbitMqUri = options.RabbitMqUri;
                                                                    o.RabbitMqUsername = options.RabbitMqUsername;
                                                                    o.RabbitMqPassword = options.RabbitMqPassword;
                                                                });
        }
    }
}
