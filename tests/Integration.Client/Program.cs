using System;
using System.Threading.Tasks;
using Bogus;
using Integration.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy;

namespace Integration.Client
{
    public class Program
    {
        private static readonly Faker<SomeCommand> SomeCommandFaker = new Faker<SomeCommand>()
                                                                      .RuleFor(x => x.CorrelationId, f => f.Random.Uuid())
                                                                      .RuleFor(x => x.PublishedDate, f => f.Date.Recent())
                                                                      .RuleFor(x => x.Bs, f => f.Company.Bs());

        public static async Task Main(string[] args)
        {
            await new MassTransitDummyWebHostBuilder(args, (configuration, options) => options.ApplicationName = "dummy_client")
                  .WithFireAndForgetSendEndpointByConvention<ISomeCommand>("dummy_server")
                  .WithMessageFactory<ISomeCommand>(() => SomeCommandFaker.Generate())
                  .RunAsync();
        }

        private class SomeCommand : ISomeCommand
        {
            public Guid CorrelationId { get; set; }

            public DateTime PublishedDate { get; set; }

            public string Bs { get; set; }
        }
    }
}
