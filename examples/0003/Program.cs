using SearchAThing;
using System.Linq;
using SearchAThing.Gui;
using Avalonia.Controls;
using Avalonia;
using OxyPlot;
using OxyPlot.Avalonia;
using static AngouriMath.Extensions.AngouriMathExtensions;
using System.Collections.Generic;
using System;

namespace analysis
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

            public MainWindow() : base(new[]
            {
                "resm:OxyPlot.Avalonia.Themes.Default.xaml?assembly=OxyPlot.Avalonia"
            })
            {
                Width = 600;
                Height = 700;

                var colors = new[] { "#9ccc65", "#6b9b37", "#42a5f5", "#0077c2", "#ef5350", "#b61827" };
                m = new Model();

                var tabs = new List<TabItem>();

                var ti0 = new TabItem() { Header = "accel" };
                tabs.Add(ti0);
                var gr = new Grid();
                gr.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                gr.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                var sp = new WrapPanel() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                gr.Children.Add(sp);
                var pv = new PlotView();
                pv.Model = new PlotModel();
                Grid.SetRow(pv, 1);
                gr.Children.Add(pv);
                ti0.Content = gr;

                var duration = 2d;
                var initialSpeed = 2d;
                var initialPos = 5d;

                Func<AngouriMath.Entity, string> toLatex = (e) => e.Latexise().Replace("\\times", "\\cdot");
                var eqMargin = new Thickness(0, 0, 100, 15);

                var accel = "1-cos(t)".Substitute("t", "(t/d*(2*pi))");
                sp.Children.Add(new CSharpMath.Avalonia.MathView() { LaTeX = $"accel(t)={toLatex(accel.Simplify())}", Margin = eqMargin });
                var accelC = accel.Substitute("d", duration).Compile("t");
                var accelS = new OxyPlot.Series.LineSeries()
                {
                    Title = "accel",
                    DataFieldX = "x",
                    DataFieldY = "y",
                    ItemsSource = SciToolkit.Range(
                        tol: 1e-3,
                        start: 0,
                        end: duration,
                        inc: duration / 100,
                        includeEnd: true).Select(x => new PlotData(x, accelC.Call(x).Real)),
                    Color = OxyColor.Parse(colors[0])
                };
                pv.Model.Series.Add(accelS);

                var speed = $"s_0+{accel.Integrate("t")}";
                sp.Children.Add(new CSharpMath.Avalonia.MathView() { LaTeX = $"speed(t)={toLatex(speed.Simplify())}", Margin = eqMargin });
                var speedC = speed.Substitute("d", duration).Substitute("s_0", initialSpeed).Compile("t");
                var speedS = new OxyPlot.Series.LineSeries()
                {
                    Title = "speed",
                    DataFieldX = "x",
                    DataFieldY = "y",
                    ItemsSource = SciToolkit.Range(
                        tol: 1e-3,
                        start: 0,
                        end: duration,
                        inc: duration / 100,
                        includeEnd: true).Select(x => new PlotData(x, speedC.Call(x).Real)),
                    Color = OxyColor.Parse(colors[2])
                };
                pv.Model.Series.Add(speedS);

                var pos = $"p_0+{speed.Integrate("t") - speed.Integrate("t").Substitute("t", 0)}";
                sp.Children.Add(new CSharpMath.Avalonia.MathView() { LaTeX = $"pos(t)={toLatex(pos.Simplify())}", Margin = eqMargin });
                var posC = pos.Substitute("d", duration).Substitute("s_0", initialSpeed).Substitute("p_0", initialPos).Compile("t");
                var posS = new OxyPlot.Series.LineSeries()
                {
                    Title = "pos",
                    DataFieldX = "x",
                    DataFieldY = "y",
                    ItemsSource = SciToolkit.Range(
                        tol: 1e-3,
                        start: 0,
                        end: duration,
                        inc: duration / 100,
                        includeEnd: true).Select(x => new PlotData(x, posC.Call(x).Real)),
                    Color = OxyColor.Parse(colors[4])
                };
                pv.Model.Series.Add(posS);

                pv.Model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation()
                {
                    TextPosition = new DataPoint(0, initialSpeed),                    
                    Text = $"s0={initialSpeed}",
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right,
                    TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom,
                    StrokeThickness = 0
                });

                pv.Model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation()
                {
                    TextPosition = new DataPoint(0, initialPos),
                    Text = $"p0={initialPos}",
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Right,
                    TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom,
                    StrokeThickness = 0
                });

                pv.Model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation()
                {
                    TextPosition = new DataPoint(duration, 0),                    
                    Text = $"d={duration}",
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                    TextVerticalAlignment = OxyPlot.VerticalAlignment.Top,
                    StrokeThickness = 0
                });

                var grRoot = new Grid() { DataContext = m, Margin = new Thickness(10) };
                var tabList = new TabControl() { Items = tabs };
                grRoot.Children.Add(tabList);
                this.Content = grRoot;

                pv.ResetAllAxes();
                foreach (var x in pv.Model.Axes)
                {
                    x.MajorGridlineStyle = LineStyle.Dot;
                }
                foreach (var ax in pv.Model.Axes)
                {
                    ax.MinimumPadding = 0.1;
                    ax.MaximumPadding = 0.1;
                }
                pv.InvalidatePlot();
            }
        }

        static void Main(string[] args)
        {
            GuiToolkit.CreateGui<MainWindow>();
        }
    }
}