using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.MathTools
{
    /// <summary>
    /// Represents a fractional number between -1 and 1 with fixed-point decimal handling using a short, 
    /// effectively creating a 2-byte float. While operations with Fraction may be slower than with float 
    /// types, they are typically faster than with decimal types. The Fraction structure is designed to 
    /// offer greater precision for values strictly between -1 and 1 compared to floats after binary opperations,
    /// due to its fixed-point decimal handling and specific scale. It balances between medium precision and low memory 
    /// usage while offering medium CPU computational efficiency. In contrast, floats provide high CPU 
    /// computational efficiency and lower precision with medium memory usage, whereas decimals offer high 
    /// precision at the cost of high memory usage and lower CPU computational efficiency.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Fraction : ISerializable
    {
        private short Data;  // -32,768 to 32,767

        #region Constants
        const long invDivisor = 429503;
        const short DecimalPrecision = 10000;
        const int FractionPrecision = 1000000; // Adjust based on required precision for the fraction.
        const float FloatConverter = 10000f;
        #endregion

        public float floatValue
        {
            get { return Data / FloatConverter; } // Simplified conversion to float
            set { Data = (short)(value * DecimalPrecision); }
        }
        public float decimalValue
        {
            get { return Data / FloatConverter; } // Simplified conversion to float
            set { Data = (short)(value * DecimalPrecision); }
        }
        public Fraction(short Numerator, short Denominator)
        {
            if (Numerator > Denominator)
            {
                throw new ArgumentException("Numerator should not be greater than Denominator.");
            }

            // This ensures the fraction is scaled according to a predefined scale (e.g., 1000 for milli-units).
            Data = (short)(((float)Numerator / (float)Denominator) * DecimalPrecision);
        }
        public Fraction(float InData)
        {
            floatValue = InData;
        }
        public Fraction(decimal InData)
        {
            decimal shifted = InData * DecimalPrecision;
            Data = (short)(shifted);
        }

        #region Operator Overloads
        private Fraction(short InData)
        {
            Data = InData;
        }
        public static Fraction operator *(Fraction a, Fraction b)
        {
            unchecked
            {
                // Correct the multiplication to match with the fixed-point representation logic.
                long result = a.Data * b.Data; // Multiplication in their scale.

                result = (result * invDivisor) >> 32; // Adjust the result by the inverse divisor and bit shift.

                return new Fraction((short)result);
            }
        }
        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b.Data == 0)
            {
                return new Fraction(32767);
            }
            //This approach maintains integer arithmetic for better CPU efficiency.
            int scaledA = a.Data * DecimalPrecision;

            int result = scaledA / b.Data;

            return new Fraction((short)result);
        }
        public static Fraction operator +(Fraction a, Fraction b)
        {
            unchecked
            {
                int result = a.Data + b.Data;
                return new Fraction((short)result);
            }
        }
        public static Fraction operator -(Fraction a, Fraction b)
        {
            unchecked
            {
                int result = a.Data - b.Data;
                return new Fraction((short)result);
            }
        }
        public static Fraction operator -(Fraction a)
        {
            unchecked
            {
                a.Data = (short)-a.Data;
                return a;
            }
        }

        public static bool operator ==(Fraction a, Fraction b)
        {
            if (!(a > b) || (b > a))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool operator !=(Fraction a, Fraction b)
        {

            if ((a > b) || (b > a))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public static bool operator >(Fraction a, Fraction b)
        {
            return (a.Data > b.Data);
        }
        public static bool operator <(Fraction a, Fraction b)
        {
            return (b > a);
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return FloatToFractionString(floatValue);
        }
        public override readonly bool Equals(object obj)
        {
            if (obj is Fraction other)
            {
                return this.Data == other.Data;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
        #endregion

        #region Private Helpers
        private string FloatToFractionString(float decimalValue)
        {
            decimalValue = Math.Abs(decimalValue);

            // Define precision for the conversion to avoid floating-point inaccuracies.
            long numerator = (long)(decimalValue * FractionPrecision);

            // Simplify the fraction (using the Greatest Common Divisor).
            long gcd = GreatestCommonDivisor(numerator, FractionPrecision);
            numerator /= gcd;
            long denominator = FractionPrecision / gcd;

            return ((decimalValue < 0) ? "-" : "") + numerator.ToString() + "/" + denominator.ToString();
        }
        private long GreatestCommonDivisor(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
        #endregion

        #region Serialization
        // Add a special constructor for deserialization
        private Fraction(SerializationInfo info, StreamingContext context)
        {
            // Only deserialize the Data field
            Data = info.GetInt16("Data");
        }

        // Implement the GetObjectData method for serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Only serialize the Data field
            info.AddValue("Data", Data);
        }
        #endregion
    }
    public class FractionPerformanceTests
    {
        private const int NumberOfOpperations = 9999999;
        public void MutiplyPerforamce()
        {
            float[] array1 = new float[NumberOfOpperations];
            float[] array2 = new float[NumberOfOpperations];


            Utilities.MathTools.Fraction[] Fractionarray1 = new Utilities.MathTools.Fraction[NumberOfOpperations];
            Utilities.MathTools.Fraction[] Fractionarray2 = new Utilities.MathTools.Fraction[NumberOfOpperations];

            Random random = new Random(); // Instance of Random for generating numbers

            for (int i = 0; i < NumberOfOpperations; i++)
            {
                // Generating random double and converting to decimal
                array1[i] = (float)(2 * random.NextDouble() - 1); // This will generate values between -1 and 1
                array2[i] = (float)(2 * random.NextDouble() - 1); // This will generate values between -1 and 1

                Fractionarray1[i] = new Utilities.MathTools.Fraction((decimal)array1[i]);
                Fractionarray2[i] = new Utilities.MathTools.Fraction((decimal)array2[i]);
            }

            HashSet<decimal> savevals = new HashSet<decimal>();
            var Traditional = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < NumberOfOpperations; i++)
            {
                float newval = array1[i] * array2[i];
            }
            Traditional.Stop();
            var newc = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < NumberOfOpperations; i++)
            {
                Utilities.MathTools.Fraction mtuli = Fractionarray1[i] * Fractionarray2[i];
            }
            newc.Stop();

            Console.WriteLine($"Traditional:[{Traditional.ElapsedMilliseconds}], New:[{newc.ElapsedMilliseconds}] Success:{newc.ElapsedMilliseconds < Traditional.ElapsedMilliseconds}");

        }
        public void AdditionPerforamce()
        {
            float[] array1 = new float[NumberOfOpperations];
            float[] array2 = new float[NumberOfOpperations];


            Utilities.MathTools.Fraction[] Fractionarray1 = new Utilities.MathTools.Fraction[NumberOfOpperations];
            Utilities.MathTools.Fraction[] Fractionarray2 = new Utilities.MathTools.Fraction[NumberOfOpperations];

            Random random = new Random(); // Instance of Random for generating numbers

            for (int i = 0; i < NumberOfOpperations; i++)
            {
                // Generating random double and converting to decimal
                array1[i] = (float)(2 * random.NextDouble() - 1); // This will generate values between -1 and 1
                array2[i] = (float)(2 * random.NextDouble() - 1); // This will generate values between -1 and 1

                Fractionarray1[i] = new Utilities.MathTools.Fraction((decimal)array1[i]);
                Fractionarray2[i] = new Utilities.MathTools.Fraction((decimal)array2[i]);
            }

            var Traditional = System.Diagnostics.Stopwatch.StartNew();
            float newval = 0;
            for (int i = 0; i < NumberOfOpperations; i++)
            {
                newval = array1[i] + array2[i];
            }
            Traditional.Stop();
            var newc = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < NumberOfOpperations; i++)
            {
                Utilities.MathTools.Fraction mtuli = Fractionarray1[i] + Fractionarray2[i];
            }
            newc.Stop();

            Console.WriteLine($"Traditional:[{Traditional.ElapsedMilliseconds}], New:[{newc.ElapsedMilliseconds}] Success:{newc.ElapsedMilliseconds < Traditional.ElapsedMilliseconds}");

        }
        public void SubtractionPerforamce()
        {
            decimal[] array1 = new decimal[NumberOfOpperations];
            decimal[] array2 = new decimal[NumberOfOpperations];


            Utilities.MathTools.Fraction[] Fractionarray1 = new Utilities.MathTools.Fraction[NumberOfOpperations];
            Utilities.MathTools.Fraction[] Fractionarray2 = new Utilities.MathTools.Fraction[NumberOfOpperations];

            Random random = new Random(); // Instance of Random for generating numbers

            for (int i = 0; i < NumberOfOpperations; i++)
            {
                // Generating random double and converting to decimal
                array1[i] = (decimal)(2 * random.NextDouble() - 1); // This will generate values between -1 and 1
                array2[i] = (decimal)(2 * random.NextDouble() - 1); // This will generate values between -1 and 1

                Fractionarray1[i] = new Utilities.MathTools.Fraction((decimal)array1[i]);
                Fractionarray2[i] = new Utilities.MathTools.Fraction((decimal)array2[i]);
            }

            HashSet<decimal> savevals = new HashSet<decimal>();
            var Traditional = System.Diagnostics.Stopwatch.StartNew();
            decimal newval = 0;
            for (int i = 0; i < NumberOfOpperations; i++)
            {
                newval = array1[i] - array2[i];
            }
            Traditional.Stop();
            var newc = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < NumberOfOpperations; i++)
            {
                Utilities.MathTools.Fraction mtuli = Fractionarray1[i] - Fractionarray2[i];
            }
            newc.Stop();

            Console.WriteLine($"Traditional:[{Traditional.ElapsedMilliseconds}], New:[{newc.ElapsedMilliseconds}] Success:{newc.ElapsedMilliseconds < Traditional.ElapsedMilliseconds}");

        }
        public void DivisionPerforamce()
        {
            float[] array1 = new float[NumberOfOpperations];
            float[] array2 = new float[NumberOfOpperations];


            Utilities.MathTools.Fraction[] Fractionarray1 = new Utilities.MathTools.Fraction[NumberOfOpperations];
            Utilities.MathTools.Fraction[] Fractionarray2 = new Utilities.MathTools.Fraction[NumberOfOpperations];

            Random random = new Random(); // Instance of Random for generating numbers

            for (int i = 0; i < NumberOfOpperations; i++)
            {
                // Generating random double and converting to decimal
                array1[i] = (float)(2 * random.NextDouble() - 1); // This will generate values between -1 and 1
                array2[i] = (float)(2 * random.NextDouble() - 1); // This will generate values between -1 and 1

                Fractionarray1[i] = new Utilities.MathTools.Fraction((decimal)array1[i]);
                Fractionarray2[i] = new Utilities.MathTools.Fraction((decimal)array2[i]);
            }

            var Traditional = System.Diagnostics.Stopwatch.StartNew();
            float newval = 0;
            for (int i = 0; i < NumberOfOpperations; i++)
            {
                newval = array1[i] / array2[i];
            }
            Traditional.Stop();
            var newc = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < NumberOfOpperations; i++)
            {
                Utilities.MathTools.Fraction mtuli = Fractionarray1[i] / Fractionarray2[i];
            }
            newc.Stop();

            Console.WriteLine($"Traditional:[{Traditional.ElapsedMilliseconds}], New:[{newc.ElapsedMilliseconds}] Success:{newc.ElapsedMilliseconds < Traditional.ElapsedMilliseconds}");

        }

    }
}
