using App_AutomaticPurchase_AMZ.Model;
using App_Crawl_TrackingFlightPackages.Model;
using Entities.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace App_AutomaticPurchase_AMZ.Repositories
{
    public class UsExAPIRepository : IUsExAPI
    {
       

        public async Task<MethodOutput> UploadImage(string file_path,string us_ex_upload_domain= "https://image.usexpress.vn")
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message="Uploaded Failed"
            };
            try
            {
                byte[] imageArray = System.IO.File.ReadAllBytes(file_path);
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                string[] path = file_path.Split(".");
                string base_64_img_full_text = "data:image/" + path[path.Count() - 1] + ";base64," + base64ImageRepresentation;
                string uploaded_url = await UpLoadHelper.UploadBase64Src(base_64_img_full_text, us_ex_upload_domain);
                if(!uploaded_url.StartsWith(us_ex_upload_domain) && !uploaded_url.StartsWith("http")){
                    uploaded_url = us_ex_upload_domain + uploaded_url;
                }
                if(uploaded_url != string.Empty)
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
