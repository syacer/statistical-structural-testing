using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticalApproach_GA
{
    using System.Data;
    using System.Linq;
    using AppFunc = Func<EnvironmentVar, Task>;

    class SUTInitialization
    {
        public string _sutPath;
        public AppFunc _next;
        public int _selectSUT;
        public Record _record;

        public SUTInitialization(string SUTPath, int selectSUT, Record record, AppFunc next)
        {
            _sutPath = SUTPath;
            _next = next;
            _selectSUT = selectSUT;
            _record = record;

        }
        public async Task<int> Invoke(EnvironmentVar enVar)
        {
            Dictionary<string, object>sutParam = SUT.SelectSUT(_selectSUT);
            sutParam["SUTPath"] = _sutPath;
            _record.sutInfo = sutParam;
            Task taskInitialSUT = Task.Run(() => {
                LocalFileAccess lfa = new LocalFileAccess();
                List<string> list = new List<string>();
                lfa.StoreLinesToList(_sutPath.ToString(), list);
                DataTable dt = new DataTable();
                dt.Columns.Add("Input", Type.GetType("System.String"));
                for (int i = 0; i < (int)sutParam["NumOfCE"]; i++)
                {
                    dt.Columns.Add((i + 1).ToString(), Type.GetType("System.String"));
                }

                foreach (string s in list)
                {
                    int length = dt.Rows.Count;

                    if (length == 0)
                    {
                        int c = Convert.ToInt32(s.Substring(s.IndexOf(' ', s.IndexOf(' ') + 1))) + 1;
                        object[] coverSeq = new object[(int)sutParam["NumOfCE"] + 1];
                        for (int i = 0; i < coverSeq.Length; i++)
                        {
                            coverSeq[i] = 0.0;
                        }
                        coverSeq[c] = 1.0;
                        coverSeq[0] = s.Substring(0, s.IndexOf(' ', s.IndexOf(' ') + 1));
                        dt.Rows.Add(
                            coverSeq
                        );
                    }
                    else if (s.Substring(0, s.IndexOf(' ', s.IndexOf(' ') + 1))
                        == dt.Rows[length - 1].ItemArray[0].ToString())
                    {
                        int c = Convert.ToInt32(s.Substring(s.IndexOf(' ', s.IndexOf(' ') + 1) + 1)) + 1;
                        dt.Rows[length - 1][c] = 1.0;
                    }
                    else
                    {
                        int c = Convert.ToInt32(s.Substring(s.IndexOf(' ', s.IndexOf(' ') + 1))) + 1;
                        object[] coverSeq = new object[(int)sutParam["NumOfCE"] + 1];
                        for (int i = 0; i < coverSeq.Length; i++)
                        {
                            coverSeq[i] = 0.0;
                        }
                        coverSeq[c] = 1.0;
                        coverSeq[0] = s.Substring(0, s.IndexOf(' ', s.IndexOf(' ') + 1));
                        dt.Rows.Add(
                            coverSeq
                        );
                    }
                }
                sutParam["Map"] = dt.AsEnumerable()
                    .ToDictionary<DataRow, string, double[]>(
                         row => row.Field<string>(0),
                         row =>
                         {
                             double[] coverSeq = new double[(int)sutParam["NumOfCE"]];
                             for (int i = 0; i < (int)sutParam["NumOfCE"]; i++)
                             {
                                 coverSeq[i] = Convert.ToDouble(row.ItemArray[i + 1]);
                             }
                             return coverSeq;
                         });
            });

            await taskInitialSUT;
            await _next(enVar);
            Console.WriteLine("All tasks are done");
            return 0;
        }
    }
}
