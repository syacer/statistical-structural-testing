using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;
using ReadSUTBranchCEData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using Accord.Statistics.Testing;

namespace GADEApproach.TrainditionalApproaches.GA
{

    public class GAAlgoithm
    {
        int cheatCEIndex= -1;
        int numOfSamples = 2000;
        int numOfSampeSet = 40;
        int generation = 0;
        int numOfLabels = -1;
        //GA Setup
        public int maxGen = 2;
        public double pho = 0.5;
        public int populationSize = 100;
        double crRate = 0.85;
        double mtRate = 0.2;
        int numOfbins;
        int globalBestIndex = -1;

        //index, fitness, fitness2, weights_Array
        Pair<int, double, double, Matrix<double>,double>[] pool;
        //SUT Setup
        SUT sut = null;

        public GAAlgoithm(int numOfMaxGen, int numLabels, SUT SUT, int CheatCEIndex)
        {
            maxGen = numOfMaxGen;
            numOfLabels = numLabels;
            sut = SUT;
            numOfbins = SUT.numOfMinIntervalInAllDim;
            cheatCEIndex = CheatCEIndex;
        }

        public void GA_Initialization()
        {
            sampledInputs = new List<int[]>();
            pool = new Pair<int, double, double, Matrix<double>,double>[populationSize];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = new Pair<int, double, double, Matrix<double>,double>();
                pool[i].index = i;
                pool[i].fitness1 = -1;
                pool[i].weightsMatrix = Matrix<double>.Build.Dense(numOfbins, sut.lowbounds.Length);
                for (int r = 0; r < pool[i].weightsMatrix.RowCount; r++)
                {
                    for (int c = 0; c < pool[i].weightsMatrix.ColumnCount; c++)
                    {
                        pool[i].weightsMatrix[r, c] = GlobalVar.rnd.Next(0,2) * 100;
                    }
                }
                //int[] indices = new int[sut.lowbounds.Length];
                //int tmpi = i;
                //for (int j = 0; j < indices.Length; j++)
                //{
                //    indices[j] = tmpi % sut.numOfMinIntervalInAllDim;
                //    tmpi = tmpi / sut.numOfMinIntervalInAllDim;
                //    pool[i].weightsMatrix[indices[j], j] = 200;
                //}
                //Console.WriteLine(pool[i].weightsMatrix.ToString());
            }
        }

        public void GA_Reproduction()
        {

            Pair<int, double, double, Matrix<double>,double>[] tempPool = new Pair<int, double, double, Matrix<double>,double>[populationSize];

            int startIndex = 1;
            //var rankOneSolution = pool.Where(x=>x.rank == pool.Max(y=>y.rank)).First();
            Console.WriteLine("aa {0}", globalBestIndex);
            tempPool[0] = Copy.DeepCopy(pool[globalBestIndex]);
            tempPool[0].index = 0;
            tempPool[0].rank = -1;

            for (int i = startIndex; i < populationSize; i++)    
            {
                int[] selectedList = FitnessPropotionateSampling();
                var child = Copy.DeepCopy(pool[selectedList[0]]);
                child.index = i;
                child.rank = -1;
                if (GlobalVar.rnd.NextDouble() <= crRate)
                {
                    child.weightsMatrix = GroupingRecombination(selectedList);
                }
                if (GlobalVar.rnd.NextDouble() <= mtRate)
                {
                    if (GlobalVar.rnd.NextDouble() < pho)
                    {
                        OnOffMutation(child.weightsMatrix);
                    }
                    else
                    {
                        ModifyWeightMutation(child.weightsMatrix);
                    }
                }

                tempPool[i] = child;
            }
            Array.Clear(pool, 0, pool.Length);
            pool = tempPool;
        }

