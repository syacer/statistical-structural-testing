using Accord.Math.Optimization;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    public class ObjectiveFunctions
    {
        Matrix<double> _aMatrix;
        double[] _expTrigProb;

        public ObjectiveFunctions(Matrix<double> aMatrix, double[] expTrigProb)
        {
            _aMatrix = aMatrix;
            _expTrigProb = expTrigProb;
        }
        public List<LinearConstraint> ConstraintConstruct()
        {

            List<LinearConstraint> list = new List<LinearConstraint>();
            list.Add(new LinearConstraint(numberOfVariables: _aMatrix.RowCount)
            {
                VariablesAtIndices = new int[_aMatrix.RowCount].Select((x,i)=>x=i).ToArray(),
                CombinedAs = new double[_aMatrix.RowCount].Select(x=>x=1).ToArray(), // when combined as 1x + 1y...
                ShouldBe = ConstraintType.EqualTo,
                Value = 1
            });
            for (int i = 0; i < _aMatrix.RowCount; i++)
            {
                list.Add(new LinearConstraint(numberOfVariables: 1)
                {
                    VariablesAtIndices = new int[1] {i},
                    ShouldBe = ConstraintType.GreaterThanOrEqualTo,
                    Value = 0
                });
            }
            return list;
        }
        public Tuple<QuadraticObjectiveFunction, List<LinearConstraint>> objectFuncQHStyle()
        {
            Matrix<double> QMatrix = Matrix<double>.Build.Dense(_aMatrix.RowCount, _aMatrix.RowCount);
            Vector<double> HVector = Vector<double>.Build.Dense(_aMatrix.RowCount);

            for (int r = 0; r < _aMatrix.RowCount; r++)
            {
                Matrix<double> Q = Matrix<double>.Build.Dense(_aMatrix.RowCount, _aMatrix.RowCount);
                Vector<double> H = Vector<double>.Build.Dense(_aMatrix.RowCount);
                for (int c1 = 0; c1 < _aMatrix.RowCount; c1++)
                {
                    for (int c2 = 0; c2 < _aMatrix.RowCount; c2++)
                    {
                        Q[c1, c2] = 2 * _aMatrix[r,c1] * _aMatrix[r,c2];
                    }
                    H[c1] = -2 * _aMatrix[r, c1] * _expTrigProb[r];
                }
                HVector = HVector.Add(H);
                QMatrix = QMatrix.Add(Q);
            }
            string[] variablesName = new string[_aMatrix.RowCount];
            for (int i = 0; i < variablesName.Length; i++)
            {
                variablesName[i] = "x" + i.ToString();
            }
            QuadraticObjectiveFunction f = new QuadraticObjectiveFunction
                (QMatrix.ToArray(),HVector.ToArray(), variablesName);
            f.ConstantTerm = _expTrigProb.Sum(x=>x*x);
            List<LinearConstraint> constraints = ConstraintConstruct();
            return new Tuple<QuadraticObjectiveFunction, List<LinearConstraint>>(item1: f, item2: constraints);
        }
    }
}
