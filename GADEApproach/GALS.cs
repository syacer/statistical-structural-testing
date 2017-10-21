using DeterministicApproach_GA;
using MathNet.Numerics.LinearAlgebra;
using StatisticalApproach_GA;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Accord.Math;

namespace GADEApproach
{
    [Serializable]

    public class Pair<T0, T1, T2>
    {
        public T0 BinIndex { set; get; }
        public T1 SetIndexPlus1 { set; get; }
        public T2 ProbInBins { set; get; }
    }
    public class Pair<T0,T1,T2,T3,T4,T5>
    {
        public T0 Index { set; get; }
        public T1 ProbLowBound { set; get; }
        public T2 binsSetup { set; get; }
        public T3 AMatrix { set; get; }
        public T4 Variance { set; get; }
        public T5 Rank { set; get; }
    }
    public class record
    {
        public double[] fitnessGen;
        public double[] probLowBoundGen;
        public solution bestSolution;
    }
    public class solution
    {
        //public int[][] inputsinSets; // [1][2] : the second inputs in set 1, Obsolete variable
        public int[] binSetup;
        public double[] setProbabilities;
        public long totalRunTime;
        public Matrix<double> Amatrix;
    }

    [Serializable]
    public class GALS
    {
        int generation = 0;
        double[] expTriProb;
        //Multi-Threading Setup
        int numberOfConcurrentTasks = 20;
        int numOfLabels = -1;
        //GA Setup
        public int maxGen = 5000;
        public int populationSize = 100;
        double crRate = 0.9;
        double mtRate = 0.2;
        double mtPropotion = 0.2;
        double IMfactor = 1;
        //index, fitness, Bins<Index,SetIndex,tri Probs. in each bin>, AMatrix, prob. low bound
        Pair<int, double,Pair<int, int, double[]>[], Matrix<double>,double,double>[] pool; //index, fitness, bins
        double[][] coverPointSetProb;

        //SUT Setup
        SUT sut = null;
        double minGoodnessOfFit = 9999999;

        public GALS(int numOfMaxGen, int numLabels, SUT SUT, int imFactor)
        {
            maxGen = numOfMaxGen;
            numOfLabels = numLabels;
            sut = SUT;
            IMfactor = imFactor;
        }

        public void WraperForCreateAMatrix(
            Pair<int,int,double[]>[] encoding,
            out int[] binsInSets,
            out double[][] binsSetup)
        {
            binsInSets = encoding.Select(x => x.SetIndexPlus1).ToArray();
            binsSetup = encoding.Select(x => x.ProbInBins).ToArray();
        }
        public void AlgorithmStart(record record)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int[] token = new int[1];
            FitnessEvaluationNoParll(token);
            MOGA_NormalizeFitness();
            while (token[0] == 0) ;
            token[0] = 0;

            Queue<double> last20fitness = new Queue<double>();
            while (generation < maxGen)
            {
                Reproduction();
                FitnessEvaluationNoParll(token);
                MOGA_NormalizeFitness();
                while (token[0] == 0) ;
                token[0] = 0;

                // Start Recording
                watch.Stop();

#if MOGA == true
                var orderedSolutions = pool.OrderByDescending(x => x.ProbLowBound);
#else
                var rankOneSolutions = pool.Where(X => X.Rank == pool.Max(y => y.Rank)).ToArray();
                double avgProbLowbound = rankOneSolutions.Select(x => x.ProbLowBound).Sum() / rankOneSolutions.Count();
                double avgVariance = rankOneSolutions.Select(x => x.Variance).Sum() / rankOneSolutions.Count();
                var arrayOfValue = rankOneSolutions.Select(x=>Math.Pow(x.Variance - avgVariance,2) + Math.Pow(x.ProbLowBound - avgProbLowbound,2)).ToArray();
                int bestSolutionIndex = Array.IndexOf(arrayOfValue,arrayOfValue.Min());
                var bestSolution = rankOneSolutions.ElementAt(bestSolutionIndex);
#endif
                Console.WriteLine("Run: {0}, lowBound: {1}, Var: {2}", generation, bestSolution.ProbLowBound, bestSolution.Variance);

                record.fitnessGen[generation] = bestSolution.ProbLowBound;
                record.probLowBoundGen[generation] = bestSolution.Variance;
                watch.Start();

                if (last20fitness.Count == 100)
                {
                    last20fitness.Dequeue();
                    last20fitness.Enqueue(1.0 / bestSolution.ProbLowBound);
                    if (last20fitness.All(x => Math.Round(x,3) == Math.Round(last20fitness.First(),3)))
                    {
                        break;
                    }
                }
                else
                {
                    last20fitness.Enqueue(1.0 / bestSolution.ProbLowBound);
                }

                generation += 1;
            }

