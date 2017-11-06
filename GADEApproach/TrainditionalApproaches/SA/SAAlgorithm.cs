using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using ReadSUTBranchCEData;
using MathNet.Numerics.Statistics;

namespace GADEApproach.TrainditionalApproaches.SA
{
    public class SAAlgorithm
    {
        int cheatCEIndex = -1;
        int numOfSamples = 8000; //CORRECT
        int numOfSampeSet = 40;
        int maxGen = -1;
        int generation = 0;
        double T = 100;
        double beta = 0.001;
        double acceptanceRatio = 1;
        List<int[]> sampledInputs;
        int numOfLabels = -1;
        int numOfbins;
        //index, fitness, fitness2, weights_Array
        Pair<int, double, double, Matrix<double>, double> solution;
        //SUT Setup
        SUT sut = null;

        public SAAlgorithm(int numOfMaxGen, int numLabels, SUT SUT, int CheatCEIndex)
        {
            maxGen = numOfMaxGen;
            numOfLabels = numLabels;
            sut = SUT;
            numOfbins = SUT.numOfMinIntervalInAllDim;
            cheatCEIndex = CheatCEIndex;
        }

        double coolingFunction(double currentT)
        {
            return currentT / (1 + beta * currentT);
        }

        Pair<int, double, double, Matrix<double>, double> Reproduction(Pair<int, double, double, Matrix<double>, double> solution)
        {
            var newSolution = Copy.DeepCopy(solution);
            newSolution.fitness1 = -1;
            for (int r = 0; r < newSolution.weightsMatrix.RowCount; r++)
            {
                for (int c = 0; c < newSolution.weightsMatrix.ColumnCount; c++)
                {
                    double rnd =  Normal.Sample(newSolution.weightsMatrix[r, c], 50);
                    //rnd = rnd > 800 ? 800 : rnd;
                    rnd = rnd < 0 ? 0 : rnd;
                    newSolution.weightsMatrix[r, c] = rnd;
                }
            }
            return newSolution;
        }

