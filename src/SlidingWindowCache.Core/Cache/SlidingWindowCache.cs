using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using SlidingWindowCache.Core.Configuration;
using SlidingWindowCache.Core.Infrastructure;
using SlidingWindowCache.Core.Infrastructure.Arithmetic;
using SlidingWindowCache.Core.Models;

namespace SlidingWindowCache.Core.Cache
{
    public class SlidingWindowCache<TKey, TData> : ISlidingWindowCache<TKey, TData>
    {
        private TKey _currentPoint;
        private TKey _lastAutoLoadDataPoint;
        private TKey _lastCleanDataPoint;
        private TaskStatus _dataLoaderStatus;
        private readonly BackgroundWorker _autoLoadBW;
        private readonly System.Timers.Timer _monitorTimer;
        private readonly ICacheKeyHelper<TKey> _keyHelper;
        private readonly ICalculator<TKey> _calculator;
        private readonly ISlidingWindowConfig<TKey> _config;
        private readonly ConcurrentDictionary<TKey, IEnumerable<TData>> _dataDict;
        private readonly ConcurrentDictionary<TKey, DataSourceTask<TData>> _taskDict;

        public event EventHandler<TaskStatus> OnDataAutoLoaderStatusChanged;
        public Func<TKey, TKey, CancellationToken, Task<IEnumerable<TData>>> DataSourceDelegate { get; set; }


        public TKey CurrentPoint
        {
            get => _currentPoint;
            set
            {
                if (_calculator.Lt(value, _config.StartPoint))
                {
                    _currentPoint = _config.StartPoint;
                    return;
                }

                if (_calculator.Gt(value, _config.EndPoint))
                {
                    _currentPoint = _config.EndPoint;
                    return;
                }

                _currentPoint = value;
                CurrentPointChanged();
            }
        }

        private TaskStatus DataLoaderStatus
        {
            get => _dataLoaderStatus;
            set
            {
                if (_dataLoaderStatus == value) return;
                _dataLoaderStatus = value;
                OnDataAutoLoaderStatusChanged?.Invoke(_autoLoadBW, _dataLoaderStatus);
            }
        }

        private void CurrentPointChanged()
        {
            _monitorTimer.Start();
        }

        public SlidingWindowCache(ISlidingWindowConfig<TKey> config)
        {
            _config = config;

            _config.VerifyConfigThrowException();

            _keyHelper = new CacheKeyHelper<TKey>(_config);
            _calculator = CalculatorFactory.Build<TKey>();
            _lastAutoLoadDataPoint = _calculator.MinValue;
            _lastCleanDataPoint = _calculator.MinValue;
            _dataDict = new ConcurrentDictionary<TKey, IEnumerable<TData>>();
            _taskDict = new ConcurrentDictionary<TKey, DataSourceTask<TData>>();
            _autoLoadBW = new BackgroundWorker { WorkerSupportsCancellation = true };
            _autoLoadBW.DoWork += AutoLoadBW_DoWork;
            _monitorTimer = new System.Timers.Timer(500);
            _monitorTimer.Elapsed += MonitorTimer_ElapsedEvent;
        }

        private bool CanAutoLoadData()
        {
            var (sT, eT) = _keyHelper.GetLoadStartAndEndKey(CurrentPoint);
            var scale = 1 - _config.LoadTriggerFrequency;
            var forward = _calculator.MultiplySingle(_calculator.Subtract(eT, CurrentPoint), scale);
            var backward = _calculator.MultiplySingle(_calculator.Subtract(sT, CurrentPoint), scale);
            var diff = _calculator.Subtract(CurrentPoint, _lastAutoLoadDataPoint);
            return _calculator.Ge(diff, _calculator.Zero)
                ? _calculator.Ge(diff, forward)
                : _calculator.Le(diff, backward);
        }

