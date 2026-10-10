using ComplexSolver.Models;
using System;
using System.Numerics;

namespace ComplexSolver.Solvers;

class BisectionSolver : IRootSolver
{
    public SolverResult Solve(ComplexEquation eq, SolverSettings settings)
    {
        double a = settings.A.Real;
        double b = settings.B.Real;

        double fa = eq.Function(new Complex(a, 0)).Real;
        double fb = eq.Function(new Complex(b, 0)).Real;

        if (fa * fb > 0)
        {
            throw new Exception(
                "На заданому відрізку немає зміни знаку функції. Метод бісекції неможливо застосувати.");
        }

        var result = new SolverResult();

        for (int i = 1; i <= settings.MaxIterations; i++)
        {
            double mid = (a + b) / 2.0;
            var z = new Complex(mid, 0);

            double fm = eq.Function(z).Real;

            result.Path.Add(z);

            if (Math.Abs(fm) < settings.Epsilon ||
                Math.Abs(b - a) < settings.Epsilon)
            {
                result.Root = z;
                result.Iterations = i;
                result.Converged = true;
                return result;
            }

            if (fa * fm < 0)
            {
                b = mid;
                fb = fm;
            }
            else
            {
                a = mid;
                fa = fm;
            }
        }

        result.Root = new Complex((a + b) / 2.0, 0);
        result.Iterations = settings.MaxIterations;
        result.Converged = false;

        return result;
    }
}