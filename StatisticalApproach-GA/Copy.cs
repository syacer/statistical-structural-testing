using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace StatisticalApproach_GA
{
    static class Copy
    {
        public static T DeepCopy<T>(T obj)
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
