﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.Threading;

// Looks like it is not a good method to use Entropy. So Sepetate each cover elment in evlovment
// Fitness Changes in FitnessPropotinateSampling. IT IS WRONG!!!! see correct implementation in WeightEvolve
namespace Core
{
    using CEPool = Dictionary<GAEncoding, double[]>;
    [Serializable]
    class CustMOGA
    {
        private int gen = 0;
        int popSize = 100;
        public int dimension = 2;
        public CEPool cePool = null;
        public CEPool tempPool = null;
        public int maxGen = 20000;
        public double pmCrossOverRate = 0.8;
        public double pmMutationRate = 0.2;
        int pNumOfParams = 50;
        int pPanCombSize = 6;
        int testSetSize = 100;
        int numOfLabel = 10;
        int numOfNNLocalSearch = 0;
        double[] lowbounds = new double[] { 0.0, 0.0 };
        double[] highbounds = new double[] { 50, 50 };
        Matrix<double> labelMatrix;
        public CEPool ceDB = null;

        public CustMOGA()
        {
            LabelMatrixCreation();
            MOGA_Initialization();
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
            for (int k = 0; k < tempPool.Count; )
            {
                // Single Thread
                //ga.CalEstFitness(testSetSize, numOfLabel, labelMatrix, lowbounds, highbounds);

                lTasks.Add(Task.Run(() =>
                {
                    GlobalVar.mutex_K.WaitOne();
                    //Console.WriteLine(k);
                    GAEncoding ga = tempPool.ElementAt(k).Key;
                    k = k + 1;
                    GlobalVar.mutex_K.ReleaseMutex();
                    //ga.CalEstFitness(testSetSize, numOfLabel, labelMatrix, lowbounds, highbounds);
                    ga.TrueFitnessCal(numOfLabel, labelMatrix);
                        // Console.WriteLine("{0}'th finished assement", k);
                    }));
                if (lTasks.Count == 8 || (tempPool.Count - k - 1 < 8))  //8,8
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

        void UpdateRecordAndDisplay()
        {
            List<string> generation = new List<string>() { gen.ToString() };
            List<string> currentCElist = new List<string>();
            List<string> currentBestSolution = new List<string>();
            List<double> currentFitnessList = new List<double>();
            List<Tuple<double, int>> fitness = new List<Tuple<double, int>>();

            Console.WriteLine("Generation: {0}", gen);
            List<GAEncoding> currentBests = cePool.Where(x => x.Key.rank == 1).Select(x => x.Key).ToList();
            // Update Records
            generation = new List<string>() { gen.ToString() };
            currentCElist = new List<string>();
            currentBestSolution = new List<string>();
            currentFitnessList = new List<double>();
            Console.WriteLine("Generation: {0}", gen);
            currentBests = cePool.Where(x => x.Key.rank == 1).Select(x => x.Key).ToList();
            LocalFileAccess lfa = new LocalFileAccess();
            foreach (var best in currentBests)
            {
                lfa.StoreListToLinesAppend(@"C:\Users\Yang Shi\Desktop\record", Print_Solution(best));
            }
            for (int i = 0; i < currentBests.Count; i++)
            {
                fitness.Add(new Tuple<double, int>(currentBests[i].entropy, currentBests[i].duplicateSamples));
                Console.WriteLine("Solution: {1}, Entropy: {0}", currentBests[i].entropy, i);
                Console.WriteLine("Solution: {1}, Duplicates: {0}", currentBests[i].duplicateSamples, i);
            }

            var solutions = Print_Solution(currentBests[0]);
        }

        public void UpdateDatabase()
        {
            for (int i = 0; i < cePool.Count; i++)
            {
                if (ceDB.Where(x => x.Key.id == cePool.ElementAt(i).Key.id).Count() == 0)
                {
                    ceDB.Add(Copy.DeepCopy(cePool.ElementAt(i).Key), new double[] {1});
                }
                else
                {
                   var tmp = ceDB.Where(x => x.Key.id == cePool.ElementAt(i).Key.id).First();
                   tmp.Key.numOfLabelsVect = (tmp.Key.numOfLabelsVect.Multiply(tmp.Value[0]) + cePool.ElementAt(i).Key.numOfLabelsVect)
                         .Divide(tmp.Value[0] + 1);
                    tmp.Value[0] += 1;
                }
            }
        }

        public void LocalSearch()
        {


            // 1. Pick up number of Nearest Neighbor Solutions
            for (int m = 0; m < popSize; m++)
            {
                CEPool NNSolutions = new CEPool();
                List<Vector<double>> L2DistVects = new List<Vector<double>>();
                for (int j = 0; j < ceDB.Count; j++)
                {
                    L2DistVects.Add((cePool.ElementAt(m).Key.weights - ceDB.ElementAt(j).Key.weights).RowNorms(2.0));
                }

                int[] dominatedList = new int[L2DistVects.Count];

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
                                L2DistVects[i].SequenceEqual(L2DistVects[j]))
                            {
                                continue;
                            }
                            if ((L2DistVects[i]-L2DistVects[j]).All(x=>x>=0))
                            {
                                dominatedList[i] += 1;
                            }
                        }
                    }
                    for (int i = 0; i < dominatedList.Length; i++)
                    {
                        if (dominatedList[i] == 0)
                        {   
                            // Value is not the fitness, need to re-evaluate Fitness
                            NNSolutions.Add(ceDB.ElementAt(i).Key, ceDB.ElementAt(i).Value);  
                            dominatedList[i] = -1;
                        }
                        else if (dominatedList[i] != -1)
                        {
                            dominatedList[i] = 0;
                        }
                    }
                    if (NNSolutions.Count >= numOfNNLocalSearch)
                    {
                        break;
                    }
                }

