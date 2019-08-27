using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlidingWindowCache.Core.Cache;
using SlidingWindowCache.Core.Configuration;
using Xunit;

namespace SlidingWindowCache.UnitTests.Cache
{
    public class SlidingWindowCacheTest
    {
        private readonly ISlidingWindowConfig<double> _config;
        private readonly ISlidingWindowCache<double, double> _cache;

        public SlidingWindowCacheTest()
        {
            _config = new SlidingWindowConfig<double>
            {
                PerLoadSize = 0.61,
                StartPoint = 11.1,
                EndPoint = 689.7,
                TotalLoadSize = 12,
                TotalCacheSize = 65.6
            };

            var rd = new Random();

            _cache = new SlidingWindowCache<double, double>(_config)
            {
                DataSourceDelegate = (a, b, c) =>
                {
                    return Task.FromResult<IEnumerable<double>>(
                        Enumerable.Range(0, (int) (b - a))
                            .Select(t => rd.NextDouble())
                            .ToList()
                    );
                },
                CurrentPoint = _config.StartPoint
            };
        }

        [Fact]
        public void CacheCount()
        {
            Task.Delay(1000).Wait();
            Assert.Equal(6, _cache.Count);
            _cache.CurrentPoint += _config.TotalLoadSize;
            Task.Delay(1000).Wait();
            Assert.Equal(10, _cache.Count);
            _cache.CurrentPoint += _config.TotalLoadSize;
            Task.Delay(1000).Wait();
            Assert.Equal(14, _cache.Count);
            _cache.CurrentPoint += _config.TotalLoadSize;
            Task.Delay(1000).Wait();
            Assert.Equal(8, _cache.Count);
            _cache.CurrentPoint += _config.TotalLoadSize;
            Task.Delay(1000).Wait();
            Assert.Equal(12, _cache.Count);
            _cache.CurrentPoint += _config.TotalLoadSize;
        }
    }
}