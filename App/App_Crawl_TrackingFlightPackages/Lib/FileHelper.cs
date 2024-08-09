using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;

namespace App_Crawl_TrackingFlightPackages.Lib
{
    public static class FileHelper
    {
        public static bool WriteLogToFile(string log, string order_code, string product_code)
        {
            try
            {
                string app_path = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\netcoreapp3.1", "");
                if (!Directory.Exists(app_path))
                {
                    Directory.CreateDirectory(app_path);
                }
                if (!File.Exists(app_path + @"\log.txt"))
                {
                    File.Create(app_path + @"\log.txt");
                }
                File.AppendAllText(app_path + @"\log.txt", order_code + " - " + product_code + " - " + log.Trim()+"\n");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool WriteOrderExcutedToFile(string order_code, string product_code,double amount,int quanity)
        {
            try
            {
                string app_path = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\netcoreapp3.1", "");
                if (!Directory.Exists(app_path))
                {
                    Directory.CreateDirectory(app_path);
                }
                string path_excuted = app_path + @"\excuted.list";
                if (!File.Exists(path_excuted))
                {
                    File.Create(path_excuted);
                }
                File.AppendAllText(path_excuted, order_code + " - " + product_code + " - " + amount + " - " + quanity + " - " + DateTime.Now +" \n ");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - FileHelper - WriteOrderExcutedToFile: " + ex.ToString());

                return false;
            }
        }
        
        public static int CheckIfExcuted(string order_code,string product_code,double amount,int quanity)
        {
            try
            {
                string app_path = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\netcoreapp3.1", "");
                if (!Directory.Exists(app_path))
                {
                    Directory.CreateDirectory(app_path);
                }
                string path_excuted = app_path + @"\excuted.list";
                string line_check = order_code + " - " + product_code + " - " + amount + " - " + quanity;
                foreach (string line in File.ReadLines(path_excuted))
                {
                    if (line.Contains(line_check))
                    {
                        return 0;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - FileHelper - CheckIfExcuted: " + ex.ToString());
                return 2;
            }
        }
    }
}
