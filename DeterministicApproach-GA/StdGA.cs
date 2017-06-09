using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeterministicApproach_GA
{
    using GeneType = GeneD2;
    using Parents = Tuple<Genotype<GeneD2>, Genotype<GeneD2>>;
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
            FitnessEvaVect();
            PopulationGen();
            double bestFitness = -1;
            int generation = 1;
            while (generation <= 999)
            {
                
                Reproduction();
                FitnessEvaVect();
                PopulationGen();
                bestFitness = enVar.population.OrderByDescending(x => x.Value).First().Value;

                enVar.genFitRecord[0] = generation;
                enVar.genFitRecord[1] = bestFitness;
                generation++;

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
                int rndLoc1 = enVar.rnd.Next(0,enVar.pmGenotypeLen-1);
                //Console.WriteLine("Crossover Loc {0}", rndLoc1);
                for (int i = rndLoc1; i < enVar.pmGenotypeLen; i++)
                {
                    genotype.ModifyGenValue(i,parents.Item2.GetGenotypeAtIndex(i));
                }

                if (enVar.rnd.NextDouble() <= enVar.pmMutationRate)
                {
                    int rndLoc2 = enVar.rnd.Next(0, enVar.pmGenotypeLen);
                    int numOfDimension = (int)enVar.pmProblem["Dimension"];
                    int[] mutatedNum = new int[numOfDimension];
                    for (int i = 0; i < numOfDimension; i++)
                    {
                        mutatedNum[i] = enVar.rnd.Next(
                            ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[i].Item1,
                            ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[i].Item2
                        );
                    }
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

        public void FitnessEvaVect()
        {
            for (int k = 0; k < enVar.pmPopSize; k++)
            {
                var genotype = enVar.tempPopulation.ElementAt(k).Key.GetGenotype();
                DenseMatrix aSolFit = new DenseMatrix((int)enVar.pmProblem["NumOfCE"], 1);
                for (int i = 0; i < enVar.pmGenotypeLen; i++)
                {
                    int[] genValues = null;

                    genotype[i].GetGenValue(ref genValues);

                    if (i == 0)
                    {
                        DenseMatrix m = new DenseMatrix(
                            (int)enVar.pmProblem["NumOfCE"], 1, RunSUT(genValues));
                        aSolFit = (DenseMatrix)aSolFit.Add(m);
                    }
                    else
                    {
                        var m = new DenseMatrix(
                            (int)enVar.pmProblem["NumOfCE"], 1, RunSUT(genValues));
                        aSolFit = (DenseMatrix)aSolFit.Append(m);
                    }
                }
                Vector<double> sumVect = aSolFit.RowSums();
                double avgFitness = sumVect.Sum() / (int)enVar.pmProblem["NumOfCE"];
                sumVect = sumVect.Subtract(avgFitness);
                var variance = sumVect.ToRowMatrix().Multiply(sumVect).Storage[0];
                var stdiv = Math.Pow((variance / ((int)enVar.pmProblem["NumOfCE"] - 1)), 0.5);
                enVar.tempPopulation[enVar.tempPopulation.ElementAt(k).Key] = avgFitness - enVar.pmK * stdiv + 5;
            }
        }

        public void FitnessEva()       //None_Vector_Form
        {
            List<int[]> report = new List<int[]>();
            for(int k = 0; k < enVar.tempPopulation.Count;  k++)
            {
                //Console.WriteLine(k);
                var genotype = enVar.tempPopulation.ElementAt(k).Key.GetGenotype();
                int[] oneReport = new int[(int)enVar.pmProblem["NumOfCE"]];
                oneReport.Initialize();
                foreach (var gene in genotype)
                {
                    int[] values = null;
                    gene.GetGenValue(ref values);
                    double[] ret = RunSUT(values);

                    for (int i = 0; i < ret.Count(); i++)
                    {
                        if (ret[i] == 1)
                        {
                            oneReport[i] = oneReport[i] + 1;
                        }
                    }
                }
                report.Add(oneReport);
                double fitness1_AvgCov = 0.0;
                double fitness2_stdCov = 0.0;

                for (int i = 0; i < (int)enVar.pmProblem["NumOfCE"]; i++)
                {
                    fitness1_AvgCov = fitness1_AvgCov + oneReport[i] * 1.0 / enVar.pmGenotypeLen;
                }
                fitness1_AvgCov = fitness1_AvgCov / (int)enVar.pmProblem["NumOfCE"];

                for (int i = 0; i < (int)enVar.pmProblem["NumOfCE"]; i++)
                {
                    fitness2_stdCov = fitness2_stdCov + Math.Pow((oneReport[i] * 1.0 / enVar.pmGenotypeLen - fitness1_AvgCov), 2);
                }
                fitness2_stdCov = Math.Pow(fitness2_stdCov / ((int)enVar.pmProblem["NumOfCE"] - 1), 0.5);
                enVar.tempPopulation[enVar.tempPopulation.ElementAt(k).Key] = fitness1_AvgCov - enVar.pmK * fitness2_stdCov + 5;
            };
        }
    }
}
