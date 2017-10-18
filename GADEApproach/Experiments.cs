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
    public static class Experiments
    {
        static int maxGen = 200;
        static int runTimes = 20;
        static string rootPath = @"C:\Users\shiya\Desktop\recordTEST\";
        static int numberOfConcurrentTasks = 7;
        
        public static void nsichneuExperimentsB()
        {
            // inputs: xxx|xxxxx|000000|
            ReadSUTBranchCEData.readBranch rbce = new ReadSUTBranchCEData.readBranch();
            int[] inputs = new int[]
            {
               1,2,3,1,1,1,1,1,0,0,0,0,0,0


            };
            List<string> outputs = new List<string>();

            for (int i1 = 0; i1 < 4; i1++)
            {
                for (int i2 = 0; i2 < 4; i2++)
                {
                    for (int i3 = 0; i3 < 4; i3++)
                    {
                        for (int i4 = 0; i4 < 10; i4++)
                        {
                            for (int i5 = 0; i5 < 10; i5++)
                            {
                                for (int i6 = 0; i6 < 10; i6++)
                                {
                                    for (int i7 = 0; i7 < 10; i7++)
                                    {
                                        for (int i8 = 0; i8 < 10; i8++)
                                        {
                                            inputs = new int[]
                                            {
                                                 i1,i2,i3,i4,i5,i6,i7,i8,0,0,0,0,0,0
                                            };
                                            int[] output = null;
                                            rbce.ReadBranchCLIFunc(inputs, ref output, 3);
                                            string outputStr = "";
                                            foreach (int x in output)
                                            {
                                                outputStr += x.ToString();
                                            }
                                            outputs.Add(outputStr);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var distinctOutputs = outputs.Distinct();

        }
        public static void BestMoveExperimentsB()
        {
            rootPath = @"C:\Users\shiya\Desktop\realSUT\bestMove\";
            maxGen = 5000;
            // Real SUT
            RealSUT bestMove = new RealSUT();
            bestMove.sutBestMove(); // Setup triggering probabilities in bins
                                    //Write bins triggering prob into excel

            GALS gals = new GALS(maxGen, bestMove.numOfLabels,bestMove);

            record record = new record();
            record.fitnessGen = new double[maxGen];
            record.goodnessOfFitGen = new double[maxGen];
            record.bestSolution = new solution();

            gals.ExpectedTriggeringProbSetUp();
            gals.GAInitialization();
            gals.AlgorithmStart(record);

            //Write adjusted and overall fitnesses into excel
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("adjustedFitness", Type.GetType("System.Double"));
            dataTable.Columns.Add("overallFitness", Type.GetType("System.Double"));
            for (int u = 0; u < record.fitnessGen.Length; u++)
            {
                object[] rowData = new object[2];
                rowData[0] = (object)record.fitnessGen[u];
                rowData[1] = (object)record.goodnessOfFitGen[u];
                var row = dataTable.NewRow();
                row.ItemArray = rowData;
                dataTable.Rows.Add(row);
            }
            string filePath = rootPath + "fitnesses.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, true, filePath, false);
            }

            //Write bins triggering prob into excel
            DataTable dataTablebinsTriProb = new DataTable();
            for (int u = 0; u < bestMove.numOfLabels; u++)
            {
                dataTablebinsTriProb.Columns.Add(u.ToString(), Type.GetType("System.Double"));
            }
            for (int u = 0; u < bestMove.bins.Length; u++)
            {
                object[] rowData = null;
                rowData = bestMove.bins[u].Item2.Select(x => (object)x).ToArray();
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

            // Write Set Probabilities into excel
            DataTable dataTableSetProbabilities = new DataTable();
            dataTableSetProbabilities.Columns.Add("Set Prob.", Type.GetType("System.Double"));
            for (int u = 0; u < record.bestSolution.setProbabilities.Length; u++)
            {
                object[] rowData = new object[1];
                rowData[0] = (object)record.bestSolution.setProbabilities[u];
                var row = dataTableSetProbabilities.NewRow();
                row.ItemArray = rowData;
                dataTableSetProbabilities.Rows.Add(row);
            }
            filePath = rootPath + "setProbabilities.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTableSetProbabilities }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTableSetProbabilities }, true, filePath, false);
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

            // Write InputsBin Mapping to Excel
            filePath = rootPath + "inputsBin.xlsx";
            DataTable InputsBinTable = bestMove.MapTestInputsToBinsBestMove();
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { InputsBinTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { InputsBinTable }, true, filePath, false);
            }
        }
            
        public async static void ExperimentsA(int[] numOfLabelArray, double[][] entropy,string root)
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
                        recordsRuns[run].goodnessOfFitGen = new double[maxGen];
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

                    for (int u = 0; u < recordsRuns[0].goodnessOfFitGen.Count(); u++)
                    {
                        object[] rowData = new object[runTimes * 2];

                        for (int k = 0; k < runTimes; k++)
                        {   // Remember, calculating true or goodnessOfFit can't be used in
                            // Total Running Time calculation
                            rowData[k * 2 + 0] = (object)recordsRuns[k].goodnessOfFitGen[u];
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
    }
}