        public List<double[]> TwoPointCrossOver(int[] data1, int[] data2)
        {
            int rndLoc1 = GlobalVar.rnd.Next(0,data1.Length-1);  //n-1 location
            int rndLoc2 = GlobalVar.rnd.Next(rndLoc1 + 1, data1.Length);
            double[] newData1 = new double[data1.Length];
            double[] newData2 = new double[data1.Length];

            for (int i = 0; i < newData1.Length; i++)
            {
                if (i <= rndLoc1 || i > rndLoc2)
                {
                    newData1[i] = data1[i];
                    newData2[i] = data2[i];
                }
                else
                {
                    newData1[i] = data2[i];
                    newData2[i] = data1[i];
                }
            }
            return new List<double[]>() {newData1, newData2 };
        }
        public Matrix<double> GroupingRecombination(int[] SelectedList)
        {
            Pair<int, double, Matrix<double>> newSolution = new Pair<int, double, Matrix<double>>();
            var wMatrix1 = pool[SelectedList[0]].weightsMatrix;
            var wMatrix2 = pool[SelectedList[1]].weightsMatrix;
            Matrix<double> newWeightMatrix = Matrix<double>.Build.Dense(wMatrix1.RowCount,wMatrix1.ColumnCount);

            for (int i = 0; i < wMatrix1.ColumnCount; i++)
            {
                int[] arrayW1 = wMatrix1.Column(i).Select(x =>
                {
                    if (x > 0)
                    {
                        return 1;
                    }
                    return 0;
                }).ToArray();
                int[] arrayW2 = wMatrix2.Column(i).Select(x =>
                {
                    if (x > 0)
                    {
                        return 1;
                    }
                    return 0;
                }).ToArray();
                var children = TwoPointCrossOver(arrayW1,arrayW2);
                for (int r = 0; r < wMatrix1.RowCount; r++)
                {
                    if (children[0][r] == 0)
                    {
                        newWeightMatrix[r, i] = 0;
                    }
                    else if (children[1][r] == 0)
                    {
                        newWeightMatrix[r, i] = wMatrix1[r, i];
                    }
                    else
                    {
                        newWeightMatrix[r, i] = (wMatrix1[r,i]+wMatrix2[r,i]) / 2;
                    }
                }
            }
            return newWeightMatrix;
        }
        internal void ModifyWeightMutation(Matrix<double> weightMatrix)
        {
            int totalLength = weightMatrix.Enumerate().Count(x=> x != 0);
            int MutationCounts = (int)(totalLength * 0.2);
            int maxCountOn = GlobalVar.rnd.Next(1, (int)Math.Ceiling((totalLength * 0.1)));
            for (int i = 0; i < MutationCounts;)
            {
                int rndColIndex = GlobalVar.rnd.Next(0, weightMatrix.ColumnCount);
                int rndRowIndex = GlobalVar.rnd.Next(0, weightMatrix.RowCount);
                if (weightMatrix[rndRowIndex, rndColIndex] != 0)
                {
                    weightMatrix[rndRowIndex, rndColIndex] = GlobalVar.rnd.NextDouble() * 200;
                    i++;
                }
            }
        }

        internal void OnOffMutation(Matrix<double> weightMatrix)
        {
            int totalLength = weightMatrix.RowCount * weightMatrix.ColumnCount;
            int maxCountOn = GlobalVar.rnd.Next(1, (int)(totalLength*0.1));
            int maxCountOff = GlobalVar.rnd.Next(1, (int)(totalLength * 0.1));

            for (int i = 0; i < maxCountOn; i++)
            {
                int rndColIndex = GlobalVar.rnd.Next(0, weightMatrix.ColumnCount);
                int rndRowIndex = GlobalVar.rnd.Next(0, weightMatrix.RowCount);
                if (weightMatrix[rndRowIndex, rndColIndex] == 0)
                {
                    weightMatrix[rndRowIndex, rndColIndex] = 100;
                }
            }

            for (int i = 0; i < maxCountOff; i++)
            {
                int rndColIndex = GlobalVar.rnd.Next(0, weightMatrix.ColumnCount);
                int rndRowIndex = GlobalVar.rnd.Next(0, weightMatrix.RowCount);
                weightMatrix[rndRowIndex, rndColIndex] = 0;
            }
        }
        internal Matrix<double> PanmicticRecomb(int[] selectedList)
        {
            // Select solution to combine
            // Find Average
            Matrix<double> avgWeight = Matrix<double>.Build.Dense(numOfbins, pool[0].weightsMatrix.ColumnCount);

            for (int i = 0; i < selectedList.Length; i++)
            {
                avgWeight += pool[i].weightsMatrix;
            }
            avgWeight = avgWeight / selectedList.Length;

            for (int i = 0; i < avgWeight.ColumnCount; i++)
            {
                for (int j = 0; j < avgWeight.RowCount; j++)
                {
                    avgWeight[j, i] = avgWeight[j, i] > 200 ? 200 : avgWeight[j,i];
                }
            }
            return avgWeight;
        }

