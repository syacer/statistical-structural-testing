using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int[][] MapTestInputsToSets(Pair<int,double,Pair<int,int,double[]>[]> solution)
        {
            int[][] inputsMapping = new int[(int)(highbounds[0] - lowbounds[0] + 1)][];
            for (int i = 0; i < (int)(highbounds[0] - lowbounds[0] + 1); i++)
            {
                inputsMapping[i] = new int[(int)(highbounds[1] - lowbounds[1] + 1)];
            }
            int totalNumberOfBins = numOfMinIntervalX * numOfMinIntervalY;
            for (int i = 0; i < totalNumberOfBins; i++)
            {
                Tuple<int, int> indexs = Calxyindex(i);
                double deltaX = (highbounds[0] - lowbounds[0] + 1) / numOfMinIntervalX;
                double deltaY = (highbounds[1] - lowbounds[1] + 1) / numOfMinIntervalY;
                int lowX = (int)deltaX * indexs.Item1;
                int lowY = (int)deltaY * indexs.Item2;
                for (int x = lowX; x < lowX + deltaX; x++)
                {
                    for (int y = lowY; y < lowY + deltaY; y++)
                    {
                        inputsMapping[x][y] = solution.Item2[i].Item1;
                    }
                }
            }
            return inputsMapping;
        }
    }
}
