using System.Linq;
using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Example.Client.Consumers
{
    public class CommandErrorConsumer : IConsumer<ICommand>
    {
        private readonly ILogger<CommandErrorConsumer> _logger;

        public CommandErrorConsumer(ILogger<CommandErrorConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ICommand> context)
        {
            _logger.LogError($"[Consumed-Command-Error-{context.Message.Count}] {context.Message.CorrelationId}");
            return Task.CompletedTask;
        }
    }
}
