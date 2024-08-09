using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppLandingPage.Behaviors;
using AppLandingPage.Engines;
using AppLandingPage.Models;
using Caching.RedisWorker;
using DAL.Generic;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;

namespace AppLandingPage
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AppLandingPage - Main: " + ex);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
