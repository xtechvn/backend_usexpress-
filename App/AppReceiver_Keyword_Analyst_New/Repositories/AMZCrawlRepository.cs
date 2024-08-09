using AppReceiver_Keyword_Analyst_New.Interfaces;
using AppReceiver_Keyword_Analyst_New.Model;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace AppReceiver_Keyword_Analyst_New.Repositories
{
    public class AMZCrawlRepository : IAMZCrawlService
    {
        IConfiguration _configuration;
        public AMZCrawlRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<AMZSearchViewModel> CrawlSearchResult(ChromeDriver driver, SLQueueItem item)
        {
            AMZSearchViewModel result = null;
            if (item.keyword == null || item.label_Id < 0 || item.cache_name == null || item.cache_name == "" || item.keyword == "")
            {
                return null;
            }
            List<AmzSearchResult> list = new List<AmzSearchResult>();
            try
            {
                string base_url = _configuration["Base_Search_URL"];
                string real_keyword = item.keyword.Trim().ToLower().Replace("/[^a-zA-Z0-9 ]/g", "");
                // -- /s?k=raiden+shogun+keycaps&page=2&qid=1632542780&ref=sr_pg_2
                string qid = new Random().Next(10000, 99999).ToString() + "" + new Random().Next(10000, 99999).ToString();
                // driver.Navigate().GoToUrl(base_url.Replace("{keyword}", real_keyword).Replace("{page_index}", item.page_index.ToString()).Replace("{qid}", qid));
                driver.Navigate().GoToUrl(base_url.Replace("{keyword}", real_keyword).Replace("{qid}", qid));
                string page_source_html = driver.PageSource;
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                document = ProductGridFilter(document);
                int case_count = Convert.ToInt32(_configuration["Regex_Search_Result_Case_Count"]) < 0 ? 0 : Convert.ToInt32(_configuration["Regex_Search_Result_Case_Count"]);
                for (int grid_case = 0; grid_case < case_count; grid_case++)
                {
                    var grid_item_list = document.DocumentNode.SelectNodes(_configuration["Regex_Search_Case_" + grid_case + ":Grid_Item_Xpath"]);
                    if (grid_item_list != null && grid_item_list.Count > 0)
                    {
                        foreach (var grid_item in grid_item_list)
                        {
                            string product_code = null;
                            string url = GetURLFromGrid(grid_item, _configuration["Regex_Search_Case_" + grid_case + ":URL"], out product_code);
                            AmzSearchResult list_item = new AmzSearchResult();
                            if (url == null || product_code == null || product_code == "" || url == "")
                            {
                                continue;
                            }
                            else
                            {
                                list_item.image_url = GetImgLink(grid_item, _configuration["Regex_Search_Case_" + grid_case + ":Img_Link"], _configuration["Regex_Search_Case_" + grid_case + ":Img_extension"]);
                                list_item.product_name = GetProductName(grid_item, _configuration["Regex_Search_Case_" + grid_case + ":Product_Name"], _configuration["Regex_Search_Case_" + grid_case + ":ProductName_Span_Class"]);
                                list_item.reviews_count = GetReviewCount(grid_item, _configuration["Regex_Search_Case_" + grid_case + ":Review_Count_Case_1"], _configuration["Regex_Search_Case_" + grid_case + ":Review_Count_Case_2"]);
                                list_item.star = GetStarPoint(grid_item, _configuration["Regex_Search_Case_" + grid_case + ":Star_Point"], _configuration["Regex_Search_Case_" + grid_case + ":Star_Point_Text"]);
                                list_item.product_code = product_code;
                                list_item.price = GetPriceFromGrid(grid_item, _configuration["Regex_Search_Case_" + grid_case + ":Price"]);
                                if (list_item.image_url == null || list_item.image_url == "" || list_item.product_name == null || list_item.product_name == "" || list_item.product_code == null || list_item.product_code == "")
                                {
                                    continue;
                                }
                                else
                                {
                                    list_item.url = CommonHelper.genLinkDetailProduct("amazon", list_item.product_code, list_item.product_name); ; // url cua san pham tren trang usexpress.vn
                                    list_item.url_store = "https://www.amazon.com/dp/" + list_item.product_code;
                                    list.Add(list_item);
                                }
                            }

                        }
                    }
                    result = new AMZSearchViewModel();
                    result.data = list;
                    result.total_page = GetSearchTotalPage(document, _configuration["Regex_Search_Case_" + grid_case + ":Total_page"], _configuration["Regex_Search_Case_" + grid_case + ":Total_Page_withoutDisabled"]);
                    if (result.total_page > 0 && result.data != null && result.data.Count > 0) break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("AppReceiver_Keyword_Analyst_New - CrawlSearchResult: Error - " + ex.ToString());
                LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst_New - CrawlSearchResult: Error - " + ex.ToString());

            }
            return result;
        }

        private int GetSearchTotalPage(HtmlDocument document, string regex_normal, string regex_without_disabled)
        {
            int total_page = 0;
            try
            {
                MatchCollection matchList = Regex.Matches(document.DocumentNode.OuterHtml, regex_normal, RegexOptions.IgnoreCase);
                var list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                foreach (string totalpage in list)
                {
                    try
                    {
                        total_page = Convert.ToInt32(totalpage.Trim()) > 0 ? Convert.ToInt32(totalpage.Trim()) : 0;

                    }
                    catch (Exception)
                    {
                        total_page = 0;
                    }
                    if (total_page > 0) break;
                }
                if (total_page < 0)
                {
                    matchList = Regex.Matches(document.DocumentNode.OuterHtml, regex_without_disabled, RegexOptions.IgnoreCase);
                    list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                    for (int i = list.Count - 1; i == 0; i--)
                    {
                        try
                        {
                            total_page = Convert.ToInt32(list[i].Trim()) > 0 ? Convert.ToInt32(list[i].Trim()) : 0;
                        }
                        catch (Exception)
                        {
                            total_page = 0;
                        }
                        if (total_page > 0) break;
                    }
                }
            }
            catch (Exception)
            {

            }
            return total_page;
        }

        private int GetReviewCount(HtmlNode grid_item, string url_regex, string url_regex_case2)
        {
            int review_count = 0;
            try
            {
                MatchCollection matchList = Regex.Matches(grid_item.OuterHtml, url_regex, RegexOptions.IgnoreCase);
                var list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                foreach (string item in list)
                {
                    try
                    {
                        review_count = Convert.ToInt32(item.Replace(",", "").Replace("(", "").Replace(")", "").Trim());
                        if (review_count > 0) break;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
                if (review_count < 1)
                {
                    matchList = Regex.Matches(grid_item.OuterHtml, url_regex_case2, RegexOptions.IgnoreCase);
                    list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                    foreach (string item in list)
                    {
                        try
                        {
                            review_count = Convert.ToInt32(item.Replace(",", "").Replace("(", "").Replace(")", "").Trim());
                            if (review_count > 0) break;
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return review_count;
        }

        private double GetStarPoint(HtmlNode grid_item, string url_regex, string filter_text)
        {
            double star_point = 0;
            try
            {
                MatchCollection matchList = Regex.Matches(grid_item.OuterHtml, url_regex, RegexOptions.IgnoreCase);
                var list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                foreach (string item in list)
                {
                    if (item.Contains(filter_text))
                    {
                        star_point = Convert.ToDouble(item.Trim().Replace(filter_text, "").Replace("-", "."));
                        break;
                    }

                }
            }
            catch (Exception)
            {

            }
            return star_point;
        }

        private string GetProductName(HtmlNode grid_item, string url_regex, string filer_class_name)
        {
            string product_name = null;
            try
            {
                MatchCollection matchList = Regex.Matches(grid_item.OuterHtml, url_regex, RegexOptions.IgnoreCase);
                var list = matchList.Cast<Match>().Select(match => match.Groups[1].Value + "-/-" + match.Groups[2].Value).ToList();
                foreach (string item in list)
                {
                    var str = item.Split("-/-");
                    if (str != null && str.Count() == 2 && str[0].Contains(filer_class_name))
                    {
                        product_name = str[1];
                        break;
                    }

                }
            }
            catch (Exception)
            {

            }
            return product_name;
        }

        private string GetImgLink(HtmlNode grid_item, string url_regex, string img_extension_list)
        {
            string imglink = null;
            try
            {
                MatchCollection matchList = Regex.Matches(grid_item.OuterHtml, url_regex, RegexOptions.IgnoreCase);
                var list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                foreach (string imglink_from_grid in list)
                {
                    foreach (var ext in img_extension_list.Split(","))
                    {
                        if (imglink_from_grid.Contains(ext))
                        {
                            imglink = imglink_from_grid;
                            break;
                        }
                    }
                    if (imglink != null) break;

                }
            }
            catch (Exception)
            {

            }
            return imglink;
        }

        private string GetURLFromGrid(HtmlNode grid_item, string url_regex, out string product_code)
        {
            string url = null;
            product_code = null;
            try
            {
                MatchCollection matchList = Regex.Matches(grid_item.OuterHtml, url_regex, RegexOptions.IgnoreCase);
                var list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                foreach (string url_from_grid in list)
                {
                    string check_url;
                    if (url_from_grid.Contains("http")) check_url = url_from_grid;
                    else check_url = "https://www.amazon.com" + url_from_grid;
                    //var rs = CommonHelper.CheckAsinByLink(check_url, out product_code);
                    product_code = GetASINFromURL(check_url);
                    if (product_code != null && product_code != "")
                    {
                        url = check_url;
                        break;
                    }
                }
            }
            catch (Exception)
            {

            }
            return url;
        }

        private HtmlDocument ProductGridFilter(HtmlDocument body)
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
                            footer_section.ParentNode.RemoveChild(footer_section);
                            Console.Write("Remove_Xpath_" + i + " - ");
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
                            Console.Write("Xpath_Grid Filter Case: " + i + " - ");
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
                //LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst_New - ProductGridFilter: Error - " + ex.ToString());
                Console.Write("Error on Filter. Return <body> - ");
            }
            return document;
        }
        private double GetPriceFromGrid(HtmlNode grid_item, string url_regex)
        {
            double price = 0;
            try
            {

                MatchCollection matchList = Regex.Matches(grid_item.OuterHtml, url_regex, RegexOptions.IgnoreCase);
                var list = matchList.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
                foreach (string pricestring in list)
                {
                    string price_correct = pricestring.Trim().Replace(",", "").Replace("$", "");
                    try
                    {
                        price = Convert.ToDouble(price_correct);

                    }
                    catch (Exception ex)
                    {
                        price = 0;
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                price = 0;
            }
            return price;
        }
        private string GetASINFromURL(string url)
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
    }
}
