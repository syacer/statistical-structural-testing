using Accord.Math.Optimization;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Data;
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
        public static Tuple<Matrix<double>,double[]> GenerateAMatrixExpTribProbFromExcel()
        {
            string excelPathBinSetup = @"C:\Users\shiya\Desktop\realSUT\bestMove\binsSetup.xlsx";
            string excelPathbinTrig = @"C:\Users\shiya\Desktop\realSUT\bestMove\triInBins.xlsx";
            int numOfBins = 42;
            Matrix<double> PrimalAMatrix = Matrix<double>.Build.Dense(numOfBins,numOfBins);
            double[] expTriProbAry = new double[numOfBins];
            for (int i = 0; i < numOfBins; i++)
            {
                expTriProbAry[i] = 1.0 / numOfBins;
            }

            DataTable dt = ExcelOperation.ReadExcelFile(excelPathBinSetup).Tables[1];
            int[] binOfSetsIndex = dt.AsEnumerable().Select(x => Convert.ToInt32(x.ItemArray[0])).ToArray();
            int[] countAryPerSets = new int[numOfBins];
            for (int i = 0; i < numOfBins; i++)
            {
                countAryPerSets[i] = binOfSetsIndex.Count(x => x == i + 1);
            }

            DataTable dt2 = ExcelOperation.ReadExcelFile(excelPathbinTrig).Tables[1];
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                for (int j = 0; j < dt2.Rows[i].ItemArray.Length; j++)
                {
                    double data = (double)dt2.Rows[i][j];
                    PrimalAMatrix[j, binOfSetsIndex[i]-1] += data;
                }
            }

            for (int c = 0; c < PrimalAMatrix.ColumnCount; c++)
            {
                for (int r = 0; r < PrimalAMatrix.RowCount; r++)
                {
                    PrimalAMatrix[r, c] /= countAryPerSets[c];
                }
            }
            int rank = PrimalAMatrix.Rank();
            return new Tuple<Matrix<double>, double[]>(item1: PrimalAMatrix, item2: expTriProbAry);
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
