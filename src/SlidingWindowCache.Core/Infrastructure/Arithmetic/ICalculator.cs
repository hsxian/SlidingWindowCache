using System;

namespace SlidingWindowCache.Core.Infrastructure.Arithmetic
{
    public interface ICalculator<T>
    {
       
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, T b);
        T MultiplySingle(T a, float b);
        T Divide(T a, T b);
        T DivideSingle(T a, float b);
        T Floor(T a);
        bool Lt(T a, T b);
        bool Le(T a, T b);
        bool Eq(T a, T b);
        bool Ne(T a, T b);
        bool Ge(T a, T b);
        bool Gt(T a, T b);
    }
}