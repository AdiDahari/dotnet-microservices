using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

namespace PlatformService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private readonly IWebHostEnvironment _env;


        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (_env.IsProduction())
            {
                Console.WriteLine("Using SQLServer DB");
                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseSqlServer(Configuration.GetConnectionString("PlatformsConnection")));
            }
            else
            {
                Console.WriteLine("Using InMem DB");
                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseInMemoryDatabase("InMem"));
            }
            services.AddScoped<IPlatformRepository, PlatformRepository>();
            services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
            services.AddSingleton<IMessageBusClient, MeesageBusClient>();
            services.AddGrpc();
            services.AddControllers();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
            });

            Console.WriteLine($"Command Service path: {Configuration["CommandService"]}");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<GrpcPlatformService>();

                endpoints.MapGet("/protos/platforms.proto", async context =>
                {
                    await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
                });
            });

            PrepDb.PrepPopulation(app, _env.IsProduction());
        }
    }
}
