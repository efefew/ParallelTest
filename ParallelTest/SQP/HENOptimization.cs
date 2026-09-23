namespace HenSQP
{
    using Accord.Math.Optimization;

    public enum ExitflagType
    {
        Success = 1,
        Error = -1,
        OverLimit = 0
    }
    public class HenOptimization
    {
        // Класс для возврата результатов (аналог выходных аргументов MATLAB)
        public class OptimResult
        {
            public double[]? XEbst { get; set; }
            public double SumEbst { get; set; }
            /// <summary>
            /// Статус успеха
            /// <br></br>
            /// Имитация exitflag из MATLAB (1 = сошелся, 0 = превышен лимит, -1 = ошибка)
            /// </summary>
            public ExitflagType Exitflag { get; set; }
        }
    
        public static OptimResult HEN_optim_EBST(double dQhe)
        {
            // 1. Начальная точка (x0)
            double[] x0 = [dQhe];
    
            // 2. Определение целевой функции через структуру Accord.NET
            int numberOfVariables = x0.Length;
            NonlinearObjectiveFunction objectiveFunction = new (numberOfVariables, HEN_criteriy_EBST);
    
            // 3. Задание ограничений-границ (LB и UB)
            List<NonlinearConstraint> constraints = new();
    
            for (int i = 0; i < numberOfVariables; i++)
            {
                int index = i; // Локальная переменная для замыкания
                
                // Нижняя граница (LB = 1e-10)
                constraints.Add(new NonlinearConstraint(numberOfVariables, x => x[index] >= 1e-10));
                
                // Верхняя граница (UB = 1000000000)
                constraints.Add(new NonlinearConstraint(numberOfVariables, x => x[index] <= 1000000000));
            }
    
            // 4. Добавление нелинейных ограничений
            AddCustomConstraints(constraints, numberOfVariables);
    
            // 5. Настройка и инициализация решателя COBYLA (вместо отсутствующего SQP)
            Cobyla cobyla = new(objectiveFunction, constraints.ToArray())
            {
                MaxIterations = 10000 // Лимит итераций
            };
    
            // 6. Запуск оптимизации
            bool success = cobyla.Minimize(x0);
            
            // 7. Формирование результатов
            return new OptimResult
            {
                XEbst = cobyla.Solution,
                SumEbst = cobyla.Value,
                // Имитация exitflag из MATLAB (1 = сошелся, 0 = превышен лимит, -1 = ошибка)
                Exitflag = success ? ExitflagType.Success : (cobyla.Iterations >= cobyla.MaxIterations ? ExitflagType.OverLimit : ExitflagType.Error)
            };
        }
    
        // Заглушка для целевой функции
        private static double HEN_criteriy_EBST(double[] x)
        {
            return x[0] * x[0];
        }
    
        // Метод для добавления нелинейных ограничений
        private static void AddCustomConstraints(List<NonlinearConstraint> constraints, int nVars)
        {
            // Ограничение-неравенство (<= 0)
            constraints.Add(new NonlinearConstraint(nVars, x => x[0] + x[1] - 5 <= 0));
            
            // Чтобы сымитировать строгое равенство (== 0), преобразуем его в два противоположных неравенства:
            constraints.Add(new NonlinearConstraint(nVars, x => x[0] * x[1] - 10 <= 0));
            constraints.Add(new NonlinearConstraint(nVars, x => x[0] * x[1] - 10 >= 0));
        }
    }
}
