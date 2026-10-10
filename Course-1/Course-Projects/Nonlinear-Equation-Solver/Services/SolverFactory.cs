using System;
using ComplexSolver.Solvers;

namespace ComplexSolver.Services;

public enum SolverType
{
    Newton,
    Bisection,
    Algebraic
}

class SolverFactory

{

    public static IRootSolver Create(SolverType type)

    {

        return type switch

        {

            SolverType.Newton => new NewtonSolver(),

            SolverType.Bisection => new BisectionSolver(),

            SolverType.Algebraic => new AlgebraicSolver(),

            _ => throw new ArgumentException("Unknown solver type")

        };

    }
}