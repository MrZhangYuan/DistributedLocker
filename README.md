<div align="center">
  <h1>DistributedLocker</h1>
  <p>
    基于第三方介质（数据库、中间件、内存）等的基本功能完善的分布式锁
  </p>
  <p>
    .NET Standard (2.0)
  </p>
</div>


## Nuget 

暂未提供



## 简介

线程锁：.NET 提供的最常用的 Monitor，关键字为 lock ，用于在同一进程内对共享资源锁定与同步。  
进程锁：可使用.NET 提供的 Mutex 系统互斥量，用于在同一系统中的多个进程间同步。  
分布式锁：.NET 暂未提供相关功能，该功能很容易自行实现，实现方式多种多样：乐观锁、悲观锁、互斥锁...，所依赖的第三方共享存储也多种多样：数据库、Redis、ZooKeeper...。  
本库提供基于内存、数据库的互斥实现，因为这是最简单的一种方式，在面对一些中小型并发量不大的系统中应用绰绰有余。基于其它如Redis已经有现成的比较成熟的实现，如Redlock...  
但本库在设计上支持水平扩展，支持随意扩展其它第三方介质，包括使用单节点实现一个非高可用的RedisLock等。



## 示例

本库在设计上有点类似于 EF Core，使用起来也比较简单，也可以与 ASP net core web api 无缝集成。  

本库目前提供了基于内存、Oracle、Postgres的实现，使用方式完全一致。
同时本库的每个方法也提供了异步版本。  

配置参数：这些参数均为加锁时无参数时的默认值，Begin 和 BeginAsync 方法都有一个含有 LockParameter 参数的重载，当提供此参数，并且一些配置有值时，则当前锁使用用户设定值。  

```csharp
//配置是否使用基于内存的缓存机制，内存缓存存在一个命中率的问题
//集群或分布式环境中的节点越多，命中率越低
.WidthCache(true)

//配置锁持续时常的默认值（毫秒）
.WidthDuation(300)

//ConflictPloy配置为ConflictPloy.Wait时的重试配置：重试次数、重试间隔
.WidthRetry(4, 100)

//冲突策略
//ConflictPloy.Exception 直接抛出 LockConflictException 异常
//ConflictPloy.Wait 使用用户配置进行等待或尝试，若仍没有拿到锁，则抛出 LockConflictException 异常
//ConflictPloy.Execute 执行用户指定的 Action，若用户没有在 Action 中抛出其它异常，则执行完 Action 后，抛出 LockConflictException 异常
.WidthConflictPloy(ConflictPloy.Wait)

//配置锁调用Keep时的默认加时时常（毫秒）
.WidthKeepDuation(500)

//是否启用自动保持，自动保持策略的自动时机受 Duation 和 KeepDuation 的影响
//当这个参数配置为 true 时，若当前的 Duation = 100; KeepDuation = 300;则：
//第一次调用 Keep 的时机为加锁后 80ms
//第二次及之后调用间隔为 240ms
//即：调用间隔为 Duation * 0.8 或 KeepDuation * 0.8，也就是在锁即将过期而用户既没保持也没有释放时
.WidthAutoKeep(false)

//配置锁是否默认为持久锁，以及持久锁的过期时常
.WidthPersistence(false, TimeSpan.FromDays(7));
```

单独使用：
```csharp
static void Main(string[] args)
{
	StandardUse();
}

public static void StandardUse()
{
    var builder = new LockOptionsBuilder()
                    .UseMemoryLock()
                    //.UsePostgresLock("ConnectionString")
                    //.UseOracleLock("ConnectionString")
                    .WidthCache(true)
                    .WidthDuation(300)
                    .WidthRetry(4, 100)
                    .WidthConflictPloy(ConflictPloy.Wait)
                    .WidthKeepDuation(500)
                    .WidthAutoKeep(false)
                    .WidthPersistence(false, TimeSpan.FromDays(7));

    using (var lockcontext = new DistributedLockContext(builder.Options))
    {
        for (int i = 0; true; i++)
        {
            using (var scope = lockcontext.Begin(new Lockey("MyLocker", i.ToString().PadLeft(6, '0'))))
            {
                //  using 内部此处便是一个基于 MyLocker 和 i.ToString().PadLeft(6, '0') 的单线程环境
                //  在此处便可以放心的处理一些需要基于当前 Key 进行互斥的操作

                Console.WriteLine("锁");
                Console.ReadKey();

                scope.Keep();
                Console.WriteLine("保持");
                Console.ReadKey();

                scope.AutoKeep();
                Console.WriteLine("自动保持");

                Console.ReadKey();
            }
        }
    }
}
```

