using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static class ReadinDataOutputExcel
    {
        static public void StartDataProcessing()
        {
            GenerateStatisticsInExcelForDataPerNumOfLabels(8, @"\\khensu\Home06\yangs\Desktop\Non-Concutive");
            GenerateStatisticsInExcelForDataPerNumOfLabels(4, @"\\khensu\Home06\yangs\Desktop\Non-Concutive");
            GenerateStatisticsInExcelForDataPerNumOfLabels(2, @"\\khensu\Home06\yangs\Desktop\Non-Concutive");
            GenerateStatisticsInExcelForDataPerNumOfLabels(10, @"\\khensu\Home06\yangs\Desktop\Non-Concutive");
        }
        static public void GenerateStatisticsInExcelForDataPerNumOfLabels(int numOfLabels, string rootPath)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Max", Type.GetType("System.Double"));
            dt.Columns.Add("Q3", Type.GetType("System.Double"));
            dt.Columns.Add("Median", Type.GetType("System.Double"));
            dt.Columns.Add("Q1", Type.GetType("System.Double"));
            dt.Columns.Add("Min", Type.GetType("System.Double"));
            dt.Columns.Add("Avg", Type.GetType("System.Double"));

            string[] Files = Directory.GetFiles(rootPath);
            string[] targetFileNames = Files.Where(x => x.Contains("Data_" + numOfLabels.ToString()) == true).ToArray();
            for (int i = 0; i < targetFileNames.Length; i++)
            {
                Console.WriteLine(targetFileNames[i]);
                var statistics = ProcessDataExcel(targetFileNames[i]);
                DataRow dr = dt.NewRow();
                object[] statData = new object[6]
                    {
                        statistics.Item1,
                        statistics.Item2,
                        statistics.Item3,
                        statistics.Item4,
                        statistics.Item5,
                        statistics.Item6
                    };
                dr.ItemArray = statData;
                dt.Rows.Add(dr);
            }
            ExcelOperation.dataTableListToExcel(new List<DataTable>() { dt }, true,
                @"\\khensu\Home06\yangs\Desktop\Non-Concutive" + "stat_" + numOfLabels.ToString() + ".xlsx", true);

        }
        static public Tuple<double, double, double, double, double, double> ProcessDataExcel(string filePath)
        {
            DataSet ds = ExcelOperation.ReadExcelFile(filePath);
            DataTable dt = ds.Tables[1];
            var enumerableDt = dt.AsEnumerable();
            List<double> goodnessOfFitRuns = new List<double>();
            for (int i = 0; i < dt.Columns.Count / 2; i++)
            {
                if ((double)dt.Rows[2].ItemArray[2 * i] != 0)
                {
                    double finalGoodnessOfFit = (double)enumerableDt.Min(x =>
                    {
                        if ((double)x.ItemArray[2 * i] != 0)
                        {
                            return (double)x.ItemArray[2 * i];
                        }
                        else
                        {
                            return 99999.0;
                        }
                    });
                    goodnessOfFitRuns.Add(finalGoodnessOfFit);
                    Console.WriteLine("Run:#{0}, GOF: #{1}", i, finalGoodnessOfFit);
                }
            }

            double median = -1;
            double Q1 = -1;
            double Q3 = -1;
            goodnessOfFitRuns.Sort();
            if (goodnessOfFitRuns.Count % 2 == 0)
            {
                double medianA = goodnessOfFitRuns[goodnessOfFitRuns.Count / 2 - 1];
                double medianB = goodnessOfFitRuns[goodnessOfFitRuns.Count / 2];
                median = (medianA + medianB) / 2;
            }
            else
            {
                median = goodnessOfFitRuns[goodnessOfFitRuns.Count / 2];
            }
            if ((goodnessOfFitRuns.Count / 2) % 2 == 0)
            {
                double Q1A = goodnessOfFitRuns[(goodnessOfFitRuns.Count / 2) / 2 - 1];
                double Q1B = goodnessOfFitRuns[(goodnessOfFitRuns.Count / 2) / 2];
                Q1 = (Q1A + Q1B) / 2;
                double Q3A = goodnessOfFitRuns[(goodnessOfFitRuns.Count / 2) + (goodnessOfFitRuns.Count / 2) / 2 - 1];
                double Q3B = goodnessOfFitRuns[(goodnessOfFitRuns.Count / 2) + (goodnessOfFitRuns.Count / 2) / 2];
                Q3 = (Q3A + Q3B) / 2;
            }
            else
            {
                Q1 = goodnessOfFitRuns[(goodnessOfFitRuns.Count / 2) / 2];
                Q3 = goodnessOfFitRuns[(goodnessOfFitRuns.Count / 2) / 2 + (goodnessOfFitRuns.Count / 2)];
            }
            double avg = goodnessOfFitRuns.Average();

            List<double> outLiers = new List<double>();
            double lowLimit = Q1 - 1.5 * (Q3 - Q1);
            double highLimit = Q3 + 1.5 * (Q3 - Q1);
            outLiers = goodnessOfFitRuns.Where(x => x < lowLimit || x > highLimit).ToList();

            for (int i = 0; i < outLiers.Count; i++)
            {
                goodnessOfFitRuns.Remove(outLiers[i]);
            }

            double max = goodnessOfFitRuns.Max();
            double min = goodnessOfFitRuns.Min();

            return new Tuple<double, double, double, double, double, double>(max, Q3, median, Q1, min, avg);
        }

        static public DataSet FakeReadExcelFiles()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.Columns.Add();
            dt.Columns.Add();
            DataRow dr = dt.NewRow();
            dr.ItemArray = new object[2] { "good", "day" };
            dt.Rows.Add(dr);
            ds.Tables.Add(dt);
            return ds;
        }
    }
}
