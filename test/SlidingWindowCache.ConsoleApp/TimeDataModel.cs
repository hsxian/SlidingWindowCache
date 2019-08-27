using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SlidingWindowCache.ConsoleApp
{
    public class DataModel
    {
        private static readonly Lazy<DataModel> _lazy = new Lazy<DataModel>(() => new DataModel());
        public static DataModel Instance => _lazy.Value;
        public long Point { get; set; }

        public Task<IEnumerable<DataModel>> LoadDataFromSource(long s, long e,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var rd = new Random();
                //模拟远程访问数据时可能的延迟
                Task.Delay(rd.Next(50, 400), cancellationToken).Wait(cancellationToken);
                var diff = (int)(e - s);
                var count = diff > 1000 ? 1000 : diff;
                var result = Enumerable.Range(0, count)
                    .Select(t => new DataModel { Point = s + rd.Next(diff) })
                    .OrderBy(t => t.Point)
                    .ToList();
                return (IEnumerable<DataModel>)result;
            }, cancellationToken);
        }
    }
}