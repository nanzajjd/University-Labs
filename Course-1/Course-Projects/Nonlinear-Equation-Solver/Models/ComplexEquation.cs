using System;
using System.Numerics;

namespace ComplexSolver.Models;

public class ComplexEquation
{
    public Func<Complex, Complex> Function { get; set; }
    public Func<Complex, Complex>? Derivative { get; set; }
}