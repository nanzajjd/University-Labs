using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Linq;

namespace ComplexSolver.Utils;

class ExpressionParser
{
    public static Func<Complex, Complex> Parse(string expr)
    {
        if (string.IsNullOrWhiteSpace(expr))
            throw new Exception("Function is empty");

        var tokens = Tokenize(expr.ToLower().Replace(',', '.'));
        var rpn = ToRpn(tokens);

        return z => EvaluateRpn(rpn, z);
    }

    static bool IsFunction(string token)
    {
        return token == "sin" ||
               token == "cos" ||
               token == "tan" ||
               token == "sqrt" ||
               token == "exp" ||
               token == "log";
    }

    static bool IsVariable(string token)
    {
        return token == "z" || token == "x";
    }

    static bool IsOperator(string token)
    {
        return token is "+" or "-" or "*" or "/" or "^" or "u-";
    }

    static List<string> Tokenize(string expr)
    {
        var tokens = new List<string>();
        int i = 0;

        while (i < expr.Length)
        {
            if (char.IsWhiteSpace(expr[i]))
            {
                i++;
                continue;
            }

            if ("+-*/^()".Contains(expr[i]))
            {
                tokens.Add(expr[i].ToString());
                i++;
                continue;
            }

            if (char.IsDigit(expr[i]) || expr[i] == '.')
            {
                string number = "";

                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                {
                    number += expr[i];
                    i++;
                }

                if (number.Count(c => c == '.') > 1)
                    throw new Exception("Incorrect number format");

                tokens.Add(number);
                continue;
            }

            if (expr[i] == 'z' || expr[i] == 'x')
            {
                tokens.Add(expr[i].ToString());
                i++;
                continue;
            }

            if (char.IsLetter(expr[i]))
            {
                string name = "";

                while (i < expr.Length && char.IsLetter(expr[i]))
                {
                    name += expr[i];
                    i++;
                }

                if (!IsFunction(name))
                    throw new Exception($"Unknown function or variable: {name}");

                tokens.Add(name);
                continue;
            }

            throw new Exception($"Invalid character: {expr[i]}");
        }

        return tokens;
    }

    static int Priority(string op) => op switch
    {
        "u-" => 4,
        "^" => 3,
        "*" or "/" => 2,
        "+" or "-" => 1,
        _ => 0
    };

    static bool IsRightAssociative(string op)
    {
        return op == "^" || op == "u-";
    }

    static List<string> ToRpn(List<string> tokens)
    {
        var output = new List<string>();
        var stack = new Stack<string>();

        string? previous = null;

        foreach (var rawToken in tokens)
        {
            string token = rawToken;

            if (token == "-" &&
                (previous == null || IsOperator(previous) || previous == "("))
            {
                token = "u-";
            }

            if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _) ||
                IsVariable(token))
            {
                output.Add(token);
            }
            else if (IsFunction(token))
            {
                stack.Push(token);
            }
            else if (IsOperator(token))
            {
                while (stack.Count > 0 &&
                       stack.Peek() != "(" &&
                       (Priority(stack.Peek()) > Priority(token) ||
                       (Priority(stack.Peek()) == Priority(token) && !IsRightAssociative(token))))
                {
                    output.Add(stack.Pop());
                }

                stack.Push(token);
            }
            else if (token == "(")
            {
                stack.Push(token);
            }
            else if (token == ")")
            {
                while (stack.Count > 0 && stack.Peek() != "(")
                    output.Add(stack.Pop());

                if (stack.Count == 0)
                    throw new Exception("Incorrect brackets");

                stack.Pop();

                if (stack.Count > 0 && IsFunction(stack.Peek()))
                    output.Add(stack.Pop());
            }
            else
            {
                throw new Exception($"Unknown token: {token}");
            }

            previous = token;
        }

        while (stack.Count > 0)
        {
            if (stack.Peek() == "(")
                throw new Exception("Incorrect brackets");

            output.Add(stack.Pop());
        }

        return output;
    }

    static Complex EvaluateRpn(List<string> rpn, Complex z)
    {
        var stack = new Stack<Complex>();

        foreach (var token in rpn)
        {
            if (IsVariable(token))
            {
                stack.Push(z);
            }
            else if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
            {
                stack.Push(new Complex(val, 0));
            }
            else if (token == "u-")
            {
                if (stack.Count < 1)
                    throw new Exception("Incorrect expression");

                var a = stack.Pop();
                stack.Push(-a);
            }
            else if (IsFunction(token))
            {
                if (stack.Count < 1)
                    throw new Exception("Incorrect function argument");

                var a = stack.Pop();

                stack.Push(token switch
                {
                    "sin" => Complex.Sin(a),
                    "cos" => Complex.Cos(a),
                    "tan" => Complex.Tan(a),
                    "sqrt" => Complex.Sqrt(a),
                    "exp" => Complex.Exp(a),
                    "log" => Complex.Log(a),
                    _ => throw new Exception("Unknown function")
                });
            }
            else if (IsOperator(token))
            {
                if (stack.Count < 2)
                    throw new Exception("Incorrect expression");

                var b = stack.Pop();
                var a = stack.Pop();

                if (token == "/" && Complex.Abs(b) < 1e-12)
                    throw new Exception("Division by zero");

                stack.Push(token switch
                {
                    "+" => a + b,
                    "-" => a - b,
                    "*" => a * b,
                    "/" => a / b,
                    "^" => Complex.Pow(a, b),
                    _ => throw new Exception("Unknown operation")
                });
            }
            else
            {
                throw new Exception($"Unknown token: {token}");
            }
        }

        if (stack.Count != 1)
            throw new Exception("Incorrect expression");

        return stack.Pop();
    }
}