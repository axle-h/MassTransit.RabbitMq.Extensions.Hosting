using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransit.RabbitMq.Extensions.Hosting.Dummy.Background
{
    public class FireAndForgetBackgroundService<TMessage> : BackgroundService
        where TMessage : class, CorrelatedBy<Guid>
    {
        private readonly IConfiguredSendEndpointProvider _provider;
        private readonly DummyOptions _options;
        private readonly Func<TMessage> _factory;
        private readonly IMessageRepository _repository;
        private readonly ILogger<FireAndForgetBackgroundService<TMessage>> _logger;

        public FireAndForgetBackgroundService(IConfiguredSendEndpointProvider provider,
                                              IOptions<DummyOptions> options,
                                              Func<TMessage> factory,
                                              IMessageRepository repository,
                                              ILogger<FireAndForgetBackgroundService<TMessage>> logger)
        {
            _provider = provider;
            _factory = factory;
            _repository = repository;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(_options.InitialDelay, cancellationToken);

            var message = _factory();
            _logger.LogInformation($"[{message.CorrelationId}] Sending fire and forget message");

            var endpoint = await _provider.GetSendEndpoint<TMessage>();

            _repository.AddPublished(message);
            await endpoint.Send(message, cancellationToken);
        }
    }
}