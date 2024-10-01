using CallaghanDev.Utilities.ConsoleHelper;
using CallaghanDev.XML.Extensions;
using CallaghanDev.ConsoleAppTest.TestClasses;
using CallaghanDev.Utilities.Math;
using CallaghanDev.Common.Math;

namespace CallaghanDev.ConsoleAppTest
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            //TestMatrix();
            TestFractions();
            Console.WriteLine("Complete");
            cnsl.Exit();
        }

        public static void TestFractions()
        {
            // Test constructors
            Fraction a = "1/2";  // From string constructor
            Fraction b = 1.5;    // From double constructor
            Fraction c = new Fraction(3, 4);  // From numerator and denominator constructor
            Fraction d = "1,1/3";  // From string with integer part constructor
            Fraction e = new Fraction(2);  // From int constructor
            Fraction f = (double)0.75;  // From decimal constructor

            // Test basic arithmetic operators
            Fraction sum = a + b;
            Fraction difference = c - a;
            Fraction product = a * b;
            Fraction quotient = c / a;

            // Test exponentiation
            Fraction exp = d ^ "2/3";

            // Test comparisons
            bool isEqual = a == new Fraction(1, 2);
            bool isNotEqual = b != new Fraction(3, 2);
            bool isGreater = c > a;
            bool isLess = b < d;

            // Output results
            Console.WriteLine($"a = {a} (Decimal: {a.DecimalValue})");
            Console.WriteLine($"b = {b} (Decimal: {b.DecimalValue})");
            Console.WriteLine($"c = {c} (Decimal: {c.DecimalValue})");
            Console.WriteLine($"d = {d} (Decimal: {d.DecimalValue})");
            Console.WriteLine($"e = {e} (Decimal: {e.DecimalValue})");
            Console.WriteLine($"f = {f} (Decimal: {f.DecimalValue})");

            Console.WriteLine($"Sum (a + b) = {sum} (Decimal: {sum.DecimalValue})");
            Console.WriteLine($"Difference (c - a) = {difference} (Decimal: {difference.DecimalValue})");
            Console.WriteLine($"Product (a * b) = {product} (Decimal: {product.DecimalValue})");
            Console.WriteLine($"Quotient (c / a) = {quotient} (Decimal: {quotient.DecimalValue})");

            Console.WriteLine($"Exponentiation (d ^ 2/3) = {exp} (Decimal: {exp.DecimalValue})");

            Console.WriteLine($"a == 1/2: {isEqual}");
            Console.WriteLine($"b != 3/2: {isNotEqual}");
            Console.WriteLine($"c > a: {isGreater}");
            Console.WriteLine($"b < d: {isLess}");
        }

        public static void TestMatrix()
        {
            Matrix<int> matrix = new  Matrix<int>();
            matrix[1, 2] = 5;
            matrix[1, 1] = 6;

            matrix.ExportToFile("Cool");

            matrix = Matrix<int>.LoadFromFile("Cool");
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