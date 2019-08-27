# SlidingWindowCache

SlidingWindowCache provides a local sliding window cache, which is mainly used to cache data organized in a certain sequence, such as time series data.The size of the load data window and the size of the cache data window can be configured.

SlidingWindowCache is compliant with the .NET standard and supports cross-platform use.

## Examples

```csharp
var config = new SlidingWindowConfig<long>
{
    PerLoadSize = new TimeSpan(0, 2, 0).Ticks,
    StartPoint = new DateTime(2019, 1, 1).Ticks,
    EndPoint = new DateTime(2019, 2, 1).Ticks,
    TotalLoadSize = new TimeSpan(0, 30, 0).Ticks,
    TotalCacheSize = new TimeSpan(7, 0, 0).Ticks
};

var cache = new SlidingWindowCache<long, DataModel>(config)
{
    DataSourceDelegate = DataModel.Instance.LoadDataFromSource,
    CurrentPoint = config.StartPoint
};

var data = await cache.GetCacheData(new DateTime(2019, 1, 1, 0, 1, 0).Ticks, new DateTime(2019, 1, 1, 0, 2, 0).Ticks, t => t.Point);
```

For more information, please check [SlidingWindowCache.ConsoleApp](test/SlidingWindowCache.ConsoleApp/Program.cs).

## List of Features

- Configuration
  - PerLoadSize
  - TotalLoadSize
  - TotalCacheSize
  - ForwardAndBackwardScale
- Automatic completion
  - Pre-cache data
  - Automatically remove data
