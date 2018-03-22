using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.RabbitMq.Extensions.Hosting.Configuration
{
    /// <summary>
    /// MassTransit builder and configurator.
    /// </summary>
    /// <seealso cref="MassTransit.RabbitMq.Extensions.Hosting.Contracts.IMassTransitRabbitMqHostingBuilder" />
    /// <seealso cref="MassTransit.RabbitMq.Extensions.Hosting.Contracts.IMassTransitRabbitMqHostingConfigurator" />
    public class MassTransitRabbitMqHostingBuilder : IMassTransitRabbitMqHostingBuilder, IMassTransitRabbitMqHostingConfigurator
    {
        private readonly IServiceCollection _services;
        private readonly IDictionary<string, ICollection<Type>> _receivers;
        private readonly ICollection<Action<IRabbitMqBusFactoryConfigurator>> _configurators;
        private readonly IDictionary<Type, string> _sendEndpoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitRabbitMqHostingBuilder"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public MassTransitRabbitMqHostingBuilder(IServiceCollection services)
        {
            _services = services;
            _receivers = new Dictionary<string, ICollection<Type>>(StringComparer.OrdinalIgnoreCase);
            _configurators = new List<Action<IRabbitMqBusFactoryConfigurator>>();
            _sendEndpoints = new Dictionary<Type, string>();
        }

        /// <summary>
        /// Registers the specified action to be run on the RabbitMQ MassTransit bus factory.
        /// </summary>
        /// <param name="configure">The configure.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">configure</exception>
        public IMassTransitRabbitMqHostingBuilder Configure(Action<IRabbitMqBusFactoryConfigurator> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            _configurators.Add(configure);
            return this;
        }

        /// <summary>
        /// Configures a consumer of the specified type.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        public IMassTransitRabbitMqHostingBuilder Consume<TConsumer, TMessage>(string queueName)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
        {
            _services.AddTransient<TConsumer>();

            ICollection<Type> list;
            if (_receivers.ContainsKey(queueName))
            {
                list = _receivers[queueName];
            }
            else
            {
                list = new List<Type>();
                _receivers.Add(queueName, list);
            }

            list.Add(typeof(TConsumer));
            return this;
        }

        /// <summary>
        /// Configures the specified send endpoint.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">queueName</exception>
        /// <exception cref="ArgumentException">TMessage</exception>
        public IMassTransitRabbitMqHostingBuilder WithSendEndpoint<TMessage>(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            if (_sendEndpoints.ContainsKey(typeof(TMessage)))
            {
                throw new ArgumentException($"Type already added: {typeof(TMessage)}", nameof(TMessage));
            }

            _sendEndpoints.Add(typeof(TMessage), queueName.TrimStart('/'));
            return this;
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
                                                             foreach (var type in kvp.Value)
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
        /// Gets the send endpoint path for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException">TMessage</exception>
        public string GetSendEndpointPath<TMessage>()
        {
            if (!_sendEndpoints.ContainsKey(typeof(TMessage)))
            {
                throw new ArgumentException($"Type not configured: {typeof(TMessage)}", nameof(TMessage));
            }

            return _sendEndpoints[typeof(TMessage)];
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
                    messageType =  messageType.GetGenericArguments().First();
                }

                return messageType.Name;
            }

            var receivers = _receivers.Select(kvp => $"Receiving {string.Join(" and ", kvp.Value.Select(GetMessageType))} on {kvp.Key}");
            var sendEndpoints = _sendEndpoints.Select(kvp => $"Sending {kvp.Key.Name} to {kvp.Value}");
            return receivers.Concat(sendEndpoints);
        }
    }
}