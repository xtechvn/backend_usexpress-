using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace AppReceiverAnalysCrawler.Common
{
    public class RequestData
    {
        public string token { get; set; }
        public string url_api { get; set; }
        public RequestData(string _token, string _url_api)
        {
            url_api = _url_api;
            token = _token;
        }

        public string CreateHttpRequest()
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                string responseFromServer = string.Empty;
                string status = string.Empty;

                string _post = "token=" + token;
                byte[] byteArray = Encoding.UTF8.GetBytes(_post);
                WebRequest request = WebRequest.Create(url_api);
                request.Timeout = 15000;
                request.Method = "POST";
                request.ContentLength = byteArray.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response =  request.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();

                sw.Stop();
                return responseFromServer;


            }
            catch (Exception ex)
            {
                sw.Stop();
                LogHelper.InsertLogTelegram("CreateHttpRequest url_api= " + url_api + ", token=" + token + " error= " + ex.ToString() + " execute_time = " + sw.ElapsedMilliseconds + " ms");
                return string.Empty;
            }
        }
    }
}
