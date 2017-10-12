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
    public class Pair<T0,T1,T2>
    {
        public T0 Item0 { set; get; }
        public T1 Item1 { set; get; }
        public T2 Item2 { set; get; }

        //public T2 Item2
        //{
        //    set
        //    {
        //        if (value.GetType().Name != typeof(double[]).Name)
        //        {
        //            Item2 = value;
        //        }
        //        else
        //        {
        //            var temp = (double[])(object)value;
        //            if (temp.Sum() == 0)
        //            {
        //                Item2 = value;
        //            }
        //            if (temp.Sum() < 0.9998 || temp.Sum() > 1.0002)
        //            {
        //                string message = "Sum Of Probabilities for " + Item0 +
        //                    " is not equal to one， " + temp.Sum();
        //                throw new Exception(message);
        //            }
        //            Item2 = value;
        //        }
        //    }

        //    get
        //    {
        //        return Item2;
        //    }
        //}
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
        Pair<int, double, Pair<int, int, double[]>[]>[] pool; //index, fitness, bins
        double[][] coverPointSetProb;

        //SUT Setup
        SUT sut = null;
        //Linear Programming storage
        double[] coesArray;

        public GALS(int numOfMaxGen, int numLabels, SUT SUT)
        {
            maxGen = numOfMaxGen;
            numOfLabels = numLabels;
            sut = SUT;
        }

        public class record
        {
            public double[] fitnessGen;
            public double[] goodnessOfFitGen;
            public solution bestSolution;
        }
        public class solution
        {
            //public int[][] inputsinSets; // [1][2] : the second inputs in set 1, Obsolete variable
            public int[] binSetup;
            public double[] setProbabilities;
            public long totalRunTime;
        }
        public void AlgorithmStart(record record)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int[] token = new int[1];
            FitnessEvaluationNoParll(token);
            while (token[0] == 0) ;
            token[0] = 0;

            Queue<double> last20fitness = new Queue<double>();
            while (generation < maxGen)
            {
                Reproduction();
                FitnessEvaluationNoParll(token);
                while (token[0] == 0) ;
                token[0] = 0;

                // Start Recording
                watch.Stop();
                var orderedSolutions = pool.OrderByDescending(x => x.Item1);

                record.fitnessGen[generation] = orderedSolutions.First().Item1;
                record.goodnessOfFitGen[generation] = orderedSolutions.First().Item1;

                Console.WriteLine("Gen #{0} Fitness: {1}",generation, record.goodnessOfFitGen[generation]);
                watch.Start();

                if (last20fitness.Count == 100)
                {
                    last20fitness.Dequeue();
                    last20fitness.Enqueue(1.0 / orderedSolutions.First().Item1);
                    if (last20fitness.All(x => x == last20fitness.First()))
                    {
                        break;
                    }
                }
                else
                {
                    last20fitness.Enqueue(1.0 / orderedSolutions.First().Item1);
                }

                generation += 1;
            }

            watch.Stop();

            var rankedOneSolution = pool.OrderByDescending(x => x.Item1).First();
            record.bestSolution.binSetup = Copy.DeepCopy(rankedOneSolution.Item2.Select(x => x.Item1).ToArray());
            var totalRuningTime = watch.ElapsedMilliseconds;
            record.bestSolution.totalRunTime = totalRuningTime;
            record.bestSolution.setProbabilities = Copy.DeepCopy(
                coverPointSetProb[rankedOneSolution.Item0]);
        }

        public void GAInitialization()
        {
            pool = new Pair<int, double, Pair<int, int, double[]>[]>[populationSize];
            coverPointSetProb = new double[populationSize][];

            for (int i = 0; i < populationSize; i++)
            {
                pool[i] = new Pair<int, double, Pair<int, int, double[]>[]>();
                pool[i].Item0 = i;
                pool[i].Item1 = -1;
                pool[i].Item2 = Copy.DeepCopy(sut.bins);
                for (int j = 0; j < pool[i].Item2.Length; j++)
                {
                    pool[i].Item2[j].Item1 = GlobalVar.rnd.Next(1,numOfLabels+1);
                }
                SolutionRepair(pool[i].Item2);
            }

        }
        public double SelfAdaptivePenaltyFunction(double[] probabilities,double cost)
        {
            int beta = 1;
            int searchBoundary = 200;
            double[] tempProbs = Copy.DeepCopy(probabilities);
            tempProbs = tempProbs.Select(x =>
            {
                double y = x * -1 - 0;
                return Math.Pow((Math.Max(0, y)),beta);
            }).ToArray();
            double totalInfeasiability = tempProbs.Sum() > searchBoundary * numOfLabels ? 100 : tempProbs.Sum() / searchBoundary * numOfLabels;
            double normalizedCost = cost / numOfLabels;
            bool infeasiablePool = coverPointSetProb.All(x => x.All(y => y < 0) == true) == true ? true : false;
            double distance = infeasiablePool == true ? totalInfeasiability
                : Math.Sqrt(Math.Pow(normalizedCost, 2) + Math.Pow(totalInfeasiability, 2));
            double X = infeasiablePool == true ? 0 : totalInfeasiability;
            double Y = probabilities.All(x => x > 0) ? 0 : normalizedCost;
            double proportionOfFeaisableSol = coverPointSetProb
                .Count(x => x.All(y => y > 0))*1.0/populationSize;
            double adjustedFitness = distance + (1- proportionOfFeaisableSol) *X 
                + proportionOfFeaisableSol * Y;

            return adjustedFitness;
        }

        public Matrix<double> AmatrixCalculation(int id)
        {

            Matrix<double> Amatrix = Matrix<double>.Build.Dense(numOfLabels, numOfLabels);

            for (int i = 0; i < numOfLabels; i++)
            {
                for (int j = 0; j < numOfLabels; j++)
                {
                    Amatrix[i, j] = pool[id].Item2.Where(x => x.Item1 == j + 1).Sum(x => x.Item2[i]);
                    Amatrix[i, j] /= pool[id].Item2.Where(x => x.Item1 == j + 1).Count();
                }
            }
            return Amatrix;
        }
        public void FitnessEvaluationNoParll(int[] token)
        {
            for (int k = 0; k < populationSize; k++)
            {
                //Console.Write("task #{0}", id);
                Matrix<double> Amatrix = Matrix<double>.Build.Dense(numOfLabels, numOfLabels - 1);
                double[] lastCoverElementSet = new double[numOfLabels];
                Vector<double> bVector = Vector<double>.Build.Dense(expTriProb);

                for (int i = 0; i < numOfLabels; i++)
                {
                    if (pool[k].Item2.Where(x => x.Item1 == numOfLabels).Count() == 0)
                    {
                        Console.WriteLine();
                    }
                    lastCoverElementSet[i] = pool[k].Item2.Where(x => x.Item1 == numOfLabels).Sum(x => x.Item2[i])
                        / pool[k].Item2.Where(x => x.Item1 == numOfLabels).Count();
                }
                bVector = bVector.Subtract(Vector<double>.Build.Dense(lastCoverElementSet));

                for (int i = 0; i < numOfLabels; i++)
                {

                    for (int j = 0; j < numOfLabels - 1; j++)
                    {
                        if (pool[k].Item2.Where(x => x.Item1 == j + 1).Count() == 0)
                        {
                            Console.WriteLine();
                        }
                        Amatrix[i, j] = pool[k].Item2.Where(x => x.Item1 == j + 1).Sum(x => x.Item2[i]);
                        Amatrix[i, j] /= pool[k].Item2.Where(x => x.Item1 == j + 1).Count();
                        Amatrix[i, j] -= lastCoverElementSet[i];
                    }
                }

                double[] w = new double[numOfLabels - 1];
                double[,] c = new double[numOfLabels, numOfLabels];
                int[] ct = new int[numOfLabels]; 

                for (int i = 0; i < numOfLabels - 1; i++)
                {
                    w[i] = 1.0 / w.Length;
                }

                for (int i = 0; i < c.Length / numOfLabels; i++)
                {
                    if (i == 0)
                    {
                        for (int j = 0; j < numOfLabels; j++)
                        {
                            c[i, j] = -1;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < numOfLabels; j++)
                        {
                            c[i, j] = (i - 1) == j ? 1 : 0;
                        }
                    }
                }
                for (int i = 0; i < ct.Length; i++)
                {
                    ct[i] = 1;
                }
                alglib.minbleicstate state;
                alglib.minbleicreport rep;
                coesArray = Amatrix.ColumnSums().ToArray();
                double epsg = 0.000001;
                double epsf = 0;
                double epsx = 0;
                int maxits = 0;

                alglib.minbleiccreate(w, out state);
                alglib.minbleicsetlc(state, c, ct);
                alglib.minbleicsetcond(state, epsg, epsf, epsx, maxits);
                alglib.minbleicoptimize(state, linearFunction_grad, null, null);
                alglib.minbleicresults(state, out w, out rep);

                double[] probForCESets = new double[numOfLabels];
                for (int i = 0; i < numOfLabels - 1; i++)
                {
                    probForCESets[i] = w[i];
                }
                probForCESets[numOfLabels - 1] = 1 - probForCESets.Sum();
                coverPointSetProb[k] = Copy.DeepCopy(probForCESets);

                double fitness = 0;
                for (int i = 0; i < w.Length; i++)
                {
                    fitness += w[i] * coesArray[i] - bVector[i];
                }
                pool[k].Item1 = fitness;
            }
            token[0] = 1;
            var a = pool[0].Item2.Where(x => x.Item1 == 12).ToList();
            //Console.WriteLine("Fitness Evaluation Done");
        }
        public void linearFunction_grad(double[] x, ref double func, double[] grad, object obj)
        {
            // this callback calculates f(x0,x1) = 100*(x0+3)^4 + (x1-3)^4
            // and its derivatives df/d0 and df/dx1
            //func = 100 * System.Math.Pow(x[0] + 3, 4) + System.Math.Pow(x[1] - 3, 4);
            func = 0;
            for (int i = 0; i < x.Length; i++)
            {
                func += -1 * x[i] * coesArray[i];
                grad[i] = -1 * coesArray[i];
            }
        }

        Pair<int,int,double[]>[] TwoPointCrossOver(Pair<int, int, double[]>[] s1, Pair<int, int, double[]>[] s2)
        {
           var a1 = s1.Select(x => x.Item1).ToList();
           var a2 = s2.Select(x => x.Item1).ToList();
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
                newBins[i].Item1 = newLabelSettings[i];
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
                    s[i].Item1 = newLabel;
                }
            }
        }
         void Reproduction()
        {
            Pair<int, double, Pair<int, int, double[]>[]>[] tempPool = new Pair<int, double, Pair<int, int, double[]>[]>[populationSize];
            var bestSolution = pool.Where(x => x.Item1 == pool.Max(y=>y.Item1)).ToList().First();
            bestSolution.Item0 = 0;
            tempPool[0] = Copy.DeepCopy(bestSolution);

            for (int i = 1; i < populationSize; i++)
            {
                int[] selectedList = FitnessPropotionateSampling();
                var child = Copy.DeepCopy(pool[selectedList[0]]);
                if (GlobalVar.rnd.NextDouble() <= crRate)
                {
                    child.Item2 = TwoPointCrossOver(pool[selectedList[0]].Item2, pool[selectedList[1]].Item2);
                }
                if (GlobalVar.rnd.NextDouble() <= mtRate)
                {
                    Mutation(child.Item2);
                }
                SolutionRepair(child.Item2);
                child.Item0 = i;
                tempPool[i] = child;
            }
            Array.Clear(pool,0, pool.Length);
            pool = tempPool;
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
                    if (bins.Where(x => x.Item1 == i + 1).Count() == 0)
                    {
                        int binIndex = bins.Select(y =>
                        {
                            double MaxValidTriProb = bins.Max(x =>
                            {
                                return !InValidBinIndex.Contains(x.Item0) ? x.Item2[i] : -10;
                            });
                            if (y.Item2[i] == MaxValidTriProb
                                && !InValidBinIndex.Contains(y.Item0))
                            {
                                return y.Item0;
                            }
                            return -1;
                        }).Where(x => x != -1).First();

                        if (bins.Where(x => x.Item1 == bins[binIndex].Item1).Count() > 1)
                        {
                            bins[binIndex].Item1 = i + 1;
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
                if (bins.Where(x => x.Item1 == i + 1).Count() == 0)
                {
                    Console.WriteLine("Invalid label {0}",i);
                }
            }
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
            double sum = pool.Sum(x=>x.Item1);
            for (int i = 0; i < max - min + 1; i++)
            {
                fitnessPropotions[i] = pool[i].Item1 / sum;
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
