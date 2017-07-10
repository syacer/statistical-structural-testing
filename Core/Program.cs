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
        public static Mutex mutex_K = new Mutex(false, "lockFork");
        private static ThreadLocal<Random> threadLocal = new ThreadLocal<Random>
            (() => new Random(Interlocked.Increment(ref seed)));
        public static Random rnd { get { return threadLocal.Value; } }

    }
    class Program
    {
        static void Main(string[] args)
        {
            Vector<double>  numOfLabelsVect = Vector<double>
                .Build.Dense(new double[] {1.0/6, 1.0 / 6, 1.0 / 6,
                1.0/6,1.0/6,1.0/6});

            double entropy = 0;
            for (int i = 0; i < 6; i++)
            {
                entropy += numOfLabelsVect[i] * Math.Log10(1.0 / (numOfLabelsVect[i] + 0.000001));
            }
            new CustMOGA().MOGA_Start();
        }
    }
}

