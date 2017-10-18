using ReadSUTBranchCEData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
   static class PathCounts
    {
       public static void CountPath()
        {
            int[] outputs = null;
            readBranch rb = new readBranch();
            List<string> strPaths = new List<string>();
            for (int i = 0; i < 512; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    int[] input = new int[] { i, j };
                    rb.ReadBranchCLIFunc(input, ref outputs, 3);
                    string tmp = null;
                    for (int m = 0; m < outputs.Length; m++)
                    {
                        tmp = tmp + outputs[m].ToString() + " ";
                    }
                    if (!strPaths.Contains(tmp))
                    {
                        strPaths.Add(tmp);
                    }
                }
            }
            Console.WriteLine("# of Paths: {0}", strPaths.Count);
            List<int> lastCEList = new List<int>();

            for (int i = 0; i < strPaths.Count; i++)
            {
                int index = strPaths[i].LastIndexOf('1');
                if (!lastCEList.Contains(index))
                {
                    lastCEList.Add(index);
                }
            }
            Console.ReadKey();
        }
    }
}
