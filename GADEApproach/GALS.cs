using DeterministicApproach_GA;
using MathNet.Numerics.LinearAlgebra;
using StatisticalApproach_GA;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Accord.Math;

namespace GADEApproach
{
    [Serializable]
    class Pair<T0,T1,T2>
    {
        public T0 Item0 { set; get; }
        public T1 Item1 { set; get; }
        public T2 Item2 { set; get; }

        //public T2 Item2
        //{
        //    set
        //    {
        //        if (value.GetType().Name != typeof(double[]).Name)
        //        {
        //            Item2 = value;
        //        }
        //        else
        //        {
        //            var temp = (double[])(object)value;
        //            if (temp.Sum() == 0)
        //            {
        //                Item2 = value;
        //            }
        //            if (temp.Sum() < 0.9998 || temp.Sum() > 1.0002)
        //            {
        //                string message = "Sum Of Probabilities for " + Item0 +
        //                    " is not equal to one， " + temp.Sum();
        //                throw new Exception(message);
        //            }
        //            Item2 = value;
        //        }
        //    }

        //    get
        //    {
        //        return Item2;
        //    }
        //}
    }
    [Serializable]
    class GALS
    {
        int generation = 0;
        double[] expTriProb;
        //Multi-Threading Setup
        int numberOfConcurrentTasks = 20;
        //Bins Setup
        readonly int numOfVars = 2;
        readonly int numOfMinIntervalX = 20;
        readonly int numOfMinIntervalY = 20;
        readonly int numOfSamplesInBin = 30;
        readonly int numOfLabels = 3;
        Pair<int, int, double[]>[] bins; //index, setIndex, triggering probabilities

        //GA Setup
        int maxGen = 5000;
        int populationSize = 100;
        double crRate = 0.8;
        double mtRate = 0.1;
        double mtPropotion = 0.1;
        Pair<int, double, Pair<int, int, double[]>[]>[] pool; //index, fitness, bins
        double[][] coverPointSetProb;

        public GALS()
        {
        }

        public class record
        {
            public double[] fitnessGen;
            public double[] goodnessOfFitGen;
            public solution bestSolution;
        }
        public class solution
        {
            public int[][] inputsinSets; // [1][2] : the second inputs in set 1
            public double[] setProbabilities;
            public long totalRunTime;
            public double[] trueTriProbs;
        }

