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
            Dictionary<string, int> pathStorage
                 = new Dictionary<string, int>();
            numOfMinIntervalX = 32;
            numOfMinIntervalY = 32;
            int minIntervalX = 512 / numOfMinIntervalX;
            int minIntervalY = 512 / numOfMinIntervalY;
            lowbounds =  new double[] { 0.0, 0.0 };
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
                List<string> paths = new List<string>();
                int count = 0;
                List<int[]> generatedList = new List<int[]>();
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
                        string path = null;
                        foreach (int e in outputs)
                        {
                            path = path + e.ToString();
                        }
                        paths.Add(path);
                        if (!pathStorage.ContainsKey(path))
                        {
                            int newValue = -1;
                            if (pathStorage.Count == 0)
                            {
                                newValue = 0;
                            }
                            else
                            {
                                newValue = pathStorage.Values.Max() + 1;
                            }
                            pathStorage.Add(path, newValue);
                        }
                    }
                }
                double[] triggeringProbilities = new double[numOfLabels];
                var distinctPaths = paths.Distinct().ToList();
                for (int o = 0; o < distinctPaths.Count; o++)
                {
                    double triProb = paths.Where(x => x == distinctPaths[o]).Count() * 1.0 / sampleSize;
                    int value = pathStorage.ContainsKey(distinctPaths[o]) ?
                        pathStorage[distinctPaths[o]] : -1;
                    if (value == -1)
                    {
                        Console.WriteLine("pathStorage WRONG");
                    }
                    triggeringProbilities[value] = triProb;
                }
                bins[i].Item2 = triggeringProbilities;
            }
        }
    }
}
