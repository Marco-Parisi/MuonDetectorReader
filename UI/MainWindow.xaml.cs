using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using DateTimePicker = Xceed.Wpf.Toolkit.DateTimePicker;
using static MuonDetectorReader.Graph;
using System.Threading.Tasks;
using MuonDetectorReader.Utils;

namespace MuonDetectorReader
{ 

    public partial class MainWindow : Window
    {

        List<double> Temp = new List<double>();
        List<double> Press = new List<double>();
        List<double> RawCounts = new List<double>();
        List<double> oldRawCounts = new List<double>();
        List<DateTime> Dates = new List<DateTime>();
        List<double> CorrCounts = new List<double>();
        List<double> FullCorrCounts = new List<double>();

        public static string GraphTitle = "Grafico";
        public static string ActiveGraph = "";

        string SettingFilePath;

        public static string FileName;

        public MainWindow()
        {
            InitializeComponent();

            SettingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CRD1 Reader\\Settings.xml");

            OpenSettingsFile();
        }

        private void PressBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender != null)
            {
                try
                {
                    string str = (sender as TextBox).Text;
                    double numstr = Convert.ToDouble(str);

                    AddPressToSettingsFile();
                }
                catch {}
            }
        }
        private void PressBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Space) || e.Key.Equals(Key.Enter))
            {
                e.Handled = true;
            }
        }
        private static readonly Regex _regex = new Regex(@"[^0-9,\,]+");
        private void PressBoxInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            string path;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "file txt |*.txt";

            if (openFileDialog.ShowDialog().Value)
            {
                path = openFileDialog.FileName;

                try
                {
                    if (MainGrid != null)
                    {
                        if (MainGrid.Children.Count >= 4)
                        {

                            MainGrid.Children.RemoveAt(3);
                            MessageTextblock.Text = "";
                        }
                    }

                    ParseTXT(path);
                    FileName = path.Remove(0, path.LastIndexOf("\\") + 1);

                    MessageTextblock.Text = "File aperto con successo. Calcola Beta per mostrare i grafici.";

                    Dates.Reverse();
                    Temp.Reverse();
                    Press.Reverse();
                    RawCounts.Reverse();

                    DataLostCheck();

                    RemoveDuplicates();

                    oldRawCounts = new List<double>(RawCounts);

                    if (BetaBox.Text != "nessuno" && double.TryParse(PressBox.Text, out double refPress))
                    {
                        PmP0.Clear();
                        Press.ForEach(point => PmP0.Add(point - refPress));

                        GenerateCorrCounts();
                        GraficiPanel.IsEnabled = true;
                        ShowHideData.IsChecked = false;

                        DateTo.Value = DateFrom.Value = null;

                        DateTo.Minimum = DateFrom.Minimum = Dates.Last();
                        DateTo.Maximum = DateFrom.Maximum = Dates.First();

                        Graph_Click(new Button() { Tag = "CC" }, null);

                        //DateTo.DisplayDateStart = DateFrom.DisplayDateStart = Dates.Last();
                        //DateTo.DisplayDateEnd = DateFrom.DisplayDateEnd = Dates.First();

                        //DatePickerResetDate();

                    }

                    BetaPanel.IsEnabled = true;
                    ShowHideData.IsEnabled = true;

                }
                catch// (Exception ex)
                {

                    BetaPanel.IsEnabled = false;
                    GraficiPanel.IsEnabled = false;
                    ExportPanel.IsEnabled = false;
                    ShowHideData.IsEnabled = false;

                    MessageTextblock.Text = "Cliccare su Apri File per leggere il file di dati.\n" +
                                            // "Cliccare su Media File per effettuare la media dei dati di 2 o più rilevatori. \n" +
                                            "(Se si vuole stimare Beta, utilizzare un file con almeno 2 mesi di dati)";

                    //MessageTextblock.Text = "Errore: Formato dei dati incompatibile.";
                    MessageBox.Show("Formato dei dati incompatibile", "Errore");
                }
            }
            //else
            //{
            //    MessageTextblock.Text = "Errore : File non supportato.";
            //    //MessageBox.Show("File non supportato", "Errore");
            //}

        }

        private Task DataLostCheck()
        {   
            TimeSpan cadence = new TimeSpan(1, 0, 0); // Tempo di integrazione 1 ora
            TimeSpan timediff = new TimeSpan();

            List<string> LostData = new List<string>();

            for (int i = 0; i < Dates.Count - 1; i++)
            {
                timediff = Dates[i] - Dates[i + 1];
                if (timediff > cadence)
                {
                    if(Dates[i].Date - Dates[i + 1].Date == new TimeSpan())
                        LostData.Add("• Il " + Dates[i + 1].ToString("dd/MM/yyyy 'dalle ore' HH:mm") + Dates[i].ToString(" 'alle ore' HH:mm"));
                    else
                        LostData.Add("• Il " + Dates[i + 1].ToString("dd/MM/yyyy 'ore' HH:mm") + " e il " + Dates[i].ToString("dd/MM/yyyy 'ore' HH:mm"));
                    LostData[LostData.Count - 1] += " → " + timediff.TotalHours.ToString() + " ore";
                }
            }

            if(LostData.Count > 0) 
            {
                string LostedData = "Sono stati trovati dei dati mancanti:\n\n";

                foreach (string str in LostData) 
                    LostedData += str + "\n";

                LostedData += "\nNome del file:\n" + FileName;

                Task.Factory.StartNew( () => MessageBox.Show(LostedData, "Attenzione"));
            }

            return Task.CompletedTask;
        }

        private void ExportGraph_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string date = DateTime.Now.ToString("dd'-'MM-yyyy' 'HH'h'mm'm'ss's'");
                string folder;
                path += "\\Moun Detector Reader - Grafici";
                Directory.CreateDirectory(path);
                folder = path;
                path += "\\"+ GraphTitle + " - " + date + ".png";

                PngExporter pngExporter = new PngExporter { Width = 1600, Height = 900, Background = OxyColors.White };

                PlotModel pm = ((MainGrid.Children[3] as Grid).Children[0] as PlotView).Model;

                pngExporter.ExportToFile(pm, path);

                MessageBox.Show("Grafico esportato correttamente nella cartella \"Moun Detector Reader - Grafici\" sul Desktop.\n \n" + path, "Esporta Grafico");

                System.Diagnostics.Process.Start(folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void ExportFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string date = DateTime.Now.ToString("dd'-'MM-yyyy' 'HH'h'mm'm'ss's'");
                string folder;
                path += "\\Moun Detector Reader - Grafici";
                Directory.CreateDirectory(path);
                folder = path;
                path += "\\" + "Moun Detector Reader Data Export - " + date + ".txt";

                StreamWriter sw = new StreamWriter(path);

                sw.WriteLine("Export dei dati: P0 = {0} ; Beta = {1}", refPress, Beta);

                if (OutlierBox.IsChecked == true)
                    sw.WriteLine("Rimozione Outliers: Attiva (" + OutlierSlider.Value.ToString("F") + "σ)");
                else
                    sw.WriteLine("Rimozione Outliers: Disattiva");

                sw.WriteLine("File di origine => " + FileName);
                sw.WriteLine("Formato dei dati : ");
                sw.WriteLine("Data\tTemp\tPress\tCont.Grezzi\tCont.Corr. P\tCont.Corr. PT");
                sw.WriteLine();

                int i = CorrCounts.Count - 1;
                for (; i >= 0; i--)
                    sw.WriteLine(Dates[i] + "\t{0:.0}\t{1:.00}\t{2}\t{3}\t{4}", Temp[i], Press[i], RawCounts[i], Math.Round(CorrCounts[i], 0), Math.Round(FullCorrCounts[i], 0));

                sw.Close();

                MessageBox.Show("File esportato correttamente nella cartella \"Moun Detector Reader - Grafici\" sul Desktop.\n \n" + path, "Esporta File");

                System.Diagnostics.Process.Start(folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void Graph_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag.ToString();

            if (MainGrid.Children.Count >= 4)
                MainGrid.Children.RemoveAt(3);

            TempCorrBox.IsEnabled = false;
            OutlierBox.IsEnabled = false;

            if(OutlierBox.IsChecked == true)
                OutlierSlider.IsEnabled = true;
            else
                OutlierSlider.IsEnabled = false;

            switch (tag)
            {
                case "Beta":
                    if (double.TryParse(PressBox.Text, out double p0))
                    {
                        try
                        {
                            GraphTitle = "Stima di Beta";
                            MainGrid.Children.Add(CalcBeta(Press, RawCounts, p0));
                            GenerateCorrCounts();
                            GraficiPanel.IsEnabled = true;
                            ShowHideData.IsEnabled = false;

                            BetaBox.Text = Beta.ToString("0.000000");
                            AddBetaToSettingsFile();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Errore nel calcolo di Beta, impossibile continuare.");
                            Close();
                        }
                    }
                    else
                        MessageBox.Show("Inserire un valore valido di Pressione");
                    OutlierBox.IsEnabled = true;
                    DatePickerPanel.IsEnabled = AvgSlider.IsEnabled = false;
                    break;
                case "CG":
                    GraphTitle = "Conteggi Grezzi";
                    MainGrid.Children.Add(GraphData(Dates, RawCounts, Color.FromArgb(180, 0, 120, 0), "Conteggi", SmoothValue.Text == "OFF"? false : true, (uint)AvgSlider.Value));;
                    OutlierBox.IsEnabled = DatePickerPanel.IsEnabled = AvgSlider.IsEnabled = true;
                    if((uint)AvgSlider.Value != AvgSlider.Minimum)
                        ShowHideData.IsEnabled = true;
                    break;
                case "CC":
                    if (TempCorrBox.IsChecked == false)
                    {
                        GraphTitle = "Conteggi Corretti in Pressione";
                        MainGrid.Children.Add(GraphData(Dates, CorrCounts, Color.FromArgb(150, 50, 110, 200), "Conteggi", SmoothValue.Text == "OFF" ? false : true, (uint)AvgSlider.Value, HorLine:true));
                    }
                    else
                    {
                        GraphTitle = "Conteggi Corretti in Pressione e Temp.";
                        MainGrid.Children.Add(GraphData(Dates, FullCorrCounts, Color.FromArgb(150, 50, 110, 200), "Conteggi", SmoothValue.Text == "OFF" ? false : true, (uint)AvgSlider.Value, HorLine: true));
                    }

                    OutlierBox.IsEnabled = DatePickerPanel.IsEnabled = TempCorrBox.IsEnabled = AvgSlider.IsEnabled = true;
                    if((uint)AvgSlider.Value != AvgSlider.Minimum)
                        ShowHideData.IsEnabled = true;
                    break;
                case "SIGMA":
                    GraphTitle = "Scarto dei Conteggi Corr. in Pressione";
                    MainGrid.Children.Add(SigmaTwo(Dates, CorrCounts));
                    OutlierBox.IsEnabled = DatePickerPanel.IsEnabled = true;
                    AvgSlider.IsEnabled = false;
                    ShowHideData.IsEnabled = false;
                    break;
                case "Temp":
                    DG_T.IsChecked = true;
                    DoubleGraphOK_Click(null, null);
                    DG_T.IsChecked = false;
                    break;
                case "Press":
                    DG_P.IsChecked = true;
                    DoubleGraphOK_Click(null, null);
                    DG_P.IsChecked = false;
                    break;
                default: return;
            }
            ActiveGraph = tag;
            DatePickerResetDate();

            if (MainGrid.Children.Count >= 4)
            {
                ExportPanel.IsEnabled = true;
                Grid.SetRow(MainGrid.Children[3], 1);
            }
        }

        private void DoubleGraph_Click(object sender, RoutedEventArgs e)
        {
            if (DoubleGraphPanel.Visibility == Visibility.Collapsed)
                DoubleGraphPanel.Visibility = Visibility.Visible;
            else
                DoubleGraphPanel.Visibility = Visibility.Collapsed;
        }

        private void DoubleGraphOK_Click(object sender, RoutedEventArgs e)
        {
            if (DG_P.IsChecked == true || DG_T.IsChecked == true)
            {
                if (MainGrid.Children.Count >= 4)
                    MainGrid.Children.RemoveAt(3);

                TempCorrBox.IsEnabled = false;
                AvgSlider.IsEnabled = false;
                ShowHideData.IsEnabled = false;
                OutlierSlider.IsEnabled = OutlierBox.IsEnabled = false;

                if (DG_CG.IsChecked == true)
                {
                    GraphTitle = DG_P.IsChecked == true ? "ContGrezzi_vs_Press" : "ContGrezzi_vs_Temp";
                    MainGrid.Children.Add(GraphTwoData(Dates, RawCounts, DG_P.IsChecked == true ? Press : Temp, "Conteggi", DG_P.IsChecked == true ? "Pressione" : "Temperatura", Color.FromArgb(180, 0, 120, 0), DG_P.IsChecked == true ? Colors.Orange : Colors.DarkMagenta, data2Div: DG_T.IsChecked == true ? 5 : 50));                   
                }
                else if (DG_CCP.IsChecked == true)
                {
                    GraphTitle = DG_P.IsChecked == true ? "ContCorrettiPress_vs_Press" : "ContCorrettiPress_vs_Temp";
                    MainGrid.Children.Add(GraphTwoData(Dates, CorrCounts, DG_P.IsChecked == true ? Press : Temp, "Conteggi", DG_P.IsChecked == true ? "Pressione" : "Temperatura", Color.FromArgb(120, 50, 110, 200), DG_P.IsChecked == true ? Colors.Orange : Colors.DarkMagenta, data2Div: DG_T.IsChecked == true ? 5 : 50));
                }
                else if (DG_CCPT.IsChecked == true)
                {
                    GraphTitle = DG_P.IsChecked == true ? "ContCorrettiPressTemp_vs_Press" : "ContCorrettiPressTemp_vs_Temp";
                    MainGrid.Children.Add(GraphTwoData(Dates, FullCorrCounts, DG_P.IsChecked == true ? Press : Temp, "Conteggi", DG_P.IsChecked == true ? "Pressione" : "Temperatura", Color.FromArgb(120, 50, 110, 200), DG_P.IsChecked == true ? Colors.Orange : Colors.DarkMagenta, data2Div: DG_T.IsChecked == true ? 5 : 50));
                }
                else
                {
                    GraphTitle = DG_P.IsChecked == true ? "Pressione" : "Temperatura";
                    MainGrid.Children.Add(GraphData(Dates, DG_P.IsChecked == true ? Press : Temp, DG_P.IsChecked == true ? Colors.Orange : Colors.DarkMagenta, GraphTitle, false, Div: DG_T.IsChecked == true ? 5 : 50));
                }

                if (MainGrid.Children.Count >= 4)
                {
                    ExportPanel.IsEnabled = true;
                    Grid.SetRow(MainGrid.Children[3], 1);
                }

                DatePickerResetDate();
            }
            else if (DG_CG.IsChecked == true || DG_CCP.IsChecked == true || DG_CCPT.IsChecked == true)
            {
                TempCorrBox.IsChecked = DG_CCPT.IsChecked;
                Graph_Click(new Button() { Tag = DG_CG.IsChecked == true ? "CG" : "CC" }, null);
            }
            
            if (sender != null)
                DoubleGraph_Click(null, null);

        }

        private void TempCorrBox_Click(object sender, RoutedEventArgs e)
        {

            //Button buttSim = new Button() { Tag = "CC" };
            //Graph_Click(buttSim, null);

            if (sender != null && MainGrid.Children.Count >= 4)
            {
                PlotView pv = ((MainGrid.Children[3] as Grid).Children[0] as PlotView);

                if (TempCorrBox.IsChecked == true)
                {
                    pv.Model.Title = GraphTitle = "Conteggi Corretti in Pressione e Temp.";
                    pv.Model.Series.Insert(0, UpdateTemperatureCorr(Dates, FullCorrCounts, pv));
                }
                else
                {
                    pv.Model.Title = GraphTitle = "Conteggi Corretti in Pressione";
                    pv.Model.Series.Insert(0, UpdateTemperatureCorr(Dates, CorrCounts, pv));
                }
                pv.Model.Series.RemoveAt(1);

                List<DataPoint> dp = (pv.Model.Series.First() as FunctionSeries).Points;

                if (pv.Model.Annotations.Count > 0)
                {
                    pv.Model.Annotations.RemoveAt(0);
                    pv.Model.Annotations.Insert(0, Smoothed(dp, (uint)AvgSlider.Value));
                }

                pv.InvalidatePlot(true);

                if (ShowHideData != null)
                {
                    if (ShowHideData.IsChecked == true)
                        ShowHideData_Click(1, null);
                }
            }

        }

        private void OutliersBox_Click(object sender, RoutedEventArgs e)
        {
            GenerateCorrCounts();
            Graph_Click(new Button() { Tag = ActiveGraph }, null);

            ShowHideData.IsChecked = false;

            if (OutlierBox.IsChecked == false)
                OutlierSlider.IsEnabled = false;
            else 
                OutlierSlider.IsEnabled = true;

        }

        private void OutlierSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OutlierSigmaValue.Text = OutlierSlider.Value.ToString("F") + "σ";

            if (OutlierBox.IsChecked == true)
            {
                GenerateCorrCounts();
                Graph_Click(new Button() { Tag = ActiveGraph }, null);
            }
        }

        private static readonly Regex _regexFile = new Regex("[a-z]+");
        private void ParseTXT(string path)
        {
            StreamReader sr = new StreamReader(path);
            string str;

            Temp.Clear();
            Press.Clear();
            RawCounts.Clear();
            Dates.Clear();
            CorrCounts.Clear();
            FullCorrCounts.Clear();

            do
            {
                    str = sr.ReadLine();
                if ((str.Contains(" * ") || str.Contains(",")) && (str.Contains("/") || str.Contains("-")) && !_regexFile.IsMatch(str))
                {
                    if (str.Contains("-"))
                        str = str.Replace("-", "/");

                    if (str.Contains("   "))
                        str = str.Replace("   ", "");

                    if (str.Contains(","))
                        str = str.Replace(",", " * ");

                    Dates.Add(Convert.ToDateTime(str.Remove(str.IndexOf("*")-1)));

                    str = str.Remove(0, str.IndexOf("*") + 1);
                    Temp.Add(Convert.ToDouble(str.Remove(str.IndexOf("*")).Replace(".", ",")));

                    str = str.Remove(0, str.IndexOf("*") + 1);
                    Press.Add(Convert.ToDouble(str.Remove(str.IndexOf("*")).Replace(".", ",")));

                    str = str.Remove(0, str.IndexOf("*") + 1);
                    if(str.Contains(".") || str.Contains(","))
                        RawCounts.Add((int)Convert.ToDouble(str.Replace(".", ",")));
                    else
                        RawCounts.Add(Convert.ToInt32(str));
                }

            } while (!sr.EndOfStream);

            sr.Close();

            if (Dates.Count == 0 || Temp.Count == 0 || Press.Count == 0 || RawCounts.Count == 0)
                throw new Exception("Formato errato");

        }


        private void RemoveDuplicates()
        {
            var seen = new HashSet<DateTime>();
            var duplicateIndexes = new List<int>();

            for (int i = 0; i < Dates.Count; i++)
            {
                if (!seen.Add(Dates[i]))
                {
                    duplicateIndexes.Add(i);
                }
            }

            foreach (var index in duplicateIndexes.OrderByDescending(i => i))
            {
                Dates.RemoveAt(index);
                Press.RemoveAt(index);
                Temp.RemoveAt(index);
                RawCounts.RemoveAt(index);
            }
        }


        public double GetTemperature(DateTime Date)
        {
            const double XC = -535250.20816;
            const double A = 10.28654;
            const double w = 203.63014;
            const double y0 = 20.93909;

            double dDate = Date.ToOADate() + 2415018.5; // Julian Day
            return y0 + A * Math.Sin(Math.PI * (dDate - XC) / w);
        }

        private void GenerateCorrCounts()
        {
            if (RawCounts.Count != 0 && PmP0.Count != 0)
            {
                if (OutlierBox.IsChecked == true)
                    RawCounts = OutlierRemover.RemoveOutliersSigma(oldRawCounts, OutlierSlider.Value); 
                else
                    RawCounts = new List<double>(oldRawCounts);

                CorrCounts.Clear();
                FullCorrCounts.Clear();

                double Temp_avg = Temp.Average();

                for (int i = 0; i < PmP0.Count; i++)
                    CorrCounts.Add(Math.Exp(-Beta * PmP0[i])  * RawCounts[i]);

                //TemperatureModel model = new TemperatureModel();
                List<double> TmT0 = new List<double>();

                List<DateTime> DT = new List<DateTime>();
                Dates.ForEach(date => DT.Add(Convert.ToDateTime(date)));


                double tempDayMean = 0;
                int meanCount = 0;
                for (int i = 0; i < DT.Count; i++)
                {
                    int ip1 = i < DT.Count - 1 ? i + 1 : i;

                    if (DT[i].Day == DT[ip1].Day)
                    {
                        tempDayMean += Temp[i];//model.GetTemperature(DT[i]);//
                        meanCount++;
                    }
                    else
                    {
                        tempDayMean /= meanCount;
                        for (int n = meanCount; n >= 0; n--)
                            TmT0.Add(Temp[i - n] - tempDayMean);

                        tempDayMean = meanCount = 0;
                    }
                }

                tempDayMean /= meanCount;
                int c = TmT0.Count;

                for (int n = meanCount-1; n >= 0; n--)
                    if (TmT0.Count < RawCounts.Count)
                        TmT0.Add(Temp[Math.Abs(c - n)] - tempDayMean);

                //for (int i = 0; i < CorrCounts.Count; i++)
                //    TmT0.Add(Temp[i] - Temp.Average());

                double kT = CalcTempCoeff(TmT0, RawCounts);

                for (int i = 0; i < CorrCounts.Count; i++)
               //     FullCorrCounts.Add(CorrCounts[i] * Math.Exp(-kT * TmT0[i]));
                FullCorrCounts.Add(CorrCounts[i] * ( 1-(kT * TmT0[i])));

                //DayCutter();


                //double avg = RawCounts.Average();
                //double avg_corr = CorrCounts.Average();
                //double avg_full = FullCorrCounts.Average();

                //for (int i = 0; i < PmP0.Count; i++)
                //{
                //    RawCounts[i] /= avg;
                //    CorrCounts[i] /= avg_corr;
                //    FullCorrCounts[i] /= avg_full;
                //}
            }
        }

        private void AvgSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender != null && MainGrid.Children.Count >= 4)
            {
                PlotView pv = ((MainGrid.Children[3] as Grid).Children[0] as PlotView);
                List<DataPoint> dp = (pv.Model.Series.First() as FunctionSeries).Points;

                if (pv.Model.Annotations.Count > 0)
                    pv.Model.Annotations.RemoveAt(0);

                if ((uint)AvgSlider.Value != AvgSlider.Minimum)
                {
                    pv.Model.Annotations.Insert(0, Smoothed(dp, (uint)AvgSlider.Value));


                    SmoothValue.Text = ((uint)AvgSlider.Value).ToString() + "pt";

                    if (ShowHideData != null)
                        if (ShowHideData.IsEnabled == false)
                            ShowHideData.IsEnabled = true;
                }
                else
                {

                    if (ShowHideData != null)
                    {
                        if (ShowHideData.IsChecked == true)
                        {
                            ShowHideData.IsChecked = false;
                            ShowHideData_Click(1, null);
                        }

                        if (ShowHideData.IsEnabled)
                            ShowHideData.IsEnabled = false;
                    }
                    SmoothValue.Text = "OFF";
                }
                pv.InvalidatePlot(true);
            }
        }


        private void ShowHideData_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && MainGrid.Children.Count >= 4)
            {
                PlotView pv = (MainGrid.Children[3] as Grid).Children[0] as PlotView;

                if (pv.Model.Series.Count > 0)
                  pv.Model.Series[0].IsVisible = !pv.Model.Series[0].IsVisible;

                pv.InvalidatePlot(true);
            }
        }

        private void OpenSettingsFile()
        {
            if (File.Exists(SettingFilePath))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(SettingFilePath);

                    PressBox.Text = xmlDoc.SelectSingleNode("Settings/RefPressure").InnerText;
                    refPress = Convert.ToDouble(PressBox.Text);

                    Beta = Convert.ToDouble(xmlDoc.SelectSingleNode("Settings/Beta").InnerText);
                    BetaBox.Text = Beta.ToString("0.000000"); 
                    
                }
                catch
                {
                    File.Delete(SettingFilePath);
                    CreateSettingsFile();
                }
            }
            else
                CreateSettingsFile();
        }

        private void CreateSettingsFile()
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement xmlSettings = xmlDoc.CreateElement("Settings");
            xmlDoc.AppendChild(xmlSettings);

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CRD1 Reader")))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CRD1 Reader"));

            xmlDoc.Save(SettingFilePath);

            AddPressToSettingsFile();
            AddBetaToSettingsFile();
        }

        private void AddPressToSettingsFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(SettingFilePath);

            XmlText xmlRefPresValue = xmlDoc.CreateTextNode(PressBox.Text);
            XmlNode xmlRefPres = xmlDoc.SelectSingleNode("Settings/RefPressure");

            if (xmlRefPres == null)
            {
                XmlNode xmlSettings = xmlDoc.SelectSingleNode("Settings");

                xmlRefPres = xmlDoc.CreateElement("RefPressure");
                xmlRefPresValue = xmlDoc.CreateTextNode(PressBox.Text);

                xmlRefPres.AppendChild(xmlRefPresValue); 
                xmlSettings.AppendChild(xmlRefPres);
            }
            else
            {
                xmlRefPres.InnerText = null;
                xmlRefPres.AppendChild(xmlRefPresValue);
            }
            
            xmlDoc.Save(SettingFilePath);
        }

        private void AddBetaToSettingsFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(SettingFilePath);

            XmlText xmlBetaValue = xmlDoc.CreateTextNode(Beta.ToString());
            XmlNode xmlBeta = xmlDoc.SelectSingleNode("Settings/Beta");

            if (xmlBeta == null)
            {
                XmlNode xmlSettings = xmlDoc.SelectSingleNode("Settings");

                xmlBeta = xmlDoc.CreateElement("Beta");
                xmlBetaValue = xmlDoc.CreateTextNode(PressBox.Text);

                xmlBeta.AppendChild(xmlBetaValue);
                xmlSettings.AppendChild(xmlBeta);

            }
            else
            {
                xmlBeta.InnerText = null;
                xmlBeta.AppendChild(xmlBetaValue);
            }

            xmlDoc.Save(SettingFilePath);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach(var child in ((sender as CheckBox).Parent as StackPanel).Children)
            {
                if((child as CheckBox) != (sender as CheckBox))
                    (child as CheckBox).IsChecked = false;
            }
        }

        private void DatePickerResetDate()
        {
            /*DateFrom.SelectedDateChanged -= DateFrom_SelectedDateChanged;
            DateTo.SelectedDateChanged -= DateTo_SelectedDateChanged;

            DateFrom.SelectedDate = DateFromOldSD = Dates.Last();
            DateTo.SelectedDate = DateToOldSD = Dates.First();

            DateFrom.SelectedDateChanged += DateFrom_SelectedDateChanged;
            DateTo.SelectedDateChanged += DateTo_SelectedDateChanged;*/

            DateFrom.ValueChanged -= DateFrom_SelectedDateChanged;
            DateTo.ValueChanged -= DateTo_SelectedDateChanged;

            DateFrom.Value = DateFromOldSD = Dates.Last();
            DateTo.Value = DateToOldSD = Dates.First();

            DateFrom.ValueChanged += DateFrom_SelectedDateChanged;
            DateTo.ValueChanged += DateTo_SelectedDateChanged;
        }


        DateTime DateFromOldSD;
        private void DateFrom_SelectedDateChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender != null && MainGrid.Children.Count >= 4)
            {
                //DateTime SD = (sender as CustomDatePicker).SelectedDate ?? new DateTime();
                DateTime SD = (sender as DateTimePicker).Value ?? new DateTime();

                if (SD > DateTo.Value)//.SelectedDate)
                {
                    MessageBox.Show("Intervallo date errato", "Attenzione");
                    return;
                }

                PlotView pv = ((MainGrid.Children[3] as Grid).Children[0] as PlotView);

                pv.Model.DefaultXAxis.Minimum = OxyPlot.Axes.DateTimeAxis.ToDouble(SD);
                pv.ResetAllAxes();
                (((pv.Parent as Grid).Children[1] as StackPanel).Children[0] as Button).Visibility = Visibility.Collapsed;
                (((pv.Parent as Grid).Children[1] as StackPanel).Children[1] as Button).Visibility = Visibility.Visible;
                pv.InvalidatePlot(true);

                DateFromOldSD = SD;
            }
        }

        DateTime DateToOldSD;
        private void DateTo_SelectedDateChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender != null && MainGrid.Children.Count >= 4)
            {
                //if (DateTo.SelectedDate != DateToOldSD)

                if (DateTo.Value != DateToOldSD)
                {
                    //DateTime SD = (sender as CustomDatePicker).SelectedDate ?? new DateTime();
                    DateTime SD = (sender as DateTimePicker).Value ?? new DateTime();

                    if (SD < DateFrom.Value)//.SelectedDate)
                    {
                        MessageBox.Show("Intervallo date errato", "Attenzione");
                        //DateTo.SelectedDate = DateToOldSD;
                        DateTo.Value = DateToOldSD;
                        return;
                    }

                    PlotView pv = ((MainGrid.Children[3] as Grid).Children[0] as PlotView);

                    pv.Model.DefaultXAxis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(SD);
                    pv.ResetAllAxes();
                    (((pv.Parent as Grid).Children[1] as StackPanel).Children[0] as Button).Visibility = Visibility.Collapsed;
                    (((pv.Parent as Grid).Children[1] as StackPanel).Children[1] as Button).Visibility = Visibility.Visible;
                    pv.InvalidatePlot(true);

                    DateToOldSD = SD;
                }
            }
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Fluxgate_Click(object sender, RoutedEventArgs e)
        {
            Fluxgate FluxgateWindow = new Fluxgate();
            FluxgateWindow.Show();

        }
    }
}
