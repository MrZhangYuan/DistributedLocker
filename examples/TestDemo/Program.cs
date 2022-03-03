using DistributedLocker;
using DistributedLocker.Extensions;
using DistributedLocker.Oracle.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace TestDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            NewMethod();
        }

        private static async void NewMethod()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddDistributedLock(_p =>
            {
                _p.UseOracleLock("User Id=emrmix;Password=Synyi123;Data Source=172.16.1.151:1521/emrmix;")
                    .WidthRetryInterval(20)
                    .WidthMemoryCache(true)
                    .WidthDuation(200)
                    .WidthRetryTimes(3)
                    .WidthConflictPloy(ConflictPloy.Wait)
                    .WidthKeepDuation(100)
                    .WidthAutoKeep(false);
            });

            var provider = services.BuildServiceProvider();
            var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DistributedLockContext>();

            for (int i = 0; true; i++)
            {
                await using (var lockerscope = await context.BeginAsync(new Lockey("MEDICAL", i + ""), new LockParameter { }))
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
