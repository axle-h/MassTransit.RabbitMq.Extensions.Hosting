using System;
using System.Collections.Generic;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.RabbitMq.Extensions.Hosting.Configuration
{
    internal class MassTransitRabbitMqHostingBuilder : IMassTransitRabbitMqHostingBuilder, IMassTransitRabbitMqHostingConfigurator
    {
        private readonly IServiceCollection _services;
        private readonly IDictionary<string, ICollection<Type>> _receivers;
        private readonly ICollection<Action<IRabbitMqBusFactoryConfigurator>> _configurators;
        private readonly IDictionary<Type, string> _sendEndpoints;

        public MassTransitRabbitMqHostingBuilder(IServiceCollection services)
        {
            _services = services;
            _receivers = new Dictionary<string, ICollection<Type>>(StringComparer.OrdinalIgnoreCase);
            _configurators = new List<Action<IRabbitMqBusFactoryConfigurator>>();
            _sendEndpoints = new Dictionary<Type, string>();
        }

        public IMassTransitRabbitMqHostingBuilder Configure(Action<IRabbitMqBusFactoryConfigurator> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            _configurators.Add(configure);
            return this;
        }

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

        public IMassTransitRabbitMqHostingBuilder WithSendEndpoint<TMessage>(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (_sendEndpoints.ContainsKey(typeof(TMessage)))
            {
                throw new ArgumentException($"Type already added: {typeof(TMessage)}", nameof(TMessage));
            }

            _sendEndpoints.Add(typeof(TMessage), path.TrimStart('/'));
            return this;
        }

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

        public void Configure(IRabbitMqBusFactoryConfigurator configurator)
        {
            foreach (var action in _configurators)
            {
                action(configurator);
            }
        }

        public string GetSendEndpointPath<TMessage>()
        {
            if (!_sendEndpoints.ContainsKey(typeof(TMessage)))
            {
                throw new ArgumentException($"Type not configured: {typeof(TMessage)}", nameof(TMessage));
            }

            return _sendEndpoints[typeof(TMessage)];
        }
    }
}