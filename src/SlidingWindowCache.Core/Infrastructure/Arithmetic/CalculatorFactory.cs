using System;

namespace SlidingWindowCache.Core.Infrastructure.Arithmetic
{
    public class CalculatorFactory
    {
        public static ICalculator<T>  Build<T>()
        {
            if(typeof(T)==typeof(int))return (ICalculator<T>) new Int32Calculator();
            if(typeof(T)==typeof(long))return (ICalculator<T>) new Int64Calculator();
            if(typeof(T)==typeof(double))return (ICalculator<T>) new DoubleCalculator();
            throw new NotSupportedException();
        }

    }
}