WebApi集成：
```csharp

static void Main(string[] args)
{
	WebApiIntergration();
}

public static void WebApiIntergration()
{
    ServiceCollection services = new ServiceCollection();

    var provider = ConfigureServices(services);

    provider.GetService<IDemoController>()
        .TestLock();
}

public static IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddDistributedLock(_p =>
    {
        //_p.UsePostgresLock("ConnectionString")
        _p.UseOracleLock("ConnectionString")
        //_p.UseMemoryLock()
        .WidthCache(true)
        .WidthDuation(300)
        .WidthRetry(4, 100)
        .WidthConflictPloy(ConflictPloy.Wait)
        .WidthKeepDuation(500)
        .WidthAutoKeep(false)
        .WidthPersistence(false, TimeSpan.FromDays(7));
    });

    services.AddScoped<IDemoController, DemoController>();

    return services.BuildServiceProvider();
}

        
public interface IDemoController
{
    void TestLock();
}
public class DemoController : IDemoController
{
    private readonly DistributedLockContext _distributedLockContext = null;

    public DemoController(DistributedLockContext distributedLockContext)
    {
        _distributedLockContext = distributedLockContext;
    }

    public void TestLock()
    {
        using (var scope = _distributedLockContext.Begin(new Lockey("APIName", "MayBeAParameter")))
        {
            //  using 内部此处便是一个基于 MyLocker 和 i.ToString().PadLeft(6, '0') 的单线程环境
            //  在此处便可以放心的处理一些需要基于当前 Key 进行互斥的操作

            Console.WriteLine("锁");
            Console.ReadKey();

            scope.Keep();
            Console.WriteLine("保持");
            Console.ReadKey();

            scope.AutoKeep();
            Console.WriteLine("自动保持");

            Console.ReadKey();
        }
    }
}

```



## 扩展

本库在设计上支持扩展其它任何第三方存储，只要实现以下几个类，以简单的扩展单节点Redis为例，分三步：  

1：首先提供一个 LockOptionsBuilder 的扩展，用于引入我们针对此库的扩展  
2：其次实现一个扩展配置类 RedisLockOptionsExtension，主要用于配置 Redis 一些参数，和注入我们的扩展需要的一些自定义服务  
3：最后实现 AsyncDistributedLock 或 DistributedLock 核心类  

使用时只要 .UseRedisLock(...) 即可。  

```csharp

//首先提供一个 LockOptionsBuilder 的扩展，用于引入我们针对此库的扩展
public static class RedisLockOptionsExtensions
{
    public static LockOptionsBuilder UseRedisLock(this LockOptionsBuilder builder, string connstr, int dbnum)
    {
        UtilMethods.ThrowIfNull(builder, nameof(builder));

        builder.WithOption<RedisLockOptionsExtension>(
                _p => _p.WithConnectionString(connstr)
                        .WithDbNum(dbnum)
            );

        return builder;
    }
}

//其次实现一个扩展配置类 RedisLockOptionsExtension，主要用于配置 Redis 一些参数，和注入我们的扩展需要的一些自定义服务
public class RedisLockOptionsExtension : ILockOptionsExtension
{
    private string _connectionString = string.Empty;
    private int _dbNum = 0;

    public int DbNum
    {
        get => _dbNum;
    }
    public string ConnectionString
    {
        get => _connectionString;
    }

    public RedisLockOptionsExtension WithConnectionString(string connstr)
    {
        this._connectionString = connstr;
        return this;
    }

    public RedisLockOptionsExtension WithDbNum(int dbnum)
    {
        this._dbNum = dbnum;
        return this;
    }

    public void ApplyServices(IServiceCollection services)
    {
        services.AddScoped<IAsyncDistributedLock, RedisDistributedLock>();
    }

    public void Validate(ILockOptions options)
    {

    }
}

//最后实现 AsyncDistributedLock 或 DistributedLock 核心类
public class RedisDistributedLock : AsyncDistributedLock
{
    public RedisDistributedLock(ILockOptions options, IDistributedLockCacher cacher)
        : base(options, cacher)
    {

    }

    public override ValueTask ExitAsync(Lockey lockey, Locker locker)
    {
        throw new NotImplementedException();
    }

    public override ValueTask KeepAsync(Lockey lockey, Locker locker, TimeSpan span)
    {
        throw new NotImplementedException();
    }

    protected override Locker Enter(Lockey lockey, Locker locker, LockParameter param)
    {
        throw new NotImplementedException();
    }

    protected override ValueTask<Locker> EnterAsync(Lockey lockey, Locker locker, LockParameter parameter)
    {
        throw new NotImplementedException();
    }

    protected override void Exit(Lockey lockey, Locker locker)
    {
        throw new NotImplementedException();
    }

    protected override void Keep(Lockey lockey, Locker locker, TimeSpan span)
    {
        throw new NotImplementedException();
    }

    protected override bool TryEnter(Lockey lockey, Locker locker, LockParameter parameter)
    {
        throw new NotImplementedException();
    }

    protected override ValueTask<bool> TryEnterAsync(Lockey lockey, Locker locker, LockParameter parameter)
    {
        throw new NotImplementedException();
    }
}
```



## 原理


## 性能

基于内存缓存的原理：

