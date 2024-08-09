using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class ProductTopEntitiesViewModel
    {
        public List<GroupProductViewModel> obj_tab { get; set; }
        public List<ProductViewModel> product_list { get; set; }
        public int campaign_id { get; set; }
       
    }
}
