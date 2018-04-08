using System.Threading.Tasks;
using Integration.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy;

namespace Integration.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await new MassTransitDummyWebHostBuilder(args, (configuration, options) => options.ApplicationName = "dummy_server")
                  .ConsumeByConvention<ISomeCommand>()
                  .RunAsync();
        }
    }
}
