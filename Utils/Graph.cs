﻿using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PlotView = OxyPlot.Wpf.PlotView;


namespace MuonDetectorReader
{
    public class Graph
    {

        public static double Beta = 0;
        public static double SigmaBeta = 0;
        public static List<double> PmP0 = new List<double>();
        private static List<double> _PmP0 = new List<double>();
        private static List<double> _logCount = new List<double>(); 

        public static double refPress;

        private static double yIntercept=0;

        public static Grid GraphData(List<DateTime> Dates, List<double> data1, Color color, string ParName, bool Smooth = false, uint Window = 3, int Div = 40, bool HorLine = false)
        {
            SolidColorBrush brC = new SolidColorBrush(color);

            OxyColor oxC = OxyColor.FromArgb(brC.Color.A, brC.Color.R, brC.Color.G, brC.Color.B);

            Grid grid = new Grid() { Margin = new Thickness(5) };

            Button resetButton = new Button()
            {
                Visibility = Visibility.Collapsed,
                Content = "Reset Grafico",
                Height = 25,
                Width = 80,
                Background = brC,
                Foreground = new SolidColorBrush(Colors.White)
            };

            Button stretchButton = new Button()
            {
                Content = "Stretch Grafico",
                Height = 25,
                Width = 90
            };

            StackPanel buttonsPanel = new StackPanel()
            {
                Margin = new Thickness(30, 30, 30, 5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                Orientation = Orientation.Horizontal

            };

            buttonsPanel.Children.Add(resetButton);
            buttonsPanel.Children.Add(stretchButton);

            PlotView chart1 = new PlotView()
            {
                //IsMouseWheelEnabled = false,
                Margin = new Thickness(10, 0, 20, 5),
                Foreground = new SolidColorBrush(Colors.Black),               
            };


            FunctionSeries fs = new FunctionSeries()
            {
                CanTrackerInterpolatePoints = false,
                Color = oxC,
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                MarkerSize = 1.5,
                MarkerFill = OxyColor.FromArgb(180, 0, 0, 0),
                MarkerStrokeThickness = 0,
                //MarkerStroke = OxyColor.FromArgb(200, 50, 50, 50),
            };


            int i = 0;

            foreach (DateTime date in Dates)
            {
                try
                {

                    fs.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data1[i]));
                    i++;
                }
                catch
                {
                     throw new FormatException();
                }

            }

            double fsPointsMean = fs.Points.Average(d => d.Y);
            double fsPointsMin, fsPointsMax;
            fsPointsMax = fs.Points.Max(p => p.Y);
            fsPointsMin = fs.Points.Min(p => p.Y);

            // Uso i per il numero di decimali da arrotondare
            if (ParName.Contains("Conteggi"))
                i = 0;
            else
                i = 2;

            PlotModel n = new PlotModel()
            {
                PlotType = PlotType.XY,
                PlotMargins = new OxyThickness(double.NaN, 0, double.NaN, double.NaN),
                LegendBackground = OxyColor.FromArgb(170,180,180,180),
                LegendTitleColor = OxyColors.Black,
                LegendTitleFontSize = 14,
                LegendTitleFont = "Arial",
                LegendTitle = "Media = " + Math.Round(fsPointsMean, i) + "\n Max = " + Math.Round(fsPointsMax, i) + "\n Min = " + Math.Round(fsPointsMin, i),
                LegendMaxHeight = 80,
                LegendMaxWidth = 170,
                LegendPosition = LegendPosition.TopRight,
                Title = MainWindow.GraphTitle,
                TitleFontSize = 14,
                Subtitle = "File: " + MainWindow.FileName,
            };


            LinearAxis yAxis = new LinearAxis()
            {
                Title = "",//ParName,        
                AxisTitleDistance = 20,
                IntervalLength = 30,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.Dot,
                Maximum = fsPointsMax + (fsPointsMax / Div),
                Minimum = fsPointsMin - (fsPointsMin / Div),
           
            };

            if (ParName.Contains("Conteggi"))
            {
                fs.TrackerFormatString = ParName + " : {Y:0.}";
                yAxis.LabelFormatter = (x) => ((x - data1.Average()) * 100 / data1.Average()).ToString("0.0") + "%";
            }
            else
                fs.TrackerFormatString = ParName + " : {Y:0.00}";


            fs.TrackerFormatString += Environment.NewLine + "Data : {2:yyyy/MM/dd}" + Environment.NewLine + "Ora: {2:HH:mm:ss}";

            DateTimeAxis xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "yyyy/MM/dd HH:mm",
                Title = "Data",
                AxisTitleDistance = 10,
                IntervalLength = 25,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.Dot,
                Angle = -35
            };

            xAxis.MinimumMajorStep = 0.002;


            xAxis.AxisChanged += (s, e) =>
            {
                resetButton.Dispatcher.Invoke(() => { resetButton.Visibility = Visibility.Visible; });
                stretchButton.Dispatcher.Invoke(() => { stretchButton.Visibility = Visibility.Collapsed; });
            };


