using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Evlove distributions by sampling with fitness: f = avgFitness - stdiv*weight + 5

namespace StatisticalApproach_GA
{
    using GeneType = GeneD3;
    using Parents = Tuple<Genotype<GeneD3>, Genotype<GeneD3>>;
    using AppFunc = Func<EnvironmentVar, Task>;
    using System.Data;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.LinearAlgebra;
    class StdGA
    {
        public AppFunc _next;
        private EnvironmentVar enVar;

        public StdGA(AppFunc next)
        {
            _next = next;
        }

        public async Task<int> Invoke(EnvironmentVar envar)
        {
            enVar = envar;

            await Task.Run(()=>GA_Start());
            await _next(enVar);
            return 0;
        }

        public void PopulationGen()
        {
            if (enVar.population.OrderByDescending(x => x.Value).First().Value >
                enVar.tempPopulation.OrderByDescending(x => x.Value).First().Value)
            {
                KeyValuePair<Genotype<GeneType>, double> temp;
                temp = Copy.DeepCopy(enVar.population.OrderByDescending(x => x.Value).First());
                enVar.population.Clear();
                enVar.population.Add(temp.Key, temp.Value);
                for (int i = 0; i < enVar.pmPopSize; i++)
                {
                    if (enVar.tempPopulation.OrderBy(X => X.Value).First().Key
                        == enVar.tempPopulation.ElementAt(i).Key)
                    {
                        continue;
                    }
                    enVar.population.Add(
                        Copy.DeepCopy(enVar.tempPopulation.ElementAt(i).Key),
                        enVar.tempPopulation.ElementAt(i).Value
                    );
                }
            }
            else
            {
                enVar.population.Clear();
                for (int i = 0; i < enVar.pmPopSize; i++)
                {
                    enVar.population.Add(
                        Copy.DeepCopy(enVar.tempPopulation.ElementAt(i).Key),
                        enVar.tempPopulation.ElementAt(i).Value
                    );
                }
            }

            enVar.tempPopulation.Clear();
        }
        public void GA_Start()
        {
            string filePath = @"D:\PhDWorkwc\PhDWorks\trunk\trunk\ResearchProgramming\DeterministicApproach-GA\StatisticalApproach-GA-NewFlow\bin\Debug\OUT\STDGA\bestmove";
            string output = null;
            LocalFileAccess lfa = new LocalFileAccess();
            double bestFitness = -1;
            int generation = 0;
            FitnessEvaVect();
            PopulationGen();
            bestFitness = enVar.population.OrderByDescending(x => x.Value).First().Value;
            Console.WriteLine("{0},{1}", generation, bestFitness);
            output = "gen: " + generation + " Fitness: " + bestFitness;
            lfa.StoreListToLinesAppend(filePath, new List<string>() { output });
            while (generation <= 500)
            {
                
                Reproduction();
                FitnessEvaVect();
                PopulationGen();
                bestFitness = enVar.population.OrderByDescending(x => x.Value).First().Value;

                enVar.genFitRecord[0] = generation;
                enVar.genFitRecord[1] = bestFitness;
                generation++;
                Console.WriteLine("{0},{1}", generation, bestFitness);
                output = "gen: " + generation + " Fitness: " + bestFitness;
                lfa.StoreListToLinesAppend(filePath, new List<string>() { output });
                enVar.continueIndicate = false;
                while (enVar.continueIndicate == false);
            }
            enVar.finishIndicate = true;
        }

        public void Reproduction()
        {
            Parents parents;
            Genotype<GeneType> child;
            int i = 0;
            while (enVar.tempPopulation.Count < enVar.pmPopSize)
            {
                //Console.WriteLine(" *****{0}****", i);
                parents = Selection();
                child = Operations(parents);
                if (child != null)
                {
                    enVar.tempPopulation.Add(child, -1);
                    i = i + 1;
                }
            }
        }

