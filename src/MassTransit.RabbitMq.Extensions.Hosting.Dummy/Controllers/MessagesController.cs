using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MassTransit.RabbitMq.Extensions.Hosting.Dummy.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController
    {
        private readonly IMessageRepository _repository;

        public MessagesController(IMessageRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("response/{type}")]
        public Task<object> GetNextResponse(string type) => _repository.NextResponseAsync(type);

        [HttpGet("consumed/{type}")]
        public Task<object> GetNextConsumed(string type) => _repository.NextConsumedAsync(type);

        [HttpGet("published/{type}")]
        public Task<object> GetNextPublished(string type) => _repository.NextPublishedAsync(type);
    }
}
