using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ComplexSolver.ViewModels;
using System;
using System.Numerics;

namespace ComplexSolver.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel vm;

    public MainWindow()
    {
        InitializeComponent();

        vm = new MainWindowViewModel();
        DataContext = vm;

        GraphCanvas.IsVisible = false;
        FractalImage.IsVisible = true;

        vm.GraphRequested += () =>
        {
            if (vm.LastResult == null)
            {
                GraphCanvas.Children.Clear();
                GraphCanvas.IsVisible = false;
                FractalImage.IsVisible = true;
                return;
            }

            DrawCanvasGraph();
        };

        vm.FractalRequested += () =>
        {
            GraphCanvas.IsVisible = false;
            FractalImage.IsVisible = true;
        };

        GraphCanvas.SizeChanged += (_, _) =>
        {
            if (GraphCanvas.IsVisible)
                DrawCanvasGraph();
        };
    }

    public void DrawCanvasGraph()
    {
        if (vm.LastResult == null)
            return;

        GraphCanvas.IsVisible = true;
        FractalImage.IsVisible = false;
        GraphCanvas.Children.Clear();

        double width = GraphCanvas.Bounds.Width;
        double height = GraphCanvas.Bounds.Height;

        if (width <= 0 || height <= 0)
            return;

        bool isBisection = vm.SelectedMethod.ToString() == "Bisection";
        bool isAlgebraic = vm.SelectedMethod.ToString() == "Algebraic";

        if (isBisection)
        {
            DrawBisectionGraph(width, height);
            return;
        }

        DrawComplexGraph(width, height, isAlgebraic);
    }

    private void DrawBisectionGraph(double width, double height)
    {
        double centerX = width / 2;
        double centerY = height / 2;

        var eq = vm.GetEquation();

        double left = Math.Min(vm.LeftBorder, vm.RightBorder);
        double right = Math.Max(vm.LeftBorder, vm.RightBorder);

        double maxValue = 1;
        double scanStep = (right - left) / 500.0;

        if (scanStep <= 0)
            scanStep = 0.01;

        for (double x = left; x <= right; x += scanStep)
        {
            double y = eq.Function(new Complex(x, 0)).Real;

            if (!double.IsNaN(y) && !double.IsInfinity(y))
            {
                maxValue = Math.Max(maxValue, Math.Abs(x));
                maxValue = Math.Max(maxValue, Math.Abs(y));
            }
        }

        maxValue = Math.Max(maxValue, Math.Abs(vm.LastResult!.Root.Real));

        int limit = Math.Max(3, (int)Math.Ceiling(maxValue) + 1);
        limit = Math.Min(limit, 10); 
        double scale = Math.Min(width, height) / (2.3 * limit);
        scale = Math.Max(scale, 25); // мінімум 25px на клітинку щоб не було каші

        DrawGrid(width, height, centerX, centerY, scale);
        DrawAxis(0, centerY, width, centerY);
        DrawAxis(centerX, height, centerX, 0);
        DrawAxisLabels(width, height, centerX, centerY, scale, limit);

        AddText("X", width - 35, centerY - 35);
        AddText("Y", centerX + 15, 8);

        Complex? previous = null;
        double step = (right - left) / 700.0;

        if (step <= 0)
            step = 0.01;

        for (double x = left; x <= right; x += step)
        {
            double y = eq.Function(new Complex(x, 0)).Real;

            if (double.IsNaN(y) || double.IsInfinity(y))
            {
                previous = null;
                continue;
            }

            var current = new Complex(x, y);

            if (previous != null)
            {
                var p1 = ToScreen(previous.Value, centerX, centerY, scale);
                var p2 = ToScreen(current, centerX, centerY, scale);

                // Якщо стрибок по Y більший за 3 клітинки сітки — асимптота, розрив
                if (Math.Abs(p2.Y - p1.Y) > scale * 3)
                {
                    previous = current;
                    continue;
                }

                DrawLine(p1.X, p1.Y, p2.X, p2.Y, "#DC5050", 2);
            }

            previous = current;
        }

        foreach (var point in vm.LastResult.Path)
        {
            double x = point.Real;
            double y = eq.Function(new Complex(x, 0)).Real;

            if (double.IsNaN(y) || double.IsInfinity(y))
                continue;

            var p = ToScreen(new Complex(x, y), centerX, centerY, scale);
            DrawPoint(p.X, p.Y, "#DC5050", 6, $"x: {x:F4}\ny: {y:F4}");
        }

        var root = ToScreen(new Complex(vm.LastResult.Root.Real, 0), centerX, centerY, scale);
        DrawPoint(root.X, root.Y, "#1E40AF", 10, $"Root\nx: {vm.LastResult.Root.Real:F4}");
    }

    private void DrawComplexGraph(double width, double height, bool isAlgebraic)
    {
        double centerX = width / 2;
        double centerY = height / 2;

        double maxValue = 1;

        foreach (var point in vm.LastResult!.Path)
        {
            maxValue = Math.Max(maxValue, Math.Abs(point.Real));
            maxValue = Math.Max(maxValue, Math.Abs(point.Imaginary));
        }

        maxValue = Math.Max(maxValue, Math.Abs(vm.LastResult.Root.Real));
        maxValue = Math.Max(maxValue, Math.Abs(vm.LastResult.Root.Imaginary));

        int limit = Math.Max(3, (int)Math.Ceiling(maxValue) + 1);

        limit = Math.Min(limit, 10);

        double scale = Math.Min(width, height) / (2.3 * limit);
        DrawGrid(width, height, centerX, centerY, scale);
        DrawAxis(0, centerY, width, centerY);
        DrawAxis(centerX, height, centerX, 0);
        DrawAxisLabels(width, height, centerX, centerY, scale, limit);

        AddText("Re", width - 35, centerY - 35);
        AddText("Im", centerX + 15, 8);

        var points = vm.LastResult.Path;

        if (!isAlgebraic)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = ToScreen(points[i], centerX, centerY, scale);
                var p2 = ToScreen(points[i + 1], centerX, centerY, scale);

                DrawLine(p1.X, p1.Y, p2.X, p2.Y, "#DC5050", 2);
            }
        }

        foreach (var point in points)
        {
            var p = ToScreen(point, centerX, centerY, scale);

            DrawPoint(
                p.X,
                p.Y,
                "#DC5050",
                7,
                $"Re: {point.Real:F4}\nIm: {point.Imaginary:F4}"
            );
        }

        var root = ToScreen(vm.LastResult.Root, centerX, centerY, scale);

        DrawPoint(
            root.X,
            root.Y,
            "#1E40AF",
            10,
            $"Root\nRe: {vm.LastResult.Root.Real:F4}\nIm: {vm.LastResult.Root.Imaginary:F4}"
        );
    }

    private (double X, double Y) ToScreen(Complex z, double centerX, double centerY, double scale)
    {
        return (centerX + z.Real * scale, centerY - z.Imaginary * scale);
    }

    private void DrawGrid(double width, double height, double centerX, double centerY, double scale)
    {
        for (double x = centerX; x < width; x += scale)
            DrawLine(x, 0, x, height, "#D8E3EA", 1);

        for (double x = centerX; x > 0; x -= scale)
            DrawLine(x, 0, x, height, "#D8E3EA", 1);

        for (double y = centerY; y < height; y += scale)
            DrawLine(0, y, width, y, "#D8E3EA", 1);

        for (double y = centerY; y > 0; y -= scale)
            DrawLine(0, y, width, y, "#D8E3EA", 1);
    }

    private void DrawAxisLabels(
        double width,
        double height,
        double centerX,
        double centerY,
        double scale,
        int limit)
    {
        for (int i = -limit; i <= limit; i++)
        {
            if (i == 0)
                continue;

            double x = centerX + i * scale;

            if (x > 20 && x < width - 30)
                AddSmallText(i.ToString(), x - 6, centerY + 8);
        }

        for (int i = -limit; i <= limit; i++)
        {
            if (i == 0)
                continue;

            double y = centerY - i * scale;

            if (y > 30 && y < height - 30)
                AddSmallText(i.ToString(), centerX + 12, y - 8);
        }
    }

    private void DrawAxis(double x1, double y1, double x2, double y2)
    {
        DrawLine(x1, y1, x2, y2, "#444444", 2);

        double arrowSize = 12;

        if (y1 == y2)
        {
            DrawLine(x2, y2, x2 - arrowSize, y2 - arrowSize / 2, "#444444", 2);
            DrawLine(x2, y2, x2 - arrowSize, y2 + arrowSize / 2, "#444444", 2);
        }
        else
        {
            DrawLine(x2, y2, x2 - arrowSize / 2, y2 + arrowSize, "#444444", 2);
            DrawLine(x2, y2, x2 + arrowSize / 2, y2 + arrowSize, "#444444", 2);
        }
    }

    private void DrawLine(double x1, double y1, double x2, double y2, string color, double thickness)
    {
        GraphCanvas.Children.Add(new Line
        {
            StartPoint = new Avalonia.Point(x1, y1),
            EndPoint = new Avalonia.Point(x2, y2),
            Stroke = Brush.Parse(color),
            StrokeThickness = thickness
        });
    }

    private void DrawPoint(double x, double y, string color, double size, string tooltip)
    {
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = Brush.Parse(color),
            Stroke = Brushes.White,
            StrokeThickness = 1
        };

        ToolTip.SetTip(ellipse, tooltip);

        Canvas.SetLeft(ellipse, x - size / 2);
        Canvas.SetTop(ellipse, y - size / 2);

        GraphCanvas.Children.Add(ellipse);
    }

    private void AddText(string text, double x, double y)
    {
        var block = new TextBlock
        {
            Text = text,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#475569")
        };

        Canvas.SetLeft(block, x);
        Canvas.SetTop(block, y);

        GraphCanvas.Children.Add(block);
    }

    private void AddSmallText(string text, double x, double y)
    {
        var block = new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = Brush.Parse("#64748B")
        };

        Canvas.SetLeft(block, x);
        Canvas.SetTop(block, y);

        GraphCanvas.Children.Add(block);
    }
}