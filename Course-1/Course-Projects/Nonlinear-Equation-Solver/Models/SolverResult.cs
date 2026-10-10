using System.Collections.Generic;
using System.Numerics;

namespace ComplexSolver.Models;

public class SolverResult
{
    public Complex Root { get; set; }
    public int Iterations { get; set; }
    public bool Converged { get; set; }
    public List<Complex> Path { get; set; } = new();
}