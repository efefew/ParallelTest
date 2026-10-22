namespace HenSQP
{
    using Accord;
    using Accord.Math.Optimization;

    public enum ExitflagType
    {
        Success = 1,
        Error = -1,
        OverLimit = 0
    }
    public class HenOptimization(HenOptimizationData data)
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

        private HenOptimizationData _data = data;

        public OptimResult HEN_optim_EBST(double dQhe)
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
            HEN_constraints_EBST(constraints, numberOfVariables);
    
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
    
        /// <summary>
        /// Критерий ЭБСТ
        /// </summary>
        /// <param name="x">Нагрузка рекуператора</param>
        /// <returns>Сумма затрат ЭБСТ</returns>
        private double HEN_criteriy_EBST(double[] x)
        {
            double heatLoadRecuperator = x[0];
            double subHotT = MILP.GetSubTemperature(_data.HotPoint, heatLoadRecuperator, false);
            double subColdT = MILP.GetSubTemperature(_data.ColdPoint, heatLoadRecuperator, true);

            _data.Ebst.CalculateCooler(_data.Milp.ColdExternalUtilities[MILP.ID_COOLER], _data.HotPoint, heatLoadRecuperator);
            _data.Ebst.CalculateHeater(_data.Milp.HotExternalUtilities[MILP.ID_HEATER], _data.ColdPoint, heatLoadRecuperator);
            _data.Ebst.CalculateRecuperator(subColdT, subHotT, _data.HotPoint, _data.ColdPoint, heatLoadRecuperator);

            return _data.Ebst.GetSummCost();
        }
    
        // Метод для добавления нелинейных ограничений
        private void HEN_constraints_EBST(List<NonlinearConstraint> constraints, int nVars)
        {

            constraints.Add(new NonlinearConstraint(nVars, x => x[0] * x[1] - 10 >= 0));
        }
    }

    public class HenOptimizationData(EBST ebst, DataMILP milp, StageInDivision hotPoint, StageInDivision coldPoint)
    {
        public StageInDivision HotPoint = hotPoint;
        public StageInDivision ColdPoint = coldPoint;
        public EBST Ebst = ebst;
        public DataMILP Milp = milp;
    }
}