            watch.Stop();

            var BestrankOneSolutions = pool.Where(X => X.Rank == pool.Max(y => y.Rank)).ToArray();
            double BestavgProbLowbound = BestrankOneSolutions.Select(x => x.ProbLowBound).Sum() / BestrankOneSolutions.Count();
            double BestavgVariance = BestrankOneSolutions.Select(x => x.Variance).Sum() / BestrankOneSolutions.Count();
            var BestarrayOfValue = BestrankOneSolutions.Select(x => Math.Pow(x.Variance - BestavgVariance, 2) + Math.Pow(x.ProbLowBound - BestavgProbLowbound, 2)).ToArray();
            int BsetbestSolutionIndex = Array.IndexOf(BestarrayOfValue, BestarrayOfValue.Min());
            var BestbestSolution = BestrankOneSolutions.ElementAt(BsetbestSolutionIndex);

            record.bestSolution.binSetup = Copy.DeepCopy(BestbestSolution.binsSetup.Select(x => x.SetIndexPlus1).ToArray());
            var totalRuningTime = watch.ElapsedMilliseconds;
            record.bestSolution.totalRunTime = totalRuningTime;
            record.bestSolution.setProbabilities = Copy.DeepCopy(
                coverPointSetProb[BestbestSolution.Index]);
            record.bestSolution.Amatrix = Copy.DeepCopy(BestbestSolution.AMatrix);

        }

