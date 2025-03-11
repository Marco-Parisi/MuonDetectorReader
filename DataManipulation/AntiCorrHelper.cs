using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MounDetectorReader
{
    public class AntiCorrHelper
    {
        public List<double> GenerateAntiCorrelationData(List<double> Data1, List<double> Data2)
        {
            if (Data1.Count != Data2.Count)
                return null;

            double[] d1 = DataNormalization(Data1);
            double[] d2 = DataNormalization(Data2);
            double[] result = new double[d1.Length];

            for (int i = 0; i < d1.Length; i++)
                result[i] = Math.Abs(d1[i] - d2[i]);

            return result.ToList();//DataNormalization(result.ToList()).ToList();

        }

        public static double[] DataNormalization(List<double> Data)
        {
            double[] dataArray = new double[Data.Count];
            double dataMax = Data.Max();
            double dataMin = Data.Min();

            for (int i = 0; i < dataArray.Length; i++)
                dataArray[i] = (dataMin - Data[i]) / (dataMin - dataMax);

            return dataArray;
        }
    }
}
