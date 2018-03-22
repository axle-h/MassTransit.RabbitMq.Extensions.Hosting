using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Example.Client.Consumers
{
    public class EventConsumer : IConsumer<IEvent>
    {
        private readonly ILogger<EventConsumer> _logger;

        public EventConsumer(ILogger<EventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<IEvent> context)
        {
            _logger.LogInformation($"Command: {context.Message.Command}");
            return Task.CompletedTask;
        }
    }
}
