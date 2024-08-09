using App_AutomaticPurchase_AMZ.Repositories;
using App_Crawl_TrackingFlightPackages.Lib;
using App_Crawl_TrackingFlightPackages.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Contants;

namespace App_Crawl_TrackingFlightPackages
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IUsExAPI _usExAPI;
        private readonly IConfiguration _configuration;
        private readonly ITrackingFlight _trackingFlight;
        private ChromeDriver _driver;
        private string app_path = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\net6.0", "");
        public Worker(ILogger<Worker> logger, IUsExAPI usExAPI,  IConfiguration configuration, ITrackingFlight trackingFlight)
        {
            _logger = logger;
            _usExAPI = usExAPI;
            _configuration = configuration;
            _trackingFlight = trackingFlight;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {

            var is_logged = ChromeInitilization().Result;
            if (is_logged)
            {
                return base.StartAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("Closing Service ...");
                return base.StopAsync(cancellationToken);
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                //-- Excuting Service
                await ExcuteService(stoppingToken);
                _logger.LogInformation("Excute Completed: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ". Wait 5 minutes");
                await Task.Delay(5*60 * 1000, stoppingToken);
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        private async Task ExcuteService(CancellationToken stoppingToken)
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error While ExcuteService: " + ex.ToString());
            }
        }

       
        /// <summary>
        /// Khởi tạo ChromeDriver + Login
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ChromeInitilization()
        {
            try
            {
                //-- Setting Option:
                var chrome_option = new ChromeOptions();
                /*
                {
                    BinaryLocation = @"C:\Program Files\Google\Chrome Beta\Application\chrome.exe",
                };*/
                chrome_option.AddArgument("--start-maximized");
                chrome_option.AddArgument("--log-level=3"); //Start Silently.
                chrome_option.AddArgument("--disable-remote-fonts");
                chrome_option.AddArgument("--disable-extensions");
                chrome_option.AddArgument("--disable-remote-fonts");

                //-- Setting ChromeDriver:
                _driver = new ChromeDriver(app_path, chrome_option, new TimeSpan(0, 0, 120));
                _driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 5);
                var homeUrl = new Uri("https://www.brcargo.com/NEC_WEB/Tracking/QuickTracking/Index");
                _driver.Navigate().GoToUrl(homeUrl);
                await Task.Delay(1000);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogInformation("ChromeInitilization Error" + ex.ToString());
            }
            return false;
        }
        private void ClickElementAndWait(IWebElement element, int delay_in_seconds = 1)
        {
            if (element != null)
            {
                IJavaScriptExecutor executor = (IJavaScriptExecutor)_driver;
                executor.ExecuteScript("arguments[0].click();", element);
                Thread.Sleep(delay_in_seconds * 1000);
            }
        }
        private async Task<string> TakeScreenshot(string order_code, string step_name)
        {

            string file_path = ImageHelper.TakeScreenshot(_driver, step_name, order_code);
            var upload_image = await _usExAPI.UploadImage(file_path, _configuration["API_NEW:UploadImageDomain"]);
            if (upload_image.status_code == (int)MethodOutputStatusCode.Success)
            {
                return (string)upload_image.data;
            }
            else
            {
                _logger.LogInformation("TakeScreenshot Failed: " + upload_image.message);
                return null;
            }
        }
    }
}
