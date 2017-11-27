using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadSUTBranchCEData;
using System.Data;
using MathNet.Numerics.LinearAlgebra;

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
            if (sut == "binarysearch")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 5);
            }
            if (sut == "select")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 6);
            }
            if (sut == "quicksort")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 7);
            }

            return Array.ConvertAll(outputs, n => (double)n);
        }

#region useless SUTs
        public void sutbinarysearch()
        {
            sutIndex = 5;
            totalNumberOfBins = 5;
            numOfMinIntervalInAllDim = 1000 / totalNumberOfBins;
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            double sampleProbability = 0.3;
            int sampleSize = (int)(numOfMinIntervalInAllDim * sampleProbability);
            numOfLabels = 23;
            Vector<double> totalTriggeringProbilities = Vector<double>.Build.Dense(502);

            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].BinIndex = i;
                bins[i].SetIndexPlus1 = -1;
                int lowBound = i * numOfMinIntervalInAllDim;
                int highBound = (i + 1) * numOfMinIntervalInAllDim;
                readBranch rbce = new readBranch();
                int count = 0;
                List<string> generatedList = new List<string>();
                int[] countsArray = new int[numOfLabels];

                while (count < sampleSize)
                {
                    int num = GlobalVar.rnd.Next(lowBound, highBound);
                    string numStr = num.ToString();

                    if (generatedList.Where(t => (t == numStr) == true).Count() == 0)
                    {
                        count += 1;
                        generatedList.Add(numStr);
                        int[] outputs = null;
                        rbce.ReadBranchCLIFunc(new int[] { num }, ref outputs, sutIndex);
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
                totalTriggeringProbilities = totalTriggeringProbilities
                    .Add(Vector<double>.Build.Dense(triggeringProbilities));

                Console.WriteLine(i);
            }
        }
        public void sutquicksort()
        {
            // All dimension variable: [0,1]

            sutIndex = 7;
            totalNumberOfBins = (int)Math.Pow(2,12);
            numOfMinIntervalInAllDim = (int)(Math.Pow(2, 20) / totalNumberOfBins);
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            double sampleProbability = 0.1;
            int sampleSize = (int)(numOfMinIntervalInAllDim * sampleProbability);
            numOfLabels = 23;
            Vector<double> totalTriggeringProbilities = Vector<double>.Build.Dense(23);

            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].BinIndex = i;
                bins[i].SetIndexPlus1 = -1;
                int lowBound = i * numOfMinIntervalInAllDim;
                int highBound = (i + 1) * numOfMinIntervalInAllDim;
                readBranch rbce = new readBranch();
                int count = 0;
                List<string> generatedList = new List<string>();
                int[] countsArray = new int[numOfLabels];

                while (count < sampleSize)
                {
                    int num = GlobalVar.rnd.Next(lowBound, highBound);
                    int[] inputs = new int[20];
                    for (int j = 19; j >= 0; j--)
                    {
                        int a = num / (int)Math.Pow(2, j);
                        num = num % (int)Math.Pow(2, j);
                        inputs[j] = a;
                    }
                    var listStr = inputs.Select(x => x.ToString());
                    string inputStr = null;
                    foreach (string s in listStr)
                    {
                        inputStr += s + ";";
                    }
                    if (generatedList.Where(t => (t == inputStr) == true).Count() == 0)
                    {
                        count += 1;
                        generatedList.Add(inputStr);
                        int[] outputs = null;
                        try
                        {
                            rbce.ReadBranchCLIFunc(inputs, ref outputs, sutIndex);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
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
                totalTriggeringProbilities = totalTriggeringProbilities
                    .Add(Vector<double>.Build.Dense(triggeringProbilities));

                Console.WriteLine(i);
            }
            List<int> zeroTriIndexes = totalTriggeringProbilities.Select((x, i) =>
            {
                if (x == 0)
                {
                    return i;
                }
                return -1;
            }).Where(i => i != -1).ToList();
            numOfLabels = 23 - zeroTriIndexes.Count;
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                int count = 0;
                double[] newProbInBins = new double[numOfLabels];
                for (int j = 0; j < bins[i].ProbInBins.Count(); j++)
                {
                    if (!zeroTriIndexes.Contains(j))
                    {
                        newProbInBins[count] = bins[i].ProbInBins[j];
                        count += 1;
                    }
                }
                bins[i].ProbInBins = newProbInBins;
            }
        }
