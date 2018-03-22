using System;
using System.Threading.Tasks;
using Example.Client.Consumers;
using Example.Client.Messages;
using MassTransit;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using IHostedService = Microsoft.Extensions.Hosting.IHostedService;

namespace Example.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("MassTransit.Messages", LogEventLevel.Error)
                        .WriteTo.Console()
                        .CreateLogger();

            var hostBuilder = new HostBuilder()
                             .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
                             .ConfigureAppConfiguration((context, config) => config.SetBasePath(Environment.CurrentDirectory)
                                                                                   .AddJsonFile("appsettings.json")
                                                                                   .AddEnvironmentVariables())
                             .ConfigureLogging((context, builder) => builder.AddSerilog(logger))
                             .ConfigureServices((context, services) =>
                                                {
                                                    // Config.
                                                    services.AddOptions();
                                                    var mode = context.Configuration.GetValue<string>("mode");
                                                    var massTransitOptions = context.Configuration.GetMassTransitOptionsConnectionString();
                                                    
                                                    // Create builder. Bother server and client will consume the events.
                                                    var builder = services.AddMassTransitRabbitMqHostedService(massTransitOptions);

                                                    switch (mode)
                                                    {
                                                        case "server":
                                                            // Only server consumes the commands.
                                                            builder.Consume<CommandConsumer, ICommand>("example_command");
                                                            break;

                                                        case "client":
                                                            // Client will produce commands and then listen for responses.
                                                            services.AddScoped<IHostedService, CommandProducer>();
                                                            builder.WithSendEndpoint<ICommand>("example_command")
                                                                   .Consume<ResponseConsumer, IResponse>("example_command_response")
                                                                   .ConsumeFault<CommandFailedConsumer, ICommand>("example_command");
                                                            break;

                                                        case "audit":
                                                            // Audit system only listens for events.
                                                            builder.Consume<EventConsumer, IEvent>("example_event_audit");
                                                            break;

                                                        default:
                                                            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Expected mode to be server, client or audit");
                                                    }
                                                });
            await hostBuilder.RunConsoleAsync();
        }
    }
}
