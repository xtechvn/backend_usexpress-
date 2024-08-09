
using CsQuery;
using CsQuery.Utility;
using Entities.ViewModels;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Utilities;
using Utilities.Contants;
using Utilities.UtilitiesNumb;

namespace Crawler.ScraperLib.Amazon
{
    public static class ParserAmz
    {
        /// <summary>
        /// GHép các thành phần lấy ra chi tiết sp
        /// </summary>
        /// <returns></returns>
        public static ProductViewModel RegexElementPage(string page_source_html, string asin, string url_page, double rate)
        {
            try
            {
                //var sw = new Stopwatch();
                //sw.Start();

                var amz_detail = new ProductViewModel();
                bool is_range_price = false;

                //Kiểm tra link chi tiết sản phẩm có đang show RANGE PRICE không
                amz_detail.price = ParserAmz.getPrice(page_source_html, out is_range_price);

                // Thực hiện Regex trang

                amz_detail.product_map_id = -1;
                amz_detail.product_code = asin;
                amz_detail.create_date = DateTime.Now;
                amz_detail.label_id = (int)LabelType.amazon;
                amz_detail.update_last = DateTime.Now;

                //Product name
                amz_detail.product_name = ParserAmz.GetProductName(page_source_html).Replace("\"", "").Replace("'", "");
                amz_detail.product_name = CommonHelper.RemoveSpecialCharacters(amz_detail.product_name);
                // Price
                // Kiểm tra có range ko
                // Nếu có lấy giá cao nhất
                // Nếu không lấy ở Offerlist                                
                amz_detail.discount = 0;
                amz_detail.amount = amz_detail.price - (amz_detail.price * amz_detail.discount) / 100;
                amz_detail.shiping_fee = ParserAmz.getShippingFee(page_source_html);

                amz_detail.rate = rate;

                #region Chia đổi tiến trình này
                //if (amz_detail.price <= 0)
                //{
                //    var seller_list = getPriceBySellers(browers, page_source_html);
                //    // Price
                //    if (seller_list != null)
                //    {
                //        // lay ra seller tot nhat
                //        var filter_seller_best_choice = ParserAmz.filterSellerChoice(seller_list);

                //        amz_detail.price = filter_seller_best_choice == null ? 0 : filter_seller_best_choice.price;                        
                //        amz_detail.shiping_fee = filter_seller_best_choice == null ? 0 : filter_seller_best_choice.shiping_fee;
                //        amz_detail.seller_id = filter_seller_best_choice == null ? string.Empty : filter_seller_best_choice.seller_id;
                //        amz_detail.seller_name = filter_seller_best_choice == null ? string.Empty : filter_seller_best_choice.seller_name;
                //    }
                //}
                //else
                //{
                //    // shipping fee
                //    amz_detail.shiping_fee = ParserAmz.getShippingFee(page_source_html);
                //}
                #endregion

                // Image list
                amz_detail.image_size_product = ParserAmz.getListSizeImage(page_source_html);

                //image thumb
                amz_detail.image_thumb = amz_detail.image_size_product.Count() > 0 ? amz_detail.image_size_product[0].Larger : string.Empty;

                //star                
                amz_detail.star = ParserAmz.GetRating(page_source_html);

                // review count
                amz_detail.reviews_table = ParserAmz.getReviewCound(page_source_html);

                //Variation
                // amz_detail.list_variations = ParserAmz.getVariation(page_source_html);
                // Tên biến thể
                // amz_detail.variation_name = ParserAmz.getVariationName(page_source_html);
                // Các biến thể của sản phẩm
                // amz_detail.color_images = ParserAmz.getJsonImgVariation(asin, page_source_html, amz_detail.list_variations, amz_detail.variation_name);

                // Detect in stock
                amz_detail.in_stock = ParserAmz.getInStock(page_source_html);

                // check has Image variation
                amz_detail.dimensions_display_image = ParserAmz.getDimensionsDisplaySubType(page_source_html);

                // Weight
                bool is_crawl_weight;
                amz_detail.item_weight = ParserAmz.getItemWeight(page_source_html, asin, out is_crawl_weight);

                // param này cho biết sp này có cân nặng hay ko
                amz_detail.is_crawl_weight = is_crawl_weight;
                amz_detail.regex_step = is_crawl_weight ? 2 : 1; //Co can nang pass qua PART 2                

                // Tính giá về tay
                //                amz_detail.amount_vnd = amz_detail.list_product_fee != null ? amz_detail.list_product_fee.amount_vnd : 0;

                amz_detail.label_name = LabelNameType.amazon;
                amz_detail.link_product = CommonHelper.genLinkDetailProduct(LabelType.amazon.ToString(), asin, amz_detail.product_name);
                amz_detail.keywork_search = url_page;
                amz_detail.seller_id = getSellerIdSelected(page_source_html, amz_detail.product_code);
                amz_detail.seller_name = CommonHelper.RemoveSpecialCharacters(getSellerNameSelected(page_source_html).Replace("\"", "").Replace("'", "").Replace("Author", "").Replace("amp", "and"));
                amz_detail.is_has_seller = checkHasSellerList(page_source_html);// page_source_html.IndexOf("olp-new") >= 0 ? true : false;

                // Lấy ảnh thông tin sản phẩm (From manufacter):                
                amz_detail.image_product = ParserAmz.GetFromManufacterImage(page_source_html);
                // Lấy số lượng đánh giá sản phẩm :                
                amz_detail.reviews_count = ParserAmz.GetReviewCount(page_source_html);
                //Lấy giá giảm:
                amz_detail.product_save_price = ParserAmz.GetDiscount(page_source_html, asin);
                if (amz_detail.product_save_price > 0)
                {
                    //  amz_detail.discount = Math.Round((amz_detail.product_save_price / (amz_detail.product_save_price + amz_detail.price)) * 100, 0);
                    amz_detail.discount = (int)(amz_detail.product_save_price / (amz_detail.product_save_price + amz_detail.price) * 100);

                }
                else
                {
                    amz_detail.discount = 0;
                }
                //Thông tin sản phẩm HTML:
                amz_detail.product_infomation_HTML = ParserAmz.RenderProductHTML(amz_detail.image_product, amz_detail.product_code, amz_detail.product_name);
                //Sản phẩm có thuộc Amazon Choice's hay không:
                amz_detail.is_amazon_choice = ParserAmz.IsAmzChoices(page_source_html);
                // List sản phẩm thường mua cùng:
               // amz_detail.product_freq_buy_with = ParserAmz.GetProductFrequentlyBoughtTogether(page_source_html);
                // List sản phẩm liên quan:
                amz_detail.product_related = ParserAmz.GetProductRelated(page_source_html, asin);

                // Thông số sản phẩm:
                var stw = new Stopwatch();
                stw.Restart();
                amz_detail.product_specification = ParserAmz.GetProductSpecification(page_source_html, asin);
                stw.Stop();
                //  Console.WriteLine("product_specification Excute Time: " + stw.ElapsedMilliseconds + " ms");
                // Thông tin về sản phẩm:
                amz_detail.product_infomation = ParserAmz.GetProductInfomation(page_source_html, asin);
                // có tồn tại trang chi tiết = amz_detail.product_name == string.Empty; // có tồn tại trang chi tiết
                amz_detail.rating = ParserAmz.GetProductRatings(page_source_html).ToString();
                //Thời gian sale:
                // amz_detail.sale_exprire_time = GetSaleExprireTimes(page_source_html, asin);
                amz_detail.page_not_found = amz_detail.product_name == string.Empty;
                //amz_detail.product_videos = GetProductVideos(page_source_html, asin);
                // Thể tích:
                amz_detail.package_volume_weight = GetPackageVolume(amz_detail.product_specification);
                // Cân nặng được chọn:
                amz_detail.selected_weight = GetSelectedWeight(amz_detail.item_weight, amz_detail.package_volume_weight, amz_detail.is_crawl_weight);
                amz_detail.item_weight = amz_detail.selected_weight;
                if (amz_detail.page_not_found)
                {
                    //LogHelper.InsertLogTelegram("Khong doc duoc DOM cho san pham asin =" + asin);
                }

                //sw.Stop();
                //LogHelper.InsertLogTelegram("RegexElementPage: Total Elment Regex = " + (sw.ElapsedMilliseconds / 1000).ToString());

                return amz_detail;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Parser AMZ --RegexElementPage: error = " + ex.ToString());
                return null;
            }
        }


