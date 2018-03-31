[![CircleCI](https://circleci.com/gh/axle-h/MassTransit.RabbitMq.Extensions.Hosting/tree/master.svg?&style=shield)](https://circleci.com/gh/axle-h/MassTransit.RabbitMq.Extensions.Hosting/tree/master)
[![NuGet](https://img.shields.io/nuget/v/MassTransit.RabbitMq.Extensions.Hosting.svg)](https://www.nuget.org/packages/MassTransit.RabbitMq.Extensions.Hosting/)

# MassTransit.RabbitMq.Extensions.Hosting

MassTransit configuration extensions for `Microsoft.Extensions.DependencyInjection` using `Microsoft.Extensions.Hosting.IHostedService` and RabbitMQ.

Some conventions assumed in this package:

* Queue naming via application name.
* Exchange routing via remote microservice name.
* Simplified concept of commands by whether they expect a response or are fire-and-forget.
* Standard fault queues for catching/auditing failed fire-and-forget commands.
* Standard error queues for auditing/debugging hard failed request-response commands.

Some features over just consuming the MassTransit NuGet package directly:

* Provides a standard configuration mechanism based on `Microsoft.Extensions.DependencyInjection`. There is official support for this but it is very poor - it literally only supports consumer injection and no way of assigning consumers to distinct receive endpoints.
* Provides an `Microsoft.Extensions.Hosting.IHostedService` that manages the massTransit bus lifetime as part of ASP.Net Core or the new hosted service provider.
* Supports starting the application without RabbitMQ being present: the first time the bus is used, the request thread will block until RabbitMQ is available.
* Setup via connection string with amqp scheme.
* Endpoint configuration at DI setup time: means we're not passing around connection strings everywhere.

## Installation

The package is available on NuGet.

```bash
dotnet add package MassTransit.RabbitMq.Extensions.Hosting
```

## Usage

To use in `Startup.cs` in ASP.Net Core or `IHostBuilder.ConfigureServices` in a hosted service application:

```C#
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // You need a RabbitMQ connection string with a amqp scheme for this to work.
    var massTransitOptions = _configuration.GetMassTransitOptionsConnectionString();

    // The application name will be prefixed to all consumer queues.
    // It must be shared with clients wishing to send a command.
    services.AddMassTransitRabbitMqHostedService("some_application_name", massTransitOptions)
            
            // Consuming by convention (a queue named some_application_name_some_command) with a single immediate retry.
            .ConsumeByConvention<CommandConsumer, ISomeCommand>(r => r.Immediate(1))
            
            // Setting up an endpoint by convention that supports the request-response pattern.
            // The convention based endpoint will send commands to some_remote_application_some_other_command.
            // The remote application should have a consumer setup with this interface by convention.
            .WithRequestResponseSendEndpointByConvention<ISomeOtherCommand, IResponse>("some_remote_application")
            
            // Setting up an endpoint by convention that supports the fire-and-forget pattern.
            // The convention is identical to the request-response endpoint above.
            .WithSendEndpointByConvention<ISomeDifferentCommand>("some_remote_application");
}
```

Registered consumers should inherit `IConsumer<TMessage>` for example:

```C#
public class CommandConsumer : IConsumer<ICommand>
{
    public async Task Consume(ConsumeContext<ICommand> context)
    {
        // Do some stuff

        // ...
        var _someResult = ...

        // Respond to client, preserving correlation ID.
        await context.RespondAsync<IResponse>(new {Result = _someResult, context.Message.CorrelationId });
    }
}
```

Clients should inject `IConfiguredSendEndpointProvider` and call `GetRequestClient<TRequest, TResponse>()` for request-response or `GetSendEndpoint<ICommand>()` for fire-and-forget e.g..

```C#
public class SomeClient
{
    private readonly IConfiguredSendEndpointProvider _configuredSendEndpointProvider;

    public SomeClient(IConfiguredSendEndpointProvider configuredSendEndpointProvider)
    {
        _configuredSendEndpointProvider = configuredSendEndpointProvider;
    }

    public async Task<IResponse> SendSomeCommand(ISomeCommand command, CancellationToken cancellationToken = default)
    {
        var requestClient = _configuredSendEndpointProvider.GetRequestClient<ISomeCommand, IResponse>();
        return await requestClient.Request(command, cancellationToken);
    }
}
```

## Example

See `example/Example.Client` for an example client (of a specific command), server (of that specific command) and auditing microservices.

Run `Run-Example.ps1` to see it working at scale in docker-compose. Whilst the example is running you can browse to `localhost:15672` to see the RabbitMQ management console i.e. to see the message flow in real-time.

Process:

1. Client sends a command to server command queue.
2. Server consumes the command:
3. Server will randomly fail to consume a command. When this happens:
   * The command is retried exactly once. If it succeeds this time then the normal success procedure is followed.
   * The retry fails so the server responds with an exception to the client.
   * MassTransit moves failed messages to a special `_error` queue: the audit service consumes these messages and logs them.
4. When the command is successfully consumed.
   * Server responds to client with a success message.
   * Server publishes an event for the command it processed.
   * Audit system and client log the event.

