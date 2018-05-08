using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Zxw.Framework.NetCore.Helpers
{
    public class CommonHelper
    {
        private static Random rnd = new Random();

        private static readonly int __staticMachine = ((0x00ffffff & Environment.MachineName.GetHashCode()) +
#if NETSTANDARD1_5 || NETSTANDARD1_6
			1
#else
                                                       AppDomain.CurrentDomain.Id
#endif
                                                      ) & 0x00ffffff;
        private static readonly int __staticPid = Process.GetCurrentProcess().Id;
        private static int __staticIncrement = rnd.Next();

        /// <summary>
        /// 生成类似Mongodb的ObjectId有序、不重复Guid
        /// </summary>
        /// <returns></returns>
        public static Guid NewMongodbId()
        {
            var now = DateTime.Now;
            var uninxtime = (int) now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff;
            var rand = rnd.Next(0, int.MaxValue);
            var guid =
                $"{uninxtime.ToString("x8").PadLeft(8, '0')}{__staticMachine.ToString("x8").PadLeft(8, '0').Substring(2, 6)}{__staticPid.ToString("x8").PadLeft(8, '0').Substring(6, 2)}{increment.ToString("x8").PadLeft(8, '0')}{rand.ToString("x8").PadLeft(8, '0')}";
            return Guid.Parse(guid);
        }

        /// <summary>
        /// 获取日期差
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string GetDateDiff(DateTime src)
        {
            string result = null;

            var currentSecond = (long)(DateTime.Now - src).TotalSeconds;

            long minSecond = 60;                //60s = 1min  
            var hourSecond = minSecond * 60;   //60*60s = 1 hour  
            var daySecond = hourSecond * 24;   //60*60*24s = 1 day  
            var weekSecond = daySecond * 7;    //60*60*24*7s = 1 week  
            var monthSecond = daySecond * 30;  //60*60*24*30s = 1 month  
            var yearSecond = daySecond * 365;  //60*60*24*365s = 1 year  

            if (currentSecond >= yearSecond)
            {
                var year = (int)(currentSecond / yearSecond);
                result = $"{year}年前";
            }
            else if (currentSecond < yearSecond && currentSecond >= monthSecond)
            {
                var month = (int)(currentSecond / monthSecond);
                result = $"{month}个月前";
            }
            else if (currentSecond < monthSecond && currentSecond >= weekSecond)
            {
                var week = (int)(currentSecond / weekSecond);
                result = $"{week}周前";
            }
            else if (currentSecond < weekSecond && currentSecond >= daySecond)
            {
                var day = (int)(currentSecond / daySecond);
                result = $"{day}天前";
            }
            else if (currentSecond < daySecond && currentSecond >= hourSecond)
            {
                var hour = (int)(currentSecond / hourSecond);
                result = $"{hour}小时前";
            }
            else if (currentSecond < hourSecond && currentSecond >= minSecond)
            {
                var min = (int)(currentSecond / minSecond);
                result = $"{min}分钟前";
            }
            else if (currentSecond < minSecond && currentSecond >= 0)
            {
                result = "刚刚";
            }
            else
            {
                result = src.ToString("yyyy/MM/dd HH:mm:ss");
            }
            return result;
        }
    }
}
