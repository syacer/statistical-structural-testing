using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static class QP
    {
        public static double[] qp(Matrix<double> _aMatrix, double[] _expTrigProb)
        {
            // IMPORTANT: this solver minimizes  following  function:
            //     f(x) = 0.5*x'*A*x + b'*x.
            // Note that quadratic term has 0.5 before it. So if you want to minimize
            // quadratic function, you should rewrite it in such way that quadratic term
            // is multiplied by 0.5 too.
            // For example, our function is f(x)=x0^2+x1^2+..., but we rewrite it as 
            //     f(x) = 0.5*(2*x0^2+2*x1^2) + ....
            // and pass diag(2,2) as quadratic term - NOT diag(1,1)!


            // Settings For Testing
            //_aMatrix = Matrix<double>.Build.Dense(2, 2);
            //_expTrigProb = new double[2];
            //_aMatrix[0, 0] = 0.4;
            //_aMatrix[0, 1] = 0.8;
            //_aMatrix[1, 0] = 0.6;
            //_aMatrix[1, 1] = 0.2;
            //_expTrigProb[0] = 0.5;
            //_expTrigProb[1] = 0.5;
            // ==================================================================

            Matrix<double> QMatrix = _aMatrix.TransposeThisAndMultiply(_aMatrix).Multiply(2);
            Vector<double> HVector = Vector<double>.Build.Dense(_expTrigProb)
                .ToRowMatrix().Multiply(_aMatrix).Row(0).Multiply(-2);
            double D = (Vector<double>.Build.Dense(_expTrigProb).ToRowMatrix()
                       * Vector<double>.Build.Dense(_expTrigProb))[0];

            double[,] a = QMatrix.ToArray();
            double[] b = HVector.ToArray();
            double[] s = new double[QMatrix.ColumnCount];
            double[,] c = new double[QMatrix.ColumnCount + 1, QMatrix.ColumnCount + 1]; //{ { 1.0, 1.0, 2.0 } };
            int[] ct = new int[c.Length/(QMatrix.ColumnCount + 1)];
            double[] x;

            for (int i = 0; i < s.Length; i++)
            {
                s[i] = 1;
            }
            for (int i = 0; i < c.Length / (QMatrix.ColumnCount + 1); i++)
            {
                if (i == 0)
                {
                    for (int j = 0; j < QMatrix.ColumnCount + 1; j++)
                    {
                        c[i, j] = 1;
                    }
                }
                else
                {
                    for (int j = 0; j < QMatrix.ColumnCount; j++)
                    {
                        c[i, j] = (i - 1) == j ? 1 : 0;
                    }
                }
            }
            ct[0] = 0;
            for (int i = 1; i < ct.Length; i++)
            {
                ct[i] = 1;
            }

            alglib.minqpstate state;
            alglib.minqpreport rep;

            // create solver, set quadratic/linear terms
            alglib.minqpcreate(QMatrix.ColumnCount, out state);
            alglib.minqpsetquadraticterm(state, a);
            alglib.minqpsetlinearterm(state, b);
            alglib.minqpsetlc(state, c, ct);

            // Set scale of the parameters.
            // It is strongly recommended that you set scale of your variables.
            // Knowing their scales is essential for evaluation of stopping criteria
            // and for preconditioning of the algorithm steps.
            // You can find more information on scaling at http://www.alglib.net/optimization/scaling.php
            alglib.minqpsetscale(state, s);

            //
            // Solve problem with BLEIC-based QP solver.
            //
            // This solver is intended for problems with moderate (up to 50) number
            // of general linear constraints and unlimited number of box constraints.
            //
            // Default stopping criteria are used.
            //
            if (c.Length / ((QMatrix.ColumnCount + 1)) <= 50)
            {
                alglib.minqpsetalgobleic(state, 0.0, 0.0, 0.0, 0);
                alglib.minqpoptimize(state);
                alglib.minqpresults(state, out x, out rep);
                System.Console.WriteLine("{0}", alglib.ap.format(x, 1));
                return x;
            }
            else
            {
                //
                // Solve problem with DENSE-AUL solver.
                //
                // This solver is optimized for problems with up to several thousands of
                // variables and large amount of general linear constraints. Problems with
                // less than 50 general linear constraints can be efficiently solved with
                // BLEIC, problems with box-only constraints can be solved with QuickQP.
                // However, DENSE-AUL will work in any (including unconstrained) case.
                //
                // Default stopping criteria are used.
                //
                alglib.minqpsetalgodenseaul(state, 1.0e-9, 1.0e+4, 5);
                alglib.minqpoptimize(state);
                alglib.minqpresults(state, out x, out rep);
                System.Console.WriteLine("{0}", alglib.ap.format(x, 1));
                return x;
            }
        }
    }
}
