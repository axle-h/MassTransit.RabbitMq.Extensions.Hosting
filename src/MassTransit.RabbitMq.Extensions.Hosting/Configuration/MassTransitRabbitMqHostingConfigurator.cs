using System;
using System.Collections.Generic;
using System.Linq;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.RabbitMq.Extensions.Hosting.Configuration
{
    public class MassTransitRabbitMqHostingConfigurator : IMassTransitRabbitMqHostingConfigurator
    {
        private readonly IDictionary<string, ReceiverConfiguration> _receivers;
        private readonly ICollection<Action<IRabbitMqBusFactoryConfigurator>> _configurators;
        private readonly IDictionary<Type, string> _sendEndpoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitRabbitMqHostingConfigurator" /> class.
        /// </summary>
        /// <param name="receivers">The receivers.</param>
        /// <param name="configurators">The configurators.</param>
        /// <param name="sendEndpoints">The send endpoints.</param>
        public MassTransitRabbitMqHostingConfigurator(IDictionary<string, ReceiverConfiguration> receivers,
                                                      ICollection<Action<IRabbitMqBusFactoryConfigurator>> configurators,
                                                      IDictionary<Type, string> sendEndpoints)
        {
            _receivers = receivers;
            _configurators = configurators;
            _sendEndpoints = sendEndpoints;
        }

        /// <summary>
        /// Creates all configured receive endpoints using the specified host and service provider.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="configure">The configure.</param>
        public void CreateReceiveEndpoints(IRabbitMqHost host, IServiceProvider provider, IRabbitMqBusFactoryConfigurator configure)
        {
            foreach (var kvp in _receivers)
            {
                configure.ReceiveEndpoint(host, kvp.Key, c =>
                                                         {
                                                             if (kvp.Value.RetryConfigurator != null)
                                                             {
                                                                 c.UseRetry(kvp.Value.RetryConfigurator);
                                                             }

                                                             foreach (var type in kvp.Value.Types)
                                                             {
                                                                 c.Consumer(type, provider.GetRequiredService);
                                                             }
                                                         });
            }
        }

        /// <summary>
        /// Runs all configured bus factory actions.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        public void Configure(IRabbitMqBusFactoryConfigurator configurator)
        {
            foreach (var action in _configurators)
            {
                action(configurator);
            }
        }

        /// <summary>
        /// Gets a set of string that represent this configuration for logging.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetConfigurationStrings()
        {
            string GetMessageType(Type consumerType)
            {
                var consumerInterface = consumerType.GetInterfaces()
                                                    .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IConsumer<>));
                var messageType = consumerInterface.GetGenericArguments().First();
                if (messageType.IsGenericType && messageType.GetGenericTypeDefinition() == typeof(Fault<>))
                {
                    messageType = messageType.GetGenericArguments().First();
                }

                return messageType.Name;
            }

            var receivers = _receivers.Select(kvp => $"Receiving {string.Join(" and ", kvp.Value.Types.Select(GetMessageType))} on {kvp.Key}");
            var sendEndpoints = _sendEndpoints.Select(kvp => $"Sending {kvp.Key.Name} to {kvp.Value}");
            return receivers.Concat(sendEndpoints);
        }
    }
}