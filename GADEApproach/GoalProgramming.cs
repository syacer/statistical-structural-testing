using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static public class GoalProgramming
    {
        static Vector<double> penaltyFactors = null;
        static int numOfVariables = -1;
        public static void SetpenaltyFactors()
        {

        }
        public static double MinTrigProbCal(Matrix<double> Amatrix, out double[] wArray)
        {
            numOfVariables = Amatrix.ColumnCount;
            double[] sAndW = new double[Amatrix.RowCount + Amatrix.ColumnCount];
            Matrix<double> c = Matrix<double>.Build.Dense(
                Amatrix.RowCount * 2 + Amatrix.ColumnCount + 1, 
                Amatrix.RowCount + Amatrix.ColumnCount + 1
            );
            int[] ct = new int[c.RowCount];

            for (int i = 0; i < sAndW.Length; i++)
            {
                sAndW[i] = 1.0;
            }

            double[] maxTriProbs = new double[Amatrix.RowCount];
            int[] maxTriProbsIndexes = new int[Amatrix.RowCount];
            for (int i = 0; i < Amatrix.RowCount; i++)
            {
                maxTriProbs[i] = Amatrix.Row(i).Max();
                maxTriProbsIndexes[i] = Amatrix.Row(i).ToList().IndexOf(maxTriProbs[i]);
            }

            // Minimize C1*S1 + C2*S2 + ... + Cm*Sm + 0* w1 + ... + 0 * wm
            // subject to:
            // a11*w1+a12*w2+ ... + a1m*wm - S1 = maxTri1
            //   :
            // am1*w1 + am2*w2 + ... + amm*wm - SM = maxTrim
            // w1 + w2 + ... = wm
            // all si > 0, all wi > 0

            double theta = 0.8; //1.0 / Amatrix.RowCount;
            penaltyFactors = Vector<double>.Build.Dense(Amatrix.RowCount, 1);
            for (int i = 0; i < penaltyFactors.Count(); i++)
            {
                if (maxTriProbs[i] < theta)
                {
                    penaltyFactors[i] = penaltyFactors[i] * (theta - maxTriProbs[i]) * 100;
                }
            }

            for (int i = 0; i < Amatrix.ColumnCount+1; i++)
            {
                c[ct.Length - 1, i+Amatrix.RowCount] = 1;
            }
            for (int row = 0; row < Amatrix.RowCount; row++)
            {
                c[row, row] = 1;
                for (int col = 0; col < Amatrix.ColumnCount; col++)
                {
                    c[row, col + Amatrix.RowCount] = Amatrix.Row(row)[col];
                }
                c[row, Amatrix.ColumnCount + Amatrix.RowCount] = maxTriProbs[row];
                ct[row] = 0;
            }

            for (int row = 0; row < Amatrix.RowCount + Amatrix.ColumnCount; row++)
            {
                c[row + Amatrix.RowCount, row] = 1;
                ct[row + Amatrix.RowCount] = 1;
                c[row + Amatrix.RowCount, Amatrix.RowCount + Amatrix.ColumnCount] = 0;
            }
            alglib.minbleicstate state;
            alglib.minbleicreport rep;
            double epsg = 0.000001;
            double epsf = 0;
            double epsx = 0;
            int maxits = 0;

            alglib.minbleiccreate(sAndW, out state);
            alglib.minbleicsetlc(state, c.ToArray(), ct);
            alglib.minbleicsetcond(state, epsg, epsf, epsx, maxits);
            alglib.minbleicoptimize(state, linearFunction_grad, null, null);
            alglib.minbleicresults(state, out sAndW, out rep);

            var sVector = Vector<double>.Build.Dense(sAndW.Take(Amatrix.RowCount).ToArray());
            var trigProbs = Vector<double>.Build.Dense(maxTriProbs) - sVector;
            wArray = new double[Amatrix.ColumnCount];
            Array.Copy(sAndW, Amatrix.RowCount, wArray, 0, Amatrix.ColumnCount);
            double fitness = trigProbs.Min();
            //var e = Amatrix.Column(9);
            //if (wArray.Where(x => Math.Round(x, 3) > 0).Count() > 1)
            //{
            //    int ttt = 0;
            //}
            //var ttt2 = Amatrix.Column(1);
            //var ValidatetrigProbs = Amatrix * Vector<double>.Build.Dense(wArray).ToColumnMatrix();
            //double ValidateSumEqualToOne = wArray.Sum();
            return fitness;
        }
        public static void linearFunction_grad(double[] x, ref double func, double[] grad, object obj)
        {
            // this callback calculates f(x0,x1) = 100*(x0+3)^4 + (x1-3)^4
            // and its derivatives df/d0 and df/dx1
            //func = 100 * System.Math.Pow(x[0] + 3, 4) + System.Math.Pow(x[1] - 3, 4);
            func = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (i >= numOfVariables)
                {
                    func += x[i] * 0;
                    grad[i] = 0;
                }
                else
                {
                    func += x[i] * penaltyFactors[i];
                    grad[i] = penaltyFactors[i];
                }
            }
        }
    }
}
