using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.ViewModels
{
    //public class OrdersListViewModel
    //{
    //    public int totalOrder { get; set; }
    //    public int curentPage { get; set; }
    //    public int pageSize { get; set; }
    //    public List<DataList> dataList { get; set; }

        
    //}
    public class Product
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }

    public class DataList
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public int OrderStatus { get; set; }
        public string OrderStatusName { get; set; }
        public double AmountVnd { get; set; }
        public List<Product> Product { get; set; }
    }


}
