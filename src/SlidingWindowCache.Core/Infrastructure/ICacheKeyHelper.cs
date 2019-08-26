using System;
using System.Collections.Generic;

namespace SlidingWindowCache.Core.Infrastructure
{
    public interface ICacheKeyHelper<TKey>
    {
        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        TKey GetKey(TKey point);


        /// <summary>
        /// 获取区间中的所有时间key
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        IEnumerable<TKey> GetKeys(TKey start, TKey end);

        /// <summary>
        /// 获取起止key
        /// </summary>
        /// <param name="point"></param>
        /// <param name="span"></param>
        /// <param name="fbScale"></param>
        /// <returns></returns>
        Tuple<TKey, TKey> GetStartAndEndKey(TKey point, TKey span, float fbScale);

        /// <summary>
        /// 获取数据载入的起止key
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        Tuple<TKey, TKey> GetLoadStartAndEndKey(TKey point);

        /// <summary>
        /// 获取数据缓存的起止key
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        Tuple<TKey, TKey> GetCacheStartAndEndKey(TKey point);
    }
}