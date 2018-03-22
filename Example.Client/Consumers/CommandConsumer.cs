using System;
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

        public async Task Consume(ConsumeContext<ICommand> context)
        {
            _logger.LogInformation($"[Consumed-Command-{context.Message.Count}] {context.Message.CorrelationId}");

            var random = new Random(context.Message.Count);
            if (random.NextDouble() > 0.5)
            {
                throw new Exception("Very bad things happened");
            }

            await _publishEndpoint.Publish<IEvent>(new {context.Message.Count, context.Message.CorrelationId });
            _logger.LogInformation($"[Produced-Event-{context.Message.Count}] {context.Message.CorrelationId}");

            await context.RespondAsync<IResponse>(new {context.Message.Count, context.Message.CorrelationId });
            _logger.LogInformation($"[Produced-Response-{context.Message.Count}] {context.Message.CorrelationId}");
        }
    }
}