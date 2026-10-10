using ComplexSolver.Models;

namespace ComplexSolver.Services;

class ComplexityAnalyzer
{
    public static string Analyze(SolverResult result)
    {
        if (result.Iterations <= 1)
            return "Complexity: O(1)";

        if (result.Iterations < 15)
            return "Complexity: O(log n)";

        return "Complexity: O(n)";
    }
}