using System;

namespace SlidingWindowCache.Core.Configuration
{
    public class SlidingWindowConfig<TKey> : ISlidingWindowConfig<TKey>
    {
        public TKey PerLoadSize { get; set; }
        public float ForwardAndBackwardScale { get; set; } = 3;
        public TKey StartPoint { get; set; }
        public TKey EndPoint { get; set; }
        public TKey TotalLoadSize { get; set; }
        public TKey TotalCacheSize { get; set; }
        public float LoadTriggerFrequency { get; set; } = 0.3f;
        public float RemoveTriggerFrequency { get; set; } = 0.3f;
        public int LoadParallelLimit { get; set; } = Environment.ProcessorCount / 2;
        public DataAcquisitionMode AcquisitionMode { get; set; } = DataAcquisitionMode.Smooth;

        public void VerifyConfigThrowException()
        {
        }
    }
}