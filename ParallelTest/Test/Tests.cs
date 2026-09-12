using System.Diagnostics;
using System.Reflection;

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
        Message.Normal($"Метод {method.GetMethodInfo().Name} выполнялся: {elapsed.ToString()} времени и {count} раз");
        return elapsed;
    }

    public static bool TestCorrect<TIn, TOut>(Func<TIn, TOut> method, TIn value, TOut correctAnswer)
    {
        TOut result = method(value);
        if (result.Equals(correctAnswer))
        {
            Message.Done($"{method.GetMethodInfo().Name}({value}) = {result} is correct");
        }
        else
        {
            Message.Error($"{method.GetMethodInfo().Name}({value}) = {result} is wrong, correct {correctAnswer}");
        }
        return result.Equals(correctAnswer);
    }

    public static bool TestCorrect<T1, T2, TOut>(Func<T1, T2, TOut> method, T1 value1, T2 value2, TOut correctAnswer)
    {
        TOut result = method(value1, value2);
        if (!result.Equals(correctAnswer))
            Message.Error($"{method.GetMethodInfo().Name}({value1}, {value2}) = {result} is wrong, correct {correctAnswer}");
        return result.Equals(correctAnswer);
    }

    public static void CompareTestTimes(Action method1, Action method2, int count = 1)
    {
        TimeSpan span1 = TestTime(method1, count);
        TimeSpan span2 = TestTime(method2, count);
        Message.Done(
            span1.TotalMilliseconds < span2.TotalMilliseconds
                ? $"Метод {method1.GetMethodInfo().Name} выполнялся в {span2.TotalMilliseconds / span1.TotalMilliseconds} раз быстрее"
                : $"Метод {method2.GetMethodInfo().Name} выполнялся в {span1.TotalMilliseconds / span2.TotalMilliseconds} раз быстрее");
    }
}