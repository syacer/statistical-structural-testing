﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    class TestDataGeneration
    {
        string _testdataFilePath = null;
        DataTable _inputSetMapDt = null;
        int _testSize = 0;
        double[] _SetProbAry = null;

        public TestDataGeneration(
            string testdataFilePath,
            DataTable inputSetMapDt,
            int testSize,
            double[] SetProbAry)
        {

            _testdataFilePath = testdataFilePath;
            _inputSetMapDt = inputSetMapDt;
            _testSize = testSize;
            _SetProbAry = SetProbAry;
        }
        public void fulltest()
        {
            for (int i = 0; i < 512; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    testFileForMiLu("bestMove",i*512+j,new int[] {i,j }, _testdataFilePath);
                }
            }
        }
        // TestDataGenerationA is intended for input domian space < 262144
        public void testDataGenerationBestMove()
        {
            string stra = SolverCLP.rootpath.Remove(SolverCLP.rootpath.Length - 1, 1);
            string functionName = stra.Substring(stra.LastIndexOf(@"\") + 1);

            List<object[]> inputSetMap = new List<object[]>();
            List<int[]> storedInputs = new List<int[]>();

            for (int i = 0; i < _inputSetMapDt.Rows.Count; i++)
            {
                inputSetMap.Add(_inputSetMapDt.Rows[i].ItemArray);
            }

            double[] AccumulateSetProbArray = new double[_SetProbAry.Length];
            for (int i = 0; i < _SetProbAry.Length; i++)
            {
                double previous = i == 0 ? 0 : AccumulateSetProbArray[i - 1];
                AccumulateSetProbArray[i] = _SetProbAry[i]+ previous;
            }

            int retries = 0;
            while (storedInputs.Count < _testSize)
            {
                double rnd = GlobalVar.rnd.NextDouble();
                int setNum = -1;
                for (int i = 0; i < AccumulateSetProbArray.Length; i++)
                {
                    double previous = i == 0 ? 0 : AccumulateSetProbArray[i - 1];
                    double current = AccumulateSetProbArray[i];
                    if (rnd > previous && rnd <= current)
                    {
                        setNum = i + 1;
                        break;
                    }
                }
                if (setNum == -1)
                {
                    Console.WriteLine("When Set random selection, no Set found!");
                    Console.ReadKey();
                }

                int SetPositionInAry = inputSetMap[0].Length - 1;
                var InputsAryWithSetNum = inputSetMap
                    .Where(x => Convert.ToInt32(x[SetPositionInAry]) == setNum)
                    .ToArray();
                int rndInputIndexInAry = GlobalVar.rnd.Next(0, InputsAryWithSetNum.Length);
                List<int> inputList = new List<int>();
                for (int i = 0; i < InputsAryWithSetNum.Count(); i++)
                {
                    if (Convert.ToInt32(InputsAryWithSetNum[rndInputIndexInAry][i]) != -9999999)
                    {
                        inputList.Add(Convert.ToInt32(InputsAryWithSetNum[rndInputIndexInAry][i]));
                    }
                    else
                    {
                        break;
                    }
                }
                if (storedInputs.Where(x => x.SequenceEqual(inputList.ToArray()) == true).Count() == 0)
                {
                    storedInputs.Add(inputList.ToArray());
                    testFileForMiLu(functionName, storedInputs.Count() - 1, inputList.ToArray(), _testdataFilePath);
                    retries = 0;
                    Console.WriteLine("Num Of Test cases: {0}", storedInputs.Count);
                }
                else
                {
                    retries++;
                    Console.WriteLine("Retries: {0}", retries);
                }

                if (retries == 5000)
                {
                    Console.WriteLine("Test Data Generation Complete with test size less than expected");
                    break;
                }
            }


        }

        public void BestMoveRandomTestSet()
        {
            List<int[]> dataSet = new List<int[]>();
            while (dataSet.Count < 1000)
            {
                int x = GlobalVar.rnd.Next(0, 512);
                int y = GlobalVar.rnd.Next(0, 512);
                var newData = new int[] { x, y };
                if (dataSet.Where(k => k.SequenceEqual(newData)).Count() == 0)
                {
                    dataSet.Add(newData);
                    testFileForMiLu("bestMove", dataSet.Count, new int[] { x, y }, _testdataFilePath);
                }
            }
        }
        public void testFileForMiLu(string functionName, int i, int[] inputsAry, string path)
        {
            LocalFileAccess lfa = new LocalFileAccess();
            string str1 = string.Format(@"int test_{0}_{1}() {{return {0}(",functionName,i);
            string str3 = string.Format(@");}}");
            string str2 = null;
            for (int j = 0; j < inputsAry.Length; j++)
            {
                str2 += inputsAry[j].ToString() + @", ";
            }
            str2 = str2.Remove(str2.Length-2,2);
            string str = str1 + str2 + str3;
            Console.WriteLine(str);
            List<string> lsr = new List<string>();
            lsr.Add(str);
            lfa.StoreListToLinesAppend(path, lsr);
        }
    }
}