        async public void ExperimentsA()
        {
            int count = 0;
            int[] numOfLabelArray = new int[] { 2, 4, 5, 8, 10, 16, 20 };
            double[][] entropy = new double[6][];
            entropy[0] = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            entropy[1] = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            entropy[2] = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            entropy[3] = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            entropy[4] = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            entropy[5] = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            entropy[6] = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            string fileName;

            for (int i = 0; i < numOfLabelArray.Count(); i++)
            {
                for (int j = 0; j < entropy.Count(); j++)
                {
                    count = 0;
                    fileName = numOfLabelArray[i].ToString() + "_" + entropy[j].ToString() + "_";
                    record[] records30Runs = new record[30];
                    while (count < 30)
                    {
                        records30Runs[count] = new record();
                        records30Runs[count].fitnessGen = new double[maxGen];
                        records30Runs[count].goodnessOfFitGen = new double[maxGen];

                        Task t = new Task(() =>
                        {
                            SyntheticSUT.SUTB(numOfLabelArray[i], entropy[i][j]);
                            ExpectedTriggeringProbSetUp();
                            BinsInitialization();
                            GAInitialization();
                            AlgorithmStart(records30Runs[count]);
                        });
                        await t;
                    }

                    // Data Print to Excel
                    DataTable dataTable = new DataTable();

                    for (int k = 0; k < 90; k++)
                    {
                        dataTable.Columns.Add(i.ToString(), Type.GetType("System.Double"));
                    }

                    for (int u = 0; u < records30Runs[0].goodnessOfFitGen.Count(); u++)
                    {
                        object[] rowData = new object[60];

                        for (int k = 0; k < 30; k++)
                        {   // Remember, calculating true or goodnessOfFit can't be used in
                            // Total Running Time calculation
                            rowData[k * 2 + 0] = (object)records30Runs[k].goodnessOfFitGen[u];
                            rowData[k * 2 + 1] = (object)records30Runs[k].fitnessGen[u];
                        }
                        var row = dataTable.NewRow();
                        row.ItemArray = rowData;
                        dataTable.Rows.Add(row);
                        string filePath = @"C:\Users\shiya\Desktop\record\Data_" + fileName + ".xlsx";
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        File.Create(filePath).Close();
                        ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, false, filePath);
                    }

                    //Write solution to Excel
                    //Input label mapping
                    for (int k = 0; k < 30; k++)
                    {
                        DataTable dataTable2 = new DataTable();
                        for (int u = 0; u < numOfLabels; u++)
                        {
                            dataTable2.Columns.Add(i.ToString(), Type.GetType("System.Double"));
                        }
                        for (int u = 0; u < records30Runs[k].bestSolution.inputsinSets.Length; u++)
                        {
                            object[] rowData2 = new object[3];
                            rowData2[0] = (object)(u % 2);
                            rowData2[1] = (object)(u / 2);
                            rowData2[2] = (object)records30Runs[k].bestSolution.inputsinSets[u % 2][u / 2];
                            var row2 = dataTable2.NewRow();
                            row2.ItemArray = rowData2;
                            dataTable2.Rows.Add(row2);
                        }

                        //Set Probabilities
                        var rowData3 = records30Runs[k].bestSolution
                            .setProbabilities.Select(x => (object)x).ToArray();
                        var row3 = dataTable2.NewRow();
                        row3.ItemArray = rowData3;
                        dataTable2.Rows.Add(row3);

                        //True Triggering Probabilities;
                        var rowData4 = records30Runs[k].bestSolution
                            .trueTriProbs.Select(x => (object)x).ToArray();
                        var row4 = dataTable2.NewRow();
                        row4.ItemArray = rowData4;
                        dataTable2.Rows.Add(row4);

                        //Total RunTime
                        object[] rowData5 = new object[1];
                        rowData5[0] = (object)records30Runs[k].bestSolution.totalRunTime;
                        var row5 = dataTable2.NewRow();
                        row5.ItemArray = rowData5;
                        dataTable2.Rows.Add(row5);

                        string filePath = @"C:\Users\shiya\Desktop\record\Solution_" + fileName + ".xlsx";

                        if (File.Exists(filePath))
                        {
                            File.Create(filePath).Close();
                        }
                        ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable2 }, false, filePath);
                    }
                    SyntheticSUT.InputDomainToExcel(@"C:\Users\shiya\Desktop\record\Label_" + fileName + ".xlsx");
                }
            }
        }
        private void ExpectedTriggeringProbSetUp()
        {
            expTriProb = new double[numOfLabels];
            for (int i = 0; i < numOfLabels; i++)
            {
                expTriProb[i] = 1.0 / numOfLabels;
            }
        }
        public double GoodnessOfFit(Pair<int, double, Pair<int, int, double[]>[]> solution)
        {
            Matrix<double> Amatrix = Matrix<double>.Build.Dense(numOfLabels, numOfLabels - 1);
            double[] lastCoverElementSet = new double[numOfLabels];
            Vector<double> bVector = Vector<double>.Build.Dense(expTriProb);

            for (int i = 0; i < numOfLabels; i++)
            {
                if (solution.Item2.Where(x => x.Item1 == numOfLabels).Count() == 0)
                {
                    Console.WriteLine();
                }
                lastCoverElementSet[i] = solution.Item2.Where(x => x.Item1 == numOfLabels).Sum(x => x.Item2[i])
                    / solution.Item2.Where(x => x.Item1 == numOfLabels).Count();
            }
            bVector = bVector.Subtract(Vector<double>.Build.Dense(lastCoverElementSet));

            for (int i = 0; i < numOfLabels; i++)
            {

                for (int j = 0; j < numOfLabels - 1; j++)
                {
                    if (solution.Item2.Where(x => x.Item1 == j + 1).Count() == 0)
                    {
                        Console.WriteLine();
                    }
                    Amatrix[i, j] = solution.Item2.Where(x => x.Item1 == j + 1).Sum(x => x.Item2[i]);
                    Amatrix[i, j] /= solution.Item2.Where(x => x.Item1 == j + 1).Count();
                    Amatrix[i, j] -= lastCoverElementSet[i];
                }
            }

            double error = 0;
            for (int row = 0; row < numOfLabels; row++)
            {
                error += Math.Pow((Amatrix.Row(row).ToRowMatrix()
                    .Multiply(Vector<double>.Build.Dense(coverPointSetProb[solution.Item0])
                    .SubVector(0, coverPointSetProb[solution.Item0].Length - 1))[0] - bVector[row]), 2);
            }

            int beta = 1;
            double[] tempProbs = Copy.DeepCopy(coverPointSetProb[solution.Item0]);
            tempProbs = tempProbs.Select(x =>
            {
                double y = x * -1 - 0;
                return Math.Pow((Math.Max(0, y)), beta);
            }).ToArray();
            double totalInfeasiability = tempProbs.Sum() / numOfLabels;
            double normalizedCost = error / numOfLabels;
            double goodnessOfFit = Math.Sqrt(Math.Pow(normalizedCost, 2) + Math.Pow(totalInfeasiability, 2));

            return goodnessOfFit;
        }

        public void AlgorithmStart(record record)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int[] token = new int[1];
            FitnessEvaluation(token);
            while (token[0] == 0) ;
            token[0] = 0;

            while (generation < maxGen)
            {
                Reproduction();
                FitnessEvaluation(token);
                while (token[0] == 0) ;
                token[0] = 0;

                // Start Recording
                watch.Stop();
                var orderedSolutions = pool.OrderByDescending(x => x.Item1);
                int index = -1;
                Pair<int, double, Pair<int, int, double[]>[]> bestSolution = null;
                double[] probabilityset = null;

                for (int i = 0; i < populationSize; i++)
                {
                    index = orderedSolutions.ElementAt(i).Item0;
                    probabilityset = coverPointSetProb[index];
                    if (probabilityset.All(x => x >= 0))
                    {
                        bestSolution = orderedSolutions.ElementAt(i);
                        break;
                    }
                }

                Console.WriteLine("Gen #{0} Fitness: {1}", generation, 1.0 / pool.Max(x => x.Item1));

                 if (bestSolution == null)
                {
                    Console.WriteLine("No Feasiable Solution, # Of negative Values {0}",
                        coverPointSetProb[orderedSolutions
                        .First().Item0].Sum(x =>
                    {
                        if (x < 0)
                        {
                            return Math.Abs(x);
                        }
                        else
                        {
                            return 0;
                        }
                    }));
                }
                else
                {
                    double[] estTriProbabilities = new double[numOfLabels];
                    for (int i = 0; i < numOfLabels; i++)
                    {
                        double[] triProbsCoverSet = new double[numOfLabels];
                        for (int j = 0; j < numOfLabels; j++)
                        {
                            triProbsCoverSet[j] = bestSolution.Item2.Where(x => x.Item1 == j + 1).Sum(x => x.Item2[i]);
                            triProbsCoverSet[j] /= bestSolution.Item2.Where(x => x.Item1 == j + 1).Count();
                        }
                        estTriProbabilities[i] = Vector<double>.Build.Dense(triProbsCoverSet).ToRowMatrix().
                            Multiply(Vector<double>.Build.Dense(coverPointSetProb[index]))[0];
                    }
                    Console.WriteLine("Estimated Triggering Probabilities {0}", estTriProbabilities.Min());
                }
                record.fitnessGen[generation] = 1.0 / orderedSolutions.First().Item1;
                // Goodness Of Fit Of Best Solution
                record.goodnessOfFitGen[generation] = GoodnessOfFit(orderedSolutions.First());
                
                //// Dump Best Solution so far to File
                //var AmatrixOfBestSoution = AmatrixCalculation(orderedSolutions.First().Item0);
                //string aMatrixStr = AmatrixOfBestSoution.ToString();
                //string centerPoint = null;
                //foreach (double d in coverPointSetProb[orderedSolutions.First().Item0])
                //{
                //    centerPoint += d.ToString() + " ";
                //}
                
                //LocalFileAccess lfa = new LocalFileAccess();
                //lfa.StoreListToLinesAppend(@"C:\Users\shiya\Desktop\record\amatrix.txt",
                //    new List<string>() {generation.ToString(),aMatrixStr, centerPoint});

                watch.Start();
                generation += 1;
            }
            watch.Stop();

            var rankedOneSolution = pool.OrderByDescending(x => x.Item1).First();
            record.bestSolution.inputsinSets = MapTestInputsToSets(rankedOneSolution);
            var totalRuningTime = watch.ElapsedMilliseconds;
            record.bestSolution.totalRunTime = totalRuningTime;

            // Below records needs to run constrained quadratic formula solver, No need for Experiment A
            //Fake True Triggering Probability, No need For Experiment A
            for (int i = 0; i < numOfLabels; i++)
            {
                record.bestSolution.trueTriProbs[i] = -1;
            }
            // Invalid set probabilities, No need For Experiment A
            record.bestSolution.setProbabilities = Copy.DeepCopy(
                coverPointSetProb[rankedOneSolution.Item0]);
        }

        public int[][] MapTestInputsToSets(Pair<int,double,Pair<int,int,double[]>[]> solution)
        {
            int[][] inputsMapping = new int[(int)(SyntheticSUT.highbounds[0] - SyntheticSUT.lowbounds[0] + 1)][];
            for (int i = 0; i < (int)(SyntheticSUT.highbounds[0] - SyntheticSUT.lowbounds[0] + 1); i++)
            {
                inputsMapping[i] = new int[(int)(SyntheticSUT.highbounds[1] - SyntheticSUT.lowbounds[1] + 1)];
            }
            int totalNumberOfBins = numOfMinIntervalX * numOfMinIntervalY;
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                Tuple<int, int> indexs = Calxyindex(i);
                double deltaX = (SyntheticSUT.highbounds[0] - SyntheticSUT.lowbounds[0] + 1) / numOfMinIntervalX;
                double deltaY = (SyntheticSUT.highbounds[1] - SyntheticSUT.lowbounds[1] + 1) / numOfMinIntervalY;
                int lowX = (int)deltaX * indexs.Item1;
                int lowY = (int)deltaY * indexs.Item2;
                for (int x = lowX; x < lowX + deltaX; x++)
                {
                    for (int y = lowY; y < lowY + deltaY; y++)
                    {
                        inputsMapping[x][y] = solution.Item2[i].Item1;
                    }
                }
            }

            return inputsMapping;
        }
        void GAInitialization()
        {
            pool = new Pair<int, double, Pair<int, int, double[]>[]>[populationSize];
            coverPointSetProb = new double[populationSize][];

            for (int i = 0; i < populationSize; i++)
            {
                pool[i] = new Pair<int, double, Pair<int, int, double[]>[]>();
                pool[i].Item0 = i;
                pool[i].Item1 = -1;
                pool[i].Item2 = Copy.DeepCopy(bins);
                for (int j = 0; j < pool[i].Item2.Length; j++)
                {
                    pool[i].Item2[j].Item1 = GlobalVar.rnd.Next(1,numOfLabels+1);
                }
                SolutionRepair(pool[i].Item2);
            }

        }
        void BinsInitialization()
        {
            // Bins Initialization
            int totalNumberOfBins = numOfMinIntervalX * numOfMinIntervalY;
            bins = new Pair<int, int, double[]>[totalNumberOfBins];
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                bins[i] = new Pair<int, int, double[]>();
                bins[i].Item0 = i;
                bins[i].Item1 = -1;
                TriProbabilitiesInBinSetup(bins[i],i);
            }
        }
        void TriProbabilitiesInBinSetup(Pair<int, int, double[]> bin, int binIndex)
        {
            double[] triggeringProbilities = new double[numOfLabels];
            var labelMatrix = SyntheticSUT.labelMatrix;
            Tuple<int,int> indexs = Calxyindex(binIndex);
            double deltaX = (SyntheticSUT.highbounds[0] - SyntheticSUT.lowbounds[0]+1) / numOfMinIntervalX;
            double deltaY = (SyntheticSUT.highbounds[1] - SyntheticSUT.lowbounds[1]+1) / numOfMinIntervalY;
            int lowX = (int)deltaX * indexs.Item1;
            int lowY = (int)deltaY * indexs.Item2;

#if true
            // Start Section: Full Sampling
            int totalNumOfinputsInBin = (int)deltaX * (int)deltaY;
            for (int i = lowX; i < lowX + deltaX; i++)
            {
                for (int j = lowY; j < lowY + deltaY; j++)
                {
                    triggeringProbilities[(int)labelMatrix[i, j]-1] += 1;
                }
            }
            triggeringProbilities = triggeringProbilities
                .Select(x => x / totalNumOfinputsInBin).ToArray();
            // End Section: Full Sampling
#else
            // Start Section: Random Sampling
            List<Tuple<int, int>> samples = new List<Tuple<int, int>>();
            while (samples.Count < numOfSamplesInBin)
            {
                int xIndex = GlobalVar.rnd.Next((int)lowX, (int)(lowX+deltaX+1));
                int yIndex = GlobalVar.rnd.Next((int)lowX, (int)(lowX + deltaX + 1));
                if (samples.Where(x => x.Item1 == xIndex
                    && x.Item2 == yIndex).Count() == 0)
                {
                    samples.Add(new Tuple<int, int>(xIndex, yIndex));
                    int labelIndex = (int)labelMatrix[xIndex, yIndex]-1;
                    triggeringProbilities[labelIndex] += 1; 
                }
            }
            triggeringProbilities = triggeringProbilities
                .Select(x => x / numOfSamplesInBin).ToArray();
#endif
            bin.Item2 = triggeringProbilities;
        }
        public double SelfAdaptivePenaltyFunction(double[] probabilities,double cost)
        {
            int beta = 1;
            double[] tempProbs = Copy.DeepCopy(probabilities);
            tempProbs = tempProbs.Select(x =>
            {
                double y = x * -1 - 0;
                return Math.Pow((Math.Max(0, y)),beta);
            }).ToArray();
            double totalInfeasiability = tempProbs.Sum() > numOfLabels ? 100 : tempProbs.Sum() / numOfLabels;
            double normalizedCost = cost / numOfLabels;
            bool infeasiablePool = coverPointSetProb.All(x => x.All(y => y < 0) == true) == true ? true : false;
            double distance = infeasiablePool == true ? totalInfeasiability
                : Math.Sqrt(Math.Pow(normalizedCost, 2) + Math.Pow(totalInfeasiability, 2));
            double X = infeasiablePool == true ? 0 : totalInfeasiability;
            double Y = probabilities.All(x => x > 0) ? 0 : normalizedCost;
            double proportionOfFeaisableSol = coverPointSetProb
                .Count(x => x.All(y => y > 0))*1.0/populationSize;
            double adjustedFitness = distance + (1- proportionOfFeaisableSol) *X 
                + proportionOfFeaisableSol * Y;

            return adjustedFitness;
        }

        public Matrix<double> AmatrixCalculation(int id)
        {

            Matrix<double> Amatrix = Matrix<double>.Build.Dense(numOfLabels, numOfLabels);

            for (int i = 0; i < numOfLabels; i++)
            {
                for (int j = 0; j < numOfLabels; j++)
                {
                    Amatrix[i, j] = pool[id].Item2.Where(x => x.Item1 == j + 1).Sum(x => x.Item2[i]);
                    Amatrix[i, j] /= pool[id].Item2.Where(x => x.Item1 == j + 1).Count();
                }
            }
            return Amatrix;
        }
        public async void FitnessEvaluation(int[] token)
        {
            List<Task> lTasks = new List<Task>();

            for (int k = 0; k < populationSize;)
            {
                lTasks.Add(Task.Run(() =>
                {
                    GlobalVar.mutex_K.WaitOne();
                    int id = k;
                    k = k + 1;
                    GlobalVar.mutex_K.ReleaseMutex();

                    //Console.Write("task #{0}", id);
                    Matrix<double> Amatrix = Matrix<double>.Build.Dense(numOfLabels, numOfLabels - 1);
                    double[] lastCoverElementSet = new double[numOfLabels];
                    Vector<double> bVector = Vector<double>.Build.Dense(expTriProb);

                    for (int i = 0; i < numOfLabels; i++)
                    {
                        if (pool[id].Item2.Where(x => x.Item1 == numOfLabels).Count() == 0)
                        {
                            Console.WriteLine();
                        }
                        lastCoverElementSet[i] = pool[id].Item2.Where(x => x.Item1 == numOfLabels).Sum(x => x.Item2[i])
                            / pool[id].Item2.Where(x => x.Item1 == numOfLabels).Count();
                    }
                    bVector = bVector.Subtract(Vector<double>.Build.Dense(lastCoverElementSet));

                    for (int i = 0; i < numOfLabels; i++)
                    {

                        for (int j = 0; j < numOfLabels - 1; j++)
                        {
                            if (pool[id].Item2.Where(x => x.Item1 == j + 1).Count() == 0)
                            {
                                Console.WriteLine();
                            }
                            Amatrix[i, j] = pool[id].Item2.Where(x => x.Item1 == j + 1).Sum(x => x.Item2[i]);
                            Amatrix[i, j] /= pool[id].Item2.Where(x => x.Item1 == j + 1).Count();
                            Amatrix[i, j] -= lastCoverElementSet[i];
                        }
                    }
                    double[] probabilities = Accord.Math.Matrix.Solve(
                        Amatrix.ToArray(), bVector.ToArray(), leastSquares: true);

                    double[] probForCESets = new double[numOfLabels];
                    for (int i = 0; i < numOfLabels - 1; i++)
                    {
                        probForCESets[i] = probabilities[i];
                    }

                    probForCESets[numOfLabels - 1] = 1 - probForCESets.Sum();
                    coverPointSetProb[id] = Copy.DeepCopy(probForCESets);
                    double error = 0;
                    for (int row = 0; row < numOfLabels; row++)
                    {
                        error += Math.Pow((Amatrix.Row(row).ToRowMatrix().Multiply(Vector<double>.Build.Dense(probForCESets).SubVector(0, probForCESets.Length - 1))[0] - bVector[row]), 2);
                    }

                    pool[id].Item1 = error;
                }));

                if (lTasks.Count == numberOfConcurrentTasks || (populationSize - k - 1 < numberOfConcurrentTasks))  
                {
                    for (int i = 0; i < lTasks.Count; i++)
                    {
                        await lTasks[i];
                    }
                    lTasks.Clear();
                }
            }

            for (int k = 0; k < populationSize; k++)
            {
                double adjustFitness = SelfAdaptivePenaltyFunction(coverPointSetProb[k], pool[k].Item1);
                pool[k].Item1 = 1.0 / adjustFitness;
            }

            token[0] = 1;
            //Console.WriteLine("Fitness Evaluation Done");
        }
        Pair<int,int,double[]>[] TwoPointCrossOver(Pair<int, int, double[]>[] s1, Pair<int, int, double[]>[] s2)
        {
           var a1 = s1.Select(x => x.Item1).ToList();
           var a2 = s2.Select(x => x.Item1).ToList();
           int po1 = GlobalVar.rnd.Next(0 , a1.Count - 1);
           int po2 = GlobalVar.rnd.Next(po1 + 1, a1.Count);
           var part1 = a1.Select((value, i) =>{ return i < po1 ? value : -1;
            }).Where(x=>x != -1);
           var part2 = a2.Select((value, i) => { return i >= po1 && i <= po2 ? value : -1;
            }).Where(x => x != -1);
           var part3 = a1.Select((value, i) => { return i > po2 ? value : -1;
            }).Where(x => x != -1);
            var newLabelSettings = part1.Concat(part2).Concat(part3).ToArray();
            var newBins = Copy.DeepCopy(bins);
            for (int i = 0; i < newBins.Length; i++)
            {
                newBins[i].Item1 = newLabelSettings[i];
            }
            return newBins;
        }
        void Mutation(Pair<int, int, double[]>[] s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (GlobalVar.rnd.NextDouble() <= mtPropotion)
                {
                    int newLabel = GlobalVar.rnd.Next(1, numOfLabels + 1);
                    s[i].Item1 = newLabel;
                }
            }
        }
         void Reproduction()
        {
            Pair<int, double, Pair<int, int, double[]>[]>[] tempPool = new Pair<int, double, Pair<int, int, double[]>[]>[populationSize];
            var bestSolution = pool.Where(x => x.Item1 == pool.Max(y=>y.Item1)).ToList().First();
            bestSolution.Item0 = 0;
            tempPool[0] = Copy.DeepCopy(bestSolution);

            for (int i = 1; i < populationSize; i++)
            {
                int[] selectedList = FitnessPropotionateSampling();
                var child = Copy.DeepCopy(pool[selectedList[0]]);
                if (GlobalVar.rnd.NextDouble() <= crRate)
                {
                    child.Item2 = TwoPointCrossOver(pool[selectedList[0]].Item2, pool[selectedList[1]].Item2);
                }
                if (GlobalVar.rnd.NextDouble() <= mtRate)
                {
                    Mutation(child.Item2);
                }
                SolutionRepair(child.Item2);
                child.Item0 = i;
                tempPool[i] = child;
            }
            Array.Clear(pool,0, pool.Length);
            pool = tempPool;
        }

        private void SolutionRepair(Pair<int, int, double[]>[] bins)
        {
            // If a cover element set doesn't contains bins, Swap the bin which has the max tri-prob with the cover
            // element set
            for (int i = 0; i < numOfLabels; i++)
            {
                List<int> InValidBinIndex = new List<int>();
                while (true)
                {
                    if (bins.Where(x => x.Item1 == i + 1).Count() == 0)
                    {
                        int binIndex = bins.Select(y =>
                        {
                            double MaxValidTriProb = bins.Max(x =>
                            {
                                return !InValidBinIndex.Contains(x.Item0) ? x.Item2[i] : -10;
                            });
                            if (y.Item2[i] == MaxValidTriProb
                                && !InValidBinIndex.Contains(y.Item0))
                            {
                                return y.Item0;
                            }
                            return -1;
                        }).Where(x => x != -1).First();

                        if (bins.Where(x => x.Item1 == bins[binIndex].Item1).Count() > 1)
                        {
                            bins[binIndex].Item1 = i + 1;
                            break;
                        }
                        else
                        {
                            InValidBinIndex.Add(binIndex);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //Validation Check, at least one bin in each cover element set
            for (int i = 0; i < numOfLabels; i++)
            {
                if (bins.Where(x => x.Item1 == i + 1).Count() == 0)
                {
                    Console.WriteLine("Invalid label {0}",i);
                }
            }
        }

        Tuple<int, int> Calxyindex(int binIndex)
        {
            int x = binIndex % numOfMinIntervalX;
            int y = binIndex / numOfMinIntervalX;
            return new Tuple<int, int>(x, y);
        }
        private int[] FitnessPropotionateSampling()
        {
            int size = 2;
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
            double sum = pool.Sum(x=>x.Item1);
            for (int i = 0; i < max - min + 1; i++)
            {
                fitnessPropotions[i] = pool[i].Item1 / sum;
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
    }
}
