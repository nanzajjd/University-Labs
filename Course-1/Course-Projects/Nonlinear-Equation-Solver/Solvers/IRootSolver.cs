using ComplexSolver.Models;

namespace ComplexSolver.Solvers;

interface IRootSolver
{
    SolverResult Solve(ComplexEquation eq, SolverSettings settings);
}