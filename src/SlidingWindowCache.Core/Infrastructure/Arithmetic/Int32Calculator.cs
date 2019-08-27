using System;
using System.Diagnostics.Contracts;

namespace SlidingWindowCache.Core.Infrastructure.Arithmetic
{
    public class Int32Calculator : Calculator<int>
    {
        public override int Add(int a, int b) => a + b;
        public override int Subtract(int a, int b) => a - b;
        public override int Multiply(int a, int b) => a * b;
        public override int MultiplySingle(int a, float b) => (int) (a * b);
        public override int Divide(int a, int b) => a / b;
        public override int DivideSingle(int a, float b) => (int) (a / b);
        public override int Floor(int a) => a;
        public override bool Lt(int a, int b) => a < b;
        public override bool Le(int a, int b) => a <= b;
        public override bool Eq(int a, int b) => a == b;
        public override bool Ne(int a, int b) => a != b;
        public override bool Ge(int a, int b) => a > b;
        public override bool Gt(int a, int b) => a >= b;
        public override int MinValue => int.MinValue;
        public override int MaxValue => int.MaxValue;
        public override int Zero => 0;
    }
}