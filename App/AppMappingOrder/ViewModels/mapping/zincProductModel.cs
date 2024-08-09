using System;
using System.Collections.Generic;
using System.Text;

namespace AppMappingOrder.ViewModels.mapping
{
    [Serializable]
    public class zincProductModel
    {
        public string asin { get; set; }
        public string title { get; set; }
        public List<string> images { get; set; } // co hoac khong
        public string price { get; set; }
        public string seller_name { get; set; }
        public string status { get; set; }
        public string extra_description { get; set; }
        public string timestamp { get; set; }
        public zinc_all_variants[] all_variants { get; set; }
        public string retailer { get; set; }
        public string pantry { get; set; }
        public string product_description { get; set; }
        public string[] feature_bullets { get; set; }
        public variant_specifics[] variant_specifics { get; set; }
        public string[] authors { get; set; }

        public zinc_package_dimensions package_dimensions { get; set; }
        //public string epids { get; set; }//
        public string product_id { get; set; }

        public string ship_price { get; set; }
        public string review_count { get; set; }
        //  public string epids_map { get; set; }

        public string question_count { get; set; }
        public string brand { get; set; }
        public string[] product_details { get; set; }
        public string[] categories { get; set; }
        public string stars { get; set; }
        // public string fresh { get; set; }


        public string main_image { get; set; } // co hoac khong





    }


    [Serializable]
    public class zinc_all_variants
    {
        public variant_specifics[] variant_specifics { get; set; }
        public string product_id { get; set; }

    }

    [Serializable]
    public class variant_specifics
    {

        public string product_name { get; set; }
        public string ASIN { get; set; }
        public string dimension { get; set; }
        public string value { get; set; }
        public string offer_id { get; set; }
        public string data_image_url { get; set; }

    }

    public class zinc_package_dimensions
    {
        public zinc_unit_Model weight { get; set; }
        public zinc_size size { get; set; }
    }
    public class zinc_unit_Model
    {
        public string amount { get; set; }
        public string unit { get; set; }
    }
    public class zinc_size
    {
        public zinc_unit_Model width { get; set; }
        public zinc_unit_Model depth { get; set; }
        public zinc_unit_Model length { get; set; }
    }

}
