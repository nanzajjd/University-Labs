using ComplexSolver.Models;
using System.Numerics;
using System;

namespace ComplexSolver.Utils;

class FunctionFactory
{
    public static ComplexEquation CreateEquation()
    {
        Console.WriteLine("Enter function f(z): ");
        Console.WriteLine("Example: z^3 - 1");

        var input = Console.ReadLine();

        var func = ExpressionParser.Parse(input);

        Func<Complex, Complex> derivative = z =>
        {
            double h = 1e-6;
            return (func(z + h) - func(z - h)) / (2 * h);
        };

        return new ComplexEquation
        {
            Function = func,
            Derivative = derivative
        };
    }
}