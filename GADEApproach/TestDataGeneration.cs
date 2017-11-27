using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    public class TestDataGeneration
    {
        public string _testdataFilePath = null;
        DataTable _inputSetMapDt = null;
        public int _testSize = 0;
        double[] _SetProbAry = null;
        SUT _sut = null;
        public TestDataGeneration(
            string testdataFilePath,
            DataTable inputSetMapDt,
            int testSize,
            double[] SetProbAry,
            SUT sut)
        {

            _testdataFilePath = testdataFilePath;
            _inputSetMapDt = inputSetMapDt;
            _testSize = testSize;
            _SetProbAry = SetProbAry;
            _sut = sut;
        }
        public void fulltest()
        {
            for (int i = 0; i < 512; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    testFileForMiLu("bestMove", i * 512 + j, new int[] { i, j }, _testdataFilePath);
                }
            }
        }

        public void testDataGenerationBestMove(string rootPath)
        {
            string stra = rootPath.Remove(rootPath.Length - 1, 1);
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
                AccumulateSetProbArray[i] = _SetProbAry[i] + previous;
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

        internal void testDataGenerationNichneu(string rootPath, int[] binSetup)
        {
            List<string> generatedList = new List<string>();

            double[] AccumulateSetProbArray = new double[_SetProbAry.Length];
            for (int i = 0; i < _SetProbAry.Length; i++)
            {
                double previous = i == 0 ? 0 : AccumulateSetProbArray[i - 1];
                AccumulateSetProbArray[i] = _SetProbAry[i] + previous;
            }

            int retries = 0;
            while (generatedList.Count < _testSize)
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
                var binIndexesOfSetNum = binSetup.Select((x, i) =>
                {
                    if (x == setNum)
                    {
                        return i;
                    }
                    return -1;
                }).Where(x => x != -1).ToList();
                int rndBinIndex = GlobalVar.rnd.Next(0, binIndexesOfSetNum.Count);
                int selectedBinIndex = binIndexesOfSetNum[rndBinIndex];
                int lowBound = _sut.numOfMinIntervalInAllDim * (selectedBinIndex);
                int highBound = _sut.numOfMinIntervalInAllDim * (selectedBinIndex + 1);

                int num = GlobalVar.rnd.Next(lowBound, highBound);
                int[] inputs = new int[8];
                for (int j = 7; j >= 0; j--)
                {
                    int a = num / (int)Math.Pow(3, j);
                    num = num % (int)Math.Pow(3, j);
                    inputs[j] = a;
                }
                var listStr = inputs.Select(x => x.ToString());
                string inputStr = null;
                foreach (string s in listStr)
                {
                    inputStr += s + ";";
                }
                if (generatedList.Where(t => (t == inputStr) == true).Count() == 0)
                {
                    generatedList.Add(inputStr);
                    testFileForMiLu(null, -1, inputs, _testdataFilePath);
                    retries = 0;
                    Console.WriteLine("Num Of Test cases: {0}", generatedList.Count);
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
            string str = "";
            List<string> lsr = new List<string>();
            if (functionName == null || i == -1)
            {
                for (int j = 0; j < inputsAry.Length; j++)
                {
                    str += inputsAry[j].ToString() + @",";
                }
                str = str.Remove(str.Length - 1);
                Console.WriteLine(str);
                lsr = new List<string>();
                lsr.Add(str);
                lfa.StoreListToLinesAppend(path, lsr);
                return;
            }
            string str1 = string.Format(@"int test_{0}_{1}() {{return {0}(", functionName, i);
            string str3 = string.Format(@");}}");
            string str2 = null;
            for (int j = 0; j < inputsAry.Length; j++)
            {
                str2 += inputsAry[j].ToString() + @", ";
            }
            str2 = str2.Remove(str2.Length - 2, 2);
            str = str1 + str2 + str3;
            Console.WriteLine(str);
            lsr = new List<string>();
            lsr.Add(str);
            lfa.StoreListToLinesAppend(path, lsr);
        }



        internal void testDataGenerationQuickSort(string rootPath, int[] binSetup)
        {
            List<string> generatedList = new List<string>();

            double[] AccumulateSetProbArray = new double[_SetProbAry.Length];
            for (int i = 0; i < _SetProbAry.Length; i++)
            {
                double previous = i == 0 ? 0 : AccumulateSetProbArray[i - 1];
                AccumulateSetProbArray[i] = _SetProbAry[i] + previous;
            }

            int retries = 0;
            while (generatedList.Count < _testSize)
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
                var binIndexesOfSetNum = binSetup.Select((x, i) =>
                {
                    if (x == setNum)
                    {
                        return i;
                    }
                    return -1;
                }).Where(x => x != -1).ToList();
                int rndBinIndex = GlobalVar.rnd.Next(0, binIndexesOfSetNum.Count);
                int selectedBinIndex = binIndexesOfSetNum[rndBinIndex];
                int lowBound = _sut.numOfMinIntervalInAllDim * (selectedBinIndex);
                int highBound = _sut.numOfMinIntervalInAllDim * (selectedBinIndex + 1);

                int num = GlobalVar.rnd.Next(lowBound, highBound);
                int[] inputs = new int[20];
                for (int j = 19; j >= 0; j--)
                {
                    int a = num / (int)Math.Pow(2, j);
                    num = num % (int)Math.Pow(2, j);
                    inputs[j] = a;
                }
                var listStr = inputs.Select(x => x.ToString());
                string inputStr = null;
                foreach (string s in listStr)
                {
                    inputStr += s + ";";
                }
                if (generatedList.Where(t => (t == inputStr) == true).Count() == 0)
                {
                    generatedList.Add(inputStr);
                    testFileForMiLu(null, -1, inputs, _testdataFilePath);
                    retries = 0;
                    Console.WriteLine("Num Of Test cases: {0}", generatedList.Count);
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


        internal void testDataGenerationMatrixInverse(string rootPath, int[] binSetup)
        {
            List<string> generatedList = new List<string>();

            double[] AccumulateSetProbArray = new double[_SetProbAry.Length];
            for (int i = 0; i < _SetProbAry.Length; i++)
            {
                double previous = i == 0 ? 0 : AccumulateSetProbArray[i - 1];
                AccumulateSetProbArray[i] = _SetProbAry[i] + previous;
            }

            int retries = 0;
            while (generatedList.Count < _testSize)
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
                var binIndexesOfSetNum = binSetup.Select((x, i) =>
                {
                    if (x == setNum)
                    {
                        return i;
                    }
                    return -1;
                }).Where(x => x != -1).ToList();
                int rndBinIndex = GlobalVar.rnd.Next(0, binIndexesOfSetNum.Count);
                int selectedBinIndex = binIndexesOfSetNum[rndBinIndex];
                int lowBound = _sut.numOfMinIntervalInAllDim * (selectedBinIndex);
                int highBound = _sut.numOfMinIntervalInAllDim * (selectedBinIndex + 1);

                int num = GlobalVar.rnd.Next(lowBound, highBound);
                int[] inputs = new int[9];
                for (int j = 8; j >= 0; j--)
                {
                    int a = num / (int)Math.Pow(7, j);
                    num = num % (int)Math.Pow(7, j);
                    inputs[j] = a-3;
                }
                var listStr = inputs.Select(x => x.ToString());
                string inputStr = null;
                foreach (string s in listStr)
                {
                    inputStr += s + ";";
                }
                if (generatedList.Where(t => (t == inputStr) == true).Count() == 0)
                {
                    generatedList.Add(inputStr);
                    testFileForMiLu(null, -1, inputs, _testdataFilePath);
                    retries = 0;
                    Console.WriteLine("Num Of Test cases: {0}", generatedList.Count);
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


        internal void testDataGenerationTriangle(string rootPath, int[] binSetup)
        {
            List<string> generatedList = new List<string>();

            double[] AccumulateSetProbArray = new double[_SetProbAry.Length];
            for (int i = 0; i < _SetProbAry.Length; i++)
            {
                double previous = i == 0 ? 0 : AccumulateSetProbArray[i - 1];
                AccumulateSetProbArray[i] = _SetProbAry[i] + previous;
            }

            int retries = 0;
            while (generatedList.Count < _testSize)
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
                var binIndexesOfSetNum = binSetup.Select((x, i) =>
                {
                    if (x == setNum)
                    {
                        return i;
                    }
                    return -1;
                }).Where(x => x != -1).ToList();
                int rndBinIndex = GlobalVar.rnd.Next(0, binIndexesOfSetNum.Count);
                int selectedBinIndex = binIndexesOfSetNum[rndBinIndex];
                int lowBound = _sut.numOfMinIntervalInAllDim * (selectedBinIndex);
                int highBound = _sut.numOfMinIntervalInAllDim * (selectedBinIndex + 1);

                int num = GlobalVar.rnd.Next(lowBound, highBound);
                int[] inputs = new int[3];
                for (int j = 2; j >= 0; j--)
                {
                    int a = num / (int)Math.Pow(10, j);
                    num = num % (int)Math.Pow(10, j);
                    inputs[j] = a - 3;
                }
                var listStr = inputs.Select(x => x.ToString());
                string inputStr = null;
                foreach (string s in listStr)
                {
                    inputStr += s + ";";
                }
                if (generatedList.Where(t => (t == inputStr) == true).Count() == 0)
                {
                    generatedList.Add(inputStr);
                    testFileForMiLu(null, -1, inputs, _testdataFilePath);
                    retries = 0;
                    Console.WriteLine("Num Of Test cases: {0}", generatedList.Count);
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

    }

}
