﻿using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using StatisticalApproach.Framework;
using MathNet.Numerics.Distributions;
using Accord.Statistics.Distributions.Multivariate;

namespace StatisticalApproach.MOGA
{
    using StatisticalApproach.Utility;

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
        public Dictionary<string, double> pathRecord;
        public Matrix<double> thetaDelta;   
        public Matrix<double> weights;
        public Vector<double> fitnessPerCE;
        public int rank = -1;
        public int duplicateSamples = 0;
        public double entropy;
        public string strTestSet;
        int _numOfParams;
        int _dimension;
        EnvironmentVar _enVar;

        public GAEncoding(int numOfParams, int dimension, EnvironmentVar enVar)
        {
            _numOfParams = numOfParams;
            _dimension = dimension;
            _enVar = enVar;
            thetaDelta = Matrix.Build.Dense(numOfParams,dimension);
            weights = Matrix.Build.Dense(numOfParams + 1, dimension);
            pathRecord = new Dictionary<string, double>();
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
                avgWeight += currentPool.ElementAt(selectedList[i]).Key.weights;
            }
            avgWeight = avgWeight / selectedList.Length;
            // ... Check the sum of each row should be equal to a constant max
            GAEncoding newSolution = new GAEncoding(_numOfParams,_dimension,_enVar);
            newSolution.FixInputs(_enVar);
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
                weightVector.Enqueue(Copy.DeepCopy(weights.Row(_numOfParams-i)));
            } 
            for (int i = 0; i < _numOfParams + 1-permuationSize; i++)
            {
                var weightVect = Copy.DeepCopy(weights.Row(_numOfParams - i - permuationSize));
                weights.SetRow(_numOfParams - i, weightVect.ToArray());
            }
            int j = 0;
            while (weightVector.Count != 0)
            {
                weights.SetRow(permuationSize-j-1, weightVector.Dequeue());
                j = j + 1;
            }
        }

        internal Vector<double> GetRandVect(EnvironmentVar enVar)
        {
            int[] selIndexArry = new int[_dimension];

            double[] lowbounds = new double[_dimension];
            double[] highbounds = new double[_dimension];
            for (int o = 0; o < _dimension; o++)
            {
                lowbounds[o] = ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[o].Item1;
                highbounds[o] = ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[o].Item2;
            }

            // First, Select a block
            for (int i = 0; i < _dimension; i++)
            {
                double lowIndex = 0.0;
                double sumOfWeight = weights.ColumnSums()[i];
                double rndNum = enVar.rnd.NextDouble() * sumOfWeight;
                for (int j = 0; j < _numOfParams; j++)
                {
                    if (rndNum >= lowIndex && rndNum <= lowIndex + weights[j,i])
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
                    result[i] = enVar.rnd.Next((int)lowbounds[i], (int)thetaDelta[i,0]);
                }
                else if (selIndexArry[i] == _numOfParams)
                {
                    double min = thetaDelta.Column(i).Sum() 
                        - thetaDelta.Row(_numOfParams-1).ElementAt(i);
                    result[i] = enVar.rnd.Next((int)min, (int)highbounds[i]);
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
                    result[i] = enVar.rnd.Next((int)min, (int)max);
                }
            }
            return Vector.Build.Dense(result);
        }
        internal void CalEstFitness(EnvironmentVar enVar)
        {
            DenseMatrix m = new DenseMatrix((int)enVar.pmProblem["NumOfCE"], _enVar.testSetSize);
            DenseMatrix mInputVectors = new DenseMatrix(_enVar.testSetSize, _dimension);
            DenseMatrix mCoverPairs = new DenseMatrix(((int)_enVar.pmProblem["NumOfCE"]+1)/2,2);  //2 outgoing edges per branch
            Vector<double> vIntersetSet = new DenseVector(((int)_enVar.pmProblem["NumOfCE"] + 1) / 2);
            duplicateSamples = 0;
            fitnessPerCE = new DenseVector((int)_enVar.pmProblem["NumOfCE"]);

            pathRecord.Clear();
            for (int i = 0; i < _enVar.testSetSize; i++)
            {
                Vector<double> vRandVect = GetRandVect(enVar);
                if (mInputVectors.EnumerateRows().Contains(vRandVect))
                {
                    duplicateSamples = duplicateSamples + 1;
                }
                else
                {
                    mInputVectors.SetRow(i, vRandVect);
                }
                double[] coverPoints = null;
                lock (SUT._locker)
                {
                    coverPoints = SUT.RunSUT(enVar, vRandVect.ToArray<double>());
                }
                string tmp = null;
                for (int o = 0; o < coverPoints.Length; o++)
                {
                    if (o < (int)_enVar.pmProblem["NumOfCE"])
                    {
                        if (coverPoints[o] == 1)
                        {
                            tmp = tmp + o.ToString() + " ";
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (pathRecord.Keys.Contains(tmp))
                {
                    pathRecord[tmp] = pathRecord[tmp] + 1;
                }
                else
                {
                    pathRecord.Add(tmp,1);
                }
                var tm = Vector.Build.Dense(coverPoints
                    .Where((x, j) => j <= (int)_enVar.pmProblem["NumOfCE"] - 1).ToArray());
                fitnessPerCE = fitnessPerCE.Add(tm);

                List<int> indexList = coverPoints.Select((x, j) =>
                {
                    if (x == 1)
                    {
                        return j;
                    }
                    return -1;
                }).Where(y=>y != -1).ToList();

                // Construct Cover Element pair count Matrix
                // |c0, c1|
                // |c2, c3|
                // |c4, c5|
                // ........................................
                double[] duplicates = new double[((int)_enVar.pmProblem["NumOfCE"] + 1) / 2];
                for (int j = 0; j < indexList.Count; j++)
                {
                    int index = indexList[j];
                    int rowIndex = index / 2;
                    int colIndex = index % 2;
                    Vector<double> vTmp = mCoverPairs.Row(rowIndex);
                    vTmp[colIndex] = vTmp[colIndex] + 1;
                    mCoverPairs.SetRow(rowIndex, vTmp.ToArray());
                    duplicates[rowIndex] = duplicates[rowIndex] + 1;
                }
                double[] tmpVect = duplicates.Select(x =>
                {
                    if (x > 0)
                    {
                        return x - 1;
                    }
                    else
                    {
                        return 0;
                    }
                }).ToArray();
                vIntersetSet = vIntersetSet.Add(Vector.Build.Dense(tmpVect));
            }

            for (int o = 0; o < pathRecord.Count; o++)
            {
                pathRecord[pathRecord.ElementAt(o).Key] /= (enVar.testSetSize*1.0);
            }
            strTestSet = mInputVectors.ToString(100,_dimension)
                .Replace("\r","").Replace("\n",";").Replace("  ","_");
            // Calculate P(In) vector
            Vector<double> vPin = mCoverPairs.RowSums().Subtract(vIntersetSet)/enVar.testSetSize;
            Matrix<double> mNormalizeRow = mCoverPairs.Subtract(
                Matrix.Build.DenseOfColumnArrays(vIntersetSet.ToArray(), vIntersetSet.ToArray())
                ).NormalizeRows(1);
            Matrix<double> mTmp = mNormalizeRow.Transpose().Add(0.00001).PointwisePower(-1).PointwiseLog10();
            var mEye = DiagonalMatrix.Build.DiagonalOfDiagonalArray(vPin.ToArray());
            var fit1Of2 = mNormalizeRow.Multiply(mTmp);
            var fit2Of2 = fit1Of2.Multiply(mEye).Diagonal();
            entropy = fit2Of2.Sum();
            fitnessPerCE = fitnessPerCE.Divide(_enVar.testSetSize);
            duplicateSamples = 1; //For Testing Only...
        }

        internal void Randomize(EnvironmentVar enVar)
        {
            Vector<double> tmp = Vector.Build.Dense(
                _dimension,
                (k) =>
                {
                    return ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[k].Item1;
                }
            );
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
                    return enVar.rnd.Next((int)tmp[k]
                        , (((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[k].Item2));
                });
                thetaDelta.SetRow(i, newParam - tmp);
                tmp = newParam;
            }

            for (int i = 0; i < _numOfParams; i++)
            {
                Vector<double> newWeight = Vector.Build.Dense(_dimension, (k) =>
                {
                    return enVar.rnd.Next(0, _numOfParams + 1 - (int)tmpW[k]);
                });
                tmpW = newWeight + tmpW;
                weights.SetRow(i,newWeight.ToArray());
            }
            weights.SetRow(_numOfParams, tmpW.SubtractFrom(_numOfParams + 1).ToArray());
        }

        internal void FixInputs(EnvironmentVar enVar)
        {
            Vector<double> low = Vector.Build.Dense(
                _dimension,
                (k) =>
                {
                    return ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[k].Item1;
                }
            );
            Vector<double> high = Vector.Build.Dense(
                _dimension,
                (k) =>
                {
                    return ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[k].Item2;
                }
            );
            Vector<double> tmp = (high - low).Add(1).Divide(_numOfParams);
            for (int i = 0; i < _numOfParams; i++)
            {
                thetaDelta.SetRow(i, tmp);
            }

            int numOfOne = Convert.ToInt16(Math.Ceiling((_numOfParams + 1) * 0.2));
            int numOfNotOne = (_numOfParams - numOfOne) / 2;

            for (int i = 0; i < _numOfParams+1; i++)
            {
                weights = Matrix.Build.Dense(_numOfParams+1,_dimension,1);
            }

            int[] cnt = new int[_dimension];
            double[] values = new double[2] { 0, 2 };

            // Initialize Weights
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < _numOfParams + 1; j++)
                {
                    weights[j, i] = enVar.rnd.NextDouble();
                }
            }
        }
    }
}
