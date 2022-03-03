using System;

namespace DistributedLocker
{
    public class Locker
    {
        public string BusinessType
        {
            get;
            internal set;
        }

        public string BusinessCode
        {
            get;
            internal set;
        }

        public long BeginTime
        {
            get;
            internal set;
        }

        public long EndTime
        {
            get;
            internal set;
        }

        public int Duation
        {
            get;
            internal set;
        }

        public string IP
        {
            get;
            internal set;
        }

        public string Token
        {
            get;
            internal set;
        }

        public int DelayTimes
        {
            get;
            internal set;
        }

        public string LockMsg
        {
            get;
            internal set;
        }

        public string ConflictMsg
        {
            get;
            internal set;
        }

        public string HostName
        {
            get;
            internal set;
        }

        public string OperCode
        {
            get;
            internal set;
        }

        public string OperName
        {
            get;
            internal set;
        }

        public string OperType
        {
            get;
            internal set;
        }

        public DateTime OperTime
        {
            get;
            internal set;
        }
    }
}
