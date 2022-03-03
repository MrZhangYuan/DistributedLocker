using DistributedLocker.Internal;
using System;

namespace DistributedLocker
{
    public class Lockey
    {
        public string BusinessType
        {
            get;
        }

        public string BusinessCode
        {
            get;
        }

        public string Token
        {
            get;
        }

        internal string InternalType
        {
            get;
        }

        internal string InternalCode
        {
            get;
        }

        public Lockey(string businessType, string businessCode)
        {
            this.BusinessType = businessType;
            this.BusinessCode = businessCode;

            this.InternalType = UtilMethods.MD5IfOverLength(businessType, 32);
            this.InternalCode = UtilMethods.MD5IfOverLength(businessCode, 32);

            this.Token = Guid.NewGuid().ToString("N").ToUpper();
        }

        public override bool Equals(object obj)
        {
            return obj is Lockey key
                && Equals(key);
        }

        public bool Equals(Lockey key)
        {
            if (object.ReferenceEquals(key, null))
            {
                return false;
            }

            return this.BusinessCode.Equals(key.BusinessCode)
                && this.BusinessType.Equals(key.BusinessType)
                && this.Token.Equals(key.Token);
        }

        //public static bool operator ==(Lockey key1, Lockey key2)
        //{
        //    return key1 != null
        //        && key1.Equals(key2);
        //}

        //public static bool operator !=(Lockey key1, Lockey key2)
        //{
        //    return !(key1 == key2);
        //}

        public override int GetHashCode()
        {
            return this.BusinessType.GetHashCode()
                ^ this.BusinessCode.GetHashCode()
                ^ this.Token.GetHashCode();
        }
    }
}
