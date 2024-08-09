
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;


namespace Payoo.Lib
{
    public class CallAPI
    {
        private readonly IConfiguration configuration;
        public CallAPI(IConfiguration _configuration)
        {
            configuration = _configuration;
        }


        public string Caller(string strAPIName, string strRequestData)
        {
            try
            {
                //System.Net.ServicePointManager.CertificatePolicy = new CertPolicy();
                var client = new RestClient(configuration["payoo:UrlPayooAPI"] + "/" + strAPIName);
                var request = new RestRequest(Method.POST);

                #region set header request
                request.AddHeader("apiusername", configuration["payoo:APIUsername"]);
                request.AddHeader("apipassword", configuration["payoo:APIPassword"]);
                request.AddHeader("apisignature", configuration["payoo:APISignature"]);
                request.AddHeader("content-type", "application/json");
                #endregion

                #region build and sign data request
                APIRequest objAPIRequest = new APIRequest();
                objAPIRequest.RequestData = strRequestData;
                // sign data
                objAPIRequest.Signature = GenerateCheckSum(configuration["ChecksumKey"] + objAPIRequest.RequestData);
                #endregion

                // set body request
               // JavaScriptSerializer objJson = new JavaScriptSerializer();
                request.AddParameter("application/json", JsonConvert.SerializeObject(objAPIRequest), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    APIResponse objAPIResponse = JsonConvert.DeserializeObject<APIResponse>(response.Content);
                    if (VerifyCheckSum(configuration["payoo:ChecksumKey"] + objAPIResponse.ResponseData, objAPIResponse.Signature))
                    {
                        return objAPIResponse.ResponseData;
                    }
                    else
                    {
                        throw new Exception("Invalid signature!");
                    }

                }
                else
                {
                    throw new Exception("response.StatusCode: " + response.StatusCode + "</br>" + response.ErrorMessage);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string GenerateCheckSum(string input)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (SHA512 hash = System.Security.Cryptography.SHA512.Create())
            {
                byte[] hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                StringBuilder hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (byte b in hashedInputBytes)
                {
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                }
                return hashedInputStringBuilder.ToString();
            }
        }

        public bool VerifyCheckSum(string Data, string CheckSum)
        {
            try
            {
                if (string.IsNullOrEmpty(Data) ||
                   string.IsNullOrEmpty(CheckSum))
                {
                    return false;
                }
                return CheckSum.Equals(GenerateCheckSum(Data), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    //class CertPolicy : ICertificatePolicy
    //{
    //    public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
    //    {
    //        return true;
    //    }
    //}
}