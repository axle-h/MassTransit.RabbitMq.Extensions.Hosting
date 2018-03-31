using System;
using System.Threading.Tasks;
using Example.Client.Consumers;
using Example.Client.Messages;
using GreenPipes;
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
                                                    
                                                    // Create builder.
                                                    var builder = services.AddMassTransitRabbitMqHostedService(mode, massTransitOptions);

                                                    switch (mode)
                                                    {
                                                        case "server":
                                                            // Only server consumes the commands. On failure, will retry immediately, exactly once.
                                                            builder.ConsumeByConvention<CommandConsumer, ICommand>(r => r.Immediate(1));
                                                            break;

                                                        case "client":
                                                            // Client will produce commands and listen for responses whilst consuming events.
                                                            services.AddScoped<IHostedService, CommandProducer>();
                                                            builder.WithRequestResponseSendEndpointByConvention<ICommand, IResponse>("server")
                                                                   .ConsumeByConvention<EventConsumer, IEvent>();
                                                            break;

                                                        case "audit":
                                                            // Audit system only listens for events and failed commands.
                                                            builder.ConsumeByConvention<EventConsumer, IEvent>()
                                                                   .ConsumeErrorByConvention<CommandErrorConsumer, ICommand>("server");
                                                            break;

                                                        default:
                                                            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Expected mode to be server, client or audit");
                                                    }
                                                });
            await hostBuilder.RunConsoleAsync();
        }
    }
}
