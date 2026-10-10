using System.IO;
using ComplexSolver.Models;

namespace ComplexSolver.Services;

class FileService
{
    public static void Save(string path, SolverResult result)
    {
        File.WriteAllText(path,
            $"Root: {result.Root}\nIterations: {result.Iterations}");
    }
}