        private int[] FitnessPropotionateSampling()
        {
            int size = 5;
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
            double sum = pool.Sum(x => x.fitness1);
            for (int i = 0; i < max - min + 1; i++)
            {
                fitnessPropotions[i] = pool[i].fitness1 / sum;
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
        List<int[]> sampledInputs = new List<int[]>();
        void GA_FitnessEvaluation()
        {
            readBranch rbce = new readBranch();
            List<Tuple<int, double[]>> populationOfCECountGroups = new List<Tuple<int, double[]>>();
            for (int i = 0; i < pool.Length; i++)
            {
                double probability = 0;
                for (int j = 0; j < sampledInputs.Count; j++)
                {
                    double prob = TestDataGeneration2.InputProbability(sampledInputs[j], pool[i].weightsMatrix, sut);
                    probability += prob;
                }

                Tuple<int,double[]> ceCountGroups = new Tuple<int,double[]>(i,new double[numOfSampeSet]);
                for (int g = 0; g < numOfSampeSet; g++)
                {
                    int ceCount = 0;
                    for (int s = 0; s < numOfSamples / numOfSampeSet; s++)
                    {
                        int[] output = null;
                        int[] inputs;
                        inputs = TestDataGeneration2.GenerateInput(pool[i].weightsMatrix, sut);
                        rbce.ReadBranchCLIFunc(inputs.ToArray(), ref output, sut.sutIndex);
                        if (output[cheatCEIndex] == 1)
                        {
                            if (sampledInputs.Count < 100
                                && sampledInputs.Where(x => x.SequenceEqual(inputs)==true).Count() == 0
                                )
                            {
                                sampledInputs.Add(inputs);
                            }
                            ceCount += 1;
                        }
                    }
                    ceCountGroups.Item2[g] = ceCount*1.0 / (numOfSamples / numOfSampeSet);
                }
                populationOfCECountGroups.Add(ceCountGroups);
                double estimateFitness = ceCountGroups.Item2.Sum() / ceCountGroups.Item2.Length;
                if (estimateFitness < 0.05)
                {
                    pool[i].fitness1 = probability;
                    pool[i].rank = 1;
                }
                else
                {
                    pool[i].fitness1 = estimateFitness;
                }
            }

            int maxCount = 0;
            for (int i = 1; i < populationOfCECountGroups.Count; i++)
            {
                int ret = HypothesisTesting.RankSumTest(
                    populationOfCECountGroups[maxCount].Item2,
                    populationOfCECountGroups[i].Item2);
                if (ret < 0)
                {
                    maxCount = i;
                }
            }
            globalBestIndex = populationOfCECountGroups[maxCount].Item1;
            if (pool[globalBestIndex].rank == 1)
            {
                globalBestIndex = pool.Where(x => x.fitness1 == pool.Max(y => y.fitness1)).First().index;
            }

            //populationOfCECountGroups.Sort( (x,y) => HypothesisTesting.RankSumTest(x.Item2, y.Item2));
            //// Higher Rank, Better fitness
            //int rank = 1;
            //for (int i = 0; i < populationOfCECountGroups.Count; i++)
            //{
            //    pool[populationOfCECountGroups[i].Item1].rank = rank;
            //    rank += 1;
            //}
            //Console.WriteLine(populationOfCECountGroups[populationOfCECountGroups.Count-1].Item1);
        }

        Dictionary<string, int[]> Sampledinputs = new Dictionary<string, int[]>();
        void GA_FitnessEvaluation_OLD()
        {
            readBranch rbce = new readBranch();
            for (int i = 0; i < pool.Length; i++)
            {
                Vector<double> coverprobs = Vector<double>.Build.Dense(numOfLabels);
                for (int j = 0; j < Sampledinputs.Count; j++)
                {
                    int[] input = TestDataGeneration2.StringToInt(Sampledinputs.ElementAt(j).Key,sut);
                    double prob = TestDataGeneration2.InputProbability(input,pool[i].weightsMatrix,sut);
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
                    inputs = TestDataGeneration2.GenerateInput(pool[i].weightsMatrix, sut);
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
                pool[i].fitness1 = fitness;
            }
        }
        public void GA_Start(record record)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            GA_Initialization();
            GA_FitnessEvaluation();
            Queue<double> lastNumOffitness = new Queue<double>();
            while (generation < maxGen)
            {
                watch.Start();
                GA_Reproduction();
                GA_FitnessEvaluation();
                watch.Stop();
                var bestSolution = pool[globalBestIndex];
                Console.WriteLine("CE: {0}, Run: {1}, Fitness: {2}", cheatCEIndex,generation, bestSolution.fitness1);
                record.fitnessGen[generation] = bestSolution.fitness1;

                var statistics = new DescriptiveStatistics(lastNumOffitness.ToArray());
                if (Math.Round(statistics.Maximum,2) == 1)
                {
                    break;
                }
                if (lastNumOffitness.Count == 20)
                {
                    lastNumOffitness.Dequeue();
                    lastNumOffitness.Enqueue(bestSolution.fitness1);

                    double stdiv = statistics.StandardDeviation;
                    if (stdiv < 0.05)
                    {
                        break;
                    }
                }
                else
                {
                    lastNumOffitness.Enqueue(bestSolution.fitness1);
                }

                generation += 1;
            }

            watch.Stop();

            var BestbestSolution = pool[globalBestIndex];
            var totalRuningTime = watch.ElapsedMilliseconds;
            record.bestSolution.totalRunTime = totalRuningTime;
            Vector<double> weightVector = Vector<double>.Build.Dense(numOfbins,1);
            for (int i = 0; i < BestbestSolution.weightsMatrix.ColumnCount; i++)
            {
                weightVector = weightVector.PointwiseMultiply(BestbestSolution.weightsMatrix.Column(i));
            }
            weightVector.Normalize(2.0);
            record.bestSolution.setProbabilities = Copy.DeepCopy(weightVector.ToArray());
            record.bestSolution.WeightMateix = BestbestSolution.weightsMatrix;
        }
    }
}
