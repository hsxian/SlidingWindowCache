namespace SlidingWindowCache.Core.Infrastructure.Arithmetic
{
    public class SingleCalculator : Calculator<float>
    {
        public override float Add(float a, float b) => a + b;
        public override float Subtract(float a, float b) => a - b;
        public override float Multiply(float a, float b) => a * b;
        public override float MultiplySingle(float a, float b) => a * b;
        public override float Divide(float a, float b) => a / b;
        public override float DivideSingle(float a, float b) => a / b;
        public override float Floor(float a) => a;
        public override bool Lt(float a, float b) => a < b;
        public override bool Le(float a, float b) => a <= b;
        public override bool Eq(float a, float b) => a == b;
        public override bool Ne(float a, float b) => a != b;
        public override bool Ge(float a, float b) => a > b;
        public override bool Gt(float a, float b) => a >= b;
    }
}