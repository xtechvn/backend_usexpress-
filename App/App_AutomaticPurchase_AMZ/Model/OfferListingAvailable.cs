using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_AutomaticPurchase_AMZ.Model
{
    public class OfferListingAvailable
    {
        public IWebElement offer { get; set; }
        public bool is_prime { get; set; } = false;
        public string seller_name { get; set; }
        public string seller_URL { get; set; }
    }
}
