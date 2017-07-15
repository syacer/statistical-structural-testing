using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

// Looks like it is not a good method to use Entropy. So Sepetate each cover elment in evlovment
namespace WeightEvolve
{
    using CEPool = Dictionary<GAEncoding, double[]>;
    [Serializable]
    class GA
    {
        private int gen = 0;
        int popSize = 100;
        public int dimension = 2;
        public CEPool cePool = null;
        public CEPool tempPool = null;
        public int maxGen = 200000;
        public double pmCrossOverRate = 0.85;
        int pNumOfParams = 20;
        int pPanCombSize = 2;
        int numOfLabel = 5;
        int numOfNNLocalSearch = 0;
        double[] lowbounds = new double[] { 0.0, 0.0 };
        double[] highbounds = new double[] { 100, 100 };
        Matrix<double> labelMatrix;
        Matrix<double> aMatrix;
        public CEPool ceDB = null;
        double[] cumulativeArray;

        public GA()
        {
            LabelMatrixCreation25();
            aMatrixCreation();
            GA_Initialization();
        }
        public void PopulationGen(int min, int max)
        {
            cePool.Clear();
            for (int i = 0; i < popSize; i++)
            {
                cePool.Add(
                    Copy.DeepCopy(tempPool.ElementAt(i).Key),
                    new double[] { tempPool.ElementAt(i).Key.entropy, -1 }
                );
            }
            tempPool.Clear();
            //double a = cePool.Max(x => x.Value[0]);
            double minimumFitness = cePool.Values.Min(y => y[0]);
            if (minimumFitness < 0)
            {
                for (int i = 0; i < cePool.Count; i++)
                {
                    cePool.ElementAt(i).Value[0] += Math.Abs(minimumFitness);
                }
            }

            double sum = cePool.Values.Sum(x => x[0]);
            if (Double.IsNaN(sum))
            {
                Console.WriteLine("N/A error of sum");
            }
            for (int i = 0; i < max - min + 1; i++)
            {
                cePool.ElementAt(i).Value[0] = cePool.ElementAt(i).Value[0] / sum;
            }
            Array.Clear(cumulativeArray, 0, cumulativeArray.Length);
            double cumulation = 0.0;
            for (int i = 0; i < popSize; i++)
            {
                cumulation = cumulation + cePool.ElementAt(i).Value[0];
                cumulativeArray[i] = cumulation;
            }

        }

