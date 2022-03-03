using System;

namespace DistributedLocker
{
    public class LockConflictException : Exception
    {
        public Locker ExistsLocker
        {
            get;
        }

        public LockConflictException(Locker locker)
            : this(locker, "检测到并发冲突")
        {
        }

        public LockConflictException(Locker locker, string message)
            : this(locker, message, null)
        {
        }

        public LockConflictException(Locker locker, Exception innerException)
            : this(locker, "检测到并发冲突", innerException)
        {
        }

        public LockConflictException(Locker locker, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ExistsLocker = locker;
        }
    }
}
