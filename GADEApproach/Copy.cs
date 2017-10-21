using MathNet.Numerics.LinearAlgebra;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GADEApproach
{
    static class Copy
    {
        public static T DeepCopy<T>(T obj)
        {
            if (typeof(T).Name == typeof(Pair<int, double, Pair<int, int, double[]>[], Matrix<double>,double,double>).Name)
            {
                Pair<int, double, Pair<int, int, double[]>[], Matrix<double>,double,double> tmp =
                    new Pair<int, double, Pair<int, int, double[]>[], Matrix<double>,double,double>();
                Pair<int, double, Pair<int, int, double[]>[], Matrix<double>,double,double> input =
                    (Pair<int, double, Pair<int, int, double[]>[], Matrix<double>,double,double>)Convert.ChangeType(obj, typeof(T));
                tmp.Index = input.Index;
                tmp.ProbLowBound = input.ProbLowBound;
                tmp.binsSetup = Copy.DeepCopy(input.binsSetup);
                tmp.Variance = input.Variance;
                tmp.Rank = input.Rank;
                if (input.AMatrix == null)
                {
                    tmp.AMatrix = null;
                }
                else
                {
                    tmp.AMatrix = Matrix<double>.Build.Dense(input.AMatrix.RowCount,input.AMatrix.ColumnCount);
                    input.AMatrix.CopyTo(tmp.AMatrix);
                }
                return (T)(object)tmp;
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    ms.Position = 0;
                    return (T)formatter.Deserialize(ms);
                }
            }
        }
    }
}
