using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeterministicApproach_GA
{
    public class Record
    {
       public DataTable fitRecord;
       public DataTable solRecord;
       public int[] currentGen;
       public bool[] updateIndicate;
       public Stopwatch gaWatch = new Stopwatch();
       public Stopwatch warmUpWatch = new Stopwatch();
       public Dictionary<string, object> sutInfo;
        public Record(int numOfRuns)
        {
            warmUpWatch.Start();
            fitRecord = new DataTable();
            solRecord = new DataTable();
            fitRecord.Columns.Add("Generation",typeof(int));
            solRecord.Columns.Add("Generation",typeof(int));
            currentGen = new int[numOfRuns];
            updateIndicate = new bool[numOfRuns];
            for (int i = 0; i < numOfRuns; i++)
            {
                fitRecord.Columns.Add(i.ToString(),typeof(double));
                solRecord.Columns.Add(i.ToString(), typeof(string));
            }
            object[] allZero = new object[numOfRuns+1];
            for (int i = 0; i < numOfRuns + 1; i++)
            {
                allZero[i] = 0;
            }
            for (int i = 0; i < 1000; i++)
            {
                allZero[0] = i; 
                fitRecord.Rows.Add(allZero);
                solRecord.Rows.Add(allZero);
            }
        }
    }
}    // Refresh Current Generation for each Run, update table, set update array,set continue to true
