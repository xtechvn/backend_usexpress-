using App_Crawl_Mapping_Receiver_Service.Models;
using App_Crawl_Mapping_Receiver_Service_v2.Models;
using App_Crawl_Mapping_Receiver_Service_v2.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;

namespace App_Crawl_Mapping_Receiver_Service_v2.LabelServices
{
    public class AmazonService
    {
        private IConfiguration _configuration;
        public AmazonService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /*
        public LocalResultModel CrawlByRegex(SLQueueItem item, ChromeDriver driver, bool is_crawl_level_2 = false, int limit_item = -1)
        {
            try
            {

                //-- Go to URL
                driver.Navigate().GoToUrl(item.linkdetail);
                Thread.Sleep(2000);
                //-- Get Page Source:
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(driver.PageSource);
                document = HtmlDocumentFilter(document);

                List<SLProductItem> list_product = new List<SLProductItem>();
                List<SLQueueItem> list_url = new List<SLQueueItem>();
                LocalResultModel result = new LocalResultModel();
                //-- Get From Page Source:
                int case_count = Convert.ToInt32(_configuration["Regex_URL:Count"]) < 1 ? 0 : Convert.ToInt32(_configuration["Regex_URL:Count"]);
                for (int i = 0; i < case_count; i++)
                {
                    if (_configuration["Regex_URL:Case_" + i] != null)
                    {
                        MatchCollection matchList = Regex.Matches(document.DocumentNode.OuterHtml, _configuration["Regex_URL:Case_" + i], RegexOptions.IgnoreCase);
                        var list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                        foreach (string url in list)
                        {
                            string check_url; int count_item = 0;
                            if (url.Contains("http")) check_url = url;
                            else check_url = "https://www.amazon.com" + url;
                            string asin = CommonServices.GetASINFromURL(check_url);
                            if (asin != null && asin != "")
                            {
                                if (!list_product.Any(x => x.product_code == asin))
                                {
                                    SLProductItem list_item = new SLProductItem();
                                    list_item.group_id = item.groupProductid;
                                    list_item.label_id = item.labelid;
                                    list_item.from_parent_url = item.linkdetail;
                                    list_item.url = check_url;
                                    list_item.product_code = CommonServices.GetASINFromURL(check_url);
                                    list_product.Add(list_item);
                                    count_item++;
                                }
                                else if (!is_crawl_level_2)
                                {
                                    list_url.Add(new SLQueueItem()
                                    {
                                        groupProductid = item.groupProductid,
                                        labelid = item.labelid,
                                        linkdetail = check_url
                                    });
                                }
                                if (is_crawl_level_2 && count_item > limit_item) break;
                            }
                        }
                        if (list_product.Count > 0)
                        {
                            list_product = list_product.Distinct().ToList();
                            Console.Write("Regex_URL Case: " + i + " . ");
                            result.list_product = list_product;
                            result.list_url = list_url;
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("App_Crawl_Mapping_Receiver_Service - URLFromGrid: Error - " + ex.ToString());
            }
            return null;
        }*/
        public LocalResultModel CrawlByXpath(SLQueueItem item, ChromeDriver driver, bool is_lv2_crawl = false, int limit_item = -1, int page_crawl=1,int max_item=60)
        {
            List<SLProductItem> list_product = new List<SLProductItem>();
            List<SLQueueItem> list_url = new List<SLQueueItem>();
            LocalResultModel result = new LocalResultModel();
            result.list_product = new List<SLProductItem>();
            //-- Go to URL
            driver.Navigate().GoToUrl(item.linkdetail);
            /*
            if (!FilterURL(driver))
            {
                LogHelper.InsertLogTelegram("Link is not checked" + driver.Url);
                return result;
            }*/
            Thread.Sleep(2000);
            //-- Get Page Source:
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(driver.PageSource);
            document = HtmlDocumentFilter(document);
            //-- excute:
            int case_source_source_count = Convert.ToInt32(_configuration["Xpath_search_case_source_count"]) < 1 ? 0 : Convert.ToInt32(_configuration["Xpath_search_case_source_count"]);
            int Xpath_case_count = Convert.ToInt32(_configuration["Xpath_case_count"]) < 1 ? 0 : Convert.ToInt32(_configuration["Xpath_case_count"]);
            int Xpath_url_case_count = Convert.ToInt32(_configuration["Xpath_URL_Count"]) < 1 ? 0 : Convert.ToInt32(_configuration["Xpath_URL_Count"]);
            List<string> xpath_url_list = new List<string>();
            for (int i = 0; i < Xpath_url_case_count; i++)
            {
                if (_configuration["Xpath_URL_" + i]!=null&& _configuration["Xpath_URL_" + i].Trim() != "")
                {
                    xpath_url_list.Add(_configuration["Xpath_URL_" + i]);
                }
            }
            //-- Get from page_source:
            for (int i = 0; i < case_source_source_count; i++)
            {
                var search_result_as_list = document.DocumentNode.SelectNodes(_configuration["Xpath_search_case_source_" + i]);
                if (search_result_as_list != null && search_result_as_list.Count > 0)
                {
                    foreach (var box_item in search_result_as_list)
                    {
                        int count_item = 0;
                        string url = GetURL_Case_Source(box_item, xpath_url_list);
                        if (url == null || url.Trim() == "")
                        {
                            continue;
                        }
                        string product_code = CommonServices.GetASINFromURL(url);
                        if (product_code != null && url != "" && product_code != "")
                        {
                            SLProductItem list_item = new SLProductItem();
                            list_item.group_id = item.groupProductid;
                            list_item.label_id = item.labelid;
                            list_item.from_parent_url = item.linkdetail;
                            list_item.url = url + (url.Trim().EndsWith("?th=1&psc=1")?"": "?th=1&psc=1");
                            list_item.product_code = product_code;
                            list_product.Add(list_item);
                        }

                        else if (!is_lv2_crawl)
                        {
                            if (url.StartsWith("https://www.amazon.com/gp/slredirect/picassoRedirect.html") || !url.StartsWith("https://www.amazon.com/deal/"))
                            {
                                continue;
                            }
                            list_url.Add(new SLQueueItem()
                            {
                                groupProductid = item.groupProductid,
                                labelid = item.labelid,
                                linkdetail = url
                            });
                        }
                        if (is_lv2_crawl && count_item > limit_item) break;
                    }
                    if (list_product != null && list_product.Count > 0)
                    {
                        Console.Write(" - Case Found: Xpath_search_case_source_" + i + " - ");
                        result.list_product = list_product;
                        result.list_url = list_url;
                        break;
                    }
                }
            }

            //document.LoadHtml(driver.FindElementByTagName("html").GetAttribute("outerHTML"));

            //-- Case Xpath with Chromedriver
            /*
            for (int i = 0; i < Xpath_case_count; i++)
            {
                var addition_list_product = new List<SLProductItem>();
                if (IsElementExistByXpath(driver, _configuration["Xpath_search_case_" + i ]))
                {
                    var grid_box = driver.FindElements(By.XPath(_configuration["Xpath_search_case_" + i]));
                    foreach (var search_item in grid_box)
                    {
                        string url = GetURL_FromXpath(search_item, xpath_url_list);
                        int count_item = 0;
                        if (url != null && url != "")
                        {
                            string product_code = CommonServices.GetASINFromURL(url);
                            if (product_code != null && product_code != "")
                            {
                                SLProductItem list_item = new SLProductItem();
                                list_item.group_id = item.groupProductid;
                                list_item.label_id = item.labelid;
                                list_item.from_parent_url = item.linkdetail;
                                list_item.url = url;
                                list_item.product_code = product_code;
                                addition_list_product.Add(list_item);

                            }

                            else if (!is_lv2_crawl)
                            {
                                list_url.Add(new SLQueueItem()
                                {
                                    groupProductid = item.groupProductid,
                                    labelid = item.labelid,
                                    linkdetail = url
                                });
                            }
                            if (is_lv2_crawl && count_item > limit_item) break;
                        }
                    }

                }
                if (addition_list_product != null && addition_list_product.Count > 0)
                {
                    Console.Write(" - Case Found: Xpath_search_case_" + i + " - ");
                    result.list_product.AddRange(addition_list_product);
                    result.list_url = list_url;
                    break;
                }
            }*/
            //-- Crawl next page if total item doesnt greather than 50
            /*
            if( result.list_product.Count< 50 && (page_crawl <= 1 || page_crawl > 10))
            {
                for(var i = 2; i < 10; i++)
                {
                    var url_node = document.DocumentNode.SelectNodes("//ul[contains(@class,\"a-pagination\")]//li[contains(@class,\"a-normal\")]//a");
                    if (url_node == null || url_node.Count < 1) break;
                    else
                    {
                        var next_item = item;
                        foreach(var atag in url_node)
                        {
                            if(atag.Attributes["href"].ToString().Trim()== i.ToString())
                            {
                                next_item.linkdetail = (atag.Attributes["href"].ToString().Trim().StartsWith("https://www.amazon.com") || atag.Attributes["href"].ToString().Trim() == "#") ? atag.Attributes["href"].ToString() : "https://www.amazon.com" + atag.Attributes["href"].ToString();
                                break;
                            }
                        }
                        if (next_item.linkdetail == "#") break;
                        var list_next_page = CrawlByXpath(next_item, driver, false, -1, i);
                        foreach (var a in list_next_page.list_product)
                        {
                            if (result.list_product.Count >= 50) break;
                            else
                            {
                                result.list_product.Add(a);
                            }
                        }
                    }
                }
            }*/
            return result;
        }
        public bool FilterURL(ChromeDriver driver)
        {
            var is_selected = false;
            try
            {
                if (driver.Url.Trim().Contains("/deals?") || driver.Url.Trim().Contains("/gp/goldbox?"))
                {
                    var dom = driver.FindElementsByXPath("//a[contains(@class,\"LinkFilterOption-module__linkFilterOption_\")]");
                    if (dom != null && dom.Count > 0)
                    {
                        foreach (var d in dom)
                        {
                            if (d.GetAttribute("innerHTML").Trim().Contains("Clear"))
                            {
                                is_selected = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    is_selected = true;
                }
            }
            catch (Exception) { }
            return is_selected;
        }
        public HtmlDocument HtmlDocumentFilter(HtmlDocument body)
        {
            HtmlDocument document = body;
            try
            {
                //-- Remove Grid:
                int remove_case_count = Convert.ToInt32(_configuration["Xpath_Grid:Remove_Count"]) < 1 ? 0 : Convert.ToInt32(_configuration["Xpath_Grid:Remove_Count"]);
                if (remove_case_count > 0)
                {
                    for (int i = 0; i < remove_case_count; i++)
                    {
                        if (_configuration["Xpath_Grid:Remove_Case_" + i] != null)
                        {
                            //Remove login div
                            HtmlNode footer_section = document.DocumentNode.SelectSingleNode(_configuration["Xpath_Grid:Remove_Case_" + i]);
                            if (footer_section != null)
                            {
                                footer_section.ParentNode.RemoveChild(footer_section);
                                Console.Write("Remove_Case_" + i + ". ");
                            }
                            else
                            {
                                //Console.Write("Remove_Xpath_" + i + " - Null - ");

                            }

                        }
                    }
                }

                //-- Select Grid:
                int case_count = Convert.ToInt32(_configuration["Xpath_Grid:Count"]) < 1 ? 0 : Convert.ToInt32(_configuration["Xpath_Grid:Count"]);
                if (case_count > 0)
                {
                    HtmlNodeCollection grid = null;
                    for (int i = 0; i < case_count; i++)
                    {
                        if (_configuration["Xpath_Grid:Case_" + i] != null)
                            grid = document.DocumentNode.SelectNodes(_configuration["Xpath_Grid:Case_" + i]);
                        if (grid != null && grid.Count > 0)
                        {
                            // Console.Write("Xpath_Grid Filter Case: " + i + " - ");
                            break;
                        }
                    }
                    if (grid != null && grid.Count > 0)
                    {
                        HtmlDocument doc = new HtmlDocument();
                        string base_html = @"<body></body>";
                        doc.LoadHtml(base_html);
                        HtmlNode grid_body = doc.DocumentNode.SelectSingleNode("//body");
                        foreach (var item in grid)
                        {
                            grid_body.AppendChild(item);
                        }
                        return doc;
                    }
                     else Console.Write("Cannot Filter by Xpath. Return <body> - ");

                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("App_Crawl_Mapping_Receiver_Service - ProductGridFilter: Error - " + ex.ToString());
                 Console.Write("Error on Filter. Return <body> - ");
            }
            return document;
        }
        private string GetURL_Case_Source(HtmlNode parent_node, List<string> list_xpath)
        {
            string url = null;
            try
            {
                foreach(var xpath in list_xpath)
                {
                    var url_node = parent_node.SelectNodes(xpath);
                    if (url_node != null && url_node.Count > 0)
                        foreach (var a in url_node)
                        {
                            url = a.Attributes["href"].Value;
                            if (url != null)
                            {
                                if (url.Contains("https://www.amazon.com"))
                                {

                                }
                                else
                                {
                                    url = "https://www.amazon.com" + url;
                                }
                                break;
                            }
                        }
                    if (url!=null&& url.StartsWith("https://www.amazon.com")) break;
                }
            }
            catch (Exception)
            {

            }
            return url;
        }
        private string GetURL_FromXpath(IWebElement root_element, List<string> xpath_list)
        {
            string url = null;
            try
            {
               if(xpath_list!=null && xpath_list.Count>0)
               {
                    foreach (var xpath in xpath_list)
                    {
                        var selected = root_element.FindElement(By.XPath(xpath));
                        if (selected != null)
                        {
                            url = selected.GetAttribute("href");
                        }
                        if (url != null && url.StartsWith("https://www.amazon.com"))
                        {
                            return url;
                        }
                    }
               }
            }
            catch (Exception)
            {
            }
            return url;

        }
        private bool IsElementExistByXpath(ChromeDriver driver, string xpath)
        {
            try
            {
                driver.FindElement(By.XPath(xpath));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        private bool IsElementExistByXpath(IWebElement root_element, string child_xpath)
        {
            try
            {
                root_element.FindElement(By.XPath(child_xpath));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }


    }
}
