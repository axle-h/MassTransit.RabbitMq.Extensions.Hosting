using System;
using System.Threading;
using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.Client
{
    public class CommandProducer : IHostedService
    {
        private readonly ITypedSendEndpointProvider _typedSendEndpointProvider;
        private Timer _timer;
        private readonly ILogger<CommandProducer> _logger;

        public CommandProducer(ITypedSendEndpointProvider typedSendEndpointProvider, ILogger<CommandProducer> logger)
        {
            _typedSendEndpointProvider = typedSendEndpointProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpoint = await _typedSendEndpointProvider.GetSendEndpoint<ICommand>();
            var count = 0;
            _timer = new Timer(c =>
                               {
                                   count++;
                                   endpoint.Send<ICommand>(new { Count = count, CorrelationId = Guid.NewGuid() }, CancellationToken.None);
                                   _logger.LogInformation($"[Produced-Command-{count}]");
                               }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }
    }
}
