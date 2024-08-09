using App_AutoPurchase_TrackingOrders.Model;
using Entities.Models;
using Entities.ViewModels.AutomaticPurchase;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;

namespace App_AutoPurchase_TrackingOrders.Repositories
{
    public interface ITrackingAmazon
    {
        public MethodOutput Login(ChromeDriver driver, string user_name, string password, bool remembered = true, bool login_need_redirect = true);
        public MethodOutput DirectToTrackingPage(ChromeDriver driver, Dictionary<string,string> dictionary);
        public MethodOutput GatherInformation(ChromeDriver driver, AutomaticPurchaseAmz model, Dictionary<string,string> xpath);
        public MethodOutput CheckIfRefund(ChromeDriver driver, AutomaticPurchaseAmz model, Dictionary<string,string> xpath);

    }
}
