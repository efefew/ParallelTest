namespace HenSQP
{
    using Accord;
    using Accord.Math.Optimization;
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
        /// <summary>
        /// Функция конфигурирования задачи оптимизации
        /// </summary>
        /// <param name="dQhe"></param>
        /// <returns></returns>
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
        /// Критерий оптимальности задачи оптимизации ЭБСТ
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

        /// <summary>
        /// Ограничения задачи оптимизации ЭБСТ
        /// </summary>
        /// <param name="constraints">Ограничения</param>
        /// <param name="nVars"></param>
        private void HEN_constraints_EBST(List<NonlinearConstraint> constraints, int nVars)
        {
            // 1. Ограничение E1: Напор на горячем конце рекуператора (dt1 >= delTij)
            // В MATLAB: delTij - (Thin - tCoutm) <= 0  ==>  Thin - tCoutm - delTij >= 0
            constraints.Add(new NonlinearConstraint(nVars, x =>
            {
                double subColdT = MILP.GetSubTemperature(_data.ColdPoint, x[0], true);
                return _data.HotPoint.TemperatureIn - subColdT - _data.Milp.Ebst.MinDeltaT;
            }));

            // 2. Ограничение E2: Напор на холодном конце рекуператора (dt2 >= delTij)
            // В MATLAB: delTij - (tHoutm - Tcin) <= 0  ==>  tHoutm - Tcin - delTij >= 0
            constraints.Add(new NonlinearConstraint(nVars, x =>
            {
                double subHotT = MILP.GetSubTemperature(_data.HotPoint, x[0], false);
                return subHotT - _data.ColdPoint.TemperatureIn - _data.Milp.Ebst.MinDeltaT;
            }));

            // 3. Ограничение R1: Напор на выходе из нагревателя (dt1 >= delTij)
            constraints.Add(new NonlinearConstraint(nVars, x =>
            {
                double subColdT = MILP.GetSubTemperature(_data.ColdPoint, x[0], true);
                return _data.Milp.HotExternalUtilities[MILP.ID_HEATER].TemperatureOut - subColdT - _data.Milp.Ebst.MinDeltaT;
            }));

            // 4. Ограничение R2: Напор на входе в нагреватель (dt2 >= delTij)
            constraints.Add(new NonlinearConstraint(nVars, x =>
            {
                return _data.Milp.HotExternalUtilities[MILP.ID_HEATER].TemperatureIn - _data.ColdPoint.TemperatureOut - _data.Milp.Ebst.MinDeltaT;
            }));

            // 5. Ограничение R3: Положительность тепловой нагрузки нагревателя (dQreb >= TOLERANCE)
            // В MATLAB: -dQreb + TOLERANCE <= 0  ==>  dQreb - TOLERANCE >= 0
            constraints.Add(new NonlinearConstraint(nVars, x =>
            {
                double dQreb = _data.ColdPoint.HeatLoad - x[0];
                return dQreb - ConfigEBST.TOLERANCE_CONSTR;
            }));

            // 6. Ограничение C1: Напор на входе в холодильник (dt1 >= delTij)
            constraints.Add(new NonlinearConstraint(nVars, x =>
            {
                double subHotT = MILP.GetSubTemperature(_data.HotPoint, x[0], false);
                return subHotT - _data.Milp.ColdExternalUtilities[MILP.ID_COOLER].TemperatureOut - _data.Milp.Ebst.MinDeltaT;
            }));

            // 7. Ограничение C2: Напор на выходе из холодильника (dt2 >= delTij)
            constraints.Add(new NonlinearConstraint(nVars, x =>
            {
                return _data.HotPoint.TemperatureOut - _data.Milp.ColdExternalUtilities[MILP.ID_COOLER].TemperatureIn - _data.Milp.Ebst.MinDeltaT;
            }));

            // 8. Ограничение C3: Положительность тепловой нагрузки холодильника (dQcol >= TOLERANCE)
            constraints.Add(new NonlinearConstraint(nVars, x =>
            {
                double dQcol = _data.HotPoint.HeatLoad - x[0];
                return dQcol - ConfigEBST.TOLERANCE_CONSTR;
            }));
        }

    }
}
