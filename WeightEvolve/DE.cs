using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.Data;

namespace WeightEvolve
{
    using Accord.MachineLearning.DecisionTrees;
    using Accord.MachineLearning.DecisionTrees.Learning;
    using Accord.Math.Optimization.Losses;
    using System.IO;
    using CEPool = Dictionary<GAEncoding, double[]>;
    [Serializable]
    class DE
    {
        private shareData _data;
        private int gen = 0;
        int totalSamples = 2000;
        int popSize = 500;
        public int dimension = 2;
        public CEPool cePool = null;
        public CEPool tempPool = null;
        public int maxGen = 200000;
        int pNumOfParams = 10;
        int numOfLabel = 2;
        int numOfNNLocalSearch = 0;
        double[] lowbounds = new double[] { 0.0, 0.0 };
        double[] highbounds = new double[] { 100, 100 };
        Matrix<double> labelMatrix;
        Matrix<double> aMatrix;
        public CEPool ceDB = null;
        DataTable dt = new DataTable();
        double[] cumulativeArray;
        List<Vector<double>>[] trainingVects;

        public DE(shareData data)
        {
            _data = data;
        }

        public void LabelPrepration()
        {
            LabelMatrixCreationA();
            LabelStoreToFile();
            LabelStoreToExcel();
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

            _data.fitness = currentBests.Where(x => x.entropy == currentBests
                            .Max(y => y.entropy)).First().entropy;
            _data.generation = gen;
            _data.strbuf = "Fitness: " + _data.fitness.ToString();
            _data.update = true;

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
            DE_Initialization();
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
        private void ParameterLearning()
        {
            int[][] inputs = new int[trainingVects.Length*trainingVects[0].Count][];
            int[] outputs = new int[trainingVects.Length * trainingVects[0].Count];
            for (int i = 0; i < trainingVects.Length; i++)
            {
                for (int j = 0; j < trainingVects[i].Count; j++)
                {
                    inputs[i*trainingVects[i].Count+j] = new int[] {
                        (int)trainingVects[i][j][0],
                        (int)trainingVects[i][j][1]
                    };
                    outputs[i * trainingVects[i].Count + j] = (int)trainingVects[i][j][2] - 1;
                }
            }

            // Create an ID3 learning algorithm
            C45Learning teacher = new C45Learning();
            DecisionVariable var1 = new DecisionVariable("A", new Accord.DoubleRange(0, 100));
            DecisionVariable var2 = new DecisionVariable("B", new Accord.DoubleRange(0, 100));
            var1.Nature = DecisionVariableKind.Continuous;
            var2.Nature = DecisionVariableKind.Continuous;
            teacher.Attributes.Add(var1);
            teacher.Attributes.Add(var2);
            var tree = teacher.Learn(inputs, outputs);

            var r = tree.ToRules();
            for (int i = 0; i < r.Count; i++)
            {
                double o = r.ElementAt(i).Output;
                string name1 = r.ElementAt(i).Variables.ElementAt(0).Name;
                string name2 = r.ElementAt(i).Variables.ElementAt(1).Name;
            }
            double error = new ZeroOneLoss(outputs).Loss(tree.Decide(inputs));
            int[] predicted = tree.Decide(inputs);
            int[][] inputs2 = new int[1][];
            inputs2[0] = new int[2] { 80, 81 };
            var tmp = tree.Decide(inputs2);
        }
        private void DE_Initialization()
        {
            cePool = new CEPool();
            tempPool = new CEPool();
            ceDB = new CEPool();
            cumulativeArray = new double[popSize];
            Console.BackgroundColor = ConsoleColor.Yellow;

            LabelRetrive();
            RandomSampleOfBlocks();
            //ParameterLearning();
            aMatrixCreation();

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

        public void RandomSampleOfBlocks()
        {
            trainingVects = new List<Vector<double>>[pNumOfParams * pNumOfParams];
            Vector<double> delta = (Vector<double>.Build.Dense(highbounds)
                - Vector<double>.Build.Dense(lowbounds)).Divide(pNumOfParams);

            for (int i = 0; i < pNumOfParams; i++)
            {
                for (int j = 0; j < pNumOfParams; j++)
                {
                    Vector<double> xVect = delta.PointwiseMultiply(
                        Vector<double>.Build.Dense(new double[] { (j + 1), (i + 1) }));
                    int numOfSamples = totalSamples/(pNumOfParams*pNumOfParams);
                    List<Vector<double>> trainingVectsOfBlock = new List<Vector<double>>();
                    while (trainingVectsOfBlock.Count < numOfSamples)
                    {
                        int x1 = GlobalVar.rnd.Next((int)(xVect[0] - delta[0]), (int)xVect[0]);
                        int x2 = GlobalVar.rnd.Next((int)(xVect[1] - delta[1]), (int)xVect[1]);
                        int label = (int)labelMatrix.At(x1, x2);
                        Vector<double> rxVect = Vector<double>.Build.Dense(
                            new double[] {x1, x2, label});

                        if (!trainingVectsOfBlock.Contains(rxVect))
                        {
                            trainingVectsOfBlock.Add(rxVect);
                        }
                    }
                    trainingVects[i * pNumOfParams + j] = trainingVectsOfBlock;
                }
            }
        }

        public void aMatrixCreation()
        {
            aMatrix = Matrix<double>.Build.Dense((int)Math.Pow(pNumOfParams, 2), numOfLabel);
            for (int i = 0; i < trainingVects.Length; i++)
            {
                var xVects = trainingVects[i];
                for (int j = 0; j < xVects.Count; j++)
                {
                    aMatrix[i, (int)xVects[j][2]-1] += 1;
                }
            }

            Vector<double> sumRows = aMatrix.RowSums();
            for (int i = 0; i < sumRows.Count; i++)
            {
                var tempRowVect = aMatrix.Row(i);
                tempRowVect = tempRowVect.Divide(sumRows[i]);
                aMatrix.SetRow(i,tempRowVect);
            }
        }

        public void LabelStoreToFile()
        {
            LocalFileAccess lfa = new LocalFileAccess();
            if (File.Exists(@"C:\Users\shiya\Desktop\record\LabelMatrix"))
            {
                File.Delete(@"C:\Users\shiya\Desktop\record\LabelMatrix");
            }
            File.Create(@"C:\Users\shiya\Desktop\record\LabelMatrix").Close();
            for (int i = 0; i < labelMatrix.RowCount; i++)
            {
                List<string> rowList = labelMatrix.Row(i).ToList().Select(x => x.ToString()).ToList();
                lfa.StoreListToLinesAppend(@"C:\Users\shiya\Desktop\record\LabelMatrix",rowList);
            }

        }

        public void LabelRetrive()
        {
            if (!File.Exists(@"C:\Users\shiya\Desktop\record\LabelMatrix"))
            {
                Console.WriteLine("NOT EXISTS");
            }

            LocalFileAccess lfa = new LocalFileAccess();
            List<string> rowList = new List<string>();
            lfa.StoreLinesToList(@"C:\Users\shiya\Desktop\record\LabelMatrix", rowList);

            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));
            for (int i = 0; i < labelMatrix.RowCount; i++)
            {
                for (int j = 0; j < labelMatrix.ColumnCount; j++)
                {
                    labelMatrix[i, j] = Convert.ToDouble(rowList[i * labelMatrix.ColumnCount + j]);
                }
            }
        }
        public void LabelStoreToExcel()
        {
            DataTable labelDt = new DataTable();

            for (int i = 0; i < highbounds[0]-lowbounds[0] + 1; i++)
            {
                labelDt.Columns.Add(i.ToString(), Type.GetType("System.Double"));
            }

            for (int i = 0; i < labelMatrix.RowCount; i++)
            {
                var row = labelDt.NewRow();
                row.ItemArray = labelMatrix.Row(labelMatrix.RowCount - i - 1).Select(x=>(object)x).ToArray();
                labelDt.Rows.Add(row);
            }

            if (File.Exists(@"C:\Users\shiya\Desktop\record\label.xlsx"))
            {
                File.Delete(@"C:\Users\shiya\Desktop\record\label.xlsx");
            }
            File.Create(@"C:\Users\shiya\Desktop\record\label.xlsx").Close();
            ExcelOperation.dataTableListToExcel(new List<DataTable>() { labelDt }, false,
                @"C:\Users\shiya\Desktop\record\label.xlsx");

            _data.strbuf = "Finish Write Labels to Excel";
            _data.update = true;
        }
        //A: Each block has only one label
        public void LabelMatrixCreationA()
        {
            labelMatrix = Matrix<double>.Build.Dense((int)(highbounds[0] - lowbounds[0] + 1),
                (int)(highbounds[1] - lowbounds[1] + 1));
            Vector<double> delta = (Vector<double>.Build.Dense(highbounds)
                   - Vector<double>.Build.Dense(lowbounds)).Divide(pNumOfParams);

            double[] labelPercentArray = new double[numOfLabel];
            ManuallyAssignPercent(labelPercentArray);

            for (int i = 0; i < numOfLabel; i++)
            {
                int count = 0;

                while (count < (int)(labelPercentArray[i] * pNumOfParams * pNumOfParams))
                {
                    _data.update = true;
                    _data.strbuf = "Label: #" + (i + 1).ToString() + " Count: #" + count.ToString();
                    int iIndx = GlobalVar.rnd.Next(0,pNumOfParams);
                    int jIndx = GlobalVar.rnd.Next(0, pNumOfParams);
                    int iLabelIndx = (int)(iIndx * delta[0] + delta[0]);
                    int jLabelIndx = (int)(jIndx * delta[1] + delta[1]);

                    if (labelMatrix[iLabelIndx, jLabelIndx] == 0)
                    {
                        int maxj = jIndx == 0 ? (int)delta[1] + 1 : (int)delta[1];
                        int maxk = iIndx == 0 ? (int)delta[0] + 1 : (int)delta[0];

                        for (int j = 0; j < maxj; j++)
                        {
                            for (int k = 0; k < maxk; k++)
                            {
                                labelMatrix[iLabelIndx - k, jLabelIndx - j] = i + 1;
                            }
                        }
                        count += 1;
                    }

                }
            }
            int checkPoint = (int)labelMatrix.ColumnSums().Sum();
        }

