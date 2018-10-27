using System;
using System.Collections.Generic;
using GreenPipes.Configurators;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MassTransit.RabbitMq.Extensions.Hosting.Configuration
{
    /// <summary>
    /// MassTransit config builder.
    /// This should be used to configure receive endpoints, consumers and the host itself.
    /// It will be used to construct <see cref="IMassTransitRabbitMqHostingConfigurator"/> and <see cref="IMassTransitRabbitMqEndpointRepository"/>.
    /// </summary>
    /// <seealso cref="IMassTransitRabbitMqHostingBuilder" />
    public class MassTransitRabbitMqHostingBuilder : IMassTransitRabbitMqHostingBuilder
    {

        private static readonly TimeSpan GlobalDefaultTimeout = TimeSpan.FromSeconds(30);
        private readonly IDictionary<string, ReceiverConfiguration> _receivers;
        private readonly ICollection<Action<IRabbitMqBusFactoryConfigurator>> _configurators;
        private readonly IDictionary<Type, string> _sendEndpoints;
        private readonly IDictionary<Type, TimeSpan> _responseTimeouts;

        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitRabbitMqHostingBuilder" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="applicationName">Name of the application.</param>
        public MassTransitRabbitMqHostingBuilder(IServiceCollection services, string applicationName)
        {
            Services = services;
            ApplicationName = applicationName;
            _receivers = new Dictionary<string, ReceiverConfiguration>(StringComparer.OrdinalIgnoreCase);
            _configurators = new List<Action<IRabbitMqBusFactoryConfigurator>>();
            _sendEndpoints = new Dictionary<Type, string>();
            _responseTimeouts = new Dictionary<Type, TimeSpan>();
        }

        internal IMassTransitRabbitMqHostingConfigurator BuildConfigurator() => new MassTransitRabbitMqHostingConfigurator(_receivers, _configurators, _sendEndpoints);

        internal IMassTransitRabbitMqEndpointRepository BuildSendEndpointRepository() => new MassTransitRabbitMqEndpointRepository(_sendEndpoints, _responseTimeouts);

        /// <summary>
        /// Gets the services.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; }

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
        /// <param name="retry">The optional retry configurator action.</param>
        /// <param name="receiveEndpointConfigurator">The optional endpoint configurator action.</param>
        /// <returns></returns>
        public IMassTransitRabbitMqHostingBuilder Consume<TConsumer, TMessage>(
            string queueName, 
            Action<IRetryConfigurator> retry = null,
            Action<IRabbitMqReceiveEndpointConfigurator> receiveEndpointConfigurator = null)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
        {
            Services.TryAddTransient<TConsumer>();

            ReceiverConfiguration config;
            if (_receivers.ContainsKey(queueName))
            {
                config = _receivers[queueName];
            }
            else
            {
                config = new ReceiverConfiguration();
                _receivers.Add(queueName, config);
            }

            config.Types.Add(typeof(TConsumer));

            if (retry != null)
            {
                if (config.RetryConfigurator != null)
                {
                    throw new ArgumentException("Retry policy already configured for queue: " + queueName, nameof(retry));
                }

                config.RetryConfigurator = retry;
            }

            if (receiveEndpointConfigurator != null)
            {
                if (config.ReceiveEndpointConfigurator != null)
                {
                    throw new ArgumentException(
                        "Endpoint configurator already configured for queue: " + queueName, 
                        nameof(receiveEndpointConfigurator));
                }

                config.ReceiveEndpointConfigurator = receiveEndpointConfigurator;
            }

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
        public IMassTransitRabbitMqHostingBuilder WithFireAndForgetSendEndpoint<TMessage>(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            if (_sendEndpoints.ContainsKey(typeof(TMessage)))
            {
                throw new ArgumentException($"Request type already added: {typeof(TMessage)}", nameof(TMessage));
            }

            _sendEndpoints.Add(typeof(TMessage), queueName.TrimStart('/'));
            return this;
        }

        /// <summary>
        /// Configures the specified send endpoint with response topology i.e. queues setup to receive responses.
        /// To use this endpoint inject <see cref="IConfiguredSendEndpointProvider" /> and call <see cref="IConfiguredSendEndpointProvider.GetRequestClient{TRequest, TResponse}" />.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="requestQueueName">Name of the request queue.</param>
        /// <param name="defaultTimeout">The optional default timeout. If not provided a global default will be used.</param>
        /// <returns></returns>
        public IMassTransitRabbitMqHostingBuilder WithRequestResponseSendEndpoint<TRequest, TResponse>(string requestQueueName, TimeSpan? defaultTimeout = null)
        {
            WithFireAndForgetSendEndpoint<TRequest>(requestQueueName);
            
            if (_responseTimeouts.ContainsKey(typeof(TResponse)))
            {
                throw new ArgumentException($"Response type already added: {typeof(TResponse)}", nameof(TResponse));
            }

            _responseTimeouts.Add(typeof(TResponse), defaultTimeout ?? GlobalDefaultTimeout);
            return this;
        }
    }
}