using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MuonDetectorReader
{
    /// <summary>
    /// Logica di interazione per AverageGraph.xaml
    /// </summary>
    public partial class AverageGraph : Window
    {
        public AverageGraph()
        {
            InitializeComponent();
        }

        private void PressBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender != null)
            {
                try
                {
                    string str = (sender as TextBox).Text;
                    double numstr = Convert.ToDouble(str);
                }
                catch { }
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
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.SafeFileName;

                try
                {
                    DataGridFiles.Items.Add(new { Filename = path });
                }
                catch
                {
                    MessageBox.Show("Formato del file corrotto", "Errore");
                }

            }
        }
    }
}
