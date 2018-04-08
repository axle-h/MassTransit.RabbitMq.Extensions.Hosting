using System;
using System.Collections.Generic;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Dummy.Configuration;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MassTransit.RabbitMq.Extensions.Hosting.Dummy
{
    public class Startup
    {
        private readonly string _applicationName;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<Action<IMassTransitRabbitMqHostingBuilder>> _builders;

        public Startup(IConfiguration configuration,
                       IOptions<DummyOptions> options,
                       IEnumerable<Action<IMassTransitRabbitMqHostingBuilder>> builders)
        {
            _applicationName = options.Value.ApplicationName;
            _configuration = configuration;
            _builders = builders;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            var massTransitOptions = _configuration.GetMassTransitOptionsConnectionString();
            var builder = services.AddMassTransitRabbitMqHostedService(_applicationName, massTransitOptions);
            foreach (var builderAction in _builders)
            {
                builderAction(builder);
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseMvc();
        }

    }
}
