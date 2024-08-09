using System;
using System.Collections.Generic;
using System.Text;

namespace AppReceiverAnalysCrawler.Redis
{
    public class RedisService
    {
        readonly RedisCache _cache;
        
        public RedisService(RedisCache redisCache)
            => _cache = redisCache;
      
        public void SetCacheRedis(string key, string value, DateTime expires)
            => _cache.SetString(key, value, expires);
        public string GetCacheRedis(string key)
        { 
            var result = _cache.GetString(key);            
            return result;
        }
        public bool removeCache(string key)
        {
             _cache.clear(key);
            return true;
        }


    }
}
