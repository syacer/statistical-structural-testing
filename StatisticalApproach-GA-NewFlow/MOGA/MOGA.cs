using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Distributions.Multivariate;
using System.Threading.Tasks;
using StatisticalApproach.Framework;

namespace StatisticalApproach.MOGA
{
    using MathNet.Numerics.LinearAlgebra;
    using Utility;
    using AppFunc = Func<EnvironmentVar, Task>;
    using CEPool = Dictionary<GAEncoding, double[]>;
    [Serializable]
    class CustMOGA
    {
        public AppFunc _next;
        private EnvironmentVar enVar;
        private Record _record;
        private int _runIndex = 0;
        private int gen = 0;
        int popSize = 50;
        public int dimension = 0;
        public CEPool cePool = null;
        public CEPool tempPool = null;
        public int maxGen = 20000;
        public double pmCrossOverRate = 0.8;
        public double pmMutationRate = 0.1;
        int pNumOfParams = 30;
        int pPanCombSize = 5;

        public CustMOGA(Record record, int runIndex, AppFunc next)
        {
            _next = next;
            _record = record;
            _runIndex = runIndex;
        }

        public async Task<int> Invoke(EnvironmentVar envar)
        {
            enVar = envar;
            dimension = (int)enVar.pmProblem["Dimension"];
            await Task.Run(() => MOGA_Start());
            await _next(enVar);
            return 0;
        }

        public void PopulationGen()
        {
            //if (gen != 0 && cePool.OrderByDescending(x => x.Value[0]).First().Value[0] >
            //    tempPool.OrderByDescending(x => x.Value[0]).First().Value[0])
            //{
            //    var temp = Copy.DeepCopy(cePool.OrderByDescending(x => x.Value[0]).First());
            //    cePool.Clear();
            //    cePool.Add(temp.Key, temp.Value);
            //    for (int i = 0; i < popSize; i++)
            //    {
            //        if (tempPool.OrderBy(X => X.Value[0]).First().Key
            //            == tempPool.ElementAt(i).Key)
            //        {
            //            continue;
            //        }
            //        cePool.Add(
            //            Copy.DeepCopy(tempPool.ElementAt(i).Key),
            //            tempPool.ElementAt(i).Value
            //        );
            //    }
            //}
            //else
            //{
                cePool.Clear();
                for (int i = 0; i < popSize; i++)
                {
                    cePool.Add(
                        Copy.DeepCopy(tempPool.ElementAt(i).Key),
                        tempPool.ElementAt(i).Value
                    );
                }
            //}
            tempPool.Clear();
        }

        public async void MOGA_FitnessEvaluation(int[] token)
        {
            List<Task> lTasks = new List<Task>();
            int remainTasks = tempPool.Count;
            // Test inputs Generation
            // Fitness Evaluation
            for (int k = 0; k < tempPool.Count; k++)
            {
                //Console.WriteLine("{0}'th genotype fitness assement", k);
                GAEncoding ga = tempPool.ElementAt(k).Key;

                //// Single Thread
                //var tmpTuple = ga.CalEstFitness(enVar);
                //tempPool[ga][0] = tmpTuple.Item1;
                //ga.SetCEFitness(tmpTuple.Item2.ToArray());

                lTasks.Add(Task.Run(() =>
                {
                    ga.CalEstFitness(enVar);
                    // Console.WriteLine("{0}'th finished assement", k);
                }));
                if (lTasks.Count == 8 || (tempPool.Count - k - 1 < 8))
                {
                    for (int i = 0; i < lTasks.Count; i++)
                    {
                        await lTasks[i];
                    }
                    lTasks.Clear();
                }
            }
            token[0] = 1;
            //Console.WriteLine("Fitness Evaluation Done");
        }

        public List<string> Print_Solution(GAEncoding solution)
        {
            string theta = null;
            string weight = null;

            for (int i = 0; i < solution.thetaDelta.RowCount; i++)
            {
                string tmp = "(";
                Vector<double> vTemp = solution.thetaDelta.Row(i);
                for (int j = 0; j < dimension; j++)
                {
                    tmp = tmp + Math.Round(vTemp[j], 2) + " ";
                }
                tmp.Remove(tmp.Length - 1, 1);
                tmp = tmp + ")";
                theta = theta + tmp + " ";
            }
            theta.Remove(theta.Length - 1, 1);

            for (int i = 0; i < solution.weights.RowCount; i++)
            {
                string tmp = "[";
                Vector<double> vTemp = solution.weights.Row(i);
                for (int j = 0; j < dimension; j++)
                {
                    tmp = tmp + Math.Round(vTemp[j], 2) + " ";
                }
                tmp.Remove(tmp.Length - 1, 1);
                tmp = tmp + "]";
                weight = weight + tmp + " ";
            }
            weight.Remove(weight.Length - 1, 1);

            return new List<string>() { theta, weight };
        }

