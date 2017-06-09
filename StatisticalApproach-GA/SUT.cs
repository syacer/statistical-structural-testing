using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalApproach_GA
{
    // SUT must has Dimension of two
    static class SUT
    {
        public static Dictionary<string, object>  SelectSUT(int selection)
        {
            if (selection == 1)
            {
                return GetProblem1Params();
            }
            else if (selection == 2)
            {
                return GetProblem2Params();
            }
            else if (selection == 3)
            {
                return GetProblem3Params();
            }
            else
            {
                return null;
            }
        }
        static Dictionary<string, object> GetProblem1Params()
        {
            Dictionary<string, object> p1Params = new Dictionary<string, object>();
            p1Params.Add("Dimension",2);
            p1Params.Add("Bound", new List<Tuple<int,int>>
                { new Tuple<int,int>(0,50), new Tuple<int, int>(0, 20) });
            p1Params.Add("NumOfCE", 6);
            p1Params.Add("SUTPath", @"C:\Users\Yang Shi\Dropbox\ResearchProgramming\DeterministicApproach-GA\" + "SUT1DATA");
            Dictionary<string, double[]> dt = new Dictionary<string, double[]>();
            p1Params.Add("Map",dt);

            return p1Params;
        }

        static Dictionary<string, object> GetProblem2Params()
        {
            Dictionary<string, object> p2Params = new Dictionary<string, object>();
            p2Params.Add("Dimension", 2);
            p2Params.Add("Bound", new List<Tuple<int, int>>
                { new Tuple<int,int>(0,511), new Tuple<int, int>(0, 511) });
            p2Params.Add("Weight", 10);
            p2Params.Add("NumOfCE", 42);
            p2Params.Add("SUTPath", @"D:\Dropbox\ResearchProgramming\DeterministicApproach-GA\" + "SUT2DATA");
            Dictionary<string, double[]> dt = new Dictionary<string, double[]>();
            p2Params.Add("Map", dt);

            return p2Params;
        }
        static Dictionary<string, object> GetProblem3Params()
        {
            Dictionary<string, object> p3Params = new Dictionary<string, object>();
            p3Params.Add("Dimension", 2);
            p3Params.Add("Bound", new List<Tuple<int, int>>
                { new Tuple<int,int>(0,511), new Tuple<int, int>(0, 511) });
            p3Params.Add("NumOfCE", 42);
            p3Params.Add("SUTPath", @"D:\Dropbox\ResearchProgramming\DeterministicApproach-GA\" + "SUT2DATA");
            Dictionary<string,double[]> dt = new Dictionary<string, double[]>();
            p3Params.Add("Map", dt);

            return p3Params;
        }
    }
}
