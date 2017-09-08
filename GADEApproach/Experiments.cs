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
        static int maxGen = 5000;
        static int runTimes = 30;
        static string rootPath = @"C:\Users\shiya\Desktop\recordTEST\";
        static int numberOfConcurrentTasks = 10;
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
                    record[] records30Runs = new record[30];
                    List<Task> lTasks = new List<Task>();
                    for (int run = 0; run < 30; run++)
                    {
                        records30Runs[run] = new record();
                        records30Runs[run].fitnessGen = new double[maxGen];
                        records30Runs[run].goodnessOfFitGen = new double[maxGen];
                        records30Runs[run].bestSolution = new solution();
                        records30Runs[run].bestSolution.setProbabilities = new double[numOfLabelArray[i]];
                        records30Runs[run].bestSolution.trueTriProbs = new double[numOfLabelArray[i]];
                    }

                    List<Task> taskList = new List<Task>();
                    bool finish = false;

                    await Task.Run(() =>
                    {
                        int count = 0;
                        while (finish == false)
                        {
                            if (count < 30)
                            {
                                taskList.Add(Task.Run(() =>
                                {
                                    GlobalVar.mutex_K.WaitOne();
                                    int id = count;
                                    count = count + 1;
                                    GlobalVar.mutex_K.ReleaseMutex();
                                    if (id > 29)
                                    {
                                        return;
                                    }
                                    //Console.WriteLine("ID: #{0}",id);
                                    //Thread.Sleep(GlobalVar.rnd.Next(1, 5) * 1000);
                                    SyntheticSUT sutTypeC = new SyntheticSUT().SUTC(numOfLabelArray[i], entropy[i][j]);
                                    GlobalVar.mutex_K.WaitOne();
                                    sutTypeC.InputDomainToExcel(rootPath + @"Label_" + fileName + "_" + "r" + id.ToString() + ".xlsx");
                                    GlobalVar.mutex_K.ReleaseMutex();
                                    GALS gals = new GALS(maxGen, numOfLabelArray[i], sutTypeC);
                                    gals.ExpectedTriggeringProbSetUp();
                                    gals.BinsInitialization();
                                    gals.GAInitialization();
                                    gals.AlgorithmStart(records30Runs[id]);
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

                    for (int k = 0; k < 60; k++)
                    {
                        dataTable.Columns.Add(k.ToString(), Type.GetType("System.Double"));
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
                    }

                    string filePath = rootPath + @"Data_" + fileName + ".xlsx";
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable }, false, filePath,true);

                    //Write solution to Excel
                    //Input label mapping
                    for (int k = 0; k < 30; k++)
                    {
                        DataTable dataTable2 = new DataTable();
//Record Solution
#if false
                        for (int u = 0; u < numOfLabelArray[i] + 1; u++)
                        {
                            dataTable2.Columns.Add(u.ToString(), Type.GetType("System.Double"));
                        }
                        int xLength = records30Runs[k].bestSolution.inputsinSets.Length;
                        int yLength = records30Runs[k].bestSolution.inputsinSets[0].Length;
                        for (int u = 0; u < xLength * yLength; u++)
                        {
                            object[] rowData2 = new object[3];
                            rowData2[0] = (object)(u % xLength);
                            rowData2[1] = (object)(u / yLength);
                            rowData2[2] = (object)records30Runs[k].bestSolution.inputsinSets[u % xLength][u / yLength];
                            var row2 = dataTable2.NewRow();
                            row2.ItemArray = rowData2;
                            dataTable2.Rows.Add(row2);
                        }
#endif
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

                        filePath = rootPath + @"Solution_" + fileName + ".xlsx";

                        if (!File.Exists(filePath))
                        {
                            ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable2 }, false, filePath, true);
                        }
                        else
                        {
                            ExcelOperation.dataTableListToExcel(new List<DataTable>() { dataTable2 }, false, filePath, false);
                        }
                    }
                }
            }
        }
    }
}
