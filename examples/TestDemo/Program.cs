﻿using DistributedLocker;
using DistributedLocker.Extensions;
using DistributedLocker.Memory.Extensions;
using DistributedLocker.Oracle.Extensions;
using DistributedLocker.Redis.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace TestDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //UseLock(_p => _p.UseOracleLock("User Id=emrmix;Password=Synyi123;Data Source=172.16.1.151:1521/emrmix;"));
            UseLock(_p => _p.UseMemoryLock());
            //UseLock(_p => _p.UseRedisLock("",0));
        }

        private static async void UseLock(Func<LockOptionsBuilder, LockOptionsBuilder> config)
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
                .WidthAutoKeep(false);
            });

            var provider = services.BuildServiceProvider();

            for (int i = 0; true; i++)
            {
                using (var scope = provider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<DistributedLockContext>();

                    await using (var lockerscope = await context.BeginAsync(new Lockey("MEDICAL", i + ""), new LockParameter { }))
                    {
                        Console.WriteLine("锁");
                        Console.ReadKey();

                        await lockerscope.KeepAsync(TimeSpan.FromMilliseconds(20 * 1000));

                        Console.WriteLine("保持");
                        Console.ReadKey();
                    }
                }
            }
        }
    }
}
