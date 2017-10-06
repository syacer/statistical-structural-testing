using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    public class SyntheticSUT
    {
        public double[] lowbounds = new double[] { 0.0, 0.0 };
        public double[] highbounds = new double[] { 99, 99 };
        public Matrix<double> labelMatrix;

         public void SUTA(int numOfLabels = 20)
        {
            //ExperimentA reveal the performance impact under the number of Labels
            //Non-Concutive Sets = false
            //Entropy = log_2(M) sizes equal

            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));

            int num = (((int)(highbounds[1] - lowbounds[1]) + 1)
                * ((int)(highbounds[0] - lowbounds[0]) + 1)) / numOfLabels;
            for (int ln = 1; ln <= numOfLabels; ln++)
            {
                int count = 0;
                while (count < num)
                {
                    int x = GlobalVar.rnd.Next(0, (int)(highbounds[0] - lowbounds[0])+1);
                    int y = GlobalVar.rnd.Next(0, (int)(highbounds[0] - lowbounds[0])+1);
                    if (labelMatrix[x, y] == 0)
                    {
                        labelMatrix[x, y] = ln;
                        count++;
                        Console.Write(" {0}",count);
                    }
                }
            }
        }
         public double[] GenerateProbabilitiesFromEntropy(int numOfLabels = 20, double entropy = 4.32)
        {
            // ExperimentB: Given numOfLabels and entropy, Generate an SUT
            int numOfTestInputs = (int)(highbounds[0] - lowbounds[0] + 1)
                * (int)(highbounds[1] - lowbounds[1] + 1);
            // Generate label propotion
            double[] probList = Enumerable
                .Repeat(1.0 / numOfTestInputs, numOfLabels)
                .ToArray();
            while (true)
            {
                double ret = ProbabilityFinder(probList, 0, entropy);
                if (ret == 1.0)
                {
                    break;
                }
            }

            return probList;
        }

         public double ProbabilityFinder(double[] probabilityArray, int index, double entropy)
        {

            //Stop when 1. no remain prob. return 1;
            //2. number of labels reached the max return 1;
            //3. if it can't find the valid prob,ie. in the below range and timeout
            //   return -1
            // The generated probability should make entropy - newEntropy > 0
            // Should no smaller than maximum of the sum of the rest infromation
            // After successfully generate a prob., call itself and pass in next index
            int numOfLabels = probabilityArray.Length;
            while (true)
            {
                if (index == numOfLabels - 1)
                {
                    double probLeft = 1 - probabilityArray.Sum();
                    probabilityArray[index] += probLeft;
                    return 1.0;
                }
                double sum = probabilityArray.Sum();
                if (Math.Round(sum, 4) == 1)
                {
                    return 1.0;
                }
                int retryTimes = 2000;
                bool success = false;
                double rnd = -1;
                while (retryTimes > 0)
                {
                    retryTimes -= 1;
                    rnd = GlobalVar.rnd.NextDouble() * (1 - sum);
                    double[] tmpProbList = Copy.DeepCopy(
                        probabilityArray);
                    tmpProbList[index] += rnd;
                    double testEntropy = EntropyCalculation(tmpProbList);
                    double remainProb = 1 - sum - rnd;
                    double[] arrayOfProbabilities = Enumerable.Repeat(
                        remainProb / (numOfLabels - index - 1), numOfLabels - index - 1).ToArray();
                    var tmpProbList2 = tmpProbList.Select((x, i) =>
                    {
                        if (i > index)
                        {
                            return x + arrayOfProbabilities[i - index - 1];
                        }
                        return x;
                    }).ToArray();
                    double maxEntTest = EntropyCalculation(tmpProbList2);

                    if (entropy > testEntropy && maxEntTest > entropy)
                    {
                        probabilityArray[index] += rnd;
                        success = true;
                        break;
                    }
                }
                double ret = -2;
                if (success == true)
                {
                    Console.WriteLine(index);
                    ret = ProbabilityFinder(probabilityArray, index + 1, entropy);
                    if (ret == 1.0)
                    {
                        return 1.0;
                    }
                    else
                    {
                        probabilityArray[index] -= rnd;
                    }
                }
                else
                {
                    return -1;
                }

            }
        }
        //Experiment B: Non-Concecutive Bins -Randomly Shuffered
        public SyntheticSUT SUTB(int numOfLabels = 20, double entropy = 4.32)
        {
            double[] probList = GenerateProbabilitiesFromEntropy(numOfLabels, entropy);

            double actualEntropy = EntropyCalculation(probList);
            double errorCheckPoint = entropy - actualEntropy;

            // Generate labels for input domain space
            int[] numOfInputsPerLabel = probList.Select(x => Convert.ToInt32(Math.Round(x 
            *(highbounds[1] - lowbounds[1] + 1)
            * (highbounds[0] - lowbounds[0] + 1)))).ToArray();
            int totalInputs = (int)(highbounds[1] - lowbounds[1] + 1) * (int)(highbounds[0] - lowbounds[0] + 1);

            int sumOfnumOfInputsPerLabel = numOfInputsPerLabel.Sum();
            numOfInputsPerLabel = numOfInputsPerLabel.Select(x =>
            {
                if (x == 0)
                {
                    return x + 1;
                }
                return x;
            }
            ).ToArray();
            for (int i = 0; i < Math.Abs(totalInputs - sumOfnumOfInputsPerLabel); i++)
            {
                int index = -1;
                if (totalInputs - sumOfnumOfInputsPerLabel > 0)
                {
                    index = GlobalVar.rnd.Next(0, numOfInputsPerLabel.Length);
                    numOfInputsPerLabel[index] += 1;

                }
                else
                {
                    while (true)
                    {
                        index = GlobalVar.rnd.Next(0, numOfInputsPerLabel.Length);
                        if (numOfInputsPerLabel[index] > 1)
                        {
                            numOfInputsPerLabel[index] -= 1;
                            break;
                        }
                    }
                }
            }
            double entCheckPoint = EntropyCalculation(numOfInputsPerLabel.Select(x => x * 1.0 / 10000).ToArray());
            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));

            for (int ln = 1; ln <= numOfLabels; ln++)
            {
                int count2 = 0;
                while (count2 < numOfInputsPerLabel[ln-1])
                {
                    int x = GlobalVar.rnd.Next(0, (int)(highbounds[0] - lowbounds[0]) + 1);
                    int y = GlobalVar.rnd.Next(0, (int)(highbounds[0] - lowbounds[0]) + 1);
                    bool valid = labelMatrix.ToColumnArrays().Any(t=>t.Any(m=>m==0) == true) ? true : false;
                    if (valid == false)
                    {
                        Console.Write("Sum Of numOfInputsPerLabel not equal to total # of inputs");
                    }
                    if (labelMatrix[x, y] == 0)
                    {
                        labelMatrix[x, y] = ln;
                        count2++;
                    }
                }
                Console.Write("Label #{0}, Count #{1}", ln+1,count2);
            }

            for (int i = 0; i < numOfLabels; i++)
            {
                Console.WriteLine("{0} test inputs in {1}",numOfInputsPerLabel[i],i+1);
            }
            return this;
        }
        //Experiment C: Concecutive Bins
        public SyntheticSUT SUTC(int numOfLabels = 20, double entropy = 4.32)
        {
            double[] probList = GenerateProbabilitiesFromEntropy(numOfLabels, entropy);

            double actualEntropy = EntropyCalculation(probList);
            double errorCheckPoint = entropy - actualEntropy;

            // Generate labels for input domain space
            int[] numOfInputsPerLabel = probList.Select(x => Convert.ToInt32(Math.Round(x
            * (highbounds[1] - lowbounds[1] + 1)
            * (highbounds[0] - lowbounds[0] + 1)))).ToArray();
            int totalInputs = (int)(highbounds[1] - lowbounds[1] + 1) * (int)(highbounds[0] - lowbounds[0] + 1);

            int sumOfnumOfInputsPerLabel = numOfInputsPerLabel.Sum();
            numOfInputsPerLabel = numOfInputsPerLabel.Select(x =>
            {
                if (x == 0)
                {
                    return x + 1;
                }
                return x;
            }
            ).ToArray();
            for (int i = 0; i < Math.Abs(totalInputs - sumOfnumOfInputsPerLabel); i++)
            {
                int index = -1;
                if (totalInputs - sumOfnumOfInputsPerLabel > 0)
                {
                    index = GlobalVar.rnd.Next(0, numOfInputsPerLabel.Length);
                    numOfInputsPerLabel[index] += 1;
                }
                else
                {
                    while (true)
                    {
                        index = GlobalVar.rnd.Next(0, numOfInputsPerLabel.Length);
                        if (numOfInputsPerLabel[index] > 1)
                        {
                            numOfInputsPerLabel[index] -= 1;
                            break;
                        }
                    }
                }
            }
            double entCheckPoint = EntropyCalculation(numOfInputsPerLabel.Select(x => x * 1.0 / 10000).ToArray());
            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));
            int accumulateCount = 0;
            int oldyIndex = 0;
            int xMode = 0;
            for (int ln = 1; ln <= numOfLabels; ln++)
            {
                for (int i = 0; i < numOfInputsPerLabel[ln - 1]; i++)
                {
                    int index = accumulateCount + i;
                    int xIndex = -1;
                    int yIndex = index / (int)(highbounds[1] - lowbounds[1] + 1);

                    if (oldyIndex != yIndex)
                    {
                        xMode = xMode == 1 ? 0 : 1;
                        oldyIndex = yIndex;
                    }
                    if (xMode == 0)
                    {
                        xIndex = index % (int)(highbounds[0] - lowbounds[0] + 1);
                    }
                    else
                    {
                        xIndex = (int)(highbounds[0] - lowbounds[0])
                            - index % (int)(highbounds[0] - lowbounds[0] + 1);
                    }
                    labelMatrix[xIndex, yIndex] = ln;
                }
                accumulateCount += numOfInputsPerLabel[ln-1];
            }
            for (int i = 0; i < numOfLabels; i++)
            {
                Console.WriteLine("{0} test inputs in {1}", numOfInputsPerLabel[i], i + 1);
            }

            return this;
        }
        double EntropyCalculation(double[] probabilities)
        {
            double entropy = 0;
            foreach (double p in probabilities)
            {
                if (p != 0.0 && p != 1.0)
                {
                    entropy += p * Math.Log(1 / p, 2);
                }
            }
            return entropy;
        }

        public void InputDomainToExcel(string path)
        {
            DataTable labelDt = new DataTable();

            for (int i = 0; i < highbounds[0] - lowbounds[0] + 1; i++)
            {
                labelDt.Columns.Add(i.ToString(), Type.GetType("System.Double"));
            }

            for (int i = 0; i < labelMatrix.RowCount; i++)
            {
                var row = labelDt.NewRow();
                row.ItemArray = labelMatrix.Row(labelMatrix.RowCount - i - 1).Select(x => (object)x).ToArray();
                labelDt.Rows.Add(row);
            }

            DateTime time = DateTime.Now;
            string format = "MM-dd-HH-mm-ss";
            string timeStr = time.ToString(format);
            string filePath = path;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            ExcelOperation.dataTableListToExcel(new List<DataTable>() { labelDt }, false, filePath, true);
            Console.WriteLine("Finish Write Labels to Excel");
        }
    }
}
