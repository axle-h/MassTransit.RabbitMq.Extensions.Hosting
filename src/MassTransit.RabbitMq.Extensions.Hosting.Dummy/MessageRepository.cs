using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransit.RabbitMq.Extensions.Hosting.Dummy
{
    public interface IMessageRepository : IDisposable
    {
        Task<object> NextPublishedAsync(string type);

        Task<object> NextResponseAsync(string type);

        Task<object> NextConsumedAsync(string type);

        void AddPublished<TMessage>(TMessage message);

        void AddResponse<TMessage>(TMessage message);

        void AddConsumed<TMessage>(TMessage message);
    }

    public class MessageRepository : IMessageRepository
    {
        private const string PublishedKey = "published";
        private const string ResponseKey = "response";
        private const string ConsumedKey = "consumed";
        private readonly ConcurrentDictionary<string, BlockingCollection<object>> _queues;
        private readonly DummyOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(IOptions<DummyOptions> options, IHttpContextAccessor httpContextAccessor, ILogger<MessageRepository> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _options = options.Value;
            _queues = new ConcurrentDictionary<string, BlockingCollection<object>>();
        }

        public Task<object> NextPublishedAsync(string type) => NextMessageAsync($"{PublishedKey}_{type}");

        public Task<object> NextResponseAsync(string type) => NextMessageAsync($"{ResponseKey}_{type}");

        public Task<object> NextConsumedAsync(string type) => NextMessageAsync($"{ConsumedKey}_{type}");

        public void AddPublished<TMessage>(TMessage message) => AddMessage<TMessage>(PublishedKey, message);

        public void AddResponse<TMessage>(TMessage message) => AddMessage<TMessage>(ResponseKey, message);

        public void AddConsumed<TMessage>(TMessage message) => AddMessage<TMessage>(ConsumedKey, message);

        private async Task<object> NextMessageAsync(string type)
        {
            // Run blocking calls on a thread-pool thread.
            return await Task.Run(() =>
                                  {
                                      var queue = _queues.GetOrAdd(type, s => new BlockingCollection<object>());
                                      queue.TryTake(out var message, Timeout.Infinite, RequestAborted);
                                      return message;
                                  }, RequestAborted);
        }

        private void AddMessage<TMessage>(string prefixKey, object message)
        {
            var key = $"{prefixKey}_{typeof(TMessage).GetSnailName()}";
            _logger.LogInformation("Adding message to repository with key: " + key);
            var queue = _queues.GetOrAdd(key, s => new BlockingCollection<object>());
            queue.Add(message);
        }

        private CancellationToken RequestAborted
        {
            get
            {
                var cts = new CancellationTokenSource(_options.ResponseTimeout);
                var requestAborted = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
                return CancellationTokenSource.CreateLinkedTokenSource(cts.Token, requestAborted).Token;
            }
        }

        public void Dispose()
        {
            foreach (var queue in _queues)
            {
                queue.Value?.Dispose();
            }
        }
    }
}
