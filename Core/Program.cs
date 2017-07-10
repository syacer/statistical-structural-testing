using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.Threading;

namespace Core
{
    public static class GlobalVar
    {
        private static int seed = Environment.TickCount;
        private static ThreadLocal<Random> threadLocal = new ThreadLocal<Random>
            (() => new Random(Interlocked.Increment(ref seed)));
        public static Random rnd { get { return threadLocal.Value; } }
    }
    class Program
    {
        static void Main(string[] args)
        {
            new CustMOGA().MOGA_Start();
        }
    }
}

