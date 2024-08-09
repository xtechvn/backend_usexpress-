using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities.ConfigModels;
using Utilities.Models;

namespace Utilities
{
    public class UpLoadHelper
    {
        /// <summary>
        /// UploadImageBase64
        /// </summary>
        /// <param name="ImageBase64">src of image</param>
        /// <returns></returns>
        public static async Task<string> UploadImageBase64(ImageBase64 modelImage)
        {
            string ImagePath = string.Empty;
            string tokenData = string.Empty;
            try
            {
                var configApiCms = FileHelpers<ApiCmsConfig>.LoadConfig("config.json");
                var apiPrefix = configApiCms.API_CMS_UPLOAD;
                var key_token_api = configApiCms.KEY_CMS_UPLOAD;
                var j_param = new Dictionary<string, string> {
                    { "data_file", modelImage.ImageData },
                    { "extend", modelImage.ImageExtension }};

                using (HttpClient httpClient = new HttpClient())
                {
                    tokenData = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                    var contentObj = " {\"token\": \"" + tokenData + "\"}";

                    var content = new StringContent(contentObj, Encoding.UTF8, "application/json");
                    var result = await httpClient.PostAsync(apiPrefix, content);
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    if (resultContent.status == "success")
                    {
                        return resultContent.url_path;
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("UploadImageBase64. Result: " + resultContent.status + ". Message: " + resultContent.msg);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UploadImageBase64 - " + ex.Message.ToString() + " Token:" + tokenData);
            }
            return ImagePath;
        }

        public static async Task<string> UploadBase64Src(string ImageSrc, string StaticDomain)
        {
            try
            {
                var objimage = StringHelpers.GetImageSrcBase64Object(ImageSrc);
                if (objimage != null)
                {
                    objimage.ImageData = ResizeBase64Image(objimage.ImageData, out string FileType);
                    if (objimage.ImageData == null || objimage.ImageData.Trim() == "") objimage.ImageData = ImageSrc;
                    if (!string.IsNullOrEmpty(FileType)) objimage.ImageExtension = FileType;

                    return await UploadImageBase64(objimage);
                }
                else
                {
                    if (ImageSrc.StartsWith(StaticDomain))
                        return ImageSrc.Replace(StaticDomain, string.Empty);
                    else
                        return ImageSrc;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UploadImageBase64 - " + ex.Message.ToString());
            }
            return string.Empty;
        }

        /// <summary>
        /// Resize image with maximum 1000px width
        /// </summary>
        /// <param name="ImageBase64"></param>
        /// <returns></returns>
        public static string ResizeBase64Image(string ImageBase64, out string FileType)
        {
            FileType = null;
            try
            {
                var IsValid = StringHelpers.TryGetFromBase64String(ImageBase64, out byte[] ImageByte);
                if (IsValid)
                {
                    using (var memoryStream = new MemoryStream(ImageByte))
                    {
                        var RootImage = Image.FromStream(memoryStream);
                        if (RootImage.Width > 1000)
                        {
                            int width = 1000;
                            int height = (int)(width / ((double)RootImage.Width / RootImage.Height));
                            var ResizeImage = (Image)(new Bitmap(RootImage, new Size(width, height)));
                            using (var stream = new MemoryStream())
                            {
                                ResizeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                ImageByte = stream.ToArray();
                            }
                            FileType = "jpg";
                            return Convert.ToBase64String(ImageByte);
                        }
                        else
                        {
                            return ImageBase64;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
