using CallaghanDev.XML.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.ConsoleAppTestFramework.TestClasses
{
    public class ExcelTestClass
    {
        [ExcelExportColumn(1)]
        public string Name { get; set; }

        [ExcelExportColumn(2)]
        public string Description { get; set; }

        [ExcelExportColumn(3)]
        public int SomeIntVal { get; set; }

        [ExcelExportColumn(5)]
        public double SomeDoubleVal { get; set; }

        [ExcelExportColumn(6)]
        public float SomeFloatVal { get; set; }

    }
}
