using System;
using System.Collections.Generic;
using SlidingWindowCache.Core.Configuration;
using SlidingWindowCache.Core.Infrastructure.Arithmetic;

namespace SlidingWindowCache.Core.Infrastructure
{
    public class CacheKeyHelper<TKey> : ICacheKeyHelper<TKey>
    {
        private readonly ISlidingWindowConfig<TKey> _config;
        private readonly ICalculator<TKey> _calculator;

        public CacheKeyHelper(ISlidingWindowConfig<TKey> config)
        {
            _config = config;
            _calculator = CalculatorFactory.Build<TKey>();
        }

        public TKey GetKey(TKey point)
        {
            var key = _config.StartPoint;
            var count = _calculator.Divide(_calculator.Subtract(point, _config.StartPoint), _config.PerLoadSize);
            count = _calculator.Floor(count);
            key = _calculator.Add(_config.StartPoint, _calculator.Multiply(count, _config.PerLoadSize));
            return key;
        }

        public IEnumerable<TKey> GetKeys(TKey start, TKey end)
        {
            var keys = new List<TKey>();
            var currT = GetKey(start);
            while (_calculator.Lt(currT, end))
            {
                keys.Add(currT);
                currT = _calculator.Add(currT, _config.PerLoadSize);
            }

            return keys;
        }

        public Tuple<TKey, TKey> GetStartAndEndKey(TKey point, TKey span, float fbScale)
        {
            var backward = _calculator.DivideSingle(span, fbScale + 1);
            var forward = _calculator.Subtract(span, backward);
            var sT = _calculator.Subtract(point, backward);
            sT = GetKey(sT);
            var eT = _calculator.Add(point, forward);
            eT = GetKey(eT);
            return new Tuple<TKey, TKey>(sT, eT);
        }

        public Tuple<TKey, TKey> GetLoadStartAndEndKey(TKey point)
        {
            var (sT, eT) = GetStartAndEndKey(point, _config.TotalLoadSize, _config.ForwardAndBackwardScale);
            sT = _calculator.Lt(sT, _config.StartPoint) ? GetKey(_config.StartPoint) : sT;
            eT = _calculator.Gt(eT, _config.EndPoint) ? GetKey(_config.EndPoint) : eT;
            return new Tuple<TKey, TKey>(sT, eT);
        }

        public Tuple<TKey, TKey> GetCacheStartAndEndKey(TKey point)
        {
            var (sT, eT) = GetStartAndEndKey(point, _config.TotalCacheSize, _config.ForwardAndBackwardScale);
            sT = _calculator.Lt(sT, _config.StartPoint) ? GetKey(_config.StartPoint) : sT;
            eT = _calculator.Gt(eT, _config.EndPoint) ? GetKey(_config.EndPoint) : eT;
            return new Tuple<TKey, TKey>(sT, eT);
        }
    }
}