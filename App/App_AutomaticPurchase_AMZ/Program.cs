using App_AutomaticPurchase_AMZ.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App_AutomaticPurchase_AMZ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IAutoPurchaseAmz, AutoPurchaseAmzRepository>();
                    services.AddSingleton<IUsExAPI, UsExAPIRepository>();
                    services.AddSingleton<IUSExOldAPI, UsExOldAPIRepository>();
                    services.AddSingleton<IConfiguration>(provider => new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build());
                });
    }
}
