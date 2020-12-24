using static System.Math;
using SearchAThing;
using System.Linq;
using SearchAThing.Gui;
using Avalonia.Controls;
using Avalonia;
using OxyPlot;
using OxyPlot.Avalonia;
using static AngouriMath.Extensions.AngouriMathExtensions;

// assemblies added to this example:
//
// dotnet add package OxyPlot.Avalonia --version 2.1.0-20201211.2
// dotnet add package Avalonia.Desktop --version 0.10.0-rc1
// dotnet add package Avalonia --version 0.10.0-rc1
// dotnet add package AngouriMath --version 1.2.0-preview.5

namespace test
{
    class Program
    {

        public class PlotData
        {
            public PlotData(double x, double y) { this.x = x; this.y = y; }
            public double x { get; set; }
            public double y { get; set; }
        }

        public class Model
        {
        }

        public class MainWindow : Win
        {
            Model m;
            PlotView pv;

            public MainWindow() : base(new[]
            {
                "resm:OxyPlot.Avalonia.Themes.Default.xaml?assembly=OxyPlot.Avalonia"
            })
            {
                m = new Model();

                var grRoot = new Grid() { DataContext = m, Margin = new Thickness(10) }; this.Content = grRoot;

                pv = new PlotView();
                pv.Model = new PlotModel();
                grRoot.Children.Add(pv);

                var f1 = new OxyPlot.Series.LineSeries()
                {
                    Title = "sin(x)",
                    DataFieldX = "x",
                    DataFieldY = "y",
                    ItemsSource = SciToolkit.Range(
                        tol: 1e-3,
                        start: 0,
                        end: 2 * PI,
                        inc: 2 * PI / 100,
                        includeEnd: true).Select(x => new PlotData(x, Sin(x))),
                    Color = OxyColor.Parse("#9ccc65")
                };
                pv.Model.Series.Add(f1);

                var f2f = "3*sin(x)"; // symbolic eval
                var f2fc = f2f.Compile("x");
                var f2 = new OxyPlot.Series.LineSeries()
                {
                    Title = "3*sin(x)",
                    DataFieldX = "x",
                    DataFieldY = "y",
                    ItemsSource = SciToolkit.Range(
                        tol: 1e-3,
                        start: 0,
                        end: 2 * PI,
                        inc: 2 * PI / 100,
                        includeEnd: true).Select(x => new PlotData(x, f2fc.Call(x).Real)),
                    Color = OxyColor.Parse("#6b9b37")
                };
                pv.Model.Series.Add(f2);

                pv.ResetAllAxes();
                pv.InvalidatePlot();

            }

            protected override void OnMeasureInvalidated()
            {
                base.OnMeasureInvalidated();

                pv.InvalidatePlot();
            }
        }

        static void Main(string[] args)
        {
            GuiToolkit.CreateGui<MainWindow>();
        }
    }
}
