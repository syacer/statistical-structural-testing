using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GADEApproach.GALS;

namespace GADEApproach
{
    public class Experiments
    {
         public int maxGen = 200;
         public int runTimes = 20;
         public string rootPath = @"C:\Users\shiya\Desktop\recordTEST2\";
         public int numberOfConcurrentTasks = 7;

#region useless SUTs
        public void binarySearchExperimentsB(string rootpath, int numOfTestCases)
        {
            //binary search too simple
            //1 test case kills all mutants
            RealSUT bs = new RealSUT();
            bs.sutbinarysearch();
        }

        public void quickSortExperimentB(string rootpath, int numOfTestCases)
        {
            //quicksort too simple
            //1 test case kills all mutants
            rootPath = rootpath;
            string filePath = null;
            string testInputsFilePath = rootpath + @"testData";
            maxGen = 3000;

            RealSUT qs = new RealSUT();
            qs.sutquicksort();

            DataTable dataTablebinsTriProb = new DataTable();

            for (int u = 0; u < qs.numOfLabels; u++)
            {
                dataTablebinsTriProb.Columns.Add(u.ToString(), Type.GetType("System.Double"));
            }
            for (int u = 0; u < qs.bins.Length; u++)
            {
                object[] rowData = null;
                rowData = qs.bins[u].ProbInBins.Select(x => (object)x).ToArray();
                var row = dataTablebinsTriProb.NewRow();
                row.ItemArray = rowData;
                dataTablebinsTriProb.Rows.Add(row);
            }

            //Task.Run(() =>sssss
            //{
            //    filePath = rootPath + "triInBins.xlsx";
            //    if (!File.Exists(filePath))
            //    {
            //        ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, true);
            //    }
            //    else
            //    {
            //        ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, false);
            //    }
            //});

            GALS gals = new GALS(maxGen, qs.numOfLabels, qs);

            record record = new record();
            record.fitnessGen = new double[maxGen];
            record.probLowBoundGen = new double[maxGen];
            record.bestSolution = new solution();

            gals.ExpectedTriggeringProbSetUp();
            gals.GAInitialization();
            gals.AlgorithmStart(record);


            DataSet ds = SolverCLP.FinalResultsinDataTables(
                null,
                record.bestSolution.binSetup,
                record.bestSolution.setProbabilities,
                record.bestSolution.Amatrix,
                @"InputSetMap",
                @"Weights",
                @"TrigProbEst",
                @"TrigProbTrue"
                );

            // Write InputsBin Mapping to Excel
            TestDataGeneration tdg = new TestDataGeneration(
                testInputsFilePath,
                null,
                numOfTestCases,
                record.bestSolution.setProbabilities,
                qs
                );
            tdg.testDataGenerationQuickSort(rootPath, record.bestSolution.binSetup);

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                if (i == 0)
                {   // ignore the inputSetMap table
                    continue;
                }
                string excelName = ds.Tables[i].TableName + ".xlsx";
                if (!File.Exists(rootpath + excelName))
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            true
                            );
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            false
                            );
                }
            }

            //Write adjusted and overall fitnesses into excel
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("adjustedFitness", Type.GetType("System.Double"));
            dataTable.Columns.Add("overallFitness", Type.GetType("System.Double"));
            for (int u = 0; u < record.fitnessGen.Length; u++)
            {
                object[] rowData = new object[2];
                rowData[0] = (object)record.fitnessGen[u];
                rowData[1] = (object)record.probLowBoundGen[u];
                var row = dataTable.NewRow();
                row.ItemArray = rowData;
                dataTable.Rows.Add(row);
            }
            filePath = rootPath + "fitnesses.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, false);
            }

            //Write bins setup into excel
            DataTable dataTablebinssetup = new DataTable();
            dataTablebinssetup.Columns.Add("binssetup", Type.GetType("System.Double"));
            for (int u = 0; u < record.bestSolution.binSetup.Length; u++)
            {
                object[] rowData = new object[1];
                rowData[0] = (object)record.bestSolution.binSetup[u];
                var row = dataTablebinssetup.NewRow();
                row.ItemArray = rowData;
                dataTablebinssetup.Rows.Add(row);
            }
            filePath = rootPath + "binsSetup.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, false);
            }

            // Write total running time into excel
            DataTable totalRunningTimeTable = new DataTable();
            totalRunningTimeTable.Columns.Add("Total Running Time", Type.GetType("System.Double"));
            object[] runningtime = new object[] { record.bestSolution.totalRunTime };
            var rowRunningTime = totalRunningTimeTable.NewRow();
            rowRunningTime.ItemArray = runningtime;
            totalRunningTimeTable.Rows.Add(rowRunningTime);
            filePath = rootPath + "runningTime.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, false);
            }
        }
