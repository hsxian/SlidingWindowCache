using System;
using System.Threading.Tasks;
using SlidingWindowCache.Core.Configuration;
using SlidingWindowCache.Core.Cache;

namespace SlidingWindowCache.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var timeConf = new SlidingWindowConfig<long>
            {
                PerLoadSize = new TimeSpan(0, 2, 0).Ticks,
                StartPoint = new DateTime(2019, 1, 1).Ticks,
                EndPoint = new DateTime(2019, 2, 1).Ticks,
                TotalLoadSize = new TimeSpan(0, 30, 0).Ticks,
                TotalCacheSize = new TimeSpan(7, 0, 0).Ticks
            };


            var win = new SlidingWindowCache<long, DataModel>(timeConf)
            {
                DataSourceDelegate = DataModel.Instance.LoadDataFromSource,
                CurrentPoint = timeConf.StartPoint
            };

            var client = new DataRequestClient(win)
            {
                DiffSize = new TimeSpan(0, 1, 0).Ticks
            };
            while (true)
            {
                var key = Console.ReadKey().Key;
                await client.DoWork(key);
            }
        }
    }
}