using System;
using CallaghanDev.XML.Excel;
using System.Reflection;
using CallaghanDev.Utilities.ConsoleHelper;
using CallaghanDev.XML.Extensions;
using CallaghanDev.ConsoleAppTest.TestClasses;
using DocumentFormat.OpenXml.InkML;

namespace CallaghanDev.ConsoleAppTest
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            TestMatrix();
            Console.WriteLine("Complete");
            cnsl.Exit();
        }
        public static void TestMatrix()
        {
            CallaghanDev.Utilities.MathTools.Matrix<int> matrix = new CallaghanDev.Utilities.MathTools.Matrix<int>();
            matrix[1, 2] = 5;
            matrix[1, 1] = 6;

            matrix.ExportToFile("Cool");

            matrix = CallaghanDev.Utilities.MathTools.Matrix<int>.LoadFromFile("Cool");
            Console.WriteLine(matrix.ToString());
        }
        public static void TestXML()
        {
            List<ExcelTestClass> excelClassList = new List<ExcelTestClass>
            {
                new ExcelTestClass { Name = "TestName1", Description = "Description1", SomeIntVal = 213, SomeDoubleVal = 2323.123123, SomeFloatVal = 2131231.123123f },
                new ExcelTestClass { Name = "TestName2", Description = "Description2", SomeIntVal = 214, SomeDoubleVal = 2324.123123, SomeFloatVal = 2131232.123123f },
                new ExcelTestClass { Name = "TestName3", Description = "Description3", SomeIntVal = 215, SomeDoubleVal = 2325.123123, SomeFloatVal = 2131233.123123f },
                new ExcelTestClass { Name = "TestName4", Description = "Description4", SomeIntVal = 216, SomeDoubleVal = 2326.123123, SomeFloatVal = 2131234.123123f },
                new ExcelTestClass { Name = "TestName5", Description = "Description5", SomeIntVal = 217, SomeDoubleVal = 2327.123123, SomeFloatVal = 2131235.123123f }
            };
            excelClassList.ExportListToExcel<ExcelTestClass>("File");
        }

    }


}