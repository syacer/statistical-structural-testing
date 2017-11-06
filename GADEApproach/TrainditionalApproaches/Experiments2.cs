using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using GADEApproach.TrainditionalApproaches.GA;
using GADEApproach.TrainditionalApproaches.HC;
using GADEApproach.TrainditionalApproaches.SA;

namespace GADEApproach.TrainditionalApproaches
{
    class Experiments2:Experiments
    {
        public void BestMoveExperimentsB2(string rootpath, int numOfTestCases,string algorithm)
        {
            List<record> records = new List<record>();
            rootPath = rootpath;
            string filePath = null;
            string testInputsFilePath = rootpath + @"testData";
            maxGen = 500;

            // Real SUT
            RealSUT bestMove = new RealSUT();
            bestMove.sutBestMove(); // Setup triggering probabilities in bins
                                    //Write bins triggering prob into excel

            List<Task> lTasks = new List<Task>();
            int remainTasks = bestMove.numOfLabels;

            for (int i = 0; i < bestMove.numOfLabels; i++)
            {
                record record = new record();
                record.fitnessGen = new double[maxGen];
                record.bestSolution = new solution();
                switch (algorithm)
                {
                    case "SA":
                        new SAAlgorithm(maxGen, bestMove.numOfLabels, bestMove, i).SA_Start(record);
                        break;
                    case "HC":
                        new HCAlgorithm(maxGen, bestMove.numOfLabels, bestMove, i).HC_Start(record);
                        break;
                    case "GA":
                        new GAAlgoithm(maxGen, bestMove.numOfLabels, bestMove, i).GA_Start(record);
                        break;
                }

                records.Add(record);
            }

            var weightMatrices = records.Select(x => x.bestSolution.WeightMateix).ToList();
            new TestDataGeneration2(testInputsFilePath,null, numOfTestCases,null)
                .testDataGenerationBestMove2(weightMatrices, bestMove,rootPath);
            //Write set probabilities into excel
            DataTable SetProbdataTable = new DataTable();
            for (int i = 0; i < bestMove.numOfLabels; i++)
            {
                SetProbdataTable.Columns.Add("setProbs_"+i.ToString(), Type.GetType("System.Double"));
            }

            for (int u = 0; u < records[0].bestSolution.setProbabilities.Length; u++)
            {
                object[] rowData = new object[records.Count];
                for (int i = 0; i < records.Count; i++)
                {
                    rowData[i] = (object)records[i].bestSolution.setProbabilities[u];
                }
                var row = SetProbdataTable.NewRow();
                row.ItemArray = rowData;
                SetProbdataTable.Rows.Add(row);
            }
            filePath = rootPath + "setProbabilities.xlsx";
            if (!File.Exists(filePath))
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { SetProbdataTable }, true, filePath, true);
            }
            else
            {
                ExcelOperation.dataTableListToExcel(new List<DataTable>() { SetProbdataTable }, true, filePath, false);
            }

            //Write overall fitnesses into excel
            DataTable dataTable = new DataTable();
            for (int i = 0; i < bestMove.numOfLabels; i++)
            {
                dataTable.Columns.Add("Fitness_CE"+i.ToString(), Type.GetType("System.Double"));
            }

            for (int u = 0; u < records[0].fitnessGen.Length; u++)
            {
                object[] rowData = new object[records.Count];
                for (int i = 0; i < records.Count; i++)
                {
                    rowData[i] = (object)records[i].fitnessGen[u];
                }
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

            // Write total running time into excel
            DataTable totalRunningTimeTable = new DataTable();

            totalRunningTimeTable.Columns.Add("CE", Type.GetType("System.Int32"));
            totalRunningTimeTable.Columns.Add("RunTime", Type.GetType("System.Double"));
            for (int u = 0; u < records.Count; u++)
            {
                object[] rowData = new object[2];
                rowData[0] = (object)u;
                rowData[1] = (object)records[u].bestSolution.totalRunTime;
                var row = dataTable.NewRow();
                row.ItemArray = rowData;
                dataTable.Rows.Add(row);
            }
            long totalRunTime = records.Sum(x => x.bestSolution.totalRunTime);
            object[] runningtime = new object[] {-100, totalRunTime};
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
