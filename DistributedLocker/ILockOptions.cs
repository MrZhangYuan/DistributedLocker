using DistributedLocker.Extensions;
using System.Collections.Generic;

namespace DistributedLocker
{
    public interface ILockOptions
    {
        IEnumerable<ILockOptionsExtension> Extensions { get; }
        TExtension FindExtension<TExtension>() where TExtension : class, ILockOptionsExtension;
        void WidthExtension<TExtension>(TExtension extension) where TExtension : class, ILockOptionsExtension;
    }
}
