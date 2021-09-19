using System;
using ISB.Runtime;
using ISB.Utilities;

namespace ISB.Lib
{
    public class Math
    {
        private readonly Random _random = new Random();

        public Math()
        {
            this.Pi = new NumberValue(3.1415926535897931m);
            this.E = new NumberValue(2.7182818284590451m);
        }

        [Doc("The constant π.")]
        public NumberValue Pi { get; private set; }

        [Doc("The natural logarithmic base.")]
        public NumberValue E { get; private set; }

        [Doc("Returns the absolute value of a specified number.")]
        public NumberValue Abs(NumberValue x) {
            double ret = System.Math.Abs((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the angle whose cosine is the specified number.")]
        public NumberValue Acos(NumberValue x) {
            double ret = System.Math.Acos((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the angle whose sine is the specified number.")]
        public NumberValue Asin(NumberValue x) {
            double ret = System.Math.Asin((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the angle whose tangent is the specified number.")]
        public NumberValue Atan(NumberValue x) {
            double ret = System.Math.Atan((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the smallest integral value greater than or equal to the specified number.")]
        public NumberValue Ceiling(NumberValue x) {
            double ret = System.Math.Ceiling((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the cosine of the specified angle.")]
        public NumberValue Cos(NumberValue x) {
            double ret = System.Math.Cos((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns e raised to the specified power.")]
        public NumberValue Exp(NumberValue x) {
            double ret = System.Math.Exp((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the largest integral value less than or equal to the specified number.")]
        public NumberValue Floor(NumberValue x) {
            double ret = System.Math.Floor((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the logarithm of a specified number.")]
        public NumberValue Log(NumberValue x) {
            double ret = System.Math.Log((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the base 10 logarithm of a specified number.")]
        public NumberValue Log10(NumberValue x) {
            double ret = System.Math.Log10((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the larger of two specified numbers.")]
        public NumberValue Max(NumberValue x, NumberValue y) {
            return x.ToNumber() > y.ToNumber() ? x : y;
        }

        [Doc("Returns the smaller of two numbers.")]
        public NumberValue Min(NumberValue x, NumberValue y) {
            return x.ToNumber() < y.ToNumber() ? x : y;
        }

        [Doc("Returns a specified number raised to the specified power.")]
        public NumberValue Pow(NumberValue x, NumberValue y) {
            double ret = System.Math.Pow((double)x.ToNumber(), (double)y.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.")]
        public NumberValue Random()
        {
            return new NumberValue((decimal)_random.NextDouble());
        }

        [Doc("Returns a non-negative random integer that is less than the specified maximum.")]
        public NumberValue RandomInt(NumberValue max)
        {
            int maxValue = (int)System.Math.Floor((double)max.ToNumber());
            if (maxValue < 0) maxValue = 0;
            return new NumberValue(_random.Next(maxValue));
        }

        [Doc("Rounds a value to the nearest integer or to the specified number of fractional digits.")]
        public NumberValue Round(NumberValue x) {
            double ret = System.Math.Round((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns an integer that indicates the sign of a number.")]
        public NumberValue Sign(NumberValue x) {
            double ret = System.Math.Sign((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the sine of the specified angle.")]
        public NumberValue Sin(NumberValue x) {
            double ret = System.Math.Sin((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the square root of a specified number.")]
        public NumberValue Sqrt(NumberValue x) {
            double ret = System.Math.Sqrt((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        [Doc("Returns the tangent of the specified angle.")]
        public NumberValue Tan(NumberValue x) {
            double ret = System.Math.Tan((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }
    }
}
