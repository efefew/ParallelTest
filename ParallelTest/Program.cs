using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System.Diagnostics;
using System.Reflection;

internal static class Program
{
    public static void Main()
    {
        TestParallelFunc();
        Console.ReadLine();
    }
    private static void TestParallelFunc()
    {
        ulong min = 0, max = 1_000_000;
        ulong countTasks = 1024;
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
   
    private static Action FindSubPrimeCount(ulong min, ulong subRange, ulong[] output, ulong idTask)
    {
        return () =>
        {
            output[idTask] = FindPrimeCount(min + (subRange * idTask), min + ((subRange * (idTask + 1)) - 1));
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

    //РЕШЕНИЕ ЗАДАЧИ MILP
    public static void SolvingMILP()
    {

    }
}