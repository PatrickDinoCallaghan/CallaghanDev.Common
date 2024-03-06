using CallaghanDev.XML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

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
        public static void ExportJsonToExcel(string jsonString, string filePath)
        {
            // Ensure the file path ends with the .xlsx extension
            if (string.IsNullOrWhiteSpace(Path.GetExtension(filePath)))
            {
                filePath += ".xlsx";
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Parse the JSON string using System.Text.Json
                var jsonDocument = JsonDocument.Parse(jsonString);
                var objects = jsonDocument.RootElement.EnumerateArray();

                // Initialize a list to store all unique column names
                var columnNames = new HashSet<string>();

                // Retrieve all keys from the JSON objects
                if (objects.Any())
                {
                    foreach (var obj in objects)
                    {
                        foreach (var prop in obj.EnumerateObject())
                        {
                            columnNames.Add(prop.Name);
                        }
                    }

                    int column = 1;
                    // Create header row with column names
                    foreach (var columnName in columnNames)
                    {
                        worksheet.Cell(1, column++).Value = columnName;
                    }

                    int row = 2;
                    // Insert data rows
                    foreach (var obj in objects)
                    {
                        column = 1;
                        foreach (var columnName in columnNames)
                        {
                            if (obj.TryGetProperty(columnName, out var value))
                            {
                                worksheet.Cell(row, column).Value = value.ToString();
                            }
                            column++;
                        }
                        row++;
                    }
                }

                // Save the workbook to the specified file path
                workbook.SaveAs(filePath);
            }
        }


    }
}
