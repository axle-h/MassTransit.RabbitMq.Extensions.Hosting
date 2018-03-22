using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Example.Client.Consumers
{
    public class CommandConsumer : IConsumer<ICommand>
    {
        private readonly ILogger<CommandConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public CommandConsumer(ILogger<CommandConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public Task Consume(ConsumeContext<ICommand> context)
        {
            var msg = $"Date: {context.Message.Date:s}, Id: {context.Message.Id}";
            _logger.LogInformation(msg);
            _publishEndpoint.Publish<IEvent>(new {Command = msg});
            return Task.CompletedTask;
        }
    }
}