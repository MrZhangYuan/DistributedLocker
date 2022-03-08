using DistributedLocker;
using DistributedLocker.Extensions;
using DistributedLocker.Memory.Extensions;
using DistributedLocker.Oracle.Extensions;
using DistributedLocker.Redis.Extensions;
using DistributedLocker.Postgres.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace TestDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //var dsds = Process.GetCurrentProcess();

            UseLock(_p => _p.UsePostgresLock(""));
            //UseLock(_p => _p.UseOracleLock(""));
            //UseLock(_p => _p.UseMemoryLock());
            //UseLock(_p => _p.UseRedisLock("",0));
        }

        private static void UseLock(Func<LockOptionsBuilder, LockOptionsBuilder> config)
        {
            ServiceCollection services = new ServiceCollection();

            services.AddDistributedLock(_p =>
            {
                config(_p)
                .WidthRetryInterval(20)
                .WidthCache(true)
                .WidthDuation(200)
                .WidthRetryTimes(3)
                .WidthConflictPloy(ConflictPloy.Wait)
                .WidthKeepDuation(100)
                .WidthAutoKeep(false)
                .WidthPersistenceDuation(TimeSpan.FromDays(7))
                .WidthDefaultPersistence(false);
            });

            var provider = services.BuildServiceProvider();

            for (int i = 1; true; i++)
            {
                using (var scope = provider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<DistributedLockContext>();

                    using (var lockerscope = context.Begin(new Lockey("MEDICAL", (i + "").PadLeft(6, '0')), new LockParameter { Duation = 200 }))
                    {
                        Console.WriteLine("锁");
                        Console.ReadKey();

                        lockerscope.Keep(TimeSpan.FromMilliseconds(20 * 1000));

                        Console.WriteLine("保持");
                        Console.ReadKey();
                    }
                }
            }
        }
    }
}
