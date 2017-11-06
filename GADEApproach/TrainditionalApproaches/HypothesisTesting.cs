using Accord.Statistics.Testing;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach.TrainditionalApproaches
{
   static public class HypothesisTesting
    {
        static public int RankSumTest(double[] dataSet1, double[] dataSet2)
        {
            try
            {
                MannWhitneyWilcoxonTest test = new MannWhitneyWilcoxonTest(dataSet1, dataSet2,TwoSampleHypothesis.ValuesAreDifferent);
                if (test.Significant == false)
                {
                    return 0;
                }
                double sum1 = test.RankSum1;
                double sum2 = test.RankSum2;

                double statistic1 = test.Statistic1; 
                double statistic2 = test.Statistic2; 

                double pvalue = test.PValue;

                //Perform A-Test
                double a12 = ((sum1 / test.NumberOfSamples1) - (test.NumberOfSamples1 + 1) / 2) / test.NumberOfSamples2;
                if (a12 > 0.65)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            catch
            {
                var stat1 = new DescriptiveStatistics(dataSet1);
                var stat2 = new DescriptiveStatistics(dataSet2);
                if (stat1.StandardDeviation == stat2.StandardDeviation)
                {
                    return 0;
                }
                else if (stat1.StandardDeviation == 0)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
}
