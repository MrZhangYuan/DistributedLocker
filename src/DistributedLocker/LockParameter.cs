using System;

namespace DistributedLocker
{
    /// <summary>
    ///     锁重入条件
    /// </summary>
    [Flags]
    public enum ReentrantContion : uint
    {
        IP,
        HostName,
        OperCode,
        OperType,
        ProcessId,
        ThreadId
    }
    public class LockParameter
    {
        /// <summary>
        ///     附加数据
        /// </summary>
        public object Data
        {
            get;
            set;
        }

        /// <summary>
        ///     冲突策略
        /// </summary>
        public ConflictPloy ConflictPloy
        {
            get;
            set;
        }

        /// <summary>
        ///     冲突事件
        /// </summary>
        public Action<Locker> OnConflict
        {
            get;
            set;
        }

        /// <summary>
        ///     重试时间间隔（毫秒）
        ///     仅在 ConflictPloy.Wait 下生效
        /// </summary>
        public int RetryInterval
        {
            get;
            set;
        }

        /// <summary>
        ///     重试次数
        ///     仅在 ConflictPloy.Wait 下生效
        /// </summary>
        public int RetryTimes
        {
            get;
            set;
        }

        /// <summary>
        ///     持续时常（毫秒）
        /// </summary>
        public int Duation
        {
            get;
            set;
        }

        /// <summary>
        ///     Keep的缺省时常（毫秒）
        /// </summary>
        public int KeepDuation
        {
            get;
            set;
        }

        /// <summary>
        ///     使用内存缓存
        /// </summary>
        public bool UseMemoryCache
        {
            get;
            set;
        }

        /// <summary>
        ///     自动保持
        ///     自动调用 Keep 或 KeepAsync
        /// </summary>
        public bool AutoKeep
        {
            get;
            set;
        }

        /// <summary>
        ///     是否持久化
        /// </summary>
        public bool IsPersistence
        {
            get;
            set;
        }

        /// <summary>
        ///     IP
        /// </summary>
        public string IP
        {
            get;
            set;
        }

        /// <summary>
        ///     锁说明
        /// </summary>
        public string LockMsg
        {
            get;
            set;
        }

        /// <summary>
        ///     指定发生冲突时的消息
        /// </summary>
        public string ConflictMsg
        {
            get;
            set;
        }

        /// <summary>
        ///     操作用户代码
        /// </summary>
        public string OperCode
        {
            get;
            set;
        }

        /// <summary>
        ///     操作用户姓名
        /// </summary>
        public string OperName
        {
            get;
            set;
        }

        /// <summary>
        ///     操作事件
        /// </summary>
        public string OperType
        {
            get;
            set;
        }
    }
}