        public void MOGA_Start()
        {
            LocalFileAccess lfa = new LocalFileAccess();
            _record.Watch[_runIndex].Start();
            MOGA_Initialization();
            int[] token = new int[1];
            Console.WriteLine("In Fitness Evaluation...");
            MOGA_FitnessEvaluation(token);
            while (token[0] == 0);
            token[0] = 0;
            MOGA_NormalizeFitness();
            PopulationGen(); //tmpPool -> cePool


            // Update Records
            List<string> generation = new List<string>() { gen.ToString() };
            List<string> currentCElist = new List<string>();
            List<string> currentBestSolution = new List<string>();
            List<double> currentFitnessList = new List<double>();
            Console.WriteLine("Generation: {0}", gen);
            List<GAEncoding> currentBests = cePool.Where(x => x.Key.rank == 1).Select(x => x.Key).ToList();
            currentBests = cePool.Where(x => x.Key.rank == 1).Select(x => x.Key).ToList();
            var currentBest = currentBests[0];
            double[] ceCurrentScores = currentBest.fitnessPerCE.ToArray();
            for (int i = 0; i < currentBests.Count; i++)
            {
                Console.WriteLine("Solution: {1}, Paths Found: {0}", currentBests[i].pathRecord.Count, i);
                Console.WriteLine("Solution: {1}, Entropy: {0}", currentBests[i].entropy, i);
                Console.WriteLine("Solution: {1}, Duplicates: {0}", currentBests[i].duplicateSamples, i);

            }


            //// Display Path Information
            //double pathEva = 0;
            //for (int i = 0; i < currentBest.pathRecord.Count; i++)
            //{
            //    pathEva = Math.Pow((currentBest.pathRecord.Values.ToArray()[i]
            //        - (1.0 / currentBest.pathRecord.Count)), 2) + pathEva;
            //}
            //pathEva = 27 + Math.Sqrt(pathEva) / currentBest.pathRecord.Count - currentBest.pathRecord.Count;
            //for (int i = 0; i < ceCurrentScores.Length; i++)
            //{
            //    string dataOut = "CE: " + i.ToString() + ", " + "Fit: " + ceCurrentScores[i].ToString();
            //    currentCElist.Add(dataOut);
            //    //currentFitnessList.Add(ceCurrentScores[i]);
            //    //currentFitnessList.Add((cePool[currentBest])[0]);
            //    currentFitnessList.Add(pathEva);
            //    Console.WriteLine(dataOut);
            //}


            //Console.WriteLine("Paths Found: {0}", currentBest.pathRecord.Count);
            //Console.WriteLine("Path Eva: {0}",pathEva);

            //Console.WriteLine("Entropy: {0}", cePool[currentBest][0]);
            //Console.WriteLine("Duplicates: {0}", currentBest.duplicateSamples);
            //Console.WriteLine("TestSet: {0}", currentBest.strTestSet);
            // Output CurrentBest Result;
            _record.currentBestSolution[_runIndex][0] = Print_Solution(currentBest);
            _record.currentGen[_runIndex] = generation;
            _record.currentCElist[_runIndex] = currentCElist;
            _record.currentFitnessList[_runIndex] = currentFitnessList;
            _record.updateIndicate[_runIndex] = true;
            while (_record.updateIndicate[_runIndex] == true) ;

            // Start MOGA
            while (gen < maxGen)
            {
                gen = gen + 1;
                Reproduction();
                MOGA_FitnessEvaluation(token);
                while (token[0] == 0);
                token[0] = 0;
                MOGA_NormalizeFitness();
                PopulationGen();

                // Update Records
                generation = new List<string>() { gen.ToString() };
                currentCElist = new List<string>();
                currentBestSolution = new List<string>();
                currentFitnessList = new List<double>();
                Console.WriteLine("Generation: {0}", gen);
                currentBests = cePool.Where(x => x.Key.rank == 1).Select(x => x.Key).ToList();
                for (int i = 0; i < currentBests.Count; i++)
                {
                    Console.WriteLine("Solution: {1}, Paths Found: {0}", currentBests[i].pathRecord.Count, i);
                    Console.WriteLine("Solution: {1}, Entropy: {0}", currentBests[i].entropy, i);
                    Console.WriteLine("Solution: {1}, Duplicates: {0}", currentBests[i].duplicateSamples, i);
                    
                }

                //for (int i = 0; i < ceCurrentScores.Length; i++)
                //{
                //    string dataOut = "CE: " + i.ToString() + ", " + "Fit: " + ceCurrentScores[i].ToString();
                //    currentCElist.Add(dataOut);
                //    //currentFitnessList.Add(ceCurrentScores[i]);
                //    //currentFitnessList.Add((cePool[currentBest])[0]);
                //    currentFitnessList.Add(pathEva);
                //    Console.WriteLine(dataOut);
                //}

                //Console.WriteLine("Path Eva: {0}", pathEva);
                //Console.WriteLine("Entropy: {0}", cePool[currentBest][0]);
                //Console.WriteLine("Duplicates: {0}", currentBest.duplicateSamples);
                //Console.WriteLine("TestSet: {0}", currentBest.strTestSet);
                //// Output CurrentBest Result;
                _record.currentBestSolution[_runIndex][0] = Print_Solution(currentBest);
                _record.currentGen[_runIndex] = generation;
                _record.currentCElist[_runIndex] = currentCElist;
                _record.currentFitnessList[_runIndex] = currentFitnessList;
                _record.updateIndicate[_runIndex] = true;
                while (_record.updateIndicate[_runIndex] == true) ;
            }
            _record.Watch[_runIndex].Stop();
            enVar.finishIndicate = true;
        }
        private void MOGA_NormalizeFitness()
        {
            //Form Total Order, (A, B) A: Entropy, B: Duplicates
            int rank = 1;
            int[] dominatedList = new int[tempPool.Count];

            while (!dominatedList.All(x=>x==-1))
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
                            tempPool.ElementAt(i).Key.entropy == tempPool.ElementAt(j).Key.entropy
                            && tempPool.ElementAt(i).Key.duplicateSamples ==
                            tempPool.ElementAt(j).Key.duplicateSamples)
                        {
                            continue;
                        }
                        if (tempPool.ElementAt(i).Key.entropy <= tempPool.ElementAt(j).Key.entropy
                            && tempPool.ElementAt(i).Key.duplicateSamples >
                            tempPool.ElementAt(j).Key.duplicateSamples)
                        {
                            dominatedList[i] += 1;
                        }
                    }
                }
                for (int i = 0; i < dominatedList.Length; i++)
                {
                    if (dominatedList[i] == 0)
                    {
                        tempPool.ElementAt(i).Key.rank = rank;
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
            for (int i = 0; i < popSize; i++)
            {
                tempPool.ElementAt(i).Value[0] = 2 * (rank - tempPool.ElementAt(i).Key.rank + 1) * 1.0 / (popSize * (popSize - 1));
            }
        }

        private void MOGA_Initialization()
        {
            //CEPool cePool = null;
            //CEPool tempPool = null;
            // public CEPool ceFitness = null;
            cePool = new CEPool();
            tempPool = new CEPool();
            for (int i = 0; i < popSize; i++)
            {
                var newSolution = new GAEncoding(pNumOfParams, dimension, enVar);
                newSolution.Randomize(enVar);
                tempPool.Add(newSolution,new double[] { -1, -1 });
            }
        }

        public void Reproduction()
        {
            GAEncoding child = new GAEncoding(pNumOfParams, dimension,enVar);
            int i = 0;
            var rankOneSols = cePool.Where(x => x.Key.rank == 1).ToList();
            foreach (var s in rankOneSols)
            {
                tempPool.Add(Copy.DeepCopy(s.Key),s.Value);
            }
            while (tempPool.Count < popSize)
            {
                int[] selectedList = FitnessPropotionateSampling(0, popSize - 1, pPanCombSize);
                child = Copy.DeepCopy(cePool.ElementAt(selectedList[0]).Key);
                if (enVar.rnd.NextDouble() <= pmCrossOverRate)
                {
                    child = cePool.ElementAt(0).Key.PanmicticRecomb(cePool,selectedList);
                }

                if (enVar.rnd.NextDouble() <= pmMutationRate)
                {
                    child.RealVectSimpleMutation();
                }
                if (child != null)
                {
                    tempPool.Add(child, new double[] {-1, -1});
                    i = i + 1;
                }
            }
        }

        private int[] FitnessPropotionateSampling(int min, int max, int size)
        {
            int[] result = new int[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = -1;
            }

            // Normalize Fitness
            int tmpCnt = 0;
            double sum = cePool.Values.Sum(x => x[0]);
            for (int i = 0; i < max - min + 1; i++)
            {
                cePool.ElementAt(i).Value[0] = cePool.ElementAt(i).Value[0] / sum;
            }
            double[] cumulativeArray = new double[popSize];
            double cumulation = 0.0;
            for (int i = 0; i < popSize; i++)
            {
                cumulation = cumulation + cePool.ElementAt(i).Value[0];
                cumulativeArray[i] = cumulation;
            }
            int timerCount = 0;
            while (true)
            {
                if (tmpCnt == size)
                {
                    break;
                }
                double nextRnd = enVar.rnd.NextDouble();
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
                    Console.WriteLine("Selection Reach 100.");
                    for (int i = 0; i < size; i++)
                    {
                        result[i] = i;
                    }
                    break;
                }
            }
            return result;
        }

        public int[] NonReplacementUniformSampling(int min, int max, int size)
        {
            int[] result = new int[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = -1;
            }

            int tmpCnt = 0;
            while (true)
            {
                if (tmpCnt == size)
                {
                    break;
                }
                int nextRnd = enVar.rnd.Next(min, max);
                if (result.Where(x => x == nextRnd).Count() == 0)
                {
                    result[tmpCnt] = nextRnd;
                    tmpCnt = tmpCnt + 1;
                }
            }
            return result;
        }
    }
}
