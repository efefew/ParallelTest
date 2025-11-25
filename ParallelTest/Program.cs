using System.Diagnostics;
using System.Reflection;

public static class Program
{
    public static void Main()
    {
        Tests.CompareTestTimes(TestParallelFunc, TestFunc);
        Console.ReadLine();
    }
    private static void TestFunc()
    {
        ulong min = 1, max = 1_00_000;
        Console.WriteLine($"TestFunc {FindPrimeCount(min, max)}");
    }
    private static void TestParallelFunc()
    {
        ulong min = 1, max = 1_00_000;
        ulong countTasks = 10;
        ulong subRange = max / countTasks;
        Task[] tasks = new Task[countTasks];
        ulong[] array = new ulong[countTasks];

        for (ulong idTask = 0; idTask < countTasks; idTask++)
        {
            tasks[idTask] = new Task(FindSubPrimeCount(min, subRange, array, idTask));
        }

        for (ulong idTask = 0; idTask < countTasks; idTask++)
            tasks[idTask].Start();

        for (ulong idTask = 0; idTask < countTasks; idTask++)
            tasks[idTask].Wait();

        ulong sum = 0;
        for (ulong idTask = 0; idTask < countTasks; idTask++)
            sum += array[idTask];

        Console.WriteLine($"TestParallelFunc {sum}");

    }

    private static Action FindSubPrimeCount(ulong min, ulong subRange, ulong[] array, ulong idTask)
    {
        return () =>
        {
            array[idTask] = FindPrimeCount(min + (subRange * idTask), min + ((subRange * (idTask + 1)) - 1));
        };
    }

    static bool IsPrime(ulong n)
    {
        if (n <= 1)
            return false; // Числа меньше или равные 1 не простые
        for (ulong id = 2; id < n; id++)
        {
            if (n % id == 0)
            {
                return false; // Найден делитель, число не простое
            }
        }

        return true; // Делителей не найдено, число простое
    }

    private static ulong FindPrimeCount(ulong a, ulong b)
    {
        if (a >= b)
            return 0;

        ulong count = 0;
        for (ulong number = a; number <= b; number++)
        {
            if (IsPrime(number))
                count++;
        }

        return count;
    }
}
public static class Tests
{
    public static TimeSpan TestTime(Action method, int count = 1)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        for (int id = 0; id < count; id++)
            method?.Invoke();
        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        Console.WriteLine($"Метод {method!.GetMethodInfo().Name} выполнялся: {elapsed} времени и {count} раз");
        return elapsed;
    }

    public static void CompareTestTimes(Action method1, Action method2, int count = 1)
    {
        TimeSpan span1 = TestTime(method1, count);
        TimeSpan span2 = TestTime(method2, count);
        Console.WriteLine(
            span1.TotalMilliseconds < span2.TotalMilliseconds
                ? $"Метод {method1.GetMethodInfo().Name} выполнялся в {span2.TotalMilliseconds / span1.TotalMilliseconds} раз быстрее"
                : $"Метод {method2.GetMethodInfo().Name} выполнялся в {span1.TotalMilliseconds / span2.TotalMilliseconds} раз быстрее");
    }
}