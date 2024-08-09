using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace App_Crawl_Mapping_Receiver_Service_v2.Services
{
    public static class CommonServices
    {
        public static string GetASINFromURL(string url)
        {
            try
            {
                // regex lấy ra domain theo link
                var link = url.Replace("http://", "https://").Replace("%", "");
                Uri myUri = new Uri(url);
                string host = myUri.Host;
                if (link.Split('/')[0].IndexOf("http") == -1)
                {
                    link = host + link;
                }
                // Convert to Single Line
                link = link.Replace("/\n/g", "");
                // regex lay ra link ID sản phẩm
                //M1: "https://www.amazon.com/gp/aw/d/B07GB4X6T7/ref=ox_sc_act_image_1?smid=AY8DYQ3EFA9NJ&psc=1"
                //M2:  https://www.amazon.com/d/Eye-Creams/Hada-Labo-Tokyo-Correcting-Cream/B00OFTIP86/ref=sr_1_2_a_it?ie=UTF8&qid=1542617568&sr=8-2-spons&keywords=Hada+Labo+Tokyo&psc=1#customerReviews
                //Regex match ASIN trường hợp 1:
                string regex_url_case_1 = "https://" + host + "/([\\w-]+/)?(dp|gp/product|gp/aw/d)/(\\w+/)?(\\w{10})";
                //Regex match ASIN trường hợp 2
                string regex_url_case_2 = "(?:[/dp/]|$)([A-Z0-9]{10})";
                //Match trường hợp 1:
                var match = Regex.Match(link, regex_url_case_1);
                string asin_match;
                if (match != null)
                {
                    asin_match = match.Value;
                }
                //Trường hợp 2
                else
                {
                    match = Regex.Match(link, regex_url_case_2);
                    asin_match = match.Value;
                }
                // Lấy ra ASIN trên link
                var array = asin_match.Split('/');
                var asin = array[array.Length - 1];
                //p2FB07DFVYZ - Loại bỏ các ký tự không liên quan đến ASIN nếu có:
                if (asin.IndexOf("B0") >= 1)
                {
                    array = asin.Split("B0");
                    asin = "B0" + array[array.Length - 1];
                }
                return asin;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public static bool IsElementPresent(By by,ChromeDriver chromeDriver, out IWebElement element)
        {
            try
            {
                element = chromeDriver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                element = null;
                return false;
            }
        }
    }
}
