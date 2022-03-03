using DistributedLocker.Extensions;
using System;
using System.Collections.Generic;

namespace DistributedLocker
{
    public class LockOptions : ILockOptions
    {
        private readonly Dictionary<Type, ILockOptionsExtension> _extensions = new Dictionary<Type, ILockOptionsExtension>();
        public IEnumerable<ILockOptionsExtension> Extensions
            => _extensions.Values;

        public TExtension FindExtension<TExtension>() where TExtension : class, ILockOptionsExtension
        {
            _extensions.TryGetValue(typeof(TExtension), out var extension);
            return (TExtension)extension;
        }

        public void WidthExtension<TExtension>(TExtension extension) where TExtension : class, ILockOptionsExtension
        {
            _extensions[extension.GetType()] = extension;
        }
    }
}
