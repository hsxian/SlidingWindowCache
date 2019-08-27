using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlidingWindowCache.Core.Cache;

namespace SlidingWindowCache.ConsoleApp
{
    public class DataRequestClient
    {
        private readonly ISlidingWindowCache<long, DataModel> _cache;

        private long _lastPoint;
        public long DiffSize { get; set; }

        public DataRequestClient(ISlidingWindowCache<long, DataModel> cache)
        {
            _cache = cache;
            _cache.OnDataAutoLoaderStatusChanged += CacheDataAutoLoaderStatusChanged;
        }

        private void CacheDataAutoLoaderStatusChanged(object sender, TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Canceled:
                    Console.WriteLine("cache data load task be canceled");
                    break;
                case TaskStatus.RanToCompletion:
                    Console.WriteLine("cache data load task is RanToCompletion");
                    break;
                case TaskStatus.Running:
                    Console.WriteLine("cache data load task is Running");
                    break;
            }
        }

        private async Task<IEnumerable<DataModel>> ReadNextTimeData()
        {
            return await _cache.GetCacheData(_lastPoint, _cache.CurrentPoint, t => t.Point);
        }

        private async Task<IEnumerable<DataModel>> ReadBeforeTimeData()
        {
            return await _cache.GetCacheData(_cache.CurrentPoint, _lastPoint, t => t.Point);
        }

        private ConsoleKey ReadConsoleKey()
        {
            while (Console.KeyAvailable == false)
            {
                return Console.ReadKey(true).Key;
            }

            return Console.ReadKey(true).Key;
        }

        private async Task<IEnumerable<DataModel>> ReadTimeData(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.RightArrow:
                    _cache.CurrentPoint += DiffSize;
                    return await ReadNextTimeData();
                case ConsoleKey.LeftArrow:
                    _cache.CurrentPoint -= DiffSize;
                    return await ReadBeforeTimeData();
                default:
                    return null;
            }
        }

        public async Task DoWork(ConsoleKey key)
        {
            var data = await ReadTimeData(key);
            Console.WriteLine(
                $"{new DateTime(_cache.CurrentPoint):yyyy-MM-dd HH:mm:ss},receive data count:{data?.Count()}");
            _lastPoint = _cache.CurrentPoint;
        }
    }
}