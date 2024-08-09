using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppReceiverAnalysCrawler_Jomashop.Common
{
    public static class LocalHelper
    {
        /// <summary>
        /// Kiểm tra xem Element có trong web không sử dụng Xpath
        /// </summary>
        /// <param name="chrome_driver"></param>
        /// <param name="elements_xpath"></param>
        /// <returns></returns>
        public static bool IsElementPresentByXpath(ChromeDriver chrome_driver, string elements_xpath)
        {
            try
            {
                chrome_driver.FindElement(By.XPath(elements_xpath));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        public static string Encode(string strString, string strKeyPhrase)
        {
            try
            {
                strString = KeyED(strString, strKeyPhrase);
                Byte[] byt = System.Text.Encoding.UTF8.GetBytes(strString);
                strString = Convert.ToBase64String(byt);
                return strString;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
        private static string KeyED(string strString, string strKeyphrase)
        {
            int strStringLength = strString.Length;
            int strKeyPhraseLength = strKeyphrase.Length;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(strString);

            for (int i = 0; i < strStringLength; i++)
            {
                int pos = i % strKeyPhraseLength;
                int xorCurrPos = (int)(strString[i]) ^ (int)(strKeyphrase[pos]);
                builder[i] = Convert.ToChar(xorCurrPos);
            }

            return builder.ToString();
        }
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
        public static string GetIMGLinkWithoutCache(string img_url)
        {
            try
            {
                string new_url = "";
                if (img_url.Contains("cache/"))
                {
                    bool cache_id_after = false;
                    string[] url_pattern = img_url.Split("/");
                    for (int i = 0; i < url_pattern.Length; i++)
                    {
                        if (!cache_id_after)
                        {
                            if (url_pattern[i] == "cache")
                            {
                                cache_id_after = true;
                                continue;
                            }
                            else
                            {
                                new_url = new_url + url_pattern[i];
                                if (i < (url_pattern.Length - 1)) new_url = new_url + "/";
                            }
                        }
                        else
                        {
                            cache_id_after = false;
                            continue;
                        }
                    }
                    return new_url;
                }
            }
            catch (Exception)
            {

            }
            return img_url;
        }
    }
}
