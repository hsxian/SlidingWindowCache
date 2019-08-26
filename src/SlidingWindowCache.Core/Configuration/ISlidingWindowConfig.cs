namespace SlidingWindowCache.Core.Configuration
{
    public interface ISlidingWindowConfig<TKey>
    {
        /// <summary>
        /// 每次缓存的大小
        /// </summary>
        TKey PerLoadSize { get; set; }

        /// <summary>
        /// 前部与后部缓存的比例
        /// </summary>
        float ForwardAndBackwardScale { get; set; }

        /// <summary>
        /// 起点
        /// </summary>
        TKey StartPoint { get; set; }

        /// <summary>
        /// 终点
        /// </summary>
        TKey EndPoint { get; set; }


        /// <summary>
        /// 数据载入跨度
        /// </summary>
        TKey TotalLoadSize { get; set; }

        /// <summary>
        /// 数据缓存跨度
        /// </summary>
        TKey TotalCacheSize { get; set; }

        /// <summary>
        /// 数据加载的触发频率(0.0-1.0。为1时，auto loader会一直运行)
        /// </summary>
        /// <value></value>
        float LoadTriggerFrequency { get; set; }

        /// <summary>
        /// 数据移除的触发频率(0.0-1.0)
        /// </summary>
        /// <value></value>
        float RemoveTriggerFrequency { get; set; }

        /// <summary>
        /// 数据加载任务的并发量
        /// </summary>
        int LoadParallelLimit { get; set; }


        /// <summary>
        /// 用于验证配置，错误则抛出异常
        /// </summary>
        void VerifyConfigThrowException();
    }
}