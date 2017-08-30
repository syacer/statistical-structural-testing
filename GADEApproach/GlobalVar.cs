using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GADEApproach
{
    public static class GlobalVar
    {
        private static int seed = Environment.TickCount;
        public static Mutex mutex_K = new Mutex(false, "lockFork");
        private static ThreadLocal<Random> threadLocal = new ThreadLocal<Random>
            (() => new Random(Interlocked.Increment(ref seed)));
        public static Random rnd { get { return threadLocal.Value; } }
    }
}
