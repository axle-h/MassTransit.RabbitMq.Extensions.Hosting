using System;
using System.Collections.Generic;
using MassTransit.RabbitMqTransport;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    /// <summary>
    /// MassTransit configurator.
    /// </summary>
    public interface IMassTransitRabbitMqHostingConfigurator
    {
        /// <summary>
        /// Creates all configured receive endpoints using the specified host and service provider.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="configure">The configure.</param>
        void CreateReceiveEndpoints(IRabbitMqHost host, IServiceProvider provider, IRabbitMqBusFactoryConfigurator configure);

        /// <summary>
        /// Runs all configured bus factory actions.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        void Configure(IRabbitMqBusFactoryConfigurator configurator);

        /// <summary>
        /// Gets the send endpoint path for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns></returns>
        string GetSendEndpointPath<TMessage>();

        /// <summary>
        /// Gets a set of string that represent this configuration for logging.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetConfigurationStrings();
    }
}