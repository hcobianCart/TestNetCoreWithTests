using HealthChecks.MongoDb;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace testAPI
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "testAPI", Version = "v1" });
            });

            

            services.AddHealthChecks()
                .AddCheck("appsettings", () =>
                {
                    Dictionary<string, object> options = new Dictionary<string, object>();
                    var ErrorStr = string.Empty;
                    if (string.IsNullOrEmpty(Configuration["Auth0Management:audience"]))
                        options.Add("Auth0Management__audience", "");
                    if (string.IsNullOrEmpty(Configuration["Auth0Management:Domain"]))
                        options.Add("Auth0Management__Domain", "");
                    if (string.IsNullOrEmpty(Configuration["Auth0Management:client_id"]))
                        options.Add("Auth0Management__client_id", "");
                    if (string.IsNullOrEmpty(Configuration["Auth0Management:client_secret"]))
                        options.Add("Auth0Management__client_secret", "");
                    if (string.IsNullOrEmpty(Configuration["Auth0:Domain"]))
                        options.Add("Auth0__Domain", "");
                    if (string.IsNullOrEmpty(Configuration["Auth0:ApiIdentifier"]))
                        options.Add("Auth0__ApiIdentifier", "");
                    if (string.IsNullOrEmpty(Configuration["CartDatabaseSettings:UsersCollectionName"]))
                        options.Add("CartDatabaseSettings__UsersCollectionName", "");
                    if (string.IsNullOrEmpty(Configuration["CartDatabaseSettings:ConnectionString"]))
                        options.Add("CartDatabaseSettings__ConnectionString", "");
                    if (string.IsNullOrEmpty(Configuration["CartDatabaseSettings:DatabaseName"]))
                        options.Add("CartDatabaseSettings__DatabaseName", "");

                    if (options.Count > 0)
                        return HealthCheckResult.Degraded($"Missing Configuration", null, options);
                    else
                        return HealthCheckResult.Healthy("appsettings file hasn't missing or empty values");
                }, new[] { "Configuration" });
            try
            {
                if (!string.IsNullOrEmpty(Configuration["CartDatabaseSettings:ConnectionString"]))
                {
                    services.AddSingleton(new MongoDbHealthCheck(Configuration["CartDatabaseSettings:ConnectionString"]));
                    services.AddHealthChecks().AddMongoDb(
                        mongodbConnectionString: Configuration["CartDatabaseSettings:ConnectionString"],
                        name: "Ping MongoDB",
                        failureStatus: HealthStatus.Unhealthy,
                        timeout: new TimeSpan(0, 0, 2),
                        tags: new[] { "DB" });
                }
            }
            catch (Exception ex)
            {
                services.AddHealthChecks().AddCheck("Ping MongoDB", () => HealthCheckResult.Unhealthy(" Error CartDatabaseSettings__ConnectionString ", ex), tags: new[] { "DB" });
            }
            try
            {
                if (!string.IsNullOrEmpty(Configuration["Auth0:Domain"]))
                    services.AddHealthChecks().AddUrlGroup(
                        uri: new Uri(Configuration["Auth0:Domain"]),
                        name: "Ping Auth0__Domain",
                        failureStatus: HealthStatus.Unhealthy,
                        timeout: new TimeSpan(0, 0, 2),
                        tags: new[] { "Endpoints" });
            }
            catch (Exception ex) {
                services.AddHealthChecks().AddCheck("Ping Auth0__Domain", () => HealthCheckResult.Unhealthy("Error Auth0__Domain ", ex), tags: new[] { "Endpoints" });
            }
            try
            {
                if (!string.IsNullOrEmpty(Configuration["Auth0Management:Domain"]))
                    services.AddHealthChecks().AddUrlGroup(
                        uri: new Uri(Configuration["Auth0Management:Domain"]),
                        name: "Ping Auth0Management__Domain",
                        failureStatus: HealthStatus.Unhealthy,
                        timeout: new TimeSpan(0, 0, 2),
                        tags: new[] { "Endpoints" });
            }
            catch (Exception ex)
            {
                services.AddHealthChecks().AddCheck("Ping Auth0Management__Domain", () => HealthCheckResult.Unhealthy("Error Auth0Management__Domain ", ex), tags: new[] { "Endpoints" });
            }
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                options.HttpsPort = 443;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "testAPI v1"));
            }


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/quickhealth", new HealthCheckOptions()
                {
                    Predicate = _ => false
                });
                endpoints.MapHealthChecks("/health/Configuration", new HealthCheckOptions()
                {
                    Predicate = reg => reg.Tags.Contains("Configuration"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/health/DB", new HealthCheckOptions()
                {
                    Predicate = reg => reg.Tags.Contains("DB"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/health/Endpoints", new HealthCheckOptions()
                {
                    Predicate = reg => reg.Tags.Contains("Endpoints"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapControllers();
            });
        }
    }
}
