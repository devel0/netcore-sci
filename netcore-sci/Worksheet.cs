using ClosedXML.Excel;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// set (row,col) to given value
        /// </summary>
        /// <param name="ws">worksheet to set cell</param>
        /// <param name="row">cell row number</param>
        /// <param name="col">cell col number</param>
        /// <param name="val">cell value</param>
        /// <returns>(row,col) cell updated</returns>
        public static IXLCell SetCell(this IXLWorksheet ws, int row, int col, object val)
        {
            var cell = ws.Cell(row, col);

            cell.Value = val;

            return cell;
        }

        /// <summary>
        /// set cell font color
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="foreground">font color</param>        
        /// <returns>given cell updated</returns>
        public static IXLCell SetFontColor(this IXLCell cell, XLColor foreground)
        {
            cell.Style.Font.SetFontColor(foreground);

            return cell;
        }

        /// <summary>
        /// set cell background color
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="background">background color</param>        
        /// <returns>given cell updated</returns>
        public static IXLCell SetBgColor(this IXLCell cell, XLColor background)
        {
            cell.Style.Fill.SetBackgroundColor(background);

            return cell;
        }

        /// <summary>
        /// set bold on given cell
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>given cell updated</returns>
        public static IXLCell SetBold(this IXLCell cell)
        {
            cell.Style.Font.SetBold();
            return cell;
        }

        /// <summary>
        /// set (row,col) to given value and set bold
        /// </summary>
        /// <param name="ws">worksheet to set cell</param>
        /// <param name="row">cell row number</param>
        /// <param name="col">cell col number</param>
        /// <param name="val">cell value</param>
        /// <returns>(row,col) cell updated</returns>
        public static IXLCell SetCellBold(this IXLWorksheet ws, int row, int col, object val) =>
            ws.SetCell(row, col, val).SetBold();

        /// <summary>
        /// set (row,col) with given R1C1 formula
        /// </summary>
        /// <param name="ws">worksheet to set cell</param>
        /// <param name="row">cell row number</param>
        /// <param name="col">cell col number</param>
        /// <param name="formulaR1C1">r1c1 formula ( eg. "RC[-1]*2" double the cell with at same row and previous column )</param>
        /// <returns>(row,col) cell updated</returns>
        public static IXLCell SetCellFormulaR1C1(this IXLWorksheet ws, int row, int col, string formulaR1C1)
        {
            var cell = ws.Cell(row, col);

            cell.FormulaR1C1 = formulaR1C1;

            return cell;
        }

    }

    /// <summary>
    /// worksheet finalization input variables
    /// </summary>
    public class FinalizeWorksheetInput
    {
        /// <summary>
        /// if not null then autofilter will applied to range
        /// (AutoFilterHeaderRow, AutoFilterFirstCol,
        /// UsedRowCount-AutoFilterHeaderRow+1, UsedColumnCount-AutoFilterFirstCol+1).
        /// i.e. AutoFilterHeaderRow=AutoFilterFirstCol=1 will apply autofilter to entire sheet using first row as header
        /// </summary>        
        public int? AutoFilterHeaderRow { get; set; } = null;

        /// <summary>
        /// used in conjunction with non null AutoFilterHeaderRow
        /// </summary>        
        public int AutoFilterFirstCol { get; set; } = 1;

        /// <summary>
        /// if true then column width will resized to fit content ( note : may overhead ).
        /// don't use this with SetDefaultColWidth
        /// </summary>        
        public bool AdjustContents { get; set; } = false;

        /// <summary>
        /// set all column width at once
        /// </summary>        
        public double? SetDefaultColWidth { get; set; } = null;

        /// <summary>
        /// freeze specified worksheet row count
        /// </summary>
        public int? FreezeRow = null;

        /// <summary>
        /// freeze specified worksheet columns count
        /// </summary>
        public int? FreezeCol = null;

        /// <summary>
        /// set worksheet zoomscale ( 100 = 100%, 150 = 150%, ... )
        /// </summary>
        public int? ZoomScale = null;
    }

    /// <summary>
    /// worksheet finalization output variables
    /// </summary>
    public class FinalizeWorksheetOutput
    {
        /// <summary>
        /// worksheet range used
        /// </summary>        
        public IXLRange RangeUsed { get; set; }

        /// <summary>
        /// worksheet column count
        /// </summary>
        /// <value></value>
        public int ColumnCount { get; set; }

        /// <summary>
        /// worksheet rows count
        /// </summary>        
        public int RowCount { get; set; }
    }

    public static partial class SciExt
    {

        /// <summary>
        /// finalize worksheet applying some recurrent operation
        /// </summary>
        /// <param name="ws">worksheet to finalize</param>
        /// <param name="input">finalization input parameters</param>
        /// <returns>finalization output variables</returns>
        public static FinalizeWorksheetOutput FinalizeWorksheet(
            this IXLWorksheet ws,
            FinalizeWorksheetInput input)
        {
            var res = new FinalizeWorksheetOutput
            {
                RangeUsed = ws.RangeUsed()
            };

            res.RowCount = res.RangeUsed.RowCount();
            res.ColumnCount = res.RangeUsed.ColumnCount();

            if (input.AutoFilterHeaderRow.HasValue)
            {
                ws.Range(
                    input.AutoFilterHeaderRow.Value, input.AutoFilterFirstCol,
                    res.RowCount - input.AutoFilterHeaderRow.Value + 1,
                    res.ColumnCount - input.AutoFilterFirstCol + 1).SetAutoFilter();
            }
            if (input.AdjustContents)
            {
                for (int c = 1; c <= res.ColumnCount; c++) ws.Column(c).AdjustToContents();
            }
            if (input.SetDefaultColWidth.HasValue)
            {
                for (int c = 1; c <= res.ColumnCount; c++) ws.Column(c).Width = input.SetDefaultColWidth.Value;
            }
            if (input.FreezeRow.HasValue && !input.FreezeCol.HasValue)
            {
                ws.SheetView.FreezeRows(input.FreezeRow.Value);
            }
            else if (!input.FreezeRow.HasValue && input.FreezeCol.HasValue)
            {
                ws.SheetView.FreezeColumns(input.FreezeCol.Value);
            }
            else if (input.FreezeRow.HasValue && input.FreezeCol.HasValue)
            {
                ws.SheetView.Freeze(input.FreezeRow.Value, input.FreezeCol.Value);
            }
            if (input.ZoomScale.HasValue)
            {
                ws.SheetView.ZoomScale = input.ZoomScale.Value;
            }

            return res;
        }

    }

}