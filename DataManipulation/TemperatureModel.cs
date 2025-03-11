using System;

namespace MounDetectorReader
{
    class TemperatureModel
    {
        private const double XC = -535250.20816;
        private const double A = 10.28654;
        private const double w = 203.63014;
        private const double y0 = 20.93909;

        public double GetTemperature(DateTime Date)
        {
            double dDate = Date.ToOADate() + 2415018.5; // Julian Day
            return y0 + A * Math.Sin(Math.PI * (dDate - XC) / w);
        }
    }
}
