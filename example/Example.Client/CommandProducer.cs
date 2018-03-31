using System;
using System.Threading;
using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using Microsoft.Extensions.Logging;
using IHostedService = Microsoft.Extensions.Hosting.IHostedService;

namespace Example.Client
{
    public class CommandProducer : IHostedService
    {
        private readonly IConfiguredSendEndpointProvider _configuredSendEndpointProvider;
        private Timer _timer;
        private readonly ILogger<CommandProducer> _logger;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public CommandProducer(IConfiguredSendEndpointProvider configuredSendEndpointProvider, ILogger<CommandProducer> logger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _configuredSendEndpointProvider = configuredSendEndpointProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token).Token;
            
            var count = 0;
            _timer = new Timer(async c =>
                               {
                                   count++;

                                   var requestClient = _configuredSendEndpointProvider.GetRequestClient<ICommand, IResponse>();
                                   var command = new {Count = count, CorrelationId = Guid.NewGuid()};
                                   var responseTask = requestClient.Request(command, linkedToken);
                                   _logger.LogInformation($"{command.CorrelationId} Produced-Command-{command.Count}");

                                   // Or fire and forget
                                   // var endpoint = await _configuredSendEndpointProvider.GetSendEndpoint<ICommand>();
                                   // await endpoint.Send<ICommand>(command, linkedToken);

                                   try
                                   {
                                       var response = await responseTask;
                                       _logger.LogInformation($"{response.CorrelationId} Received-Response-{response.Count}");
                                   }
                                   catch (Exception e)
                                   {
                                       _logger.LogError($"{command.CorrelationId} Command-Fault-{command.Count} {e.Message}");
                                   }
                                   
                               }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _timer?.Dispose();
            return Task.CompletedTask;
        }
    }
}
