using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ComplexSolver.Models;
using ComplexSolver.Services;
using ComplexSolver.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace ComplexSolver.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string FunctionInput { get; set; } = "z^3-1";
    public double Epsilon { get; set; } = 0.001;
    public double LeftBorder { get; set; } = -2;
    public double RightBorder { get; set; } = 2;

    public string GraphTitle { get; set; } = "Coordinate Plane";

    public List<SolverType> Methods { get; } =
        new() { SolverType.Newton, SolverType.Bisection, SolverType.Algebraic };

    private SolverType _selectedMethod = SolverType.Newton;

    public SolverType SelectedMethod
    {
        get => _selectedMethod;
        set
        {
            _selectedMethod = value;

            RaisePropertyChanged(nameof(SelectedMethod));
            RaisePropertyChanged(nameof(ShowFractalButton));
            RaisePropertyChanged(nameof(ShowBorders));
            RaisePropertyChanged(nameof(FunctionLabel));
        }
    }

    public string FunctionLabel =>
        SelectedMethod == SolverType.Bisection
            ? "Function f(x):"
            : "Function f(z):";

    public bool ShowFractalButton =>
        SelectedMethod == SolverType.Newton;

    public bool ShowBorders =>
        SelectedMethod == SolverType.Bisection;

    public string ResultText { get; set; } = "";

    public Bitmap? FractalBitmap { get; set; }

    private SolverResult? _lastResult;

    public SolverResult? LastResult => _lastResult;

    public event Action? GraphRequested;
    public event Action? FractalRequested;

    public ICommand SolveCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ComplexityCommand { get; }
    public ICommand FractalCommand { get; }
    public ICommand GraphCommand { get; }

    public ICommand ClearCommand { get; }

    public MainWindowViewModel()
    {
        SolveCommand = new RelayCommand(Solve);
        SaveCommand = new RelayCommand(SaveToFile);
        ComplexityCommand = new RelayCommand(ShowComplexity);
        FractalCommand = new RelayCommand(DrawFractal);
        GraphCommand = new RelayCommand(DrawGraph);
        ClearCommand = new RelayCommand(ClearData);
    }

    void Solve()
    {
        try
        {
            if (Epsilon <= 0)
                throw new Exception("Epsilon must be greater than zero");

            if (Epsilon >= 1)
                throw new Exception("Epsilon must be less than 1");

            if (Epsilon < 1e-12)
                throw new Exception("Epsilon is too small");

            if (SelectedMethod == SolverType.Bisection)

            {

                if (LeftBorder >= RightBorder)

                    throw new Exception("Left border must be less than right border");

            }
            var solver = SolverFactory.Create(SelectedMethod);

            _lastResult = solver.Solve(GetEquation(), new SolverSettings
            {
                InitialGuess = new Complex(1, 1),
                MaxIterations = 100,
                Epsilon = Epsilon,
                A = new Complex(LeftBorder, 0),
                B = new Complex(RightBorder, 0),
                FunctionInput = FunctionInput
            });

            if (SelectedMethod == SolverType.Algebraic)
            {
                ResultText = "Алгебраїчний метод\n\n";

                for (int i = 0; i < _lastResult.Path.Count; i++)
                    ResultText += $"x{i + 1} = {_lastResult.Path[i]}\n";

                ResultText += $"\nIterations: {_lastResult.Iterations}";
            }
            else
            {
                ResultText =
                    $"Root: {_lastResult.Root}\n" +
                    $"Iterations: {_lastResult.Iterations}";
            }

            ResultText += _lastResult.Converged
                ? "\n✓ Solution found"
                : "\n✗ Solution not found";

            RaisePropertyChanged(nameof(ResultText));
            RaisePropertyChanged(nameof(LastResult));
        }
        catch (Exception ex)
        {
            ResultText = $"Помилка: {ex.Message}";
            RaisePropertyChanged(nameof(ResultText));
        }
    }

    async void SaveToFile()
    {
        if (_lastResult == null)
        {
            ResultText = "Помилка: спочатку виконайте розрахунок.";
            RaisePropertyChanged(nameof(ResultText));
            return;
        }

        try
        {
            if (App.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow == null)
            {
                throw new Exception("Cannot open save dialog.");
            }

            var file = await desktop.MainWindow.StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title = "Save result",
                    SuggestedFileName = "result.txt",
                    DefaultExtension = "txt",
                    FileTypeChoices = new[]
                    {
                    new FilePickerFileType("Text file")
                    {
                        Patterns = new[] { "*.txt" }
                    }
                    }
                });

            if (file == null)
                return;

            await using var stream = await file.OpenWriteAsync();
            using var writer = new StreamWriter(stream);

            await writer.WriteAsync(ResultText);

            ResultText += "\n\n✓ Result saved successfully";
            RaisePropertyChanged(nameof(ResultText));
        }
        catch (Exception ex)
        {
            ResultText += $"\n✗ Помилка збереження: {ex.Message}";
            RaisePropertyChanged(nameof(ResultText));
        }
    }

    void ShowComplexity()
    {
        if (_lastResult == null)
        {
            ResultText = "Помилка: спочатку виконайте розрахунок.";
            RaisePropertyChanged(nameof(ResultText));
            return;
        }

        ResultText += "\n" + ComplexityAnalyzer.Analyze(_lastResult);
        RaisePropertyChanged(nameof(ResultText));
    }

    void DrawGraph()
    {
        if (_lastResult == null)
        {
            ResultText = "Помилка: спочатку виконайте розрахунок.";
            RaisePropertyChanged(nameof(ResultText));
            return;
        }

        GraphTitle = "Coordinate Plane";
        RaisePropertyChanged(nameof(GraphTitle));

        GraphRequested?.Invoke();
    }

    public ComplexEquation GetEquation()
    {
        if (string.IsNullOrWhiteSpace(FunctionInput))
            throw new Exception("Function field is empty");

        var f = ExpressionParser.Parse(FunctionInput);

        return new ComplexEquation
        {
            Function = f,
            Derivative = z =>
            {
                double h = 1e-6;

                try
                {
                    return (f(z + h) - f(z - h)) / (2 * h);
                }
                catch
                {
                    throw new Exception("Cannot calculate derivative");
                }
            }
        };
    }

    async void DrawFractal()
    {
        if (SelectedMethod != SolverType.Newton)
            return;

        try
        {
            var eq = GetEquation();

            int width = 600;
            int height = 400;
            double epsilonLocal = Epsilon;

            var wb = await System.Threading.Tasks.Task.Run(() =>
            {
                var bitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Avalonia.Vector(96, 96),
                    PixelFormat.Bgra8888,
                    AlphaFormat.Premul);

                using (var fb = bitmap.Lock())
                {
                    unsafe
                    {
                        byte* ptr = (byte*)fb.Address;
                        int stride = fb.RowBytes;

                        double xmin = -2, xmax = 2;
                        double ymin = -1, ymax = 1;

                        for (int y = 0; y < height; y++)
                        {
                            double im = ymax - y * (ymax - ymin) / height;

                            for (int x = 0; x < width; x++)
                            {
                                double re = xmin + x * (xmax - xmin) / width;
                                var z = new Complex(re, im);

                                int iter = 0;

                                for (; iter < 30; iter++)
                                {
                                    var f = eq.Function(z);
                                    var df = eq.Derivative!(z);

                                    if (Complex.Abs(df) < 1e-10)
                                        break;

                                    var z2 = z - f / df;

                                    if (Complex.Abs(z2 - z) < epsilonLocal)
                                        break;

                                    z = z2;
                                }

                                byte* pixel = ptr + y * stride + x * 4;

                                pixel[0] = (byte)(255 - iter * 8);
                                pixel[1] = (byte)(iter * 6);
                                pixel[2] = 150;
                                pixel[3] = 255;
                            }
                        }
                    }
                }

                return bitmap;
            });

            FractalBitmap = wb;
            RaisePropertyChanged(nameof(FractalBitmap));

            GraphTitle = "Newton Fractal";
            RaisePropertyChanged(nameof(GraphTitle));

            FractalRequested?.Invoke();
        }
        catch (Exception ex)
        {
            ResultText = $"Помилка фракталу: {ex.Message}";
            RaisePropertyChanged(nameof(ResultText));
        }
    }
    void ClearData()
    {
        FunctionInput = "";
        Epsilon = 0.001;
        LeftBorder = -2;
        RightBorder = 2;

        ResultText = "";
        FractalBitmap = null;
        _lastResult = null;

        GraphTitle = "Coordinate Plane";

        RaisePropertyChanged(nameof(FunctionInput));
        RaisePropertyChanged(nameof(Epsilon));
        RaisePropertyChanged(nameof(LeftBorder));
        RaisePropertyChanged(nameof(RightBorder));

        RaisePropertyChanged(nameof(ResultText));
        RaisePropertyChanged(nameof(FractalBitmap));
        RaisePropertyChanged(nameof(LastResult));
        RaisePropertyChanged(nameof(GraphTitle));

        GraphRequested?.Invoke();
    }
}