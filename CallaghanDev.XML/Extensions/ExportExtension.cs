using CallaghanDev.XML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CallaghanDev.XML.Extensions
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelExportColumn : System.Attribute
    {
        public int ColumnNumber { get; }
        public string ColumnName { get; }

        public ExcelExportColumn(int columnNumber)
        {
            ColumnNumber = columnNumber;
        }


        public ExcelExportColumn(int columnNumber, string columnName)
        {
            ColumnNumber = columnNumber;
            ColumnName = columnName;
        }
    }
    public static class ExportExtension
    {
        public static void ExportListToExcel<T>(this List<T> objects, string filePath) where T : class
        {
            if (string.IsNullOrWhiteSpace(Path.GetExtension(filePath)))
            {
                filePath += ".xlsx";
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                          .Where(p => p.GetGetMethod(true) != null && System.Attribute.IsDefined(p, typeof(ExcelExportColumn)))
                                          .OrderBy(p => ((ExcelExportColumn)p.GetCustomAttribute(typeof(ExcelExportColumn), true)).ColumnNumber);

                foreach (var prop in properties)
                {
                    var attribute = (ExcelExportColumn)prop.GetCustomAttribute(typeof(ExcelExportColumn), true);
 
                    string columnTitle = string.IsNullOrEmpty(attribute.ColumnName) ? prop.Name : attribute.ColumnName;
                    int column = attribute.ColumnNumber;

                    worksheet.Cell(1, column).Value = columnTitle;

                    int row = 2;
                    foreach (var obj in objects)
                    {
                        worksheet.Cell(row, column).Value = prop.GetValue(obj, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null)?.ToString() ?? string.Empty;
                        row++;
                    }
                }

                workbook.SaveAs(filePath);
            }
        }
    }
}
