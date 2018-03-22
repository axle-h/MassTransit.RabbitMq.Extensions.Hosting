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
        private readonly IConfiguredSendEndpointProvider _configuredSendEndpointProvider;
        private Timer _timer;
        private readonly ILogger<CommandProducer> _logger;

        public CommandProducer(IConfiguredSendEndpointProvider configuredSendEndpointProvider, ILogger<CommandProducer> logger)
        {
            _configuredSendEndpointProvider = configuredSendEndpointProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpoint = await _configuredSendEndpointProvider.GetSendEndpoint<ICommand>();
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
