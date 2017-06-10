using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using StatisticalApproach.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if false
namespace StatisticalApproach.CACO
{
    [Serializable]
    internal class CACO3
    {
        public Vector<double>[] thetas;
        public Vector<double>[] weights;
        public Vector<double>[] varThetas;
        public Vector<double>[] varWeights;
        public int rank = -1;
        public double omega;
        public int ceIndex = -1;
        int _numOfParams;
        int _dimension;
        private int _testSizeOfCEx = -1;

        public CACO3(int numOfParams, int dimension, int ceindex)
        {
            _numOfParams = numOfParams;
            _dimension = dimension;
            ceIndex = ceindex;
            thetas = new Vector<double>[numOfParams];
            weights = new Vector<double>[numOfParams + 1];
            varThetas = new Vector<double>[numOfParams];
            varWeights = new Vector<double>[numOfParams + 1];

            for (int i = 0; i < numOfParams; i++)
            {
                thetas[i] = Vector.Build.Dense(dimension); ;
                varThetas[i] = Vector.Build.Dense(dimension);
            }

            for (int i = 0; i < numOfParams + 1; i++)
            {
                weights[i] = Vector.Build.Dense(dimension); ;
                varWeights[i] = Vector.Build.Dense(dimension);
            }
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

                thetas[i] = newParam;
                tmp = newParam;
            }

            for (int i = 0; i < _numOfParams; i++)
            {
                Vector<double> newWeight = Vector.Build.Dense(_dimension, (k) =>
                {
                    return enVar.rnd.Next(0, _numOfParams + 1 - (int)tmpW[k]);
                });
                tmpW = newWeight + tmpW;
                weights[i] = newWeight;
            }
            weights[_numOfParams] = tmpW.SubtractFrom(_numOfParams + 1);
        }

        internal double[] CalculateVarianceForTheta<T>(double paramZeta, int currentParamIndex, Dictionary<T, double[]> solutionPool)
        {
            double[] tmpTheta = new double[_dimension];
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < solutionPool.Count; j++)
                {
                    double value = ((CACO3)(object)solutionPool.ElementAt(j).Key).thetas[currentParamIndex][i];
                    tmpTheta[i] = tmpTheta[i] + (Math.Abs(
                        value - thetas[currentParamIndex][i]) / (solutionPool.Count)
                    );
                }
                //ScatterplotBox.Show(data[i]);
                tmpTheta[i] = tmpTheta[i] * paramZeta;
            }

            return tmpTheta;
        }

        internal double[] CalculateVarianceForWeight<T>(double paramZeta, int currentWeightIndex, Dictionary<T, double[]> solutionPool)
        {
            double[] tmpWeight = new double[_dimension];
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < solutionPool.Count; j++)
                {
                    double value = ((CACO3)(object)solutionPool.ElementAt(j).Key).weights[currentWeightIndex][i];
                    tmpWeight[i] = tmpWeight[i] + (Math.Abs(
                        value - weights[currentWeightIndex][i]) / (solutionPool.Count)
                    );

                }
                //ScatterplotBox.Show(data[i]);
                tmpWeight[i] = tmpWeight[i] * paramZeta;
            }

            return tmpWeight;
        }

        internal void PheromoneUpdate<T>(double paramZeta, Dictionary<T, double[]> solutionPool)
        {

            // 1. Theta variance update
            for (int i = 0; i < _numOfParams; i++)
            {
                varThetas[i].SetValues(CalculateVarianceForTheta(paramZeta, i, solutionPool));
            }
            // 2. Weight variance update
            for (int i = 0; i < _numOfParams + 1; i++)
            {
                varWeights[i].SetValues(CalculateVarianceForWeight(paramZeta,i,solutionPool));
            }
        }

        internal void SolutionRankUpdate(int thisRank)
        {
            rank = thisRank;
        }

        internal void GaussianWeightUpdate(double paramQ, int poolSize)
        {
            omega = (1.0 / (paramQ * poolSize * Math.Sqrt(2 * Math.PI)))
                * Math.Pow(Math.E, -1 * (Math.Pow(rank - 1, 2) / (2 * Math.Pow(paramQ * poolSize, 2))));
        }

        internal void GetEstSizeAndTestSizeOfCEx(int testSizeOfCEx)
        {
            _testSizeOfCEx = testSizeOfCEx;
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
                double sumOfWeight = _numOfParams+1;
                double rndNum = enVar.rnd.NextDouble() * sumOfWeight;
                for (int j = 0; j < _numOfParams; j++)
                {
                    if (rndNum >= lowIndex && rndNum <= lowIndex + weights[j][i])
                    {
                        selIndexArry[i] = j;
                        break;
                    }
                    lowIndex = lowIndex + weights[j][i];
                }
            }

            double[] result = new double[_dimension];
            for (int i = 0; i < _dimension; i++)
            {
                if (selIndexArry[i] == 0)
                {
                    result[i] = enVar.rnd.Next((int)lowbounds[i], (int)thetas[0][i]);
                }
                else if (selIndexArry[i] == _numOfParams)
                {
                    result[i] = enVar.rnd.Next((int)thetas[_numOfParams - 1][i], (int)highbounds[i]);
                }
                else
                {
                    result[i] = enVar.rnd.Next((int)thetas[selIndexArry[i]-1][i],(int)thetas[selIndexArry[i]][i]);
                }
            }
            return Vector.Build.Dense(result);
        }
        internal double  CalEstFitness(EnvironmentVar enVar)
        {
            DenseMatrix m = new DenseMatrix((int)enVar.pmProblem["NumOfCE"], _testSizeOfCEx);
            DenseMatrix mInputVectors = new DenseMatrix(_testSizeOfCEx, _dimension);
            Vector<double> vCoverPointCounts = Vector.Build.Dense((int)enVar.pmProblem["NumOfCE"]);
            int estNumOfCEInSamples = 0;
            int duplicateVectsInSamples = 0;

            for (int i = 0; i < _testSizeOfCEx; i++)
            {
                Vector<double> vRandVect = GetRandVect(enVar);
                if (mInputVectors.EnumerateRows().Contains(vRandVect))
                {
                    duplicateVectsInSamples = duplicateVectsInSamples = 1;
                }
                else
                {
                    mInputVectors.SetRow(i,vRandVect);
                }
                double[] coverPoints = null;
                lock (SUT._locker)
                {
                    coverPoints = SUT.RunSUT(enVar, vRandVect.ToArray<double>());
                }
                if ((int)coverPoints[ceIndex] == 1)
                {
                    estNumOfCEInSamples = estNumOfCEInSamples + 1;
                }
            }
            double f1 = estNumOfCEInSamples * 1.0 / (_testSizeOfCEx); //Question: Should I consider Duplicates
            return f1;
        }
    }
}
#endif