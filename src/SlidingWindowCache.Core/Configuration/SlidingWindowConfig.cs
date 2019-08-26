using System;

namespace SlidingWindowCache.Core.Configuration
{
    public class SlidingWindowConfig<TKey> : ISlidingWindowConfig<TKey>
    {
        public TKey PerLoadSize { get; set; }
        public float ForwardAndBackwardScale { get; set; }
        public TKey StartPoint { get; set; }
        public TKey EndPoint { get; set; }
        public TKey TotalLoadSize { get; set; }
        public TKey TotalCacheSize { get; set; }
        public float LoadTriggerFrequency { get; set; }
        public float RemoveTriggerFrequency { get; set; }
        public int LoadParallelLimit { get; set; }
        public void VerifyConfigThrowException()
        {
            throw new NotImplementedException();
        }
    }
}