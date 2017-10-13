using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadSUTBranchCEData;

namespace GADEApproach
{
    class RealSUT:SUT
    {
        public double[] RunSUT(string sut, params double[] num)
        {
            // Given an input, give SUT's output
            // Main purpose: Testing
            int[] outputs = null;
            readBranch rb = new readBranch();
            int[] inputs = Array.ConvertAll(num, n => (int)n);

            if (sut == "tri")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 0);
            }
            if (sut == "gcd")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 1);
            }
            if (sut == "calday")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 2);
            }
            if (sut == "bestmove")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 3);
            }
            if (sut == "neichneu")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 4);
            }
            return Array.ConvertAll(outputs, n => (double)n);
        }
        public void sutBestMove()
        {
            numOfMinIntervalX = 32;
            numOfMinIntervalY = 32;
            int minIntervalX = 512 / numOfMinIntervalX;
            int minIntervalY = 512 / numOfMinIntervalY;
            lowbounds = new double[] { 0.0, 0.0 };
            highbounds = new double[] { 511, 511 };
            numOfLabels = 42;
            double sampleProbability = 0.3;
            int totalNumberOfBins = numOfMinIntervalX * numOfMinIntervalY;
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].Item0 = i;
                bins[i].Item1 = -1;
                int xlowBoundIndex = (i % numOfMinIntervalX) * minIntervalX;
                int ylowBoundIndex = (i / numOfMinIntervalY) * minIntervalY;
                int sampleSize = (int)(minIntervalX * minIntervalY * sampleProbability);
                readBranch rbce = new readBranch();
                int count = 0;
                List<int[]> generatedList = new List<int[]>();
                int[] countsArray = new int[numOfLabels];
                while (count < sampleSize)
                {
                    int x = GlobalVar.rnd.Next(xlowBoundIndex, xlowBoundIndex + minIntervalX);
                    int y = GlobalVar.rnd.Next(ylowBoundIndex, ylowBoundIndex + minIntervalY);
                    if (generatedList.Where(t => (t[0] == x && t[1] == y) == true).Count() == 0)
                    {
                        count += 1;
                        generatedList.Add(new int[] { x, y });
                        int[] inputs = new int[2] { x, y };
                        int[] outputs = null;
                        rbce.ReadBranchCLIFunc(inputs, ref outputs, 3);
                        for (int k = 0; k < numOfLabels; k++)
                        {
                            countsArray[k] += outputs[k] == 1 ? 1 : 0;
                        }
                    }
                }
                double[] triggeringProbilities = new double[numOfLabels];
                for (int k = 0; k < triggeringProbilities.Length; k++)
                {
                    triggeringProbilities[k] = countsArray[k] * 1.0 / sampleSize;
                }
                bins[i].Item2 = triggeringProbilities;
            }

            var b = bins.Where(x => x.Item2[14] != 0);
            var o = bins.Where(x => x.Item2[17] != 0);
            var d = bins.Where(x => x.Item2[33] != 0);
            var e = bins.Where(x => x.Item2[37] != 0);
            var f = bins.Where(x => x.Item2[40] != 0);
            var g = bins.Where(x => x.Item2[41] != 0);
        }
    }
}
