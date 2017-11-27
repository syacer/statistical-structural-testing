using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static class ProcessMutationScore
    {
        static double[] RetriveMutationScore(string path)
        {
            LocalFileAccess lfa = new LocalFileAccess();
            List<string> linesStr = new List<string>();
            lfa.StoreLinesToList(path,linesStr);
            double[] scores = new double[linesStr.Count];
            for (int i = 0; i < linesStr.Count; i++)
            {
                string line = linesStr[i];
                var substrings = line.Split(' ');
                int numOftestcases = Convert.ToInt32(substrings[0]);
                double score = Convert.ToDouble(substrings[1]);
                scores[i] = score;
            }
            return scores;
        }

        public static void ConvertMutationScoreToExcel(string root, string name, int numOfFiles)
        {
            string path;
            DataTable dt = new DataTable();
            object[] rowData = new object[200];
            for (int j = 0; j < 200; j++)
            {
                dt.Columns.Add(j.ToString(), Type.GetType("System.Double"));
            }
            for (int i = 0; i < numOfFiles; i++)
            {
                Array.Clear(rowData,0,rowData.Length);
                path = root + name + (i + 1).ToString();
                double[] scores = RetriveMutationScore(path);
                for (int j = 0; j < 200; j++)
                {
                    if (j < scores.Length-1)
                    {
                        rowData[j] = (object)scores[j];
                    }
                    if (j == scores.Length - 1)
                    {
                        rowData[199] = (object)scores[scores.Length - 1];
                    }
                }
                var row = dt.NewRow();
                row.ItemArray = rowData;
                dt.Rows.Add(row);
            }

            string filePath = root + "mtScores.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dt }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dt }, true, filePath, false);
            }
        }

    }
}
