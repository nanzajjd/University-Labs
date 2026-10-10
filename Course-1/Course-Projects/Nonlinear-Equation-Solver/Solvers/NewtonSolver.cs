using ComplexSolver.Models;
using System.Numerics;

namespace ComplexSolver.Solvers;

class NewtonSolver : IRootSolver
{
    public SolverResult Solve(ComplexEquation eq, SolverSettings settings)
    {
        var z = settings.InitialGuess;
        var result = new SolverResult();

        result.Path.Add(z);

        for (int i = 0; i < settings.MaxIterations; i++)
        {
            var f = eq.Function(z);
            var df = eq.Derivative!(z);

            if (Complex.Abs(df) < 1e-12)
                break;

            var zNext = z - f / df;

            result.Path.Add(zNext);

            if (Complex.Abs(zNext - z) < settings.Epsilon)
            {
                result.Root = zNext;
                result.Iterations = i + 1;
                result.Converged = true;
                return result;
            }

            z = zNext;
        }

        result.Root = z;
        result.Iterations = result.Path.Count - 1;
        result.Converged = false;

        return result;
    }
}