using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using ReadSUTBranchCEData;

namespace GADEApproach.TrainditionalApproaches
{
    public class TestDataGeneration2:TestDataGeneration
    {
        public TestDataGeneration2(string testdataFilePath, DataTable inputSetMapDt, int testSize, double[] SetProbAry, SUT sut) : base(testdataFilePath, inputSetMapDt, testSize, SetProbAry,sut)
        {
        }

        static public double InputProbability(int[] inputs, Matrix<double> weightsMatrix, SUT sut)
        {
            double probability = 1;
            for (int i = 0; i < weightsMatrix.ColumnCount; i++)
            {
                var columni = weightsMatrix.Column(i);
                var intervalSize = (sut.highbounds[i] - sut.lowbounds[i] + 1)
                    / sut.numOfMinIntervalInAllDim;
                var index = (int)(inputs[i] / intervalSize);
                if (columni.Sum() == 0)
                {
                    probability = 0;
                }
                else
                {
                    probability *= (columni[index] / columni.Sum()) / intervalSize;
                }
            }
            return probability;
        }

        static public int[] StringToInt(string input, SUT sut)
        {
            int[] numsInt = new int[sut.lowbounds.Length];
            string[] nums = input.Split(' ');
            for (int i = 0; i < nums.Length; i++)
            {
                numsInt[i] = Convert.ToInt32(nums[i]);
            }
            return numsInt;
        }
        static public string IntToString(int[] input)
        {
            string num = null;
            for (int i = 0; i < input.Length; i++)
            {
                num += input[i].ToString() + " ";
            }
            num = num.Substring(0, num.Length - 1);
            return num;
        }

        public static int[] GenerateInput(Matrix<double> weightsMatrix, SUT sut)
        {
            double[] sutLowbounds = sut.lowbounds;
            double[] sutHighbounds = sut.highbounds;
            int sutNumOfIntervals = sut.numOfMinIntervalInAllDim;
            List<int> inputs = new List<int>();
            // Randomly select a interval from each dimension
            for (int c = 0; c < weightsMatrix.ColumnCount; c++)
            {
                Vector<double> colVect = weightsMatrix.Column(c);
                double sum = weightsMatrix.Column(c).Sum();
                int rndValue = GlobalVar.rnd.Next(0, (int)Math.Floor(sum));
                double accumulation = 0;
                int isInterval = -1;
                for (int i = 0; i < colVect.Count; i++)
                {
                    if (rndValue >= accumulation && rndValue <= colVect[i] + accumulation)
                    {
                        isInterval = i;
                        break;
                    }
                    else
                    {
                        accumulation += colVect[i];
                    }
                }
                int intervalSize = (int)((sutHighbounds[c] - sutLowbounds[c] + 1) / sut.numOfMinIntervalInAllDim);
                int boundL = (int)sutLowbounds[c] + isInterval * intervalSize;
                int boundH = (int)sutLowbounds[c] + (isInterval + 1) * intervalSize;
                inputs.Add(GlobalVar.rnd.Next(boundL,boundH));
            }

            return inputs.ToArray();
        }

        public void testDataGenerationBestMove2(
            List<Matrix<double>> weightMatrices, SUT sut, string rootPath)
        {
            string functionName = @"bestMove";
            List<int[]> testSet = RoundRobinSampling.RRSampling(weightMatrices,sut,_testSize);

            for (int i = 0; i < testSet.Count; i++)
            {
                testFileForMiLu(functionName, i, testSet[i], _testdataFilePath);

            }
        }
    }
}
