using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class ClientDAL : GenericService<Client>
    {
        private static DbWorker _DbWorker;
        public ClientDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public DataTable GetPagingList(ClientSearchModel searchModel, int currentPage, int pageSize)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[22];

                objParam[0] = new SqlParameter("@ClientName", searchModel.ClientName ?? string.Empty);
                objParam[1] = new SqlParameter("@Email", searchModel.Email ?? string.Empty);
                objParam[2] = new SqlParameter("@Phone", searchModel.Phone ?? string.Empty);
                objParam[3] = new SqlParameter("@OrderCode", searchModel.OrderCode ?? string.Empty);
                objParam[4] = new SqlParameter("@ProvinceId", searchModel.ProvinceId ?? string.Empty);
                objParam[5] = new SqlParameter("@DistrictId", searchModel.DistrictId ?? string.Empty);
                objParam[6] = new SqlParameter("@WardId", searchModel.WardId ?? string.Empty);
                objParam[7] = new SqlParameter("@Address", searchModel.Address ?? string.Empty);
                objParam[8] = new SqlParameter("@FromQuantity", searchModel.FromQuantity);
                objParam[9] = new SqlParameter("@ToQuantity", searchModel.ToQuantity);
                objParam[10] = new SqlParameter("@FromAmount", searchModel.FromAmount);
                objParam[11] = new SqlParameter("@ToAmount", searchModel.ToAmount);

                if (searchModel.FromDate != DateTime.MinValue)
                    objParam[12] = new SqlParameter("@FromPaymentDate", searchModel.FromDate);
                else
                    objParam[12] = new SqlParameter("@FromPaymentDate", DBNull.Value);

                if (searchModel.ToDate != DateTime.MinValue)
                    objParam[13] = new SqlParameter("@ToPaymentDate", searchModel.ToDate);
                else
                    objParam[13] = new SqlParameter("@ToPaymentDate", DBNull.Value);

                objParam[14] = new SqlParameter("@LableId", searchModel.LableId ?? string.Empty);

                if (searchModel.FromJoinDate != DateTime.MinValue)
                    objParam[15] = new SqlParameter("@FromJoinDate", searchModel.FromJoinDate);
                else
                    objParam[15] = new SqlParameter("@FromJoinDate", DBNull.Value);

                if (searchModel.ToJoinDate != DateTime.MinValue)
                    objParam[16] = new SqlParameter("@ToJoinDate", searchModel.ToJoinDate);
                else
                    objParam[16] = new SqlParameter("@ToJoinDate", DBNull.Value);

                objParam[17] = new SqlParameter("@CurentPage", currentPage);
                objParam[18] = new SqlParameter("@PageSize", pageSize);
                objParam[19] = new SqlParameter("@IsBackClientInDay", searchModel.IsBackClientInDay);
                objParam[20] = new SqlParameter("@IsPaymentInDay", searchModel.IsPaymentInDay);
                objParam[21] = new SqlParameter("@ReferralId", searchModel.ReferralId ?? string.Empty);
                
                return _DbWorker.GetDataTable(ProcedureConstants.CLIENT_SEARCH, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return null;
        }

        public DataTable GetClientReport(ClientSearchModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[17];
                objParam[0] = new SqlParameter("@ClientName", searchModel.ClientName ?? string.Empty);
                objParam[1] = new SqlParameter("@Email", searchModel.Email ?? string.Empty);
                objParam[2] = new SqlParameter("@Phone", searchModel.Phone ?? string.Empty);
                objParam[3] = new SqlParameter("@OrderCode", searchModel.OrderCode ?? string.Empty);
                objParam[4] = new SqlParameter("@ProvinceId", searchModel.ProvinceId ?? string.Empty);
                objParam[5] = new SqlParameter("@DistrictId", searchModel.DistrictId ?? string.Empty);
                objParam[6] = new SqlParameter("@WardId", searchModel.WardId ?? string.Empty);
                objParam[7] = new SqlParameter("@Address", searchModel.Address ?? string.Empty);
                objParam[8] = new SqlParameter("@FromQuantity", searchModel.FromQuantity);
                objParam[9] = new SqlParameter("@ToQuantity", searchModel.ToQuantity);
                objParam[10] = new SqlParameter("@FromAmount", searchModel.FromAmount);
                objParam[11] = new SqlParameter("@ToAmount", searchModel.ToAmount);

                if (searchModel.FromDate != DateTime.MinValue)
                    objParam[12] = new SqlParameter("@FromPaymentDate", searchModel.FromDate);
                else
                    objParam[12] = new SqlParameter("@FromPaymentDate", DBNull.Value);

                if (searchModel.ToDate != DateTime.MinValue)
                    objParam[13] = new SqlParameter("@ToPaymentDate", searchModel.ToDate);
                else
                    objParam[13] = new SqlParameter("@ToPaymentDate", DBNull.Value);

                objParam[14] = new SqlParameter("@LableId", searchModel.LableId ?? string.Empty);

                if (searchModel.FromJoinDate != DateTime.MinValue)
                    objParam[15] = new SqlParameter("@FromJoinDate", searchModel.FromJoinDate);
                else
                    objParam[15] = new SqlParameter("@FromJoinDate", DBNull.Value);

                if (searchModel.ToJoinDate != DateTime.MinValue)
                    objParam[16] = new SqlParameter("@ToJoinDate", searchModel.ToJoinDate);
                else
                    objParam[16] = new SqlParameter("@ToJoinDate", DBNull.Value);

                return _DbWorker.GetDataTable(ProcedureConstants.CLIENT_REPORT, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientReport - ClientDAL: " + ex);
            }
            return null;
        }

        public async Task<ClientDetailModel> GetDetail(long clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await (from _client in _DbContext.Client.AsNoTracking()
                                        join a in _DbContext.AddressClient.AsNoTracking() on _client.Id equals a.ClientId into af
                                        from _addr in af.DefaultIfEmpty()
                                        join p in _DbContext.Province.AsNoTracking() on _addr.ProvinceId equals p.ProvinceId into pf
                                        from _province in pf.DefaultIfEmpty()
                                        join d in _DbContext.District.AsNoTracking() on _addr.DistrictId equals d.DistrictId into df
                                        from _district in df.DefaultIfEmpty()
                                        join w in _DbContext.Ward.AsNoTracking() on _addr.DistrictId equals w.DistrictId into wf
                                        from _ward in wf.DefaultIfEmpty()
                                        where _client.Id == clientId
                                        select new ClientDetailModel
                                        {
                                            Id = _client.Id,
                                            ClientName = _client.ClientName,
                                            Phone = _addr.Phone,
                                            Email = _client.Email,
                                            JoinDate = _client.JoinDate,
                                            Province = _province.Name,
                                            District = _district.Name,
                                            Ward = _ward.Name,
                                            Address = _addr.Address,
                                            Status = _client.Status,
                                            Avartar = _client.Avartar,
                                            ClientMapId = _client.ClientMapId,
                                            ReferralId = _client.ReferralId
                                        }).FirstOrDefaultAsync();
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - ClientDAL: " + ex);
            }
            return null;
        }

        /// <summary>
        /// Sử dụng chung cho FR
        /// Nếu sửa cấu trúc thì phải kế thừa. Ko được sửa vào hàm này
        /// Thêm trường thì ok
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<List<AddressModel>> GetClientAddressList(long clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var dataList = await (from addr in _DbContext.AddressClient.AsNoTracking()
                                          join p in _DbContext.Province.AsNoTracking() on addr.ProvinceId equals p.ProvinceId into pf
                                          from _province in pf.DefaultIfEmpty()
                                          join d in _DbContext.District.AsNoTracking() on addr.DistrictId equals d.DistrictId into df
                                          from _district in df.DefaultIfEmpty()
                                          join w in _DbContext.Ward.AsNoTracking() on addr.WardId equals w.WardId into wf
                                          from _ward in wf.DefaultIfEmpty()
                                          where addr.ClientId == clientId
                                          select new AddressModel
                                          {
                                              Id = addr.Id,
                                              ReceiverName = addr.ReceiverName,
                                              Phone = addr.Phone,
                                              IsActive = addr.IsActive,
                                              ClientId = clientId,
                                              ProvinceId = _province.ProvinceId,
                                              DistrictId = _district.DistrictId,
                                              WardId = _ward.WardId,
                                              FullAddress = string.Join(" - ", new string[] { addr.Address, _ward.Name, _district.Name, _province.Name })
                                          }
                                   ).ToListAsync();
                    return dataList;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientAddressList - ClientDAL: " + ex);
                return null;
            }
        }

        public async Task<long> ChangeDefaultAddress(long addressId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var _addressModel = await _DbContext.AddressClient.FirstOrDefaultAsync(s => s.Id == addressId);
                    if (_addressModel == null)
                    {
                        return 0;
                    }
                    else
                    {
                        var _activedAddress = await _DbContext.AddressClient.FirstOrDefaultAsync(s => s.ClientId == _addressModel.ClientId && s.IsActive == true);
                        _addressModel.IsActive = true;
                        _activedAddress.IsActive = false;
                        await _DbContext.SaveChangesAsync();
                    }
                }
                return addressId;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeDefaultAddress - ClientDAL: " + ex);
                return -1;
            }
        }

        public async Task<Client> getClientByEmail(string email, long client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Client.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email.Trim() && x.Id != client_id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientByEmail - ClientDAL: " + ex);
                return null;
            }
        }

        public async Task<AddressClient> getClientByPhone(string phone, long client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AddressClient.AsNoTracking().FirstOrDefaultAsync(x => x.Phone == phone.Trim() && x.ClientId != client_id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientByPhone - ClientDAL: " + ex);
                return null;
            }
        }

        public async Task<long> addNewClient(Client model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Client.Add(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("addNewClient - ClientDAL: " + ex);
                return -1;
            }
        }
        public async Task<long> updateClient(Client model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Client.Update(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("updateClient - ClientDAL: " + ex);
                return -1;
            }
        }
        public async Task<long> addNewAddressClient(AddressClient model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.AddressClient.Add(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("addNewAddressClient - ClientDAL: " + ex);
                return -1;
            }
        }
        public async Task<long> UpdateAddressClient(AddressClient model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.AddressClient.Update(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAddressClient - ClientDAL: " + ex);
                return -1;
            }
        }
        public async Task<AddressClient> GetAddressClient(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var addressClient = await _DbContext.AddressClient.FindAsync(id);
                    return addressClient;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAddressClient - ClientDAL: " + ex);
                return null;
            }
        }
        public async Task<AddressClient> GetAddressDetailByClientId(long client_id, bool is_active)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var addressClient = _DbContext.AddressClient.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == client_id && x.IsActive == is_active);
                    if (addressClient != null)
                    {
                        return await addressClient;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAddressClient - ClientDAL: " + ex);
                return null;
            }
        }

        public async Task<long> GetAddressClient(AddressClient model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.AddressClient.Update(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAddressClient - ClientDAL: " + ex);
                return -1;
            }
        }

        /// <summary>
        /// Lấy ra chi tiết khách hàng có địa chỉ đang active
        /// </summary>
        /// <param name="client_id"></param>
        /// <returns></returns>
        public async Task<ClientViewModel> getClientDetail(long client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var address_active = await _DbContext.AddressClient.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == client_id && x.IsActive == true);

                    var detail = (from n in _DbContext.Client.AsNoTracking()
                                  join a in _DbContext.AddressClient.AsNoTracking() on n.Id equals a.ClientId
                                  where n.Id == client_id && a.Id == (address_active != null ? address_active.Id : a.Id)
                                  select new ClientViewModel
                                  {
                                      AddressId = a.Id,
                                      ClientId = n.Id,
                                      ClientMapID = n.ClientMapId != null ? n.ClientMapId.Value : 0,
                                      SourceRegisterId = n.SourceRegisterId ?? -1,
                                      SourceLoginId = n.SourceLoginId,
                                      Email = n.Email,
                                      TotalOrder = n.TotalOrder != null ? n.TotalOrder.Value : 0,
                                      TokenCreatedDate = n.TokenCreatedDate.Value,
                                      Phone = a.Phone,
                                      ClientName = n.ClientName,
                                      Avartar = n.Avartar,
                                      Status = n.Status,
                                      Note = n.Note,
                                      JoinDate = n.JoinDate,
                                      Password = n.Password,
                                      IsReceiverInfoEmail = n.IsReceiverInfoEmail ?? false,
                                      FullAddress = a.Address,
                                      DateOfBirth = n.Birthday ?? DateTime.Now,
                                      ProvinceId = a.ProvinceId == null ? "-1" : a.ProvinceId,
                                      DistrictId = a.DistrictId == null ? "-1" : a.DistrictId,
                                      WardId = a.WardId == null ? "-1" : a.WardId,
                                      Gender = n.Gender ?? 0,
                                      IsRegisterAffiliate = n.IsRegisterAffiliate ?? false,
                                      ReferralId = n.ReferralId ?? ""
                                  }).FirstOrDefaultAsync();

                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientDetail - ClientDAL: " + ex);
                return null;
            }
        }

        public async Task<ClientViewModel> getClientDetailByEmail(string email)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var detail = (from n in _DbContext.Client.AsNoTracking()
                                  join a in _DbContext.AddressClient.AsNoTracking() on n.Id equals a.ClientId
                                  where n.Email.ToLower() == email.ToLower()
                                  select new ClientViewModel
                                  {
                                      ClientId = n.Id,
                                      Email = n.Email,
                                      SourceRegisterId = n.SourceRegisterId ?? -1,
                                      Phone = a.Phone,
                                      ClientName = n.ClientName,
                                      Status = n.Status,
                                      JoinDate = n.JoinDate,
                                      IsReceiverInfoEmail = n.IsReceiverInfoEmail ?? false,
                                      Password = n.Password,
                                      IsRegisterAffiliate = n.IsRegisterAffiliate ?? false,
                                      ReferralId = n.ReferralId ?? ""
                                  }
                                  ).FirstOrDefaultAsync();

                    if (detail != null)
                    {
                        return await detail;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientDetailByEmail - " + ex);
                return null;
            }
        }
        /// <summary>
        /// cuonglv update từ int sang long ngày 13-03-2021
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Client> GetByIdClient(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Client.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(" GetByIdClient - ClientDAL: " + ex);
                return null;
            }
        }

        // Lấy ra danh sách khách hàng theo client map id: là id khách hàng bên hệ thống cũ
        public async Task<Client> getClientByClientMapId(long clientMapId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Client.AsNoTracking().FirstOrDefaultAsync(x => x.ClientMapId == clientMapId);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(" getClientByClientMapId - ClientDAL: " + ex);
                return null;
            }
        }
        public long GetTotalInDay()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var total = _DbContext.Client.Where(n => n.JoinDate.Date == DateTime.Today).Count();
                    return total;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalInDay - ClientDAL: " + ex);
                return -1;
            }
        }

        public async void resetActiveAddressByClientId(long client_id, bool is_active)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var address_list = _DbContext.AddressClient.AsNoTracking().Where(x => x.ClientId == client_id).ToList();
                    foreach (var item in address_list)
                    {
                        item.IsActive = is_active;
                    }
                    _DbContext.AddressClient.UpdateRange(address_list);
                    await _DbContext.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram(" resetActiveAddressByClientId - ClientDAL: " + ex);
            }
        }

        public async Task<long> deleteAddressClient(long client_id, long address_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AddressClient.FirstOrDefault(x => x.ClientId == client_id && x.Id == address_id);
                    _DbContext.AddressClient.Remove(detail);
                    await _DbContext.SaveChangesAsync();
                }
                return client_id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("deleteAddressClient - ClientDAL:  address_id =" + address_id + "" + ex);
                return -1;
            }
        }

        public async Task<List<AddressReceiverOrderViewModel>> GetAddressReceiverByAddressId(long address_Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await (from addr in _DbContext.AddressClient.AsNoTracking()
                                        join p in _DbContext.Province.AsNoTracking() on addr.ProvinceId equals p.ProvinceId into pf
                                        from _province in pf.DefaultIfEmpty()
                                        join d in _DbContext.District.AsNoTracking() on addr.DistrictId equals d.DistrictId into df
                                        from _district in df.DefaultIfEmpty()
                                        join w in _DbContext.Ward.AsNoTracking() on addr.WardId equals w.WardId into wf
                                        from _ward in wf.DefaultIfEmpty()
                                        where addr.Id == address_Id
                                        select new AddressReceiverOrderViewModel
                                        {
                                            Id = addr.Id,
                                            ClientId = addr.ClientId,
                                            ReceiverName = addr.ReceiverName,
                                            Phone = addr.Phone,
                                            IsActive = addr.IsActive,
                                            ProvinceId = _province.ProvinceId,
                                            DistrictId = _district.DistrictId,
                                            WardId = _ward.WardId,
                                            FullAddress = string.Join(" - ", new string[] { addr.Address, _ward.Name, _district.Name, _province.Name })
                                        }
                                   ).ToListAsync();
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAddressClientByAddressId - ClientDAL: " + ex);
                return null;
            }
        }
        public async Task<long> UpdateClientMapId(long clientId, long clientMapId)
        {
            try
            {
                var clientModel = await FindAsync(clientId);
                clientModel.ClientMapId = clientMapId;
                await UpdateAsync(clientModel);
                return clientId;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateClientMapId - ClientDAL: " + ex);
                return 0;
            }
        }

        public async Task<long> registerAffiliate(long clientId, string ReferralId)
        {
            try
            {
                var clientModel = await FindAsync(clientId);
                clientModel.IsRegisterAffiliate = true;
                clientModel.ReferralId = ReferralId;
                await UpdateAsync(clientModel);
                return clientId;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("registerAffiliate - ClientDAL: " + ex);
                return 0;
            }
        }
        public async Task<long> GetAddressIDByClientID(long client_id)
        {
            long address_id = 0;
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var address_active = await _DbContext.AddressClient.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == client_id && x.IsActive == true);
                    address_id = address_active.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAddressIDByClientID - ClientDAL: " + ex);
            }
            return address_id;

        }
        public string GetClientEmailByRefferalCode(string referral_code)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var clientModel =  _DbContext.Client.AsNoTracking().Where(x => x.ReferralId.Trim() == referral_code.Trim().Replace("us_", "")).FirstOrDefault();
                    if(clientModel!=null && clientModel.Id>0)
                    {
                        return clientModel.Email;
                    }    
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientEmailByRefferalCode - ClientDAL: " + ex);
            }
            return null;
        }

    }
}
