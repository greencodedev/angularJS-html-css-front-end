using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils.Excel
{
    public class NpoiUtils
    {
        public static ICellStyle GetHeaderStyle(IWorkbook workbook)
        {
            IFont font = workbook.CreateFont();
            font.Boldweight = (short)FontBoldWeight.Bold;

            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.SetFont(font);

            return headerStyle;
        }

        public static ICell CreateCell(IRow row, int column, int value)
        {
            ICell cell = row.CreateCell(column);
            cell.SetCellValue(value);
            return cell;
        }

        public static ICell CreateCell(IRow row, int column, double value)
        {
            ICell cell = row.CreateCell(column);
            cell.SetCellValue(value);
            return cell;
        }

        public static ICell CreateCell(IRow row, int column, bool value)
        {
            ICell cell = row.CreateCell(column);
            cell.SetCellValue(value);
            return cell;
        }

        public static ICell CreateCell(IRow row, int column, string value)
        {
            return CreateCell(row, column, value, null);
        }

        public static ICell CreateCell(IRow row, int column, string value, ICellStyle style)
        {
            return CreateCell(row, column, value, style, null);
        }

        public static ICell CreateCell(IRow row, int column, string value, ICellStyle style, string link)
        {
            ICell cell = row.CreateCell(column);
            cell.SetCellValue(value);

            if (style != null)
            {
                cell.CellStyle = style;
            }

            if (!string.IsNullOrEmpty(link))
            {
                IHyperlink hyperLink = new HSSFHyperlink(HyperlinkType.Url)
                {
                    Address = link
                };
                cell.Hyperlink = hyperLink;
            }

            return cell;
        }

        public static string GetCellStringValue(ICell cell)
        {
            if (cell == null)
                return null;

            if (cell.CellType == CellType.Numeric)
            {
                return cell.NumericCellValue.ToString();
            }

            return cell.StringCellValue;
        }

        public static int GetCellIntValue(ICell cell)
        {
            if (cell == null)
                return 0;

            if (cell.CellType == CellType.Numeric)
            {
                return cell != null ? (int)cell.NumericCellValue : 0;
            }
            else if (cell.CellType == CellType.Formula)
            {
                cell.SetCellType(CellType.String);
            }

            return UtilsLib.ParseIntOrZero(GetCellStringValue(cell));
        }

        public static void CreateBorder(ISheet sheet, int firstRow, int lastRow, int firstColumn, int lastColumn, BorderStyle borderStyle)
        {
            // top line
            for (int column = firstColumn + 1; column < lastColumn; column++)
            {
                ICell topCell = GetCell(sheet, firstRow, column);
                ICellStyle topStyle = CreateCellStyle(topCell);
                using (new CellBorderLock(topStyle))
                {
                    topStyle.BorderTop = borderStyle;
                }
                topCell.CellStyle = topStyle;
            }
            // top left corner
            ICell topLeftCell = GetCell(sheet, firstRow, firstColumn);
            ICellStyle topLeftStyle = CreateCellStyle(topLeftCell);
            using (new CellBorderLock(topLeftStyle))
            {
                topLeftStyle.BorderTop = borderStyle;
                topLeftStyle.BorderLeft = borderStyle;
            }
            topLeftCell.CellStyle = topLeftStyle;
            // top right corner
            ICell topRightCell = GetCell(sheet, firstRow, lastColumn);
            ICellStyle topRightStyle = CreateCellStyle(topRightCell);
            using (new CellBorderLock(topRightStyle))
            {
                topRightStyle.BorderTop = borderStyle;
                topRightStyle.BorderRight = borderStyle;
            }
            topRightCell.CellStyle = topRightStyle;

            // left line
            for (int row = firstRow + 1; row < lastRow; row++)
            {
                ICell leftCell = GetCell(sheet, row, firstColumn);
                ICellStyle leftStyle = CreateCellStyle(leftCell);
                using (new CellBorderLock(leftStyle))
                {
                    leftStyle.BorderLeft = borderStyle;
                }
                leftCell.CellStyle = leftStyle;
            }

            // right line
            for (int row = firstRow + 1; row < lastRow; row++)
            {
                ICell rightCell = GetCell(sheet, row, lastColumn);
                ICellStyle rightStyle = CreateCellStyle(rightCell);
                using (new CellBorderLock(rightStyle))
                {
                    rightStyle.BorderRight = borderStyle;
                }
                rightCell.CellStyle = rightStyle;
            }

            // bottom line
            for (int column = firstColumn + 1; column < lastColumn; column++)
            {
                ICell bottomCell = GetCell(sheet, lastRow, column);
                ICellStyle bottomStyle = CreateCellStyle(bottomCell);
                using (new CellBorderLock(bottomStyle))
                {
                    bottomStyle.BorderBottom = borderStyle;
                }
                bottomCell.CellStyle = bottomStyle;
            }

            // bottom left corner
            ICell bottomLeftCell = GetCell(sheet, lastRow, firstColumn);
            ICellStyle bottomLeftStyle = CreateCellStyle(bottomLeftCell);
            using (new CellBorderLock(bottomLeftStyle))
            {
                bottomLeftStyle.BorderBottom = borderStyle;
                bottomLeftStyle.BorderLeft = borderStyle;
            }
            bottomLeftCell.CellStyle = bottomLeftStyle;

            // bottom right corner
            ICell bottomRightCell = GetCell(sheet, lastRow, lastColumn);
            ICellStyle bottomRightStyle = CreateCellStyle(bottomRightCell);
            using (new CellBorderLock(bottomRightStyle))
            {
                bottomRightStyle.BorderBottom = borderStyle;
                bottomRightStyle.BorderRight = borderStyle;
            }
            bottomRightCell.CellStyle = bottomRightStyle;
        }

        private static ICellStyle CreateCellStyle(ICell cell)
        {
            var style = cell.Sheet.Workbook.CreateCellStyle();
            style.CloneStyleFrom(cell.CellStyle);
            return style;
        }

        private static ICell GetCell(ISheet sheet, int row, int column)
        {
            IRow r = sheet.GetRow(row) ?? sheet.CreateRow(row);
            return r.GetCell(column) ?? r.CreateCell(column);
        }
    }

    public sealed class CellBorderLock : IDisposable
    {
        private readonly ICellStyle style;

        public CellBorderLock(ICellStyle style)
        {
            this.style = style;
            style.BorderDiagonalLineStyle = BorderStyle.Thin;
            style.BorderDiagonal = BorderDiagonal.Forward;
        }

        public void Dispose()
        {
            style.BorderDiagonalLineStyle = BorderStyle.None;
            style.BorderDiagonal = BorderDiagonal.None;
        }
    }
}