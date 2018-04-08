using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransit.RabbitMq.Extensions.Hosting.Dummy.Background
{
    public class SendReceiveBackgroundService<TRequest, TResponse> : BackgroundService
        where TRequest : class, CorrelatedBy<Guid>
        where TResponse : class
    {
        private readonly IConfiguredSendEndpointProvider _provider;
        private readonly DummyOptions _options;
        private readonly Func<TRequest> _factory;
        private readonly IMessageRepository _repository;
        private readonly ILogger<SendReceiveBackgroundService<TRequest, TResponse>> _logger;

        public SendReceiveBackgroundService(IConfiguredSendEndpointProvider provider,
                                            IOptions<DummyOptions> options,
                                            Func<TRequest> factory,
                                            IMessageRepository repository,
                                            ILogger<SendReceiveBackgroundService<TRequest, TResponse>> logger)
        {
            _provider = provider;
            _options = options.Value;
            _factory = factory;
            _repository = repository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(_options.InitialDelay, cancellationToken);

            var client = _provider.GetRequestClient<TRequest, TResponse>();

            var message = _factory();
            _logger.LogInformation($"[{message.CorrelationId}] Sending request-response message");

            _repository.AddPublished(message);
            var response = await client.Request(message, cancellationToken);
            _repository.AddResponse(response);
        }
    }
}
