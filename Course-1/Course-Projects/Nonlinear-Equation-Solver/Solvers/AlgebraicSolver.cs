using System;
using System.Collections.Generic;
using System.Numerics;
using ComplexSolver.Models;

namespace ComplexSolver.Solvers;

class AlgebraicSolver : IRootSolver
{
    public SolverResult Solve(ComplexEquation eq, SolverSettings settings)
    {
        string input = settings.FunctionInput
            .ToLower()
            .Replace(" ", "")
            .Replace(",", ".");

        var roots = SolveSimpleAlgebraic(input);

        return new SolverResult
        {
            Root = roots[0],
            Iterations = 1,
            Converged = true,
            Path = roots
        };
    }

    private List<Complex> SolveSimpleAlgebraic(string input)
    {
        input = input.Replace("x", "z");

        if (TryParseSimplePowerEquation(input, out int power, out double value))
        {
            return FindRoots(power, value);
        }

        throw new Exception(
            "Алгебраїчний метод підтримує рівняння виду z^n-a або z^n+a, де n від 2 до 10."
        );
    }

    private bool TryParseSimplePowerEquation(
        string input,
        out int power,
        out double value)
    {
        power = 0;
        value = 0;

        if (!input.StartsWith("z^"))
            return false;

        int signIndex = input.IndexOf('+', 3);

        if (signIndex == -1)
            signIndex = input.IndexOf('-', 3);

        if (signIndex == -1)
            return false;

        string powerPart = input.Substring(2, signIndex - 2);
        string valuePart = input.Substring(signIndex + 1);

        if (!int.TryParse(powerPart, out power))
            return false;

        if (power < 2 || power > 10)
            return false;

        if (!double.TryParse(
                valuePart,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double number))
            return false;

        char sign = input[signIndex];

        // z^n - a = 0  => z^n = a
        // z^n + a = 0  => z^n = -a
        value = sign == '-' ? number : -number;

        return true;
    }

    private List<Complex> FindRoots(int power, double value)
    {
        var roots = new List<Complex>();

        double radius = Math.Pow(Math.Abs(value), 1.0 / power);
        double baseAngle = value >= 0 ? 0 : Math.PI;

        for (int k = 0; k < power; k++)
        {
            double angle = (baseAngle + 2 * Math.PI * k) / power;

            var root = Complex.FromPolarCoordinates(radius, angle);

            root = new Complex(
                Math.Abs(root.Real) < 1e-10 ? 0 : root.Real,
                Math.Abs(root.Imaginary) < 1e-10 ? 0 : root.Imaginary
            );

            roots.Add(root);
        }

        return roots;
    }
}