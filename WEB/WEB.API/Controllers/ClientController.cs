using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;
using Utilities.Contants;

namespace WEB.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : BaseController
    {

        private readonly IClientRepository clientRepository;
        private readonly IOrderRepository orderRepository;
        public IConfiguration configuration;

        public ClientController(IConfiguration config, IClientRepository _clientRepository, IOrderRepository _orderRepository)
        {
            configuration = config;
            clientRepository = _clientRepository;
            orderRepository = _orderRepository;
        }

        /// <summary>
        /// Thắng: api này dùng để add thông tin khách hàng
        /// Mapping từ khách hàng cũ về DB khách hàng của hệ thống mới
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        //      
        [HttpPost("addnew.json")]
        public async Task<ActionResult> addNewClient(string token)
        {
            JArray objParr = null;
            long client_id = -1;
            try
            {
                #region TEST DATA
                //var client_model = new ClientViewModel
                //{
                //    ClientId = -1,// -1: tạo mới | >0 sửa
                //    ClientMapID = 70,
                //    ClientName = "client_2",
                //    SourceRegisterId = (int)ClientSourceType.SourceType.SYSTEM_OLD,
                //    Email = "email_2@gmail.com",
                //    Phone = "0134564",
                //    Password = "ashdnsaklndsadnsadsadnsadsahdjouwqh902uj2o1ihed79aho21",
                //    PasswordBackup = "ashdnsaklndsadnsadsadnsadsahdjouwqh902uj2o1ihed79aho21",
                //    DateOfBirth = DateTime.Now,
                //    Avartar = "a.jpg",
                //    ConfirmPassword = "ashdnsaklndsadnsadsadnsadsahdjouwqh902uj2o1ihed79aho21",
                //    Gender = 1,
                //    TokenCreatedDate = DateTime.Now,
                //    ActiveToken = "ashdnsaklndsadnsadsadnsadsahdjouwqh902uj2o1ihed79aho21",
                //    ForgotPasswordToken = "ashdnsaklndsadnsadsadnsadsahdjouwqh902uj2o1ihed79aho21",
                //    Status = 1,
                //    Note = "Thêm mới khách hàng từ Frontend cũ ",
                //    JoinDate = DateTime.Now,
                //    TotalOrder = 20,
                //};
                //var address_client_model = new AddressClientViewModel()
                //{
                //    ReceiverName = client_model.ClientName,
                //    Phone = client_model.Phone,
                //    Address = "Số 1 Hoàng Đạo Thúy - Nhân Chính - Thanh Xuân - Hà Nội",
                //    Status = 1,
                //    IsActive = true,
                //    CreatedOn = DateTime.Now,
                //    WardId = "5",
                //    ProvinceId = "4",
                //    DistrictId = "5",
                //};


                //string j_param = "{'client_info': '" + JsonConvert.SerializeObject(client_model) +
                //    "','address_info': '" + JsonConvert.SerializeObject(address_client_model) + "'}";
                // token = CommonHelper.Encode(j_param, EncryptApi);

                #endregion

                if (!CommonHelper.GetParamWithKey(token, out objParr, EncryptApi))
                {
                    var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.EXISTS.ToString()},
                            {"msg", "Token invalid !!!"},
                            {"token",token},
                            {"client_id_response","-1"},
                        };

                    return Content(JsonConvert.SerializeObject(result));
                }
                else
                {
                    // Token hợp lệ                    

                    var clientModel = JsonConvert.DeserializeObject<ClientViewModel>(objParr[0]["client_info"].ToString());
                    var addressClientModel = JsonConvert.DeserializeObject<AddressClientViewModel>(objParr[0]["address_info"].ToString());



                    client_id = await clientRepository.addNewClient(clientModel);

                    addressClientModel.ClientId = client_id;
                    var address_id = await clientRepository.addNewAddressClient(addressClientModel);

                    if (client_id > 0)
                    {
                        var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.SUCCESS.ToString()},
                            {"msg","Add new success !"},
                            {"token",token},
                            {"client_id_response",client_id.ToString()},
                        };

                        return Content(JsonConvert.SerializeObject(result));
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("Token Client: " + token);
                        var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.FAILED.ToString()},
                            {"msg", "Add new fail !"},
                            {"token",token},
                            {"client_id_response",client_id.ToString()},
                        };

                        return Content(JsonConvert.SerializeObject(result));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("addNewClient - ClientController " + ex.Message + " client_id=" + client_id.ToString());
                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.ERROR.ToString()},
                            {"msg", "Add new fail !"},
                            {"token",token},
                            {"client_id_response","-1"},
                        };
                LogHelper.InsertLogTelegram("Token Client: " + token);
                return Content(JsonConvert.SerializeObject(result));
            }
        }

        /// <summary>
        /// Login qua hàm này để lấy Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("login.json")]
        public async Task<ActionResult> getToken(string token)
        {
            try
            {

                //create claims details based on the user information
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("token", token),
                   };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token_jwt = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn);

                var tokenJson = new JwtSecurityTokenHandler().WriteToken(token_jwt);

                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.SUCCESS.ToString()},
                            {"token",tokenJson}
                        };

                return Content(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getToken -ClientController " + ex.Message);
                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.FAILED.ToString()},
                            {"token",token}
                        };
                return Content(JsonConvert.SerializeObject(result));
            }
        }
        //[Authorize] //Add authorization attribute to the controller.   
        /// <summary>
        /// Reset pass bên us old
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="password_new"></param>
        /// <returns></returns>
        [HttpPost("putPassword.json")]
        public async Task<ActionResult> putPassword(int clientId, string password_new)
        {
            string token = "";
            try
            {
                var apiPrefix = "http://usexpress.vn/client/ressetpassword";
                string j_param = "{'password_new':'" + password_new + "','client_id':'" + clientId + "'}";
                token = CommonHelper.Encode(j_param, EncryptApi);

                HttpClient httpClient = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token)
                });

                var rs = httpClient.PostAsync(apiPrefix, content);

                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.SUCCESS.ToString()},
                            {"token",token}
                        };
                return Content(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("putPassword - ClientController " + ex.Message);
                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.FAILED.ToString()},
                            {"token",token}
                        };
                return Content(JsonConvert.SerializeObject(result));
            }
        }

        /// <summary>
        /// Reset pass bên us old
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="password_new"></param>
        /// <returns></returns>
        [HttpPost("resetPassword.json")]
        public async Task<ActionResult> resetPassword(string token)
        {
            JArray objParr = null;
            try
            {
                string j_param = "{'client_map_id':'" + 10190 + "'}";
                token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.EXISTS.ToString()},
                            {"msg", "Token invalid !!!"},
                            {"token",token},
                        };

                    return Content(JsonConvert.SerializeObject(result));
                }
                else
                {
                    var clientMapId = int.Parse(objParr[0]["client_map_id"].ToString());
                    var client_detail = await clientRepository.getClientByClientMapId(clientMapId);
                    if (client_detail != null)
                    {
                        client_detail.Password = PresentationUtils.Encrypt("123456");
                        var resultUpdate = await clientRepository.UpdateClient(client_detail);
                        if (resultUpdate == -1)
                        {
                            LogHelper.InsertLogTelegram("Reset password thất bại cho clientMapId = " + clientMapId);
                            var result = new Dictionary<string, string>
                                {
                                    {"status",ResponseType.SUCCESS.ToString()},
                                    {"token",token},
                                    {"msg","Reset password thất bại "},
                                };
                            return Content(JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            var result = new Dictionary<string, string>
                                {
                                    {"status",ResponseType.SUCCESS.ToString()},
                                    {"token",token},
                                    {"msg","Reset password thành công cho khách hàng "+ client_detail.ClientName},
                                };
                            return Content(JsonConvert.SerializeObject(result));
                        }
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("Reset password thất bại cho clientMapId = " + clientMapId + " do không tồn tại trên hệ thống");
                        var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.SUCCESS.ToString()},
                            {"token",token},
                            {"msg","Client Map Id: " + clientMapId + "không tồn tại trên hệ thống"},
                        };
                        return Content(JsonConvert.SerializeObject(result));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("resetPassword - ClientController " + ex.Message);
                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.FAILED.ToString()},
                            {"token",token},
                            {"msg","Lỗi reset password: "+ ex},
                        };
                return Content(JsonConvert.SerializeObject(result));
            }
        }

        [HttpPost("detail.json")]
        public async Task<ActionResult> getClientDetail(string token)
        {
            string j_param = "{'clientId':'41814'}";
            // token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    long clientId = Convert.ToInt64(objParr[0]["clientId"]);
                    var clientInfo = await clientRepository.getClientDetail(clientId);
                    if (clientInfo == null)
                    {
                        return Ok(new
                        {
                            status = ResponseType.EXISTS.ToString(),
                            client_detail = new ClientViewModel(),
                            address_client_list = new List<AddressModel>(),
                            msg = "Not exists !!!"
                        });
                    }
                    var addressClientList = await clientRepository.GetClientAddressList(clientId);
                    addressClientList = addressClientList.Where(n => n.IsActive).ToList();

                    var rs = Ok(new
                    {
                        status = ResponseType.SUCCESS.ToString(),
                        client_detail = clientInfo,
                        address_client_list = addressClientList,
                        msg = "Successfully !!!"
                    });

                    return rs;
                }
                else
                {
                    return Ok(new { status = ResponseType.ERROR.ToString(), token = token, msg = "token valid !!!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientDetail - ClientController " + ex
                    + ". Token = " + token);
                return Ok(new
                {
                    status = ResponseType.FAILED.ToString(),
                    client_detail = new ClientViewModel(),
                    address_client_list = new List<AddressModel>(),
                    msg = "Fail !!!"
                });
            }
        }
        /// <summary>
        /// Thêm mới và cập nhật khách hàng
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("add-new-address.json")]
        public async Task<ActionResult> addNewAddressReceiver(string token)
        {
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    var model = Newtonsoft.Json.JsonConvert.DeserializeObject<AddressClientViewModel>(objParr[0]["address_item"].ToString());

                    // Kiem tra so dien thoai nay co bi trung voi so điện thoại của USER khác không
                    bool isValid = clientRepository.checkPhoneExist(model.Phone, model.ClientId).Result;
                    if (!isValid)
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "Số điện thoại này đã được sử dụng bởi một tài khoản khác" });
                    }

                    var address_result = await clientRepository.addNewAddressReceiverClient(model);
                    if (address_result > 0)
                    {
                        // Cập nhật địa chỉ lên số hợp đồng nếu truyền số hợp đồng sang
                        // Case: đổi địa chỉ sau khi đã tạo đơn
                        if (model.order_id > 0)
                        {
                            var address_detail = await clientRepository.GetAddressReceiverByAddressId(model.Id);
                            string full_address = address_detail[0].FullAddress;
                            string phone = address_detail[0].Phone;
                            string receiver_name = address_detail[0].ReceiverName;
                            var rs = await orderRepository.updateAdressReceiver(full_address, phone, receiver_name, model.order_id);
                        }

                        #region Push Queue để sync data client sang hệ thống cũ
                        var queue = new QueueServiceController(configuration);
                        var j_param_input = new Dictionary<string, string>
                        {
                            {"client_id",model.ClientId.ToString()},
                            {"address_id", model.Id.ToString()},
                            {"order_id", model.order_id.ToString()} // sau khi consummer sync data client xong bên db old. Sẽ tiếp tục sync địa chỉ đơn
                        };

                        var j_param = new Dictionary<string, string>
                        {
                            {"data_push",JsonConvert.SerializeObject(j_param_input)},
                            {"type",TaskQueueName.client_new_convert_queue},
                        };
                        token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), configuration["key_decode"]);
                        queue.pushDataToQueue(token);
                        #endregion                      

                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "Success", address_id = address_result });
                    }
                    else
                    {
                        string displayUrl = UriHelper.GetDisplayUrl(Request);
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "add new address error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("add-new-address.json: token valid !!! token =" + token);
                    return Ok(new { status = ResponseType.EXISTS.ToString(), _token = token, msg = "token valid !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API]--> add-new-address.json - ClientController " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = ex.ToString()
                });
            }
        }
        [HttpPost("delete-address.json")]
        public async Task<ActionResult> deleteAddressReceiver(string token)
        {
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    long client_id = Convert.ToInt64(objParr[0]["client_id"]);
                    long address_id = Convert.ToInt64(objParr[0]["address_id"]);

                    var address_result = await clientRepository.deleteAddressClient(client_id, address_id);
                    if (address_result > 0)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "Success", address_id = address_result });
                    }
                    else
                    {
                        string displayUrl = UriHelper.GetDisplayUrl(Request);
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "deleteAddressReceiver error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("deleteAddressReceiver: token valid !!! token =" + token);
                    return Ok(new { status = ResponseType.EXISTS.ToString(), _token = token, msg = "token valid !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API]-->delete-address.json - ClientController " + ex);
                return Ok(new
                {
                    status = ResponseType.FAILED.ToString(),
                    msg = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản
        /// 11-03-2021
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("update-client-info.json")]
        public async Task<ActionResult> updateClientInfo(string token)
        {
            try
            {
                //var model = new ClientInfoViewModel
                //{
                //    ClientId = 13301,
                //    Id= 13295,
                //    ReceiverName = "liu liu",
                //    Gender = 1,
                //    BirthdayDay = 15,
                //    BirthdayMonth = 2,
                //    BirthdayYear = 2022,
                //    ProvinceId = "01",
                //    DistrictId = "020",
                //    WardId = "00643",
                //    FullAddress = "Chung cư Eco Dream, Xã Tân Triều -  Thanh Trì -  Hà Nội",
                //    PasswordNew = "e10adc3949ba59abbe56e057f20f883e",
                //    PasswordOld = "e10adc3949ba59abbe56e057f20f883e", // old = new thì k cần update db
                //    ConfirmPasswordNew = "e10adc3949ba59abbe56e057f20f883e" // new # confirm sẽ k cho update vào db                    
                //};
                //string j_param = "{'client_info':'" + Newtonsoft.Json.JsonConvert.SerializeObject(model) + "'}";
                //token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    var client_model = JsonConvert.DeserializeObject<ClientInfoViewModel>(objParr[0]["client_info"].ToString());

                    var address_result = await clientRepository.updateClientInfoAddress(client_model);
                    if (address_result > 0)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "Success", address_id = address_result });
                    }
                    else
                    {
                        string displayUrl = UriHelper.GetDisplayUrl(Request);
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "add cart error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    return Ok(new { status = ResponseType.EXISTS.ToString(), _token = token, msg = "token valid !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API]--> add-new-address.json - ClientController " + ex);
                return Ok(new
                {
                    status = ResponseType.FAILED.ToString(),
                    msg = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("update-client-change-pass.json")]
        public async Task<ActionResult> updateClientChangePassword(string token)
        {
            try
            {

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    var client_model = JsonConvert.DeserializeObject<ClientChangePasswordViewModel>(objParr[0]["client_info"].ToString());

                    var result = await clientRepository.updateClientChangePassword(client_model);
                    if (result > 0)
                    {
                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = "Success" });
                    }
                    else
                    {
                        string displayUrl = UriHelper.GetDisplayUrl(Request);
                        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "update-client-change-pass.json error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    return Ok(new { status = ((int)ResponseType.EXISTS).ToString(), _token = token, msg = "token valid !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API]--> update-client-change-pass.json - ClientController " + ex);
                return Ok(new
                {
                    status = ((int)ResponseType.FAILED).ToString(),
                    msg = ex.ToString()
                });
            }
        }


        [HttpPost("register-aff.json")]
        public async Task<ActionResult> registerAffiliate(string token)
        {
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    var client_id = Convert.ToInt64(objParr[0]["client_id"]);
                    var ReferralId = objParr[0]["referral_id"].ToString();

                    var result = await clientRepository.registerAffiliate(client_id, ReferralId);
                    if (result > 0)
                    {
                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = "Success" });
                    }
                    else
                    {
                        string displayUrl = UriHelper.GetDisplayUrl(Request);
                        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "update registerAffiliate error displayUrl = " + displayUrl });
                    }
                }
                else
                {
                    return Ok(new { status = ((int)ResponseType.EXISTS).ToString(), _token = token, msg = "token valid !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API]--> register-aff.json - ClientController " + ex);
                return Ok(new
                {
                    status = ((int)ResponseType.FAILED).ToString(),
                    msg = ex.ToString()
                });
            }
        }
    }
}