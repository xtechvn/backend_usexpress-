using System;
using System.Collections.Generic;
using System.Text;

namespace AppReceiverAnalysCrawler_Jomashop.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class VariantDescription
    {
        public string __typename { get; set; }
        public string html { get; set; }
    }

    public class VariantDiscount
    {
        public string __typename { get; set; }
        public double amount_off { get; set; }
        public double percent_off { get; set; }
    }

    public class VariantDiscountOnMsrp
    {
        public string __typename { get; set; }
        public double amount_off { get; set; }
        public double percent_off { get; set; }
    }

    public class VariantFinalPrice
    {
        public string __typename { get; set; }
        public double value { get; set; }
        public string currency { get; set; }
    }

    public class VariantMediaGallery
    {
        public string __typename { get; set; }
        public string label { get; set; }
        public string role { get; set; }
        public string url { get; set; }
        public int position { get; set; }
        public List<VariantSize> sizes { get; set; }
        public string url_nocache { get; set; }
    }

    public class VariantMinimumPrice
    {
        public string __typename { get; set; }
        public VariantRegularPrice regular_price { get; set; }
        public VariantFinalPrice final_price { get; set; }
        public string price_promo_text { get; set; }
        public VariantMsrpPrice msrp_price { get; set; }
        public VariantDiscountOnMsrp discount_on_msrp { get; set; }
        public VariantDiscount discount { get; set; }
        public VariantPlpPrice plp_price { get; set; }
    }

    public class VariantMsrpPrice
    {
        public string __typename { get; set; }
        public double value { get; set; }
        public string currency { get; set; }
    }

    public class VariantPlpPrice
    {
        public string __typename { get; set; }
        public double now_price { get; set; }
    }

    public class VariantPriceRange
    {
        public string __typename { get; set; }
        public VariantMinimumPrice minimum_price { get; set; }
    }

    public class VariantRegularPrice
    {
        public string __typename { get; set; }
        public double value { get; set; }
        public string currency { get; set; }
    }

    public class JomaV2Variant
    {
        public string __typename { get; set; }
        public int id { get; set; }
        public string brand_name { get; set; }
        public string brand_url { get; set; }
        public string brand_size { get; set; }
        public string manufacturer { get; set; }
        public string shipping_availability { get; set; }
        public string is_shipping_free_message { get; set; }
        public string shipping_question_mark_note { get; set; }
        public string name_wout_brand { get; set; }
        public double msrp { get; set; }
        public string price_promo_text { get; set; }
        public object promotext_code { get; set; }
        public object promotext_type { get; set; }
        public object promotext_value { get; set; }
        public int is_preowned { get; set; }
        public string model_id { get; set; }
        public string on_hand_priority_text { get; set; }
        public int on_hand_priority { get; set; }
        public VariantPriceRange price_range { get; set; }
        public List<VariantMediaGallery> media_gallery { get; set; }
        public string sku { get; set; }
        public string stock_status { get; set; }
        public VariantDescription description { get; set; }
    }

    public class VariantSize
    {
        public string __typename { get; set; }
        public string image_id { get; set; }
        public string url { get; set; }
    }


}
