using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

namespace Play.Inventory.Service
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
            Random jitterer = new();

            services.AddMongo()
                    .AddMongoRepository<InventoryItem>("inventory-items");

            services
                .AddHttpClient<CatalogClient>(client =>
                {
                    client.BaseAddress = new Uri("https://localhost:5001");
                })
                .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                    5,
                    retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                    onRetry: (outcome, timespan, retryAttemp) =>
                    {
                        var serviceProvider = services.BuildServiceProvider();
                        serviceProvider.GetService<ILogger<CatalogClient>>()?.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttemp}");
                    }
                ))
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1))
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    //avoid ssl error on wsl system
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    return handler;
                });

            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
