using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SlidingWindowCache.Core.Models
{
    public class DataSourceTask<TData>
    {
        public Task<IEnumerable<TData>> Loader { get; set; }
        public CancellationTokenSource CancellationSource { get; set; }

        public DataSourceTask(Task<IEnumerable<TData>> task, CancellationTokenSource cancel)
        {
            Loader = task;
            CancellationSource = cancel;
        }
    }
}