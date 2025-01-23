using CallaghanDev.XML.Extensions;
using CallaghanDev.ConsoleAppTest.TestClasses;
using CallaghanDev.Utilities.Math;
using CallaghanDev.Common.Math;

namespace CallaghanDev.ConsoleAppTest
{

    public static class Program
    {
        public static void TestPolynomialFunction( )
        {
            Scalar cool = new Scalar(-1);
            PolynomialFunction polynomial ="x-1+x^2-x";

            PolynomialFunction polynomial2 = "x+1";

            Console.WriteLine(polynomial* polynomial2);


        }

        public static void Main(string[] args)
        {
            // Example points: (1, 2), (2, 3), (3, 5)
            var points = new List<(decimal x, decimal y)>
            {
                (1, 2),
                (2, 3),
                (3, 5)
            }; TestPolynomialFunction();

            Console.WriteLine(PolynomialFunction.GenerateLagrangePolynomial(points));


        }
        public static void TestFractions()
        {
            // Test constructors
            Scalar a = "1/2";  // From string constructor
            Scalar b = 1.5;    // From double constructor
            Scalar c = new Scalar(3, 4);  // From numerator and denominator constructor
            Scalar d = "1+1/3";  // From string with integer part constructor
            Scalar e = new Scalar(2);  // From int constructor
            Scalar f = (decimal)0.75;  // From decimal constructor

            // Test additional constructors and edge cases
            Scalar zero = new Scalar(0); // Integer zero
            Scalar negativeFraction = new Scalar(-3, 4); // Negative fraction
            Scalar wholeNumberFraction = new Scalar("4/2"); // Fraction equal to whole number (2)
            Scalar verySmallDecimal = new Scalar(0.00001); // Small decimal
            Scalar largeNumerator = new Scalar(1000000, 3); // Large numbers
            Scalar largeDenominator = new Scalar(1, 1000000); // Small fraction

            // Test fraction simplification and improper fraction handling
            Scalar simplifyTest = new Scalar(6, 8); // Should simplify to 3/4
            Scalar improperFraction = new Scalar(9, 4); // Should handle improper fraction as 2,1/4

            // Test basic arithmetic operators
            Scalar sum = a + b;
            Scalar difference = c - a;
            Scalar product = a * b;
            Scalar quotient = c / a;

            // Test more advanced arithmetic operations
            Scalar largeProduct = e * d; // Product of integer and fraction
            Scalar largeQuotient = e / a; // Division of integer by fraction
            Scalar additionWithNegative = negativeFraction + a; // Adding positive and negative fractions
            Scalar subtractionWithNegative = d - negativeFraction; // Subtracting a negative fraction

            // Test exponentiation
            Scalar exp = d ^ "1/7";
            Scalar square = a ^ 2; // Simple square
            Scalar cube = b ^ 3; // Cube of a fraction
            Scalar rootExp = c ^ 0.5; // Square root of fraction
            Console.WriteLine(exp);
            // Test comparisons
            bool isEqual = a == new Scalar(1, 2);
            bool isNotEqual = b != new Scalar(3, 2);
            bool isGreater = c > a;
            bool isLess = b < d;
            bool isEqualToWholeNumber = e == new Scalar(2, 1); // Whole number comparison
            bool isGreaterThanNegative = c > negativeFraction; // Positive vs negative
            bool zeroEquality = zero == new Scalar(0); // Zero equality
            bool largeNumberComparison = largeNumerator > d; // Comparison with large numbers

            // Test implicit conversions
            Scalar intImplicit = 3; // Implicit int to fraction
            Scalar doubleImplicit = 0.25; // Implicit double to fraction

            // Test decimal representation accuracy
            double expectedDecimalA = 0.5;
            double expectedDecimalB = 1.5;
            double expectedDecimalC = 0.75;
            double expectedDecimalD = 1.3333333333333333;
            double tolerance = 1e-10; // Allowable tolerance for decimal representation

            // Output results
            Console.WriteLine($"a = {a} (Decimal: {a.DecimalValue})");
            Console.WriteLine($"b = {b} (Decimal: {b.DecimalValue})");
            Console.WriteLine($"c = {c} (Decimal: {c.DecimalValue})");
            Console.WriteLine($"d = {d} (Decimal: {d.DecimalValue})");
            Console.WriteLine($"e = {e} (Decimal: {e.DecimalValue})");
            Console.WriteLine($"f = {f} (Decimal: {f.DecimalValue})");
            Console.WriteLine($"zero = {zero} (Decimal: {zero.DecimalValue})");
            Console.WriteLine($"negativeFraction = {negativeFraction} (Decimal: {negativeFraction.DecimalValue})");
            Console.WriteLine($"wholeNumberFraction = {wholeNumberFraction} (Decimal: {wholeNumberFraction.DecimalValue})");
            Console.WriteLine($"largeNumerator = {largeNumerator} (Decimal: {largeNumerator.DecimalValue})");
            Console.WriteLine($"largeDenominator = {largeDenominator} (Decimal: {largeDenominator.DecimalValue})");

            Console.WriteLine($"Sum (a + b) = {a} + {b} =  {sum} (Decimal: {sum.DecimalValue})");
            Console.WriteLine($"Difference (c - a) = ({c} - {a}) {difference} (Decimal: {difference.DecimalValue})");
            Console.WriteLine($"Product (a * b) = ({a} * {b}) = {product} (Decimal: {product.DecimalValue})");
            Console.WriteLine($"Quotient (c / a) = ({c} / {a}) = {quotient} (Decimal: {quotient.DecimalValue})");

            Console.WriteLine($"Exponentiation (d ^ 2/3) = ({d} ^ (2/3)) = ({d} ^ 2/3) = {exp} (Decimal: {exp.DecimalValue})");
            Console.WriteLine($"Exponentiation (a ^ 2) = ({a} ^ 2) = {square} (Decimal: {square.DecimalValue})");
            Console.WriteLine($"Exponentiation (b ^ 3) = ({b} ^ 3) = {cube} (Decimal: {cube.DecimalValue})");
            Console.WriteLine($"Square root (c ^ 0.5) = {rootExp} (Decimal: {rootExp.DecimalValue})");

            Console.WriteLine($"Simplified Fraction of (6/8) = {simplifyTest} (Decimal: {simplifyTest.DecimalValue})");
            Console.WriteLine($"Improper Fraction (9/4) as mixed = {improperFraction} (Decimal: {improperFraction.DecimalValue})");

            Console.WriteLine($"Integer implicit = 3 -> {intImplicit} (Decimal: {intImplicit.DecimalValue})");
            Console.WriteLine($"Double implicit = 0.25 -> {doubleImplicit} (Decimal: {doubleImplicit.DecimalValue})");

            Console.WriteLine($"a == 1/2: {isEqual}");
            Console.WriteLine($"b != 3/2: {isNotEqual}");
            Console.WriteLine($"c > a: {isGreater}");
            Console.WriteLine($"b < d: {isLess}");
            Console.WriteLine($"e == 2: {isEqualToWholeNumber}");
            Console.WriteLine($"c > negativeFraction: {isGreaterThanNegative}");
            Console.WriteLine($"zero == 0: {zeroEquality}");
            Console.WriteLine($"largeNumerator > d: {largeNumberComparison}");

            // Decimal representation checks
            Console.WriteLine($"a decimal is approximately {expectedDecimalA}: {System.Math.Abs(a.DecimalValue - expectedDecimalA) < tolerance}");
            Console.WriteLine($"b decimal is approximately {expectedDecimalB}: {System.Math.Abs(b.DecimalValue - expectedDecimalB) < tolerance}");
            Console.WriteLine($"c decimal is approximately {expectedDecimalC}: {System.Math.Abs(c.DecimalValue - expectedDecimalC) < tolerance}");
            Console.WriteLine($"d decimal is approximately {expectedDecimalD}: {System.Math.Abs(d.DecimalValue - expectedDecimalD) < tolerance}");
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