        public void GAInitialization()
        {
            pool = new Pair<int, double, Pair<int, int, double[]>[], Matrix<double>,double,double>[populationSize];
            coverPointSetProb = new double[populationSize][];

            for (int i = 0; i < populationSize; i++)
            {
                pool[i] = new Pair<int, double, Pair<int, int, double[]>[], Matrix<double>,double,double>();
                pool[i].Index = i;
                pool[i].ProbLowBound = -1;
                pool[i].binsSetup = Copy.DeepCopy(sut.bins);
                for (int j = 0; j < pool[i].binsSetup.Length; j++)
                {
                    pool[i].binsSetup[j].SetIndexPlus1 = GlobalVar.rnd.Next(1,numOfLabels+1);
                }
                SolutionRepair(pool[i].binsSetup);
            }
        }
        private void SolutionRepair(Pair<int, int, double[]>[] bins)
        {
            // If a cover element set doesn't contains bins, Swap the bin which has the max tri-prob with the cover
            // element set
            for (int i = 0; i < numOfLabels; i++)
            {
                List<int> InValidBinIndex = new List<int>();
                while (true)
                {
                    if (bins.Where(x => x.SetIndexPlus1 == i + 1).Count() == 0)
                    {
                        int binIndex = bins.Select(y =>
                        {
                            double MaxValidTriProb = bins.Max(x =>
                            {
                                return !InValidBinIndex.Contains(x.BinIndex) ? x.ProbInBins[i] : -10;
                            });
                            if (y.ProbInBins[i] == MaxValidTriProb
                                && !InValidBinIndex.Contains(y.BinIndex))
                            {
                                return y.BinIndex;
                            }
                            return -1;
                        }).Where(x => x != -1).First();

                        if (bins.Where(x => x.SetIndexPlus1 == bins[binIndex].SetIndexPlus1).Count() > 1)
                        {
                            bins[binIndex].SetIndexPlus1 = i + 1;
                            break;
                        }
                        else
                        {
                            InValidBinIndex.Add(binIndex);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //Validation Check, at least one bin in each cover element set
            for (int i = 0; i < numOfLabels; i++)
            {
                if (bins.Where(x => x.SetIndexPlus1 == i + 1).Count() == 0)
                {
                    Console.WriteLine("Invalid label {0}", i);
                }
            }
        }

        public Matrix<double> AmatrixCalculation(int id)
        {

            Matrix<double> Amatrix = Matrix<double>.Build.Dense(numOfLabels, numOfLabels);

            for (int i = 0; i < numOfLabels; i++)
            {
                for (int j = 0; j < numOfLabels; j++)
                {
                    Amatrix[i, j] = pool[id].binsSetup.Where(x => x.SetIndexPlus1 == j + 1).Sum(x => x.ProbInBins[i]);
                    Amatrix[i, j] /= pool[id].binsSetup.Where(x => x.SetIndexPlus1 == j + 1).Count();
                }
            }
            return Amatrix;
        }
       //using the min(Pbranch)
        public void FitnessEvaluationNoParll(int[] token)
        {
            for (int k = 0; k < populationSize; k++)
            {
                //Console.Write("task #{0}", id);
                Matrix<double> Amatrix = null;
                int[] binsInSets = null;
                int[] newBinsInSets = null;
                double[][] binsSetup = null;

                if (!(generation != 0 && k == 0))
                {
                    WraperForCreateAMatrix(pool[k].binsSetup, out binsInSets, out binsSetup);
                    ConvertInvertibleAMatrix.CreateInvertibleAMatrix(
                        ref Amatrix, binsInSets, binsSetup, out newBinsInSets, numOfLabels);

                    // New Add begins
                    for (int i = 0; i < pool[k].binsSetup.Length; i++)
                    {
                        pool[k].binsSetup[i].SetIndexPlus1 = newBinsInSets[i];
                    }

                    // Check above to see if the maximum is less than ~17/8/9， should not reach 42
                    pool[k].AMatrix = Amatrix;
                }
                else
                {
                    Amatrix = pool[k].AMatrix;
                    newBinsInSets = pool[k].binsSetup.Select(x => x.SetIndexPlus1).ToArray();
                }

                // End of section

                double minInRow = 9999;
                int colIndexOfMin = -1;
                for (int i = 0; i < Amatrix.RowCount; i++)
                {
                    double maxInCol = -1;
                    int colIndexOfMax = -1;
                    for (int j = 0; j < Amatrix.ColumnCount; j++)
                    {
                        if (Amatrix[i, j] > maxInCol)
                        {
                            maxInCol = Amatrix[i, j];
                            colIndexOfMax = j;
                        }
                    }
                    if (maxInCol < minInRow)
                    {
                        minInRow = maxInCol;
                        colIndexOfMin = colIndexOfMax;
                    }
                }

                int[] sizeOfSetsAry = new int[Amatrix.ColumnCount];
                int maxSize = -1;
                int colIndexOfMaxSize = -1;
                for (int i = 0; i < Amatrix.ColumnCount; i++)
                {
                    sizeOfSetsAry[i] = newBinsInSets.Count(x => x==i+1);
                    if (sizeOfSetsAry[i] > maxSize)
                    {
                        maxSize = sizeOfSetsAry[i];
                        colIndexOfMaxSize = i;
                    }
                }

                double SVar = (Math.Pow(newBinsInSets.Count(x=> x== colIndexOfMin+1),2)-1)/12;
                double XVar = (Math.Pow(maxSize,2)-1)/12;
                double SMinTri = Amatrix.Column(colIndexOfMaxSize).Min();
                double XMinTri = minInRow;

                double w1 = 0;
                if (Math.Round(XMinTri - SMinTri, 4) != 0)
                {
                    w1 = (XMinTri * IMfactor - SMinTri) / (XMinTri - SMinTri);
                }
                double w1t = 0;
                if (Math.Round(XVar + SVar, 4) != 0)
                {
                    w1t = 2 * XVar / (XVar + SVar) - 1;
                }

                if (w1 >= w1t)
                {
                    w1 = 1;
                }

                double w2 = 1 - w1;

                // fitness = P/PMax + var/varMax
                // PMax = 1, VarMax = numofBins * numOfBins

                double maxVar = (Math.Pow(pool[k].binsSetup.Length,2)+1)/12;
                double p = XMinTri * w1 + SMinTri * w2;
                double v = SVar * Math.Pow(w1, 2) + XVar * Math.Pow(w2, 2);

                //if (pool[k].ProbLowBound != -1 && 0 == k)
                //{
                //    if (pool[k].ProbLowBound != p || pool[k].Variance != v / maxVar)
                //    {
                //        Console.ReadKey();
                //        var a =testBestSolution;
                //    }
                //    for (int i = 0; i < 1024; i++)
                //    {
                //        if (testBestSolution.binsSetup[i].SetIndexPlus1 !=
                //            pool[k].binsSetup[i].SetIndexPlus1)
                //        {
                //            Console.Write("sSS");
                //        }
                //    }
                //}

                pool[k].ProbLowBound = p;
                pool[k].Variance = v/ maxVar;
                if (colIndexOfMin == colIndexOfMaxSize)
                {
                    //for (int i = 0; i < Amatrix.ColumnCount; i++)
                    //{
                    //    Console.WriteLine(pool[k].Item2.Where(x => x.Item1 == i + 1).Count());
                    //}
                    w2 = w1;
                }
                coverPointSetProb[k] = new double[Amatrix.ColumnCount];
                coverPointSetProb[k][colIndexOfMin] = w1;
                coverPointSetProb[k][colIndexOfMaxSize] = w2;
            }
            token[0] = 1;
            //Console.WriteLine("Fitness Evaluation Done");
        }
        private void MOGA_NormalizeFitness()
        {
            int rank = 1;
            int[] dominatedList = new int[pool.Count()];
            int[] rankingList = new int[pool.Count()];

            while (!dominatedList.All(x => x == -1))
            {
                for (int i = 0; i < dominatedList.Length; i++)
                {
                    if (dominatedList[i] == -1)
                    {
                        continue;
                    }
                    for (int j = 0; j < dominatedList.Length; j++)
                    {
                        if (dominatedList[j] == -1 ||
                            pool.ElementAt(i).ProbLowBound == pool.ElementAt(j).ProbLowBound
                            && pool.ElementAt(i).Variance==
                            pool.ElementAt(j).Variance)
                        {
                            continue;
                        }
                        if (pool.ElementAt(i).ProbLowBound <= pool.ElementAt(j).ProbLowBound
                            && pool.ElementAt(i).Variance <=
                            pool.ElementAt(j).Variance)
                        {
                            dominatedList[i] += 1;
                        }
                    }
                }

                for (int i = 0; i < dominatedList.Length; i++)
                {
                    if (dominatedList[i] == 0)
                    {
                        rankingList[i] = rank;
                        dominatedList[i] = -1;
                    }
                    else if (dominatedList[i] != -1)
                    {
                        dominatedList[i] = 0;
                    }
                }
                rank += 1;
            }
            rank -= 1;
            // Linear Ranking with rank reversal
            for (int i = 0; i < pool.Count(); i++)
            {
                pool.ElementAt(i).Rank = 2 * (rank - rankingList[i] + 1) * 1.0 / (pool.Count() * (pool.Count() - 1));
            }
        }

        Pair<int,int,double[]>[] TwoPointCrossOver(Pair<int, int, double[]>[] s1, Pair<int, int, double[]>[] s2)
        {
           var a1 = s1.Select(x => x.SetIndexPlus1).ToList();
           var a2 = s2.Select(x => x.SetIndexPlus1).ToList();
           int po1 = GlobalVar.rnd.Next(0 , a1.Count - 1);
           int po2 = GlobalVar.rnd.Next(po1 + 1, a1.Count);
           var part1 = a1.Select((value, i) =>{ return i < po1 ? value : -1;
            }).Where(x=>x != -1);
           var part2 = a2.Select((value, i) => { return i >= po1 && i <= po2 ? value : -1;
            }).Where(x => x != -1);
           var part3 = a1.Select((value, i) => { return i > po2 ? value : -1;
            }).Where(x => x != -1);
            var newLabelSettings = part1.Concat(part2).Concat(part3).ToArray();
            var newBins = Copy.DeepCopy(sut.bins);
            for (int i = 0; i < newBins.Length; i++)
            {
                newBins[i].SetIndexPlus1 = newLabelSettings[i];
            }
            return newBins;
        }
        void Mutation(Pair<int, int, double[]>[] s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (GlobalVar.rnd.NextDouble() <= mtPropotion)
                {
                    int newLabel = GlobalVar.rnd.Next(1, numOfLabels + 1);
                    s[i].SetIndexPlus1 = newLabel;
                }
            }
        }
        //int testBestSolutionIndex;
        Pair<int, double, Pair<int, int, double[]>[], Matrix<double>, double, double> testBestSolution = null;
         void Reproduction()
        {
            //For MOGA- NO ELITISM applied
            Pair<int, double, Pair<int, int, double[]>[],Matrix<double>,double,double>[] tempPool = 
                new Pair<int, double, Pair<int, int, double[]>[],Matrix<double>,double,double>[populationSize];

            int startIndex = 0;
#if MOGA == true
            var rankOneSolutions = pool.Where(X => X.Rank == pool.Max(y=>y.Rank)).ToArray()[0];
            tempPool[0] = Copy.DeepCopy(rankOneSolutions);
            startIndex = 1;
#else
            var rankOneSolutions = pool.Where(X => X.Rank == pool.Max(y => y.Rank)).ToArray();
            double avgProbLowbound = rankOneSolutions.Select(x => x.ProbLowBound).Sum() / rankOneSolutions.Count();
            double avgVariance = rankOneSolutions.Select(x => x.Variance).Sum() / rankOneSolutions.Count();
            var arrayOfValue = rankOneSolutions.Select(x => Math.Pow(x.Variance - avgVariance, 2) + Math.Pow(x.ProbLowBound - avgProbLowbound, 2)).ToArray();
            int bestSolutionIndex = Array.IndexOf(arrayOfValue, arrayOfValue.Min());
            var bestSolution = rankOneSolutions.ElementAt(bestSolutionIndex);
            testBestSolution = Copy.DeepCopy(bestSolution);
            tempPool[0] = Copy.DeepCopy(bestSolution);
            tempPool[0].Index = 0;
            startIndex = 1;
#endif
            //testBestSolutionIndex = bestSolution.Index;
           // Console.WriteLine("QQ: {0}, lowBound: {1}, Var: {2}", bestSolutionIndex, bestSolution.ProbLowBound, bestSolution.Variance);
            //int rank = bestSolution.AMatrix.Rank();
            for (int i = startIndex; i < populationSize; i++)    // MOGA i= 1 ---> i = 0
            {
                int[] selectedList = FitnessPropotionateSampling();
                var child = Copy.DeepCopy(pool[selectedList[0]]);
                if (GlobalVar.rnd.NextDouble() <= crRate)
                {
                    child.binsSetup = TwoPointCrossOver(pool[selectedList[0]].binsSetup, pool[selectedList[1]].binsSetup);
                }
                if (GlobalVar.rnd.NextDouble() <= mtRate)
                {
                    Mutation(child.binsSetup);
                }
                child.Index = i;
                tempPool[i] = child;
            }
            Array.Clear(pool,0, pool.Length);
            pool = tempPool;
        }
        public void ExpectedTriggeringProbSetUp()
        {
            expTriProb = new double[numOfLabels];
            for (int i = 0; i < numOfLabels; i++)
            {
                expTriProb[i] = 1.0 / numOfLabels;
            }
        }
        private int[] FitnessPropotionateSampling()
        {
            int size = 2;
            int max = populationSize - 1;
            int min = 0;
            int[] result = new int[size];
            double[] fitnessPropotions = new double[populationSize];
            for (int i = 0; i < size; i++)
            {
                result[i] = -1;
            }

            // Normalize Fitness
            int tmpCnt = 0;
            double sum = pool.Sum(x=>x.Rank);
            for (int i = 0; i < max - min + 1; i++)
            {
                fitnessPropotions[i] = pool[i].Rank / sum;
            }
            double[] cumulativeArray = new double[populationSize];
            double cumulation = 0.0;
            for (int i = 0; i < populationSize; i++)
            {
                cumulation = cumulation + fitnessPropotions[i];
                cumulativeArray[i] = cumulation;
            }
            int timerCount = 0;
            while (true)
            {
                if (tmpCnt == size)
                {
                    break;
                }
                double nextRnd = GlobalVar.rnd.NextDouble();
                int selectIndex = 0;
                for (int i = 0; i < cumulativeArray.Length; i++)
                {
                    if (nextRnd < cumulativeArray[i])
                    {
                        selectIndex = i;
                        break;
                    }
                }
                if (result.Where(x => x == selectIndex).Count() == 0)
                {
                    result[tmpCnt] = selectIndex;
                    tmpCnt = tmpCnt + 1;
                }
                timerCount += 1;
                if (timerCount == 100)
                {
                    //Console.WriteLine("Selection Reach 100.");
                    for (int i = 0; i < size; i++)
                    {
                        result[i] = i;
                    }
                    break;
                }
            }
            return result;
        }
    }
}
