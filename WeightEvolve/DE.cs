using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.Data;

namespace WeightEvolve
{
    using CEPool = Dictionary<GAEncoding, double[]>;
    [Serializable]
    class DE
    {
        private int gen = 0;
        int popSize = 500;
        public int dimension = 2;
        public CEPool cePool = null;
        public CEPool tempPool = null;
        public int maxGen = 200000;
        int pNumOfParams = 10;
        int numOfLabel = 5;
        int numOfNNLocalSearch = 0;
        double[] lowbounds = new double[] { 0.0, 0.0 };
        double[] highbounds = new double[] { 100, 100 };
        Matrix<double> labelMatrix;
        Matrix<double> aMatrix;
        public CEPool ceDB = null;
        DataTable dt = new DataTable();
        double[] cumulativeArray;

        public DE()
        {
            LabelMatrixCreation25();
            aMatrixCreation();
            GA_Initialization();
        }
        public void PopulationGen(int min, int max)
        {
            cePool.Clear();

            //List<int> nonDominatedList = NonDomininatedSolution(tempPool);
            tempPool.Where(x => x.Key.entropy == tempPool.Max(y => y.Key.entropy)).First().Key.rank = 0;
            for (int i = 0; i < popSize; i++)
            {
                cePool.Add(
                    Copy.DeepCopy(tempPool.ElementAt(i).Key),
                    new double[] { tempPool.ElementAt(i).Key.entropy,
                            tempPool.ElementAt(i).Key.diversity }
                );
            }
            tempPool.Clear();
        }

        public List<int> NonDomininatedSolution(CEPool pool)
        {
            int[] dominatedList = new int[pool.Count];
            List<int> nonDominatedList = new List<int> ();

            Array.Clear(dominatedList,0,dominatedList.Length-1);
            for (int i = 0; i < dominatedList.Length; i++)
            {
                for (int j = 0; j < dominatedList.Length; j++)
                {
                    if (pool.ElementAt(i).Key.entropy == pool.ElementAt(j).Key.entropy
                        && pool.ElementAt(i).Key.diversity ==
                        pool.ElementAt(j).Key.diversity)
                    {
                        continue;
                    }
                    if (pool.ElementAt(i).Key.entropy <= pool.ElementAt(j).Key.entropy
                        && pool.ElementAt(i).Key.diversity <=
                        pool.ElementAt(j).Key.diversity)
                    {
                        dominatedList[i] += 1;
                    }
                }
            }

            for (int i = 0; i < dominatedList.Length; i++)
            {
                if (dominatedList[i] == 0)
                {
                    nonDominatedList.Add(i);
                }
            }

            return nonDominatedList;
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

            for (int i = 0; i < solution.weightsVect.Count; i++)
            {
                weight = weight + solution.weightsVect[i] + " ";
            }
            weight.Remove(weight.Length - 1, 1);

            return new List<string>() { theta, weight };
        }

