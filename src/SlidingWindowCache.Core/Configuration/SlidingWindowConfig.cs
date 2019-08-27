using System;
using SlidingWindowCache.Core.Infrastructure.Arithmetic;

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
            var ccf = CalculatorFactory.Build<TKey>();

            if (ccf.Gt(PerLoadSize, TotalLoadSize))
                throw new ArgumentException("expect: PerLoadSize < TotalLoadSize");
            if (ccf.Gt(TotalLoadSize, TotalCacheSize))
                throw new ArgumentException("expect: TotalLoadSize < TotalCacheSize");
            if (ccf.Gt(TotalCacheSize, ccf.Subtract(EndPoint, StartPoint)))
                throw new ArgumentException("expect: TotalCacheSize < (EndPoint - StartPoint)");

            if (ForwardAndBackwardScale < 0)
                throw new ArgumentException("expect: ForwardAndBackwardScale > 0");

            if (LoadTriggerFrequency < 0 || LoadTriggerFrequency > 1)
                throw new ArgumentException("expect: 0<= LoadTriggerFrequency <= 1");
            if (RemoveTriggerFrequency < 0 || RemoveTriggerFrequency > 1)
                throw new ArgumentException("expect: 0<= RemoveTriggerFrequency <= 1");
        }
    }
}