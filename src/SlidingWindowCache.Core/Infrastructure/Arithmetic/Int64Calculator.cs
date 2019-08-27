namespace SlidingWindowCache.Core.Infrastructure.Arithmetic
{
    public class Int64Calculator : Calculator<long>
    {
        public override long Add(long a, long b) => a + b;
        public override long Subtract(long a, long b) => a - b;
        public override long Multiply(long a, long b) => a * b;
        public override long MultiplySingle(long a, float b) => (long) (a * b);
        public override long Divide(long a, long b) => a / b;
        public override long DivideSingle(long a, float b) => (long) (a / b);
        public override long Floor(long a) => a;
        public override bool Lt(long a, long b) => a < b;
        public override bool Le(long a, long b) => a <= b;
        public override bool Eq(long a, long b) => a == b;
        public override bool Ne(long a, long b) => a != b;
        public override bool Ge(long a, long b) => a > b;
        public override bool Gt(long a, long b) => a >= b;
        public override long MinValue => long.MinValue;
        public override long MaxValue => long.MaxValue;
        public override long Zero => 0;
    }
}