using System;

namespace MassTransit.RabbitMq.Extensions.Hosting.Configuration
{
    public class MassTransitRabbitMqHostingOptions
    {
        /// <summary>
        /// Gets or sets the RabbitMQ URI.
        /// Should be in the form: <c>rabbitmq://localhost/vhost_name/queue_name</c>
        /// </summary>
        public Uri RabbitMqUri { get; set; }

        /// <summary>
        /// Gets or sets the RabbitMQ username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the RabbitMQ password.
        /// </summary>
        public string Password { get; set; }
    }
}
