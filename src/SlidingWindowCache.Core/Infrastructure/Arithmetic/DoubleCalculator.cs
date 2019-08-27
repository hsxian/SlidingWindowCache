using System;

namespace SlidingWindowCache.Core.Infrastructure.Arithmetic
{
    public class DoubleCalculator : Calculator<double>
    {
        public override double Add(double a, double b) => a + b;
        public override double Subtract(double a, double b) => a - b;
        public override double Multiply(double a, double b) => a * b;
        public override double MultiplySingle(double a, float b) => a * b;
        public override double Divide(double a, double b) => a / b;
        public override double DivideSingle(double a, float b) => a / b;
        public override double Floor(double a) => Math.Floor(a);
        public override bool Lt(double a, double b) => a < b;
        public override bool Le(double a, double b) => a <= b;
        public override bool Eq(double a, double b) => a == b;
        public override bool Ne(double a, double b) => a != b;
        public override bool Ge(double a, double b) => a > b;
        public override bool Gt(double a, double b) => a >= b;
        public override double MinValue => double.MinValue;
        public override double MaxValue => double.MaxValue;
        public override double Zero => 0;
    }
}