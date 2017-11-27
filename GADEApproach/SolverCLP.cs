using Accord.Math.Decompositions;
using Accord.Math.Optimization;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static class SolverCLP
    {
        public delegate Tuple<QuadraticObjectiveFunction,List<LinearConstraint>> objFuncDel();

        public static string rootpath = @"C:\Users\shiya\Desktop\realSUT\bestMove\";
        static string excelPathBinSetup = rootpath + @"binsSetup.xlsx";
        static string excelPathbinTrig = rootpath + @"triInBins.xlsx";
        public static string excelPathInputSetMap = rootpath + @"InputSetMap";
        public static string excelPathWeights = rootpath + @"Weights";
        static string excelTrigProbsEst = rootpath + @"TrigProbEst";
        static string excelTrigProbTrue = rootpath + @"TrigProbTrue.xlsx";

#pragma warning disable CS0414 // The field 'SolverCLP.numOfLabels' is assigned but its value is never used
        static int numOfLabels = 42;
#pragma warning restore CS0414 // The field 'SolverCLP.numOfLabels' is assigned but its value is never used

        public static Tuple<double[], double> solver2(Matrix<double> AMatrix, double[] expTribProbAry)
        {

            Matrix<double> QMatrix = AMatrix.TransposeThisAndMultiply(AMatrix);
            // Report the definitness of Matrix A
            EigenvalueDecomposition ed = new EigenvalueDecomposition(QMatrix.ToArray());
            var c = ed.DiagonalMatrix;
            double[] diagAry = new double[(int)Math.Sqrt(c.Length)];
            for (int i = 0; i < Math.Sqrt(c.Length); i++)
            {
                diagAry[i] = c[i, i];
            }
            if (diagAry.Contains(0))
            {
                Console.WriteLine("A^TA is semi-difinite");
            }
            if (diagAry.Where(x => x > 0).Count() > 0
                && diagAry.Where(x => x < 0).Count() > 0)
            {
                Console.WriteLine("A^TA is indifinite");
            }
            // Report x, and final distance-to-optimal
            double[] weights = QP.qp(AMatrix, expTribProbAry);

            Vector<double> HVector = Vector<double>.Build.Dense(expTribProbAry)
                .ToRowMatrix().Multiply(AMatrix).Row(0).Multiply(-2);
            double d = (Vector<double>.Build.Dense(expTribProbAry).ToRowMatrix()
                       * Vector<double>.Build.Dense(expTribProbAry))[0];

            var weightsVect = Vector<double>.Build.Dense(weights).ToColumnMatrix();
            var QuardaticTerm = weightsVect.Transpose().Multiply(AMatrix.Transpose())
                .Multiply(AMatrix).Multiply(weightsVect);
            if (!(QuardaticTerm.RowCount == 1 && QuardaticTerm.ColumnCount == 1))
            {
                Console.WriteLine("Calculating Final Quardatic Term Wrong");
                Console.ReadKey();
            }
            var LinearTerm = HVector.ToRowMatrix().Multiply(weightsVect);
            if (!(LinearTerm.RowCount == 1 && LinearTerm.ColumnCount == 1))
            {
                Console.WriteLine("Calculating Final LinearTerm Wrong");
                Console.ReadKey();
            }
            double distToOptimal = QuardaticTerm[0, 0] + LinearTerm[0, 0] + d;

            return new Tuple<double[], double>(item1: weights, item2: distToOptimal);
        }
        public static Tuple<double[], double> solver1(objFuncDel handle = null)
        { // This Solver uses Accord GoldFarbIdnni approach, only works for Positive Definite Matrix
          // We have Semi-Definite Matricies, It is not suitable. Use Solver 2: QP solver instead

            Tuple<QuadraticObjectiveFunction, List<LinearConstraint>> fAndc = null;
            fAndc = handle == null ? objectiveFuncForLabelTest() : handle.Invoke();
            var solver = new GoldfarbIdnani(fAndc.Item1, fAndc.Item2);
            bool success = solver.Minimize();
            if (success == false)
            {
                Console.WriteLine(solver.Status.ToString());
            }
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

        static public DataTable WriteFinalSolutionToExcel(DataTable inputsBinDt, int[] newBinsSetup, double[] weights, Matrix<double> AMatrix)
        {
            SUT sut = new SUT();
            if (weights == null)    // using linear programming for branch coverage, weights remain the same, copy setProbilities.xlsx
            {
                string filePath = rootpath + @"setProbabilities.xlsx";
                var weightDt = ExcelOperation.ReadExcelFile(filePath).Tables[1];
                weights = weightDt.AsEnumerable().Select(x => Convert.ToDouble(x.ItemArray[0])).ToArray();
            }
            DataSet ds = FinalResultsinDataTables(
                inputsBinDt,
                newBinsSetup, 
                weights, 
                AMatrix,
                @"InputSetMap",
                @"Weights",
                @"TrigProbEst",
                @"TrigProbTrue"
                );

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                string excelName = ds.Tables[i].TableName + ".xlsx";
                if (!File.Exists(rootpath + excelName))
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            true
                            );
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            false
                            );
                }
            }
            return ds.Tables["InputSetMap"];
        }

        public static DataSet FinalResultsinDataTables(
            DataTable inputsBinDt,
            int[] newBinsSetup, 
            double[] weights, 
            Matrix<double> aMatrix, 
            string InputSetMap, 
            string Weights, 
            string TrigProbEst, 
            string TrigProbTrue)
        {
            DataSet ds = new DataSet();

            // Add InputSetMap to DataTable
            ds.Tables.Add(InputSetMap);
            string tablePath = rootpath + "inputsBin.xlsx";
            DataTable dt = null;
            if (inputsBinDt == null)
            {
                try
                {
                    dt = ExcelOperation.ReadExcelFile(tablePath).Tables[1];
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                dt = inputsBinDt;
            }
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    List<int> dataRowInputSet = new List<int>();
                    object[] rowData = dt.Rows[i].ItemArray;
                    if (i == 0)
                    {
                        for (int j = 0; j < rowData.Length; j++)
                        {
                            ds.Tables[InputSetMap].Columns.Add(j.ToString(), Type.GetType("System.Int32"));
                        }
                    }
                    for (int j = 0; j < rowData.Length; j++)
                    {
                        if (Convert.ToInt32(rowData[j]) != -9999999)
                        {
                            dataRowInputSet.Add(Convert.ToInt32(rowData[j]));
                        }
                        else
                        {
                            dataRowInputSet.Add(Convert.ToInt32(rowData[j + 1]));
                            break;
                        }
                    }
                    int binIndex = dataRowInputSet[dataRowInputSet.Count - 1];
                    int setIndex = newBinsSetup[binIndex - 1];
                    dataRowInputSet[dataRowInputSet.Count - 1] = -9999999;
                    dataRowInputSet.Add(setIndex);
                    int[] dataRowInputSetAry = dataRowInputSet.ToArray();
                    var newRow = ds.Tables[InputSetMap].NewRow();
                    newRow.ItemArray = dataRowInputSetAry.Select(x => (object)x).ToArray();
                    ds.Tables[InputSetMap].Rows.Add(newRow);
                }
            }
            
            // Add Weightd to DataTable
            ds.Tables.Add(Weights);
            ds.Tables[Weights].Columns.Add("weight", Type.GetType("System.Double"));
            for (int i = 0; i < weights.Length; i++)
            {
                object[] data = new object[] {weights[i]};
                var newRow = ds.Tables[Weights].NewRow();
                newRow.ItemArray = data;
                ds.Tables[Weights].Rows.Add(newRow);
            }

            //Add Triggering Prob. Estimate to DataTable
            ds.Tables.Add(TrigProbEst);
            ds.Tables[TrigProbEst].Columns.Add("triProbEst", Type.GetType("System.Double"));
            for (int i = 0; i < aMatrix.RowCount; i++)
            {
                double triProbithCE = aMatrix.Row(i).ToRowMatrix()
                    .Multiply(Vector<double>.Build.Dense(weights))[0];

                object[] data = new object[] { triProbithCE };
                var newRow = ds.Tables[TrigProbEst].NewRow();
                newRow.ItemArray = data;
                ds.Tables[TrigProbEst].Rows.Add(newRow);
            }

            // Not implement true triggering prob.

            return ds;
        }
    }
}
