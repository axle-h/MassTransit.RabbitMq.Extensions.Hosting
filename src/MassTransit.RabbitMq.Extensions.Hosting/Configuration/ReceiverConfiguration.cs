using System;
using System.Collections.Generic;
using GreenPipes.Configurators;

namespace MassTransit.RabbitMq.Extensions.Hosting.Configuration
{
    /// <summary>
    /// Configuration for a MassTransit receiver.
    /// </summary>
    public class ReceiverConfiguration
    {
        /// <summary>
        /// Gets or sets the types to consume on this configured receive endpoint.
        /// </summary>
        public ICollection<Type> Types { get; set; } = new List<Type>();

        /// <summary>
        /// Gets or sets the retry configurator to apply on this configured receive endpoint.
        /// </summary>
        public Action<IRetryConfigurator> RetryConfigurator { get; set; }
    }
}