        public int FitnessEvaluation(
                Pair<int, double, double, Matrix<double>, double> solution,
                Pair<int, double, double, Matrix<double>, double> newSolution
            )
        {
            readBranch rbce = new readBranch();
            double[] ceCountGroupsNewSolution = new double[numOfSampeSet];
            for (int g = 0; g < numOfSampeSet; g++)
            {
                int ceCount = 0;
                for (int s = 0; s < numOfSamples / numOfSampeSet; s++)
                {
                    int[] output = null;
                    int[] inputs;
                    inputs = TestDataGeneration2.GenerateInput(newSolution.weightsMatrix, sut);
                    rbce.ReadBranchCLIFunc(inputs.ToArray(), ref output, sut.sutIndex);
                    if (output[cheatCEIndex] == 1)
                    {
                        ceCount += 1;
                    }
                }
                ceCountGroupsNewSolution[g] = ceCount * 1.0 / (numOfSamples / numOfSampeSet);
            }
            newSolution.fitness1 = ceCountGroupsNewSolution.Sum() / ceCountGroupsNewSolution.Length;

            double[] ceCountGroupsSolution = new double[numOfSampeSet];
            for (int g = 0; g < numOfSampeSet; g++)
            {
                int ceCount = 0;
                for (int s = 0; s < numOfSamples / numOfSampeSet; s++)
                {
                    int[] output = null;
                    int[] inputs;
                    inputs = TestDataGeneration2.GenerateInput(solution.weightsMatrix, sut);
                    rbce.ReadBranchCLIFunc(inputs.ToArray(), ref output, sut.sutIndex);
                    if (output[cheatCEIndex] == 1)
                    {
                        if (sampledInputs.Count < 100
                                && sampledInputs.Where(x => x.SequenceEqual(inputs) == true).Count() == 0
                                )
                        {
                            sampledInputs.Add(inputs);
                        }
                        ceCount += 1;
                    }
                }
                ceCountGroupsSolution[g] = ceCount * 1.0 / (numOfSamples / numOfSampeSet);
            }

            solution.fitness1 = ceCountGroupsSolution.Sum() / (ceCountGroupsSolution.Length);

            if (newSolution.fitness1 < 0.05)
            {
                double probSolution = 0;
                double probNewSolution = 0;

                // Testing Code: Uniqueness of sampled inputs
                //for (int i = 0; i < sampledInputs.Count; i++)
                //{
                //    for (int j = 0; j < sampledInputs.Count; j++)
                //        if (sampledInputs[i].SequenceEqual(sampledInputs[j]) && j != i)
                //        {
                //            int a = 0;
                //        }
                //}

                for (int j = 0; j < sampledInputs.Count; j++)
                {
                    probSolution += TestDataGeneration2.InputProbability(sampledInputs[j], solution.weightsMatrix, sut);
                    probNewSolution += TestDataGeneration2.InputProbability(sampledInputs[j], newSolution.weightsMatrix, sut);
                }

                // Testing Code: Probability Shouldn't > 1
                //if (probSolution > 1 || probNewSolution > 1)
                //{
                //    int a = 0;
                //}

                solution.fitness1 = probSolution;
                newSolution.fitness1 = probNewSolution;

                if (probNewSolution > probSolution)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            int ret = HypothesisTesting.RankSumTest(ceCountGroupsSolution, ceCountGroupsNewSolution);
            return ret;
        }

        public void SA_Start(record record)
        {
            sampledInputs = new List<int[]>();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            solution = new Pair<int, double, double, Matrix<double>, double>();
            solution.index = 0;
            solution.fitness1 = -1;
            solution.weightsMatrix = Matrix<double>.Build.Dense(numOfbins, sut.lowbounds.Length);
            Queue<double> lastNumOffitness = new Queue<double>();

            for (int r = 0; r < solution.weightsMatrix.RowCount; r++)
            {
                for (int c = 0; c < solution.weightsMatrix.ColumnCount; c++)
                {
                    solution.weightsMatrix[r, c] = 100;
                }
            }

            while (generation < maxGen)
            {
                Pair<int, double, double, Matrix<double>, double> newSolution = Reproduction(solution);
                int ret = FitnessEvaluation(solution, newSolution);
                if (ret == -1)
                {
                    solution = newSolution;
                    Console.WriteLine("New Solution");
                }
                else if (ret == 1)
                {
                    for (int r = 0; r < solution.weightsMatrix.RowCount; r++)
                    {
                        for (int c = 0; c < solution.weightsMatrix.ColumnCount; c++)
                        {
                            double rnd = GlobalVar.rnd.NextDouble();
                            if (rnd < acceptanceRatio / T)
                            {
                                solution.weightsMatrix[r, c] = newSolution.weightsMatrix[r, c];
                            }
                        }
                    }
                }
                T = coolingFunction(T);
                Console.WriteLine("T:{0}",T);
                var statistics = new DescriptiveStatistics(lastNumOffitness.ToArray());
                if (Math.Round(statistics.Maximum, 2) == 1)
                {
                    break;
                }
                if (lastNumOffitness.Count == 100)
                {
                    lastNumOffitness.Dequeue();
                    lastNumOffitness.Enqueue(solution.fitness1);
                    double stdiv = statistics.StandardDeviation;
                    if (statistics.Mean > 0.5 && stdiv < 0.05)
                    {
                        break;
                    }
                }
                else
                {
                    lastNumOffitness.Enqueue(solution.fitness1);
                }
                watch.Stop();
                Console.WriteLine("CE: {0}, Run: {1}, Fitness: {2}", cheatCEIndex, generation, solution.fitness1);
                record.fitnessGen[generation] = solution.fitness1;
                watch.Start();
                generation += 1;
            }

            watch.Stop();
            var totalRuningTime = watch.ElapsedMilliseconds;
            record.bestSolution.totalRunTime = totalRuningTime;
            Vector<double> weightVector = Vector<double>.Build.Dense(numOfbins, 1);
            for (int i = 0; i < solution.weightsMatrix.ColumnCount; i++)
            {
                weightVector = weightVector.PointwiseMultiply(solution.weightsMatrix.Column(i));
            }
            weightVector.Normalize(2.0);
            record.bestSolution.setProbabilities = Copy.DeepCopy(weightVector.ToArray());
            record.bestSolution.WeightMateix = solution.weightsMatrix;
        }
    }
}
