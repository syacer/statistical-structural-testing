
using System.Collections.Generic;
using Accord.Statistics.Distributions.Univariate;

namespace StatisticalApproach.Framework
{
    using System;
    using System.Threading;
    [Serializable]
    public class EnvironmentVar
    {
        /** Monitor**/
        public int taskNum;
        public bool finishIndicate = false;
        public bool continueIndicate = true;
        public object[] genFitRecord = new object[3] { 0, 0, 0 };
        public string solutionRecord;
        /** EndOfMonitor**/

        /**GeneralFitnessFitnessSetting**/
        public double[] AryWeights = new double[2] { 1, 0 }; // [1]: weights on esitmate average, weights on estiamte div
        public int evaToken = 0;   // 0 as using esitmate fitness in GA, 1 as using true fitness in GA.
        public int testSetSize = 10000;
        //** EndOfGeneralFitness**/

        /** SUT Related Info Import **/
        public Dictionary<string, object> pmProblem;
        /** EndOfSUT **/

        public bool emptyQueue;
        /** Monitor **/

        private static int seed;
        private static ThreadLocal<Random> threadLocal = new ThreadLocal<Random>
            (() => new Random(Interlocked.Increment(ref seed)));
        public void initializeEnvironmentVar(int taskNumber)
        {
            taskNum = taskNumber;
            emptyQueue = false;
            seed = Environment.TickCount;
        }
        public Random rnd { get { return threadLocal.Value; }}
        //public NormalDistribution gRnd = new NormalDistribution();
    }
}
