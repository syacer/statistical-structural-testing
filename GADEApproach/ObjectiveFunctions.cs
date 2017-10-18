using Accord.Math.Optimization;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math.Decompositions;

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
            list.Add(new LinearConstraint(numberOfVariables: _aMatrix.ColumnCount)
            {
                VariablesAtIndices = new int[_aMatrix.ColumnCount].Select((x,i)=>x=i).ToArray(),
                CombinedAs = new double[_aMatrix.ColumnCount].Select(x=>x=1).ToArray(), // when combined as 1x + 1y...
                ShouldBe = ConstraintType.EqualTo,
                Value = 1
            });
            for (int i = 0; i < _aMatrix.ColumnCount; i++)
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
            Matrix <double>  QMatrix = _aMatrix.TransposeThisAndMultiply(_aMatrix);
            EigenvalueDecomposition ed = new EigenvalueDecomposition(QMatrix.ToArray());
            var c = ed.DiagonalMatrix;
            var a = QMatrix.Inverse();
            Vector<double>  HVector = Vector<double>.Build.Dense(_expTrigProb).ToRowMatrix().Multiply(_aMatrix).Row(0);
            double S = (Vector<double>.Build.Dense(_expTrigProb).ToRowMatrix()
                       * Vector<double>.Build.Dense(_expTrigProb))[0];
            int rank = QMatrix.Rank();
            string[] variablesName = new string[_aMatrix.ColumnCount];
            for (int i = 0; i < variablesName.Length; i++)
            {
                variablesName[i] = "x" + i.ToString();
            }
            QuadraticObjectiveFunction f = new QuadraticObjectiveFunction
                (QMatrix.ToArray(),HVector.ToArray(), variablesName);
            f.ConstantTerm = S;
            List<LinearConstraint> constraints = ConstraintConstruct();
            return new Tuple<QuadraticObjectiveFunction, List<LinearConstraint>>(item1: f, item2: constraints);
        }
    }
}
