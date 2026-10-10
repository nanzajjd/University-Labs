using System.Numerics;

namespace ComplexSolver.Models;

class SolverSettings
{
    public double Epsilon { get; set; }

    public int MaxIterations { get; set; }

    public Complex InitialGuess { get; set; }
    public string FunctionInput { get; set; } = "";

    public Complex A { get; set; }

    public Complex B { get; set; }
}