                // Using NNSolutions to build up Surrogate Model
                // Trust-Region Related Local Search to Find Local Optima
                // This is only Fake Test
                double maxEntropy = 0;
                int index = 0;
                for (int i = 0; i < NNSolutions.Count; i++)
                {
                    double entropy = 0;
                    for (int j = 0; j < numOfLabel; j++)
                    {
                        entropy += NNSolutions.ElementAt(i).Key.numOfLabelsVect[j] 
                            * Math.Log10(1.0 / (NNSolutions.ElementAt(i).Key.numOfLabelsVect[j] + 0.000001));
                    }
                    if (entropy > maxEntropy)
                    {
                        maxEntropy = entropy;
                        index = i;
                    }
                }
                cePool.Remove(cePool.ElementAt(m).Key);
                cePool.Add(Copy.DeepCopy(ceDB.ElementAt(index).Key), new double[] { -1 });
            }
        }
        public void MOGA_Start()
        {
            MOGA_Initialization();
            int[] token = new int[1];
            Console.WriteLine("In Fitness Evaluation...");
            MOGA_FitnessEvaluation(token);
            while (token[0] == 0) ;
            token[0] = 0;
            MOGA_NormalizeFitness(true);
           // AdditionalSampling();
            PopulationGen(); //tmpPool -> cePool
            UpdateDatabase();
            UpdateRecordAndDisplay();


            // Start MOGA
            while (gen < maxGen)
            {
                gen = gen + 1;
                Reproduction();
                MOGA_FitnessEvaluation(token);
                while (token[0] == 0) ;
                token[0] = 0;
                MOGA_NormalizeFitness(true);
               // AdditionalSampling();
                PopulationGen();
                UpdateRecordAndDisplay();
                if (ceDB.Count < numOfNNLocalSearch)
                {
                    UpdateDatabase();
                }
                else
                {
                    //LocalSearch();
                    ceDB.Clear();
                }
            }
        }

        private void AdditionalSampling()
        {
            int paramR_Max = 10;
            int paramM = 3;
            double paramAlfa = 2;
            int n = 0, n_min = testSetSize, n_max = testSetSize * 100;
            double r = 0;

            paramR_Max = tempPool.Max(x => x.Key.rank);
            paramM = (int)(paramR_Max * 0.4);
            var tmp = Copy.DeepCopy(tempPool.Where(x => x.Key.rank > paramM).ToList());
            var tmp2 = tempPool.Where((x)=> x.Key.rank > paramM);
            
            while (tmp2.Count() != 0)
            {
                tempPool.Remove(tmp2.ElementAt(0).Key);
            }
            //Linear Dynamic Sampling
            for (int i = 0; i < tempPool.Count; i++)
            {
                int R = tempPool.ElementAt(i).Key.rank;
                if (R >= paramR_Max)
                {
                    r = 1;
                }
                else
                {
                    r = 1 - Math.Pow((Math.Min(R, paramM) - 1)*1.0 / (Math.Min(paramR_Max, paramM) - 1), paramAlfa);
                }
                n = Convert.ToInt16(n_min + (n_max - n_min) * r) + n_min;
                n = 1000;
                tempPool.ElementAt(i).Key.CalEstFitness(n,numOfLabel,labelMatrix,lowbounds,highbounds);
            }
            MOGA_NormalizeFitness(false);

            for (int i = 0; i < tmp.Count(); i++)
            {
                tempPool.Add(tmp.ElementAt(i).Key, tmp.ElementAt(i).Value);
            }
        }

        private void MOGA_NormalizeFitness(bool needFintessCal)
        {
            //Form Total Order, (A, B) A: Entropy, B: Duplicates
            int rank = 1;
            int[] dominatedList = new int[tempPool.Count];

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
                            tempPool.ElementAt(i).Key.entropy == tempPool.ElementAt(j).Key.entropy
                            && tempPool.ElementAt(i).Key.duplicateSamples ==
                            tempPool.ElementAt(j).Key.duplicateSamples)
                        {
                            continue;
                        }
                        if (tempPool.ElementAt(i).Key.entropy <= tempPool.ElementAt(j).Key.entropy
                            && tempPool.ElementAt(i).Key.duplicateSamples >=
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
            // Fitness calculation: Linear Ranking with rank reversal
            if (needFintessCal == false)
            {
                return;
            }
            for (int i = 0; i < popSize; i++)
            {
                tempPool.ElementAt(i).Value[0] = 2 * (rank - tempPool.ElementAt(i).Key.rank + 1) * 1.0 / (popSize * (popSize - 1));
            }
        }

        private void MOGA_Initialization()
        {
            cePool = new CEPool();
            tempPool = new CEPool();
            ceDB = new CEPool();
            Console.BackgroundColor = ConsoleColor.DarkRed;
            numOfNNLocalSearch = 1000; //(pNumOfParams + 1) * (pNumOfParams + 2) / 2 * dimension;
            for (int i = 0; i < popSize; i++)
            {
                var newSolution = new GAEncoding(pNumOfParams, dimension, lowbounds, highbounds);
                newSolution.FixInputs(lowbounds, highbounds);
                tempPool.Add(newSolution, new double[] { -1, -1 });
            }
        }

        public void Reproduction()
        {
            GAEncoding child = new GAEncoding(pNumOfParams, dimension, lowbounds, highbounds);
            int i = 0;
            var rankOneSols = cePool.Where(x => x.Key.rank == 1).ToList();
            foreach (var s in rankOneSols)
            {
                tempPool.Add(Copy.DeepCopy(s.Key), s.Value);
            }
            while (tempPool.Count < popSize)
            {
                int[] selectedList = FitnessPropotionateSampling(0, popSize - 1, pPanCombSize);
                child = Copy.DeepCopy(cePool.ElementAt(selectedList[0]).Key);
                if (GlobalVar.rnd.NextDouble() <= pmCrossOverRate)
                {
                    if (GlobalVar.rnd.NextDouble() <= 0.8)
                    {
                        child = cePool.ElementAt(0).Key.PanmicticDiscreteRecomb(cePool, selectedList);
                    }
                    else
                    {
                        child = cePool.ElementAt(0).Key.PanmicticAvgRecomb(cePool, selectedList);
                    }
                }

                if (GlobalVar.rnd.NextDouble() <= pmMutationRate)
                {
                    child.RealVectSimpleMutation();
                }
                if (child != null)
                {
                    tempPool.Add(child, new double[] { -1, -1 });
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
                int nextRnd = GlobalVar.rnd.Next(min, max);
                if (result.Where(x => x == nextRnd).Count() == 0)
                {
                    result[tmpCnt] = nextRnd;
                    tmpCnt = tmpCnt + 1;
                }
            }
            return result;
        }
        public void LabelMatrixCreation()
        {
            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));
            for (int i = 0; i < (int)(highbounds[0] - lowbounds[0] + 1); i++)
            {
                for (int j = 0; j < (int)(highbounds[1] - lowbounds[1] + 1); j++)
                {
                    if (i <= 2)
                    {
                        if (j <= 1)
                        {
                            labelMatrix[i, j] = 1;
                        }
                        else if (j <= 4)
                        {
                            labelMatrix[i, j] = 8;
                        }
                        else if (j <= 8)
                        {
                            labelMatrix[i, j] = 3;
                        }
                        else if (j <= 40)
                        {
                            labelMatrix[i, j] = 8;
                        }
                        else if (j <= 50)
                        {
                            labelMatrix[i, j] = 9;
                        }
                    }
                    else if (i <= 4)
                    {
                        if (j <= 1)
                        {
                            labelMatrix[i, j] = 5;
                        }
                        else if (j <= 4)
                        {
                            labelMatrix[i, j] = 4;
                        }
                        else if (j <= 8)
                        {
                            labelMatrix[i, j] = 2;
                        }
                        else if (j <= 40)
                        {
                            labelMatrix[i, j] = 2;
                        }
                        else if (j <= 50)
                        {
                            labelMatrix[i, j] = 10;
                        }
                    }
                    else if (i <= 49)
                    {
                        if (j <= 1)
                        {
                            labelMatrix[i, j] = 6;
                        }
                        else if (j <= 4)
                        {
                            labelMatrix[i, j] = 3;
                        }
                        else if (j <= 8)
                        {
                            labelMatrix[i, j] = 2;
                        }
                        else if (j <= 40)
                        {
                            labelMatrix[i, j] = 2;
                        }
                        else if (j <= 50)
                        {
                            labelMatrix[i, j] = 9;
                        }
                    }
                    else if (i <= 50)
                    {
                        if (j <= 1)
                        {
                            labelMatrix[i, j] = 7;
                        }
                        else if (j <= 4)
                        {
                            labelMatrix[i, j] = 6;
                        }
                        else if (j <= 8)
                        {
                            labelMatrix[i, j] = 7;
                        }
                        else if (j <= 40)
                        {
                            labelMatrix[i, j] = 1;
                        }
                        else if (j <= 50)
                        {
                            labelMatrix[i, j] = 10;
                        }
                    }
                }
            }
        }

        public void LabelMatrixCreation2()
        {
            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));
            for (int i = 0; i < (int)(highbounds[0] - lowbounds[0] + 1); i++)
            {
                for (int j = 0; j < (int)(highbounds[1] - lowbounds[1] + 1); j++)
                {
                    if (i < 400)
                    {
                        if (j < 400)
                        {
                            labelMatrix[i, j] = 1;
                        }
                        else if (j < 450)
                        {
                            labelMatrix[i, j] = 4;
                        }
                        else if (j < 500)
                        {
                            labelMatrix[i, j] = 1;
                        }
                        else if (j < 1000)
                        {
                            labelMatrix[i, j] = 6;
                        }
                    }
                    else if (i < 450)
                    {
                        if (j < 400)
                        {
                            labelMatrix[i, j] = 1;
                        }
                        else if (j < 45)
                        {
                            labelMatrix[i, j] = 5;
                        }
                        else if (j < 925)
                        {
                            labelMatrix[i, j] = 3;
                        }
                        else if (j < 1000)
                        {
                            labelMatrix[i, j] = 5;
                        }
                    }
                    else if (i < 875)
                    {
                        if (j < 5)
                        {
                            labelMatrix[i, j] = 2;
                        }
                        else if (j < 20)
                        {
                            labelMatrix[i, j] = 6;
                        }
                        else if (j < 30)
                        {
                            labelMatrix[i, j] = 3;
                        }
                        else if (j < 1000)
                        {
                            labelMatrix[i, j] = 3;
                        }
                    }
                    else
                    {
                        if (j < 900)
                        {
                            labelMatrix[i, j] = 4;
                        }
                        else if (j < 910)
                        {
                            labelMatrix[i, j] = 1;
                        }
                        else if (j < 980)
                        {
                            labelMatrix[i, j] = 5;
                        }
                        else if (j < 1000)
                        {
                            labelMatrix[i, j] = 3;
                        }
                    }
                }
            }
        }
    }

}