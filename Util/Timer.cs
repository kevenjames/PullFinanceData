using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace PullFinanceData.Util
{
    public class Timer
    {
        private static readonly long s_tc;
        public static ulong f;
        protected ulong a, b, total;

        static Timer()
        {
            if (QueryPerformanceFrequency(out f) == 0)
                throw new Win32Exception();
            var tc = GetTickCount();
            var dt = DateTime.Now;
            s_tc = dt.Ticks - tc;
        }

        // Construction
        public Timer()
        {
            a = b = total = 0UL;
            Thread.Sleep(0);
            QueryPerformanceCounter(out a);
        }

        // Properties

        private ulong ElapsedTicks => b - a;

        public float ElapsedMillSeconds => ElapsedMicroseconds / 1000.0f;

        private ulong ElapsedMicroseconds
        {
            get
            {
                if (total < 0x10c6f7a0b5edUL) // 2^64 / 1e6
                    return total * 1000000UL / f;
                return total / f * 1000000UL;
            }
        }

        private static long GetTickCount()
        {
            ulong c;
            QueryPerformanceCounter(out c);
            return (long)((double)c * 1000 * 10000 / f);
        }

        public static DateTime GetExactNow()
        {
            return new DateTime(s_tc + GetTickCount());
        }

        public static string GetExactNowString()
        {
            return GetExactNow().ToString("yyyy-MM-dd hh:mm:ss.fff");
        }

        public ulong Stop()
        {
            QueryPerformanceCounter(out b);
            total += ElapsedTicks;
            return ElapsedTicks;
        }

        // Implementation

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern int QueryPerformanceFrequency(out ulong x);

        [DllImport("kernel32.dll")]
        protected static extern int QueryPerformanceCounter(out ulong x);
    }
}