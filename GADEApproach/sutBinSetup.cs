using ReadSUTBranchCEData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static class sutBinSetup
    {

        public static void sutBestMove(
            Pair<int, int, double[]>[] bins, int numOfLabels
            )
        {
            Dictionary<string, int> pathStorage 
                 = new Dictionary<string, int>();
            int numOfMinIntervalX = 32;
            int numOfMinIntervalY = 32;
            int minIntervalX = 512 / numOfMinIntervalX;
            int minIntervalY = 512 / numOfMinIntervalY;
            double sampleProbability = 0.3;
            int totalNumberOfBins = numOfMinIntervalX * numOfMinIntervalY;
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].Item0 = i;
                bins[i].Item1 = -1;
                int xlowBoundIndex = (i % numOfMinIntervalX) *minIntervalX;
                int ylowBoundIndex = (i / numOfMinIntervalY) *minIntervalY;
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
                        generatedList.Add(new int[] {x, y});
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
                            int newValue = pathStorage.Values.Max() + 1;
                            pathStorage.Add(path, newValue);
                        }
                    }
                }
                double[] triggeringProbilities = new double[numOfLabels];
                var distinctPaths = paths.Distinct().ToList();
                for (int o = 0; o < distinctPaths.Count; o++)
                {
                    double triProb = paths.Count(x => x == distinctPaths[o])/sampleSize;
                    int value =  pathStorage.ContainsKey(distinctPaths[o]) ?
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
