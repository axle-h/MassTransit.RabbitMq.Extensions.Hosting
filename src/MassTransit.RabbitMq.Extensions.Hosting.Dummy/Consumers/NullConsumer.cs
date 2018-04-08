using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MassTransit.RabbitMq.Extensions.Hosting.Dummy.Consumers
{
    public class NullConsumer<TMessage> : IConsumer<TMessage>
        where TMessage : class
    {
        private readonly IMessageRepository _repository;
        private readonly ILogger<NullConsumer<TMessage>> _logger;

        public NullConsumer(IMessageRepository repository, ILogger<NullConsumer<TMessage>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<TMessage> context)
        {
            _logger.LogInformation($"[{context.CorrelationId}] Received message");
            _repository.AddConsumed(context.Message);
            return Task.CompletedTask;
        }
    }
}