        public Parents Selection()
        {
            int[] selectedIndex = new int[2] {-1,-1 };

            double sumFitness = enVar.population.Values.Sum();

            int j = 0;
            while (j < 2)
            {
                double rndNum = enVar.rnd.NextDouble() * sumFitness;
                double lowbound = 0.0;
                for (int i = 0; i < enVar.pmPopSize; i++)
                {
                    if (rndNum > lowbound && rndNum <= lowbound + enVar.population.ElementAt(i).Value)
                    {
                        selectedIndex[j] = i;
                        break;
                    }
                    lowbound = lowbound + enVar.population.ElementAt(i).Value;
                }
                if (selectedIndex[0] == selectedIndex[1])
                {
                    j = 1;
                }
                else
                {
                    j = j + 1;
                }
            }
            //Console.WriteLine("Selection: {0}, {1}", selectedIndex[0], selectedIndex[1]);
            return new Parents(
                enVar.population.ElementAt(selectedIndex[0]).Key, 
                enVar.population.ElementAt(selectedIndex[1]).Key
            );
        }

        public Genotype<GeneType> Operations(Parents parents)
        {
            Genotype<GeneType> genotype;
            genotype = Copy.DeepCopy(parents.Item1);

            if (enVar.rnd.NextDouble() <= enVar.pmCrossOverRate)
            {
                // This is two point crossover
                int rndLoc11 = enVar.rnd.Next(0,enVar.pmGenotypeLen-3);
                int rndLoc12 = enVar.rnd.Next(rndLoc11+2,enVar.pmGenotypeLen - 1);
                //Console.WriteLine("Crossover Loc {0}", rndLoc1);
                for (int i = rndLoc11+1; i < rndLoc12-1; i++)
                {
                    genotype.ModifyGenValue(i,parents.Item2.GetGenotypeAtIndex(i));
                }

                if (enVar.rnd.NextDouble() <= enVar.pmMutationRate)
                {
                    int rndLoc2 = enVar.rnd.Next(0, enVar.pmGenotypeLen);
                    int numOfDimension = (int)enVar.pmProblem["Dimension"];
                    int[] mutatedNum = new int[numOfDimension + 1];   //incorperate with weights
                    int decodeLow = 0;
                    int decodeHigh = 0;

                    decodeLow = (int)Math.Pow((((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[0].Item1), 2)
                                     + (((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[0].Item1);
                    decodeHigh = (int)Math.Pow((((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[1].Item2), 2)
                                     + (((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[1].Item2);

                    int lowBound = enVar.rnd.Next(decodeLow, decodeHigh);
                    int highBound = enVar.rnd.Next(lowBound, decodeHigh);
                    mutatedNum[0] = lowBound;
                    mutatedNum[1] = highBound;
                    mutatedNum[2] = enVar.rnd.Next(1, (int)enVar.pmProblem["Weight"]);
                    genotype.ModifyGenValue(rndLoc2, mutatedNum);
                    //Console.WriteLine("Mutation Loc {0}, mutatedNum {1} {2}", rndLoc2, mutatedNum[0], mutatedNum[1]);
                }
                return genotype;
            }
            return null;
        }

        public double[] RunSUT(params int[] num)
        {
            double[] report = new double[(int)enVar.pmProblem["NumOfCE"]];
            string input = null;
            foreach (var n in num)
            {
                input = input + " " + n.ToString();
            }
            input = input.Remove(0, 1);
            report = ((Dictionary<string, double[]>)enVar.pmProblem["Map"])[input];
            return report;
        }
        public void FitnessEvaVectIn(int k)
        {
            Console.WriteLine(k);
            int[] testcase = null;
            testcase = new int[(int)enVar.pmProblem["Dimension"]];
            DenseMatrix aSolFit = new DenseMatrix((int)enVar.pmProblem["NumOfCE"], 1);
            for (int i = 0; i < enVar.testSetSize; i++)
            {
                testSetGeneration(enVar.tempPopulation.ElementAt(k).Key, ref testcase);
                if (i == 0)
                {
                    DenseMatrix m = new DenseMatrix(
                        (int)enVar.pmProblem["NumOfCE"], 1, RunSUT(testcase));
                    aSolFit = (DenseMatrix)aSolFit.Add(m);
                }
                else
                {
                    var m = new DenseMatrix(
                        (int)enVar.pmProblem["NumOfCE"], 1, RunSUT(testcase));
                    aSolFit = (DenseMatrix)aSolFit.Append(m);
                }
            }
            Vector<double> sumVect = aSolFit.RowSums();
            double avgFitness = sumVect.Sum() / (int)enVar.pmProblem["NumOfCE"];
            avgFitness = avgFitness / enVar.testSetSize;
            //sumVect = sumVect.Subtract(avgFitness);
            //var variance = (sumVect.ToRowMatrix().Multiply(sumVect).Storage[0])*1.0/enVar.testSetSize;
            //var stdiv = Math.Pow((variance / ((int)enVar.pmProblem["NumOfCE"] - 1)), 0.5);
            enVar.tempPopulation[enVar.tempPopulation.ElementAt(k).Key] = avgFitness /*- enVar.pmK * stdiv + 5*/;
        }
        public async void  FitnessEvaVectPARALLEL(int[] token)
        {
            List<Task> lTasks = new List<Task>();
            int remainTasks = enVar.pmPopSize;

            for (int k = 0; k < enVar.pmPopSize; k++)
            {
                lTasks.Add(Task.Run(() =>
                {
                    FitnessEvaVectIn(k);
                }));
                if (lTasks.Count == 6 || (enVar.pmPopSize - k - 1 < 6))
                {
                    for (int i = 0; i < lTasks.Count; i++)
                    {
                        await lTasks[i];
                    }
                    lTasks.Clear();
                }
            }
            token[0] = 1;
        }

        public void FitnessEvaVect()
        {

            for (int k = 0; k < enVar.pmPopSize; k++)
            {
                Console.WriteLine(k);
                int[] testcase = null;
                testcase = new int[(int)enVar.pmProblem["Dimension"]];
                DenseMatrix aSolFit = new DenseMatrix((int)enVar.pmProblem["NumOfCE"], 1);
                for (int i = 0; i < enVar.testSetSize; i++)
                {
                    testSetGeneration(enVar.tempPopulation.ElementAt(k).Key, ref testcase);
                    if (i == 0)
                    {
                        DenseMatrix m = new DenseMatrix(
                            (int)enVar.pmProblem["NumOfCE"], 1, RunSUT(testcase));
                        aSolFit = (DenseMatrix)aSolFit.Add(m);
                    }
                    else
                    {
                        var m = new DenseMatrix(
                            (int)enVar.pmProblem["NumOfCE"], 1, RunSUT(testcase));
                        aSolFit = (DenseMatrix)aSolFit.Append(m);
                    }
                }
                Vector<double> sumVect = aSolFit.RowSums();
                double avgFitness = sumVect.Sum() / (int)enVar.pmProblem["NumOfCE"];
                avgFitness = avgFitness / enVar.testSetSize;
                //sumVect = sumVect.Subtract(avgFitness);
                //var variance = (sumVect.ToRowMatrix().Multiply(sumVect).Storage[0])*1.0/enVar.testSetSize;
                //var stdiv = Math.Pow((variance / ((int)enVar.pmProblem["NumOfCE"] - 1)), 0.5);
                enVar.tempPopulation[enVar.tempPopulation.ElementAt(k).Key] = avgFitness /*- enVar.pmK * stdiv + 5*/;
            }
        }
        public void testSetGeneration(Genotype<GeneType> genotype, ref int[] testcase)
        {
            int sumOfWeight = genotype.GetGenotype().Sum(x => x.GetGenValueByIndex(2));

            double rndNum = enVar.rnd.NextDouble() * sumOfWeight;
            double low = 0.0;
            int selectedIndex = 0;
            for (int i = 0; i < genotype.GetGenotype().Length; i++)
            {
                if (rndNum > low && rndNum <= low + genotype
                    .GetGenotype()[i].GetGenValueByIndex(2))
                {
                    selectedIndex = i;
                    break;
                }
                low = low + genotype.GetGenotype()[i].GetGenValueByIndex(2);
            }

            int lowBound = genotype.GetGenotype()[selectedIndex].GetGenValueByIndex(0);
            int highBound = genotype.GetGenotype()[selectedIndex].GetGenValueByIndex(1);

            int rawData = enVar.rnd.Next(lowBound, highBound);
            testcase[0] = rawData / (((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[0].Item2);
            testcase[1] = rawData % (((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[0].Item2);
        }
    }
}
