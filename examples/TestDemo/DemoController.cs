using DistributedLocker;
using System;

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
}
