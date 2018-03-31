using System;
using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Example.Client.Consumers
{
    public class CommandConsumer : IConsumer<ICommand>
    {
        private static readonly Random Random = new Random();
        private readonly ILogger<CommandConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public CommandConsumer(ILogger<CommandConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<ICommand> context)
        {
            if (Random.NextDouble() > 0.4)
            {
                _logger.LogWarning($"{context.Message.CorrelationId} Consumed-Command-Failed-{context.Message.Count}");
                throw new Exception("Very bad things happened");
            }

            _logger.LogInformation($"{context.Message.CorrelationId} Consumed-Command-{context.Message.Count}");

            await _publishEndpoint.Publish<IEvent>(new {context.Message.Count, context.Message.CorrelationId });
            _logger.LogInformation($"{context.Message.CorrelationId} Produced-Event-{context.Message.Count}");

            await context.RespondAsync<IResponse>(new {context.Message.Count, context.Message.CorrelationId });
            _logger.LogInformation($"{context.Message.CorrelationId} Produced-Response-{context.Message.Count}");
        }
    }
}