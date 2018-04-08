using System;

namespace MassTransit.RabbitMq.Extensions.Hosting.Dummy.Configuration
{
    /// <summary>
    /// Options for the dummy MassTransit web host.
    /// </summary>
    public class DummyOptions
    {
        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the initial delay.
        /// </summary>
        public TimeSpan InitialDelay { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the response timeout.
        /// </summary>
        public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