        private void ManuallyAssignPercent(double[] labelPercentArray)
        {
            labelPercentArray[0] = 0.5;
            labelPercentArray[1] = 0.5;
            //labelPercentArray[2] = 0.01;
            //labelPercentArray[3] = 0.01;
            //labelPercentArray[4] = 0.01;
            //labelPercentArray[5] = 0.01;
            //labelPercentArray[6] = 0.01;
            //labelPercentArray[7] = 0.01;
            //labelPercentArray[8] = 0.01;
            //labelPercentArray[9] = 0.01;
            //labelPercentArray[10] = 0.01;
            //labelPercentArray[11] = 0.01;
            //labelPercentArray[12] = 0.01;
            //labelPercentArray[13] = 0.01;
            //labelPercentArray[14] = 0.01;
            //labelPercentArray[15] = 0.01;
            //labelPercentArray[16] = 0.01;
            //labelPercentArray[17] = 0.01;
            //labelPercentArray[18] = 0.01;
            //labelPercentArray[19] = 0.01;
            //labelPercentArray[20] = 0.03;
            //labelPercentArray[21] = 0.02;
            //labelPercentArray[22] = 0.05;
            //labelPercentArray[23] = 0.10;
            //labelPercentArray[24] = 0.04;
            //labelPercentArray[25] = 0.02;
            //labelPercentArray[26] = 0.03;
            //labelPercentArray[27] = 0.01;
            //labelPercentArray[28] = 0.20;
            //labelPercentArray[29] = 0.05;
            //labelPercentArray[30] = 0.02;
            //labelPercentArray[31] = 0.02;
            //labelPercentArray[32] = 0.08;
            //labelPercentArray[33] = 0.01;
            //labelPercentArray[34] = 0.02;
            //labelPercentArray[35] = 0.03;
            //labelPercentArray[36] = 0.01;
            //labelPercentArray[37] = 0.01;
            //labelPercentArray[38] = 0.03;
            //labelPercentArray[39] = 0.02;

            //var checkSum = labelPercentArray.Sum();
        }
    }
}