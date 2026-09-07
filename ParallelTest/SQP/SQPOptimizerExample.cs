using OwnSQP;
internal class SQPOptimizerExample
{
    public void Run()
    {
        // 1. Целевая функция f(x)
        SQPOptimizer.ObjectiveFunc f = (x) => (x[0] * x[0]) + (x[1] * x[1]);

        // 2. Ограничения g(x) <= 0
        SQPOptimizer.ConstraintsFunc g = (x) =>
        [
            1.0 - x[0] - x[1] // Эквивалентно x0 + x1 >= 1
        ];

        // 3. Начальная точка
        double[] x0 = [0.1, 0.1];

        // 4. Запуск оптимизатора
        double[] result = SQPOptimizer.Minimize(f, g, x0);

        Console.WriteLine($"Оптимальное решение: x0 = {result[0]:F4}, x1 = {result[1]:F4}");
    }
}