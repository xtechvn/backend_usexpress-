using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using App_AutoPurchase_TrackingOrders.Model;
using Utilities.Contants;
using Newtonsoft.Json;
using Utilities;
using System.Linq;
using Entities.ViewModels.AutomaticPurchase;
using Entities.Models;

namespace App_AutoPurchase_TrackingOrders.Repositories
{
    public class UsExAPIRepository : IUsExAPI
    {
        public async Task<MethodOutput> GetTrackingList(string url)
        {
            MethodOutput methodOutput = new MethodOutput();
            try
            {
                HttpClient httpClient_2 = new HttpClient();
                var apiQueueService = url;
                var token = "ShFYQFBcSmxZXWN7eQhXZXwb";
                var content = new FormUrlEncodedContent(new[]
                   {
                       new KeyValuePair<string, string>("token", token),
                   });
                var result_2 = await httpClient_2.PostAsync(apiQueueService, content);
                dynamic resultContent_2 = Newtonsoft.Json.Linq.JObject.Parse(result_2.Content.ReadAsStringAsync().Result);
                var status = (int)resultContent_2.status;
                if (status == (int)MethodOutputStatusCode.Success)
                {
                    if (resultContent_2.data != null)
                    {
                        methodOutput = new MethodOutput()
{
                            status_code = (int)MethodOutputStatusCode.Success,
                            message = "Success",
                            data = JsonConvert.SerializeObject(resultContent_2.data)
                        };
                    }
                    else
                    {
                        methodOutput = new MethodOutput()
                        {
                            status_code = (int)MethodOutputStatusCode.Failed,
                            message = "No Data"
                        };
                    }

                }
                else
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Failed,
                        message = (string)resultContent_2.msg,
                    };
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }
        public async Task<MethodOutput> UpdateTrackingDetail(AutomaticPurchaseAmz new_detail, string url, string log, int user_excution = 64, string key = "1372498309AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k")
        {
            MethodOutput methodOutput = new MethodOutput();
            try
            {
                HttpClient httpClient_2 = new HttpClient();
                var apiQueueService = url;
                var input = new
                {
                    data = JsonConvert.SerializeObject(new_detail),
                    user_id = user_excution,
                    log = log
                };
                var token = CommonHelper.Encode(JsonConvert.SerializeObject(input), key);
                var content = new FormUrlEncodedContent(new[]
                   {
                       new KeyValuePair<string, string>("token", token),
                   });
                var result_2 = await httpClient_2.PostAsync(apiQueueService, content);
                dynamic resultContent_2 = Newtonsoft.Json.Linq.JObject.Parse(result_2.Content.ReadAsStringAsync().Result);
                var status = (int)resultContent_2.status;
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
                        message = (string)resultContent_2.msg,
                    };
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public async Task<MethodOutput> UploadImage(string file_path, string us_ex_upload_domain = "https://image.usexpress.vn")
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "Uploaded Failed"
            };
            try
            {
                byte[] imageArray = System.IO.File.ReadAllBytes(file_path);
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                string[] path = file_path.Split(".");
                string base_64_img_full_text = "data:image/" + path[path.Count() - 1] + ";base64," + base64ImageRepresentation;
                string uploaded_url = await UpLoadHelper.UploadBase64Src(base_64_img_full_text, us_ex_upload_domain);
                if (!uploaded_url.StartsWith(us_ex_upload_domain) && !uploaded_url.StartsWith("http"))
                {
                    uploaded_url = us_ex_upload_domain + uploaded_url;
                }
                if (uploaded_url != string.Empty)
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    methodOutput.message = uploaded_url;
                    methodOutput.data = uploaded_url;
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }
    }
}
