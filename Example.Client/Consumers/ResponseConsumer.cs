using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Example.Client.Consumers
{
    public class ResponseConsumer : IConsumer<IResponse>
    {
        private readonly ILogger<ResponseConsumer> _logger;

        public ResponseConsumer(ILogger<ResponseConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<IResponse> context)
        {
            _logger.LogInformation($"[Consumed-Response-{context.Message.Count}] {context.Message.CorrelationId}");
            return Task.CompletedTask;
        }
    }
}
