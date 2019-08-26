using System;
using System.Linq;
using SlidingWindowCache.Core.Configuration;
using SlidingWindowCache.Core.Infrastructure;
using Xunit;

namespace SlidingWindowCache.UnitTests.Infrastructure
{
    public class DoubleCacheKeyTests
    {
        private readonly ISlidingWindowConfig<double> _windowConfig;
        private readonly ICacheKeyHelper<double> _keyHelper;

        public DoubleCacheKeyTests()
        {
            _windowConfig = new SlidingWindowConfig<double>
            {
                StartPoint = 9.7,
                EndPoint = 65.3,
                PerLoadSize = 7.411
            };
            _keyHelper = new CacheKeyHelper<double>(_windowConfig);
        }

        [Fact]
        public void DoubleKey()
        {
            Assert.Equal(9.7, _keyHelper.GetKey(11.8));
            Assert.Equal(24.522, _keyHelper.GetKey(25.4));
        }

        [Fact]
        public void DoubleKeys()
        {
            var keys = _keyHelper.GetKeys(10.7, 23.9);
            Assert.Equal(9.7, keys.First(), 3);
            Assert.Equal(17.111, keys.Last(), 3);
        }

        [Fact]
        public void DoubleStartAndEndKey()
        {
            //forward=7.370,backward=10.529
            var (s, e) = _keyHelper.GetStartAndEndKey(36.7, 17.9, 0.7f);
            Assert.Equal(24.522, s,3);
            Assert.Equal(39.344, e,3);
        }
    }
}