using DistributedLocker;
using DistributedLocker.Extensions;
using DistributedLocker.Memory.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace TestDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            StandardUse();
            //WebApiIntergration();
        }

        #region WebApiIntergration


        public static void WebApiIntergration()
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
                //_p.UsePostgresLock("ConnectionString")
                //_p.UseOracleLock("ConnectionString")
                _p.UseMemoryLock()
                .WidthCache(true)
                .WidthDuation(10 * 1000)
                .WidthRetry(4, 100)
                .WidthConflictPloy(ConflictPloy.Wait)
                .WidthKeepDuation(5 * 1000)
                .WidthAutoKeep(false)
                .WidthPersistence(false, TimeSpan.FromDays(7));
            });

            services.AddScoped<IDemoController, DemoController>();

            return services.BuildServiceProvider();
        }

        #endregion


        #region StandardUse

        public static void StandardUse()
        {
            var builder = new LockOptionsBuilder()
                            .UseMemoryLock()
                            //.UsePostgresLock("ConnectionString")
                            //.UseOracleLock("ConnectionString")
                            //.UseSqlServerLock("Data Source=*.*.*.*,1433;Initial Catalog=SYS_LOCKER_TEST;User ID=sa;Password=******;TrustServerCertificate=true")
                            .WidthCache(true)
                            .WidthDuation(2 * 1000)
                            .WidthRetry(4, 100)
                            .WidthConflictPloy(ConflictPloy.Wait)
                            .WidthKeepDuation(1 * 1000)
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

        #endregion

    }
}
