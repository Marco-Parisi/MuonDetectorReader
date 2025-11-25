using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static MuonDetectorReader.Graph;

namespace MuonDetectorReader
{
    /// <summary>
    /// Logica di interazione per Fluxgate.xaml
    /// </summary>
    public partial class Fluxgate : Window
    {
        List<DateTime> Dates = new List<DateTime>();
        List<double> U_Count = new List<double>();
        List<double> L_Count = new List<double>();
        List<double> Diff = new List<double>();

        public Fluxgate()
        {
            InitializeComponent();
        }

        private void FluxgateClick(object sender, RoutedEventArgs e)
        {
            if (MainPanel != null)
            {
                if (MainPanel.Children.Count >= 3)
                    MainPanel.Children.RemoveAt(2);
            }

            string path;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "file txt |*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    path = openFileDialog.FileName;

                    ParseTXT(path);
                    Graph_Click(new Button() { Tag = "Diff" }, null);
                    GraphPanel.IsEnabled = true;
                    MainWindow.FileName = path.Remove(0, path.LastIndexOf("\\") + 1);
                }
                catch
                {
                    MessageTextblock.Text = "Errore: Formato dei dati incompatibile.";
                }
            }
            else
                GraphPanel.IsEnabled = false;
        }

        private static readonly Regex _regexFile = new Regex("[a-z]+");
        private void ParseTXT(string path)
        {
            StreamReader sr = new StreamReader(path);
            string str;

            Dates.Clear();
            U_Count.Clear();
            L_Count.Clear();
            Diff.Clear();

            do
            {
                str = sr.ReadLine();
                int c = str.LastIndexOf("*");

                if (str.Contains(" * ") && (str.Contains("/") || str.Contains("-")) && !_regexFile.IsMatch(str))
                {
                    /*if (str.Contains("-"))
                        str = str.Replace("-", "/");
                    */
                    if (str.Contains("   "))
                        str = str.Replace("   ", "");

                    Dates.Add(Convert.ToDateTime(str.Remove(str.IndexOf("*") - 1)));

                    str = str.Remove(0, str.IndexOf("*") + 1);
                    if (str.IndexOf("*") == -1)
                        U_Count.Add(Convert.ToDouble(str));
                    else
                    {
                        U_Count.Add(Convert.ToDouble(str.Remove(str.IndexOf("*"))));

                        str = str.Remove(0, str.IndexOf("*") + 1);
                        L_Count.Add(Convert.ToDouble(str.Remove(str.IndexOf("*"))));

                        str = str.Remove(0, str.IndexOf("*") + 1);
                        Diff.Add(Convert.ToDouble(str));

                        if (str.IndexOf("*") != -1)
                            throw new Exception("Formato errato");
                    }
                }

            } while (!sr.EndOfStream);

            sr.Close();

            if (Dates.Count == 0 || U_Count.Count == 0 )
                throw new Exception("Formato errato");
        }

        private void Graph_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag.ToString();

            if (MainPanel != null)
            {
                if (MainPanel.Children.Count >= 3)
                    MainPanel.Children.RemoveAt(2);
            }

            if (tag == "Upper")
            {
                MainWindow.GraphTitle = "Upper Count";

                MainPanel.Children.Add(GraphData(Dates, U_Count, Colors.DodgerBlue, MainWindow.GraphTitle));
                Grid.SetRow(MainPanel.Children[2], 1);
            }
            else if (tag == "Lower")
            {
                MainWindow.GraphTitle = "Lower Count";

                MainPanel.Children.Add(GraphData(Dates, L_Count, Colors.DarkOrange, MainWindow.GraphTitle));
                Grid.SetRow(MainPanel.Children[2], 1);

            }
            else if (tag == "Diff")
            {
                MainWindow.GraphTitle = "Diff Count";

                MainPanel.Children.Add(GraphData(Dates, Diff, Colors.MediumSeaGreen, MainWindow.GraphTitle));
                Grid.SetRow(MainPanel.Children[2], 1);

            }
        }

        private void ExportGraph_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string date = DateTime.Now.ToString("dd'-'MM-yyyy' 'HH'h'mm'm'ss's'");
                string folder;
                path += "\\Fluxgate - Grafici";
                Directory.CreateDirectory(path);
                folder = path;
                path += "\\" + "Fluxgate_" + MainWindow.GraphTitle + " " + date + ".png";

                PngExporter pngExporter = new PngExporter { Width = 1600, Height = 900, Background = OxyColors.White };

                PlotModel pm = ((MainPanel.Children[2] as Grid).Children[0] as PlotView).Model;

                pngExporter.ExportToFile(pm, path);

                MessageBox.Show("Grafico esportato correttamente nella cartella \"Fluxgate - Grafici\" sul Desktop.\n \n" + path, "Esporta Grafico");

                System.Diagnostics.Process.Start(folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
    }
}
