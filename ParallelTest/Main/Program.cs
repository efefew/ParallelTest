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
        ParallelFunc(SolvingMILP);
        Console.ReadLine();
    }
    private static void ParallelFunc(Func<double, double, double> func)
    {
        const double MIN = 0;
        const double MAX = 1_000_000;
        const ulong COUNT_TASKS = 1024;
        const double SUB_RANGE = MAX / COUNT_TASKS;
        
        Task[] tasks = new Task[COUNT_TASKS];
        double[] array = new double[COUNT_TASKS];

        for (ulong idTask = 0; idTask < COUNT_TASKS; idTask++)
        {
            tasks[idTask] = new Task(FindSubPrimeCount(MIN, SUB_RANGE, array, idTask, func));
        }

        for (ulong idTask = 0; idTask < COUNT_TASKS; idTask++)
            tasks[idTask].Start();

        for (ulong idTask = 0; idTask < COUNT_TASKS; idTask++)
            tasks[idTask].Wait();

        double sum = 0;
        for (ulong idTask = 0; idTask < COUNT_TASKS; idTask++)
            sum += array[idTask];

        Console.WriteLine($"TestParallelFunc {sum}");

    }
   
    private static Action FindSubPrimeCount(double min, double subRange, double[] output, ulong idTask, Func<double, double, double> func)
    {
        return () => output[idTask] = func(min + subRange * idTask, min + (subRange * (idTask + 1) - 1));
    }
    //РЕШЕНИЕ ЗАДАЧИ MILP
    public static double SolvingMILP(double a, double b)
    {
        return 0;
    }
}