using System.Linq;
using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Example.Client.Consumers
{
    public class CommandFailedConsumer : IConsumer<Fault<ICommand>>
    {
        private readonly ILogger<CommandFailedConsumer> _logger;

        public CommandFailedConsumer(ILogger<CommandFailedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Fault<ICommand>> context)
        {
            var message = context.Message.Message;
            var exception = context.Message.Exceptions.FirstOrDefault()?.Message;

            _logger.LogError($"[Consumed-Command-Fault-{message.Count}] {message.CorrelationId} {exception}");
            return Task.CompletedTask;
        }
    }
}
