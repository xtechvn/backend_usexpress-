using System;
using System.Configuration;
using System.IO;
using System.Linq;
using ServiceReceiverMedia.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServiceReceiverMedia.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        // GET: MediaController
        [Route("index")]
        public string Index()
        {
            return "Index View";
        }
        [HttpPost("upload")]
        public IActionResult Upload([FromBody] object body)
        {
            try
            {
                var Jobject = JObject.Parse(body.ToString());
                string token = Jobject["token"].ToString();

                //Array kiểm tra nếu file name là file ảnh thông thường.
                string[] stringArray = {"PNG", "JPG", "JPEG", "GIF", "BMP"};
                //Lấy thông tin từ file .config:
                string imgPath_base = @ConfigurationManager.AppSettings["img_root_folder_path"];
                string imgPath_url_base = @ConfigurationManager.AppSettings["img_url_base"];
                //Max Size file có thể upload: mặc định là 3MB
                int max_file_size = 3 * 1024 * 1024;

                try
                {
                    if (Convert.ToInt32(@ConfigurationManager.AppSettings["max_file_size_in_byte"]) > 0)
                    {
                        max_file_size = Convert.ToInt32(@ConfigurationManager.AppSettings["max_file_size_in_byte"]);
                    }
                }
                catch (FormatException)
                {

                }

                //Decode token để lấy JSON:
                string param = Decode(token, ConfigurationManager.AppSettings["token_encode_key"]);

                //Model hóa JSON:
                ImageDetail img_detail = JsonConvert.DeserializeObject<ImageDetail>(param);

                //// Nếu extend thuộc ảnh
                //if (!stringArray.Contains(img_detail.extend.ToUpper()))
                //{
                //    //Trả kết quả sai file type:
                //    return BadRequest(new { status = "error", message = "Invalid File Type", url_path = "" });
                //}

                //Nếu lấy ra được thông tin:
                if (IsBase64String(img_detail.data_file.Split(",")[1]))
                {
                    //Lấy thông tin thời gian hiện tại
                    DateTime time = DateTime.Now;
                    string year = time.Year.ToString();
                    string month = time.Month.ToString();
                    string day = time.Day.ToString();
                    // string hour = time.Hour.ToString();

                    //Thông tin file và build đường dẫn local:
                    string file_name = Guid.NewGuid() + "." + img_detail.extend;
                    string imgPath_year = @"\" + year + @"\";
                    string imgPath_month = imgPath_year + month + @"\";
                    string imgPath_day = imgPath_month + day + @"\";

                    //Nếu folder trống,tạo mới, nếu file exsist, thêm _[i] vào sau tên file
                    if (!Directory.Exists(imgPath_base))
                    {
                        Directory.CreateDirectory(imgPath_base);
                    }

                    if (!Directory.Exists(imgPath_base + imgPath_year))
                    {
                        Directory.CreateDirectory(imgPath_base + imgPath_year);
                    }

                    if (!Directory.Exists(imgPath_base + imgPath_month))
                    {
                        Directory.CreateDirectory(imgPath_base + imgPath_month);
                    }

                    if (!Directory.Exists(imgPath_base + imgPath_day))
                    {
                        Directory.CreateDirectory(imgPath_base + imgPath_day);
                    }


                    string imgPath_full = (imgPath_base + imgPath_day + file_name);
                    byte[] bytes = System.Convert.FromBase64String(img_detail.data_file.Split(",")[1]);
                    //Kiểm tra nếu file vượt quá max size:
                    if (bytes.Length > max_file_size)
                    {
                        return BadRequest(new { status = "error", message = "The file image exceeds the maximum allowed size: " + max_file_size + " bytes", file_path = file_name, url_path = "" });
                    }
                    else
                    {
                        //Ghi byte[] vào file đã tạo:
                        using (var fs = new FileStream(imgPath_full, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    //Build đường link local:
                    string urlPath_full = imgPath_url_base + imgPath_day + file_name;

                    //Trả kết quả
                    return Ok(new { status = "success", message = "Images Received", url_path = urlPath_full.Replace(@"\", @"/") });
                }
                //Thông tin không được encode với key trong file .config hoặc thông tin convert ra null:
                else
                {
                    return BadRequest(new { status = "error", message = "Invalid Format", url_path = "" });
                }
            }
            catch (Exception e)
            {
                //Log Telegram:
                //LogHelper.InsertLogTelegram("ServiceReceiverMedia - Upload Error: " + e.ToString());

                //Lỗi trên API
                return BadRequest(new { status = "error", message = "On Execution", url_path = "" });
            }
        }
        [HttpPost("upload-payment")]
        public IActionResult UploadPaymentImage([FromBody] object body)
        {
            try
            {
                var Jobject = JObject.Parse(body.ToString());
                string token = Jobject["token"].ToString();

                //Array kiểm tra nếu file name là file ảnh thông thường.
                string[] stringArray = { "PNG", "JPG", "JPEG", "GIF", "BMP" };
                //Lấy thông tin từ file .config:
                string imgPath_base = @ConfigurationManager.AppSettings["img_root_folder_path"];
                string imgPath_url_base = @ConfigurationManager.AppSettings["payment_img_url_base"];
                //Max Size file có thể upload: mặc định là 6MB
                int max_file_size = 6 * 1024 * 1024;

                try
                {
                    if (Convert.ToInt32(@ConfigurationManager.AppSettings["max_file_size_in_byte"]) > 0)
                    {
                        max_file_size = Convert.ToInt32(@ConfigurationManager.AppSettings["max_file_size_in_byte"]);
                    }
                }
                catch (FormatException)
                {

                }

                //Decode token để lấy JSON:
                string param = Decode(token, ConfigurationManager.AppSettings["token_encode_key"]);

                //Model hóa JSON:
                PaymentImageDetail img_detail = JsonConvert.DeserializeObject<PaymentImageDetail>(param);

                // Nếu extend thuộc ảnh
                if (!stringArray.Contains(img_detail.extend.ToUpper()))
                {
                    //Trả kết quả sai file type:
                    return BadRequest(new { status = "error", message = "Invalid File Type", url_path = "" });
                }

                //Nếu lấy ra được thông tin:
                if (IsBase64String(img_detail.data_file))
                {
                    //Lấy thông tin thời gian hiện tại
                    DateTime time = DateTime.Now;
                    string year = time.Year.ToString();
                    string month = time.Month.ToString();
                    string day = time.Day.ToString();
                    // string hour = time.Hour.ToString();

                    //Thông tin file và build đường dẫn local:
                    string file_name = Guid.NewGuid() + "." + img_detail.extend;
                    string imgPath_year = @"\" + year + @"\";
                    string imgPath_month = imgPath_year + month + @"\";
                    string imgPath_day = imgPath_month + day + @"\";

                    //Nếu folder trống,tạo mới, nếu file exsist, thêm _[i] vào sau tên file
                    if (!Directory.Exists(imgPath_base))
                    {
                        Directory.CreateDirectory(imgPath_base);
                    }

                    if (!Directory.Exists(imgPath_base + imgPath_year))
                    {
                        Directory.CreateDirectory(imgPath_base + imgPath_year);
                    }

                    if (!Directory.Exists(imgPath_base + imgPath_month))
                    {
                        Directory.CreateDirectory(imgPath_base + imgPath_month);
                    }

                    if (!Directory.Exists(imgPath_base + imgPath_day))
                    {
                        Directory.CreateDirectory(imgPath_base + imgPath_day);
                    }

                    #region Nếu file đã tồn tại(không cần thiết)
                    //if (System.IO.File.Exists(imgPath_base + imgPath_day + file_name))
                    //{
                    //    int i = 0;
                    //    string[] a = file_name.Split(".");
                    //    string file_extend = a[a.Length - 1];
                    //    // string file_name_withoutExtend = img_detail.data_file.Replace("/[^a-zA-Z0-9]/g", "").Substring(0,6);
                    //    string file_name_withoutExtend = year + month + day + hour;
                    //    do
                    //    {
                    //        i++;
                    //        //Build file_name với _[i] và kiểm tra trùng lặp
                    //        file_name = file_name_withoutExtend + "_" + i + "." + file_extend;
                    //    }
                    //    while (System.IO.File.Exists(imgPath_base + imgPath_day + file_name));
                    //}
                    #endregion

                    string imgPath_full = (imgPath_base + imgPath_day + file_name);
                    byte[] bytes = System.Convert.FromBase64String(img_detail.data_file);
                    //Kiểm tra nếu file vượt quá max size:
                    if (bytes.Length > max_file_size)
                    {
                        return BadRequest(new { status = "error", message = "The file image exceeds the maximum allowed size: " + max_file_size + " bytes", file_path = file_name, url_path = "" });
                    }
                    else
                    {
                        //Ghi byte[] vào file đã tạo:
                        using (var fs = new FileStream(imgPath_full, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }

                    //Build đường link local:
                    string urlPath_full = imgPath_url_base + imgPath_day + file_name;

                    //Trả kết quả
                    return Ok(new { status = "success", message = "Images Received", url_path = urlPath_full.Replace(@"\", @"/") });
                }
                //Thông tin không được encode với key trong file .config hoặc thông tin convert ra null:
                else
                {
                    return BadRequest(new { status = "error", message = "Invalid Format", url_path = "" });
                }
            }
            catch (Exception e)
            {
                //Log Telegram:
               // LogHelper.InsertLogTelegram("ServiceReceiverMedia - Upload Error: " + e.ToString());

                //Lỗi trên API
                return BadRequest(new { status = "error", message = "On Execution", url_path = "" });
            }
        }

        public static bool IsBase64String(string s)
        {
            try
            {
                byte[] bytes = System.Convert.FromBase64String(s);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static string Decode(string strString, string strKeyPhrase)
        {
            try
            {
                Byte[] byt = Convert.FromBase64String(strString);
                strString = System.Text.Encoding.UTF8.GetString(byt);
                strString = KeyED(strString, strKeyPhrase);
                return strString;
            }
            catch (Exception ex)
            {

                return strString;
            }
        }
        public static string Encode(string strString, string strKeyPhrase)
        {

            strString = KeyED(strString, strKeyPhrase);
            Byte[] byt = System.Text.Encoding.UTF8.GetBytes(strString);
            strString = Convert.ToBase64String(byt);
            return strString;
        }
        private static string KeyED(string strString, string strKeyphrase)
        {
            int strStringLength = strString.Length;
            int strKeyPhraseLength = strKeyphrase.Length;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(strString);

            for (int i = 0; i < strStringLength; i++)
            {
                int pos = i % strKeyPhraseLength;
                int xorCurrPos = (int)(strString[i]) ^ (int)(strKeyphrase[pos]);
                builder[i] = Convert.ToChar(xorCurrPos);
            }

            return builder.ToString();
        }
    }
}
