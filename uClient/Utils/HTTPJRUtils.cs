using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace uClient.Comm
{
    public class TimestampAndKeyGenerator
    {
        private static readonly Random random = new Random();

        public string GenerateTimestampAndKey(string infAddress)
        {
            string timestamp = G();
            string key = w(infAddress, timestamp, "v1");
            // 记录日志（在C#中使用日志库如NLog, log4net，或者Console）
            //Console.WriteLine($"timestamp: {timestamp}, key: {key}");
            return timestamp + "," + key;
        }

        public string G()
        {
            // 这里写一个固定值1000模拟时间差
            long D = DateTimeOffset.Now.ToUnixTimeMilliseconds() - 1000;
            long nanoTime = DateTime.UtcNow.Ticks; // System.nanoTime() 的模拟
            return (D / 1000).ToString() + (nanoTime + Math.Abs(random.NextLong()) % nanoTime).ToString();
        }

        public long D()
        {
            // 这里写一个固定值1000模拟时间差
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() - 1000;
        }

        public string w(string str, string str2, string str3)
        {
            // 字符串排序
            char[] charArray = str.ToCharArray();
            Array.Sort(charArray);

            string str4 = new string(charArray);

            // 获取当前时间戳（不含毫秒）
            string format = DateTimeOffset.FromUnixTimeMilliseconds(D()).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string valueOf = str4 + format + str3 + str2;

            byte[] hashBytes = null;
            try
            {
                using (SHA1 sha1 = SHA1.Create())
                {
                    hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(valueOf));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            string d2 = DToString(hashBytes);
            return d2.Length >= 16 ? d2.Substring(0, 16) : d2;
        }

        private string DToString(byte[] bArr)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bArr)
            {
                builder.AppendFormat("{0:x2}", b); // %02x 转换成小写十六进制
            }
            return builder.ToString();
        }
    }

    public static class RandomExtensions
    {
        private static readonly Random random = new Random();

        // C# 没有 NextLong，自己实现一个生成 Long 的随机数方法
        public static long NextLong(this Random rand)
        {
            byte[] buffer = new byte[8];
            rand.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }
    }

}
