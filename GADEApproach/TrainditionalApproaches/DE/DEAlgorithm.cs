using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;

namespace GADEApproach.TrainditionalApproaches.DE
{

    [Serializable]
    class DE
    {
        private int gen = 0;
        int popSize = 100;
        public int maxGen = 500;
        int numOfLabel = -1;
        Pair<int, double, double[]>[] pool;      // array of label properties
        Pair<int, int, double[]>[] bins;
        double[] expTriProb;
        public DE(Pair<int, int, double[]>[] bs, int nOfLabels, double[] etp)
        {
            bins = bs;
            numOfLabel = nOfLabels;
            expTriProb = etp;
        }

        public void FitnessCal(Pair<int, double, double[]> s)
        {
            if (s.ProbInBins.Where(x => x < 0 == true).Count() != 0)
            {
                s.SetIndexPlus1 = -1 * s.ProbInBins.Where(x => x < 0 == true).Count();
                return;
            }

            Matrix<double> Amatrix = Matrix<double>.Build.Dense(numOfLabel, numOfLabel);
            for (int i = 0; i < numOfLabel; i++)
            {
                for (int j = 0; j < numOfLabel; j++)
                {
                    Amatrix[i, j] = bins.Where(x => x.SetIndexPlus1 == j + 1).Sum(x => x.ProbInBins[i]);
                    Amatrix[i, j] /= bins.Where(x => x.SetIndexPlus1 == j + 1).Count();
                }
            }
            double error = 0;
            for (int row = 0; row < numOfLabel; row++)
            {
                error += Math.Pow((Amatrix.Row(row).ToRowMatrix().Multiply(Vector<double>
                    .Build.Dense(s.ProbInBins))[0] - expTriProb[row]), 2);
            }

            s.SetIndexPlus1 = 1.0 / error;
        }
        public async void DE_FitnessEvaluation(int[] token)
        {
            List<Task> lTasks = new List<Task>();
            Mutex mutex_K = new Mutex(false, "lockFork");
            // Test inputs Generation
            // Fitness Evaluation
            for (int k = 0; k < popSize; k++)
            {
                //FitnessCal(pool[k]);

                lTasks.Add(Task.Run(() =>
                {
                    mutex_K.WaitOne();
                    // Console.WriteLine(k);
                    int id = k;
                    k = k + 1;
                    mutex_K.ReleaseMutex();
                    FitnessCal(pool[id]);
                    //Console.WriteLine("{0} job finish",id);
                }));
                if (lTasks.Count == 20 || (popSize - k - 1 < 20))  //8,8
                {
                    for (int i = 0; i < lTasks.Count; i++)
                    {
                        await lTasks[i];
                    }
                    lTasks.Clear();
                }
            }
            token[0] = 1;
            ////Console.WriteLine("Fitness Evaluation Done");
        }
        public Pair<int, double, double[]> DE_Start()
        {
            DE_Initialization();
            int[] token = new int[1];
            DE_FitnessEvaluation(token);
            while (token[0] == 0) ;
            token[0] = 0;
            double[] historyOfFitness = new double[20];

            while (gen < maxGen)
            {
                gen = gen + 1;
                Reproduction();
                if (gen % 20 != 0 && gen != 0)
                {
                    historyOfFitness[gen % 20] = pool.Max(y => y.SetIndexPlus1);
                }
                else
                {
                    double dist = Vector.Build.Dense(historyOfFitness)
                        .Subtract(historyOfFitness.Sum() / historyOfFitness.Count())
                        .L2Norm();
                    if (dist < 500)
                    {
                        break;
                    }
                }
            }
            return pool.Where(x => x.SetIndexPlus1 == pool.Max(y => y.SetIndexPlus1)).First();
        }

        private void DE_Initialization()
        {
            pool = new Pair<int, double, double[]>[popSize];
            for (int i = 0; i < popSize; i++)
            {
                double[] ceSetProbabilities = new double[numOfLabel];
                int lowEnd = numOfLabel;
                for (int j = 0; j < numOfLabel; j++)
                {
                    var result = GlobalVar.rnd.Next(0, lowEnd + 1);
                    ceSetProbabilities[j] = result * 1.0 / numOfLabel;
                    lowEnd = lowEnd - result;
                }
                pool[i] = new Pair<int, double, double[]>();
                pool[i].BinIndex = i;
                pool[i].SetIndexPlus1 = -1.0;
                pool[i].ProbInBins = ceSetProbabilities;
            }

        }
        Pair<int, double, double[]> DifferentialCrossover(int[] selectedList, int sIndex)
        {
            double F = 1.5; //[0,2]
            var s1 = pool[selectedList[0]].ProbInBins;
            var s2 = pool[selectedList[1]].ProbInBins;
            var s3 = pool[selectedList[2]].ProbInBins;

            var solutionCopy = Copy.DeepCopy(pool[sIndex]);
            int R = GlobalVar.rnd.Next(1, numOfLabel);
            for (int i = 0; i < numOfLabel - 1; i++)
            {
                double r = GlobalVar.rnd.NextDouble();
                if (r <= 0.9 || i == R)
                {
                    solutionCopy.ProbInBins[i] = s1[i] + F * (s2[i] - s3[i]);
                }
            }

            solutionCopy.ProbInBins[numOfLabel - 1] =
                1 - (solutionCopy.ProbInBins.Sum()
                - solutionCopy.ProbInBins[numOfLabel - 1]);

            return solutionCopy;
        }
        public void Reproduction()
        {
            Pair<int, double, double[]>[] tempPool;
            tempPool = new Pair<int, double, double[]>[popSize];

            for (int i = 0; i < popSize; i++)
            {
                int[] selectedList = DifferentialSelection(i);
                var newSolution = DifferentialCrossover(selectedList, i);
                FitnessCal(newSolution);
                tempPool[i] = newSolution.SetIndexPlus1 > pool[i].SetIndexPlus1 ? newSolution : Copy.DeepCopy(pool[i]);
                tempPool[i].BinIndex = i;
            }

            Array.Clear(pool, 0, pool.Length);
            pool = tempPool;
        }

        //Select 3 individuals that is distinct from each other, and from index
        private int[] DifferentialSelection(int index)
        {
            int success = 0;
            int a = -1, b = -1, c = -1;
            while (success == 0)
            {
                a = GlobalVar.rnd.Next(0, popSize - 1);
                b = GlobalVar.rnd.Next(0, popSize - 1);
                c = GlobalVar.rnd.Next(0, popSize - 1);

                if (!(index == a || index == b
                    || index == c || a == b
                    || a == c || b == c))
                {
                    success = 1;
                }
            }
            return new int[] { a, b, c };
        }
    }
}
