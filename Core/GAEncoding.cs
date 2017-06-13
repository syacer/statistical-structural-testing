using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Core
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

        public Matrix<double> thetaDelta;
        Vector<double> numOfLabelsVect;
        public Matrix<double> weights;
        public int rank = -1;
        public int duplicateSamples = 0;
        public double entropy;
        int _numOfParams;
        int _dimension;

        public GAEncoding(int numOfParams, int dimension)
        {
            _numOfParams = numOfParams;
            _dimension = dimension;
            thetaDelta = Matrix.Build.Dense(numOfParams, dimension);
            weights = Matrix.Build.Dense(numOfParams + 1, dimension);
        }

        // Recombination: create a new solution, call this function;
        // Add the new solution to the population
        internal GAEncoding PanmicticRecomb(CEPool currentPool, int[] selectedList)
        {
            // Select solution to combine
            // Find Average
            Matrix<double> avgWeight = Matrix.Build.Dense(_numOfParams + 1, _dimension);

            for (int i = 0; i < selectedList.Length; i++)
            {
                avgWeight += currentPool.ElementAt(i).Key.weights;
            }
            avgWeight = avgWeight / selectedList.Length;
            // ... Check the sum of each row should be equal to a constant max
            GAEncoding newSolution = new GAEncoding(_numOfParams, _dimension);
            newSolution.weights = avgWeight;

            return newSolution;
        }

        //Simple Mutation Assumes independence of each variable
        internal void PermutationMutation(int permuationSize)
        {
            //Shift N weight vector
            Queue<Vector<double>> weightVector = new Queue<Vector<double>>();
            // grab last N weight vector, insert into Queue
            for (int i = 0; i < permuationSize; i++)
            {
                weightVector.Enqueue(Copy.DeepCopy(weights.Row(_numOfParams - i)));
            }
            for (int i = 0; i < _numOfParams + 1 - permuationSize; i++)
            {
                var weightVect = Copy.DeepCopy(weights.Row(_numOfParams - i - permuationSize));
                weights.SetRow(_numOfParams - i, weightVect.ToArray());
            }
            int j = 0;
            while (weightVector.Count != 0)
            {
                weights.SetRow(permuationSize - j - 1, weightVector.Dequeue());
                j = j + 1;
            }
        }

        internal Vector<double> GetRandVect(double[] lowbounds, double[] highbounds)
        {
            int[] selIndexArry = new int[_dimension];
            
            // First, Select a block
            for (int i = 0; i < _dimension; i++)
            {
                double lowIndex = 0.0;
                double sumOfWeight = _numOfParams + 1;
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
                else if (selIndexArry[i] == _numOfParams)
                {
                    double min = thetaDelta.Column(i).Sum()
                        - thetaDelta.Row(_numOfParams - 1).ElementAt(i);
                    result[i] = GlobalVar.rnd.Next((int)min, (int)highbounds[i]);
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
                numOfLabelsVect[label-1] += 1;
            }
            // Calculate p of Labals
            numOfLabelsVect = numOfLabelsVect.Divide(testSetSize);
            entropy = 0;
            for (int i = 0; i < numOfLabel; i++)
            {
                entropy += numOfLabelsVect[i] * Math.Log10(1.0/(numOfLabelsVect[i]+0.000001));
            }
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
                    return GlobalVar.rnd.Next((int)tmp[k], (int)highbounds[k]);
                });
                thetaDelta.SetRow(i, newParam - tmp);
                tmp = newParam;
            }

            for (int i = 0; i < _numOfParams; i++)
            {
                Vector<double> newWeight = Vector.Build.Dense(_dimension, (k) =>
                {
                    return GlobalVar.rnd.Next(0, _numOfParams + 1 - (int)tmpW[k]);
                });
                tmpW = newWeight + tmpW;
                weights.SetRow(i, newWeight.ToArray());
            }
            weights.SetRow(_numOfParams, tmpW.SubtractFrom(_numOfParams + 1).ToArray());
        }

        internal void FixInputs(double[] lowbounds, double[] highbounds)
        {
            Vector<double> low = Vector.Build.Dense(lowbounds);
            Vector<double> high = Vector.Build.Dense(highbounds);
            Vector<double> tmp = (high - low).Add(1).Divide(_numOfParams);
            for (int i = 0; i < _numOfParams; i++)
            {
                thetaDelta.SetRow(i, tmp);
            }

            int numOfOne = Convert.ToInt16(Math.Ceiling((_numOfParams + 1) * 0.2));
            int numOfNotOne = (_numOfParams - numOfOne) / 2;

            for (int i = 0; i < _numOfParams + 1; i++)
            {
                weights = Matrix.Build.Dense(_numOfParams + 1, _dimension, 1);
            }

            int[] cnt = new int[_dimension];
            double[] values = new double[2] { 1.5, 0.5 };

            for (int j = 0; j < 2; j++)
            {
                Array.Clear(cnt, 0, cnt.Length);
                while (cnt.Any(c => c < numOfNotOne))
                {
                    Vector<double> newIndexes = Vector.Build.Dense(_dimension, (k) =>
                    {
                        return GlobalVar.rnd.Next(0, _numOfParams + 1);
                    });
                    for (int i = 0; i < _dimension; i++)
                    {
                        if (cnt[i] == numOfNotOne)
                        {
                            continue;
                        }
                        else if (weights[(int)newIndexes[i], i] == 1)
                        {
                            weights[(int)newIndexes[i], i] = values[j];
                            cnt[i] += 1;
                        }
                    }
                }
            }
        }
    }
}
