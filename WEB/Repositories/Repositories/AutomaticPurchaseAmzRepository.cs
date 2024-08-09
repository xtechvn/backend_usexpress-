using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels.AutomaticPurchase;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class AutomaticPurchaseAmzRepository : IAutomaticPurchaseAmzRepository
    {
        // private readonly EsConnection es_repository;
        private readonly AutomaticPurchaseDAL _automaticPurchaseDAL;
        private readonly OrderDAL _OrderDAL;
        private readonly OrderItemDAL _OrderItemDAL;
        private readonly ProductDAL _ProductDAL;

        public AutomaticPurchaseAmzRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var _StrConnection = dataBaseConfig.Value.SqlServer.ConnectionString;
            _OrderDAL = new OrderDAL(_StrConnection);
            _automaticPurchaseDAL = new AutomaticPurchaseDAL(_StrConnection);
            _OrderItemDAL = new OrderItemDAL(_StrConnection);
            _ProductDAL = new ProductDAL(_StrConnection);

        }

        public async Task<int> AddNewPurchaseDetail(long order_id)
        {
            try
            {
                var order = await _OrderDAL.GetOrderDetail(order_id);
                if (order != null && order.Id == order_id)
                {
                    var order_items = await _OrderItemDAL.GetByOrderId(order.Id);
                    if (order_items != null && order_items.Count > 0)
                    {
                        foreach (var orderItem in order_items)
                        {
                            var product = await _ProductDAL.GetById(orderItem.ProductId);
                            product.SellerId = (product.SellerId == null || product.SellerId.Trim() == "") ? "" : product.SellerId.Trim();
                            AutomaticPurchaseAmz new_detail = new AutomaticPurchaseAmz()
                            {
                                OrderId = order.Id,
                                Amount = orderItem.Price,
                                CreateDate = DateTime.Now,
                                Quanity = (int)orderItem.Quantity,
                                UpdateLast = DateTime.Now,
                                PurchaseStatus = (int)AutomaticPurchaseStatus.New,
                                PurchaseUrl = product.LinkSource.Trim().EndsWith("?smid=" + product.SellerId + "&psc=1") ? product.LinkSource.Trim() : product.LinkSource.Trim() + "?smid=" + product.SellerId + "&psc=1",
                                OrderCode = order.OrderNo,
                                ProductCode = product.ProductCode,
                                PurchasedSellerName = product.SellerId,
                                
                            };
                            await _automaticPurchaseDAL.AddNewPurchaseDetail(new_detail);
                        }
                        return (int)ResponseType.SUCCESS;
                    }
                    return (int)ResponseType.FAILED;

                }
                else
                {
                    return (int)ResponseType.NOT_EXISTS;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddNewPurchaseDetail - AutomaticPurchaseAmzRepository: " + ex);
                return (int)ResponseType.ERROR;
            }
        }

        public async Task<long> AddNewPurchaseDetail(AutomaticPurchaseAmz new_detail)
        {
            try
            {
                return await _automaticPurchaseDAL.AddNewPurchaseDetail(new_detail);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddNewPurchaseDetail - AutomaticPurchaseAmzRepository: " + ex);
                return -1;
            }
        }
        public async Task<long> AddOrUpdatePurchaseDetail(AutomaticPurchaseAmz new_detail)
        {
            try
            {
                return await _automaticPurchaseDAL.AddOrUpdatePurchaseDetail(new_detail);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddNewPurchaseDetail - AutomaticPurchaseAmzRepository: " + ex);
                return -1;
            }
        }
        public async Task<AutomaticPurchaseAmz> GetById(long id)
        {
            return await _automaticPurchaseDAL.GetById(id);
        }

        public async Task<long> GetIDByDetail(AutomaticPurchaseAmz new_detail)
        {
            return await _automaticPurchaseDAL.GetIDByDetail(new_detail);
        }

        public async Task<List<AutomaticPurchaseAmz>> GetNewPurchaseItems()
        {
            return await _automaticPurchaseDAL.GetListByPurchaseStatus((int)AutomaticPurchaseStatus.New);
        }
        public async Task<List<AutomaticPurchaseAmz>> GetTrackingList()
        {
            return await _automaticPurchaseDAL.GetTrackingList();
        }
        public async Task<Entities.ViewModels.GenericViewModel<AutomaticPurchaseAmzViewModel>> GetPagingList(AutomaticPurchaseSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            return _automaticPurchaseDAL.GetPagingList(searchModel, currentPage, pageSize);
        }

        public async Task<List<AutomaticPurchaseAmz>> GetRetryPurchaseItems()
        {
            return await _automaticPurchaseDAL.GetListByPurchaseStatus((int)AutomaticPurchaseStatus.ErrorOnExcution);
        }

        public async Task<int> UpdatePurchaseDetail(AutomaticPurchaseAmz new_detail)
        {
            await _automaticPurchaseDAL.UpdatePurchaseDetail(new_detail);
            return (int)ResponseType.SUCCESS;

        }

        public async Task<List<AutomaticPurchaseAmz>> GetEstimatedDeliveryDateByOrderNo(string orderNo)
        {
            return await _automaticPurchaseDAL.GetByConditionAsync(s => s.OrderCode == orderNo && s.DeliveryStatus == 3);
        }
    }
}
