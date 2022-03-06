namespace DistributedLocker
{
    public interface IAutoKeeper
    {
        void AddLockScope(IAsyncLockScope scope);
    }

}
