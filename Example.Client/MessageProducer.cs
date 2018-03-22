using System;
using System.Threading;
using System.Threading.Tasks;
using Example.Client.Messages;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.Client
{
    public class MessageProducer : IHostedService
    {
        private readonly ITypedSendEndpointProvider _typedSendEndpointProvider;
        private Timer _timer;
        private readonly ILogger<MessageProducer> _logger;

        public MessageProducer(ITypedSendEndpointProvider typedSendEndpointProvider, ILogger<MessageProducer> logger)
        {
            _typedSendEndpointProvider = typedSendEndpointProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpoint = await _typedSendEndpointProvider.GetSendEndpoint<ICommand>();
            _timer = new Timer(c =>
                               {
                                   var msg = new {Date = DateTimeOffset.UtcNow, Id = Guid.NewGuid()};
                                   _logger.LogInformation($"Date: {msg.Date:s}, Id: {msg.Id}");
                                   endpoint.Send<ICommand>(msg, CancellationToken.None);
                               }, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }
    }
}