        public static bool HaveCapCha(string amazon_html_string)
        {
            try
            {
                if (amazon_html_string == null) return true;
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(amazon_html_string);
                HtmlNode capcha = document.GetElementbyId("captchacharacters");
                return capcha != null;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("HaveCapCha: error = " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Returns product ASIN - unique product identifier, used in URLs
        /// </summary>
        /// <param name="itemHtml"></param>
        /// <returns>String ASIN value</returns>
        public static string GetProductAsin(string itemHtml)
        {
            try
            {
                string productAsinPattern = @"(?<=data-asin="").*?(?="" )";
                return CommonHelper.GetSingleRegExMatch(itemHtml, productAsinPattern);
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }
        public static string getItemWeight(string amazon_html_string, string asin, out bool is_crawl_weight)
        {
            is_crawl_weight = false;
            try
            {
                var pattern_weight = new List<string> { "detailBullets_feature_div", "productDetails_techSpec_section_2", "productDetails_techSpec_section_1", "prodDetails", "dpx-aplus-product-description_feature_div", "productDetails_detailBullets_sections1", "content-grid-widget-v1.0" };
                for (int i = 0; i <= pattern_weight.Count() - 1; i++)
                {
                    string element_id_price = pattern_weight[i];
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(amazon_html_string);
                    HtmlNode node_price_Html = document.GetElementbyId(element_id_price);

                    if (node_price_Html == null) continue;
                    string dom_has_weight = node_price_Html.InnerHtml;

                    //string pattern = @"(.*?) (pounds|ounces|oz|grams|kilograms|tonne|kiloton|inches)";
                    string pattern = @"[0-9]+[0-9\s]*(\.(\s*[0-9]){0,9})? (Pounds|pounds|ounces|Ounces|Grams|grams|kilograms|kg|tonne|kiloton|oz|lb|Lb|lbs|Lbs)";
                    Match weight_data = Regex.Match(dom_has_weight, pattern);

                    if (!string.IsNullOrEmpty(weight_data.ToString()))
                    {
                        is_crawl_weight = true; // Quét mặt trang thấy có cân nặng
                        return weight_data.ToString().ToLower();
                    }
                    else
                    {
                        //LogHelper.InsertLogTelegram("Crawl weight not found for asin:" + asin + " id dom using:" + JsonConvert.SerializeObject(pattern_weight));
                    }
                }
                return "1 pounds";
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getItemWeight asin = " + asin + ": Weight not found" + ex.ToString());
                return "1 pounds";
            }
        }
        public static string GetProductName(string itemHtml)
        {
            try
            {
                //string productNamePattern = @"(?<= title="").*?(?="" )";
                //string match = CommonHelper.GetSingleRegExMatch(itemHtml, productNamePattern);

                //if (match.Length == 0)
                //{ return null; }

                //string productName = CommonHelper.DecodeHTML(match);

                //return productName;
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(itemHtml);
                HtmlNode node = document.GetElementbyId("productTitle");
                if (node != null)
                {
                    string rs = CommonHelper.ConvertAllNonCharacterToSpace(node.InnerText).Trim();
                    return CommonHelper.DecodeHTML(rs).Trim();
                }
                else
                {
                    node = document.GetElementbyId("ebooksProductTitle");
                    string rs = node == null ? string.Empty : CommonHelper.ConvertAllNonCharacterToSpace(node.InnerText).Trim();
                    return rs;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("GetProductName error" + ex.ToString());
                return null;
            }
        }
        // còn hàng hay ko
        public static string getInStock(string amazon_html_string)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(amazon_html_string);
                HtmlNode node = document.GetElementbyId("availability");
                if (node != null)
                {
                    string rs = CommonHelper.ConvertAllNonCharacterToSpace(node.InnerText).Trim();
                    return CommonHelper.DecodeHTML(rs).Trim();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getInStock error" + ex.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// Parses a DoubleRange object representing the "high" and "low"
        /// prices from the item's html.
        /// </summary>
        /// If there is only one price supplied, set the range to that single value.
        /// <param name="html">Single product result html</param>
        /// swatchSelect: la gia duoc chon khi co variation 
        /// <returns>DoubleRange representing the product's pricerange</returns>
        /// Thời gian đo: 05-12-2021 : 2202 ms
        /// 
        public static double getPrice(string amazon_html_string, out bool is_range_price)
        {

            is_range_price = false;
            double price = 0;
            try
            {
                //var sw = new Stopwatch();
                //sw.Start();

                string swatchSelect = getSelectedVariationValues(amazon_html_string);

                var pattern_id_price = new List<string> { "price", "corePrice_desktop", "price_inside_buybox", "newBuyBoxPrice", "priceblock_ourprice", "usedBuySection", "buyNew_noncbb", "mediaNoAccordion", "unqualified-buybox-olp", "buybox", "MediaMatrix", "olp-upd-new", "olp-upd-new-freeshipping-threshold", swatchSelect, "almDetailPagePrice_buying_price", "olp-upd-new-used", "olp_feature_div" }; //nhung vi tri co gia tren mat trang
                for (int i = 0; i <= pattern_id_price.Count() - 1; i++)
                {
                    string element_id_price = pattern_id_price[i];
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(amazon_html_string);
                    HtmlNode node_price_Html = document.GetElementbyId(element_id_price);


                    if (node_price_Html == null) continue;
                    string dom_has_price = node_price_Html.InnerHtml;

                    // Kiểm tra xem có giá Giảm không
                    if (dom_has_price.IndexOf("priceblock_dealprice") >= 0)
                    {
                        document.LoadHtml(amazon_html_string);
                        HtmlNode node_price_sale_Html = document.GetElementbyId("priceblock_dealprice");
                        if (node_price_sale_Html == null) continue;
                        string dom_price_sale_Html = node_price_sale_Html.OuterHtml;
                        price = CommonHelper.RegexPriceInHtmlPage(dom_price_sale_Html);
                    }
                    else
                    {
                        price = CommonHelper.RegexPriceInHtmlPage(dom_has_price);
                    }

                    if (element_id_price == "mediaNoAccordion" && price <= 0)
                    {
                        price = CommonHelper.RegexPriceInHtmlPage(dom_has_price);
                    }

                    if (price > 0)
                    {
                        //LogHelper.InsertLogTelegram("getPrice time" + sw.ElapsedMilliseconds + " ms");
                        return price;
                    }
                    else
                    {
                        string pattern = CommonHelper.dollarCurrencyFormat;
                        var result = CommonHelper.GetSingleRegExMatch(dom_has_price, pattern);

                        price = result == string.Empty ? 0 : Convert.ToDouble(result.Replace("$", ""));
                        if (price == 0) continue;
                    }
                }
                //sw.Stop();
                //LogHelper.InsertLogTelegram("getPrice time" + sw.ElapsedMilliseconds + " ms");

                return price;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getPrice error" + ex.ToString());
                return 0;
            }
        }
        public static DoubleRange GetPriceRange(string dom_has_price)
        {
            try
            {
                // Dollarsign and Digits grouped by commas plus decimal
                // and change (change is required)
                string dollarCurrencyFormat = @"\$(\d{1,3}(,\d{3})*).(\d{2})";

                // Optional spaces and hyphen
                string spacesAndHyphen = @"\s+-\s+";

                // Grab the end of the preceeding tag, the dollar amount, and
                // optionally a hyphen and a high range amount before the
                // beginning bracket of the next tag
                string pricePattern = ">" + dollarCurrencyFormat + "(" + spacesAndHyphen + dollarCurrencyFormat + ")?" + "<";

                string match = CommonHelper.GetSingleRegExMatch(dom_has_price, pricePattern);

                // Need to remove the tag beginning and end:
                match = match.Trim(new char[] { '<', '>' });

                if (match.Length == 0)
                {
                    return new DoubleRange();
                }

                List<Double> prices = CommonHelper.ParseDoubleValues(match, 2);
                DoubleRange priceRange = new DoubleRange();
                if (prices.Count > 0)
                {
                    priceRange.Low = prices[0];
                }

                if (prices.Count > 1)
                {
                    priceRange.High = prices[1];
                }

                if (!priceRange.HasHigh)
                {
                    priceRange.High = priceRange.Low;
                }

                return priceRange;
                // }
                // return new DoubleRange();
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("GetPriceRange error" + ex.ToString());
                return null;
            }
        }

        public static double getShippingFee(string amazon_html_string)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                var pattern_price_shipping = new List<string> { "price-shipping-message", "ourprice_shippingmessage", "priceblock_ourprice_ifdmsg", "olp_feature_div", "a-size-base" };
                string text1 = Regex.Replace(amazon_html_string, "<style.*?</style>", "", RegexOptions.Singleline);
                amazon_html_string = text1;
                string text2 = Regex.Replace(amazon_html_string, "<script.*?</script>", "", RegexOptions.Singleline);
                amazon_html_string = text2;
                document.LoadHtml(amazon_html_string);

                for (int i = 0; i <= pattern_price_shipping.Count() - 1; i++)
                {
                    HtmlNode node01 = document.GetElementbyId(pattern_price_shipping[i]);
                    if (node01 != null)
                    {
                        if (node01.InnerText.ToLower().Contains("free shipping")) return 0;
                        var options = RegexOptions.Multiline;
                        var gr = Regex.Matches(node01.InnerText.ToString(), @"\$[\d,.]+", options);
                        return gr.Count > 0 ? Convert.ToDouble(gr.Last().Value.Trim().Replace("$", "")) : 0;
                    }
                    else
                    {
                        continue;
                    }
                }
                HtmlNode element = document.DocumentNode.SelectSingleNode("//span[@data-csa-c-content-id=\"DEXUnifiedCXPDM\"]");
                if (element != null && element.Attributes["data-csa-c-delivery-price"] != null)
                {
                    if (!element.Attributes["data-csa-c-delivery-price"].Value.ToUpper().Contains("FREE"))
                    {
                        try
                        {
                            return Convert.ToDouble(element.Attributes["data-csa-c-delivery-price"].Value.Replace("$", "").Replace(",", ""));
                        }
                        catch { }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getShippingFee error" + ex.ToString());
                return 0;
            }
        }
        public static double getShippingFeeBySeller(string item_html)
        {
            try
            {
                string pattern = @"\$(\d{1,3}(,\d{3})*).(\d{2})";
                var shipping_fee = CommonHelper.GetSingleRegExMatch(item_html, pattern);
                return shipping_fee == string.Empty ? 0 : Convert.ToDouble(shipping_fee.Replace("$", ""));
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getShippingFeeBySeller error" + ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Lấy ra id Seller selected
        /// </summary>
        /// <param name="item_html">ftSelectMerchant</param>
        /// <returns></returns>
        public static string getSellerIdSelected(string item_html, string product_code)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(item_html);
                HtmlNode node = document.GetElementbyId("ftSelectMerchant");
                if (node != null)
                {
                    return node.GetAttributes("value") == null ? "" : node.GetAttributes("value").ToList()[0].Value;
                }
                else
                {
                    ////LogHelper.InsertLogTelegram("getSellerIdSelected error: Không lấy được seller id cho sản phẩm product_code =" + product_code);
                }
                return "N/A";
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getSellerIdSelected error" + ex.ToString());
                return "";
            }
        }
        /// <summary>
        /// Lấy ra id Seller selected
        /// </summary>
        /// <param name="item_html">ftSelectMerchant</param>
        /// <returns></returns>
        public static string getSellerNameSelected(string item_html)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(item_html);
                HtmlNode node = document.GetElementbyId("bylineInfo_feature_div");
                string rs = node == null ? string.Empty : CommonHelper.ConvertAllNonCharacterToSpace(node.InnerText).Replace("Visit the", "").Trim();
                return rs == string.Empty ? "Amazon" : CommonHelper.CheckMaxLength(rs, 80);
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getSellerNameSelected error" + ex.ToString());
                return "";
            }
        }

        public static List<string> getListImage(string amazon_html_string)
        {
            var lstr2 = new List<string>();
            try
            {
                string outerHtml = Regex.Match(amazon_html_string, "'colorImages':.*?}]").Value;
                if (!string.IsNullOrEmpty(outerHtml))
                {
                    lstr2 = Regex.Matches(outerHtml, "https://\\S{5,30}amazon.com/images/(I|(G/01/x-site/icons))/[^/\"]*?(\\.jpg\"|\\.png\"|\\.gif\")").Cast<Match>().Select(q => q.Value.Replace("\"", "")).ToList();
                    //str2 = Globals.ConvertTojsonString((from q in ).ToList<string>());
                }
                else
                {
                    int num1;
                    outerHtml = Regex.Match(amazon_html_string, "'imageGalleryData' :.*?}]", RegexOptions.Singleline).Value;
                    if (string.IsNullOrEmpty(outerHtml) || !outerHtml.Contains("http"))
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = Convert.ToInt32(!outerHtml.Contains("'imageGalleryData' : []"));
                    }
                    if (num1 != 0)
                    {
                        lstr2 = Regex.Matches(outerHtml, "https://\\S{5,30}amazon.com/images/(I|(G/01/x-site/icons))/[^/\"]*?(\\.jpg\"|\\.png\"|\\.gif\")").Cast<Match>().Select(q => q.Value.Replace("\"", "")).ToList();
                        //lstr2 = Globals.ConvertTojsonString((from q in ).ToList<string>());
                    }
                    else
                    {
                        HtmlDocument document = new HtmlDocument();
                        document.LoadHtml(amazon_html_string);
                        HtmlNode node = document.GetElementbyId("imgBlkFront");
                        if (node == null) node = document.GetElementbyId("ebooksImgBlkFront");
                        if (node == null)
                        {
                            lstr2 = null;
                        }
                        else
                        {
                            outerHtml = node.OuterHtml;
                            outerHtml = CommonHelper.DecodeHTML(Regex.Match(outerHtml, "data-a-dynamic-image=\".*?\">", RegexOptions.Singleline).Value);
                            if (!string.IsNullOrEmpty(outerHtml))
                            {
                                lstr2 = Regex.Matches(outerHtml, "https://\\S{5,30}amazon.com/images/(I|(G/01/x-site/icons))/[^/\"]*?(\\.jpg\"|\\.png\"|\\.gif\")").Cast<Match>().Select(q => q.Value.Replace("\"", "")).ToList();
                                //lstr2 = Globals.ConvertTojsonString((from q in ).ToList<string>());
                            }
                            else
                            {
                                lstr2 = null;
                            }
                        }
                    }
                }

                if (lstr2 != null && lstr2.Count > 0)
                {
                    for (int i = 0; i < lstr2.Count; i++)
                    {
                        string rs = Regex.Match(lstr2[i], @"https://\S+?amazon.com/images/(I|G)/(\S{11,17}|01/x-site/icons/no-img-sm)\.", RegexOptions.Singleline).Value;
                        if (!string.IsNullOrEmpty(rs))
                        {
                            lstr2[i] = rs + "_SL1500_.jpg";
                        }
                        else
                        {
                            //LogHelper.InsertLogTelegram("getListImage error: " + lstr2[i]);
                        }
                    }
                    return lstr2.Distinct().ToList();
                }
                else
                {
                    return lstr2;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getListImage error" + ex.ToString());
                return lstr2;
            }
        }
        /// <summary>
        /// regex các cỡ ảnh sản phẩm
        /// </summary>
        /// <param name="amazon_html_string"></param>
        /// <returns></returns>
        public static List<ImageSizeViewModel> getListSizeImage(string amazon_html_string)
        {
            var lst_img_size = new List<ImageSizeViewModel>();
            try
            {
                string regex_start = "'initial':";
                string outerHtml = Regex.Match(amazon_html_string, "" + regex_start + ".*?}]").Value.Replace("" + regex_start + "", ""); // regex lấy băt đầu từ ký tự
                //(?<='colorImages': { 'initial': ).*(?=]},)
                //string outerHtml = CommonHelper.GetSingleRegExMatch(amazon_html_string, "(?<='colorImages': { 'initial': ).*(?=},)");
                if (!string.IsNullOrEmpty(outerHtml))
                {
                    var json_img_data = JArray.Parse("[" + outerHtml + "]");
                    var lst_img = json_img_data[0].Children().ToList();
                    foreach (JToken item in lst_img)
                    {
                        var img = new ImageSizeViewModel
                        {
                            Larger = (item["hiRes"].ToString() == string.Empty ? item["large"] : item["hiRes"]).ToString().Replace("\"", ""),
                            Thumb = item["thumb"].ToString().Replace("\"", "")
                        };
                        lst_img_size.Add(img);
                    }
                }
                else
                {
                    var list_img = getListImage(amazon_html_string);
                    if (list_img == null) return lst_img_size;
                    foreach (var item in list_img)
                    {
                        var img = new ImageSizeViewModel
                        {
                            Larger = item,
                            Thumb = item
                        };
                        lst_img_size.Add(img);
                    }
                }
                return lst_img_size;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getListSizeImage error" + ex.ToString());
                return lst_img_size;
            }
        }


        /// <summary>
        /// Returns a product's average review rating (double)
        /// </summary>
        /// <param name="reviewHistogramHtml">html of the review histogram</param>
        /// <returns>-1 if no rating, otherwise double rating value</returns>
        public static double GetRating(string reviewHistogramHtml)
        {
            string ratingPattern = @"[0-5].?[0-9]? out of 5 stars";

            string rating = CommonHelper.GetSingleRegExMatch(reviewHistogramHtml, ratingPattern);

            double result = -1;

            if (rating.Length == 0) return result;

            try
            {
                // Two possible formats:
                // 1) Decimal value included, e.g. 4.5
                if (rating.Contains("."))
                {

                    result = Convert.ToDouble(rating.Substring(0, 3));
                }
                else // 2) No decimal, e.g. 4
                {
                    result = Convert.ToDouble(rating.Substring(0, 1));
                }

            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("GetRating error" + ex.ToString());
            }

            return result;
        }

        public static List<string> getDimensionsDisplaySubType(string amazon_html_string)
        {
            var variation_list_name = new List<string>();
            try
            {
                string pattern = @"""dimensionsDisplaySubType""  : (.*),";


                Match variation_data = Regex.Match(amazon_html_string, pattern);

                if (!string.IsNullOrEmpty(variation_data.ToString()))
                {
                    var json_img_data = JArray.Parse(variation_data.Groups[1].Value);
                    var lst_img = json_img_data.Children().ToList();
                    foreach (JToken item in lst_img)
                    {
                        string data_type = CommonHelper.RemoveAllNonCharacter(item.ToString());
                        if (data_type != string.Empty)
                        {
                            variation_list_name.Add(data_type);
                        }
                    }
                }
                return variation_list_name;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getDimensionsDisplaySubType error" + ex.ToString());
                return variation_list_name;
            }
        }

        /// <summary>
        /// Thông tin chi tiết các biến thể
        /// </summary>
        /// <param name="amazon_html_string"></param>
        /// <returns></returns>
        /// 
        public static List<VariationViewModel> getVariation(string amazon_html_string)
        {
            try
            {
                string data = Regex.Match(amazon_html_string, "\"dimensionValuesDisplayData\" : \\{.*?\\]\\},", RegexOptions.Singleline).Value;
                if (!string.IsNullOrEmpty(data))
                {
                    var list_variation = new List<VariationViewModel>();
                    string optionstring = Regex.Match(amazon_html_string, "\"dimensionsDisplay\" : \\[.*?\\],", RegexOptions.Singleline).Value;
                    optionstring = Regex.Replace(optionstring, "^\"dimensionsDisplay\" : ", "");
                    optionstring = Regex.Replace(optionstring, ",$", "", RegexOptions.Singleline);

                    string currentassin = Regex.Match(amazon_html_string, "\"currentAsin\" : \".*?\",", RegexOptions.Singleline).Value;
                    currentassin = Regex.Replace(currentassin, "^\"currentAsin\" : \"", "");
                    currentassin = Regex.Replace(currentassin, "\",$", "", RegexOptions.Singleline);

                    string[] options = CommonHelper.ConvertFromJsonString<string[]>(optionstring);
                    for (int i = 0; i < options.Length; i++)
                    {
                        options[i] = CommonHelper.RemoveAllNonCharacter(options[i]);
                    }

                    data = Regex.Replace(data, "^\"dimensionValuesDisplayData\" : ", "", RegexOptions.Singleline);
                    data = Regex.Replace(data, ",$", "", RegexOptions.Singleline);
                    var obj = CommonHelper.ConvertFromJsonString<DataObjectViewModel>(data);
                    foreach (var item in obj.Keys)
                    {
                        var variation = new VariationViewModel();
                        variation.asin = item;
                        if (item == currentassin)
                        {
                            variation.selected = true;
                        }
                        else
                        {
                            variation.selected = false;
                        }
                        string[] child = CommonHelper.ConvertFromJsonString<string[]>(obj[item].ToString());
                        DataObjectViewModel dim = new DataObjectViewModel();
                        for (int i = 0; i < options.Length; i++)
                        {
                            dim[options[i]] = child[i].Replace("'", "");
                        }
                        variation.dimensions = dim;
                        list_variation.Add(variation);
                    }
                    return list_variation;
                }
                else
                {
                    return new List<VariationViewModel>();
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getVariation error" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// lấy ra các ảnh của màu
        /// </summary>
        /// <param name="amazon_html_string"></param>
        /// <returns></returns>
        public static List<ColorImagesViewModel> getJsonImgVariation(string asin, string amazon_html_string, List<VariationViewModel> obj_variation, List<string> variation_name)
        {
            var color_img = new List<ColorImagesViewModel>();
            try
            {
                // Lấy ra chuỗi Json
                //string variation_data = CommonHelper.GetSingleRegExMatch(amazon_html_string, @"(?<=jQuery.parseJSON\(\').*(?=')");
                Match variation_data = Regex.Match(amazon_html_string, @"(?<=jQuery.parseJSON\(\').*(?=')");

                if (!string.IsNullOrEmpty(variation_data.ToString()))
                {

                    var json_img_data = JArray.Parse("[" + variation_data + "]");

                    // COLOR_image
                    string colorImages = json_img_data[0]["colorImages"].ToString();
                    string colorToAsin = json_img_data[0]["colorToAsin"].ToString();

                    var json_lst_image_asin = JArray.Parse("[" + colorToAsin + "]");
                    var json_lst_image = JArray.Parse("[" + colorImages + "]");

                    var lst_img = json_lst_image[0].Children().ToList();
                    var lst_img_asin = json_lst_image_asin[0].Children().ToList();

                    if (lst_img_asin.Count == 0 && lst_img_asin.Count == 0) return null;

                    int d = 0;
                    foreach (JToken item in lst_img)
                    {
                        var color_detail = new ColorImagesViewModel();

                        var j_size_list_data = JArray.Parse(item.Root.ToString());

                        color_detail.product_code = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JProperty)lst_img_asin[d]).Value).First).Value.ToString();
                        color_detail.color_name = (((Newtonsoft.Json.Linq.JProperty)item).Name).Replace("'", " ").Replace("\\", "");

                        string data_size_list = ((Newtonsoft.Json.Linq.JProperty)item).Value.ToString();
                        var j_size_list = JArray.Parse(data_size_list);
                        var size_list = j_size_list[0].Children().ToList();

                        var size_lst = new List<ImageSizeViewModel>();


                        var size = new ImageSizeViewModel
                        {
                            Larger = j_size_list[0]["hiRes"] == null ? j_size_list[0]["large"].ToString() : j_size_list[0]["hiRes"].ToString(),
                            Thumb = j_size_list[0]["thumb"].ToString()
                        };
                        size_lst.Add(size);
                        color_detail.obj_list_img_size = size_lst;
                        color_img.Add(color_detail);
                        d += 1;
                    }

                    // Group by lấy ra màu của sản phẩm đang được focus
                    if (obj_variation.Count > 0 && variation_name.Count > 0 && color_img.Count > 0)
                    {
                        // Lấy ra Index variation được chọn
                        string item_current_selected = string.Empty;
                        var list_current_dimension = getIndexVariationSelected(amazon_html_string, ""); //example: 0,1,2,1
                        int k = 0;
                        foreach (var item in variation_name)
                        {
                            // Xác định biến thể Color để lấy ra các sản phẩm cùng loại
                            var display_sub_type = getDimensionsDisplaySubType(amazon_html_string);
                            if (display_sub_type.Count() == 0) break;
                            if (display_sub_type.Count > k)
                            {
                                bool dimensions_display_image = display_sub_type[k].IndexOf("IMAGE") >= 0;
                                if (dimensions_display_image)
                                {
                                    // Thực hiện lấy ra nhưng asin cùng nhóm với biến thể này
                                    string index_dimentsion = list_current_dimension[k];

                                    for (int j = 0; j <= list_current_dimension.Count - 1; j++)
                                    {
                                        if (item_current_selected != string.Empty) item_current_selected += "_";
                                        if (k == j)
                                        {
                                            // build Regex 
                                            item_current_selected += "[0-9]"; //Nhóm những variation biến thiên trong khoảng từ 0-9
                                        }
                                        else
                                        {
                                            // Giữ nguyên các biến thể focus của sản phẩm
                                            item_current_selected += list_current_dimension[j];
                                        }
                                    }
                                }
                                k += 1;
                            }
                        }

                        //string pattern_dimension = item_current_selected; // @"0_0_[0-9]_0";
                        var group_dimension = getGroupDimensionAsin(amazon_html_string, item_current_selected);
                        if (group_dimension != null)
                        {
                            color_img = color_img.Where(x => group_dimension.Contains(x.product_code)).ToList();
                        }
                        else
                        {
                            //LogHelper.InsertLogTelegram("getJsonImgVariation error" + "Loi trong qua trinh Regex getGroupDimensionAsin");
                        }
                    }


                }
                return color_img;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("[" + asin + "]getJsonImgVariation error: " + ex.ToString());
                return null;
            }
        }

        private static List<string> getIndexVariationSelected(string amazon_html_string, string asin)
        {
            try
            {
                var rs_variation_data = new List<string>();
                string pattern = @"\""currentDimensionCombinationId\"" : (.*),";


                Match variation_data = Regex.Match(amazon_html_string, pattern);

                if (!string.IsNullOrEmpty(variation_data.ToString()))
                {
                    string value_regex = (variation_data.Groups[1].ToString().Replace("\"", ""));

                    rs_variation_data = value_regex.Trim().Split('_').ToList();
                    return rs_variation_data;
                }
                else
                {
                    //LogHelper.InsertLogTelegram("getIndexVariationSelected crawl not found: asin = " + asin);
                    return null;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getIndexVariationSelected error" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Lấy ra nhóm asin cùng 1 biến thể
        /// </summary>
        /// <param name="amazon_html_string"></param>
        /// <returns></returns>
        private static List<string> getGroupDimensionAsin(string amazon_html_string, string pattern_dimension)
        {
            try
            {
                var group_asin = new List<string>();
                var rs_variation_data = new List<string>();
                string pattern = @"\""dimensionToAsinMap\"" : (.*),";

                Match variation_data = Regex.Match(amazon_html_string, pattern);

                if (!string.IsNullOrEmpty(variation_data.ToString()))
                {
                    RegexOptions options = RegexOptions.Multiline;
                    foreach (Match m in Regex.Matches(variation_data.ToString(), pattern_dimension, options))
                    {
                        string value_regex = (variation_data.Groups[1].ToString());

                        var json_img_data = JArray.Parse("[" + value_regex.ToString() + "]");
                        //string item_dimension = json_img_data[0]["dimensionToAsinMap"].ToString();
                        var lst_dimension_lib = json_img_data[0].Children().ToList();

                        foreach (JToken item_dimension in lst_dimension_lib)
                        {
                            string item_variation = ((Newtonsoft.Json.Linq.JProperty)item_dimension).Name.ToString();
                            //m: Là giá trị regex được theo biểu thức chính quy.
                            if (item_variation == m.Value)
                            {
                                group_asin.Add(((Newtonsoft.Json.Linq.JProperty)item_dimension).Value.ToString());
                            }
                        }

                    }

                    return group_asin;
                }
                else
                {
                    //LogHelper.InsertLogTelegram("getDimensionToAsinMap crawl not found: pattern = " + pattern);
                }
                return null;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getDimensionToAsinMap error" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Lay ra variation item dang active va build ra id
        /// </summary>
        /// <param name="amazon_html_string"></param>
        /// <returns></returns>
        public static string getSelectedVariationValues(string amazon_html_string)
        {
            var variation_list_name = new List<string>();
            try
            {
                string pattern = @"""selectedVariationValues"" : (.*),";

                Match variation_data = Regex.Match(amazon_html_string, pattern);

                if (!string.IsNullOrEmpty(variation_data.ToString()))
                {
                    var json_data = JArray.Parse("[" + variation_data.Groups[1].Value + "]");
                    var lst_img = json_data.Children().ToList();
                    foreach (JToken item in lst_img)
                    {
                        // variation_list_name.Add(CommonHelper.RemoveAllNonCharacter(item.ToString()));
                        string name = ((Newtonsoft.Json.Linq.JProperty)item.Last()).Name.ToString();
                        string value = ((Newtonsoft.Json.Linq.JValue)((Newtonsoft.Json.Linq.JContainer)item.Last()).First).Value.ToString();
                        return name + "_" + value;
                    }
                }
                return "swatchSelect";
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getSelectedVariationValues error" + ex.ToString());
                return "swatchSelect";
            }
        }

        /// <summary>
        /// Lấy ra tên các biến thể của 1 sản phẩm
        /// </summary>
        /// <param name="amazon_html_string"></param>
        /// <returns></returns>
        public static List<string> getVariationName(string amazon_html_string)
        {
            var variation_list_name = new List<string>();
            try
            {
                string pattern = @"""dimensionsDisplay"" : (.*),";


                Match variation_data = Regex.Match(amazon_html_string, pattern);

                if (!string.IsNullOrEmpty(variation_data.ToString()))
                {
                    var json_img_data = JArray.Parse(variation_data.Groups[1].Value);
                    var lst_img = json_img_data.Children().ToList();
                    foreach (JToken item in lst_img)
                    {
                        variation_list_name.Add(CommonHelper.RemoveAllNonCharacter(item.ToString()));
                    }
                }
                return variation_list_name;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getVariationName error" + ex.ToString());
                return variation_list_name;
            }
        }
        public static bool GetFuzzyPrimeEligibility(string itemHtml)
        {
            try
            {
                string fuzzyPrimeEligibilityPattern = @".*?FREE.*?Shipping";
                string match = CommonHelper.GetSingleRegExMatch(itemHtml, fuzzyPrimeEligibilityPattern);
                return (match.Length > 0);
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("GetFuzzyPrimeEligibility error" + ex.ToString());
                return false;
            }
        }

        public static double[] getReviewCound(string amazon_html_string)
        {
            try
            {
                double[] reviews = new double[5];
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(amazon_html_string);
                HtmlNode node_reviewHistogramHtml = document.GetElementbyId("histogramTable");

                if (node_reviewHistogramHtml == null) return null;

                // Find each instance of review percentage. This regex includes more than we need, but we
                // wind up only grabbing the first five results, which are the ones we care about.
                string reviewDistributionPatterh = @"(?<=title="").*?(?=%)";

                List<string> matches = CommonHelper.GetMultipleRegExMatches(node_reviewHistogramHtml.InnerHtml, reviewDistributionPatterh);

                // If we can't find any more results, exit
                if (matches.Count == 0)
                { return null; }

                var distinct_rate = matches.ToList().Distinct().ToList().OrderBy(x => x).ToList();
                var rank_list = new Dictionary<int, int>();

                foreach (var item in distinct_rate)
                {
                    int level_star = Convert.ToInt32(item.Split(" ").First());
                    int percen = Convert.ToInt32(item.Split(" ").Last());
                    rank_list.Add(level_star, Convert.ToInt32(percen));
                }

                for (int i = 0; i <= reviews.Length - 1; i++)
                {
                    string percent = rank_list.FirstOrDefault(x => x.Key == i + 1).Value.ToString();
                    reviews[i] = Convert.ToDouble(percent);
                }

                return reviews;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getReviewCound error" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Lấy ra danh sách seller
        /// </summary>
        /// <param name="item_html_seller_list"></param>
        /// <returns></returns>
        public static List<SellerListViewModel> getPriceBySellerList(string item_html_seller_list)
        {
            try
            {
                var seller_list = new List<SellerListViewModel>();

                CQ dom = item_html_seller_list.ToString().Trim();
                var rows_seller = dom["#aod-offer-list #aod-offer"];

                if (rows_seller != null)
                {
                    if (rows_seller.Count() > 0)
                    {
                        foreach (var item in rows_seller)
                        {
                            var seller_item = new SellerListViewModel();

                            string s_html_row = item.OuterHTML;
                            CQ row_dom = s_html_row.Trim();

                            //1. lay ra gia AMZ
                            //string item_html_price = row_dom[".a-offscreen"].Html().Trim();
                            seller_item.price = CommonHelper.RegexPriceInHtmlPage(s_html_row);

                            //2. Lấy ra phí ship nội địa $("#aod-offer-price .a-fixed-right-grid-col .a-size-base").innerHTML
                            string item_html_shipping_fee = row_dom["#aod-offer-price .a-fixed-right-grid-col .a-size-base"].Html();
                            seller_item.shiping_fee = getShippingFeeBySeller(item_html_shipping_fee);

                            //3. Lấy ra condition
                            string item_html_condition = row_dom["#aod-offer-heading h5"].Html();
                            seller_item.condition = CommonHelper.RemoveAllNonCharacter(item_html_condition);

                            //4. ngay du kien hang ve som nhat 
                            string item_html_fastest_delivery = row_dom["#upsell-message"].Html();
                            seller_item.fastest_delivery = CommonHelper.RemoveAllNonCharacter(item_html_fastest_delivery);

                            //5. ngay du kien hang ve 
                            string item_html_arrives = row_dom["#delivery-message"].Html();
                            seller_item.arrives = CommonHelper.RemoveAllNonCharacter(item_html_arrives);

                            //6. seller_name         
                            string item_seller_name = row_dom["#aod-offer-soldBy a"].Html();
                            seller_item.seller_name = CommonHelper.RemoveAllNonCharacter(item_seller_name);


                            seller_list.Add(seller_item);
                        }
                    }
                }

                return seller_list;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getPriceBySellerList error" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Chọn ra seller
        /// </summary>
        /// <returns></returns>
        public static SellerListViewModel filterSellerChoice(List<SellerListViewModel> seller_list)
        {
            try
            {
                // Lựa ra seller tốt nhất
                //1.  Chỉ hàng NEW
                var seller_filter = (from n in seller_list
                                     where n.condition.ToLower() == "new"
                                     select n).ToList();
                //2.Ưu tiên Price = min
                if (seller_filter.Count > 0)
                {
                    seller_filter = (from n in seller_filter
                                     where n.price == seller_filter.Min(y => Convert.ToDouble(y.price))
                                     select n).ToList();
                }

                // 3. Ưu tiên seller amazon.com
                if (seller_filter.Count > 0)
                {
                    var amazon_filter = (from n in seller_filter
                                         where n.seller_name.ToLower().IndexOf("amazon") >= 0
                                         select n).ToList();
                    if (amazon_filter.Count() > 0)
                    {
                        seller_filter = amazon_filter;
                    }
                }

                //4. Nếu ko có sp nào là new thì sẽ lấy ra 1 sản phẩm tân trang đầu tiên để hiển thị
                if (seller_filter.Count == 0)
                {
                    seller_filter.Add(seller_list.FirstOrDefault());
                }

                return seller_filter.FirstOrDefault();

            }
            catch (Exception ex)
            {

                //LogHelper.InsertLogTelegram("filterSellerChoice error" + ex.ToString());
                return null;
            }
        }

        // Lấy ra tổng lượt đánh giá sp
        public static string getTotalRatings(string dom_html)
        {
            try
            {
                string ratingPattern = @"[0-9]+ ratings";

                string rating = CommonHelper.GetSingleRegExMatch(dom_html, ratingPattern);

                return rating.Replace("ratings", "").Trim() == "" ? "0" : rating.Replace("ratings", "").Trim();
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getTotalRatings error" + ex.ToString());
                return "0";
            }
        }


        /// <summary>
        /// Crawl Weight by product
        /// crawl_pound: true là lấy được cân nặng. False là ko lấy dc cân nặng và set mặc định là 1 pound
        /// </summary>
        /// <returns></returns>      


        //private string getStar(ChromeDriver browers)
        //{
        //    try
        //    {
        //        var star = browers.FindElement(By.XPath("//*[@id='acrPopover']/span[1]/a/i[1]/span"));

        //        return star != null ? star.GetAttribute("innerHTML").Split(" ").First() : "0";

        //    }
        //    catch (Exception ex)
        //    {
        //        //LogHelper.InsertLogTelegram("getStar error" + ex.ToString());
        //        return "0";
        //    }
        //}

        public static bool checkHasSellerList(string page_source)
        {
            try
            {
                var pattern_seller_list = new List<string> { "olp-new", "olp-upd-new" };
                foreach (var item in pattern_seller_list)
                {
                    if (page_source.IndexOf(item) >= 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("checkHasSellerList error" + ex.ToString());
                return false;
            }
        }
        public static List<string> GetFromManufacterImage(string page_source_html)
        {
            try
            {
                List<string> from_manufacter_list = new List<string>();
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                List<string> div_class = new List<string>();
                div_class.Add("celwidget aplus-module");
                div_class.Add("content-grid-row-wrapper");
                div_class.Add("feature_BtfImage celwidget");
                foreach (var div in div_class)
                {
                    string xpath = "//div[contains(@class,\"" + div + "\")]//img";
                    var nodes = document.DocumentNode.SelectNodes(xpath);
                    string img_link;
                    if (nodes == null || nodes.Count < 1)
                    {
                        continue;
                    }
                    foreach (var node in nodes)
                    {
                        img_link = node.Attributes["src"].Value;
                        if (img_link != null && img_link.Contains("media-amazon.com/images"))
                        {
                            from_manufacter_list.Add(img_link);
                        }
                    }
                    if (from_manufacter_list.Count > 0)
                    {
                        break;
                    }
                }
                return (from_manufacter_list.Count > 0) ? from_manufacter_list : null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetFromManufacterImage  error" + ex.ToString());
                Console.WriteLine(ex);
            }
            return null;
        }
        public static int GetReviewCount(string page_source_html)
        {
            try
            {
                int review_count = 0;
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                string xpath = "//div[@id=\"ask_feature_div\"]//span[contains(@class,\"a-size-base\")]";
                var nodes = document.DocumentNode.SelectNodes(xpath);
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        var review_text = node.InnerHtml;
                        if (review_text.Contains(" answered questions"))
                        {
                            try
                            {
                                review_count = Convert.ToInt32(review_text.Replace(" answered questions", "").Replace("+", "").Replace(",", ""));
                                if (review_count > 0) break;
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
                return review_count;
            }
            catch (Exception)
            {
                return 0;
            }

        }
        public static double GetDiscount(string page_source_html, string product_code)
        {
            double discount = 0;
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                List<string> xpath_list = new List<string>();
                xpath_list.Add("//div[contains(@id,\"price\")]//td[contains(@class,\"priceBlockSavingsString\")]");
                xpath_list.Add("//span[contains(@id,\"savingsAmount\")]");
                xpath_list.Add("//*[contains(@class, \"a-color-price\")]/span[contains(@data-a-color, \"price\")]/span[contains(@class, \"a-offscreen\")]");
                foreach (var xpath in xpath_list)
                {
                    var node = document.DocumentNode.SelectSingleNode(xpath);
                    if (node != null && node.InnerHtml.Contains("$"))
                    {
                        var text = node.InnerHtml;
                        if (text != null && text.Contains("(") && text.Contains(")"))
                        {
                            var a = text.Split("(")[0].Replace("Save", "").Replace(":", "");
                            try
                            {
                                string c = a.Trim().Replace("$", "");
                                discount = Convert.ToDouble(c);
                                if (discount > 0) break;
                            }
                            catch { }
                        }
                        else if (text != null && text.Contains("$"))
                        {
                            try
                            {
                                string c = text.Trim().Replace("$", "");
                                discount = Convert.ToDouble(c);
                                if (discount > 0) break;
                            }
                            catch { }
                        }
                    }
                }
                return discount;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage GetDiscount - 1.0 with [ " + product_code + " ] : error = " + ex.ToString());
                return 0;
            }


        }
        private static bool IsAmzChoices(string page_source_html)
        {
            bool is_amz_choice = false;
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                string xpath = "//div[@id=\"acBadge_feature_div\"]//div[contains(@class,\"ac-badge-wrapper\")]";
                var nodes = document.DocumentNode.SelectNodes(xpath);
                if (nodes != null && nodes.Count > 0)
                {
                    is_amz_choice = true;
                }
            }
            catch (Exception)
            {
            }
            return is_amz_choice;
        }
        private static string GetProductFrequentlyBoughtTogether(string page_source_html)
        {
            string product_list = null;
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);

                string xpath = "//ul[contains(@class,\"a-unordered-list a-nostyle a-horizontal\")]//a[contains(@class,\"a-link-normal\")]";
                var nodes = document.DocumentNode.SelectNodes(xpath);
                if (nodes != null && nodes.Count > 0)
                {
                    product_list = "";
                    foreach (var node in nodes)
                    {
                        if (product_list.Trim() != "") product_list += ",";
                        else product_list = "";
                        var review_text = node.Attributes["href"].Value;
                        if (!review_text.Contains("http")) review_text = "https://www.amazon.com/" + review_text;
                        string asin = null;
                        CommonHelper.CheckAsinByLink(review_text, out asin);
                        if (asin != null && asin.Trim() != "")
                        {
                            try
                            {
                                product_list += asin;
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return product_list;
        }
        private static List<ProductRelated> GetProductRelated(string page_source_html, string asin)
        {
            List<ProductRelated> product_list = null;
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                List<string> xpath_list = new List<string>();
                xpath_list.Add("//div[contains(@id,\"sp_detail\")]//li[contains(@class,\"a-carousel-card\")]");
                xpath_list.Add("//div[contains(@id,\"device-dp-recommendations_feature_div\")]//li[contains(@class,\"a-carousel-card\")]");
                foreach (var xpath in xpath_list)
                {
                    if (product_list == null) product_list = new List<ProductRelated>();
                    var block_list = document.DocumentNode.SelectNodes(xpath);
                    if (block_list != null && block_list.Count > 0)
                    {
                        HtmlNode block = null;
                        foreach (var n in block_list)
                        {
                            string pcode = null;
                            try
                            {
                                block = n.SelectSingleNode(".//div[contains(@id,\"sp_detail\")]");
                                if (block != null)
                                {
                                    pcode = block.Attributes["data-asin"].Value;
                                }
                            }
                            catch (Exception)
                            {
                                pcode = null;
                            }
                            if (pcode != null)
                            {
                                // Case 1 : Other Product
                                // ProductCode:
                                ProductRelated productRelated = new ProductRelated();
                                productRelated.product_code = pcode;
                                block = n.SelectSingleNode(".//a[contains(@class,\"a-link-normal\")]//img");
                                if (block != null)
                                {
                                    // Image
                                    productRelated.product_image = block.Attributes["src"].Value;
                                    // Product Name
                                    try
                                    {
                                        productRelated.product_name = block.Attributes["alt"].Value;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                //Star (rate):
                                block = n.SelectSingleNode(".//i[contains(@class,\"a-icon a-icon-star\")]");
                                if (block != null)
                                {
                                    string star_class = block.Attributes["class"].Value;
                                    var arr = star_class.Split(" ");
                                    foreach (var item in arr)
                                    {
                                        double result = 0;
                                        var rs = double.TryParse(item.Trim().Replace("a-star-", "").Replace("-", "."), out result);
                                        if (rs)
                                        {
                                            productRelated.rate = result;
                                            break;
                                        }
                                    }
                                }
                                // Ratings
                                block = n.SelectSingleNode(".//span[contains(@class,\"a-color-link\")]");
                                if (block != null)
                                {
                                    double result = 0;
                                    var rs = double.TryParse(block.InnerHtml.Trim(), out result);
                                    if (rs)
                                    {
                                        productRelated.ratings = result;
                                    }
                                }
                                // Price
                                block = n.SelectSingleNode(".//a[contains(@class,\"a-link-normal\")]//span[contains(@class,\"a-color-price\")]");
                                if (block != null)
                                {
                                    double result = 0;
                                    var rs = double.TryParse(block.InnerHtml.Trim().Replace(",", "").Replace("$", ""), out result);
                                    if (rs)
                                    {
                                        productRelated.price = result;
                                    }
                                }
                                // Is prime:
                                block = n.SelectSingleNode(".//i[contains(@class,\"a-icon-prime\")]");
                                if (block != null)
                                {
                                    productRelated.is_prime_eligible = true;
                                }
                                else
                                {
                                    productRelated.is_prime_eligible = false;
                                }
                                if (productRelated.product_code != null && productRelated.product_name != null && productRelated.product_image != null)
                                {
                                    product_list.Add(productRelated);
                                }
                            }
                            else
                            {
                                // Case 2 : Amazon Product
                                // Check valid:
                                pcode = null;
                                block = n.SelectSingleNode(".//a[contains(@class,\"a-link-normal\")]");
                                string url = block.Attributes["href"].Value.Contains("https") ? block.Attributes["href"].Value : "https://www.amazon.com" + block.Attributes["href"].Value;
                                CommonHelper.CheckAsinByLink(url, out pcode);
                                if (pcode == null || pcode.Trim() == "") continue;
                                ProductRelated productRelated = new ProductRelated();
                                //Product Code:
                                productRelated.product_code = pcode;
                                block = n.SelectSingleNode(".//a[contains(@class,\"a-link-normal\")]//img");
                                if (block != null)
                                {
                                    // Image
                                    productRelated.product_image = block.Attributes["src"].Value;
                                }
                                block = n.SelectSingleNode(".//div[contains(@class,\"p13n-sc-truncate-desktop\")]");
                                if (block != null)
                                {
                                    // Product Name
                                    productRelated.product_name = CommonHelper.RemoveSpecialCharacters(block.InnerHtml.Trim());
                                }
                                //Star (rate):
                                block = n.SelectSingleNode(".//i[contains(@class,\"a-icon a-icon-star\")]");
                                if (block != null)
                                {
                                    string star_class = block.Attributes["class"].Value;
                                    var arr = star_class.Split(" ");
                                    foreach (var item in arr)
                                    {
                                        double result = 0;
                                        var rs = double.TryParse(item.Trim().Replace("a-star-small-", "").Replace("-", "."), out result);
                                        if (rs)
                                        {
                                            productRelated.rate = result;
                                            break;
                                        }
                                    }
                                }
                                // Ratings
                                block = n.SelectSingleNode(".//span[contains(@class,\"a-size-small\")]");
                                if (block != null)
                                {
                                    double result = 0;
                                    var rs = double.TryParse(block.InnerHtml.Trim(), out result);
                                    if (rs)
                                    {
                                        productRelated.ratings = result;
                                    }
                                }
                                // Price
                                block = n.SelectSingleNode(".//a[contains(@class,\"a-link-normal\")]//span[contains(@class,\"a-offscreen\")]");
                                if (block != null)
                                {
                                    double result = 0;
                                    var rs = double.TryParse(block.InnerHtml.Trim().Replace(",", "").Replace("$", ""), out result);
                                    if (rs)
                                    {
                                        productRelated.price = result;
                                    }
                                }
                                // Is prime:
                                block = n.SelectSingleNode(".//i[contains(@class,\"a-icon-prime\")]");
                                if (block != null)
                                {
                                    productRelated.is_prime_eligible = true;
                                }
                                else
                                {
                                    productRelated.is_prime_eligible = false;
                                }
                                if (productRelated.product_code != null && productRelated.product_name != null && productRelated.product_image != null)
                                {
                                    product_list.Add(productRelated);
                                }
                            }
                            if (product_list.Count >= 5) break;
                        }
                    }
                    if (product_list.Count >= 5) break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage - GetProductRelated [ " + asin + " ]: error = " + ex.ToString());

            }
            return product_list;
        }
        private static Dictionary<string, string> GetProductSpecification(string page_source_html, string asin)
        {

            Dictionary<string, string> product_specification = null;
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                // Table Specification:
                //-- By ID:
                List<string> table_id = new List<string>();
                table_id.Add("technicalSpecifications");
                table_id.Add("productDetails_detailBullets");
                table_id.Add("productDetails_techSpec");
                int count = 0;
                foreach (var id in table_id)
                {
                    var xpath = "//table[contains(@id,\"" + id + "\")]//tr";
                    var nodes = document.DocumentNode.SelectNodes(xpath);
                    if (nodes != null && nodes.Count > 0)
                    {
                        if (product_specification == null) product_specification = new Dictionary<string, string>();

                        foreach (var node in nodes)
                        {
                            var th = node.SelectSingleNode(".//th");
                            var td = node.SelectSingleNode(".//td");
                            if (th != null && td != null)
                            {
                                string key = "", value = "";
                                var strong_text = th.SelectSingleNode(".//strong");
                                if (strong_text != null)
                                {
                                    key = strong_text.InnerHtml.Trim();
                                }
                                else
                                {
                                    key = th.InnerHtml.Trim();
                                }
                                var p_text = td.SelectSingleNode(".//p");
                                if (p_text != null)
                                {
                                    value = CommonHelper.StripTagsRegex(p_text.InnerHtml.Trim());
                                }
                                else
                                {
                                    value = CommonHelper.StripTagsRegex(td.InnerHtml.Trim());
                                }
                                if (value != null && !key.Contains("Customer Reviews"))
                                {
                                    key = CommonHelper.RemoveSpecialCharactersExceptDot(key.Replace("\r\n", "")).Trim();
                                    value = CommonHelper.StripTagsRegex(value.Replace("\r\n", "").Replace("Learn More", ""));
                                    value = CommonHelper.RemoveSpecialCharactersExceptDot(value);
                                    if (!product_specification.ContainsKey(key))
                                    {
                                        product_specification.Add(key.Trim(), value.Trim());
                                        count++;
                                        if (count > 3) break;
                                    }
                                }
                            }
                            if (count > 3) break;
                        }
                    }
                }
                //-- By class: Not Implemented

                // Div Specification (beauty product,...):
                List<string> div_id = new List<string>();
                div_id.Add("detailBullets_feature_div");
                foreach (var id in div_id)
                {
                    var xpath = "//div[contains(@id,\"" + id + "\")]//li";
                    var nodes = document.DocumentNode.SelectNodes(xpath);
                    if (nodes != null && nodes.Count > 0)
                    {
                        if (product_specification == null) product_specification = new Dictionary<string, string>();

                        foreach (var node in nodes)
                        {
                            var th = node.SelectSingleNode(".//span[contains(@class,\"a-text-bold\")]");
                            var td = node.SelectSingleNode(".//span[not(@class)]");
                            if (th != null && td != null && !th.InnerHtml.Contains("Customer Reviews"))
                            {
                                string key = CommonHelper.RemoveSpecialCharactersExceptDot(th.InnerHtml.Split(":")[0].Replace("\r\n", "").Trim());
                                string value = CommonHelper.StripTagsRegex(td.InnerHtml.Replace("\r\n", "").Replace("Learn More", "").Trim());
                                value = CommonHelper.RemoveSpecialCharactersExceptDot(value);
                                if (!product_specification.ContainsKey(key)) product_specification.Add(key, value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage - GetProductSpecification [ " + asin + " ] : error = " + ex.ToString());
            }
            return product_specification;
        }
        public static List<string> GetProductInfomation(string page_source_html, string asin)
        {

            List<string> product_infomation = null;
            try
            {

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                string xpath = "//div[contains(@id,\"featurebullets_feature_div\")]//span[contains(@class,\"a-list-item\")]";
                var nodes = document.DocumentNode.SelectNodes(xpath);
                if (nodes != null && nodes.Count > 0)
                {
                    if (product_infomation == null) product_infomation = new List<string>();
                    foreach (var node in nodes)
                    {
                        var value = node.InnerHtml.Trim();
                        if (value.Contains("span") || value.Contains("<a"))
                        {

                        }
                        else
                        {
                            value = CommonHelper.StripTagsRegex(value);
                            value = Regex.Replace(value, "&#?[a-z0-9]+;", " ");
                            value = CommonHelper.RemoveSpecialCharacters(value);
                            product_infomation.Add(value);
                        }
                    }
                }
                if (product_infomation == null || product_infomation.Count < 0)
                {
                    xpath = "//div[contains(@id,\"iframeContent\")]";
                    var node = document.DocumentNode.SelectSingleNode(xpath);
                    if (node != null && node.InnerHtml != null && node.InnerHtml.Trim() != "")
                    {
                        string text = CommonHelper.StripTagsRegex(node.InnerHtml.Trim());
                        text = CommonHelper.RemoveSpecialCharacters(text);
                        product_infomation.Add(text);
                    }
                }
                if (product_infomation != null && product_infomation.Count > 0)
                {
                    product_infomation.RemoveAll(u => u.Trim() == "");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage - GetProductInfomation [ " + asin + " ] : error = " + ex.ToString());
            }
            return product_infomation;
        }

        private static long GetProductRatings(string page_source_html)
        {
            long ratings = 0;
            try
            {

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                string xpath = "//span[contains(@id,\"acrCustomerReviewText\")]";
                var nodes = document.DocumentNode.SelectNodes(xpath);
                if (nodes != null && nodes.Count > 0)
                {
                    foreach (var node in nodes)
                    {
                        try
                        {
                            ratings = Convert.ToInt64(node.InnerHtml.Trim().Replace("ratings", "").Replace(" ", "").Replace(",", ""));
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage - GetProductRatings: error = " + ex.ToString());
            }
            return ratings;
        }
        public static DateTime GetSaleExprireTimes(string page_source_html, string product_code)
        {
            DateTime time_exprire = DateTime.Now;
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                string xpath = "//span[contains(@id,\"deal_expiry\")]";
                var node = document.DocumentNode.SelectSingleNode(xpath);
                if (node != null && node.InnerHtml.Contains("Ends in"))
                {
                    var text = node.InnerHtml;
                    text = text.Trim().Replace("Ends in ", "");
                    var a = text.Split(" ");
                    if (a == null || a.Count() < 1)
                    {

                    }
                    else
                    {
                        int c = -1;
                        foreach (var b in a)
                        {
                            if (b.Contains("h"))
                            {
                                c = Convert.ToInt32(b.Trim().Replace("h", ""));
                                if (c > 0) time_exprire = time_exprire.AddHours(c);
                            }
                            else if (b.Contains("m"))
                            {
                                c = Convert.ToInt32(b.Trim().Replace("m", ""));
                                if (c > 0) time_exprire = time_exprire.AddMinutes(c);
                            }
                            else if (b.Contains("s"))
                            {
                                c = Convert.ToInt32(b.Trim().Replace("s", ""));
                                if (c > 0) time_exprire = time_exprire.AddSeconds(c);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage - GetSaleExprireTimes with [ " + product_code + " ] : error = " + ex.ToString());
            }
            return time_exprire;
        }
        public static List<ProductVideo> GetProductVideos(string page_source_html, string product_code)
        {
            List<ProductVideo> videos = null;
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(page_source_html);
                string xpath = "//div[contains(@id,\"ajaxBlockComponents_feature_div\")]//script";
                List<string> other_xpath_list = new List<string>();
                other_xpath_list.Add("//script[contains(@type,\"a-state\") and contains(@data-a-state,\"imageblock-player\")]");
                videos = new List<ProductVideo>();
                var node = document.DocumentNode.SelectSingleNode(xpath);
                if (node == null)
                {
                    foreach (var xpath_other in other_xpath_list)
                    {
                        node = document.DocumentNode.SelectSingleNode(xpath);
                        if (node == null)
                        {
                            continue;
                        }
                        else
                        {
                            var obj = node.InnerHtml;
                            ProductVideo v = new ProductVideo();
                            string c = Regex.Match(obj, "\"videoUrl\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline).Groups[1].Value;
                            if (c != null && c.Trim() != "")
                            {
                                v.url = c.Trim();
                            }
                            c = Regex.Match(obj, "\"videoHeight\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline).Groups[1].Value;
                            if (c != null && c.Trim() != "")
                            {
                                v.height = Convert.ToInt32(c) < 0 ? 0 : Convert.ToInt32(c);
                            }
                            c = Regex.Match(obj, "\"videoWidth\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline).Groups[1].Value;
                            if (c != null && c.Trim() != "")
                            {
                                v.width = Convert.ToInt32(c) < 0 ? 0 : Convert.ToInt32(c);
                            }
                            c = Regex.Match(obj, "\"imageUrl\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline).Groups[1].Value;
                            if (c != null && c.Trim() != "")
                            {
                                v.thumb = c.Trim();
                            }
                            if (v.url == null || v.thumb == null || v.url == "" || v.thumb == "")
                            {
                                continue;
                            }
                            else
                            {
                                videos.Add(v);
                            }

                        }
                    }
                }
                else
                {
                    string script = node.InnerHtml;
                    if (script.Contains("var obj = A.$.parseJSON('"))
                    {
                        var lines = script.Split("\r\n");
                        foreach (var l in lines)
                        {
                            if (l == null || l.Trim() == "" || !l.Contains("var obj = A.$.parseJSON('"))
                            {
                                continue;
                            }
                            else
                            {
                                var json = l.Replace("var obj = A.$.parseJSON('", "").Replace("');", "");
                                MatchCollection matchList = Regex.Matches(json, "{.*?}");
                                var list = matchList.Cast<Match>().Select(match => match.Value).ToList();
                                foreach (var item in list)
                                {
                                    ProductVideo v = new ProductVideo();
                                    string c = Regex.Match(item, "\"videoUrl\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline).Groups[1].Value;
                                    if (c != null && c.Trim() != "")
                                    {
                                        v.url = c.Trim();
                                    }
                                    c = Regex.Match(item, "\"videoHeight\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline).Groups[1].Value;
                                    if (c != null && c.Trim() != "")
                                    {
                                        v.height = Convert.ToInt32(c) < 0 ? 0 : Convert.ToInt32(c);
                                    }
                                    c = Regex.Match(item, "\"videoWidth\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline).Groups[1].Value;
                                    if (c != null && c.Trim() != "")
                                    {
                                        v.width = Convert.ToInt32(c) < 0 ? 0 : Convert.ToInt32(c);
                                    }
                                    c = Regex.Match(item, "\"imageUrl\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline).Groups[1].Value;
                                    if (c != null && c.Trim() != "")
                                    {
                                        v.thumb = c.Trim();
                                    }
                                    if (v.url == null || v.thumb == null || v.url == "" || v.thumb == "")
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        videos.Add(v);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage - GetProductVideos with [ " + product_code + " ] : error = " + ex.ToString());
            }
            return videos;
        }
        public static string RenderProductHTML(List<string> img_link, string product_code, string product_name)
        {
            string html = "";
            string template = "<img alt=\"" + product_name + "\" data-original=\"{img_link}\" class=\"lazydetail\" src=\"{img_link}\" style =\"display: inline;\">";
            try
            {
                if (img_link != null && img_link.Count > 0)
                {
                    foreach (var img in img_link)
                    {
                        html += template.Replace("{img_link}", img);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage - RenderProductHTML with [ " + product_code + " ] : error = " + ex.ToString());
            }
            return html;
        }
        public static double getPriceForExtension(string amazon_html_string, out bool is_range_price)
        {

            is_range_price = false;
            double price = 0;
            List<string> extension_xpath = new List<string>() { "//div[@id=\"price\"]" };
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(amazon_html_string);

                foreach (var xpath in extension_xpath)
                {
                    try
                    {
                        var text = document.DocumentNode.SelectSingleNode(xpath).InnerHtml.Trim();
                        price = Convert.ToDouble(text.Replace("$", "").Replace(",", ""));
                    }
                    catch
                    {

                    }
                }
                return price;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        private static double GetPackageVolume(Dictionary<string, string> product_specification)
        {
            double volume = 0;
            try
            {
                if (product_specification != null && product_specification.Any(kvp => kvp.Key.Contains("Dimensions")))
                {
                    var list = product_specification.Where(p => p.Key.Contains("Dimensions")).Select(p => p.Value);
                    if (list.Count() > 0)
                    {
                        foreach (var s in list)
                        {
                            if (s.ToLower().Contains("x"))
                            {
                                var x = s.Split("(")[0].ToLower();
                                x = x.Split(";")[0];
                                x = x.Replace("in.", "");
                                x = x.Replace("\"", "");
                                x = x.Replace("inches", "");
                                x = x.Replace("l", "").Replace("w", "").Replace("h", "");
                                List<double> a = new List<double>();
                                var b = x.Split("x");
                                foreach (var c in b)
                                {
                                    try
                                    {
                                        a.Add(Convert.ToDouble(c));
                                    }
                                    catch { continue; }
                                }
                                if (a.Count >= 3)
                                {
                                    volume = CommonHelper.PackageVolumeFromDimensions(a);
                                    break;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return volume;
        }
        private static string GetSelectedWeight(string item_weight, double package_volume, bool is_crawl_weight)
        {
            string selected_weight = item_weight;
            try
            {
                if (package_volume > 0)
                {
                    var cubictoweight = CommonHelper.PackageVolumeToPound(package_volume);
                    if (!is_crawl_weight)
                    {
                        selected_weight = cubictoweight + " pounds";
                    }
                    else
                    {
                        string[] weight_value = item_weight.Split(" ");
                        var round_weight = Convert.ToDouble(weight_value[0]);
                        var unit = weight_value[1].Trim();
                        var crawl_pound = CommonHelper.convertToPound(round_weight, unit);
                        selected_weight = (cubictoweight >= crawl_pound ? cubictoweight : crawl_pound) + " pounds";
                    }
                }
                else
                {
                    if (!is_crawl_weight)
                    {
                        selected_weight = "1 pounds";
                    }
                }
            }
            catch
            {
            }
            return selected_weight;
        }

    }
}
