using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace WeightEvolve
{
    using CEPool = Dictionary<GAEncoding, double[]>;

    [Serializable]
    class GAEncoding
    {
        // thetaDelta Matrix Example
        //       dim1, dim2, dim 3
        //parm1 |p1,    p2,    p3 |
        //parm2 |p4,    p5,    p6 |
        //parm3 |p7,    p8,    p9 |
        //........................
        // Each weight ranges within [0,1]
        public string id;
        public Matrix<double> thetaDelta;
        public Vector<double> numOfLabelsVect;
        public Vector<double> numOfLabelsVect2;
        public Vector<double> weightsVect;
        public int rank = -1;
        public int duplicateSamples = 0;
        public double entropy = 0;
        public double diversity = 0;
        int _numOfParams;
        int _dimension;
        double[] _lowbounds;
        double[] _highbounds;

        public GAEncoding(int numOfParams, int dimension, double[] lowbounds, double[] highbounds)
        {
            _numOfParams = numOfParams;
            _dimension = dimension;
            _lowbounds = lowbounds;
            _highbounds = highbounds;
            thetaDelta = Matrix.Build.Dense(numOfParams, dimension);
            weightsVect = Vector.Build.Dense(numOfParams*numOfParams);
        }

        public void UpdateID()
        {
            id = Guid.NewGuid().ToString();
        }

        // Recombination: create a new solution, call this function;
        // Add the new solution to the population
        internal GAEncoding IntermidinateRecomb(CEPool currentPool, int[] selectedList)
        {
            double d = 0.25;
            GAEncoding newSolution = new GAEncoding(_numOfParams, _dimension, _lowbounds, _highbounds);
            double alpha = GlobalVar.rnd.NextDouble();
            alpha = (1 + d - -1 * d) * alpha + -1 * d;
            var deltaW = currentPool.ElementAt(selectedList[1]).Key.weightsVect
                       - currentPool.ElementAt(selectedList[0]).Key.weightsVect;
            newSolution.weightsVect = 
                deltaW.Multiply(alpha) + currentPool.ElementAt(selectedList[0]).Key.weightsVect;

            double test = newSolution.weightsVect.Sum();
            if (test > _numOfParams * _numOfParams + 0.001 || test < _numOfParams * _numOfParams - 0.001)
            {
                Console.WriteLine();
            }
            newSolution.FixInputs(_lowbounds, _highbounds);
            newSolution.UpdateID();
            return newSolution;
        }

        internal GAEncoding DifferentialCrossover(CEPool currentPool, int[] selectedList)
        {
            double F = 1.5; //[0,2]
            UpdateID();
            
            for (int i = 0; i < weightsVect.Count; i++)
            {
                if (GlobalVar.rnd.NextDouble() <= 0.9)
                {
                    weightsVect[i] = currentPool.ElementAt(selectedList[0]).Key.weightsVect[i]
                        + F * currentPool.ElementAt(selectedList[0]).Key.weightsVect[i] -
                        currentPool.ElementAt(selectedList[1]).Key.weightsVect[i];
                    if (weightsVect[i] < 0)
                    {
                        weightsVect[i] = 0;
                    }
                    if (weightsVect[i] > 10000)
                    {
                        weightsVect[i] = 10000;
                    }
                }
            }


            double test = this.weightsVect.Sum();
            if (test > _numOfParams * _numOfParams + 0.001 || test < _numOfParams * _numOfParams - 0.001)
            {
                //Console.WriteLine("AAAA");
            }
            return this;
        }
        //Simple Mutation Assumes independence of each variable
        internal GAEncoding RealVectMutation()
        {
            GAEncoding newSolution = Copy.DeepCopy(this);
            UpdateID();
            newSolution.FixInputs(_lowbounds, _highbounds);

            //for (int i = 0; i < _numOfParams * _numOfParams; i++)
            //{
            //    if (GlobalVar.rnd.NextDouble()
            //        <= 2.0 / (_numOfParams * _numOfParams))
            //    {
            //        double k_MutPrecision = 1;
            //        double u = GlobalVar.rnd.NextDouble();
            //        int s_Direction = GlobalVar.rnd.NextDouble() <= 0.5 ? -1 : 1;
            //        double a_StepSize1 = 10*u + s_Direction;
            //        double mean = weightsVect.Sum()/(_numOfParams*_numOfParams);
            //        weightsVect[i] = weightsVect[i] * a_StepSize1 + mean * (1 - a_StepSize1);
            //        if (weightsVect[i] <= 0)
            //        {
            //            weightsVect[i] = 0.0001;
            //        }
            //    }
            //}

            if (GlobalVar.rnd.NextDouble() <= 1.0 / 100)
            {
                for (int i = 0; i < _numOfParams * _numOfParams; i++)
                {
                        double k_MutPrecision = 1;
                        double u = GlobalVar.rnd.NextDouble();
                        int s_Direction = GlobalVar.rnd.NextDouble() <= 0.5 ? -1 : 1;
                        double a_StepSize1 = 10 * u + s_Direction;
                        double mean = weightsVect.Sum() / (_numOfParams * _numOfParams);
                        weightsVect[i] = weightsVect[i] * a_StepSize1 + mean * (1 - a_StepSize1);
                        if (weightsVect[i] <= 0)
                        {
                            weightsVect[i] = 0.0001;
                        }
                }
            }

            weightsVect = weightsVect.Normalize(1).Multiply(_numOfParams * _numOfParams);

            double test = weightsVect.Sum();
            if (test > _numOfParams * _numOfParams + 0.001 || test < _numOfParams * _numOfParams - 0.001)
            {
                Console.WriteLine();
            }
            return newSolution;
        }
        //Simple Mutation Assumes independence of each variable
  
        internal Vector<double> GetRandVect(double[] lowbounds, double[] highbounds)
        {
            int blockIndex = -1;
            double sumOfWeight = weightsVect.Sum();

            // First, Select a block
            for (int i = 0; i < _numOfParams*_numOfParams; i++)
            {
                double lowIndex = 0.0;
                double rndNum = GlobalVar.rnd.NextDouble() * sumOfWeight;
                if (rndNum >= lowIndex && rndNum <= lowIndex + weightsVect[i])
                {
                    blockIndex = i;
                    break;
                }
                lowIndex = lowIndex + weightsVect[i];
            }

            int x2 = blockIndex / _numOfParams;
            int x1 = blockIndex % _numOfParams;
            int[] selIndexArry = new int[2] {x1,x2 };
            double[] result = new double[_dimension];

            for (int i = 0; i < _dimension; i++)
            {
                if (selIndexArry[i] == 0)
                {
                    result[i] = GlobalVar.rnd.Next((int)lowbounds[i], (int)thetaDelta[i, 0]);
                }
                else if (selIndexArry[i] == _numOfParams-1)
                {
                    double min = thetaDelta.Column(i).Sum()
                        - thetaDelta.Row(_numOfParams-1).ElementAt(i);
                    result[i] = GlobalVar.rnd.Next((int)min, (int)highbounds[i]+1);
                }
                else
                {
                    double min = thetaDelta.Column(i)
                        .Select((x, k) =>
                        {
                            if (k <= selIndexArry[i] - 1)
                            {
                                return x;
                            }
                            return 0;
                        }).Sum();
                    double max = min + thetaDelta.Column(i).ElementAt(selIndexArry[i]);
                    result[i] = GlobalVar.rnd.Next((int)min, (int)max);
                }
            }
            return Vector.Build.Dense(result);
        }
        internal void CalEstFitness(int testSetSize, int numOfLabel, Matrix<double> labelMatrix, double[] lowbounds, double[] highbounds)
        {
            numOfLabelsVect = Vector<double>.Build.Dense(numOfLabel);
            DenseMatrix mInputVectors = new DenseMatrix(testSetSize, _dimension);
            duplicateSamples = 0;

            for (int i = 0; i < testSetSize; i++)
            {
                Vector<double> vRandVect = GetRandVect(lowbounds, highbounds);
                if (mInputVectors.EnumerateRows().Contains(vRandVect))
                {
                    duplicateSamples = duplicateSamples + 1;
                }
                else
                {
                    mInputVectors.SetRow(i, vRandVect);
                }
                int label = (int)labelMatrix.At((int)vRandVect[0], (int)vRandVect[1]);
                if (label <= numOfLabel)
                {
                    numOfLabelsVect[label - 1] += 1;
                }
            }
            // Calculate p of Labals
            numOfLabelsVect = numOfLabelsVect.Divide(testSetSize);
            entropy = 0;
            for (int i = 0; i < numOfLabel; i++)
            {
                entropy += numOfLabelsVect[i] * Math.Log10(1.0/(numOfLabelsVect[i]+0.000001));
            }
            duplicateSamples = 0;
        }

        internal void FitnessCal(Matrix<double> aMatrix)
        {
            // First, Calculate Total Weights;
            // And create weight Vector from weight matrix
            double totalWeights = weightsVect.Sum();
            diversity = 0;
            entropy = 0;

            // Calculate Entropy
            numOfLabelsVect2 = Vector.Build.Dense(
                aMatrix.Transpose()
                .Multiply(weightsVect)
                .Divide(totalWeights).ToArray());

            for (int i = 0; i < numOfLabelsVect2.Count; i++)
            {
                entropy += numOfLabelsVect2[i] * Math.Log(Math.Pow(numOfLabelsVect2[i] + 0.000001, -1), 2);
            }

            //// Calculate Diversity
            //Vector<double> meanVect = numOfLabelsVect2;
            //Vector<double>[] temp = new Vector<double>[aMatrix.ColumnCount];
            //Vector<double>[] temp2 = new Vector<double>[aMatrix.RowCount];
            //for (int i = 0; i < temp.Length; i++)
            //{
            //    temp[i] = weightsVect.Divide(weightsVect.Sum());
            //}
            //for (int i = 0; i < temp2.Length; i++)
            //{
            //    temp2[i] = meanVect;
            //}
            //Matrix<double> wMatrix = Matrix.Build.DenseOfColumnVectors(temp);
            //Matrix<double> meanMatrix = Matrix.Build.DenseOfRowVectors(temp2);
            //Vector<double> tempVect = aMatrix.PointwiseMultiply(wMatrix)
            //    .Subtract(meanMatrix)
            //    .PointwisePower(2)
            //    .ColumnSums().Divide(weightsVect.Count);
            //diversity = tempVect.Sum();
        }
        internal void FixInputs(double[] lowbounds, double[] highbounds)
        {
            Vector<double> low = Vector.Build.Dense(lowbounds);
            Vector<double> high = Vector.Build.Dense(highbounds);
            Vector<double> tmp = (high - low).Divide(_numOfParams);
            for (int i = 0; i < _numOfParams; i++)
            {
                thetaDelta.SetRow(i, tmp);
            }

            // Initialize Weights
            for (int i = 0; i < _numOfParams * _numOfParams; i++)
            {
                weightsVect[i] = GlobalVar.rnd.NextDouble();
            }

            weightsVect = weightsVect.Normalize(1).Multiply(_numOfParams * _numOfParams);

            var test = weightsVect.Sum() == _numOfParams*_numOfParams?1:0;

            UpdateID();
        }
    }
}
