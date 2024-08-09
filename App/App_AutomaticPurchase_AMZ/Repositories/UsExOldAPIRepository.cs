using App_AutomaticPurchase_AMZ.Model;
using Entities.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WinAppCheckoutAmazon.DBContext;

namespace App_AutomaticPurchase_AMZ.Repositories
{
    public class UsExOldAPIRepository : IUSExOldAPI
    {
        private readonly IUsExAPI _usExAPI;
        public UsExOldAPIRepository(IUsExAPI usExAPI)
        {
            _usExAPI = usExAPI;
        }
        public async Task<MethodOutput> GetAmazonCart(string update_toNew_URL, string old_db_url, USOLDToken api_Token, string key = "U1qYbPRVdnNdKMC7pmJ0Qm96vJCLefzb6TKzPuEFRyZVPz1RwJ7Kbw6oUrXRh14ItgwPB7xFy4r6IrLL")
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed
            };
            try
            {
                HttpClient httpClient_2 = new HttpClient();
                //setup client
                httpClient_2.DefaultRequestHeaders.Accept.Clear();
                httpClient_2.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient_2.DefaultRequestHeaders.Add("Authorization", "Bearer " + api_Token.token);
                //send request
                var responseMessage = await httpClient_2.GetAsync(old_db_url);
                //get access token from response body
                var responseJson = responseMessage.Content.ReadAsStringAsync().Result;
                var jObject = JObject.Parse(responseJson);
                if (responseMessage.StatusCode == HttpStatusCode.OK && jObject.GetValue("Code").ToString() == "0")
                {
                    List<AmazonCart> carts = null;
                    try
                    {
                        var str = JsonConvert.SerializeObject(jObject.GetValue("Data"));
                        carts = JsonConvert.DeserializeObject<List<AmazonCart>>(str);
                    }
                    catch(Exception ex)
                    {
                        methodOutput.message = "Cannot Convert Data: " + jObject.GetValue("Data").ToString();
                        return methodOutput;
                    }
                    List<AutomaticPurchaseAmz> carts_new = new List<AutomaticPurchaseAmz>();
                    if (carts!=null && carts.Count > 0)
                    {
                        foreach(var cart in carts)
                        {
                            AutomaticPurchaseAmz model = new AutomaticPurchaseAmz()
                            {
                                Amount = Convert.ToDouble(cart.Amount),
                                CreateDate = DateTime.Now,
                                ManualNote = "Cập nhật từ DB OLD",
                                OrderCode = cart.OrderCode,
                                OrderMappingId = cart.OrderId.ToString(),
                                ProductCode = cart.ASIN.ToUpper(),
                                PurchaseStatus = (int)AutomaticPurchaseStatus.New,
                                PurchaseMessage = "",
                                PurchaseUrl = cart.PurchaseURL,
                                Quanity = cart.Quantity == null ? 1 : (int)cart.Quantity,
                                UpdateLast = DateTime.Now,
                                AutoBuyMappingId = cart.Id
                            };
                            var add_new=await _usExAPI.AddNewItem(update_toNew_URL, model);
                            try
                            {
                                AutomaticPurchaseAmz return_model = JsonConvert.DeserializeObject<AutomaticPurchaseAmz>(JsonConvert.SerializeObject(add_new.data));
                                model.Id = return_model.Id;
                                model.OrderId = return_model.OrderId;
                                carts_new.Add(model);
                            }
                            catch
                            {
                                methodOutput.message = "Update to DB New Failed: " + add_new.message;
                                return methodOutput;
                            }
                        }
                    }
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    methodOutput.message = jObject.GetValue("Message").ToString() + methodOutput.message;
                    methodOutput.data = JsonConvert.SerializeObject(carts_new);
                }
                else
                {
                    methodOutput.message = jObject.GetValue("Data").ToString();
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public async Task<MethodOutput> GetToken(string url, string user_name, string password)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed
            };
            try
            {
                HttpClient httpClient_2 = new HttpClient();
                //setup client
                httpClient_2.BaseAddress = new Uri(url);
                httpClient_2.DefaultRequestHeaders.Accept.Clear();
                httpClient_2.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", user_name),
                        new KeyValuePair<string, string>("password", password),
                    });
                //send request
                var responseMessage = await httpClient_2.PostAsync("token", formContent);
                //get access token from response body
                var responseJson = responseMessage.Content.ReadAsStringAsync().Result;
                var jObject = JObject.Parse(responseJson);
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    methodOutput.message = jObject.GetValue("access_token").ToString();
                    try{
                        string token_path = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\net6.0", "") + @"\token.json";
                        if (!File.Exists(token_path))
                        {
                            File.Create(token_path);
                            File.WriteAllText(token_path, jObject.GetValue("access_token").ToString());
                        }
                    }
                    catch { }
                }
                else
                {
                    methodOutput.message = "Get Token failed:" + responseJson;
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
           
        }

        public async Task<MethodOutput> SendEmailAPI(string email_url, USOLDToken api_Token, Dictionary<string,string> emailTemplate)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed
            };
            try
            {
                var email_data = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("FromEmail",emailTemplate["FromEmail"]),
                    new KeyValuePair<string, string>("FromName","USExpress - App_AutomaticPurchase_AMZ BOT"),
                    new KeyValuePair<string, string>("ToEmail",emailTemplate["ToEmail"] ),
                    new KeyValuePair<string, string>("Subject",emailTemplate["Subject"] ),
                    new KeyValuePair<string, string>("Body",emailTemplate["Body"] ),
                };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (api_Token==null || string.IsNullOrEmpty(api_Token.token))
                    {
                        methodOutput.message = "SendEmailAPI - Token Invalid: ";
                        return methodOutput;
                    }
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + api_Token.token);
                    var formContent = new FormUrlEncodedContent(email_data);

                    var responseMessage = await client.PostAsync(email_url, formContent);
                    var responseJson = responseMessage.Content.ReadAsStringAsync().Result;
                    if (responseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                        methodOutput.message = responseJson;
                        return methodOutput;
                    }
                    else
                    {
                        methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                        methodOutput.message = responseJson;
                    }
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public async Task<MethodOutput> UpdateAmazonCart(AutomaticPurchaseAmz new_detail, USOLDToken api_Token, string url, string key = "U1qYbPRVdnNdKMC7pmJ0Qm96vJCLefzb6TKzPuEFRyZVPz1RwJ7Kbw6oUrXRh14ItgwPB7xFy4r6IrLL")
        {
            MethodOutput methodOutput = new MethodOutput();
            try
            {
                HttpClient httpClient_2 = new HttpClient();
                //setup client
                httpClient_2.DefaultRequestHeaders.Accept.Clear();
                httpClient_2.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient_2.DefaultRequestHeaders.Add("Authorization", "Bearer " + api_Token.token);
                var apiQueueService = url;
                var token_2 = JsonConvert.SerializeObject(new_detail);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(token_2);
                var token = System.Convert.ToBase64String(plainTextBytes);
                var queryString = new StringContent("\""+ token+"\"", Encoding.UTF8, "text/html");
                var result_2 = await httpClient_2.PostAsync(apiQueueService+ "?key="+key, queryString);
                dynamic resultContent_2 = Newtonsoft.Json.Linq.JObject.Parse(result_2.Content.ReadAsStringAsync().Result);
                var status = (int)resultContent_2.Code;
                if (status == (int)ResponseType.SUCCESS)
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Success,
                        message = "Success",
                    };
                }
                else
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Failed,
                        message = (string)resultContent_2.Data,
                    };
                    LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - UsExOldAPIRepository - UpdateAmazonCart to DB_OLD with AutoPurchaseID: " + new_detail.Id + "  Purchase URL: " + new_detail.PurchaseUrl + "  \nError: " + JsonConvert.SerializeObject(resultContent_2));
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
                LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - UsExOldAPIRepository - UpdateAmazonCart to DB_OLD with AutoPurchaseID: " + new_detail.Id + "  Purchase URL: " + new_detail.PurchaseUrl + "  \nError: " +ex.ToString());

            }
            return methodOutput;
        }
    }
}
