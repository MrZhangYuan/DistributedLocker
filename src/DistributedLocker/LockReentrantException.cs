using System;

namespace DistributedLocker
{
    /// <summary>
    ///     Enter 相关的锁重入异常
    /// </summary>
    public class LockReentrantException : Exception
    {
        public Lockey ReentrantLockey
        {
            get;
        }

        public LockReentrantException(Lockey lockey)
            : this(lockey, "发生了可能的锁重入")
        {
        }

        public LockReentrantException(Lockey lockey, string message)
            : this(lockey, message, null)
        {
        }

        public LockReentrantException(Lockey lockey, Exception innerException)
            : this(lockey, "发生了可能的锁重入", innerException)
        {
        }

        public LockReentrantException(Lockey lockey, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ReentrantLockey = lockey;
        }
    }
}
