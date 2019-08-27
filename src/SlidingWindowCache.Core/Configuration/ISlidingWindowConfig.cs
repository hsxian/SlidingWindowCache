namespace SlidingWindowCache.Core.Configuration
{
    public interface ISlidingWindowConfig<TKey>
    {       
        TKey StartPoint { get; set; }
        TKey EndPoint { get; set; } 
        TKey PerLoadSize { get; set; }      
        TKey TotalLoadSize { get; set; }
        TKey TotalCacheSize { get; set; }
        int LoadParallelLimit { get; set; }
        float LoadTriggerFrequency { get; set; }
        float RemoveTriggerFrequency { get; set; }
        float ForwardAndBackwardScale { get; set; }      
        DataAcquisitionMode AcquisitionMode{ get; set; }
        void VerifyConfigThrowException();
    }
}