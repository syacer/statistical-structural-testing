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
        public Matrix<double> weights;
        public int rank = -1;
        public int duplicateSamples = 0;
        public double entropy = 0;
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
            weights = Matrix.Build.Dense(numOfParams, dimension);
        }

        public void UpdateID()
        {
            id = Guid.NewGuid().ToString();
        }
        public void TrueFitnessCal(int numOfLabel, Matrix<double>labelMatrix)
        {
            numOfLabelsVect = Vector<double>.Build.Dense(numOfLabel);
            double totalWeights = (weights.ColumnSums()[0] + weights.Column(0)[weights.Column(0).Count-1])
                * (weights.ColumnSums()[1] + weights.Column(1)[weights.Column(1).Count - 1]);
            double vectWeight = 0;
            double temp = 0;
            for (double i = _lowbounds[0]; i <= _highbounds[0]; i++)
            {
                for (double j = _lowbounds[1]; j <= _highbounds[1]; j++)
                {
                    vectWeight = WeightsFinder(new double[] {i,j })/totalWeights;
                    temp = vectWeight + temp;
                    int label = (int)labelMatrix.At((int)i, (int)j);
                    if (label <= numOfLabel)
                    {
                        numOfLabelsVect[label - 1] += vectWeight;
                    }

                }
            }
            
            double entropy = 0;
            for (int i = 0; i < numOfLabel; i++)
            {
                entropy += numOfLabelsVect[i] * Math.Log10(1.0 / (numOfLabelsVect[i] + 0.000001));
            }
        }
        public double WeightsFinder(double[] input)
        {
            double[] resultWeights = new double[_dimension];
            for (int i = 0; i < _dimension; i++)
            {
                double temp = _lowbounds[i];
                for (int j = 0; j < _numOfParams; j++)
                {
                    if (input[i] <= temp)
                    {
                        resultWeights[i] = weights[j, i];
                        break;
                    }
                    else
                    {
                        temp = temp + thetaDelta[j,i];
                        if (temp == _highbounds[i])
                        {
                            resultWeights[i] = weights[j, i];
                        }
                    }
                }
            }

            return resultWeights[0] * resultWeights[1];
        }
        // Recombination: create a new solution, call this function;
        // Add the new solution to the population
        internal GAEncoding IntermidinateRecomb(CEPool currentPool, int[] selectedList)
        {
            double d = 0.25;
            GAEncoding newSolution = new GAEncoding(_numOfParams, _dimension, _lowbounds, _highbounds);
            double alpha = GlobalVar.rnd.NextDouble();
            alpha = (1 + d - -1 * d) * alpha + -1 * d;
            var deltaW = currentPool.ElementAt(selectedList[1]).Key.weights
                       - currentPool.ElementAt(selectedList[0]).Key.weights;
            newSolution.weights = 
                deltaW.Multiply(alpha) + currentPool.ElementAt(selectedList[0]).Key.weights;

            var test = weights.ColumnSums()[0] * weights.ColumnSums()[1];

            newSolution.FixInputs(_lowbounds, _highbounds);
            newSolution.UpdateID();
            return newSolution;
        }
        internal GAEncoding IntermidinateRecomb_old(CEPool currentPool, int[] selectedList)
        {
            double d = 0.25;
            GAEncoding newSolution = new GAEncoding(_numOfParams, _dimension, _lowbounds, _highbounds);
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < _numOfParams; j++)
                {
                    double alpha = GlobalVar.rnd.NextDouble();
                    alpha = (1 + d - -1 * d) * alpha + -1 * d;
                    newSolution.weights[j, i] =
                        currentPool.ElementAt(selectedList[0])
                        .Key.weights[j, i] * alpha +
                        currentPool.ElementAt(selectedList[1])
                        .Key.weights[j, i] * (1 - alpha);
                    if (newSolution.weights[j, i] > 1)
                    {
                        newSolution.weights[j, i] = 1;
                    }
                    if (newSolution.weights[j, i] <= 0)
                    {
                        newSolution.weights[j, i] = 0.00001;
                    }
                }
            }
            newSolution.FixInputs(_lowbounds, _highbounds);
            newSolution.UpdateID();
            return newSolution;
        }

        internal GAEncoding PanmicticDiscreteRecomb(CEPool currentPool, int[] selectedList)
        {

            GAEncoding newSolution = new GAEncoding(_numOfParams, _dimension, _lowbounds, _highbounds);

            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < _numOfParams; j++)
                {
                    int selectedIndex = selectedList[GlobalVar.rnd.Next(0, selectedList.Length - 1 + 1)];
                    newSolution.weights[j, i] = currentPool.ElementAt(selectedIndex).Key.weights[j, i];
                    if (newSolution.weights[j, i] > 1)
                    {
                        newSolution.weights[j, i] = 1;
                    }
                    if (newSolution.weights[j, i] <= 0)
                    {
                        newSolution.weights[j, i] = 0.00001;
                    }
                }
            }
            newSolution.FixInputs(_lowbounds, _highbounds);
            newSolution.UpdateID();
            return newSolution;
        }
        //Simple Mutation Assumes independence of each variable
        internal GAEncoding RealVectMutation()
        {
            GAEncoding newSolution = Copy.DeepCopy(this);
            UpdateID();
            newSolution.FixInputs(_lowbounds, _highbounds);

            for (int i = 0; i < _numOfParams; i++)
            {
                for (int j = 0; j < _numOfParams; j++)
                {
                    if (GlobalVar.rnd.NextDouble()
                        <= 1.0 / (_numOfParams * _numOfParams))
                    {
                        double k_MutPrecision = 1;
                        double u = GlobalVar.rnd.NextDouble();
                        int s_Direction = GlobalVar.rnd.NextDouble() <= 0.5 ? -1 : 1;
                        double a_StepSize1 = 10*u + s_Direction;

                        u = GlobalVar.rnd.NextDouble();
                        s_Direction = GlobalVar.rnd.NextDouble() <= 0.5 ? -1 : 1;
                        double a_StepSize2 = 10*u + s_Direction;
                        Vector<double> mean = weights.ColumnSums().Divide(_numOfParams);
                        weights[i, 0] = weights[i, 0] * a_StepSize1 + mean[0] * (1 - a_StepSize1);
                        weights[j, 1] = weights[j, 1] * a_StepSize2 + mean[1] * (1 - a_StepSize2);
                        if (weights[i, 0] <= 0)
                        {
                            weights[i, 0] = 0.0001;
                        }
                        if (weights[j, 1] <= 0)
                        {
                            weights[j, 1] = 0.0001;
                        }
                    }
                }
            }

            // Normalize
            for (int i = 0; i < _dimension; i++)
            {
                double total = weights.ColumnSums()[i];
                for (int j = 0; j < _numOfParams; j++)
                {
                    weights[j, i] = (weights[j, i] / total) * _numOfParams;
                }
            }
            var test = weights.ColumnSums()[0] * weights.ColumnSums()[1];

            return newSolution;
        }
        //Simple Mutation Assumes independence of each variable
        internal void RealVectMutation_old()
        {
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < _numOfParams; j++)
                {
                    if (GlobalVar.rnd.NextDouble() 
                        <= 1.0 / (_numOfParams * _dimension))
                    {
                        int s_Direction = GlobalVar.rnd.NextDouble() <= 0.5 ? -1 : 1;
                        double r_MutRange = 0.1 * 1; //1: domain range
                        double k_MutPrecision = 16;
                        double u = GlobalVar.rnd.NextDouble();
                        double a_StepSize = Math.Pow(2, -1 * u * k_MutPrecision);
                        weights[j, i] += s_Direction * r_MutRange * a_StepSize;

                        if (weights[j, i] > 1)
                        {
                            weights[j, i] = 1;
                        }
                        if (weights[j, i] <= 0)
                        {
                            weights[j, i] = 0.00001;
                        }
                    }
                }
            }
            UpdateID();
        }

        internal Vector<double> GetRandVect(double[] lowbounds, double[] highbounds)
        {
            int[] selIndexArry = new int[_dimension];
            
            // First, Select a block
            for (int i = 0; i < _dimension; i++)
            {
                double lowIndex = 0.0;
                double sumOfWeight = weights.ColumnSums()[i];
                double rndNum = GlobalVar.rnd.NextDouble() * sumOfWeight;
                for (int j = 0; j < _numOfParams; j++)
                {
                    if (rndNum >= lowIndex && rndNum <= lowIndex + weights[j, i])
                    {
                        selIndexArry[i] = j;
                        break;
                    }
                    lowIndex = lowIndex + weights[j, i];
                }
            }
            
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

        internal void Randomize(double[] lowbounds, double[] highbounds)
        {
            Vector<double> tmp = Vector.Build.Dense(lowbounds);
            Vector<double> tmpW = Vector.Build.Dense(
                _dimension,
                (k) =>
                {
                    return 0;
                }
            );

            for (int i = 0; i < _numOfParams; i++)
            {
                Vector<double> newParam = Vector.Build.Dense(_dimension, (k) =>
                {
                    return GlobalVar.rnd.Next((int)tmp[k], (int)highbounds[k] + 1);
                });
                thetaDelta.SetRow(i, newParam - tmp);
                tmp = newParam;
            }

            for (int i = 0; i < _numOfParams; i++)
            {
                Vector<double> newWeight = Vector.Build.Dense(_dimension, (k) =>
                {
                    return GlobalVar.rnd.Next(0, _numOfParams - (int)tmpW[k] + 1);
                });
                tmpW = newWeight + tmpW;
                weights.SetRow(i, newWeight.ToArray());
            }
            weights.SetRow(_numOfParams, tmpW.SubtractFrom(_numOfParams).ToArray());
        }

        internal void FitnessCal(Matrix<double> aMatrix)
        {
            // First, Calculate Total Weights;
            // And create weight Vector from weight matrix
            double totalWeights = 0;
            entropy = 0;
            Vector<double> weightVect = Vector<double>.Build
                .Dense((int)Math.Pow(_numOfParams,2));
            for (int i = 0; i < _numOfParams; i++)
            {
                for (int j = 0; j < _numOfParams; j++)
                {
                    double weight = weights[j, 0] * weights[i, 1];
                    totalWeights += weight;
                    int vVectIndex = j + i * _numOfParams;
                    weightVect[vVectIndex] = weight;
                }
            }

            // Calculate Entropy
            numOfLabelsVect2 = Vector<double>.Build.Dense(aMatrix.ColumnCount);
            
            for (int i = 0; i < aMatrix.ColumnCount; i++)
            {
                double temp = aMatrix.Column(i).ToRowMatrix().Multiply(weightVect).First();
                double pOfLabel = temp / totalWeights;
                numOfLabelsVect2[i] = pOfLabel;
                entropy += pOfLabel*Math.Log(Math.Pow(pOfLabel+0.000001,-1),2);
            }

            // Only Testing
            //double entropy2 = 0;
            //Vector<double> weightVect2 = Vector<double>.Build
            //    .Dense((int)Math.Pow(_numOfParams, 2));
            //weightVect2 = weightVect2.Add(1);
            //totalWeights = weightVect2.Sum();
            //for (int i = 0; i < aMatrix.ColumnCount; i++)
            //{
            //    double temp = aMatrix.Column(i).ToRowMatrix().Multiply(weightVect2).First();
            //    double pOfLabel = temp / totalWeights;
            //    numOfLabelsVect2[i] = pOfLabel;
            //    entropy2 += pOfLabel * Math.Log(Math.Pow(pOfLabel + 0.000001, -1), 2);
            //}
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
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < _numOfParams; j++)
                {
                    weights[j, i] = GlobalVar.rnd.NextDouble();
                }
            }
            double sumWeights = weights.ColumnSums()[0];
            for (int j = 0; j < _numOfParams; j++)
            {
                weights[j, 0] = (weights[j,0] / sumWeights) *_numOfParams;
            }
            sumWeights = weights.ColumnSums()[1];
            for (int j = 0; j < _numOfParams; j++)
            {
                weights[j, 1] = (weights[j, 1] / sumWeights) * _numOfParams;
            }
            var test = weights.ColumnSums()[0] * weights.ColumnSums()[1];
            UpdateID();
        }
    }
}
