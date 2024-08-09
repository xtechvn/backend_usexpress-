using App_Crawl_Mapping_Receiver_Service_v2.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace App_Crawl_Mapping_Receiver_Service_v2
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private MainService _mainService;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _mainService = new MainService(_logger);

            return base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _mainService.ExcuteMainService();
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(300000, stoppingToken);
            }
        }
        
    }
    
}