#endregion


        public void nsichneuExperimentsB(string rootpath, int numOfTestCases)
        {
            rootPath = rootpath;
            string filePath = null;
            string testInputsFilePath = rootpath + @"testData";
            maxGen = 3000;
            int partCount = 1;
            RealSUT neichneu = new RealSUT();
            var listPartitions = neichneu.sutnsichneu(partCount);

            DataTable dataTablebinsTriProb = new DataTable();

            for (int u = 0; u < neichneu.numOfLabels; u++)
            {
                dataTablebinsTriProb.Columns.Add(u.ToString(), Type.GetType("System.Double"));
            }
            for (int u = 0; u < neichneu.bins.Length; u++)
            {
                object[] rowData = null;
                rowData = neichneu.bins[u].ProbInBins.Select(x => (object)x).ToArray();
                var row = dataTablebinsTriProb.NewRow();
                row.ItemArray = rowData;
                dataTablebinsTriProb.Rows.Add(row);
            }

            //Task.Run(() =>
            //{
            //    filePath = rootPath + "triInBins.xlsx";
            //    if (!File.Exists(filePath))
            //    {
            //        ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, true);
            //    }
            //    else
            //    {
            //        ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, false);
            //    }
            //});

            for (int o = partCount-1; o >= 0; o--)
            {
                neichneu.setPartBinsToBinSutsichneu(listPartitions[o]);
                GALS gals = new GALS(maxGen, neichneu.numOfLabels, neichneu);

                record record = new record();
                record.fitnessGen = new double[maxGen];
                record.probLowBoundGen = new double[maxGen];
                record.bestSolution = new solution();

                gals.ExpectedTriggeringProbSetUp();
                gals.GAInitialization();
                gals.AlgorithmStart(record);


                DataSet ds = SolverCLP.FinalResultsinDataTables(
                    null,
                    record.bestSolution.binSetup,
                    record.bestSolution.setProbabilities,
                    record.bestSolution.Amatrix,
                    @"InputSetMap",
                    @"Weights",
                    @"TrigProbEst",
                    @"TrigProbTrue"
                    );

                // Write InputsBin Mapping to Excel
                TestDataGeneration tdg = new TestDataGeneration(
                    testInputsFilePath,
                    null,
                    numOfTestCases / partCount,
                    record.bestSolution.setProbabilities,
                    neichneu
                    );
                tdg.testDataGenerationNichneu(rootPath,record.bestSolution.binSetup);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    if (i == 0)
                    {   // ignore the inputSetMap table
                        continue;
                    }
                    string excelName = ds.Tables[i].TableName + ".xlsx";
                    if (!File.Exists(rootpath + excelName))
                    {
                        ExcelOperation.dataTableListToExcel(
                                new List<DataTable>() { ds.Tables[i] },
                                true,
                                rootpath + excelName,
                                true
                                );
                    }
                    else
                    {
                        ExcelOperation.dataTableListToExcel(
                                new List<DataTable>() { ds.Tables[i] },
                                true,
                                rootpath + excelName,
                                false
                                );
                    }
                }

                //Write adjusted and overall fitnesses into excel
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("adjustedFitness", Type.GetType("System.Double"));
                dataTable.Columns.Add("overallFitness", Type.GetType("System.Double"));
                for (int u = 0; u < record.fitnessGen.Length; u++)
                {
                    object[] rowData = new object[2];
                    rowData[0] = (object)record.fitnessGen[u];
                    rowData[1] = (object)record.probLowBoundGen[u];
                    var row = dataTable.NewRow();
                    row.ItemArray = rowData;
                    dataTable.Rows.Add(row);
                }
                filePath = rootPath + "fitnesses.xlsx";
                if (!File.Exists(filePath))
                {
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, true);
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, false);
                }

                //Write bins setup into excel
                DataTable dataTablebinssetup = new DataTable();
                dataTablebinssetup.Columns.Add("binssetup", Type.GetType("System.Double"));
                for (int u = 0; u < record.bestSolution.binSetup.Length; u++)
                {
                    object[] rowData = new object[1];
                    rowData[0] = (object)record.bestSolution.binSetup[u];
                    var row = dataTablebinssetup.NewRow();
                    row.ItemArray = rowData;
                    dataTablebinssetup.Rows.Add(row);
                }
                filePath = rootPath + "binsSetup.xlsx";
                if (!File.Exists(filePath))
                {
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, true);
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, false);
                }

                // Write total running time into excel
                DataTable totalRunningTimeTable = new DataTable();
                totalRunningTimeTable.Columns.Add("Total Running Time", Type.GetType("System.Double"));
                object[] runningtime = new object[] { record.bestSolution.totalRunTime };
                var rowRunningTime = totalRunningTimeTable.NewRow();
                rowRunningTime.ItemArray = runningtime;
                totalRunningTimeTable.Rows.Add(rowRunningTime);
                filePath = rootPath + "runningTime.xlsx";
                if (!File.Exists(filePath))
                {
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, true);
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, false);
                }
            }
        }
        public void BestMoveExperimentsB(string rootpath, int numOfTestCases)
        {
            rootPath = rootpath;
            string filePath = null;
            string testInputsFilePath = rootpath + @"testData";
            maxGen = 3000;

            // Real SUT
            RealSUT bestMove = new RealSUT();
            bestMove.sutBestMove(); // Setup triggering probabilities in bins
                                    //Write bins triggering prob into excel

            DataTable dataTablebinsTriProb = new DataTable();
            for (int u = 0; u < bestMove.numOfLabels; u++)
            {
                dataTablebinsTriProb.Columns.Add(u.ToString(), Type.GetType("System.Double"));
            }
            for (int u = 0; u < bestMove.bins.Length; u++)
            {
                object[] rowData = null;
                rowData = bestMove.bins[u].ProbInBins.Select(x => (object)x).ToArray();
                var row = dataTablebinsTriProb.NewRow();
                row.ItemArray = rowData;
                dataTablebinsTriProb.Rows.Add(row);
            }
            filePath = rootPath + "triInBins.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, false);
            }


            GALS gals = new GALS(maxGen, bestMove.numOfLabels,bestMove);

            record record = new record();
            record.fitnessGen = new double[maxGen];
            record.probLowBoundGen = new double[maxGen];
            record.bestSolution = new solution();

            gals.ExpectedTriggeringProbSetUp();
            gals.GAInitialization();
            gals.AlgorithmStart(record);

            DataTable InputsBinTable = bestMove.MapTestInputsToBinsBestMove();
            DataSet ds = SolverCLP.FinalResultsinDataTables(
                InputsBinTable,
                record.bestSolution.binSetup,
                record.bestSolution.setProbabilities,
                record.bestSolution.Amatrix,
                @"InputSetMap",
                @"Weights",
                @"TrigProbEst",
                @"TrigProbTrue"
                );

            // Write InputsBin Mapping to Excel
            TestDataGeneration tdg = new TestDataGeneration(
                testInputsFilePath,
                ds.Tables[0],
                numOfTestCases,
                record.bestSolution.setProbabilities,
                bestMove
                );
            tdg.testDataGenerationBestMove(rootpath);

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                if (i == 0)
                {   // ignore the inputSetMap table
                    continue;
                }
                string excelName = ds.Tables[i].TableName + ".xlsx";
                if (!File.Exists(rootpath + excelName))
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            true
                            );
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            false
                            );
                }
            }

            //Write adjusted and overall fitnesses into excel
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("adjustedFitness", Type.GetType("System.Double"));
            dataTable.Columns.Add("overallFitness", Type.GetType("System.Double"));
            for (int u = 0; u < record.fitnessGen.Length; u++)
            {
                object[] rowData = new object[2];
                rowData[0] = (object)record.fitnessGen[u];
                rowData[1] = (object)record.probLowBoundGen[u];
                var row = dataTable.NewRow();
                row.ItemArray = rowData;
                dataTable.Rows.Add(row);
            }
            filePath = rootPath + "fitnesses.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, false);
            }

            //Write bins setup into excel
            DataTable dataTablebinssetup = new DataTable();
            dataTablebinssetup.Columns.Add("binssetup", Type.GetType("System.Double"));
            for (int u = 0; u < record.bestSolution.binSetup.Length; u++)
            {
                object[] rowData = new object[1];
                rowData[0] = (object)record.bestSolution.binSetup[u];
                var row = dataTablebinssetup.NewRow();
                row.ItemArray = rowData;
                dataTablebinssetup.Rows.Add(row);
            }
            filePath = rootPath + "binsSetup.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, false);
            }

            // Write total running time into excel
            DataTable totalRunningTimeTable = new DataTable();
            totalRunningTimeTable.Columns.Add("Total Running Time", Type.GetType("System.Double"));
            object[] runningtime = new object[] {record.bestSolution.totalRunTime};
            var rowRunningTime = totalRunningTimeTable.NewRow();
            rowRunningTime.ItemArray = runningtime;
            totalRunningTimeTable.Rows.Add(rowRunningTime);
            filePath = rootPath + "runningTime.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, true);
            }
            else
            { 
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, false);
            }
        }
            
        public async void ExperimentsA(int[] numOfLabelArray, double[][] entropy,string root)
        {
            rootPath = root + @"\";
            for (int i = 0; i < numOfLabelArray.Length; i++)
            {
                for (int k = 0; k < entropy[i].Count(); k++)
                {
                    entropy[i][k] *= Math.Log(numOfLabelArray[i], 2);
                }
            }

            string fileName;
            for (int i = 0; i < numOfLabelArray.Count(); i++)
            {
                for (int j = 0; j < entropy[0].Count(); j++)
                {
                    fileName = numOfLabelArray[i].ToString() + "_" + entropy[i][j].ToString() + "_";
                    record[] recordsRuns = new record[runTimes];
                    List<Task> lTasks = new List<Task>();
                    for (int run = 0; run < runTimes; run++)
                    {
                        recordsRuns[run] = new record();
                        recordsRuns[run].fitnessGen = new double[maxGen];
                        recordsRuns[run].probLowBoundGen = new double[maxGen];
                        recordsRuns[run].bestSolution = new solution();
                        recordsRuns[run].bestSolution.setProbabilities = new double[numOfLabelArray[i]];
                    }

                    List<Task> taskList = new List<Task>();
                    bool finish = false;

                    await Task.Run(() =>
                    {
                        int count = 0;
                        while (finish == false)
                        {
                            if (count < runTimes)
                            {
                                taskList.Add(Task.Run(() =>
                                {
                                    GlobalVar.mutex_K.WaitOne();
                                    int id = count;
                                    count = count + 1;
                                    if (id > runTimes - 1)
                                    {
                                        GlobalVar.mutex_K.ReleaseMutex();
                                        return;
                                    }
                                    SyntheticSUT sutTypeC = new SyntheticSUT().SUTB(numOfLabelArray[i], entropy[i][j]);
                                    sutTypeC.InputDomainToExcel(rootPath + @"Label_" + fileName + "_" + "r" + id.ToString() + ".xlsx");
                                    GlobalVar.mutex_K.ReleaseMutex();
                                    GALS gals = new GALS(maxGen, numOfLabelArray[i], sutTypeC);
                                    gals.ExpectedTriggeringProbSetUp();
                                    sutTypeC.BinsInitialization();
                                    gals.GAInitialization();
                                    gals.AlgorithmStart(recordsRuns[id]);
                                    Console.WriteLine(id);
                                }));
                            }
                            else
                            {
                                if (taskList.Count == 0)
                                {
                                    finish = true;
                                }
                                for (int ti = 0; ti < taskList.Count; ti++)
                                {
                                    if (taskList[ti].Status == TaskStatus.RanToCompletion)
                                    {
                                        taskList.Remove(taskList[ti]);
                                        break;
                                    }
                                }
                            }

                            while (taskList.Count == numberOfConcurrentTasks)
                            {
                                for (int ti = 0; ti < taskList.Count; ti++)
                                {
                                    if (taskList[ti].Status == TaskStatus.RanToCompletion)
                                    {
                                        taskList.Remove(taskList[ti]);
                                        break;
                                    }
                                }
                            }
                        }
                    });

                    // Data Print to Excel
                    DataTable dataTable = new DataTable();

                    for (int k = 0; k < runTimes * 2; k++)
                    {
                        dataTable.Columns.Add(k.ToString(), Type.GetType("System.Double"));
                    }

                    for (int u = 0; u < recordsRuns[0].probLowBoundGen.Count(); u++)
                    {
                        object[] rowData = new object[runTimes * 2];

                        for (int k = 0; k < runTimes; k++)
                        {   // Remember, calculating true or goodnessOfFit can't be used in
                            // Total Running Time calculation
                            rowData[k * 2 + 0] = (object)recordsRuns[k].probLowBoundGen[u];
                            rowData[k * 2 + 1] = (object)recordsRuns[k].fitnessGen[u];
                        }
                        var row = dataTable.NewRow();
                        row.ItemArray = rowData;
                        dataTable.Rows.Add(row);
                    }

                    string filePath = rootPath + @"Data_" + fileName + ".xlsx";
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath,true);

                    //Write solution to Excel
                    //Input label mapping
                    for (int k = 0; k < runTimes; k++)
                    {
                        DataTable dataTable2 = new DataTable();
//Record Solution

                        for (int u = 0; u < numOfLabelArray[i] + 1; u++)
                        {
                            dataTable2.Columns.Add(u.ToString(), Type.GetType("System.Double"));
                        }
#if false
                        int xLength = recordsRuns[k].bestSolution.inputsinSets.Length;
                        int yLength = recordsRuns[k].bestSolution.inputsinSets[0].Length;
                        for (int u = 0; u < xLength * yLength; u++)
                        {
                            object[] rowData2 = new object[3];
                            rowData2[0] = (object)(u % xLength);
                            rowData2[1] = (object)(u / yLength);
                            rowData2[2] = (object)recordsRuns[k].bestSolution.inputsinSets[u % xLength][u / yLength];
                            var row2 = dataTable2.NewRow();
                            row2.ItemArray = rowData2;
                            dataTable2.Rows.Add(row2);
                        }
#endif
                        //Set Probabilities
                        var rowData3 = recordsRuns[k].bestSolution
                            .setProbabilities.Select(x => (object)x).ToArray();
                        var row3 = dataTable2.NewRow();
                        row3.ItemArray = rowData3;
                        dataTable2.Rows.Add(row3);

                        //Total RunTime
                        object[] rowData5 = new object[1];
                        rowData5[0] = (object)recordsRuns[k].bestSolution.totalRunTime;
                        var row5 = dataTable2.NewRow();
                        row5.ItemArray = rowData5;
                        dataTable2.Rows.Add(row5);

                        filePath = rootPath + @"Solution_" + fileName + ".xlsx";

                        if (!File.Exists(filePath))
                        {
                            ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable2 }, true, filePath, true);
                        }
                        else
                        {
                            ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable2 }, true, filePath, false);
                        }
                    }
                }
            }
        }

        public void MatrixInverseExperimentB(string rootpath, int numOfTestCases)
        {

            rootPath = rootpath;
            string filePath = null;
            string testInputsFilePath = rootpath + @"testData";
            maxGen = 3000;

            RealSUT mi = new RealSUT();
            mi.sutmatrixinverse();

            DataTable dataTablebinsTriProb = new DataTable();

            for (int u = 0; u < mi.numOfLabels; u++)
            {
                dataTablebinsTriProb.Columns.Add(u.ToString(), Type.GetType("System.Double"));
            }
            for (int u = 0; u < mi.bins.Length; u++)
            {
                object[] rowData = null;
                rowData = mi.bins[u].ProbInBins.Select(x => (object)x).ToArray();
                var row = dataTablebinsTriProb.NewRow();
                row.ItemArray = rowData;
                dataTablebinsTriProb.Rows.Add(row);
            }

            //Task.Run(() =>
            //{
            //    filePath = rootPath + "triInBins.xlsx";
            //    if (!File.Exists(filePath))
            //    {
            //        ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, true);
            //    }
            //    else
            //    {
            //        ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, false);
            //    }
            //});

            GALS gals = new GALS(maxGen, mi.numOfLabels, mi);

            record record = new record();
            record.fitnessGen = new double[maxGen];
            record.probLowBoundGen = new double[maxGen];
            record.bestSolution = new solution();

            gals.ExpectedTriggeringProbSetUp();
            gals.GAInitialization();
           gals.AlgorithmStart(record);


            DataSet ds = SolverCLP.FinalResultsinDataTables(
                null,
                record.bestSolution.binSetup,
                record.bestSolution.setProbabilities,
                record.bestSolution.Amatrix,
                @"InputSetMap",
                @"Weights",
                @"TrigProbEst",
                @"TrigProbTrue"
                );

            // Write InputsBin Mapping to Excel
            TestDataGeneration tdg = new TestDataGeneration(
                testInputsFilePath,
                null,
                numOfTestCases,
                record.bestSolution.setProbabilities,
                mi
                );
            tdg.testDataGenerationMatrixInverse(rootPath, record.bestSolution.binSetup);

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                if (i == 0)
                {   // ignore the inputSetMap table
                    continue;
                }
                string excelName = ds.Tables[i].TableName + ".xlsx";
                if (!File.Exists(rootpath + excelName))
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            true
                            );
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            false
                            );
                }
            }

            //Write adjusted and overall fitnesses into excel
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("adjustedFitness", Type.GetType("System.Double"));
            dataTable.Columns.Add("overallFitness", Type.GetType("System.Double"));
            for (int u = 0; u < record.fitnessGen.Length; u++)
            {
                object[] rowData = new object[2];
                rowData[0] = (object)record.fitnessGen[u];
                rowData[1] = (object)record.probLowBoundGen[u];
                var row = dataTable.NewRow();
                row.ItemArray = rowData;
                dataTable.Rows.Add(row);
            }
            filePath = rootPath + "fitnesses.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, false);
            }

            //Write bins setup into excel
            DataTable dataTablebinssetup = new DataTable();
            dataTablebinssetup.Columns.Add("binssetup", Type.GetType("System.Double"));
            for (int u = 0; u < record.bestSolution.binSetup.Length; u++)
            {
                object[] rowData = new object[1];
                rowData[0] = (object)record.bestSolution.binSetup[u];
                var row = dataTablebinssetup.NewRow();
                row.ItemArray = rowData;
                dataTablebinssetup.Rows.Add(row);
            }
            filePath = rootPath + "binsSetup.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, false);
            }

            // Write total running time into excel
            DataTable totalRunningTimeTable = new DataTable();
            totalRunningTimeTable.Columns.Add("Total Running Time", Type.GetType("System.Double"));
            object[] runningtime = new object[] { record.bestSolution.totalRunTime };
            var rowRunningTime = totalRunningTimeTable.NewRow();
            rowRunningTime.ItemArray = runningtime;
            totalRunningTimeTable.Rows.Add(rowRunningTime);
            filePath = rootPath + "runningTime.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, false);
            }
        }

        public void TriangleExperimentB(string rootpath, int numOfTestCases)
        {

            rootPath = rootpath;
            string filePath = null;
            string testInputsFilePath = rootpath + @"testData";
            maxGen = 3000;

            RealSUT tri = new RealSUT();
            tri.sutTriangle();

            DataTable dataTablebinsTriProb = new DataTable();

            for (int u = 0; u < tri.numOfLabels; u++)
            {
                dataTablebinsTriProb.Columns.Add(u.ToString(), Type.GetType("System.Double"));
            }
            for (int u = 0; u < tri.bins.Length; u++)
            {
                object[] rowData = null;
                rowData = tri.bins[u].ProbInBins.Select(x => (object)x).ToArray();
                var row = dataTablebinsTriProb.NewRow();
                row.ItemArray = rowData;
                dataTablebinsTriProb.Rows.Add(row);
            }

            Task.Run(() =>
            {
                filePath = rootPath + "triInBins.xlsx";
                if (!File.Exists(filePath))
                {
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, true);
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinsTriProb }, true, filePath, false);
                }
            });

            GALS gals = new GALS(maxGen, tri.numOfLabels, tri);

            record record = new record();
            record.fitnessGen = new double[maxGen];
            record.probLowBoundGen = new double[maxGen];
            record.bestSolution = new solution();

            gals.ExpectedTriggeringProbSetUp();
            gals.GAInitialization();
            gals.AlgorithmStart(record);


            DataSet ds = SolverCLP.FinalResultsinDataTables(
                null,
                record.bestSolution.binSetup,
                record.bestSolution.setProbabilities,
                record.bestSolution.Amatrix,
                @"InputSetMap",
                @"Weights",
                @"TrigProbEst",
                @"TrigProbTrue"
                );

            // Write InputsBin Mapping to Excel
            TestDataGeneration tdg = new TestDataGeneration(
                testInputsFilePath,
                null,
                numOfTestCases,
                record.bestSolution.setProbabilities,
                tri
                );
            tdg.testDataGenerationTriangle(rootPath, record.bestSolution.binSetup);

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                if (i == 0)
                {   // ignore the inputSetMap table
                    continue;
                }
                string excelName = ds.Tables[i].TableName + ".xlsx";
                if (!File.Exists(rootpath + excelName))
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            true
                            );
                }
                else
                {
                    ExcelOperation.dataTableListToExcel(
                            new List<DataTable>() { ds.Tables[i] },
                            true,
                            rootpath + excelName,
                            false
                            );
                }
            }

            //Write adjusted and overall fitnesses into excel
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("adjustedFitness", Type.GetType("System.Double"));
            dataTable.Columns.Add("overallFitness", Type.GetType("System.Double"));
            for (int u = 0; u < record.fitnessGen.Length; u++)
            {
                object[] rowData = new object[2];
                rowData[0] = (object)record.fitnessGen[u];
                rowData[1] = (object)record.probLowBoundGen[u];
                var row = dataTable.NewRow();
                row.ItemArray = rowData;
                dataTable.Rows.Add(row);
            }
            filePath = rootPath + "fitnesses.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, false);
            }

            //Write bins setup into excel
            DataTable dataTablebinssetup = new DataTable();
            dataTablebinssetup.Columns.Add("binssetup", Type.GetType("System.Double"));
            for (int u = 0; u < record.bestSolution.binSetup.Length; u++)
            {
                object[] rowData = new object[1];
                rowData[0] = (object)record.bestSolution.binSetup[u];
                var row = dataTablebinssetup.NewRow();
                row.ItemArray = rowData;
                dataTablebinssetup.Rows.Add(row);
            }
            filePath = rootPath + "binsSetup.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTablebinssetup }, true, filePath, false);
            }

            // Write total running time into excel
            DataTable totalRunningTimeTable = new DataTable();
            totalRunningTimeTable.Columns.Add("Total Running Time", Type.GetType("System.Double"));
            object[] runningtime = new object[] { record.bestSolution.totalRunTime };
            var rowRunningTime = totalRunningTimeTable.NewRow();
            rowRunningTime.ItemArray = runningtime;
            totalRunningTimeTable.Rows.Add(rowRunningTime);
            filePath = rootPath + "runningTime.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { totalRunningTimeTable }, true, filePath, false);
            }
        }
    }
}
