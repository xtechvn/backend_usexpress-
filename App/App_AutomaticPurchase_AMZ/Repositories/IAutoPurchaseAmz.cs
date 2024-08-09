using App_AutomaticPurchase_AMZ.Model;
using Entities.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_AutomaticPurchase_AMZ.Repositories
{
    public interface IAutoPurchaseAmz
    {
        public MethodOutput Login(ChromeDriver driver, string user_name, string password, bool remembered = true, bool login_need_redirect = true);
        public MethodOutput CheckProductAvailable(ChromeDriver driver);
        public MethodOutput CheckBuyNewOption(ChromeDriver driver, List<string> dictionary, out IWebElement buy_new_btn);

        public MethodOutput CheckAddToCartButtonAvailable(ChromeDriver driver,List<string> dictionary,out IWebElement add_to_cart_button);
        public MethodOutput CheckDealOrDiscountAvailable(ChromeDriver driver, Dictionary<string, string> dictionary);
        public MethodOutput CheckSeller(ChromeDriver driver,string product_code);
        public MethodOutput CheckPrice(ChromeDriver driver, List<string> price_xpath, double order_price_orginal, Dictionary<string,string> shipping_fee_xpath = null);
        public MethodOutput SelectQuanity(ChromeDriver driver, int quanity,List<string> add_to_cart_xpath);
        public MethodOutput CheckOfferListing(ChromeDriver driver, string offer_xpath);
        public MethodOutput LoadMoreOffers(ChromeDriver driver, string normal_offer_xpath);
        public MethodOutput CloseAdsPopupAfterClickAddToCart(ChromeDriver driver,List<string> skip_btn_xpath);
        public MethodOutput CartRemoveNonOrderItems(ChromeDriver driver, Dictionary<string, string> dictionary, AutomaticPurchaseAmz item);
        public MethodOutput ReCheckProductInCart(ChromeDriver driver, Dictionary<string, string> dictionary, AutomaticPurchaseAmz item);
        public MethodOutput ProccessCheckOut(ChromeDriver driver, Dictionary<string, string> dictionary, AutomaticPurchaseAmz item);
        public MethodOutput CartCheckCoupon(ChromeDriver driver,  AutomaticPurchaseAmz item);
        public MethodOutput PurchasedSuccess(ChromeDriver driver, Dictionary<string, string> dictionary, AutomaticPurchaseAmz item);
        public MethodOutput OfferListing_OfferCheck(ChromeDriver driver, IWebElement selected_offer,Dictionary<string,string> offer_xpath, AutomaticPurchaseAmz item);
        public MethodOutput OfferListing_AddToCart(ChromeDriver driver, IWebElement selected_offer, Dictionary<string, string> offer_xpath);

    }
}
