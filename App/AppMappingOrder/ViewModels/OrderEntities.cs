using System;
using System.Collections.Generic;
using System.Text;

namespace AppMappingOrder.ViewModels
{
    public class OrderEntities
    {
        public OrderModel obj_order { get; set; }
        public List<OrderItemModel> obj_order_item { get; set; }
    }
}
    