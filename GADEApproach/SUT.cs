using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace GADEApproach
{
    public class SUT
    {
        public int sutIndex = -1;
        public int numOfLabels = -1;
        public double[] lowbounds = null;
        public double[] highbounds = null;
        public int numOfMinIntervalX = -1;   //This is only for bestMove
        public int numOfMinIntervalY = -1;   //This is only for bestMove
        public int numOfMinIntervalInAllDim;
        public Pair<int, int, double[]>[] bins; //index, setIndex, triggering probabilities

        public Tuple<int, int> Calxyindex(int binIndex)
        {
            int x = binIndex % numOfMinIntervalX;
            int y = binIndex / numOfMinIntervalX;
            return new Tuple<int, int>(x, y);
        }
    }
}
