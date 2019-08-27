namespace SlidingWindowCache.Core.Infrastructure.Arithmetic
{
    public abstract class Calculator<T> : ICalculator<T>
    {
        public abstract T Add(T a, T b);
        public abstract T Subtract(T a, T b);
        public abstract T Multiply(T a, T b);
        public abstract T MultiplySingle(T a, float b);
        public abstract T Divide(T a, T b);
        public abstract T DivideSingle(T a, float b);
        public abstract T Floor(T a);
        public abstract bool Lt(T a, T b);
        public abstract bool Le(T a, T b);
        public abstract bool Eq(T a, T b);
        public abstract bool Ne(T a, T b);
        public abstract bool Ge(T a, T b);
        public abstract bool Gt(T a, T b);
        public abstract T MinValue { get; }
        public abstract T MaxValue { get;}
        public abstract T Zero { get; }
    }
}