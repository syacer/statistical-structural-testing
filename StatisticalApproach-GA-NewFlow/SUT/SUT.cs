using System;
using System.Collections.Generic;
using ReadSUTBranchCEData;
using StatisticalApproach.Framework;

namespace StatisticalApproach
{
    // SUT must has Dimension of two
    static class SUT
    {
        static int numofCEParam3 = 0;
        static public readonly object _locker = new object();
        public static Dictionary<string, object> SelectSUT(int selection)
        {
            if (selection == 1)
            {
                return GetProblembestMoveParams();
            }

            else if (selection == 2)
            {
                return GetProblemTriParams();
            }
            else if (selection == 3)
            {
                return GetProblemGcdParams();
            }
            else if (selection == 4)
            {
                return GetProblemCalDayParams();
            }
            else if (selection == 5)
            {
                return GetProblemSUT3Params();
            }
            else if (selection == 6)
            {
                return GetProblemSimpleFuncParams();
            }
            else
            {
                return null;
            }
        }
        static Dictionary<string, object> GetProblembestMoveParams()
        {
            Dictionary<string, object> p2Params = new Dictionary<string, object>();
            p2Params.Add("Name", "bestmove");
            p2Params.Add("Dimension", 2);
			p2Params.Add("TestSize", 1500);
			p2Params.Add("MaxGen",200);
            p2Params.Add("Bound", new List<Tuple<int, int>>
                { new Tuple<int,int>(0,511), new Tuple<int, int>(0, 511) });
            p2Params.Add("Weight", 10);
            p2Params.Add("NumOfCE", 42);
            p2Params.Add("SUTPath", @"D:\Dropbox\ResearchProgramming\DeterministicApproach-GA\" + "bestmove");
            Dictionary<string, double[]> dt = new Dictionary<string, double[]>();
            p2Params.Add("Map", dt);

            return p2Params;
        }
        static Dictionary<string, object> GetProblemCalDayParams()
        {   // CE2, CE6 NOT INCLUDED
            Dictionary<string, object> pTriParam = new Dictionary<string, object>();
            pTriParam.Add("Name", "calday");
            pTriParam.Add("Dimension", 3);
            pTriParam.Add("TestSize", 300);
            pTriParam.Add("MaxGen",200);
            pTriParam.Add("Bound", new List<Tuple<int, int>>
                { new Tuple<int,int>(0,20), new Tuple<int, int>(0, 20), new Tuple<int, int>(0, 20)});
            pTriParam.Add("NumOfCE", 20);
            pTriParam.Add("SUTPath", @"D:\Dropbox\PhDWorks\trunk\ResearchProgramming\DeterministicApproach-GA\SUTs\" + "calday");
            Dictionary<string, double[]> dt = new Dictionary<string, double[]>();
            pTriParam.Add("Map", dt);

            return pTriParam;
        }

        static Dictionary<string, object> GetProblemGcdParams()
        {
            // CE0,  NOT INCLUDED
            Dictionary<string, object> pTriParam = new Dictionary<string, object>();
            pTriParam.Add("Name", "gcd");
            pTriParam.Add("Dimension", 2);
            pTriParam.Add("TestSize", 200);
            pTriParam.Add("MaxGen", 40);
            pTriParam.Add("Bound", new List<Tuple<int, int>>
                { new Tuple<int,int>(0,50), new Tuple<int, int>(0, 50)});
            pTriParam.Add("NumOfCE", 6);
            pTriParam.Add("SUTPath", @"D:\Dropbox\PhDWorks\trunk\ResearchProgramming\DeterministicApproach-GA\SUTs\" + "gcd");
            Dictionary<string, double[]> dt = new Dictionary<string, double[]>();
            pTriParam.Add("Map", dt);

            return pTriParam;
        }

        static Dictionary<string, object> GetProblemTriParams()
        {
            Dictionary<string, object> pTriParam = new Dictionary<string, object>();
            pTriParam.Add("Name","tri");
            pTriParam.Add("Dimension", 3);
            pTriParam.Add("MaxGen", 200);
            pTriParam.Add("Bound", new List<Tuple<int, int>>
                { new Tuple<int,int>(0,20), new Tuple<int, int>(0, 20), new Tuple<int, int>(0,20) });
            pTriParam.Add("NumOfCE", 27);
            pTriParam.Add("SUTPath", @"D:\Dropbox\PhDWorks\trunk\ResearchProgramming\DeterministicApproach-GA\SUTs\" + "tri");
            Dictionary<string, double[]> dt = new Dictionary<string, double[]>();
            pTriParam.Add("Map", dt);

            return pTriParam;
        }
        static Dictionary<string, object> GetProblemSimpleFuncParams()
        {
            Dictionary<string, object> p1Params = new Dictionary<string, object>();
            p1Params.Add("Name", "simpleFunc");
            p1Params.Add("Dimension", 2);
            p1Params.Add("Bound", new List<Tuple<int, int>>
                { new Tuple<int,int>(0,50), new Tuple<int, int>(0, 20) });
            p1Params.Add("NumOfCE", 6);
            p1Params.Add("SUTPath", @"C:\Users\Yang Shi\Dropbox\ResearchProgramming\DeterministicApproach-GA\" + "SUT1DATA");
            Dictionary<string, double[]> dt = new Dictionary<string, double[]>();
            p1Params.Add("Map", dt);

            return p1Params;
        }
        static Dictionary<string, object> GetProblemSUT3Params()  
        {
            Dictionary<string, object> p3Params = new Dictionary<string, object>();
            p3Params.Add("Name", "sut3data");
            p3Params.Add("Dimension", 2);
            p3Params.Add("Bound", new List<Tuple<int, int>>
                { new Tuple<int,int>(0,50), new Tuple<int, int>(0, 50) });
            p3Params.Add("NumOfCE", numofCEParam3);

            //"D:\Dropbox\ResearchProgramming\DeterministicApproach-GA\"
            //"C:\Users\Yang Shi\Dropbox\ResearchProgramming\"
            p3Params.Add("SUTPath", @"D:\Dropbox\ResearchProgramming\DeterministicApproach-GA\" + "sut3data");
            Dictionary<string, double[]> dt = new Dictionary<string, double[]>();
            p3Params.Add("Map", dt);

            return p3Params;
        }
        public static double[] RunSUT(EnvironmentVar enVar, params double[] num)
        {
            int[] outputs = null;
            readBranch rb = new readBranch();
            int[] inputs = Array.ConvertAll(num, n=>(int)n);

            if ((string)enVar.pmProblem["Name"] == "tri")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 0);
            }
            if ((string)enVar.pmProblem["Name"] == "gcd")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 1);
            }
            if ((string)enVar.pmProblem["Name"] == "calday")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 2);
            }
            if ((string)enVar.pmProblem["Name"] == "bestmove")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 3);
            }
            return Array.ConvertAll(outputs, n => (double)n);
        }
        public static double[] RunSUT_obsolete(EnvironmentVar enVar, params double[] num)
        {
            double[] report = new double[(int)enVar.pmProblem["NumOfCE"]];
            string input = null;
            foreach (var n in num)
            {
                input = input + " " + n.ToString();
            }
            input = input.Remove(0, 1);
            report = ((Dictionary<string, double[]>)enVar.pmProblem["Map"])[input];
            return report;
        }
    }
}
