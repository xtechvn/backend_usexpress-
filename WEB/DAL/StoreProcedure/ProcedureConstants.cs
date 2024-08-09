using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.StoreProcedure
{
    public class ProcedureConstants
    {
        #region Client
        public const string CLIENT_SEARCH = "sp_client_search";
        public const string CLIENT_REPORT = "Client_Report";
        #endregion

        #region Order
        public const string ORDER_SEARCH = "sp_order_search";
        public const string ORDER_REPORT = "Order_Report";
        public const string ORDER_GetListByClientId = "Order_GetListByClientId";
        public const string ORDER_FE_GetDetailById = "Order_FE_GetDetailById";
        public const string Order_FE_SelectLatestOrderbyClientID = "Order_FE_SelectLatestOrderbyClientID";
        public const string Order_FE_GetOrderCountByClientID = "Order_FE_GetOrderCount";
        public const string ORDER_GetErrorOrderCount = "Order_GetErrorOrderCount";
        public const string Order_GetShippingExpectedDays = "Order_GetShippingExpectedDays";
        public const string OrderExpected_Export = "OrderExpected_Export";

        #endregion

        #region product
        public const string PRODUCT_GetBoughtQuantity = "Product_GetBoughtQuantity";
        public const string PRODUCT_GetInterestedProduct = "Product_GetInterestedProduct";
        public const string PRODUCT_GetInterestedProduct_TotalRecord = "Product_GetInterestedProduct_TotalRecord";

        #endregion

        #region Summary
        public const string GET_REVENU_DAY = "sp_GetPercentRevenue";
        #endregion

        #region Chart
        public const string GET_REVENU_DATE_RANGE = "sp_GetRevenueByDateRange";
        public const string GET_LABEL_REVENU_DATE_RANGE = "sp_GetLabelRevenueByDateRange";
        public const string GET_LABEL_QUANTITY_DATE_RANGE = "sp_GetOrderCountForEachLabelByDateRange";
        #endregion

        #region NEWS
        public const string ARTICLE_SEARCH = "Article_Search";
        #endregion
        #region AUTOMATIC PURCHASE
        public const string AUTOMATIC_PURCHASE_SEARCH = "AutomaticPurchase_Search";
        #endregion
    }
}
