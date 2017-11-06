using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadSUTBranchCEData;
using System.Data;

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
            sutIndex = 3;
            numOfMinIntervalX = 128;
            numOfMinIntervalY = 128;
            int minIntervalX = 512 / numOfMinIntervalX;
            int minIntervalY = 512 / numOfMinIntervalY;
            numOfMinIntervalInAllDim = numOfMinIntervalX;
            lowbounds = new double[] { 0.0, 0.0 };
            highbounds = new double[] { 511, 511 };
            numOfLabels = 42;
            double sampleProbability = 0.3;
            int totalNumberOfBins = numOfMinIntervalX * numOfMinIntervalY;
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].BinIndex = i;
                bins[i].SetIndexPlus1 = -1;
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
                bins[i].ProbInBins = triggeringProbilities;
            }

            var b = bins.Where(x => x.ProbInBins[14] != 0);
            var o = bins.Where(x => x.ProbInBins[17] != 0);
            var d = bins.Where(x => x.ProbInBins[33] != 0);
            var e = bins.Where(x => x.ProbInBins[37] != 0);
            var f = bins.Where(x => x.ProbInBins[40] != 0);
            var g = bins.Where(x => x.ProbInBins[41] != 0);
        }
        public DataTable MapTestInputsToBinsBestMove()
        {
            DataTable InputsBinTable = new DataTable();
            InputsBinTable.Columns.Add("Input x", Type.GetType("System.Int32"));
            InputsBinTable.Columns.Add("Input y", Type.GetType("System.Int32"));
            InputsBinTable.Columns.Add("IntentionallyBlank", Type.GetType("System.Int32"));
            InputsBinTable.Columns.Add("BinIndexFrom1", Type.GetType("System.Int32"));

            int totalNumberOfBins = numOfMinIntervalX * numOfMinIntervalY;
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                Tuple<int, int> indexs = Calxyindex(i);
                double deltaX = (highbounds[0] - lowbounds[0] + 1) / numOfMinIntervalX;
                double deltaY = (highbounds[1] - lowbounds[1] + 1) / numOfMinIntervalY;
                int lowX = (int)deltaX * indexs.Item1;
                int lowY = (int)deltaY * indexs.Item2;
                for (int x = lowX; x < lowX + deltaX; x++)
                {
                    for (int y = lowY; y < lowY + deltaY; y++)
                    {
                        object[] rowData = new object[] { x, y, -9999999, i+1};
                        var row = InputsBinTable.NewRow();
                        row.ItemArray = rowData;
                        InputsBinTable.Rows.Add(row);
                    }
                }
            }
            return InputsBinTable;
        }
    }
}
