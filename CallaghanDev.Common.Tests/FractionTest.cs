using CallaghanDev.Utilities.Math;
using System.Numerics;

namespace CallaghanDev.Common.Tests
{
    public class FractionTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Constructor_ValidFraction_CreatesFraction()
        {
            var fraction = new Fraction(new BigInteger(3), new BigInteger(4));
            Assert.AreEqual(new BigInteger(3), fraction.Numerator);
            Assert.AreEqual(new BigInteger(4), fraction.Denominator);
            Assert.AreEqual(0, fraction.IntegerPart);
        }

        [Test]
        public void Constructor_ZeroDenominator_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Fraction(new BigInteger(1), new BigInteger(0)));
        }

        [Test]
        public void Simplify_Fraction_SimplifiesCorrectly()
        {
            var fraction = new Fraction(new BigInteger(2), new BigInteger(4));
            Assert.AreEqual(new BigInteger(1), fraction.Numerator);
            Assert.AreEqual(new BigInteger(2), fraction.Denominator);
        }

        [Test]
        public void DecimalValue_Get_ReturnsCorrectDecimalValue()
        {
            var fraction = new Fraction(new BigInteger(1), new BigInteger(2));
            Assert.AreEqual(0.5, fraction.DecimalValue);
        }

        [Test]
        public void DecimalValue_Set_SetsCorrectFraction()
        {
            var fraction = new Fraction(0.5);
            Assert.AreEqual(new BigInteger(1), fraction.Numerator);
            Assert.AreEqual(new BigInteger(2), fraction.Denominator);
        }

        [Test]
        public void FromString_ValidString_CreatesFraction()
        {
            var fraction = new Fraction("1,(3/4)");
            Assert.AreEqual(1, fraction.IntegerPart);
            Assert.AreEqual(new BigInteger(3), fraction.Numerator);
            Assert.AreEqual(new BigInteger(4), fraction.Denominator);
        }

        [Test]
        public void FromString_InvalidString_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => new Fraction("invalid"));
        }

        [Test]
        public void Operator_Multiplication_MultipliesCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(2));
            var b = new Fraction(new BigInteger(3), new BigInteger(4));
            var result = a * b;
            Assert.AreEqual(new BigInteger(3), result.Numerator);
            Assert.AreEqual(new BigInteger(8), result.Denominator);
        }

        [Test]
        public void Operator_Division_DividesCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(2));
            var b = new Fraction(new BigInteger(1), new BigInteger(4));
            var result = a / b;
            Assert.AreEqual(new BigInteger(6), result.Numerator);
            Assert.AreEqual(new BigInteger(4), result.Denominator);
        }

        [Test]
        public void Operator_Addition_AddsCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(2));
            var b = new Fraction(new BigInteger(1), new BigInteger(4));
            var result = a + b;
            Assert.AreEqual(new Fraction(0.75), result);
        }

        [Test]
        public void Operator_Subtraction_SubtractsCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(2));
            var b = new Fraction(new BigInteger(1), new BigInteger(4));
            var result = a - b;
            Assert.AreEqual(new Fraction(0.25), result);
        }

        [Test]
        public void Operator_Equality_ComparesCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(2));
            var b = new Fraction(new BigInteger(1), new BigInteger(2));
            Assert.IsTrue(a == b);
        }

        [Test]
        public void Operator_Inequality_ComparesCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(2));
            var b = new Fraction(new BigInteger(1), new BigInteger(3));
            Assert.IsTrue(a != b);
        }

        [Test]
        public void Operator_GreaterThan_ComparesCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(2));
            var b = new Fraction(new BigInteger(1), new BigInteger(3));
            Assert.IsTrue(a > b);
        }

        [Test]
        public void Operator_LessThan_ComparesCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(3));
            var b = new Fraction(new BigInteger(1), new BigInteger(2));
            Assert.IsTrue(a < b);
        }

        [Test]
        public void Operator_Exponentiation_RaisesCorrectly()
        {
            var a = new Fraction(new BigInteger(2), new BigInteger(3));
            var result = a ^ 2;
            Assert.AreEqual(new BigInteger(4), result.Numerator);
            Assert.AreEqual(new BigInteger(9), result.Denominator);
        }

        [Test]
        public void ImplicitConversion_Int_ConvertsCorrectly()
        {
            Fraction fraction = 5;
            Assert.AreEqual(5, fraction.IntegerPart);
            Assert.AreEqual(new BigInteger(1), fraction.Denominator);
            Assert.AreEqual(new BigInteger(0), fraction.Numerator);
        }

        [Test]
        public void ImplicitConversion_Double_ConvertsCorrectly()
        {
            Fraction fraction = 0.5;
            Assert.AreEqual(new BigInteger(1), fraction.Numerator);
            Assert.AreEqual(new BigInteger(2), fraction.Denominator);
        }

        [Test]
        public void ImplicitConversion_String_ConvertsCorrectly()
        {
            Fraction fraction = "1,(1/2)";
            Assert.AreEqual(1, fraction.IntegerPart);
            Assert.AreEqual(new BigInteger(1), fraction.Numerator);
            Assert.AreEqual(new BigInteger(2), fraction.Denominator);
        }

        [Test]
        public void Equals_Object_EqualsCorrectly()
        {
            var a = new Fraction(new BigInteger(1), new BigInteger(2));
            var b = new Fraction(new BigInteger(1), new BigInteger(2));
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void GetHashCode_GeneratesCorrectHashCode()
        {
            var fraction = new Fraction(new BigInteger(1), new BigInteger(2));
            var expectedHashCode = HashCode.Combine(0, new BigInteger(1), new BigInteger(2));
            Assert.AreEqual(expectedHashCode, fraction.GetHashCode());
        }
    }
}