        private bool CanCleanData()
        {
            var (sT, eT) = _keyHelper.GetCacheStartAndEndKey(CurrentPoint);
            var scale = 1 - _config.RemoveTriggerFrequency;
            var forward = _calculator.MultiplySingle(_calculator.Subtract(eT, CurrentPoint), scale);
            var backward = _calculator.MultiplySingle(_calculator.Subtract(sT, CurrentPoint), scale);
            var diff = _calculator.Subtract(CurrentPoint, _lastCleanDataPoint);
            return _calculator.Ge(diff, _calculator.Zero)
                ? _calculator.Ge(diff, forward)
                : _calculator.Le(diff, backward);
        }

        private bool CancelAutoLoadData()
        {
            var result = false;
            var (sT, eT) = _keyHelper.GetLoadStartAndEndKey(_lastAutoLoadDataPoint);
            if (_dataDict.ContainsKey(_calculator.Add(sT, _config.PerLoadSize)) &&
                _dataDict.ContainsKey(_calculator.Subtract(eT, _config.PerLoadSize)))
                result = true;
            var (sl, el) = _keyHelper.GetCacheStartAndEndKey(CurrentPoint);
            if (_calculator.Lt(sT, sl) || _calculator.Gt(eT, el))
                result = true;
            return result;
        }

        private void MonitorTimer_ElapsedEvent(object sender, ElapsedEventArgs e)
        {
            SetValueOfLastMember();

            if (_calculator.Eq(_lastCleanDataPoint, _calculator.MinValue) ||
                _calculator.Eq(_lastAutoLoadDataPoint, _calculator.MinValue))
            {
                return;
            }

            if (_autoLoadBW.IsBusy == false && CanAutoLoadData())
            {
                _autoLoadBW.RunWorkerAsync(CurrentPoint);
            }

            if (CanCleanData())
            {
                RemoveTimeData();
            }

            RemoveDataLoadTask();

            MonitorDataLoadTimer();

            if (_autoLoadBW.CancellationPending)
            {
                DataLoaderStatus = TaskStatus.Canceled;
            }
        }

        private void AutoLoadBW_DoWork(object sender, DoWorkEventArgs e)
        {
            DataLoaderStatus = TaskStatus.Running;
            _lastAutoLoadDataPoint = _keyHelper.GetKey((TKey)e.Argument);
            var (sT, eT) = _keyHelper.GetLoadStartAndEndKey(_lastAutoLoadDataPoint);
            while (_autoLoadBW.CancellationPending == false)
            {
                var curT = _lastAutoLoadDataPoint;
                while (_calculator.Lt(curT, eT))
                {
                    if (_dataDict.ContainsKey(curT) == false)
                        LoadDataFromProxy(curT);

                    curT = _calculator.Add(curT, _config.PerLoadSize);
                }

                curT = _lastAutoLoadDataPoint;
                while (_calculator.Gt(curT, sT))
                {
                    if (_dataDict.ContainsKey(curT) == false)
                        LoadDataFromProxy(curT);

                    curT = _calculator.Subtract(curT, _config.PerLoadSize);
                }

                if (CancelAutoLoadData())
                    break;
            }
        }

        private void MonitorDataLoadTimer()
        {
            var (sT, eT) = _keyHelper.GetLoadStartAndEndKey(_lastAutoLoadDataPoint);
            eT = _calculator.Subtract(eT, _config.PerLoadSize);
            var curT = sT;
            while (_calculator.Lt(curT, eT))
            {
                curT = _calculator.Add(curT, _config.PerLoadSize);

                if (_dataDict.TryGetValue(curT, out var data) == false || data == null)
                {
                    if (_autoLoadBW.IsBusy == false)
                    {
                        _autoLoadBW.RunWorkerAsync(_lastAutoLoadDataPoint);
                    }

                    return;
                }
            }

            if (_dataDict.Count == 0 || _autoLoadBW.IsBusy) return;
            DataLoaderStatus = TaskStatus.RanToCompletion;
            _monitorTimer.Stop();
        }

