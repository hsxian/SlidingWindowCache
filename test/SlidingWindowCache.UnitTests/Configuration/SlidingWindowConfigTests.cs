using System;
using SlidingWindowCache.Core.Configuration;
using Xunit;

namespace SlidingWindowCache.UnitTests.Configuration
{
    public class SlidingWindowConfigTests
    {
        [Fact]
        public void ErrorConfig()
        {
            var timeConf = new SlidingWindowConfig<long>
            {
                PerLoadSize = 100,
                StartPoint = 10,
                EndPoint = 9,
                TotalLoadSize = 50,
                TotalCacheSize = 0
            };
            Assert.Throws<ArgumentException>(() => timeConf.VerifyConfigThrowException());
        }
    }
}