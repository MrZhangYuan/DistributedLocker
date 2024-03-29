﻿using System;

namespace DistributedLocker
{
    public interface IDistributedLock : IDisposable
    {
        LockParameter CreatOrSetDefaultParameter(Lockey lockey, LockParameter param);
        Locker Enter(Lockey lockey, LockParameter parameter);
        bool TryEnter(Lockey lockey, LockParameter parameter, out Locker locker);
        void Keep(Lockey lockey, TimeSpan span);
        void Keep(Lockey lockey);
        void Exit(Lockey lockey);
    }
}
