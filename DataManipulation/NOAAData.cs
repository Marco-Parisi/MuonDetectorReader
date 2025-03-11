using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MuonDetectorReader
{
    public class NOAAData
    {
        private const string Bologna_StationID = "16144";
        //private const string Udine_StationID = "16045";
        //private const string Novara_StationID = "16064";

        //private NOAADataSet Bologna_DataSet;
        //private NOAADataSet Udine_DataSet;
        //private NOAADataSet Novara_DataSet;

        public NOAADataSet DataSet;


        public async void GetData()
        {
            DateTime TimeFrom = new DateTime(2020, 10, 5);
            DateTime TimeTo = new DateTime(2021, 4, 19);
            string timeFrom = TimeFrom.Year.ToString() + TimeFrom.Month.ToString("00") + TimeFrom.Day.ToString("00") + "00";
            string timeTo = TimeTo.Year.ToString() + TimeTo.Month.ToString("00") + TimeTo.Day.ToString("00") + "00";

            HttpClient client = new HttpClient();
            string uriStr = "https://ruc.noaa.gov/raobs/GetRaobs.cgi?shour=All+Times&ltype=All+Levels&wunits=Knots&bdate=" + timeFrom + "&edate=" + timeTo + "&access=WMO+Station+Identifier&view=NO&StationIDs=" + Bologna_StationID + "&osort=Station+Series+Sort&oformat=FSL+format+%28ASCII+text%29";
            client.BaseAddress = new Uri(uriStr);

            HttpResponseMessage HttpResponse = await client.GetAsync(client.BaseAddress);
            string ContentString = await HttpResponse.Content.ReadAsStringAsync();

            DataSet = ParseDataString(ContentString);
        }


        private NOAADataSet ParseDataString(string contentStr)
        {

            DataSet = new NOAADataSet();
            DataSet.Dates = contentStr.Split('\n').Where(s => s.Contains("    254      0") || s.Contains("    254     12")).Select(s =>
            {
                s = s.Replace("    254     ", "");
                var ele = s.Split(' ').Where(r => !r.Equals("")).ToList();
                int h = Convert.ToInt32(ele[0]);
                int d = Convert.ToInt32(ele[1]);
                int m = DateTime.ParseExact(ele[2],"MMM", CultureInfo.GetCultureInfo("en-GB")).Month;
                int y = Convert.ToInt32(ele[3]);
                DateTime dt1 = new DateTime(y, m, d, h, 0, 0);

                return dt1;

            }).ToList();

            List<string> multipleDataStr = contentStr.Split('\n').Where(s => s.Contains("4   1000")).Select(s =>
            {
                s = s.Replace("      4   1000  ", "");
                return s.Remove(s.LastIndexOf('-') + 4);
            }).ToList();

            multipleDataStr.ForEach(dataStr =>
            {
                try
                {
                    DataSet.Altitude.Add(Convert.ToDouble(dataStr.Remove(5)));
                    dataStr = dataStr.Replace(dataStr.Remove(5) + "   ", "");
                    DataSet.Temperature.Add(Convert.ToDouble(dataStr.Remove(4)) / 10);
                    dataStr = dataStr.Replace(dataStr.Remove(4) + "   ", "");
                    DataSet.DewPoint.Add(Convert.ToDouble(dataStr) / 10);
                }
                catch 
                {
                    DataSet.Altitude.Add(DataSet.Altitude.Last());
                    DataSet.Temperature.Add(DataSet.Temperature.Last());
                    DataSet.DewPoint.Add(DataSet.DewPoint.Last());
                }

            });
            return DataSet;
        }

        public class NOAADataSet
        {
            public List<double> Altitude = new List<double>();
            public List<double> Temperature = new List<double>();
            public List<double> DewPoint = new List<double>();
            public List<DateTime> Dates = new List<DateTime>();
        }
    } 
}
