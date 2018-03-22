using MassTransit.RabbitMq.Extensions.Hosting.Contracts;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IMassTransitRabbitMqHostingBuilder WithTypeConventionSendEndpoint<TMessage>(this IMassTransitRabbitMqHostingBuilder builder)
            => builder.WithSendEndpoint<TMessage>(GetQueueName<TMessage>());

        public static IMassTransitRabbitMqHostingBuilder ConsumeWithTypeConvention<TConsumer, TMessage>(this IMassTransitRabbitMqHostingBuilder builder)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
            => builder.Consume<TConsumer, TMessage>(GetQueueName<TMessage>());

        private static string GetQueueName<TMessage>() => $"Queue.{typeof(TMessage)}";
    }
}
