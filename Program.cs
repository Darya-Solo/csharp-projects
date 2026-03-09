using System;

namespace CalculatorApp
{
    public static class CalculatorEngine
    {
        public static double Calculate(double num1, double num2, string operation)
        {
            return operation switch
            {
                "+" => num1 + num2,
                "-" => num1 - num2,
                "*" => num1 * num2,
                "/" => num2 != 0 ? num1 / num2 : throw new DivideByZeroException(),
                _ => throw new ArgumentException("Неверная операция")
            };
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Введите 'q' для выхода.");

            while (true)
            {
                Console.Write("Введите первое число: ");
                string input1 = Console.ReadLine() ?? "";
                if (input1.ToLower() == "q") break;

                if (!double.TryParse(input1.Replace('.', ','), out double n1))
                {
                    Console.WriteLine("Это не число!");
                    continue;
                }

                Console.Write("Операция (+, -, *, /): ");
                string op = Console.ReadLine() ?? "";
                if (op.ToLower() == "q") break;

                Console.Write("Введите второе число: ");
                string input2 = Console.ReadLine() ?? "";
                if (input2.ToLower() == "q") break;

                if (!double.TryParse(input2.Replace('.', ','), out double n2))
                {
                    Console.WriteLine("Это не число!");
                    continue;
                }

                try
                {
                    double result = CalculatorEngine.Calculate(n1, n2, op);
                    Console.WriteLine($"Результат: {n1} {op} {n2} = {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}