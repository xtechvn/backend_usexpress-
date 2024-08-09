using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_AutomaticPurchase_AMZ.Lib
{
    public static class XpathHelper
    {
        public static bool IsElementPresent(ChromeDriver driver, By by, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(by);
                return true;
            }
            catch (Exception ex)
            {
                element = null;
                return false;
            }
        }
        public static bool IsElementPresent(IWebElement parentElement, By by, out IWebElement element)
        {
            try
            {
                element = parentElement.FindElement(by);
                return true;
            }
            catch (Exception ex)
            {
                element = null;
                return false;
            }
        }
        public static bool IsElementPresents(ChromeDriver driver, By by, out IReadOnlyCollection<IWebElement> element)
        {
            try
            {
                element = driver.FindElements(by);
                return true;
            }
            catch (Exception ex)
            {
                element = null;
                return false;
            }
        }
        
    }
}
