using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppCrawl_Joma_Receiver.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Image
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class MediaGallery
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class Breadcrumbs
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class ShortDescription
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class Description
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class Moredetails
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class PriceRange
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class ConfigurableOption
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class Variant
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class Yotpo
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class JomaCoreDetailV1
    {
        public string __typename { get; set; }
        public int id { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string name_wout_brand { get; set; }
        public string on_hand_priority_text { get; set; }
        public int on_hand_priority { get; set; }
        public int is_preowned { get; set; }
        public string brand_name { get; set; }
        public string brand_url { get; set; }
        public string manufacturer { get; set; }
        public string url_key { get; set; }
        public string stock_status { get; set; }
        public int out_of_stock_template { get; set; }
        public string out_of_stock_template_text { get; set; }
        public string price_promo_text { get; set; }
        public string promotext_code { get; set; }
        public string promotext_type { get; set; }
        public string promotext_value { get; set; }
        public string shipping_availability { get; set; }
        public string is_shipping_free_message { get; set; }
        public string shipping_question_mark_note { get; set; }
        public string model_id { get; set; }
        public Image image { get; set; }
        public string upc_code { get; set; }
        public string item_variation { get; set; }
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
        public string meta_keyword { get; set; }
        public string meta_description { get; set; }
        public string canonical_url { get; set; }
        public Yotpo yotpo { get; set; }
    }
    public class JomaPriceJSONModel
    {
        public double value { get; set; }
        public string currency { get; set; }
        public string __typename { get; set; }
    }
    public class JomaPriceViewModel
    {
        public double value { get; set; }
        public string currency { get; set; }
        public string __typename { get; set; }
    }
    public class JomaDiscountViewModel
    {
        public double amount_off { get; set; }
        public double percent_off { get; set; }
        public string __typename { get; set; }
    }
    public class JomaImageViewModel
    {
        public string label { get; set; }
        public string url { get; set; }
        public string __typename { get; set; }
    }
    public class JomaImageDetailViewModel
    {
        public string image_id { get; set; }
        public string url { get; set; }
        public string __typename { get; set; }
    }
    public class JomaReviewerViewModel
    {
        public double average_score { get; set; }
        public int reviews_count { get; set; }
        public string __typename { get; set; }
    }


    //--Variation Label

    public class JomaVariationAttribute
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class JomaVariationProduct
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class JomaVariationLabelViewModel
    {
        public List<JomaVariationAttribute> attributes { get; set; }
        public JomaVariationProduct product { get; set; }
        public string __typename { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Brand
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string name { get; set; }
    }

    public class Manufacturer
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string name { get; set; }
    }

    public class Seller
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string name { get; set; }
    }

    public class OfferList
    {
        [JsonProperty("@type")]
        public string? Type { get; set; }
        public string? priceCurrency { get; set; }
        public string? price { get; set; }
        public string? availability { get; set; }
        public string? itemCondition { get; set; }
        public string? url { get; set; }
        public string? sku { get; set; }
        public string? description { get; set; }
    }
    public class AggregateRating
    {
        [JsonProperty("@type")]
        public string? Type { get; set; }
        public float? ratingValue { get; set; }
        public int? reviewCount { get; set; }
        public float? bestRating { get; set; }
    }
    public class Offers
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string url { get; set; }
        public string priceCurrency { get; set; }
        public int offerCount { get; set; }
        public string highPrice { get; set; }
        public string lowPrice { get; set; }
        public string priceValidUntil { get; set; }
        public string availability { get; set; }
        public Seller seller { get; set; }
        public List<OfferList>? offerList { get; set; }
    }

    public class JSONJomaDetail
    {
        [JsonProperty("@context")]
        public string Context { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }
        public string name { get; set; }
        public List<string> image { get; set; }
        public string description { get; set; }
        public AggregateRating? aggregateRating { get; set; }
        public string sku { get; set; }
        public string mpn { get; set; }
        public string model { get; set; }
        public Brand brand { get; set; }
        public Manufacturer manufacturer { get; set; }
        public Offers offers { get; set; }
    }
    public class JomaVariantDetailPriceRange
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class JomaVariantDetailMediaGallery
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class JomaVariantDetailDescription
    {
        public string type { get; set; }
        public bool generated { get; set; }
        public string id { get; set; }
        public string typename { get; set; }
    }

    public class JomaVariantDetailViewModel
    {
        public long id { get; set; }
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
        public string promotext_code { get; set; }
        public string promotext_type { get; set; }
        public string promotext_value { get; set; }
        public int is_preowned { get; set; }
        public string model_id { get; set; }
        public string on_hand_priority_text { get; set; }
        public int on_hand_priority { get; set; }
        public JomaVariantDetailPriceRange price_range { get; set; }
        public List<JomaVariantDetailMediaGallery> media_gallery { get; set; }
        public string sku { get; set; }
        public string stock_status { get; set; }
        public JomaVariantDetailDescription description { get; set; }
        public string __typename { get; set; }
    }
}
