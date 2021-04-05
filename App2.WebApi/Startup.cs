using System;
using App2.WebApi.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace App2.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISqlRepository, SqlRepository>();
            services.AddTransient<IRabbitRepository, RabbitRepository>();

            services.AddControllers().AddNewtonsoftJson();
            services.AddOpenTelemetryTracing((sp, builder) =>
            {
                builder.AddAspNetCoreInstrumentation()
                    .AddSource(nameof(RabbitRepository))
                    .AddSqlClientInstrumentation()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App2"))
                    .AddJaegerExporter(opts =>
                    {
                        opts.AgentHost = Configuration["Jaeger:AgentHost"];
                        opts.AgentPort = Convert.ToInt32(Configuration["Jaeger:AgentPort"]);
                        opts.ExportProcessorType = ExportProcessorType.Simple;
                    });
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
