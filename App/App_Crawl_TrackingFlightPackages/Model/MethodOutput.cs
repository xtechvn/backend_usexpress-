using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_Crawl_TrackingFlightPackages.Model
{
    public class MethodOutput
    {
        public int status_code { get; set; }
        public string message { get; set; } = "";
        public dynamic? data { get; set; }
        public  string error_img_path { get; set; } = string.Empty;
    }
}
