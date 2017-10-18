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
        public int numOfLabels = -1;
        public double[] lowbounds = null;
        public double[] highbounds = null;
        public int numOfMinIntervalX = -1;
        public int numOfMinIntervalY = -1;
        public Pair<int, int, double[]>[] bins; //index, setIndex, triggering probabilities

        public Tuple<int, int> Calxyindex(int binIndex)
        {
            int x = binIndex % numOfMinIntervalX;
            int y = binIndex / numOfMinIntervalX;
            return new Tuple<int, int>(x, y);
        }
    }
}
