using Accord.Math.Optimization;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static class SolverCLP
    {
        public delegate Tuple<QuadraticObjectiveFunction,List<LinearConstraint>> objFuncDel();
        
        public static Tuple<double[], double> solver(objFuncDel handle = null)
        {
            Tuple<QuadraticObjectiveFunction, List<LinearConstraint>> fAndc = null;
            fAndc = handle == null ? objectiveFuncForLabelTest() : handle.Invoke();
            var solver = new GoldfarbIdnani(fAndc.Item1, fAndc.Item2);
            bool success = solver.Minimize();
            double value = solver.Value;
            double[] solution = new double[solver.Solution.Length];

            for (int i = 0; i < solution.Length; i++)
            {
                solution[i] = solver.Solution[i]; 
            }
            return new Tuple<double[], double>(item1:solution,item2:value);
        }
        public static Tuple<QuadraticObjectiveFunction, List<LinearConstraint>> objectiveFuncForLabelTest()
        {   
            // x1, x2    Pl
            //[0.2,0.3][0.7]
            //[0.8,0.7][0.3]
            
            double x = 0, y = 0;
            QuadraticObjectiveFunction f = new QuadraticObjectiveFunction(
                () =>
                    0.68 * (x * x) + 0.58 * (y * y) + 1.24 * (x * y) - 0.76 * x - 0.84 * y + 0.58
            );
            return new Tuple<QuadraticObjectiveFunction, List<LinearConstraint>> (item1:f,item2: new List<LinearConstraint>());
        }
    }
}
