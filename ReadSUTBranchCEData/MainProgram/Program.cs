using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadSUTBranchCEData;
namespace MainProgram
{
    // selectSUT = 0: Triangle SUT
    class Program
    {
        static void Main(string[] args)
        {
            int selectSUT = 1;
            if (selectSUT == 0)
            {
                GenTriangleData(selectSUT);
            }
            if (selectSUT == 1)
            {
                GenGCDData(1);
            }
            if (selectSUT == 2)
            {
                GenCalDay(2);
            }
        }
        static void GenCalDay(int selectSUT)
        {
            int var1L = 0;
            int var1H = 12;
            int var2L = 0;
            int var2H = 365;
            int var3L = 0;
            int var3H = 100;

            readBranch tmp = new readBranch();
            int[] inputs = new int[3] { 1, 2, 3 };
            int[] ces = null;
            LocalFileAccess lfa = new LocalFileAccess();
            List<string> dataToFile = new List<string>();

            for (int i = var1L; i <= var1H; i++)
            {
                for (int j = var2L; j <= var2H; j++)
                {
                    for (int k = var3L; k <= var3H; k++)
                    {
                        Console.WriteLine("{0},{1},{2}",var1H-i,var2H-j,var3H-k);
                        inputs[0] = i;
                        inputs[1] = j;
                        inputs[2] = k;
                        tmp.ReadBranchCLIFunc(inputs, ref ces, selectSUT);
                        string strData = null;
                        for (int l = 0; l < ces.Length; l++)
                        {
                            if (ces[l] == 1)
                            {
                                strData = i.ToString() + " " + j.ToString() + " " + k.ToString() + " " + l.ToString();
                                dataToFile.Add(strData);
                                lfa.StoreListToLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\calday", dataToFile);
                                dataToFile.Clear();
                            }
                        }
                    }
                }
            }
            
        }
        static void GenTriangleData(int selectSUT)
        {
            int var1L = 0;
            int var1H = 100;
            int var2L = 0;
            int var2H = 100;
            int var3L = 0;
            int var3H = 100;

            readBranch tmp = new readBranch();
            int[] inputs = new int[3] { 1, 2, 3 };
            int[] ces = null;
            LocalFileAccess lfa = new LocalFileAccess();
            List<string> dataToFile = new List<string>();

            for (int i = var1L; i <= var1H; i++)
            {
                for (int j = var2L; j <= var2H; j++)
                {
                    for (int k = var3L; k <= var3H; k++)
                    {
                        inputs[0] = i;
                        inputs[1] = j;
                        inputs[2] = k;
                        tmp.ReadBranchCLIFunc(inputs, ref ces, selectSUT);
                        string strData = null;
                        for (int l = 0; l < ces.Length; l++)
                        {
                            if (ces[l] == 1)
                            {
                                strData = i.ToString() + " " + j.ToString() + " " + k.ToString() + " " + l.ToString();
                                dataToFile.Add(strData);
                            }
                        }
                    }
                }
            }
            lfa.StoreListToLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\triangle", dataToFile);
        }
        static void GenGCDData(int selectSUT)
        {
            int var1L = 0;
            int var1H = 100;
            int var2L = 0;
            int var2H = 100;

            readBranch tmp = new readBranch();
            int[] inputs = new int[2] { 1, 2};
            int[] ces = null;
            LocalFileAccess lfa = new LocalFileAccess();
            List<string> dataToFile = new List<string>();
            
            for (int i = var1L; i <= var1H; i++)
            {
                for (int j = var2L; j <= var2H; j++)
                {
                        inputs[0] = i;
                        inputs[1] = j;
                        tmp.ReadBranchCLIFunc(inputs, ref ces, selectSUT);
                        string strData = null;
                        for (int l = 0; l < ces.Length; l++)
                        {
                            if(ces[l] == 1)
                            {
                                strData = i.ToString() + " " + j.ToString() + " " + " " + l.ToString();
                                dataToFile.Add(strData);
                            }
                        }
                }
            }
            lfa.StoreListToLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+@"\gcd", dataToFile);
        }
    }
}
