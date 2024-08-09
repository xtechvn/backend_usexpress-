using Aspose.Cells;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ClientDAL _ClientDAL;
        private readonly AddressClientDAL _AddressClientDAL;
        private readonly MailConfig _MailConfig;

        public ClientRepository(IOptions<DataBaseConfig> dataBaseConfig, IOptions<MailConfig> mailConfig)
        {
            _ClientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _AddressClientDAL = new AddressClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _MailConfig = mailConfig.Value;
        }

        public GenericViewModel<ClientListingModel> GetPagingList(ClientSearchModel searchModel, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<ClientListingModel>();
            try
            {
                DataTable dt = _ClientDAL.GetPagingList(searchModel, currentPage, pageSize);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model.ListData = (from row in dt.AsEnumerable()
                                      select new ClientListingModel
                                      {
                                          Id = Convert.ToInt64(row["Id"]),
                                          ClientName = row["ClientName"].ToString(),
                                          Email = row["Email"].ToString(),
                                          Phone = row["Phone"].ToString(),
                                          JoinDate = Convert.ToDateTime(!row["JoinDate"].Equals(DBNull.Value) ? row["JoinDate"] : null),
                                          Address = row["Address"].ToString(),
                                          //Ward = row["Ward"].ToString(),
                                          //District = row["District"].ToString(),
                                          //Province = row["Province"].ToString(),
                                          TotalOrder = Convert.ToInt32(row["TotalOrder"]),
                                          TotalReferralOrder = Convert.ToInt32(row["TotalReferralOrder"])
                                      }).ToList();

                    model.CurrentPage = currentPage;
                    model.PageSize = pageSize;
                    model.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / pageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList -ClientRepository: " + ex);
            }
            return model;
        }

        public string ReportClient(ClientSearchModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                DataTable dataTable = _ClientDAL.GetClientReport(searchModel);
                var ListClient = (from row in dataTable.AsEnumerable()
                                  select new ClientListingModel
                                  {
                                      Id = Convert.ToInt64(row["Id"]),
                                      ClientName = row["ClientName"].ToString(),
                                      Email = row["Email"].ToString(),
                                      Phone = row["Phone"].ToString(),
                                      JoinDate = Convert.ToDateTime(!row["JoinDate"].Equals(DBNull.Value) ? row["JoinDate"] : null),
                                      Address = row["Address"].ToString(),
                                      Ward = row["Ward"].ToString(),
                                      District = row["District"].ToString(),
                                      Province = row["Province"].ToString(),
                                      TotalOrder = Convert.ToInt32(row["TotalOrder"]),
                                      RefferalID= row["ReferralId"].ToString(),
                                      UTMMedium=row["ReferralId"].ToString() ==string.Empty?string.Empty: "us_"+ row["ReferralId"].ToString()
                                  }).ToList();

                if (ListClient != null && ListClient.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Báo cáo khách hàng";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 12);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 30);
                    cell.SetColumnWidth(2, 15);
                    cell.SetColumnWidth(3, 20);
                    cell.SetColumnWidth(4, 30);
                    cell.SetColumnWidth(5, 40);
                    cell.SetColumnWidth(6, 25);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
                    cell.SetColumnWidth(9, 25);
                    cell.SetColumnWidth(10, 25);
                    cell.SetColumnWidth(11, 25);
                    cell.SetColumnWidth(12, 25);

                    // Set header value
                    ws.Cells["A1"].PutValue("STT");
                    ws.Cells["B1"].PutValue("Tên khách hàng");
                    ws.Cells["C1"].PutValue("Phone");
                    ws.Cells["D1"].PutValue("Email");
                    ws.Cells["E1"].PutValue("Ngày đăng kí");
                    ws.Cells["F1"].PutValue("Địa chỉ");
                    ws.Cells["G1"].PutValue("Phường / Xã");
                    ws.Cells["H1"].PutValue("Quận / Huyện");
                    ws.Cells["I1"].PutValue("Tỉnh / Thành phố");
                    ws.Cells["J1"].PutValue("Tổng đơn hàng");
                    ws.Cells["K1"].PutValue("ReferralId");
                    ws.Cells["L1"].PutValue("UTM Medium");
                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, ListClient.Count, 12);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignCenterStyle = ws.Cells["A2"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["J2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 1;

                    foreach (var item in ListClient)
                    {
                        RowIndex++;
                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 1);
                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);

                        ws.Cells["B" + RowIndex].PutValue(item.ClientName);
                        ws.Cells["C" + RowIndex].PutValue(item.Phone);
                        ws.Cells["D" + RowIndex].PutValue(item.Email);
                        ws.Cells["E" + RowIndex].PutValue(item.JoinDate.Year > 2016 ? item.JoinDate.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                        ws.Cells["F" + RowIndex].PutValue(item.Address);
                        ws.Cells["G" + RowIndex].PutValue(item.Ward);
                        ws.Cells["H" + RowIndex].PutValue(item.District);
                        ws.Cells["I" + RowIndex].PutValue(item.Province);
                        ws.Cells["J" + RowIndex].PutValue(item.TotalOrder);
                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                        ws.Cells["K" + RowIndex].PutValue(item.RefferalID); 
                        ws.Cells["L" + RowIndex].PutValue(item.UTMMedium);
                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("ReportOrder - OrderRepository: " + ex);
            }
            return pathResult;
        }

        public async Task<ClientDetailModel> GetDetail(long clientId)
        {
            try
            {
                return await _ClientDAL.GetDetail(clientId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - ClientRepository: " + ex);
                return null;
            }
        }

        public async Task<List<AddressModel>> GetClientAddressList(long clientId)
        {
            try
            {
                return await _ClientDAL.GetClientAddressList(clientId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientAddressList - ClientRepository: " + ex);
                return null;
            }
        }

        public async Task<AddressClient> getAddressClientById(long address_id)
        {
            try
            {
                return await _ClientDAL.GetAddressClient(address_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getAddressClientById - ClientRepository: " + ex);
                return null;
            }
        }

        public async Task<List<AddressReceiverOrderViewModel>> GetAddressReceiverByAddressId(long address_id)
        {
            try
            {
                return await _ClientDAL.GetAddressReceiverByAddressId(address_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAddressReceiverByAddressId - ClientRepository: " + ex);
                return null;
            }
        }

        public async Task<string> ResetPassword(long clientId)
        {
            var rs = string.Empty;
            try
            {
                var _model = await _ClientDAL.FindAsync(clientId);
                var _newPassword = StringHelpers.CreateRandomPassword();
                _model.PasswordBackup = EncodeHelpers.MD5Hash(_newPassword);
                LogHelper.InsertLogTelegram("ResetPassword." + JsonConvert.SerializeObject(_model));
                await _ClientDAL.UpdateAsync(_model);
                rs = _newPassword;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetPassword - ClientRepository: " + ex);
            }
            return rs;
        }

        public async Task<string> ResetPasswordDefault(long clientId)
        {
            var rs = string.Empty;
            try
            {
                var _model = await _ClientDAL.FindAsync(clientId);
                var _newPassword = "123456";
                _model.PasswordBackup = EncodeHelpers.MD5Hash(_newPassword);
                LogHelper.InsertLogTelegram("ResetPassword." + JsonConvert.SerializeObject(_model));
                await _ClientDAL.UpdateAsync(_model);
                rs = _newPassword;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetPassword - ClientRepository: " + ex);
            }
            return rs;
        }

        public async Task<int> ChangeStatus(long clientId)
        {
            try
            {
                var model = await _ClientDAL.FindAsync(clientId);
                if (model.Status == 0)
                {
                    model.Status = 1;
                }
                else
                {
                    model.Status = 0;
                }
                LogHelper.InsertLogTelegram("ChangeStatus." + JsonConvert.SerializeObject(model));
                await _ClientDAL.UpdateAsync(model);
                return model.Status;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeStatus - ClientRepository: " + ex);
                return -1;
            }
        }

        public async Task<long> ChangeDefaultAddress(long addressId)
        {
            try
            {
                return await _ClientDAL.ChangeDefaultAddress(addressId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeDefaultAddress - ClientRepository: " + ex);
                return -1;
            }
        }

        public async Task<bool> checkPhoneExist(string phone, long client_id)
        {
            try
            {
                var detail = await _ClientDAL.getClientByPhone(phone, client_id);
                if (detail != null)
                {
                    return false;
                }
                else { return true; }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("checkPhoneExist - ClientRepository: " + ex);
                LogHelper.InsertLogTelegram(ex.Message);
                return false;
            }
        }

        public async Task<bool> checkEmailExist(string email, long client_id)
        {
            try
            {
                var detail = await _ClientDAL.getClientByEmail(email, client_id);
                if (detail != null)
                {
                    return false;
                }
                else { return true; }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("checkEmailExist - ClientRepository: " + ex);
                LogHelper.InsertLogTelegram(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Hàm này đang dùng cho CMS, FR, API 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<long> addNewClient(ClientViewModel model)
        {
            try
            {
                var client_model = new Client
                {
                    SourceRegisterId = model.SourceRegisterId,
                    ClientName = model.ClientName,
                    Email = model.Email,
                    ClientMapId = model.ClientMapID,
                    Password = model.SourceRegisterId == (int)ClientSourceType.SourceType.SYSTEM_OLD
                    ? model.Password : PresentationUtils.Encrypt(model.Password),
                    PasswordBackup = model.SourceRegisterId == (int)ClientSourceType.SourceType.SYSTEM_OLD
                    ? model.Password : PresentationUtils.Encrypt(model.Password),
                    Gender = model.Gender,
                    Avartar = model.Avartar,
                    ActiveToken = model.ActiveToken,
                    TokenCreatedDate = DateTime.Now,
                    ForgotPasswordToken = model.ForgotPasswordToken,
                    Status = Convert.ToInt32(StatusType.BINH_THUONG),
                    Note = model.SourceRegisterId == (int)ClientSourceType.SourceType.SYSTEM_OLD
                    ? "Backup từ hệ thống cũ" : "Đăng ký từ frontend",
                    JoinDate = model.SourceRegisterId == (int)ClientSourceType.SourceType.SYSTEM_OLD ? model.JoinDate : DateTime.Now,
                    IsReceiverInfoEmail = model.IsReceiverInfoEmail,
                    Phone = model.Phone
                };
                if (client_model.ClientMapId > 0) // Kiểm tra khách hàng old đã có trong db mới chưa
                {
                    var orgInfo = _ClientDAL.getClientByClientMapId(client_model.ClientMapId.Value);
                    if (orgInfo != null && orgInfo.Result != null)
                    {
                        orgInfo.Result.ClientName = client_model.ClientName;
                        orgInfo.Result.Avartar = client_model.Avartar;
                        orgInfo.Result.ActiveToken = client_model.ActiveToken;
                        orgInfo.Result.ForgotPasswordToken = client_model.ForgotPasswordToken;
                        orgInfo.Result.Gender = client_model.Gender;
                        orgInfo.Result.IsReceiverInfoEmail = client_model.IsReceiverInfoEmail;
                        orgInfo.Result.JoinDate = client_model.JoinDate;
                        orgInfo.Result.Note = client_model.Note;
                        orgInfo.Result.Password = client_model.Password;
                        orgInfo.Result.PasswordBackup = client_model.PasswordBackup;
                        orgInfo.Result.SourceRegisterId = client_model.SourceRegisterId;
                        orgInfo.Result.Status = client_model.Status;
                        orgInfo.Result.TokenCreatedDate = client_model.TokenCreatedDate;
                        var clientId = await _ClientDAL.updateClient(orgInfo.Result);
                        return clientId;
                    }
                    else
                    {
                        var clientId = await _ClientDAL.addNewClient(client_model);
                        return clientId;
                    }
                }
                else
                {
                    var clientId = await _ClientDAL.addNewClient(client_model);
                    return clientId;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("addNewClient - ClientRepository" + ex);
                return -1;
            }
        }

        public async Task<long> addNewAddressClient(AddressClientViewModel model)
        {
            try
            {
                var orgAddressCLient = await _AddressClientDAL.GetByClientId((int)model.ClientId);
                // add new address
                if (orgAddressCLient != null)
                {
                    orgAddressCLient.ClientId = model.ClientId;
                    orgAddressCLient.ReceiverName = model.ReceiverName;
                    orgAddressCLient.IsActive = model.IsActive;
                    orgAddressCLient.Phone = model.Phone;
                    orgAddressCLient.WardId = model.WardId;
                    orgAddressCLient.ProvinceId = model.ProvinceId;
                    orgAddressCLient.DistrictId = model.DistrictId;
                    orgAddressCLient.CreatedOn = DateTime.Now;
                    orgAddressCLient.Status = (int)(StatusType.BINH_THUONG);
                    orgAddressCLient.Address = model.Address;
                    var address_id = await _ClientDAL.UpdateAddressClient(orgAddressCLient);
                    return address_id;
                }
                else
                {
                    var address_model = new AddressClient
                    {
                        ClientId = model.ClientId,
                        ReceiverName = model.ReceiverName,
                        IsActive = model.IsActive,
                        Phone = model.Phone,
                        WardId = model.WardId,
                        ProvinceId = model.ProvinceId,
                        DistrictId = model.DistrictId,
                        CreatedOn = DateTime.Now,
                        Status = (int)(StatusType.BINH_THUONG),
                        Address = model.Address
                    };
                    var address_id = await _ClientDAL.addNewAddressClient(address_model);
                    return address_id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("addNewAddressClient - ClientRepository" + ex);
                return -1;
            }
        }

        public async Task<long> UpdateAddressClient(AddressClientViewModel model)
        {
            try
            {
                var orgAddressCLient = await _AddressClientDAL.GetByClientId((int)model.ClientId);
                // add new address
                if (orgAddressCLient != null)
                {
                    orgAddressCLient.ClientId = model.ClientId;
                    orgAddressCLient.ReceiverName = model.ReceiverName;
                    orgAddressCLient.IsActive = true;
                    orgAddressCLient.Phone = model.Phone;
                    orgAddressCLient.WardId = model.WardId;
                    orgAddressCLient.ProvinceId = model.ProvinceId;
                    orgAddressCLient.DistrictId = model.DistrictId;
                    orgAddressCLient.CreatedOn = DateTime.Now;
                    orgAddressCLient.Status = (int)(StatusType.BINH_THUONG);
                    orgAddressCLient.Address = model.Address;
                    var address_id = await _ClientDAL.UpdateAddressClient(orgAddressCLient);
                    return address_id;
                }
                else
                {
                    var address_model = new AddressClient
                    {
                        ClientId = model.ClientId,
                        ReceiverName = model.ReceiverName,
                        IsActive = true,
                        Phone = model.Phone,
                        WardId = model.WardId,
                        ProvinceId = model.ProvinceId,
                        DistrictId = model.DistrictId,
                        CreatedOn = DateTime.Now,
                        Status = (int)(StatusType.BINH_THUONG),
                        Address = model.Address
                    };
                    var address_id = await _ClientDAL.addNewAddressClient(address_model);
                    return address_id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("addNewAddressClient - ClientRepository" + ex);
                return -1;
            }
        }

        public async Task<ClientViewModel> getClientDetail(long client_id)
        {
            try
            {
                var detail = await _ClientDAL.getClientDetail(client_id);
                return detail;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientDetail - ClientRepository: " + ex);
                return null;
            }
        }

        public async Task<ClientViewModel> getClientByClientMapId(int client_map_id)
        {
            try
            {
                var detail = await _ClientDAL.getClientByClientMapId(client_map_id);
                if (detail == null)
                {
                    return null;
                }
                ClientViewModel clientViewModel = new ClientViewModel();
                clientViewModel.ClientId = detail.Id;
                clientViewModel.ActiveToken = detail.ActiveToken;
                clientViewModel.Avartar = detail.Avartar;
                clientViewModel.ClientMapID = detail.ClientMapId != null ? detail.ClientMapId.Value : 0;
                clientViewModel.ClientName = detail.ClientName;
                clientViewModel.Email = detail.Email;
                clientViewModel.Gender = detail.Gender != null ? detail.Gender.Value : 0;
                clientViewModel.JoinDate = detail.JoinDate;
                clientViewModel.Note = detail.Note;
                clientViewModel.SourceLoginId = detail.SourceLoginId;
                clientViewModel.SourceRegisterId = detail.SourceRegisterId != null ?
                    detail.SourceRegisterId.Value : 0;
                clientViewModel.Status = detail.Status;
                clientViewModel.TokenCreatedDate = detail.TokenCreatedDate.Value;
                clientViewModel.TotalOrder = detail.TotalOrder != null ? detail.TotalOrder.Value : 0;
                return clientViewModel;
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("getClientByClientMapId - ClientRepository: " + ex);
                return null;
            }
        }

        public async Task<ClientViewModel> getClientDetailByEmail(string email)
        {
            try
            {
                var detail = await _ClientDAL.getClientDetailByEmail(email);
                return detail;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientDetailByEmail - ClientRepository: " + ex);
                LogHelper.InsertLogTelegram(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// ////code_email: la mã được gửi vào dc email khách hàng. trường hợp gửi lại thì đọc từ session ra
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code_email"></param>
        /// <returns></returns>
        public async Task<string> sendMailCode(string email, string code_email)
        {

            try
            {
                string code_verify = code_email == string.Empty ? StringHelpers.CreateRandomNumb(6) : code_email.Trim();
                var _Subject = "Tin nhắn được gửi từ hệ thống UsExpress";
                var _Body = "<b>" + code_verify + "</b> là mã xác minh của bạn";
                await EmailHelper.SendMailAsync(email, _Subject, _Body, string.Empty, string.Empty);
                return code_verify;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("sendMailCode - ClientRepository: " + ex);
                return string.Empty;
            }
        }

        public int GetTotalClientInDay()
        {
            try
            {
                var total = 0;
                total = (int)_ClientDAL.GetTotalInDay();
                return total;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalClientInDay - ClientRepository" + ex);
                return 0;
            }
        }

        /// <summary>
        /// Thêm mới/ cập nhật địa chỉ giao hàng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<long> addNewAddressReceiverClient(AddressClientViewModel model)
        {
            try
            {
                //resset
                if (model.IsActive)
                {
                    _ClientDAL.resetActiveAddressByClientId(model.ClientId, !model.IsActive);
                }
                //update
                var orgAddressCLient = await getAddressClientById(model.Id);
                // add new address
                if (orgAddressCLient != null)
                {
                    orgAddressCLient.ClientId = model.ClientId;
                    orgAddressCLient.ReceiverName = model.ReceiverName;
                    orgAddressCLient.IsActive = model.IsActive;
                    orgAddressCLient.Phone = model.Phone;
                    orgAddressCLient.WardId = model.WardId;
                    orgAddressCLient.ProvinceId = model.ProvinceId;
                    orgAddressCLient.DistrictId = model.DistrictId;
                    orgAddressCLient.CreatedOn = DateTime.Now;
                    orgAddressCLient.Status = (int)(StatusType.BINH_THUONG);
                    orgAddressCLient.Address = model.Address;
                    var address_id = await _ClientDAL.UpdateAddressClient(orgAddressCLient);
                    return address_id;
                }
                else
                {
                    var address_model = new AddressClient
                    {
                        ClientId = model.ClientId,
                        ReceiverName = model.ReceiverName,
                        IsActive = model.IsActive,
                        Phone = model.Phone,
                        WardId = model.WardId,
                        ProvinceId = model.ProvinceId,
                        DistrictId = model.DistrictId,
                        CreatedOn = DateTime.Now,
                        Status = (int)(StatusType.BINH_THUONG),
                        Address = model.Address
                    };
                    var address_id = await _ClientDAL.addNewAddressClient(address_model);
                    return address_id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("addNewAddressClient - ClientRepository" + ex);
                return -1;
            }
        }

        public async Task<long> deleteAddressClient(long client_id, long address_id)
        {
            try
            {
                var orgAddressCLient = await _ClientDAL.deleteAddressClient(client_id, address_id);
                return orgAddressCLient;
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("deleteAddressClient - ClientRepository" + ex);
                return -1;
            }
        }

        public async Task<long> UpdateClientMapId(long client_id, long client_map_id)
        {
            try
            {
                return await _ClientDAL.UpdateClientMapId(client_id, client_map_id);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("deleteAddressClient - ClientRepository" + ex);
                return -1;
            }
        }

        public async Task<long> UpdateClient(ClientViewModel clientViewModel)
        {
            try
            {
                var model = new Client();
                model.ActiveToken = clientViewModel.ActiveToken;
                model.Avartar = clientViewModel.Avartar;
                model.ClientName = clientViewModel.ClientName;
                model.ClientMapId = clientViewModel.ClientMapID;
                model.Email = clientViewModel.Email;
                model.Password = clientViewModel.Password;
                model.PasswordBackup = clientViewModel.PasswordBackup;
                model.Gender = clientViewModel.Gender;
                model.TokenCreatedDate = clientViewModel.TokenCreatedDate;
                model.ForgotPasswordToken = clientViewModel.ForgotPasswordToken;
                model.Status = clientViewModel.Status;
                model.Note = clientViewModel.Note;
                model.JoinDate = clientViewModel.JoinDate;
                model.TotalOrder = clientViewModel.TotalOrder;
                model.SourceLoginId = clientViewModel.SourceLoginId;
                model.Phone = clientViewModel.Phone;
                model.Id = clientViewModel.ClientId;
                var rs = await _ClientDAL.updateClient(model);
                return rs;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateClient/ClientRepository Lỗi update client: " + ex);
                return -1;
            }
        }

        /// <summary>
        /// Cập nhật thông tin  khach hàng và địa chỉ của khách hàng đang chọn
        /// </summary>
        /// <param name="clientViewModel"></param>
        /// <returns></returns>
        public async Task<long> updateClientInfoAddress(ClientInfoViewModel model)
        {
            try
            {
                var ad_detail = await _ClientDAL.GetAddressClient(model.Id); //addressID
                ad_detail.ReceiverName = model.ReceiverName;
                ad_detail.Address = model.FullAddress;
                ad_detail.WardId = model.WardId;
                ad_detail.ProvinceId = model.ProvinceId;
                ad_detail.DistrictId = model.DistrictId;
                ad_detail.IsActive = true;
                ad_detail.UpdateTime = DateTime.Now;
                var rs_ad = _AddressClientDAL.UpdateAsync(ad_detail);

                var client = await _ClientDAL.GetByIdClient(model.ClientId);
                if (client.Password != model.PasswordNew && (!string.IsNullOrEmpty(model.PasswordNew)))
                {
                    client.Password = model.PasswordNew;
                    client.PasswordBackup = model.PasswordNew;
                    client.UpdateTime = DateTime.Now;
                }
                client.Birthday = Convert.ToDateTime(model.BirthdayYear + "/" + model.BirthdayMonth + "/" + model.BirthdayDay);
                client.Gender = model.Gender;
                client.ClientName = model.ReceiverName;
                var rs = await _ClientDAL.updateClient(client);
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateClient/ClientRepository Lỗi update client: " + ex);
                return -1;
            }
        }

        public async Task<long> updateClientChangePassword(ClientChangePasswordViewModel model)
        {
            try
            {
                var client = await _ClientDAL.getClientByEmail(model.Email, -1);

                if (model.PasswordNew == model.ConfirmPasswordNew && (!string.IsNullOrEmpty(model.PasswordNew)))
                {
                    client.Password = model.PasswordNew;
                    client.PasswordBackup = model.PasswordNew;
                    client.UpdateTime = DateTime.Now;
                }
                var rs = await _ClientDAL.updateClient(client);
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ClientRepository ] -- > updateClientChangePassword/ Lỗi update client: " + ex + " model =" + model.Email);
                return -1;
            }
        }

        public async Task<long> registerAffiliate(long client_id, string ReferralId)
        {
            try
            {
                var _client_id = await _ClientDAL.registerAffiliate(client_id, ReferralId);

                return _client_id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ClientRepository ] -- > registerAffiliate/ Lỗi update client: " + ex + " client_id =" + client_id);
                return -1;
            }
        }
        public async Task<long> GetAddressIDByClientID(long client_id)
        {
            try
            {
                var _client_id = await _ClientDAL.GetAddressIDByClientID(client_id);

                return _client_id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ClientRepository] -- > GetAddressIDByClientID : " + ex);
                return -1;
            }
        }
    }
}
