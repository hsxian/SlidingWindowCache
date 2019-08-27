using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SlidingWindowCache.Core.Cache
{
    public interface ISlidingWindowCache<TKey, TData>:IDisposable
    {
        TKey CurrentPoint { get; set; }
        Task<IEnumerable<TData>> GetCacheData(TKey start, TKey end, Func<TData, TKey> keyOfTData);
        Func<TKey, TKey, CancellationToken, Task<IEnumerable<TData>>> DataSourceDelegate { get; set; }
        event EventHandler<TaskStatus> OnDataAutoLoaderStatusChanged;
    }
}