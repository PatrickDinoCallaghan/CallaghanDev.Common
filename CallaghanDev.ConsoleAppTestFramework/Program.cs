using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallaghanDev.ConsoleAppTestFramework.TestClasses;

using CallaghanDev.XML.Extensions;
namespace CallaghanDev.ConsoleAppTestFramework
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            TestXML();
            Console.WriteLine("Complete");
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
