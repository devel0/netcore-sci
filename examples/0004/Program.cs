using SearchAThing;
using System.Linq;
using SearchAThing.Gui;
using Avalonia.Controls;
using Avalonia;
using OxyPlot;
using OxyPlot.Avalonia;
using static AngouriMath.Extensions.AngouriMathExtensions;
using System;
using AngouriMath;
using Exts;
using ClosedXML.Excel;
using System.IO;
using System.Diagnostics;
using static System.FormattableString;
using static System.Math;

namespace Exts
{

    public static class Ext
    {

        public static Entity VeryExpensiveSimplify(this Entity expr, int level)
        {
            Entity ExpensiveSimplify(Entity expr)
            {
                return expr.Replace(x => x.Simplify());
            }

            if (level <= 0)
                return expr;
            return VeryExpensiveSimplify(ExpensiveSimplify(expr), level - 1);
        }

    }

}

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

        static (IXLRange rng_used, int col_cnt, int row_cnt) FinalizeWorksheet(IXLWorksheet ws)
        {
            var rng_used = ws.RangeUsed();
            var col_cnt = rng_used.ColumnCount();
            var row_cnt = rng_used.RowCount();

            (IXLRange rng_used, int row_cnt, int col_cnt) res = (rng_used, row_cnt, col_cnt);

            //ws.Range(1, 1, row_cnt, col_cnt).SetAutoFilter();
            //for (int c = 1; c <= col_cnt; c++) ws.Column(c).AdjustToContents();
            for (int c = 1; c <= col_cnt; c++) ws.Column(c).Width = 25;

            //ws.SheetView.Freeze(1, 0);

            return res;
        }

        public class MainWindow : Win
        {
            public MainWindow() : base(new[]
            {
                "resm:OxyPlot.Avalonia.Themes.Default.xaml?assembly=OxyPlot.Avalonia"
            })
            {
                Width = 600;
                Height = 700;

                var colors = new[] { "#9ccc65", "#6b9b37", "#42a5f5", "#0077c2", "#ef5350", "#b61827" };

                Action refresh = () => { };

                var grRoot = new Grid() { Margin = new Thickness(10) };
                grRoot.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                grRoot.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                Content = grRoot;

                var grInputs = new Grid();
                grInputs.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Auto));
                grInputs.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grRoot.Children.Add(grInputs);

                // f(x) tbox
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                var tblk = new TextBlock() { Text = "f(x)", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
                Grid.SetRow(tblk, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(tblk);
                var fxTBox = new TextBox() { Text = "sin(x)-x", Margin = new Thickness(10, 10, 0, 0) };
                Grid.SetColumn(fxTBox, 1);
                Grid.SetRow(fxTBox, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(fxTBox);

                // xFrom tbox
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                tblk = new TextBlock() { Text = "xFrom", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
                Grid.SetRow(tblk, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(tblk);
                var xFromTBox = new TextBox() { Text = "0", Margin = new Thickness(10, 10, 0, 0) };
                Grid.SetColumn(xFromTBox, 1);
                Grid.SetRow(xFromTBox, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(xFromTBox);

                // xTo tbox
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                tblk = new TextBlock() { Text = "xTo", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
                Grid.SetRow(tblk, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(tblk);
                var xToTBox = new TextBox() { Text = "2*pi", Margin = new Thickness(10, 10, 0, 0) };
                Grid.SetColumn(xToTBox, 1);
                Grid.SetRow(xToTBox, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(xToTBox);

                // LUT size
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                tblk = new TextBlock() { Text = "LUT SIZE", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
                Grid.SetRow(tblk, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(tblk);
                var lutSizeTBox = new TextBox() { Text = "1000000", Margin = new Thickness(10, 10, 0, 0) };
                Grid.SetColumn(lutSizeTBox, 1);
                Grid.SetRow(lutSizeTBox, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(lutSizeTBox);

                // RENDER size
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                tblk = new TextBlock() { Text = "RENDER SIZE", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
                Grid.SetRow(tblk, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(tblk);
                var renderSizeTBox = new TextBox() { Text = "100", Margin = new Thickness(10, 10, 0, 0) };
                Grid.SetColumn(renderSizeTBox, 1);
                Grid.SetRow(renderSizeTBox, grInputs.RowDefinitions.Count - 1);
                grInputs.Children.Add(renderSizeTBox);

                // lut xlsx report btn
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                var btnXlsxCalc = new Button() { Content = "Generate LUT Xlsx report", Margin = new Thickness(0, 10, 0, 0) };
                btnXlsxCalc.Click += (a, b) =>
                {
                    var N = int.Parse(lutSizeTBox.Text);
                    var fx = fxTBox.Text;
                    var xFromVal = xFromTBox.Text.EvalNumerical().ToNumerics().Real;
                    var xToVal = xToTBox.Text.EvalNumerical().ToNumerics().Real;
                    var renderSize = int.Parse(renderSizeTBox.Text);

                    var wb = new ClosedXML.Excel.XLWorkbook();

                    var ws = wb.AddWorksheet();

                    IXLCell cell = null;

                    Func<int, int, IXLCell> getCell = (r, c) =>
                    {
                        return ws.Cell(r, c);
                    };

                    Action<int, int, object> setCell = (r, c, val) =>
                    {
                        cell = getCell(r, c);
                        cell.Value = val;
                    };

                    Action<int, int, string> setCellFormulaR1C1 = (r, c, formula) =>
                    {
                        cell = getCell(r, c);
                        cell.FormulaR1C1 = formula;
                    };

                    Action<int, int, object> setCellBold = (r, c, val) =>
                    {
                        setCell(r, c, val);
                        cell.Style.Font.SetBold();
                    };

                    int row = 1;
                    int col = 1;

                    int coly = col;
                    setCellBold(row, col, "y");
                    ++col;

                    int colx_interp = col;
                    setCellBold(row, col, "interpX");
                    ++col;

                    int colfx = col;
                    setCellBold(row, col, "f(x)");
                    ++col;

                    int coldiff = col;
                    setCellBold(row, col, "f(x)-y");
                    ++col;

                    int colabsdiff = col;
                    setCellBold(row, col, "abs(f(x)-y)");
                    ++col;

                    col = 1;
                    ++row;

                    var fxC = fx.Compile("x");
                    LUT lut = null;

                    lut = new LUT(x => fxC.Call(x).Real, xFromVal, xToVal, N);

                    var y = lut.YFrom;
                    var yStep = (lut.YTo - lut.YFrom) / renderSize;

                    for (int k = 0; k <= renderSize; ++k, y += yStep)
                    {
                        setCell(row, coly, y);

                        var x = lut.ComputeX(y);

                        setCell(row, colx_interp, x);

                        var y_recalc = fxC.Call(x).Real;
                        setCell(row, colfx, y_recalc);
                        //setCellFormulaR1C1(row, colfx, "sin(RC[-1])-RC[-1]");

                        setCellFormulaR1C1(row, coldiff, "RC[-1]-RC[-3]");

                        setCellFormulaR1C1(row, colabsdiff, "abs(RC[-1])");

                        ++row;
                    }

                    FinalizeWorksheet(ws);
                    ws.SheetView.Freeze(1, 0);
                    ws.SheetView.ZoomScale = 150;

                    var pathfilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "lut.xlsx");
                    wb.SaveAs(pathfilename);

                    var psi = new ProcessStartInfo(pathfilename);
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                };
                Grid.SetRow(btnXlsxCalc, grInputs.RowDefinitions.Count - 1);
                Grid.SetColumnSpan(btnXlsxCalc, 2);
                grInputs.Children.Add(btnXlsxCalc);

                // copy C code to clipboard
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                var btnCopyC = new Button() { Content = "Copy C code to clipboard", Margin = new Thickness(0, 10, 0, 0) };
                btnCopyC.Click += (a, b) =>
                {
                    var swTbl = new StringWriter();
                    var sw = new StringWriter();


                    var N = int.Parse(lutSizeTBox.Text);
                    var fx = fxTBox.Text;
                    var xFromVal = xFromTBox.Text.EvalNumerical().ToNumerics().Real;
                    var xToVal = xToTBox.Text.EvalNumerical().ToNumerics().Real;
                    var renderSize = int.Parse(renderSizeTBox.Text);

                    var fxC = fx.Compile("x");
                    LUT lut = null;

                    lut = new LUT(x => fxC.Call(x).Real, xFromVal, xToVal, N);

                    var y = lut.YFrom;
                    var yStep = (lut.YTo - lut.YFrom) / renderSize;

                    double tolMin = 0, tolMax = 0, tolAvg = 0;

                    for (int k = 0; k <= renderSize; ++k, y += yStep)
                    {
                        var x = lut.ComputeX(y);

                        var y_recalc = fxC.Call(x).Real;

                        var dy = Abs(y_recalc - y);

                        if (k == 0)
                        {
                            tolMin = tolMax = dy;
                        }
                        else if (k != 0 && k != renderSize)
                        {
                            tolMin = Min(tolMin, dy);
                            tolMax = Max(tolMax, dy);
                        }
                        tolAvg += dy;

                        swTbl.WriteLine(Invariant($"{x}{(k == renderSize ? "" : ",")} /* y={y} dy={dy}*/ "));
                    }
                    tolAvg /= renderSize - 1;

                    sw.WriteLine($"// y={fxTBox.Text}");
                    sw.WriteLine($"// tolerance   min:{tolMin}   max:{tolMax}   avg:{tolAvg}");                    
                    sw.WriteLine($"const double LUT_Y_FROM = {lut.YFrom};");
                    sw.WriteLine($"const double LUT_Y_TO = {lut.YTo};");
                    sw.WriteLine($"const double LUT_Y_STEP = {yStep};");
                    sw.WriteLine("const double LUT[] = {");
                    sw.Write(swTbl.ToString());
                    sw.WriteLine("};");
                    sw.WriteLine($"const int LUT_SIZE = sizeof(LUT) / sizeof(double); // {renderSize + 1}");

                    Application.Current.Clipboard.SetTextAsync(sw.ToString());
                };
                Grid.SetRow(btnCopyC, grInputs.RowDefinitions.Count - 1);
                Grid.SetColumnSpan(btnCopyC, 2);
                grInputs.Children.Add(btnCopyC);

                // refresh btn
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                var btnCalc = new Button() { Content = "Refresh graph", Margin = new Thickness(0, 10, 0, 0) };
                btnCalc.Click += (a, b) => refresh();
                Grid.SetRow(btnCalc, grInputs.RowDefinitions.Count - 1);
                Grid.SetColumnSpan(btnCalc, 2);
                grInputs.Children.Add(btnCalc);

                // plot view
                var pv = new PlotView();
                pv.Model = new PlotModel();
                Grid.SetRow(pv, 1);
                grRoot.Children.Add(pv);

                // f(x) formula display
                grInputs.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                var grFormulaRender = new Grid();
                Grid.SetRow(grFormulaRender, grInputs.RowDefinitions.Count - 1);
                Grid.SetColumnSpan(grFormulaRender, 2);
                grInputs.Children.Add(grFormulaRender);

                refresh = () =>
                {
                    Func<Entity, Entity> mySimplify = (e) =>
                    {
                        var q1 = e.Simplify().ToString();
                        var q2 = e.VeryExpensiveSimplify(2).ToString();

                        if (q2.Length < q1.Length)
                            return q2;
                        else
                            return q1;
                    };

                    Func<AngouriMath.Entity, string> toLatex = (e) =>
                    {
                        var res = mySimplify(e).Latexise().Replace("\\times", "\\cdot");

                        if (res.StartsWith("\\left\\{") && res.EndsWith("\\right\\}"))
                        {
                            res = res.StripBegin("\\left\\{");
                            res = res.StripEnd("\\right\\}").Trim();
                        }

                        return res;
                    };

                    var N = int.Parse(renderSizeTBox.Text);
                    var fx = fxTBox.Text;

                    grFormulaRender.Children.Clear();
                    grFormulaRender.Children.Add(new CSharpMath.Avalonia.MathView()
                    {
                        LaTeX = $"f(x)={toLatex(fx)}",
                        Margin = new Thickness(10)
                    });

                    pv.Model.Series.Clear();

                    var xFromVal = xFromTBox.Text.EvalNumerical().ToNumerics().Real;
                    var xToVal = xToTBox.Text.EvalNumerical().ToNumerics().Real;

                    var fxC = fx.Compile("x");
                    var fS = new OxyPlot.Series.LineSeries()
                    {
                        //Title = "f",
                        DataFieldX = "x",
                        DataFieldY = "y",
                        ItemsSource = SciToolkit.Range(
                            tol: 1e-3,
                            start: xFromVal,
                            end: xToVal,
                            inc: (xToVal - xFromVal) / N,
                            includeEnd: true).Select(x => new PlotData(x, fxC.Call(x).Real)),
                        Color = OxyColor.Parse(colors[0])
                    };
                    pv.Model.Series.Add(fS);

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

                };

                refresh();
            }
        }

        static void Main(string[] args)
        {
            GuiToolkit.CreateGui<MainWindow>();
        }
    }
}