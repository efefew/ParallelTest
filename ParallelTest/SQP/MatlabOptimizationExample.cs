using MatlabSQP;
internal class MatlabOptimizationExample
{
    public void Run()
    {
        // 1. Определяем целевую функцию
        Func<double[], double> objective = (x) => 3 * Math.Sin(x[0]) + Math.Exp(x[1]);

        // 2. Начальная точка
        double[] x0 = { 1.0, 1.0 };

        // 3. Нижние границы (LB =)
        double[] lb = { 0.0, 0.0 };

        // 4. Настройки
        var options = new OptimizationOptions
        {
            Algorithm = "interior-point",
            Display = "final"
        };

        // Вызов функции
        int exitFlag = MatlabSolver.Fmincon(
            fun: objective,
            x0: x0,
            a: null, b: null,      // Нет линейных неравенств
            aeq: null, beq: null,  // Нет линейных равенств
            lb: lb,
            ub: null,              // Нет верхних границ
            nonlcon: null,         // Нет нелинейных ограничений
            options: options,
            out double[] x,
            out double fval,
            out OptimizationOutput output,
            out LagrangeMultipliers lambda,
            out double[] grad,
            out double[,] hessian
        );

        Console.WriteLine($"Exit Flag: {exitFlag}");
        Console.WriteLine($"Оптимальное X: [{string.Join(", ", x)}]");
        Console.WriteLine($"Минимум функции: {fval}");
    }
}