#endregion

        public void setPartBinsToBinSutsichneu(Pair<int, int, double[]>[] thebins)
        {
            bins = thebins;
            numOfLabels = bins[0].ProbInBins.Length;
        }
        public List<Pair<int, int, double[]>[]> sutnsichneu(int partitionNum)
        {
            // All dimension variable: [1,2,3]

            sutIndex = 4;
            totalNumberOfBins = (int)Math.Pow(3, 8);
            numOfMinIntervalInAllDim = (int)Math.Pow(3, 8) / totalNumberOfBins; ///(int)(((1-Math.Pow(3,14+1))/(1-3))/ totalNumberOfBins);
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            double sampleProbability = 1;
            int sampleSize = (int)(numOfMinIntervalInAllDim * sampleProbability);
            numOfLabels = 502;
            Vector<double> totalTriggeringProbilities = Vector<double>.Build.Dense(502);

            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].BinIndex = i;
                bins[i].SetIndexPlus1 = -1;
                int lowBound = i * numOfMinIntervalInAllDim;
                int highBound = (i + 1) * numOfMinIntervalInAllDim;
                readBranch rbce = new readBranch();
                int count = 0;
                List<string> generatedList = new List<string>();
                int[] countsArray = new int[numOfLabels];

                while (count < sampleSize)
                {
                    int num = GlobalVar.rnd.Next(lowBound, highBound);
                    int tmp = num;
                    int[] inputs = new int[8];
                    num = tmp;
                    for (int j = 7; j >= 0; j--)
                    {
                        int a = num / (int)Math.Pow(3, j);
                        num = num % (int)Math.Pow(3, j);
                        inputs[j] = a;
                    }
                    if (inputs.Contains(3) == true)
                    {
                        Console.WriteLine();
                    }
                    var listStr = inputs.Select(x => x.ToString());
                    string inputStr = null;
                    foreach (string s in listStr)
                    {
                        inputStr += s + ";";
                    }
                    if (generatedList.Where(t => (t == inputStr) == true).Count() == 0)
                    {
                        count += 1;
                        generatedList.Add(inputStr);
                        int[] outputs = null;
                        int[] inputsTemp = new int[14];
                        for (int j = 0; j < 8; j++)
                        {
                            inputsTemp[j] = inputs[j];
                        }
                        rbce.ReadBranchCLIFunc(inputsTemp, ref outputs, sutIndex);
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
                totalTriggeringProbilities = totalTriggeringProbilities
                    .Add(Vector<double>.Build.Dense(triggeringProbilities));

                Console.WriteLine(i);
            }
            totalTriggeringProbilities = totalTriggeringProbilities.Divide(totalNumberOfBins);
            List<int> zeroOrlessTriIndexes = totalTriggeringProbilities.Select((x, i) =>
             {
                 if (x == 0 || x > 0.04)
                 {
                     return i;
                 }
                 return -1;
             }).Where(i => i != -1).ToList();
            Double min = totalTriggeringProbilities.Where(x => x != 0).Min();
            //double minTri = totalTriggeringProbilities.Where(x=> x != 0).Min();
            //List<int> zeroOrlessTriIndexes = totalTriggeringProbilities.Select((x, i) =>
            //{
            //    if (x == 0 || x > minTri)
            //    {
            //        return i;
            //    }
            //    return -1;
            //}).Where(i => i != -1).ToList();
            numOfLabels = 502 - zeroOrlessTriIndexes.Count;
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                int count = 0;
                double[] newProbInBins = new double[numOfLabels];
                for (int j = 0; j < bins[i].ProbInBins.Count(); j++)
                {
                    if (!zeroOrlessTriIndexes.Contains(j))
                    {
                        newProbInBins[count] = bins[i].ProbInBins[j];
                        count += 1;
                    }
                }
                bins[i].ProbInBins = newProbInBins;
            }

            List<Pair<int, int, double[]>[]> listPartBins = new List<Pair<int, int, double[]>[]>();

            int numofcePartition = numOfLabels / partitionNum;
            int lastNumofcePartition = numofcePartition + numOfLabels % partitionNum;
            for (int i = 0; i < partitionNum; i++)
            {
                var newPartBin = Copy.DeepCopy(bins);
                listPartBins.Add(newPartBin);
                int numOfcePart = i == partitionNum - 1 ? lastNumofcePartition : numofcePartition;
                for (int j = 0; j < newPartBin.Length; j++)
                {
                    double[] newPartTriProbAry = new double[numOfcePart];
                    Array.Copy(newPartBin[j].ProbInBins, i * numofcePartition, newPartTriProbAry, 0 ,numOfcePart);
                    newPartBin[j].ProbInBins = newPartTriProbAry;
                }
            }
            return listPartBins;
        }
        public void sutBestMove()
        {
            sutIndex = 3;
            numOfMinIntervalX = 32;
            numOfMinIntervalY = 32;
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
                        rbce.ReadBranchCLIFunc(inputs, ref outputs, sutIndex);
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

        public DataTable MapTestInputsToBinsNichneu()
        {
            DataTable InputsBinTable = new DataTable();
            for (int i = 0; i < 14; i++)
            {
                InputsBinTable.Columns.Add("x"+i.ToString(), Type.GetType("System.Int32"));
            }
            InputsBinTable.Columns.Add("IntentionallyBlank", Type.GetType("System.Int32"));
            InputsBinTable.Columns.Add("BinIndexFrom1", Type.GetType("System.Int32"));
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                int lowBound = i * numOfMinIntervalInAllDim;
                int highBound = (i + 1) * numOfMinIntervalInAllDim;
                for (int j = lowBound; j < highBound; j++)
                {
                    int[] inputs = new int[14];
                    for (int k = 13; k >= 0; k--)
                    {
                        int a = j / (int)Math.Pow(3, k);
                        j = j % (int)Math.Pow(3, k);
                        inputs[j] = a;
                    }
                    object[] rowData = new object[] {
                        inputs[0], inputs[1],inputs[2],inputs[3],
                        inputs[4],inputs[5],inputs[6],inputs[7],
                        inputs[8],inputs[9],inputs[10],inputs[11],
                        inputs[12],inputs[13],
                        -9999999, i + 1 };
                    var row = InputsBinTable.NewRow();
                    row.ItemArray = rowData;
                    InputsBinTable.Rows.Add(row);
                }
            }
            return InputsBinTable;
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

        public void sutmatrixinverse()
        {
            // All dimension variable:  [0,6] ->[-3,3]
            // inputs = inputs - 3
            sutIndex = 8;
            totalNumberOfBins = (int)Math.Pow(7, 6);
            numOfMinIntervalInAllDim = (int)(Math.Pow(7, 9) / totalNumberOfBins);
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            double sampleProbability = 0.1;
            int sampleSize = (int)(numOfMinIntervalInAllDim * sampleProbability);
            numOfLabels = 28;
            Vector<double> totalTriggeringProbilities = Vector<double>.Build.Dense(numOfLabels);

            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].BinIndex = i;
                bins[i].SetIndexPlus1 = -1;
                int lowBound = i * numOfMinIntervalInAllDim;
                int highBound = (i + 1) * numOfMinIntervalInAllDim;
                readBranch rbce = new readBranch();
                int count = 0;
                List<string> generatedList = new List<string>();
                int[] countsArray = new int[numOfLabels];

                while (count < sampleSize)
                {
                    int num = GlobalVar.rnd.Next(lowBound, highBound);
                    int[] inputs = new int[9];
                    for (int j = 8; j >= 0; j--)
                    {
                        int a = num / (int)Math.Pow(7, j);
                        num = num % (int)Math.Pow(7, j);
                        inputs[j] = a - 3;
                    }
                    var listStr = inputs.Select(x => x.ToString());
                    string inputStr = null;
                    foreach (string s in listStr)
                    {
                        inputStr += s + ";";
                    }
                    if (generatedList.Where(t => (t == inputStr) == true).Count() == 0)
                    {
                        count += 1;
                        generatedList.Add(inputStr);
                        int[] outputs = null;
                        try
                        {
                            rbce.ReadBranchCLIFunc(inputs, ref outputs, sutIndex);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
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
                totalTriggeringProbilities = totalTriggeringProbilities
                    .Add(Vector<double>.Build.Dense(triggeringProbilities));

                Console.WriteLine(i);
            }
            List<int> zeroTriIndexes = totalTriggeringProbilities.Select((x, i) =>
            {
                if (x == 0)
                {
                    return i;
                }
                return -1;
            }).Where(i => i != -1).ToList();
            numOfLabels = numOfLabels - zeroTriIndexes.Count;
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                int count = 0;
                double[] newProbInBins = new double[numOfLabels];
                for (int j = 0; j < bins[i].ProbInBins.Count(); j++)
                {
                    if (!zeroTriIndexes.Contains(j))
                    {
                        newProbInBins[count] = bins[i].ProbInBins[j];
                        count += 1;
                    }
                }
                bins[i].ProbInBins = newProbInBins;
            }
        }

        public void sutTriangle()
        {
            // 3 inputs for all :[0,9]
            // inputs = inputs - 3
            sutIndex = 0;
            totalNumberOfBins = 10  ;// (int)Math.Pow(10, 3);
            numOfMinIntervalInAllDim = (int)(Math.Pow(10, 3) / totalNumberOfBins);
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            double sampleProbability = 1.0;
            int sampleSize = (int)(numOfMinIntervalInAllDim * sampleProbability);
            numOfLabels = 28;
            Vector<double> totalTriggeringProbilities = Vector<double>.Build.Dense(numOfLabels);

            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].BinIndex = i;
                bins[i].SetIndexPlus1 = -1;
                int lowBound = i * numOfMinIntervalInAllDim;
                int highBound = (i + 1) * numOfMinIntervalInAllDim;
                readBranch rbce = new readBranch();
                int count = 0;
                List<string> generatedList = new List<string>();
                int[] countsArray = new int[numOfLabels];

                while (count < sampleSize)
                {
                    int num = GlobalVar.rnd.Next(lowBound, highBound);
                    int[] inputs = new int[3];
                    for (int j = 2; j >= 0; j--)
                    {
                        int a = num / (int)Math.Pow(10, j);
                        num = num % (int)Math.Pow(10, j);
                        inputs[j] = a - 3;
                    }
                    var listStr = inputs.Select(x => x.ToString());
                    string inputStr = null;
                    foreach (string s in listStr)
                    {
                        inputStr += s + ";";
                    }
                    if (generatedList.Where(t => (t == inputStr) == true).Count() == 0)
                    {
                        count += 1;
                        generatedList.Add(inputStr);
                        int[] outputs = null;
                        try
                        {
                            rbce.ReadBranchCLIFunc(inputs, ref outputs, sutIndex);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
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
                totalTriggeringProbilities = totalTriggeringProbilities
                    .Add(Vector<double>.Build.Dense(triggeringProbilities));

                Console.WriteLine(i);
            }
            double trilowUniform = totalTriggeringProbilities.Divide(totalNumberOfBins).Where(x=>x!=0).Min();
            List<int> zeroTriIndexes = totalTriggeringProbilities.Select((x, i) =>
            {
                if (x == 0)
                {
                    return i;
                }
                return -1;
            }).Where(i => i != -1).ToList();
            numOfLabels = numOfLabels - zeroTriIndexes.Count;
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                int count = 0;
                double[] newProbInBins = new double[numOfLabels];
                for (int j = 0; j < bins[i].ProbInBins.Count(); j++)
                {
                    if (!zeroTriIndexes.Contains(j))
                    {
                        newProbInBins[count] = bins[i].ProbInBins[j];
                        count += 1;
                    }
                }
                bins[i].ProbInBins = newProbInBins;
            }
        }
    }
}
