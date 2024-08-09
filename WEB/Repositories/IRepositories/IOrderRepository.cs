using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Affiliate;
using Entities.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IOrderRepository
    {
        GenericViewModel<OrderGridModel> GetPagingList(OrderSearchModel searchModel, int currentPage, int pageSize);
        string ReportOrder(OrderSearchModel searchModel, string FilePath);
        List<ChartRevenuViewModel> GetRevenuByDateRange(OrderSearchModel searchModel, bool isNow = true);
        List<ChartRevenuViewModel> GetLabelRevenuByDateRange(OrderSearchModel searchModel);
        List<ChartRevenuViewModel> GetLabelQuantityByDateRange(OrderSearchModel searchModel);
        double GetRevenuDay();
        long GetTotalErrorOrderCount();
        Task<OrderViewModel> GetOrderDetail(long Id);
        Task<OrderApiViewModel> GetOrderDetailForApi(long Id);
        Task<double> GetOrderTotalAmount(long Id);
        Task<List<OrderItemViewModel>> GetOrderItemList(long Id);
        Task<long> CreateOrder(OrderViewModel order, List<OrderItemViewModel> orderItems, List<NoteModel> notes);
        Task<object> GetOrderSuggestionList(string orderNo);
        Task<long> FindOrderIdByOrderNo(string orderNo);
        Task<Order> FindOrderByOrderId(long orderId);
        RevenueViewModel SummaryRevenuToday();
        RevenueViewModel SummaryRevenuTodayTemp();
        Task<List<OrderGridModel>> GetOrderListByClientId(long ClientId);
        Task<List<OrderGridModel>> GetOrderListByReferralId(string ReferralId);
        Task<RevenueMinMax> GetMinMaxOrderAmount();
        Task<OrderViewModel> GetOrderDetailByContractNo(string orderNo);
        Task<string> BuildOrderNo(int label_id);
        Task<long> Update(OrderViewModel model);
        Task<long> UpdateOrderMapId(long order_id, long order_map_id);
        long GetTotalVoucherUse(long voucher_id,string email_client);
        public object GetOrderListFEByClientId(int clientID, string keyword, int order_status, int current_page, int page_size);
        public object GetOrderDetailFEByID(int OrderId);
        public object GetFELastestRecordByClientID(int ClientId);
        public object GetFEOrderCountByClientID(int ClientId);
        Task<bool> UpdatePaymentReCheckOut(long order_id, int address_id, short pay_type);
        Task<bool> updateAdressReceiver(string full_address, string phone, string receiver_name, long order_id);
        Task<int> GetTotalReturningClientInDay();
        Task<long> GetTotalPaymentClientInDay();
        
        Task<Order> FindAsync(long Id);
        Task<long> UpdateAsync(Order entity);
        public Task<double> getTotalOrderByEmail(string email);
        public Task<OrderAppModel> GetOrderDetailByOrderNo(string order_no);
        public Task<object> GetOrderListByClientPhone(string client_phone);
        public Task<object> GetOrderTrackingByOrderNo(string order_no);
        public Task<string> UpdateOrderStatus(string order_no, int order_status);
        Task<List<AffOrder>> GetAffiliateOrderItems(DateTime time_start, DateTime time_end, List<string> utm_source);
        public List<OrderLogShippingDateViewModel> GetOrderShippingLogToday();
        public string ExportOrderExpected(string FilePath);
        public Task<OrderViewModel> CheckOrderDetail(long Id);

    }
}
