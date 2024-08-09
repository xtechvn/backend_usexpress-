using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities.ConfigModels;

namespace Utilities
{
    public class FileHelpers<TEntity> where TEntity : class
    {
        public static TEntity LoadConfig(string FilePath)
        {
            try
            {
                using (StreamReader r = new StreamReader(FilePath))
                {
                    string json = r.ReadToEnd();
                    return JsonConvert.DeserializeObject<TEntity>(json);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
