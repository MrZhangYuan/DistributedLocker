using DistributedLocker;
using DistributedLocker.Extensions;
using DistributedLocker.Memory.Extensions;
using DistributedLocker.Oracle.Extensions;
using DistributedLocker.Redis.Extensions;
using DistributedLocker.Postgres.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace TestDemo
{
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
            Task.Factory.StartNew(async () =>
           {
               for (int i = 0; true; i++)
               {


                   await _distributedLockContext.TryBeginAsync(new Lockey("MEDICAL", (i + "").PadLeft(6, '0')), out var scope);
                   Console.WriteLine("锁");
                   await Task.Delay(new Random().Next(300,3000));
                   await scope.ExitAsync();
                   Console.WriteLine("解锁");
                   Console.WriteLine();




                   //using (var lockerscope = _distributedLockContext.Begin(
                   //                            new Lockey("MEDICAL", (i + "").PadLeft(6, '0'))))
                   //{
                   //    Console.WriteLine("锁");
                   //    Console.ReadKey();

                   //    //lockerscope.Keep(TimeSpan.FromMilliseconds(20 * 1000));
                   //    //Console.WriteLine("保持");

                   //    lockerscope.AutoKeep();
                   //    Console.WriteLine("自动保持");

                   //    Console.ReadKey();
                   //}
               }
           });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();

            var provider = ConfigureServices(services);

            provider.GetService<IDemoController>()
                .TestLock();

            Console.ReadKey();
        }

        public static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedLock(_p =>
            {
                //_p.UsePostgresLock("")
                _p.UseOracleLock("")
                //_p.UseMemoryLock()
                .WidthCache(true)
                .WidthDuation(300)
                .WidthRetry(4, 100)
                .WidthConflictPloy(ConflictPloy.Wait)
                .WidthKeepDuation(500)
                .WidthAutoKeep(true)
                .WidthPersistence(false, TimeSpan.FromDays(7));
            });

            services.AddScoped<IDemoController, DemoController>();

            return services.BuildServiceProvider();
        }
    }
}
