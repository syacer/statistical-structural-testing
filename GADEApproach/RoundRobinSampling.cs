using GADEApproach.TrainditionalApproaches;
using MathNet.Numerics.LinearAlgebra;
using ReadSUTBranchCEData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static class RoundRobinSampling
    {
        public static List<int[]> RRSampling(
            List<Matrix<double>> weightMatrices,
            SUT sut,
            int sampleSize
            )
        {
            int count = 0;
            readBranch rbce = new readBranch();
            List<int[]> testSet = new List<int[]>();
            for (int i = 0; i < sampleSize; i++)
            {
                int[] input;
                int retryCounter = 0;
                while (true)
                {
                    int[] output = null;
                    input = TestDataGeneration2
                        .GenerateInput(weightMatrices[count], sut);
                    rbce.ReadBranchCLIFunc(input,ref output,sut.sutIndex);
                    if (output[count] == 1 
                        && testSet.Where(x=>x.SequenceEqual(input)).Count() == 0)
                    {
                        testSet.Add(input);
                        break;
                    }
                    if (output[count] == 0
                        || testSet.Where(x => x.SequenceEqual(input)).Count() != 0)
                    {
                        retryCounter += 1;
                    }
                    if (retryCounter > 5000)
                    {
                        Console.WriteLine("NumOfSamples: {0}; Retry Counter Expires at 5000",i);
                        break;
                    }
                }
                count = (count + 1) % sut.numOfLabels;
            }
            return testSet;
        }
    }
}
