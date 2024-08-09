using System;
using System.Collections.Generic;
using System.Text;

namespace AppReceiverAnalysCrawler_Jomashop.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Attribute
    {
        public string __typename { get; set; }
        public string code { get; set; }
        public double value_index { get; set; }
        public string label { get; set; }
    }

    public class Breadcrumbs
    {
        public string __typename { get; set; }
        public string path { get; set; }
        public List<Category> categories { get; set; }
    }

    public class Category
    {
        public string __ref { get; set; }
    }

    public class ConfigurableOption
    {
        public string __ref { get; set; }
    }

    public class Description
    {
        public string __typename { get; set; }
        public string html { get; set; }
    }

    public class Discount
    {
        public string __typename { get; set; }
        public double amount_off { get; set; }
        public double percent_off { get; set; }
    }

    public class DiscountOnMsrp
    {
        public string __typename { get; set; }
        public double amount_off { get; set; }
        public double percent_off { get; set; }
    }

    public class FinalPrice
    {
        public string __typename { get; set; }
        public double value { get; set; }
        public string currency { get; set; }
    }

    public class GroupAttribute
    {
        public string __typename { get; set; }
        public string attribute_id { get; set; }
        public string attribute_label { get; set; }
        public string attribute_value { get; set; }
    }

    public class Image
    {
        public string __typename { get; set; }
        public string label { get; set; }
        public string url { get; set; }
    }

    public class MediaGallery
    {
        public string __typename { get; set; }
        public string label { get; set; }
        public string role { get; set; }
        public string url { get; set; }
        public double position { get; set; }
        public List<Size> sizes { get; set; }
        public string url_nocache { get; set; }
    }

    public class MinimumPrice
    {
        public string __typename { get; set; }
        public RegularPrice regular_price { get; set; }
        public FinalPrice final_price { get; set; }
        public string price_promo_text { get; set; }
        public MsrpPrice msrp_price { get; set; }
        public DiscountOnMsrp discount_on_msrp { get; set; }
        public Discount discount { get; set; }
        public PlpPrice plp_price { get; set; }
    }

    public class MoreDetail
    {
        public string __typename { get; set; }
        public string group_id { get; set; }
        public string group_label { get; set; }
        public List<GroupAttribute> group_attributes { get; set; }
    }

    public class Moredetails
    {
        public string __typename { get; set; }
        public List<MoreDetail> more_details { get; set; }
    }

    public class MsrpPrice
    {
        public string __typename { get; set; }
        public double value { get; set; }
        public string currency { get; set; }
    }

    public class PlpPrice
    {
        public string __typename { get; set; }
        public double now_price { get; set; }
    }

    public class PriceRange
    {
        public string __typename { get; set; }
        public MinimumPrice minimum_price { get; set; }
    }

    public class Product
    {
        public string __ref { get; set; }
    }

    public class RegularPrice
    {
        public string __typename { get; set; }
        public double value { get; set; }
        public string currency { get; set; }
    }

    public class JomaCoreDetailV2
    {
        public string __typename { get; set; }
        public double id { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string name_wout_brand { get; set; }
        public string on_hand_priority_text { get; set; }
        public double on_hand_priority { get; set; }
        public double is_preowned { get; set; }
        public string brand_name { get; set; }
        public string brand_url { get; set; }
        public string manufacturer { get; set; }
        public string url_key { get; set; }
        public string stock_status { get; set; }
        public double out_of_stock_template { get; set; }
        public string out_of_stock_template_text { get; set; }
        public string price_promo_text { get; set; }
        public object promotext_code { get; set; }
        public object promotext_type { get; set; }
        public object promotext_value { get; set; }
        public string shipping_availability { get; set; }
        public string is_shipping_free_message { get; set; }
        public string shipping_question_mark_note { get; set; }
        public string model_id { get; set; }
        public Image image { get; set; }
        public object upc_code { get; set; }
        public object item_variation { get; set; }
        public List<MediaGallery> media_gallery { get; set; }
        public Breadcrumbs breadcrumbs { get; set; }
        public ShortDescription short_description { get; set; }
        public Description description { get; set; }
        public Moredetails moredetails { get; set; }
        public double msrp { get; set; }
        public PriceRange price_range { get; set; }
        public List<ConfigurableOption> configurable_options { get; set; }
        public List<Variant> variants { get; set; }
        public bool meta_seo_no_index { get; set; }
        public string meta_title { get; set; }
        public object meta_keyword { get; set; }
        public string meta_description { get; set; }
        public string canonical_url { get; set; }
        public Yotpo yotpo { get; set; }
    }

    public class ShortDescription
    {
        public string __typename { get; set; }
        public string html { get; set; }
    }

    public class Size
    {
        public string __typename { get; set; }
        public string image_id { get; set; }
        public string url { get; set; }
    }

    public class Variant
    {
        public string __typename { get; set; }
        public List<Attribute> attributes { get; set; }
        public Product product { get; set; }
    }

    public class Yotpo
    {
        public string __typename { get; set; }
        public double average_score { get; set; }
        public int reviews_count { get; set; }
    }


}
