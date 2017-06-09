using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalApproach.Framework
{
    using System.IO;
    using Utility;
    using AppFunc = Func<EnvironmentVar, Task>;
    class Monitor : App
    {
        private Record _record;
        private EnvironmentVar[] _enVars;

        public Monitor(Record record, EnvironmentVar[] enVars, AppFunc next)
        {
            _enVars = enVars;
            _record = record;
        }

        public async Task<int> Invoke(EnvironmentVar enVar)
        {
            Task monitor = Task.Run(() =>
              {
                  LocalFileAccess lfa = new LocalFileAccess();
                  while (!_enVars.All(x => x.finishIndicate == true))
                  {
                      for (int i = 0; i < _enVars.Length; i++)
                      {
                          if (_record.updateIndicate[i] == true)
                          {
                              /** update fitRecord to File **/
                              lfa.StoreListToLinesAppend(Directory.GetCurrentDirectory() + @"\OUT\GA\" + _enVars[i].pmProblem["Name"] + @"\CEOutput_" + i.ToString(), _record.currentGen[i]);
                              lfa.StoreListToLinesAppend(Directory.GetCurrentDirectory() + @"\OUT\GA\" + _enVars[i].pmProblem["Name"] + @"\CEOutput_" + i.ToString(), _record.currentCElist[i]);
                              for (int k = 0; k < _record.currentBestSolution[i].Length; k++)
                              {
                                  if (_record.currentBestSolution[i][k] != null)
                                  {
                                      lfa.StoreListToLinesAppend(Directory.GetCurrentDirectory() + @"\OUT\GA\" + _enVars[i].pmProblem["Name"] + @"\CEOutput_" + i.ToString(), _record.currentBestSolution[i][k]);
                                  }
                              }
                              _record.updateDisplay[i] = true;
                              while (_record.updateDisplay[i] == true) ;
                              _record.updateIndicate[i] = false;
                          }
                      }
                  }
                  for (int i = 0; i < _enVars.Length; i++)
                  {
                      string timeElaspe = @"Total Time: " + Math.Round(_record.Watch[i].ElapsedMilliseconds*1.0/(1000*60),3).ToString();
                      lfa.StoreListToLinesAppend(Directory.GetCurrentDirectory() + @"\OUT\GA\" + _enVars[i].pmProblem["Name"] + @"\CEOutput_" + i.ToString(), new List<string>() {timeElaspe});
                  }

              });

            await monitor;
            Console.WriteLine("Mointor Closed.");
            return 0;
        }
    }
}
