using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DistributedLocker.Internal
{
    internal static class UtilMethods
    {
        public static ValueTask DefaultValueTask()
        {
            return default;
        }

        public static ValueTask<T> ValueTaskFromResult<T>(T value)
        {
            return new ValueTask<T>(value);
        }

        public static void ThrowIfNull(object obj, string argname)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(argname);
            }
        }


        public static bool NotNull(object obj)
        {
            return obj != null;
        }


        public static long GetTimeStamp()
        {
            return (DateTime.UtcNow.Ticks - 621355968000000000) / 10000;
        }

        public static string MD5IfOverLength(string str, int maxlength)
        {
            if (str == null
                || str.Length < maxlength)
            {
                return str;
            }

            using (MD5CryptoServiceProvider md = new MD5CryptoServiceProvider())
            {
                var md5 = BitConverter.ToString(md.ComputeHash(Encoding.Default.GetBytes(str)))
                            .Replace("-", "");

                return md5.Length <= 32 ? md5 : md5.Substring(0, 32);
            }
        }
    }

}
