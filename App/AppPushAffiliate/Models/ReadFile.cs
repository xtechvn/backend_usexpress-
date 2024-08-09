﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace App_Crawl_SearchList_Push_Worker.Models
{
    public class ReadFile
    {
        public static AppSettings LoadConfig()
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                var result = JsonConvert.DeserializeObject<AppSettings>(json);
                return result;
            }
        }
    }
}
