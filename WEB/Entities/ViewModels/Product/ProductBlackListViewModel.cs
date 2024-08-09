using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Product
{
    public class ProductBlackListViewModel : ProductBlackList
    { 
        public string label_name { get; set; }
        public string status_name { get; set; }
        public string create_date_string { get; set; }
        public string keyword_type_str { get; set; }
    }
}
