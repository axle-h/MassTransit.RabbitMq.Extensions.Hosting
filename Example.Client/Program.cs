using System.Threading.Tasks;
using Example.Client.Consumers;
using Example.Client.Messages;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Example.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                              .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
                              .ConfigureAppConfiguration((context, config) => config.AddEnvironmentVariables())
                              .ConfigureLogging((context, builder) =>
                                                {
                                                    var logger = new LoggerConfiguration()
                                                                 .MinimumLevel.Debug()
                                                                 .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                                                                 .CreateLogger();
                                                    builder.AddSerilog(logger);
                                                })
                              .ConfigureServices((context, services) =>
                              {
                                  // Config.
                                  services.AddOptions();

                                  // Logging
                                  services.AddLogging();

                                  services.AddMassTransitRabbitMqHostedService()
                                          .ConsumeWithTypeConvention<CommandConsumer, ICommand>()
                                          .ConsumeWithTypeConvention<EventConsumer, IEvent>()
                                          .WithTypeConventionSendEndpoint<ICommand>();

                                  services.AddScoped<IHostedService, MessageProducer>();
                              });
            await hostBuilder.RunConsoleAsync();
        }
    }
}
