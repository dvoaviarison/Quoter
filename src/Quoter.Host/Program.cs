using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Quoter.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Logger.Information("Starting Quoter");

                Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                    .UseSerilog()
                    .Build()
                    .Run();

                Log.Logger.Information("Quoter stopped gracefully");
            }
            catch(Exception e)
            {
                Log.Logger.Error(e, "Quoter exited unexpectedly. {Message}", e.Message);
            }
        }
    }
}