        public async void GA_FitnessEvaluation(int[] token)
        {
            List<Task> lTasks = new List<Task>();

            // Test inputs Generation
            // Fitness Evaluation
            for (int k = 0; k < tempPool.Count;)
            {
                // Single Thread
                //ga.CalEstFitness(testSetSize, numOfLabel, labelMatrix, lowbounds, highbounds);

                lTasks.Add(Task.Run(() =>
                {
                    GlobalVar.mutex_K.WaitOne();
                    // Console.WriteLine(k);
                    GAEncoding ga = tempPool.ElementAt(k).Key;
                    int id = k;
                    k = k + 1;
                    GlobalVar.mutex_K.ReleaseMutex();
                    //ga.CalEstFitness(testSetSize, numOfLabel, labelMatrix, lowbounds, highbounds);
                    //ga.TrueFitnessCal(numOfLabel, labelMatrix);
                    ga.FitnessCal(aMatrix);
                    //Console.WriteLine("{0} job finish",id);
                }));
                if (lTasks.Count == 20 || (tempPool.Count - k - 1 < 20))  //8,8
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

            Console.WriteLine("Generation: {0}", gen);
            List<GAEncoding> currentBests = cePool.Where(x => x.Key.entropy == cePool.Max(y => y.Key.entropy))
                .Select(x => x.Key).ToList();
            //LocalFileAccess lfa = new LocalFileAccess();
            //foreach (var best in currentBests)
            //{
            //    lfa.StoreListToLinesAppend(@"C:\Users\Yang Shi\Desktop\record", Print_Solution(best));
            //}
            for (int i = 0; i < currentBests.Count; i++)
            {
                Console.WriteLine("Solution: {1}, Entropy: {0}", currentBests[i].entropy, i);
            }

            var solutions = Print_Solution(currentBests[0]);
        }

        public void UpdateDatabase()
        {
            for (int i = 0; i < cePool.Count; i++)
            {
                if (ceDB.Where(x => x.Key.id == cePool.ElementAt(i).Key.id).Count() == 0)
                {
                    ceDB.Add(Copy.DeepCopy(cePool.ElementAt(i).Key), new double[] { 1 });
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
        public void GA_Start()
        {
            GA_Initialization();
            int[] token = new int[1];
            Console.WriteLine("In Fitness Evaluation...");
            GA_FitnessEvaluation(token);
            while (token[0] == 0) ;
            token[0] = 0;
            PopulationGen(0, popSize - 1); //tmpPool -> cePool
            UpdateRecordAndDisplay();


            // Start MOGA
            while (gen < maxGen)
            {
                gen = gen + 1;
                Reproduction(token);
                while (token[0] == 0) ;
                token[0] = 0;
                GA_FitnessEvaluation(token);
                if (cePool.Count < 100)
                {
                    Console.WriteLine("EEE");
                }
                while (token[0] == 0) ;
                token[0] = 0;
                PopulationGen(0, popSize - 1);
                if (cePool.Count < 100)
                {
                    Console.WriteLine("VVV");
                }
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

        private void GA_Initialization()
        {
            cePool = new CEPool();
            tempPool = new CEPool();
            ceDB = new CEPool();
            cumulativeArray = new double[popSize];
            Console.BackgroundColor = ConsoleColor.Yellow;
            for (int i = 0; i < popSize; i++)
            {
                var newSolution = new GAEncoding(pNumOfParams, dimension, lowbounds, highbounds);
                newSolution.FixInputs(lowbounds, highbounds);
                tempPool.Add(newSolution, new double[] { -1, -1 });
            }
        }

        public async void Reproduction(int[] token)
        {
            int maxNumOfTasks = 20;
            GAEncoding child = new GAEncoding(pNumOfParams, dimension, lowbounds, highbounds);
            int k = 0;
            var rankOneSol = cePool.Where(x => x.Value[0] == cePool.Max(y => y.Value[0])).First();
            tempPool.Add(Copy.DeepCopy(rankOneSol.Key), rankOneSol.Value);

            List<Task> lTasks = new List<Task>();

            if (cePool.Count < 100)
            {
                Console.WriteLine("RRRRRR");
            }

            while (tempPool.Count < popSize)
            {
                lTasks.Add(Task.Run(() =>
                {
                    int[] selectedList = FitnessPropotionateSampling(pPanCombSize);
                    child = Copy.DeepCopy(cePool.ElementAt(selectedList[0]).Key);
                    if (GlobalVar.rnd.NextDouble() <= pmCrossOverRate)
                    {
                        if (cePool.Count < 100)
                        {
                            Console.WriteLine();
                        }
                        if (GlobalVar.rnd.NextDouble() <= -1)
                        {
                            child = cePool.ElementAt(0).Key.PanmicticDiscreteRecomb(cePool, selectedList);
                        }
                        else
                        {
                            child = cePool.ElementAt(0).Key.IntermidinateRecomb(cePool, selectedList);
                        }
                        child = child.RealVectMutation();
                        GlobalVar.mutex_K.WaitOne();
                        if (!tempPool.ContainsKey(child))
                        {

                            tempPool.Add(child, new double[] { -1, -1 });
                            k = k + 1;
                        }
                        GlobalVar.mutex_K.ReleaseMutex();
                    }
                }));

                if (lTasks.Count == maxNumOfTasks)
                {
                    for (int i = 0; i < lTasks.Count; i++)
                    {
                        await lTasks[i];
                    }
                    lTasks.Clear();
                    int remainTasks = cePool.Count - k;
                    if (remainTasks <= maxNumOfTasks)
                    {
                        maxNumOfTasks = remainTasks;
                    }
                }
            }

            token[0] = 1;
        }

        private int[] FitnessPropotionateSampling(int size)
        {
            int[] result = new int[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = -1;
            }

            int timerCount = 0;
            int tmpCnt = 0;
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
                if (timerCount == 1000)
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
        public void LabelMatrixCreation()
        {
            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));
            aMatrix = Matrix<double>.Build.Dense((int)Math.Pow(pNumOfParams, 2), numOfLabel);

            for (int i = 0; i < (int)(highbounds[0] - lowbounds[0] + 1); i++)
            {
                for (int j = 0; j < (int)(highbounds[1] - lowbounds[1] + 1); j++)
                {
                    if (i <= 2)
                    {
                        if (j <= 1)
                        {
                            labelMatrix[i, j] = 1;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 4)
                        {
                            labelMatrix[i, j] = 8;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 8)
                        {
                            labelMatrix[i, j] = 3;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 40)
                        {
                            labelMatrix[i, j] = 8;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 50)
                        {
                            labelMatrix[i, j] = 9;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                    }
                    else if (i <= 4)
                    {
                        if (j <= 1)
                        {
                            labelMatrix[i, j] = 5;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 4)
                        {
                            labelMatrix[i, j] = 4;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 8)
                        {
                            labelMatrix[i, j] = 2;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 40)
                        {
                            labelMatrix[i, j] = 2;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 50)
                        {
                            labelMatrix[i, j] = 10;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                    }
                    else if (i <= 49)
                    {
                        if (j <= 1)
                        {
                            labelMatrix[i, j] = 6;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 4)
                        {
                            labelMatrix[i, j] = 3;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 8)
                        {
                            labelMatrix[i, j] = 2;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 40)
                        {
                            labelMatrix[i, j] = 2;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 50)
                        {
                            labelMatrix[i, j] = 9;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                    }
                    else if (i <= 50)
                    {
                        if (j <= 1)
                        {
                            labelMatrix[i, j] = 7;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 4)
                        {
                            labelMatrix[i, j] = 6;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 8)
                        {
                            labelMatrix[i, j] = 7;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 40)
                        {
                            labelMatrix[i, j] = 1;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                        else if (j <= 50)
                        {
                            labelMatrix[i, j] = 10;
                            int ti = i % pNumOfParams;
                            int tj = j % pNumOfParams;
                            int aIndex = tj * pNumOfParams + ti;
                            aMatrix[aIndex, (int)labelMatrix[i, j] - 1] += 1;
                        }
                    }
                }
            }
            aMatrix = aMatrix.NormalizeRows(2.0);
        }
        public void aMatrixCreation()
        {
            aMatrix = Matrix<double>.Build.Dense((int)Math.Pow(pNumOfParams, 2), numOfLabel);

            Vector<double> delta = (Vector<double>.Build.Dense(highbounds)
                - Vector<double>.Build.Dense(lowbounds)).Divide(pNumOfParams);

            for (int i = 0; i < pNumOfParams; i++)
            {
                for (int j = 0; j < pNumOfParams; j++)
                {
                    Vector<double> xVect = delta.PointwiseMultiply(
                        Vector<double>.Build.Dense(new double[] {(j+1),(i+1)}));
                    var indToLabelM = xVect.PointwiseRound();
                    int label = (int)labelMatrix.At((int)indToLabelM[0], (int)indToLabelM[1]);
                    aMatrix[i* pNumOfParams + j, label-1] += 1;
                }
            }
            //aMatrix = aMatrix.NormalizeRows(2.0);
        }
        public void LabelMatrixCreation25()
        {
            labelMatrix = Matrix<double>.Build.Dense((int) (highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));


            for (int i = 0; i < (int)(highbounds[0] - lowbounds[0] + 1); i++)
            {
                for (int j = 0; j < (int)(highbounds[1] - lowbounds[1] + 1); j++)
                {
                    labelMatrix[i, j] = 1;
                }
            }
            labelMatrix[1, 1] = 1;
            labelMatrix[1, 2] = 1;
            labelMatrix[1, 3] = 1;
            labelMatrix[1, 4] = 1;
            labelMatrix[1, 5] = 1;
            labelMatrix[2, 1] = 1;
            labelMatrix[2, 2] = 1;
            labelMatrix[2, 3] = 4;
            labelMatrix[2, 4] = 4;
            labelMatrix[2, 5] = 4;
            labelMatrix[3, 1] = 1;
            labelMatrix[3, 2] = 1;
            labelMatrix[3, 3] = 1;
            labelMatrix[30, 30] = 3;
            labelMatrix[3, 5] = 1;
            labelMatrix[4, 1] = 1;
            labelMatrix[4, 2] = 1;
            labelMatrix[4, 3] = 1;
            labelMatrix[4, 4] = 1;
            labelMatrix[4, 5] = 1;
            labelMatrix[5, 1] = 1;
            labelMatrix[5, 2] = 2;
            labelMatrix[5, 3] = 1;
            labelMatrix[5, 4] = 1;
            labelMatrix[5, 5] = 5;
        }
    }
}