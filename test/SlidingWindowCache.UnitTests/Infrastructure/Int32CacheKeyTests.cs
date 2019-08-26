using System;
using SlidingWindowCache.Core.Configuration;
using SlidingWindowCache.Core.Infrastructure;
using Xunit;

namespace SlidingWindowCache.UnitTests.Infrastructure
{
    public class Int32CacheKeyTests
    {
        private readonly ISlidingWindowConfig<int> _windowConfig;
        private readonly ICacheKeyHelper<int> _keyHelper;

        public Int32CacheKeyTests()
        {
            _windowConfig = new SlidingWindowConfig<int>
            {
                StartPoint = 9,
                EndPoint = 65,
                PerLoadSize = 7
            };
            _keyHelper = new CacheKeyHelper<int>(_windowConfig);
        }

        [Fact]
        public void Int32Key()
        {
            Assert.Equal(9, _keyHelper.GetKey(11));
            Assert.Equal(23, _keyHelper.GetKey(25));
        }

        [Fact]
        public void Int32Keys()
        {
            var keys = _keyHelper.GetKeys(10, 23);
            Assert.Contains(9, keys);
            Assert.Contains(16, keys);
            Assert.DoesNotContain(23,keys);
            
            keys = _keyHelper.GetKeys(10, 24);
            Assert.Contains(23, keys);
        }

        [Fact]
        public void Int32StartAndEndKey()
        {
            //forward=7,backward=10
            var (s, e) = _keyHelper.GetStartAndEndKey(36, 17, 0.7f);
            Assert.Equal(23, s);
            Assert.Equal(37, e);
        }
    }
}