using System.Threading.Tasks;

namespace MassTransit.RabbitMq.Extensions.Hosting.Contracts
{
    public interface ITypedSendEndpointProvider
    {
        Task<ISendEndpoint> GetSendEndpoint<TMessage>();
    }
}
