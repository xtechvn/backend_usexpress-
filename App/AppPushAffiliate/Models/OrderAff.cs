using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AppPushAffiliate.Models
{
    public static class OrderAff
    {
        public static string path = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\netcoreapp3.1\", @"\") + "pushed_order.list";
        public static List<string> LoadPushedOrders()
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                else
                {
                    using (StreamReader r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();
                        var result = json.Trim().Split(",").ToList();
                        return result;
                    }
                }
            }
            catch (Exception)
            {

            }
            return new List<string>();
        }
        public static int SavePushedOrders(string item)
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                File.AppendAllText(path, item + ",");
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public static bool IsOrderPushed(string order_id)
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    if (json.Contains(order_id)) return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
