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
        List<double> Count = new List<double>();
        List<double> Voltage = new List<double>();

        string GraphTitle;

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

                    GraphTitle = "Conteggi";

                    MainPanel.Children.Add(GraphData(Dates, Count, Colors.DodgerBlue, GraphTitle));

                    Grid.SetRow(MainPanel.Children[2], 1);
                    GraphPanel.IsEnabled = true;
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
            Count.Clear();
            Voltage.Clear();

            do
            {
                str = sr.ReadLine();
                int c = str.LastIndexOf("*");

                if (str.Contains(" * ") && (str.Contains("/") || str.Contains("-")) && !_regexFile.IsMatch(str))
                {
                    if (str.Contains("-"))
                        str = str.Replace("-", "/");

                    if (str.Contains("   "))
                        str = str.Replace("   ", "");

                    Dates.Add(Convert.ToDateTime(str.Remove(str.IndexOf("*") - 1)));

                    str = str.Remove(0, str.IndexOf("*") + 1);
                    if (str.IndexOf("*") == -1)
                        Count.Add(Convert.ToDouble(str));
                    else
                    {
                        Count.Add(Convert.ToDouble(str.Remove(str.IndexOf("*"))));

                        str = str.Remove(0, str.IndexOf("*") + 1);
                        Voltage.Add(Convert.ToDouble(str.Replace(".", ",")));

                        if (str.IndexOf("*") != -1)
                            throw new Exception("Formato errato");
                    }
                }

            } while (!sr.EndOfStream);

            sr.Close();

            if (Dates.Count == 0 || Count.Count == 0 )
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

            if (tag == "Count")
            {
                GraphTitle = "Conteggi";

                MainPanel.Children.Add(GraphData(Dates, Count, Colors.DodgerBlue, GraphTitle));

                Grid.SetRow(MainPanel.Children[2], 1);
            }
            else if (tag == "Volt")
            {
                if (Voltage.Count == 0)
                    MessageTextblock.Text = "Voltaggio non presente nel file di dati.";
                else if (Voltage.Count != Dates.Count)
                    MessageTextblock.Text = "I dati sul Voltaggio sono incompleti.";
                else
                {
                    GraphTitle = "Volts";

                    MainPanel.Children.Add(GraphData(Dates, Voltage, Colors.DarkOrange, GraphTitle));
                    Grid.SetRow(MainPanel.Children[2], 1);
                }
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
                path += "\\" + "Fluxgate_" + GraphTitle + " " + date + ".png";

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
