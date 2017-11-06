using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using ReadSUTBranchCEData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach.TrainditionalApproaches.HC
{
    class HCAlgorithm
    {
        int cheatCEIndex = -1;
        int numOfSamples = 8000; //CORRECT
        int numOfSampeSet = 40;
        int generation = 0;
        //Multi-Threading Setup
        int numOfLabels = -1;

        //HC Setup
        double pho = 0.9;
        public double minDelta = 200.0 / 2;
        public int maxGen = 2;
        Dictionary<string, int[]> Sampledinputs;
        int numOfbins;
        List<int[]> sampledInputs;

        //index, fitness, fitness2, weights_Array
        Pair<int, double, double, Matrix<double>,double> solution;
        //SUT Setup
        SUT sut = null;

        public HCAlgorithm(int numOfMaxGen, int numLabels, SUT SUT, int CheatCEIndex)
        {
            maxGen = numOfMaxGen;
            numOfLabels = numLabels;
            sut = SUT;
            numOfbins = SUT.numOfMinIntervalInAllDim;
            cheatCEIndex = CheatCEIndex;
        }

        public void HC_Start(record record)
        {
            sampledInputs = new List<int[]>();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Sampledinputs = new Dictionary<string, int[]>();
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

        private Pair<int, double, double, Matrix<double>, double> GroupingOperator(Pair<int, double, double, Matrix<double>, double> solution)
        {
            int totalLength = solution.weightsMatrix.ColumnCount * solution.weightsMatrix.RowCount;
            int maxCount = GlobalVar.rnd.Next(1, (int)(totalLength*0.25));
            var newSolution = Copy.DeepCopy(solution);
            newSolution.fitness1 = 0;
            for (int i = 0; i < maxCount; i++)
            {
                int rndColIndex = GlobalVar.rnd.Next(0, solution.weightsMatrix.ColumnCount);
                int rndRowIndex = GlobalVar.rnd.Next(0, solution.weightsMatrix.RowCount);
                newSolution.weightsMatrix[rndRowIndex, rndColIndex] = 0;
            }
            return newSolution;
        }

        private Pair<int, double, double, Matrix<double>, double> WeightRefineOperator(Pair<int, double, double, Matrix<double>, double> solution)
        {
            int totalLength = solution.weightsMatrix.ColumnCount * solution.weightsMatrix.RowCount;
            int maxCount = GlobalVar.rnd.Next(1, (int)(totalLength * 0.25));
            var newSolution = Copy.DeepCopy(solution);
            newSolution.fitness1 = 0;
            for (int i = 0; i < maxCount; i++)
            {
                int rndColIndex = GlobalVar.rnd.Next(0, solution.weightsMatrix.ColumnCount);
                int sign = GlobalVar.rnd.Next(0, 2) == 0 ? -1 : 1;
                int rndRowIndex = GlobalVar.rnd.Next(0, solution.weightsMatrix.RowCount);
                newSolution.weightsMatrix[rndRowIndex, rndColIndex] += sign * GlobalVar.rnd.NextDouble() * minDelta;
                if (newSolution.weightsMatrix[rndRowIndex, rndColIndex] > 200)
                    newSolution.weightsMatrix[rndRowIndex, rndColIndex] = 200;
                if (newSolution.weightsMatrix[rndRowIndex, rndColIndex] < 0)
                    newSolution.weightsMatrix[rndRowIndex, rndColIndex] = 0;
            }
            return newSolution;
        }

        private Pair<int, double, double, Matrix<double>, double> Reproduction(Pair<int, double, double, Matrix<double>, double> solution)
        {
            double rnd = GlobalVar.rnd.NextDouble();
            if (rnd < pho)
            {
                return GroupingOperator(solution);
            }

            return WeightRefineOperator(solution);
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

            solution.fitness1 =  ceCountGroupsSolution.Sum() / (ceCountGroupsSolution.Length);
            
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
        private void FitnessEvaluation_old(Pair<int, double, double, Matrix<double>, double> solution)
        {
            //99,412 113,398 114,397 141,370 149,362
            //156,355 177,334 225,286 270,241 282,229
            //284,227 330,181 338,173 354,157 396,115 165,346

            readBranch rbce = new readBranch();
            Vector<double> coverprobs = Vector<double>.Build.Dense(numOfLabels);
            for (int j = 0; j < Sampledinputs.Count; j++)
            {
                int[] input = TestDataGeneration2.StringToInt(Sampledinputs.ElementAt(j).Key,sut);
                double prob = TestDataGeneration2.InputProbability(input, solution.weightsMatrix,sut);
                for (int k = 0; k < sut.numOfLabels; k++)
                {
                    if (Sampledinputs.ElementAt(j).Value[k] == 1)
                    {
                        coverprobs[k] += prob;
                    }
                }
            }
            Vector<double> ceCounts = Vector<double>.Build.Dense(sut.numOfLabels);
            int count = 0;
            for (int k = 0; k < numOfSamples; k++)
            {
                int[] output = null;
                int[] inputs;
                inputs = TestDataGeneration2.GenerateInput(solution.weightsMatrix, sut);
                count += 1;
                rbce.ReadBranchCLIFunc(inputs.ToArray(), ref output, sut.sutIndex);
                if (output[cheatCEIndex] == 1 && Sampledinputs.Count < 100)
                {
                    if (!Sampledinputs.ContainsKey(TestDataGeneration2.IntToString(inputs)))
                    {
                        Sampledinputs.Add(
                            TestDataGeneration2.IntToString(inputs),
                            output
                            );
                    }
                }
                for (int j = 0; j < ceCounts.Count; j++)
                {
                    if (output[j] == 1)
                    {
                        ceCounts[j] += 1;
                    }
                }
            }
            double fitness = -1;
            if (ceCounts.Divide(numOfSamples)[cheatCEIndex] < 0.1)
            {
                fitness = ceCounts.Divide(numOfSamples).Add(coverprobs)[cheatCEIndex];
            }
            else
            {
                fitness = ceCounts.Divide(numOfSamples)[cheatCEIndex];
            }
            solution.fitness1 = fitness;
        }
    }
}
