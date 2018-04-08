using System;
using System.Threading.Tasks;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy.Background;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy.Consumers;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace MassTransit.RabbitMq.Extensions.Hosting.Dummy
{
    public class MassTransitDummyWebHostBuilder
    {
        private readonly IWebHostBuilder _builder;

        public MassTransitDummyWebHostBuilder(string[] args, Action<IConfiguration, DummyOptions> configurator)
        {
            _builder = WebHost.CreateDefaultBuilder(args)
                              .UseSerilog((context, configuration) => configuration.MinimumLevel.Information()
                                                                                   .MinimumLevel.Override("MassTransit.Messages", LogEventLevel.Error)
                                                                                   .WriteTo.Console())
                              .ConfigureServices((context, services) => services
                                                                        .AddSingleton<IMessageRepository, MessageRepository>()
                                                                        .Configure<DummyOptions>(o => configurator(context.Configuration, o)))
                              .UseStartup<Startup>();
        }

        public MassTransitDummyWebHostBuilder WithMessageFactory<TMessage>(Func<TMessage> factory)
            where TMessage : class
        {
            _builder.ConfigureServices(services => services.AddSingleton(factory));
            return this;
        }
        public MassTransitDummyWebHostBuilder WithRequestResponseSendEndpoint<TRequest, TResponse>(string requestQueueName)
            where TRequest : class, CorrelatedBy<Guid>
            where TResponse : class
        {
            AddHostedService<SendReceiveBackgroundService<TRequest, TResponse>>();
            return Add(o => o.WithRequestResponseSendEndpoint<TRequest, TResponse>(requestQueueName));
        }

        public MassTransitDummyWebHostBuilder WithRequestResponseSendEndpointByConvention<TRequest, TResponse>(string remoteApplicationName)
            where TRequest : class, CorrelatedBy<Guid>
            where TResponse : class
        {
            AddHostedService<SendReceiveBackgroundService<TRequest, TResponse>>();
            return Add(o => o.WithRequestResponseSendEndpointByConvention<TRequest, TResponse>(remoteApplicationName));
        }

        public MassTransitDummyWebHostBuilder WithFireAndForgetSendEndpoint<TMessage>(string queueName)
            where TMessage : class, CorrelatedBy<Guid>
        {
            AddHostedService<FireAndForgetBackgroundService<TMessage>>();
            return Add(o => o.WithFireAndForgetSendEndpoint<TMessage>(queueName));
        }

        public MassTransitDummyWebHostBuilder WithFireAndForgetSendEndpointByConvention<TMessage>(string remoteApplicationName)
            where TMessage : class, CorrelatedBy<Guid>
        {
            AddHostedService<FireAndForgetBackgroundService<TMessage>>();
            return Add(o => o.WithFireAndForgetSendEndpointByConvention<TMessage>(remoteApplicationName));
        }

        public MassTransitDummyWebHostBuilder Consume<TMessage>(string queueName)
            where TMessage : class
        {
            return Add(o => o.Consume<NullConsumer<TMessage>, TMessage>(queueName));
        }

        public MassTransitDummyWebHostBuilder ConsumeByConvention<TMessage>()
            where TMessage : class
        {
            return Add(o => o.ConsumeByConvention<NullConsumer<TMessage>, TMessage>());
        }

        private MassTransitDummyWebHostBuilder Add(Action<IMassTransitRabbitMqHostingBuilder> action)
        {
            _builder.ConfigureServices(services => services.AddSingleton(action));
            return this;
        }

        private MassTransitDummyWebHostBuilder AddHostedService<THostedService>()
            where THostedService : class, Microsoft.Extensions.Hosting.IHostedService
        {
            _builder.ConfigureServices(services => services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, THostedService>());
            return this;
        }

        public Task RunAsync() => _builder.Build().RunAsync();
    }
}