        void UpdateRecordAndDisplay()
        {


            Console.WriteLine("Generation: {0}", gen);
            List<GAEncoding> currentBests = cePool.Where(x => x.Key.rank == 0)
                .Select(x => x.Key).ToList();

            //for (int i = 0; i < currentBests.Count; i++)
            //{
            //    Console.WriteLine("Solution: {1}, Entropy: {0}", currentBests[i].entropy, i);
            //    Console.WriteLine("Solution: {1}, Diversity: {0}", currentBests[i].diversity, i);
            //}
            Console.WriteLine("Max_Entropy: {0}", currentBests.Where(x=>x.entropy == currentBests
                            .Max(y=>y.entropy)).First().entropy);
            Console.WriteLine("Max_Diversity: {0}", currentBests.Where(x => x.diversity == currentBests
                            .Max(y => y.diversity)).First().diversity);
            var a = currentBests[0].weightsVect.Sum();
            var solutions = Print_Solution(currentBests[0]);

            object[] temp = new object[pNumOfParams * pNumOfParams];
            var newRow = dt.NewRow();
            for (int i = 0; i < pNumOfParams * pNumOfParams; i++)
            {
                temp[i] = currentBests[0].weightsVect.ToArray()[i];
            }
            newRow.ItemArray = temp;
            dt.Rows.Add(newRow);

            int fake = 0;
            if (fake > 0)
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dt }, false,
                    @"C:\Users\shiya\Desktop\record\a.xlsx");
            }
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
        public void DE_Start()
        {
            int[] token = new int[1];
            Console.WriteLine("In Fitness Evaluation...");
            GA_FitnessEvaluation(token);
            while (token[0] == 0);
            token[0] = 0;
            PopulationGen(0, popSize - 1); //tmpPool -> cePool
            UpdateRecordAndDisplay();

            while (gen < maxGen)
            {
                gen = gen + 1;
                Reproduction(token);
                while (token[0] == 0) ;
                token[0] = 0;
                PopulationGen(0, popSize - 1);
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
            for (int i = 0; i < pNumOfParams * pNumOfParams; i++)
            {
                dt.Columns.Add(i.ToString(), Type.GetType("System.Double"));
            }
        }

        public async void Reproduction(int[] token)
        {
            int maxNumOfTasks = 20;
            int k = 0;

            List<Task> lTasks = new List<Task>();

            while (tempPool.Count < popSize)
            {
                lTasks.Add(Task.Run(() =>
                {
                    GlobalVar.mutex_K.WaitOne();
                    int index = k;
                    k = k + 1;
                    GlobalVar.mutex_K.ReleaseMutex();
                    GAEncoding child = Copy.DeepCopy(cePool.ElementAt(index).Key);
                    int[] selectedList = DifferentialSelection(index);
                    child = child.DifferentialCrossover(cePool,selectedList);

                    child.FitnessCal(aMatrix);
                    if (child.entropy > cePool.ElementAt(index).Key.entropy
                        /*&& child.diversity > cePool.ElementAt(index).Key.diversity*/)
                    {
                        GlobalVar.mutex_K.WaitOne();
                        tempPool.Add(child, new double[] { child.entropy, -1 });
                        GlobalVar.mutex_K.ReleaseMutex();
                    }
                    else
                    {
                        GlobalVar.mutex_K.WaitOne();
                        tempPool.Add(cePool.ElementAt(index).Key,
                            new double[] { cePool.ElementAt(index).Key.entropy, -1 });
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

        //Select 3 individuals that is distinct from each other, and from index
        private int[] DifferentialSelection(int index)
        {
            int success = 0;
            int a=-1, b=-1, c=-1;
            while (success == 0)
            {
                a = GlobalVar.rnd.Next(0, popSize-1);
                b = GlobalVar.rnd.Next(0, popSize - 1);
                c = GlobalVar.rnd.Next(0, popSize - 1);

                if (!(index == a || index == b
                    || index == c || a == b
                    || a == c || b == c))
                {
                    success = 1;
                }
            }
            return new int[] {a,b,c };
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
                        Vector<double>.Build.Dense(new double[] { (j + 1), (i + 1) }));
                    var indToLabelM = xVect.PointwiseRound();
                    int label = (int)labelMatrix.At((int)indToLabelM[0], (int)indToLabelM[1]);
                    aMatrix[i * pNumOfParams + j, label - 1] += 1;
                }
            }
            var a = aMatrix.ColumnSums();

        }
        public void LabelMatrixCreation25()
        {
            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
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
            labelMatrix[10, 10] = 2;
            labelMatrix[10, 20] = 2;
            labelMatrix[10, 30] = 2;
            labelMatrix[10, 40] = 2;
            //labelMatrix[20, 10] = 3;
            labelMatrix[20, 20] = 3;
            //labelMatrix[20, 30] = 3;
            //labelMatrix[20, 40] = 3;
            //labelMatrix[30, 10] = 4;
            //labelMatrix[30, 20] = 4;
            labelMatrix[30, 30] = 4;
            //labelMatrix[30, 40] = 4;
            //labelMatrix[40, 10] = 5;
            //labelMatrix[40, 20] = 5;
            //labelMatrix[40, 30] = 5;
            labelMatrix[40, 40] = 5;
            //labelMatrix[50, 50] = 1;
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