            yAxis.AxisChanged += (s, e) =>
            {
                resetButton.Dispatcher.Invoke(() => { resetButton.Visibility = Visibility.Visible; });
                stretchButton.Dispatcher.Invoke(() => { stretchButton.Visibility = Visibility.Collapsed; });
            };

            resetButton.Click += (s, e) =>
            {
                if (s != null)
                {
                    PlotView p = grid.Children[0] as PlotView;
                    if (resetButton.Visibility == Visibility.Visible)
                    {
                        p.Model.Axes[1].Maximum = fsPointsMax + (fsPointsMax / Div);
                        p.Model.Axes[1].Minimum = fsPointsMin - (fsPointsMin / Div);
                        p.ResetAllAxes();
                        resetButton.Visibility = Visibility.Collapsed;
                        stretchButton.Visibility = Visibility.Visible;
                    }
                }
            };

            stretchButton.Click += (s, e) =>
            {
                if (s != null)
                {
                    PlotView p = grid.Children[0] as PlotView;

                    double YAxisActualMin = p.Model.Axes[0].ActualMinimum;
                    double YAxisActualMax = p.Model.Axes[0].ActualMaximum;
                    List<DataPoint> RangePoints = fs.Points.FindAll((d) => d.X >= YAxisActualMin && d.X <= YAxisActualMax);


                    double max = RangePoints.Max(d => d.Y);
                    double min = RangePoints.Min(d => d.Y);
                    double med = RangePoints.Average(d => d.Y);

                    RangePoints.Clear();

                    p.Model.Axes[1].Maximum = max + (med / 1000);
                    p.Model.Axes[1].Minimum = min - (med / 1000);

                    p.InvalidatePlot();

                    resetButton.Visibility = Visibility.Visible;
                    stretchButton.Visibility = Visibility.Collapsed;
                }
            };

            xAxis.MajorGridlineColor = xAxis.TicklineColor = yAxis.TicklineColor = yAxis.MajorGridlineColor = OxyColors.Gray;

            n.Series.Add(fs);

            LinearAxis yAxis2 = new LinearAxis();
            FunctionSeries fs2 = new FunctionSeries();

            if (Smooth)
            {
                n.Series.Add(AvgLine(fs.Points));
                n.Annotations.Add(Smoothed(fs.Points, Window));
            }

            n.Axes.Add(xAxis);
            n.Axes.Add(yAxis);

            if (HorLine)
            {

                fs2 = new FunctionSeries()
                {
                    CanTrackerInterpolatePoints = false,
                    Color = OxyColors.DarkGreen,
                    LineStyle = LineStyle.Solid,
                    MarkerSize = 0,
                    StrokeThickness = 2,
                    YAxisKey = "yAxis2",
                    TrackerFormatString = "Retta Mobile"
                };

                AvgLine(fs.Points).Points.ForEach(p => fs2.Points.Add(p));

                yAxis2 = new LinearAxis()
                {
                    Key = "yAxis2",
                    Position = AxisPosition.Right,
                    Title = "< < < < Retta Mobile > > > >",
                    Angle = 90,
                    AxisTitleDistance = -15,
                    IntervalLength = 20,
                    TextColor = OxyColors.White,
                    TicklineColor = OxyColors.White,
                    MajorGridlineStyle = LineStyle.None,
                    MinorGridlineStyle = LineStyle.None,
                    Minimum = fs2.Points.Min(p => p.Y) - (fs2.Points.Min(p => p.Y) / 200),
                    Maximum = fs2.Points.Max(p => p.Y) + (fs2.Points.Max(p => p.Y) / 200),
                };


                //n.Series.Add(LinearRegression(fs.Points.Select(dp => dp.X).ToList(), fs.Points.Select(dp => dp.Y).ToList(), out double boh, out double qq));
                n.Series.Add(fs2);
                n.Axes.Add(yAxis2);

            }

            chart1.Model = n;

