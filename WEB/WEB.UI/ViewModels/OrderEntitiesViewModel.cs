using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class OrderEntitiesViewModel
    {
        public OrderViewModel order { get; set; }
        public List<OrderItemViewModel> order_item { get; set; }
        public string order_description { get; set; }
    }
    public class OrderEntitiesApiViewModel
    {
        public OrderViewModel order_info { get; set; }
        public List<OrderItemViewModel> list_order_item_info { get; set; }
        public List<ProductViewModel> list_product_info { get; set; }        
        public List<ImageProductViewModel> list_image_product { get; set; }
        public List<NoteViewModel> list_note_order { get; set; }
    }
}
