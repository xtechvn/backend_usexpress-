using System;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace AppReceiver_Keyword_Analyst.Redis
{
    public class RedisConnectionManager
    {

        readonly Lazy<IConnectionMultiplexer> _connectionMultiplexer;

        public IConnectionMultiplexer Connection => _connectionMultiplexer.Value;

        public RedisConnectionManager(IOptions<RedisOptions> options)
            => _connectionMultiplexer = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options.Value.ConfigurationOptions));

    }
}
