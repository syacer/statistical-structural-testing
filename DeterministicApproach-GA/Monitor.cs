using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeterministicApproach_GA
{
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
                  while (!_enVars.All(x => x.finishIndicate == true))
                  {
                      for (int i = 0; i < _enVars.Length; i++)
                      {
                          if (_enVars[i].continueIndicate == false)
                          {
                              /** update fitRecord table **/
                              int gen = (int)_enVars[i].genFitRecord[0];
                              double fit = (double)_enVars[i].genFitRecord[1];
                              _record.fitRecord.Rows[gen][i + 1] = fit;

                              /** update SolRecord table **/
                              string strSolution = null;
                              Array.ForEach(_enVars[i].population
                                .OrderByDescending(x => x.Value)
                                .First().Key
                                .GetGenotype(), x =>
                                {
                                    int[] tmp = null;
                                    x.GetGenValue(ref tmp);
                                    string tempStr = "(";
                                    foreach (var item in tmp)
                                    {
                                        tempStr = tempStr + item.ToString() + " ";
                                    }
                                    tempStr = tempStr + ")";
                                    strSolution = strSolution + tempStr;
                                });
                              _record.solRecord.Rows[gen][i + 1] = strSolution;

                              _record.currentGen[i] = gen;
                              _record.updateIndicate[i] = true;
                              _enVars[i].continueIndicate = true;
                          }
                      }
                  }
              ExcelOperation.dataTableListToExcel(
                  new List<DataTable> { _record.fitRecord, _record.solRecord },
                  false,
                  Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"/out.xlsx"
                      );
              });

            await monitor;
            Console.WriteLine("Mointor Closed.");
            return 0;
        }
    }
}
