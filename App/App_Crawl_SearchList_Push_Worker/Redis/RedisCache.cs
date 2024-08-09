using Microsoft.Extensions.Options;

using StackExchange.Redis;
using System;

namespace AppReceiver_Keyword_Analyst.Redis
{
    public class RedisCache
    {
        readonly IDatabase _database;
        readonly string? _keyPrefix;

        public RedisCache(RedisConnectionManager redisConnectionManager, IOptions<RedisOptions> options)
            => (_database, _keyPrefix) = (redisConnectionManager.Connection.GetDatabase(options.Value.DBIndex), options.Value.KeyPrefix);

     
        public void SetString(string key, string value)
            => _database.StringSet((key), value);

        public string GetString(string key)
            => _database.StringGet((key));

        public void SetString(string key, string value, DateTime expires)
        {            
            var expiryTimeSpan = expires.Subtract(DateTime.Now);
            _database.StringSet((key), value, expiryTimeSpan);
        }

       

        string FormatKey(string key) => string.IsNullOrWhiteSpace(_keyPrefix) ? key : $"{_keyPrefix}{key}";
    }
}
