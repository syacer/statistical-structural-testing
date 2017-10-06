using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadSUTBranchCEData;

namespace GADEApproach
{
    static class SUT
    {
        public static double[] RunSUT(string sut, params double[] num)
        {
            int[] outputs = null;
            readBranch rb = new readBranch();
            int[] inputs = Array.ConvertAll(num, n => (int)n);

            if (sut == "tri")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 0);
            }
            if (sut == "gcd")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 1);
            }
            if (sut == "calday")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 2);
            }
            if (sut == "bestmove")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 3);
            }
            if (sut == "neichneu")
            {
                rb.ReadBranchCLIFunc(inputs, ref outputs, 4);
            }
            return Array.ConvertAll(outputs, n => (double)n);
        }
    }
}
