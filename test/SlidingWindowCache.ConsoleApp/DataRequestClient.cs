using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlidingWindowCache.Core.Cache;

namespace SlidingWindowCache.ConsoleApp
{
    public class DataRequestClient
    {
        private readonly ISlidingWindowCache<long, DataModel> _timeCache;

        private long _lastPoint;
        public long DiffSize { get; set; }

        public DataRequestClient(ISlidingWindowCache<long, DataModel> timeCache)
        {
            _timeCache = timeCache;
            _timeCache.OnDataAutoLoaderStatusChanged += TimeCache_DataAutoLoaderStatusChanged;
        }

        private void TimeCache_DataAutoLoaderStatusChanged(object sender, TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Canceled:
                    Console.WriteLine("time cache data load task be canceled");
                    break;
                case TaskStatus.RanToCompletion:
                    Console.WriteLine("time cache data load task is RanToCompletion");
                    break;
                case TaskStatus.Running:
                    Console.WriteLine("time cache data load task is Running");
                    break;
            }
        }

        private async Task<IEnumerable<DataModel>> ReadNextTimeData()
        {
            return await _timeCache.GetCacheData(_lastPoint, _timeCache.CurrentPoint, t => t.Point);
        }

        private async Task<IEnumerable<DataModel>> ReadBeforeTimeData()
        {
            return await _timeCache.GetCacheData(_timeCache.CurrentPoint, _lastPoint, t => t.Point);
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
                    _timeCache.CurrentPoint += DiffSize;
                    return await ReadNextTimeData();
                case ConsoleKey.LeftArrow:
                    _timeCache.CurrentPoint -= DiffSize;
                    return await ReadBeforeTimeData();
                default:
                    return null;
            }
        }

        public async Task DoWork(ConsoleKey key)
        {
            var data = await ReadTimeData(key);
            Console.WriteLine(
                $"{new DateTime(_timeCache.CurrentPoint):yyyy-MM-dd HH:mm:ss},receive data count:{data?.Count()}");
            _lastPoint = _timeCache.CurrentPoint;
        }
    }
}