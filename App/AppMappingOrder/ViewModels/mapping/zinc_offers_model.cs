using System;
using System.Collections.Generic;
using System.Text;

namespace AppMappingOrder.ViewModels.mapping
{
    public class zinc_offers_model
    {
        public string status { get; set; }
        public string asin { get; set; }

        public List<offers_model> offers { get; set; }
    }
    [Serializable]
    public class offers_model
    {
        public string seller_percent_positive { get; set; }
        public string prime { get; set; }

        public string ship_price { get; set; }
        public string seller_name { get; set; }

        public double price { get; set; }


        public string prime_only { get; set; }
        public string seller_num_ratings { get; set; }

        public string comments { get; set; }

        public string merchant_id { get; set; }
        public string greytext { get; set; }
        public string condition { get; set; }

        public string handling_days_min { get; set; }
        public string handling_days_max { get; set; }
        public int Seller_percent_positive { get; set; }

        public string offerlisting_id { get; set; }
        public string stars { get; set; }



    }
}
