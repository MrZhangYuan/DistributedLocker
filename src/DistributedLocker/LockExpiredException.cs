using System;

namespace DistributedLocker
{
    public class LockExpiredException : Exception
    {
        public Lockey ExpiredLockey
        {
            get;
        }

        public LockExpiredException(Lockey lockey)
            : this(lockey, "当前锁已过期")
        {
        }

        public LockExpiredException(Lockey lockey, string message)
            : this(lockey, message, null)
        {
        }

        public LockExpiredException(Lockey lockey, Exception innerException)
            : this(lockey, "当前锁已过期", innerException)
        {
        }

        public LockExpiredException(Lockey lockey, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ExpiredLockey = lockey;
        }
    }
}
