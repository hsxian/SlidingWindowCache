# c#滑窗缓存

> 前言

在大数据时代，软件系统需要具备处理海量数据的能力，同时也更加依赖于系统强大的存储能力与数据响应能力。各种大数据的工具如雨后春笋般孕育而生，这对于系统来说是极大的利好。但在后端采用分布式、云存储和虚拟化等技术大刀阔斧地解决大部分存储问题后，仍然不足以满足所有的业务需求。对于以用户为终点的软件系统来说，无论后台多么强大都难以避免有一部分数据流向终端，面向用户。为了应对这最后一公里的通勤问题，我们得在终端缓存部分数据来提高系统的响应效率。另外一方面，受限于用户终端的机器性能，缓存大量的数据反而会降低系统响应速度，甚至让系统崩溃。为此，我们需要一个根据系统当前状态动态调整最急需的数据的缓存器，滑窗缓存是一个很不错的选择。最终，我们找到了[SlidingWindowCache](https://github.com/hsxian/SlidingWindowCache),一个基于 .NET standard 实现的滑窗缓存。

> SlidingWindowCache 简介

SlidingWindowCache 基于键值对缓存，可以缓存以特定序列序列组织的数据,比如时间序列数据。其本身带有预先缓存的能力，当系统状态满足预设条件后将自动缓存数据。每次自动缓存的量可自行配置。当缓存超出窗口后即被视为无用数据，会被自动释放。同样的，缓存窗口大小可进行配置。

作为 key/value 缓存，该缓存的 value 可以是任意类型的数据。但为了满足有序组织，目前的 key 只支持 int、long、float 和 double 四种类型。对于时间序列数据来说，可以将时间转化为 long 作为 key 使用。后面将以 DataTime 转为 Ticks 为例进行演示（事实上转为时间戳更具有通用性），直接展示使用例程更加容易说明问题。

> SlidingWindowCache 使用

## SlidingWindowCache 配置

SlidingWindowCache 的绝大部分配置都在`ISlidingWindowConfig<TKey>`接口中定义，目前具有一下重要的配置:

- `TKey StartPoint { get; set; }` —— 缓存序列的起点
- `TKey EndPoint { get; set; }` —— 缓存序列的终点
- `TKey PerLoadSize { get; set; }` —— 每次缓存请求的大小。在自动缓存中，将自动向数据源请求数据
- `TKey TotalLoadSize { get; set; }` —— 总共加载的数据大小。在自动缓存中，缓存数据到达该阈值则停止自动缓存
- `TKey TotalCacheSize { get; set; }` —— 总共缓存的数据大小。即滑动窗口的大小，超出该窗口的数据被自动释放
- `int LoadParallelLimit { get; set; }` —— 自动加载数据时并发量阈值
- `float LoadTriggerFrequency { get; set; }` —— 加载触发频率。为 1 时，只要状态一改变，立即触发自动加载。
- `float RemoveTriggerFrequency { get; set; }` —— 移除触发频率。为 1 时，只要状态一改变，立即触发自动移除。
- `float ForwardAndBackwardScale { get; set; }` —— 前后比例（TKey 大端为前，习惯了以时间箭头为前）。以缓存大小来说，当前 TKey 作为分割点。

我们可以用形象的比喻来做进一步的解释。`StartPoint`和`EndPoint`限定了窗体能滑动的边界。`TotalCacheSize`限定了窗体的大小，在某种意义上来说，该窗体是残破不堪的，因其并未随时拥有所有的数据。它等待着修补匠进行破窗修补（数据源加载）。`TotalLoadSize`限定了每个状态生命周期中修补破窗的总大小，也就是自动请求数据量的大小。`PerLoadSize`则为每次修补的大小，即每次向数据源请求的数据量。`LoadParallelLimit`可以理解为可以同时工作的修补匠的最多人数。`LoadTriggerFrequency`则可以理解为当状态变更时，修补匠的出勤率。

## SlidingWindowCache 缓存

SlidingWindowCache 当前只提供少数重要的功能，全在`ISlidingWindowCache<TKey, TData>`接口中进行定义。

```csharp
// 当前点，用来标记缓存状态
TKey CurrentPoint { get; set; }
// 当前缓存的key的个数
int Count { get; }
// 从缓存中获取数据
Task<IEnumerable<TData>> GetCacheData(TKey start, TKey end, Func<TData, TKey> keyOfTData);
// 加载源数据的委托（必须进行赋值）
Func<TKey, TKey, CancellationToken, Task<IEnumerable<TData>>> DataSourceDelegate { get; set; }
// 自动加载任务状态报告事件
event EventHandler<TaskStatus> OnDataAutoLoaderStatusChanged;
```

## SlidingWindowCache 具体使用

下面以缓存时间序列数据为例做一具体使用介绍

```csharp
// 自定义数据模拟类
public class DataModel
{
    private static readonly Lazy<DataModel> _lazy = new Lazy<DataModel>(() => new DataModel());
    public static DataModel Instance => _lazy.Value;
    public long Point { get; set; }

    // 模拟服务器数据请求
    public Task<IEnumerable<DataModel>> LoadDataFromSource(long s, long e,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var rd = new Random();
            // 模拟远程访问数据时可能的延迟
            Task.Delay(rd.Next(50, 400), cancellationToken).Wait(cancellationToken);
            var diff = (int)(e - s);
            var count = diff > 100 ? 100 : diff;
            var result = Enumerable.Range(0, count)
                .Select(t => new DataModel { Point = s + rd.Next(diff) })
                .OrderBy(t => t.Point)
                .ToList();
            return (IEnumerable<DataModel>)result;
        }, cancellationToken);
    }
}

// 滑窗配置
var config = new SlidingWindowConfig<long>
{
    PerLoadSize = new TimeSpan(0, 2, 0).Ticks,
    StartPoint = new DateTime(2019, 1, 1).Ticks,
    EndPoint = new DateTime(2019, 2, 1).Ticks,
    TotalLoadSize = new TimeSpan(0, 30, 0).Ticks,
    TotalCacheSize = new TimeSpan(7, 0, 0).Ticks
};

// 实例化缓存器
var cache = new SlidingWindowCache<long, DataModel>(config)
{
    // 提供获取源数据的委托
    DataSourceDelegate = DataModel.Instance.LoadDataFromSource,
    CurrentPoint = config.StartPoint
};


// 获取2019-1-1 0:1:39至2019-1-1 0:2:0之间的数据
// lamda表达式t => t.Point提供缓存类型DataModel中的TKey的获取方法，用于数据过滤
var data = await cache.GetCacheData(
                new DateTime(2019, 1, 1, 0, 1, 39).Ticks,
                new DateTime(2019, 1, 1, 0, 2, 0).Ticks,
                t => t.Point);
```

上述例子中，我们可能查看的数据总范围为：2019-1-1 至 2019-2-1，总共为一个月的数据量。而终端机器允许缓存的数据量最多只能有7个小时。为了减少服务器压力，每次请求两分钟的数据量，自动缓存为半小时的数据量。在某一次数据获取中(2019-1-1 0:1:39至2019-1-1 0:2:0),获取21秒的数据，lamda将提供自动筛选的凭据。

> 后记

SlidingWindowCache 已经投入实际使用环境中，每次请求的量达到千级甚至万级，总共缓存的量达到百万级别(后端使用 Hbase 作为最终的存储方案，前端以 SlidingWindowCache 作为最终的缓存方案)。

[SlidingWindowCache](https://github.com/hsxian/SlidingWindowCache) 项目刚刚起步，欢迎提出改进意见。
