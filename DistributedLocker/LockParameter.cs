using System;

namespace DistributedLocker
{
    public class LockParameter
    {
        public object Data
        {
            get;
            set;
        }

        public ConflictPloy ConflictPloy
        {
            get;
            set;
        }

        public Action<Locker> OnConflict
        {
            get;
            set;
        }

        public int RetryInterval
        {
            get;
            set;
        }

        public int RetryTimes
        {
            get;
            set;
        }

        /// <summary>
        /// 持续时常（毫秒）
        /// </summary>
        public int Duation
        {
            get;
            set;
        }

        public int KeepDuation
        {
            get;
            set;
        }

        public bool UseMemoryCache
        {
            get;
            set;
        }
        public bool AutoKeep
        {
            get;
            set;
        }

        public string IP
        {
            get;
            set;
        }

        public string LockMsg
        {
            get;
            set;
        }

        public string ConflictMsg
        {
            get;
            set;
        }

        public string OperCode
        {
            get;
            set;
        }

        public string OperName
        {
            get;
            set;
        }

        public string OperType
        {
            get;
            set;
        }
    }
}