        private async Task<IEnumerable<TData>> LoadDataFromProxy(TKey point)
        {
            var key = _keyHelper.GetKey(point);
            var end = _calculator.Add(key, _config.PerLoadSize);
            IEnumerable<TData> result = null;

            if (_dataDict.TryGetValue(key, out var data))
            {
                Debug.WriteLine($"load data from dict,key:{key}");
                result = data;
            }
            else if (_taskDict.Count < _config.LoadParallelLimit) //限制并发量
            {
                _dataDict.TryAdd(key, null);

                var cancel = new CancellationTokenSource();
                var task = DataSourceDelegate(key, end, cancel.Token);
                _taskDict.TryAdd(key, new DataSourceTask<TData>(task, cancel));
                result = await task;
                if (_dataDict.ContainsKey(key)) _dataDict[key] = result;
                Debug.WriteLine($"load data from new task,key:{key}");
            }

            return result;
        }

        private void RemoveDataLoadTask()
        {
            _taskDict
                .Where(t => t.Value.Loader.Status == TaskStatus.RanToCompletion)
                .Select(t => t.Key)
                .ToList()
                .ForEach(t =>
                {
                    if (!_taskDict.TryRemove(t, out var task) || task == null) return;
                    task.Loader.Dispose();
                    task.CancellationSource.Dispose();
                });
        }

        private void RemoveTimeData()
        {
            _lastCleanDataPoint = _keyHelper.GetKey(CurrentPoint);

            var (sT, eT) = _keyHelper.GetCacheStartAndEndKey(_lastCleanDataPoint);
            var removes = _dataDict.Keys.Where(t => _calculator.Lt(t, sT) || _calculator.Gt(t, eT)).ToList();

            foreach (var key in removes)
            {
                _dataDict.TryRemove(key, out _);

                if (_taskDict.TryRemove(key, out var task) && task.Loader.Status != TaskStatus.RanToCompletion)
                {
                    task.CancellationSource.Cancel();
                }

                Debug.WriteLine($"remove data from dict,key:{key}");
            }
        }

        private void SetValueOfLastMember()
        {
            if (_calculator.Eq(_lastAutoLoadDataPoint, _calculator.MinValue))
            {
                _lastAutoLoadDataPoint = CurrentPoint;
                if (_autoLoadBW.IsBusy == false)
                {
                    _autoLoadBW.RunWorkerAsync(CurrentPoint);
                }
            }

            if (_calculator.Eq(_lastCleanDataPoint, _calculator.MinValue))
                _lastCleanDataPoint = CurrentPoint;
        }

        private async Task<IEnumerable<TData>> GetTimeDataFromKey(TKey key)
        {
            if (_dataDict.TryGetValue(key, out var data) && data != null) return data;
            if (_config.AcquisitionMode == DataAcquisitionMode.Smooth) return data;
            if (_taskDict.TryGetValue(key, out var task) && task != null)
            {
                data = await task.Loader;
            }
            else
            {
                data = await LoadDataFromProxy(key);
            }

            return data;
        }

        public int Count => _dataDict.Count;

        public async Task<IEnumerable<TData>> GetCacheData(TKey start, TKey end, Func<TData, TKey> keyOfTData)
        {
            if (_calculator.Lt(start, _config.StartPoint) || _calculator.Gt(end, _config.EndPoint)) return null;

            var result = new List<TData>();
            var keys = _keyHelper.GetKeys(start, end);
            foreach (var key in keys)
            {
                var data = await GetTimeDataFromKey(key);
                if (data != null)
                    result.AddRange(data.Where(t =>
                        _calculator.Gt(keyOfTData(t), start) && _calculator.Lt(keyOfTData(t), end)).ToList());
                else if (_config.AcquisitionMode == DataAcquisitionMode.Comprehensive)
                {
                    data = await LoadDataFromProxy(key);
                    if (data != null)
                        result.AddRange(data.Where(t =>
                            _calculator.Gt(keyOfTData(t), start) && _calculator.Lt(keyOfTData(t), end)).ToList());
                }
            }

            return result;
        }


        public void Dispose()
        {
            DataLoaderStatus = TaskStatus.Canceled;
            _autoLoadBW.CancelAsync();
            _autoLoadBW.Dispose();

            _monitorTimer.Stop();
            _monitorTimer.Dispose();

            _dataDict.Clear();
            _taskDict.Clear();
        }
    }
}