            chart1.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack);
            chart1.ActualController.UnbindMouseDown(OxyMouseButton.Right);
            chart1.ActualController.BindMouseDown(OxyMouseButton.Right, PlotCommands.ZoomRectangle);
            chart1.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            chart1.ActualController.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);

            grid.Children.Add(chart1);
            grid.Children.Add(buttonsPanel);
           

            return grid;
        }

        public static Grid GraphTwoData(List<DateTime> Dates, List<double> data1, List<double> data2, string dataTitle1, string dataTitle2, Color color, Color color2, int data1Div = 50, int data2Div = 50)
        {
            SolidColorBrush oxC = new SolidColorBrush(color);
            SolidColorBrush oxC2 = new SolidColorBrush(color2);

            Grid grid = new Grid();

            Button resetButton = new Button()
            {
                Visibility = Visibility.Collapsed,
                Content = "Reset Zoom",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                Height=30, Width=80,
                Margin = new Thickness(30, 30, 30, 5),
                Background = oxC,
                Foreground = new SolidColorBrush(Colors.White)
            };

            PlotView chart1 = new PlotView()
            {
                //IsMouseWheelEnabled = false,
                Margin = new Thickness(10, 0, 20, 5),
                Foreground = new SolidColorBrush(Colors.Black),
            };

            PlotModel n = new PlotModel()
            {
                PlotType = PlotType.XY,
                PlotMargins = new OxyThickness(double.NaN, 0, double.NaN, double.NaN),
                Title = MainWindow.GraphTitle,
                TitleFontSize = 14,
                Subtitle = "File: "+MainWindow.FileName,
            };

            LinearAxis yAxis = new LinearAxis()
            {
                Key = "yAxis",
                Title = dataTitle1,
                AxisTitleDistance = 20,
                IntervalLength = 20,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.Dot,
            };

            LinearAxis yAxis2 = new LinearAxis()
            {
                Key = "yAxis2",
                Position = AxisPosition.Right,
                Title = dataTitle2,
                AxisTitleDistance = 20,
                IntervalLength = 20,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.Dot,
            };

            DateTimeAxis xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "yyyy/MM/dd HH:mm",
                Title = "Data",
                AxisTitleDistance = 10,
                IntervalLength = 30,
                MinimumMajorStep = 0.001,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                Angle = -35
            };

            FunctionSeries fs = new FunctionSeries()
            {
                CanTrackerInterpolatePoints = false,
                Color = OxyColor.FromArgb(oxC.Color.A, oxC.Color.R, oxC.Color.G, oxC.Color.B),
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.None,
                MarkerSize = 1.5,
                MarkerStroke = OxyColor.FromArgb(180, 0, 0, 0),
                YAxisKey = "yAxis",
                TrackerFormatString = dataTitle1 + " : {Y:0.#}" + Environment.NewLine + "Data : {2:yyyy/MM/dd}" + Environment.NewLine + "Ora: {2:HH:mm:ss}"
            };

            FunctionSeries fs2 = new FunctionSeries()
            {
                CanTrackerInterpolatePoints = false,
                Color = OxyColor.FromArgb(oxC2.Color.A, oxC2.Color.R, oxC2.Color.G, oxC2.Color.B),
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.None,
                MarkerSize = 1.5,
                MarkerStroke = OxyColor.FromArgb(180, 0, 0, 0),
                YAxisKey = "yAxis2",
                TrackerFormatString = dataTitle2 + " : {Y:0.#}" + Environment.NewLine + "Data : {2:yyyy-MM-dd}" + Environment.NewLine + "Ora: {2:HH:mm:ss}"
            };

            int i = 0;

            foreach (DateTime date in Dates)
            {
                try
                {
                    //dt = Convert.ToDateTime(date);// DateTime.ParseExact(date, "g", new CultureInfo("ja-JP")); 

                    fs.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data1[i]));
                    fs2.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data2[i]));
                    i++;
                }
                catch
                {
                    throw new FormatException();
                }

            }

            xAxis.AxisChanged += (s, e) =>
            {
                resetButton.Dispatcher.Invoke((Action)(() => { resetButton.Visibility = Visibility.Visible; }));
            };

            yAxis2.AxisChanged += (s, e) =>
            {
                resetButton.Dispatcher.Invoke((Action)(() => { resetButton.Visibility = Visibility.Visible; }));
            };

            resetButton.Click += (s, e) =>
            {
                if (s != null)
                {
                    Button b = (s as Button);
                    PlotView p = ((b.Parent as Grid).Children[0] as PlotView);
                    if (b.Visibility == Visibility.Visible)
                    {
                        p.ResetAllAxes();
                        b.Visibility = Visibility.Collapsed;
                    }
                }
            };

            xAxis.MajorGridlineColor = xAxis.TicklineColor = yAxis.TicklineColor = yAxis.MajorGridlineColor = OxyColors.Gray;


            double yAxisMin = yAxis.Minimum = fs.Points.Min(p => p.Y) - (fs.Points.Min(p => p.Y) / data1Div);
            double yAxisMax = yAxis.Maximum = fs.Points.Max(p => p.Y) + (fs.Points.Max(p => p.Y) / data1Div);
            double yAxis2Min = yAxis2.Minimum = fs2.Points.Min(p => p.Y) - (fs2.Points.Min(p => p.Y) / data2Div);
            double yAxis2Max = yAxis2.Maximum = fs2.Points.Max(p => p.Y) + (fs2.Points.Max(p => p.Y) / data2Div);

            n.Axes.Add(xAxis);
            n.Axes.Add(yAxis);
            n.Axes.Add(yAxis2);

            n.Series.Add(fs);
            n.Series.Add(fs2);
            
            chart1.Model = n;

            chart1.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack);
            chart1.ActualController.UnbindMouseDown(OxyMouseButton.Right);
            chart1.ActualController.BindMouseDown(OxyMouseButton.Right, PlotCommands.ZoomRectangle);
            chart1.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            chart1.ActualController.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);
            
            grid.Children.Add(chart1);
            grid.Children.Add(resetButton);

            return grid;
        }

        public static Grid CalcBeta(List<double> _Press, List<double> _RCount, double _refPress)
        {
            //if (refPress != _refPress || _PmP0.Count == 0)
            {
                PmP0.Clear();
                double _RCountMean = _RCount.Average();

                _Press.ForEach(point => PmP0.Add(_Press[_Press.IndexOf(point)] - _refPress));

                _PmP0.Clear();
                _logCount.Clear();
                _RCount.ForEach(count =>
                {
                    //if (!((count) < _RCountMean - (_RCountMean / 2)))
                    {
                        _logCount.Add(Math.Log(_RCount[_RCount.IndexOf(count)]));
                        _PmP0.Add(PmP0[_RCount.IndexOf(count)]);
                    }
                });

                if (_logCount.Count > 0)
                {
                    LinearRegression(_PmP0, _logCount, out Beta, out SigmaBeta, out yIntercept);

                    refPress = _refPress;

                    return GraphBeta(_PmP0, _logCount, Colors.Red);
                }
                else
                    return new Grid();
            }
           // else
            //    return GraphBeta(_PmP0, _logCount, Colors.Red);

        }

        public static Grid GraphBeta(List<double> press, List<double> logN, Color color)
        {
            SolidColorBrush oxC = new SolidColorBrush(color);

            Grid grid = new Grid();

            Button resetButton = new Button()
            {
                Visibility = Visibility.Collapsed,
                Content = "Reset Zoom",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                Height = 30,
                Width = 80,
                Margin = new Thickness(30, 30, 30, 15),
                Background = oxC,
                Foreground = new SolidColorBrush(Colors.White)
            };

            PlotView chart1 = new PlotView()
            {
                Margin = new Thickness(5, 0, 20, 5),
                Foreground = new SolidColorBrush(Colors.Black),
                DefaultTrackerTemplate = null,
            };

            FunctionSeries fs = new FunctionSeries()
            {
                Color = OxyColor.FromArgb(oxC.Color.A, oxC.Color.R, oxC.Color.G, oxC.Color.B),
                LineStyle = LineStyle.None,
                MarkerFill = OxyColor.FromArgb(oxC.Color.A, oxC.Color.R, oxC.Color.G, oxC.Color.B),
                MarkerType = MarkerType.Square,
                MarkerSize = 3,
                MarkerStroke = OxyColor.FromArgb(180, 50, 50, 50),
            };

            int i = 0;

            foreach (double pr in press)
            {
                try
                {
                    fs.Points.Add(new DataPoint(pr, logN[i]));
                    i++;
                }
                catch
                {
                    throw new FormatException();
                }

            }
            i = fs.Points.Count;


            PlotModel n = new PlotModel()
            {
                PlotType = PlotType.XY,
                PlotMargins = new OxyThickness(double.NaN, 0, double.NaN, double.NaN),
                LegendBackground = OxyColor.FromArgb(220, 30, 70, 255),
                LegendTitleColor = OxyColors.White,
                LegendTitle = "Beta = " + Math.Round(Beta, 10).ToString() + "±" + Math.Round(SigmaBeta,10).ToString(),
                LegendTitleFontSize = 18,
                LegendMaxHeight = 60, 
                LegendMaxWidth = 280,
                Title = MainWindow.GraphTitle,
                TitleFontSize = 14,
                Subtitle = "File: "+MainWindow.FileName,
            };


            LinearAxis yAxis = new LinearAxis()
            {
                Title = "Log(N)",
                AxisTitleDistance = 20,
                IntervalLength = 20,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                Minimum = fs.Points.Min(p => p.Y) - (fs.Points.Min(p => p.Y) / 200),
                Maximum = fs.Points.Max(p => p.Y) + (fs.Points.Max(p => p.Y) / 200)
            };

            LinearAxis xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "(P-P0)",
                AxisTitleDistance = 20,
                IntervalLength = 60,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                Minimum = fs.Points.Min(p => p.X) - (fs.Points.Min(p => p.X) / 200),
                Maximum = fs.Points.Max(p => p.X) + (fs.Points.Max(p => p.X) / 200)
            };

            FunctionSeries TrendFS = new FunctionSeries()
            {
                Color = OxyColors.Blue,
                LineStyle = LineStyle.Dash,
            };

            TrendFS.Points.Add(new DataPoint(press.Min(), press.Min() * Beta + yIntercept));
            TrendFS.Points.Add(new DataPoint(press.Max(), press.Max() * Beta + yIntercept));


            xAxis.AxisChanged += (s, e) =>
            {
                resetButton.Dispatcher.Invoke((Action)(() => { resetButton.Visibility = Visibility.Visible; }));
            };

            yAxis.AxisChanged += (s, e) =>
            {
                resetButton.Dispatcher.Invoke((Action)(() => { resetButton.Visibility = Visibility.Visible; }));
            };

            resetButton.Click += (s, e) =>
            {
                if (s != null)
                {
                    Button b = (s as Button);
                    PlotView p = ((b.Parent as Grid).Children[0] as PlotView);
                    if (b.Visibility == Visibility.Visible)
                    {
                        p.ResetAllAxes();
                        b.Visibility = Visibility.Collapsed;
                    }
                }
            };

            xAxis.MajorGridlineColor = xAxis.TicklineColor = yAxis.TicklineColor = yAxis.MajorGridlineColor = OxyColors.Gray;

            n.Series.Add(fs);
            n.Series.Add(TrendFS);
            n.Axes.Add(xAxis);
            n.Axes.Add(yAxis);
            chart1.Model = n;

            chart1.ActualController.UnbindMouseDown(OxyMouseButton.Right);
            chart1.ActualController.BindMouseDown(OxyMouseButton.Right, PlotCommands.ZoomRectangle);
            chart1.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            chart1.ActualController.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);

            grid.Children.Add(chart1);
            grid.Children.Add(resetButton);

            return grid;
        }

        public static double CalcTempCoeff(List<double> temp, List<double> counts)
        {
            //double TempMean = temp.Average();
            double countMean = counts.Average();

            List<double> Temp = new List<double>();
            List<double> Count = new List<double>();
            temp.ForEach(t => Temp.Add(t));
            counts.ForEach(c => Count.Add(c / countMean));

            LinearRegression(Temp, Count, out double m, out double s_m, out double q);

            Temp.Clear();
            Count.Clear();

            //List<double> CountMeanCount = new List<double>();
            //temp.ForEach(t => TempDiff.Add((t - TempMean)*corrCounts[temp.IndexOf(t)]));

            //Regression(TempDiff, corrCounts, out double m, out double q);

            //TempDiff.Clear();
            //CountMeanCount.Clear();

            return m;

        }

        public static Grid SigmaTwo(List<DateTime> Dates, List<double> data1)
        {

            Grid grid = new Grid() { Margin = new Thickness(5) };

            Button resetButton = new Button()
            {
                Visibility = Visibility.Collapsed,
                Content = "Reset Grafico",
                Height = 25,
                Width = 80,
                Background = new SolidColorBrush(Colors.OrangeRed),
                Foreground = new SolidColorBrush(Colors.White)
            };

            Button stretchButton = new Button()
            {
                Content = "Stretch Grafico",
                Height = 25,
                Width = 90
            };

            StackPanel buttonsPanel = new StackPanel()
            {
                Margin = new Thickness(30, 30, 30, 5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                Orientation = Orientation.Horizontal

            };

            buttonsPanel.Children.Add(resetButton);
            buttonsPanel.Children.Add(stretchButton);

            PlotView chart1 = new PlotView()
            {
                //IsMouseWheelEnabled = false,
                Margin = new Thickness(10, 0, 20, 5),
                Foreground = new SolidColorBrush(Colors.Black),
            };


            FunctionSeries fs = new FunctionSeries()
            {
                CanTrackerInterpolatePoints = false,
                Color = OxyColor.FromArgb(150, 50, 50, 50),
                LineStyle = LineStyle.None,
                MarkerType = MarkerType.Circle,
                MarkerSize = 2.5,
                MarkerStroke = OxyColor.FromArgb(170, 0, 0, 0),
                TrackerFormatString = "Cont. : {Y:0.}" + Environment.NewLine + "Data : {2:yyyy-MM-dd}" + Environment.NewLine + "Ora: {2:HH:mm:ss}"
            };

            int i = 0;

            foreach (DateTime date in Dates)
            {
                try
                {
                    //dt = Convert.ToDateTime(date);// DateTime.ParseExact(date, "g", new CultureInfo("ja-JP")); 

                    fs.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data1[i]));
                    i++;
                }
                catch
                {
                    throw new FormatException();
                }

            }
            i = fs.Points.Count;

            double fsPointsMean = fs.Points.Average(d => d.Y);
            double fsPointsMin, fsPointsMax;
            fsPointsMax = fs.Points.Max(p => p.Y);
            fsPointsMin = fs.Points.Min(p => p.Y);

            double Sigma = 0;
            data1.ForEach(d => Sigma += (d - fsPointsMean) * (d - fsPointsMean));
            Sigma = Math.Sqrt(Sigma / (data1.Count - 1));

            PlotModel n = new PlotModel()
            {
                PlotType = PlotType.XY,
                PlotMargins = new OxyThickness(double.NaN, 0, double.NaN, double.NaN),
                LegendBackground = OxyColor.FromArgb(170, 180, 180, 180),
                LegendTitleColor = OxyColors.Black,
                LegendTitleFontSize = 14,
                LegendTitleFont = "Arial",
                LegendTitle = "Media = " + Math.Round(fsPointsMean, 0) + "\nMax = " + Math.Round(fsPointsMax, 0) + "\nMin = " + Math.Round(fsPointsMin, 0) + "\nStd.Dev. = " + Sigma.ToString("0.00"),
                LegendMaxHeight = 100,
                LegendMaxWidth = 200,
                LegendPosition = LegendPosition.TopRight,
                Title = MainWindow.GraphTitle,
                TitleFontSize = 14,
                Subtitle = "File: "+MainWindow.FileName,
                
            };
           
            LinearAxis yAxis = new LinearAxis()
            {
                Title = "",
                AxisTitleDistance = 20,
                IntervalLength = 30,
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray,
                Maximum = fsPointsMax + (fsPointsMax / 40),
                Minimum = fsPointsMin - (fsPointsMin / 40)
            };

            //yAxis.LabelFormatter = (x) => ((x - data1.Average()) * 100 / data1.Average()).ToString("0.0") + "%";

            DateTimeAxis xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "yyyy/MM/dd HH:mm",
                Title = "Data",
                AxisTitleDistance = 10,
                IntervalLength = 30,
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = yAxis.MajorGridlineColor,
                MinorGridlineColor = yAxis.MinorGridlineColor,
                Angle = -35
            };

            //if (DateFormat)
                xAxis.MinimumMajorStep = 0.001;

            //else
            //    xAxis.MinimumMajorStep = 0.0625;


            xAxis.AxisChanged += (s, e) =>
            {
                if (chart1.IsLoaded)
                {
                    resetButton.Dispatcher.Invoke(() => { resetButton.Visibility = Visibility.Visible; });

                    foreach (Annotation ann in n.Annotations)
                    {
                        if (ann as TextAnnotation != null)
                        {
                            TextAnnotation ta = ann as TextAnnotation;
                            ta.TextPosition = xAxis.InverseTransform(n.PlotArea.Center.X, xAxis.Transform(ta.TextPosition.X, ta.TextPosition.Y, yAxis).Y, yAxis);
                        }
                    }
                }
            };

            xAxis.TransformChanged += (s, e) =>
            {
                if (chart1.IsLoaded)
                {
                    foreach (Annotation ann in n.Annotations)
                    {
                        if (ann as TextAnnotation != null)
                        {
                            TextAnnotation ta = ann as TextAnnotation;
                             ta.TextPosition = xAxis.InverseTransform(n.PlotArea.Center.X, xAxis.Transform(ta.TextPosition.X, ta.TextPosition.Y, yAxis).Y, yAxis);
                        }
                    }
                }
            };

            xAxis.AxisChanged += (s, e) =>
            {
                resetButton.Dispatcher.Invoke(() => { resetButton.Visibility = Visibility.Visible; });
                stretchButton.Dispatcher.Invoke(() => { stretchButton.Visibility = Visibility.Collapsed; });
            };


            yAxis.AxisChanged += (s, e) =>
            {
                resetButton.Dispatcher.Invoke(() => { resetButton.Visibility = Visibility.Visible; });
                stretchButton.Dispatcher.Invoke(() => { stretchButton.Visibility = Visibility.Collapsed; });
            };

            resetButton.Click += (s, e) =>
            {
                if (s != null)
                {
                    PlotView p = grid.Children[0] as PlotView;
                    if (resetButton.Visibility == Visibility.Visible)
                    {
                        p.Model.Axes[1].Maximum = fsPointsMax + (fsPointsMax / 40);
                        p.Model.Axes[1].Minimum = fsPointsMin - (fsPointsMin / 40);
                        p.ResetAllAxes();
                        resetButton.Visibility = Visibility.Collapsed;
                        stretchButton.Visibility = Visibility.Visible;
                    }

                }
            };

            stretchButton.Click += (s, e) =>
            {
                if (s != null)
                {
                    PlotView p = grid.Children[0] as PlotView;

                    double YAxisActualMin = p.Model.Axes[0].ActualMinimum;
                    double YAxisActualMax = p.Model.Axes[0].ActualMaximum;
                    List<DataPoint> RangePoints = fs.Points.FindAll((d) => d.X >= YAxisActualMin && d.X <= YAxisActualMax);


                    double max = RangePoints.Max(d => d.Y);
                    double min = RangePoints.Min(d => d.Y);
                    double med = RangePoints.Average(d => d.Y);

                    RangePoints.Clear();

                    p.Model.Axes[1].Maximum = max + (med / 1000);
                    p.Model.Axes[1].Minimum = min - (med / 1000);

                    p.InvalidatePlot();

                    resetButton.Visibility = Visibility.Visible;
                    stretchButton.Visibility = Visibility.Collapsed;

                }
            };

            xAxis.MajorGridlineColor = xAxis.TicklineColor = yAxis.TicklineColor = yAxis.MajorGridlineColor = OxyColors.Gray;

            n.Series.Add(fs);            

            n.Axes.Add(xAxis);
            n.Axes.Add(yAxis);

            FunctionSeries fsMean = new FunctionSeries()
            {
                CanTrackerInterpolatePoints = false,
                Color = OxyColor.FromArgb(150, 30, 90, 255),
                LineStyle = LineStyle.Solid,
                MarkerSize = 0,
                StrokeThickness = 3,
                TrackerFormatString = "Media = " + fsPointsMean.ToString("0.00")
            };

            AvgLine(fs.Points).Points.ForEach(p => fsMean.Points.Add(p));

            n.Series.Add(fsMean);


            OxyPlot.Annotations.
            RectangleAnnotation SigmaAnn = new RectangleAnnotation()
            {
                MinimumY = -2 * Sigma + fsPointsMean,
                MaximumY = 2 * Sigma + fsPointsMean,
                //MinimumX = DateTimeAxis.ToDouble(Dates.First()),
                //MaximumX = DateTimeAxis.ToDouble(Dates.Last()),
                Fill = OxyColor.FromArgb(100, 255, 0, 0),
                Stroke = OxyColors.Red,
                StrokeThickness = 2,
                Layer = AnnotationLayer.BelowSeries,
            };

            TextAnnotation SigmaTextAnn = new TextAnnotation()
            {
                Text = "+2σ",
                TextPosition = new DataPoint(DateTimeAxis.ToDouble(Dates.ElementAt(Dates.Count / 2)), SigmaAnn.MaximumY),
                FontSize = 12,
                FontWeight = OxyPlot.FontWeights.Bold,
                StrokeThickness = 1,
                Background = OxyColors.White,
                
            }; 
            TextAnnotation SigmaMinTextAnn = new TextAnnotation()
            {
                Text = "-2σ",
                TextPosition = new DataPoint(DateTimeAxis.ToDouble(Dates.ElementAt(Dates.Count / 2)), SigmaAnn.MinimumY),
                Offset = new ScreenVector(0,14),
                FontSize = SigmaTextAnn.FontSize,
                FontWeight = SigmaTextAnn.FontWeight,
                StrokeThickness = SigmaTextAnn.StrokeThickness,
                Background = SigmaTextAnn.Background
            };

            for (int c = 0; c < 6; c++)
            {
                //if (c == 6)
                //    c = 10;

                LineAnnotation LineAnnPlus = new LineAnnotation()
                {
                    Y = fsPointsMean + ((0.05 + (c / 100.0)) * fsPointsMean),
                    Type = LineAnnotationType.Horizontal,
                    Color = OxyColors.DarkGreen,
                    StrokeThickness = 1.25,
                    Layer = AnnotationLayer.BelowSeries,
                    LineStyle = LineStyle.Dash
                };

                LineAnnotation LineAnnMinus = new LineAnnotation()
                {
                    Y = fsPointsMean - ((0.05 + (c / 100.0)) * fsPointsMean),
                    Type = LineAnnPlus.Type,
                    Color = LineAnnPlus.Color,
                    StrokeThickness = LineAnnPlus.StrokeThickness,
                    Layer = LineAnnPlus.Layer,
                    LineStyle = LineAnnPlus.LineStyle
                };

                int dateIndex = ((Dates.Count / 2) - (Dates.Count / 20));

                TextAnnotation PercTextAnnPlus = new TextAnnotation()
                {
                    Text = ((0.05 + (c / 100.0)) * 100).ToString("F0") + "%",
                    TextPosition = new DataPoint(DateTimeAxis.ToDouble(Dates.ElementAt(dateIndex)), LineAnnPlus.Y),
                    Offset = new ScreenVector(22 * (c + 2), 0),
                    FontSize = 10,
                    TextColor = OxyColors.Black,
                    FontWeight = OxyPlot.FontWeights.Bold,
                    StrokeThickness = 1,
                    Stroke = OxyColors.Gray,
                    Background = OxyColor.FromArgb(60, LineAnnPlus.Color.R , LineAnnPlus.Color.G, LineAnnPlus.Color.B),
                };

                TextAnnotation PercTextAnnMinus = new TextAnnotation()
                {
                    Text = PercTextAnnPlus.Text,
                    TextPosition = new DataPoint(DateTimeAxis.ToDouble(Dates.ElementAt(dateIndex)), LineAnnMinus.Y),
                    FontSize = PercTextAnnPlus.FontSize,
                    Offset = new ScreenVector(PercTextAnnPlus.Offset.X, PercTextAnnPlus.FontSize + 2),
                    TextColor = PercTextAnnPlus.TextColor,
                    FontWeight = PercTextAnnPlus.FontWeight,
                    StrokeThickness = PercTextAnnPlus.StrokeThickness,
                    Stroke = PercTextAnnPlus.Stroke,
                    Background = PercTextAnnPlus.Background,
                };

                n.Annotations.Add(LineAnnPlus);
                n.Annotations.Add(LineAnnMinus);
                n.Annotations.Add(PercTextAnnPlus);
                n.Annotations.Add(PercTextAnnMinus);
            }

            n.Annotations.Add(SigmaAnn);
            n.Annotations.Add(SigmaTextAnn);
            n.Annotations.Add(SigmaMinTextAnn);

            chart1.Model = n;
            
            chart1.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack);
            chart1.ActualController.UnbindMouseDown(OxyMouseButton.Right);
            chart1.ActualController.BindMouseDown(OxyMouseButton.Right, PlotCommands.ZoomRectangle);
            chart1.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            chart1.ActualController.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);

            grid.Children.Add(chart1);
            grid.Children.Add(buttonsPanel);

            return grid;
        }

        public static FunctionSeries UpdateTemperatureCorr(List<DateTime> Dates, List<double> data, PlotView pv)
        {
            FunctionSeries fs = new FunctionSeries()
            {
                CanTrackerInterpolatePoints = false,
                Color = (pv.Model.Series.First() as FunctionSeries).Color,
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                MarkerSize = 1.5,
                MarkerFill = OxyColors.Black,
                MarkerStrokeThickness = 0,
                TrackerFormatString = (pv.Model.Series.First() as FunctionSeries).TrackerFormatString
            };

            int i = 0;

            foreach (DateTime date in Dates)
            {
                try
                {
                    fs.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(date), data[i]));
                    i++;
                }
                catch
                {
                    throw new FormatException();
                }

            }

            i = fs.Points.Count;

            double fsPointsMean = fs.Points.Average(d => d.Y);
            double fsPointsMin, fsPointsMax;
            fsPointsMax = fs.Points.Max(p => p.Y);
            fsPointsMin = fs.Points.Min(p => p.Y);

            pv.Model.LegendTitle = "Media = " + Math.Round(fsPointsMean, 0) + "\n Max = " + Math.Round(fsPointsMax, 0) + "\n Min = " + Math.Round(fsPointsMin, 0);

            return fs;
        }

        private static FunctionSeries LinearRegression(List<double> xListRef, List<double> yListRef, out double m, out double sigma_m, out double q)
        {
            List<double> xList = xListRef.ToList();
            List<double> yList = yListRef.ToList();

            FunctionSeries fsFit = new FunctionSeries()
            {
                Color = OxyColors.Black,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 3,
            };

            int numPoints = yList.Count;

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            for (int i = 0; i < numPoints; i++)
            {
                var x = xList[i];
                var y = yList[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            var ssX = sumOfXSq - ((sumOfX * sumOfX) / numPoints);
            var ssY = sumOfYSq - ((sumOfY * sumOfY) / numPoints);

            var rNumerator = (numPoints * sumCodeviates) - (sumOfX * sumOfY);
            var rDenom = (numPoints * sumOfXSq - (sumOfX * sumOfX)) * (numPoints * sumOfYSq - (sumOfY * sumOfY));
            var sCo = sumCodeviates - ((sumOfX * sumOfY) / numPoints);

            var meanX = sumOfX / numPoints;
            var meanY = sumOfY / numPoints;
            var dblR = rNumerator / Math.Sqrt(rDenom);

            m = sCo / ssX;
            double sigma = 1.0 / (numPoints * (numPoints - 2)) * (numPoints * sumOfYSq - Math.Pow(sumOfY, 2) - Math.Pow(m, 2) * (numPoints * sumOfXSq - Math.Pow(sumOfX, 2)));
            sigma_m = numPoints * sigma / (numPoints * sumOfXSq - Math.Pow(sumOfX, 2));
            q = meanY - (m * meanX);

            fsFit.Points.Add(new DataPoint(xList.Min(), xList.Min() * m + q));
            fsFit.Points.Add(new DataPoint(xList.Max(), xList.Max() * m + q));

            return fsFit;
        }

        public static PolylineAnnotation Smoothed(List<DataPoint> DP, uint Window = 4)
        {
            PolylineAnnotation fsFFT = new PolylineAnnotation()
            {
                //CanTrackerInterpolatePoints = false,
                LineJoin = LineJoin.Round,
                Color = OxyColor.FromArgb(200, 255, 40, 40),
                LineStyle = LineStyle.Solid,
                StrokeThickness = 3,
                //InterpolationAlgorithm = InterpolationAlgorithms.UniformCatmullRomSpline,              
                //TrackerFormatString = "Cont. (Smussati) : {Y:0.}" + Environment.NewLine + "Data : {2:yyyy-MM-dd}" + Environment.NewLine + "Ora: {2:HH:mm:ss}"
                //TrackerFormatString = " Andamento Smussato ",
            };

            double xLocalMean = 0, yLocalMean = 0;
            int i = 0;
            DataPoint dp;

            if (Window >= DP.Count / 2)
                return fsFFT;

            for (int k = 0; k < DP.Count;)
            {
                dp = DP[k + i];

                if (i >= Window)
                {
                    fsFFT.Points.Add(new DataPoint(xLocalMean / Window, yLocalMean / Window));
                    yLocalMean = xLocalMean = i = 0;

                    if (k < DP.Count - Window - 1)
                        k++;
                    else
                        break;
                }

                xLocalMean += dp.X;
                yLocalMean += dp.Y;
                i++;

            }


            //foreach (DataPoint dp in DP)
            //{
            //    if (i >= Window)
            //    {
            //        fsFFT.Points.Add(new DataPoint(xLocalMean / i, yLocalMean / i));
            //        yLocalMean = xLocalMean = i = 0;
            //    }

            //    xLocalMean += dp.X;
            //    yLocalMean += dp.Y;
            //    i++;

            //}

            return fsFFT;
        }

        public static FunctionSeries AvgLine(List<DataPoint> DP)
        {
            double yMean = DP.Average( p => p.Y);
            DataPoint firtsDP = new DataPoint(DP.First().X, yMean);
            DataPoint LastDP = new DataPoint(DP.Last().X, yMean);

            FunctionSeries fs = new FunctionSeries()
            {
                CanTrackerInterpolatePoints = false,
                Color = OxyColors.Orange,
                LineStyle = LineStyle.DashDot,
                StrokeThickness = 3,
                TrackerFormatString = " Retta Media ",
            };

            fs.Points.Add(firtsDP);
            fs.Points.Add(LastDP);

            return fs;
        }
    }
}
