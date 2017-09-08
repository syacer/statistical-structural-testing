using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    class ProcessingExcels
    {

        public Tuple<double,double,double> ProcessDataExcel(string filePath)
        {
           DataSet ds = FakeReadExcelFiles(); //This is only For testing
           DataTable dt = ds.Tables[0];
           var enumerableDt = dt.AsEnumerable();
           List<double> goodnessOfFitRuns = new List<double>();
           for (int i = 0; i < dt.Columns.Count/2; i++)
           {
                if ((double)dt.Rows[2].ItemArray[2 * i] != 0)
                {
                    double finalGoodnessOfFit = (double)enumerableDt.Min(x =>
                    {
                        if ((double)x.ItemArray[2 * i] != 0)
                        {
                            return x.ItemArray[2 * i];
                        }
                        else
                        {
                            return 99999;
                        }
                    });
                    goodnessOfFitRuns.Add(finalGoodnessOfFit);
                }
           }
            double max = goodnessOfFitRuns.Max();
            double min = goodnessOfFitRuns.Min();
            double avg = goodnessOfFitRuns.Average();

            return new Tuple<double, double, double>(max,min,avg);
        }

        public DataSet FakeReadExcelFiles()
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
