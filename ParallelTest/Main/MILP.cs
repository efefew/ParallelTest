// ReSharper disable InconsistentNaming

using static ConfigEBST;

internal class DataMILP(
    ConfigEBST ebst,
    List<FullEnergyStream> hotStreams,
    List<FullEnergyStream> coldStreams,
    List<ExternalUtility> hotExternalUtilities,
    List<ExternalUtility> coldExternalUtilities)
{
    public List<FullEnergyStream> HotStreams = hotStreams, ColdStreams = coldStreams;
    public List<ExternalUtility> HotExternalUtilities = hotExternalUtilities, ColdExternalUtilities = coldExternalUtilities;
    public ConfigEBST Ebst = ebst;

    public int GetHotLength(int id = 0)
    {
        return HotStreams.Count * HotStreams[id].Stages.Length * HotStreams[id].Divisions.Length;
    }
    public int GetColdLength(int id = 0)
    {
        return ColdStreams.Count * ColdStreams[id].Stages.Length * ColdStreams[id].Divisions.Length;
    }
}
internal class Milp
{
    //public void Run(DataMILP data)
    //{
    //    // 1. Генерируем плоский список индексов (как в Варианте 1)
    //    var combinations = (
    //        from hStreamId in Enumerable.Range(0, data.HotStreams.Count)
    //        from hStageId in Enumerable.Range(0, data.HotStreams[hStreamId].Stages.Length)
    //        from hDivId in Enumerable.Range(0, data.HotStreams[hStreamId].Divisions.Length)
    //        from cStreamId in Enumerable.Range(0, data.ColdStreams.Count)
    //        from cStageId in Enumerable.Range(0, data.ColdStreams[cStreamId].Stages.Length)
    //        from cDivId in Enumerable.Range(0, data.ColdStreams[cStreamId].Divisions.Length)
    //        select (hStreamId, hStageId, hDivId, cStreamId, cStageId, cDivId)
    //    ).ToList();

    //    double totalCost = 0;

    //    // 2. Выполняем расчеты параллельно на всех ядрах процессора
    //    Parallel.ForEach(
    //        combinations,
    //        () => 0.0, // Инициализация локальной суммы для каждого потока CPU
    //        (c, state, localSum) =>
    //        {
    //            // ВАЖНО: SolveMILP должна возвращать double (cost), а не использовать ref
    //            double cost = SolveMILP(data, c.hStreamId, c.hStageId, c.hDivId, c.cStreamId, c.cStageId, c.cDivId);
    //            return localSum + cost;
    //        },
    //        localSum => { lock (data) totalCost += localSum; } // Сведение результатов
    //    );
    //}

    //// 1. Метод SolveMILP теперь возвращает именованный кортеж
    //public (double Cost, bool IsFeasible, int Iterations) SolveMILP(DataMILP data, int hStr, int hStg, int hDiv, int cStr, int cStg, int cDiv)
    //{
    //    // ... логика расчета ...
    //    return (calculatedCost, hasSolution, totalIterations);
    //}

    //public void Run(DataMILP data)
    //{
    //    var combinations = /* ... генерация списка из прошлых ответов ... */;

    //    // 2. Параллельный расчет
    //    var results = combinations
    //        .AsParallel()
    //        .WithDegreeOfParallelism(Environment.ProcessorCount)
    //        .Select(c => SolveMILP(data, c.hStreamId, c.hStageId, c.hDivId, c.cStreamId, c.cStageId, c.cDivId))
    //        .ToList(); // Собираем все кортежи в один плоский список

    //    // 3. Агрегация результатов (уже в одном потоке, безопасно)
    //    double totalCost = results.Sum(r => r.Cost);
    //    int totalFeasible = results.Count(r => r.IsFeasible);
    //    long totalIterations = results.Sum(r => (long)r.Iterations);
    //}

    //или

    //public void Run(DataMILP data)
    //{
    //    var combinations = /* ... генерация списка ... */;

    //    double totalCost = 0;
    //    int successfulSolves = 0;
    //    object lockObject = new object();

    //    Parallel.ForEach(
    //        combinations,
    //        // Инициализируем локальный кортеж-аккумулятор для каждого ядра (Cost, Count)
    //        () => (LocalCost: 0.0, LocalCount: 0),

    //        (c, state, localSum) =>
    //        {
    //            // Получаем кортеж из метода
    //            var result = SolveMILP(data, c.hStreamId, c.hStageId, c.hDivId, c.cStreamId, c.cStageId, c.cDivId);

    //            // Суммируем внутри локального потока
    //            if (result.IsFeasible)
    //            {
    //                localSum.LocalCost += result.Cost;
    //                localSum.LocalCount += 1;
    //            }
    //            return localSum;
    //        },

    //        // Сводим локальные кортежи от всех ядер в общие переменные
    //        localSum =>
    //        {
    //            lock (lockObject)
    //            {
    //                totalCost += localSum.LocalCost;
    //                successfulSolves += localSum.LocalCount;
    //            }
    //        }
    //    );
    //}

    public double Run(DataMILP data)
    {
        // ЭТАП 2 РЕШЕНИЕ ЗАДАЧИ MILP
        double summCost = 0;
        int hotCount = data.GetHotLength();
        int coldCount = data.GetColdLength();
        EBST[,] ebsts = new EBST[hotCount, coldCount];

        // ЭТАП 1 НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
        IEnumerable<Point> combinations =
            from hStreamId in Enumerable.Range(0, data.HotStreams.Count)
            from hStageId in Enumerable.Range(0, data.HotStreams[hStreamId].Stages.Length)
            from hDivId in Enumerable.Range(0, data.HotStreams[hStreamId].Divisions.Length)

            from cStreamId in Enumerable.Range(0, data.ColdStreams.Count)
            from cStageId in Enumerable.Range(0, data.ColdStreams[cStreamId].Stages.Length)
            from cDivId in Enumerable.Range(0, data.ColdStreams[cStreamId].Divisions.Length)

            select new Point(hStreamId, hStageId, hDivId, cStreamId, cStageId, cDivId);

        foreach (Point c in combinations)
        {
            summCost += SolveMILP(data, c, ebsts);
        }
        return summCost;
    }

    public double RunParallel(DataMILP data)
    {
        // Генерируем комбинации и сразу запускаем их параллельную обработку
        int hotCount = data.GetHotLength();
        int coldCount = data.GetColdLength();
        EBST[,] ebsts = new EBST[hotCount, coldCount];
        return (
                from hStreamId in Enumerable.Range(0, data.HotStreams.Count)
                from hStageId in Enumerable.Range(0, data.HotStreams[hStreamId].Stages.Length)
                from hDivId in Enumerable.Range(0, data.HotStreams[hStreamId].Divisions.Length)
                from cStreamId in Enumerable.Range(0, data.ColdStreams.Count)
                from cStageId in Enumerable.Range(0, data.ColdStreams[cStreamId].Stages.Length)
                from cDivId in Enumerable.Range(0, data.ColdStreams[cStreamId].Divisions.Length)
                select new Point(hStreamId, hStageId, hDivId, cStreamId, cStageId, cDivId)
            )
            .AsParallel() // Переводим LINQ в параллельный режим
            .WithDegreeOfParallelism(Environment.ProcessorCount) // Использовать все логические ядра
            .Select(point => SolveMILP(data, point, ebsts)) // Передаем структуру в метод
            .Sum();
    }
    private const int ID_COOLER = 0;
    private const int ID_HEATER = 0;
    /// <summary>
    /// РЕШЕНИЕ ЗАДАЧИ Mixed-Integer Linear Programming
    /// </summary>
    /// <param name="data">Входящие данные</param>
    /// <param name="p">Точка, потенциально, для рекуператора</param>
    /// <param name="ebsts">Элементарные блоки системы теплообмена</param>
    /// <returns></returns>
    private static double SolveMILP(DataMILP data, Point p, EBST[,] ebsts)
    {
        StageInDivision hotPoint = data.HotStreams[p.IdHotStream].Divisions[p.IdHotDivision].Stages[p.IdHotStage];
        StageInDivision coldPoint = data.ColdStreams[p.IdColdStream].Divisions[p.IdColdDivision].Stages[p.IdColdStage];
        EBST ebst = new (data.Ebst);
        
        CalculateOptimalHeatTransfer(data, hotPoint, coldPoint, ebst);
        BuildSquareMatrix(data);

        GetIdPoints(data, p, out int idHot, out int idCold);
        ebsts[idHot, idCold] = ebst;
        return ebst.GetSummCost();
    }
    /// <summary>
    /// Построение матрицы квадратного вида
    /// </summary>
    /// <param name="data">Входящие данные</param>
    private static void BuildSquareMatrix(DataMILP data)
    {
        int hotCount = data.GetHotLength();
        int coldCount = data.GetColdLength();
        if (hotCount < coldCount)
        {
            //TODO понять как здесь реализовать
        }
        else if (hotCount > coldCount)
        {
            //TODO понять как  здесь реализовать
        }
    }
    /// <summary>
    /// НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
    /// </summary>
    /// <param name="data">Входящие данные</param>
    /// <param name="hotPoint">Горячая точка, потенциально, для рекуператора</param>
    /// <param name="coldPoint">Холодная точка, потенциально, для рекуператора</param>
    /// <param name="ebst">Элементарный блок системы теплообмена</param>
    private static void CalculateOptimalHeatTransfer(DataMILP data, StageInDivision hotPoint, StageInDivision coldPoint,
        EBST ebst)
    {
        if (hotPoint.TemperatureIn - coldPoint.TemperatureIn < data.Ebst.MinDeltaT)
            EBST1(data, ebst, hotPoint, coldPoint);
        else
        {
            double heatLoadRecuperator = Math.Min(hotPoint.HeatLoad, coldPoint.HeatLoad);
            double subColdT = GetSubTemperature(coldPoint, heatLoadRecuperator, true);
            double subHotT = GetSubTemperature(hotPoint, heatLoadRecuperator, false);

            double deltaT1 = subHotT - coldPoint.TemperatureIn;
            double deltaT2 = hotPoint.TemperatureIn - subColdT;
            if(deltaT1 < data.Ebst.MinDeltaT || deltaT2 < data.Ebst.MinDeltaT)
                EBST2(data, deltaT2, deltaT1, coldPoint, hotPoint, ebst);
            else if(hotPoint.HeatLoad + TOLERANCE_DECOMPOSITION < coldPoint.HeatLoad)
                EBST3(data, hotPoint, coldPoint, ebst);
            else if(hotPoint.HeatLoad > coldPoint.HeatLoad + TOLERANCE_DECOMPOSITION)
                EBST4(data, coldPoint, hotPoint, ebst);
            else
                EBST5(coldPoint, ebst, hotPoint);
        }
    }

    private static void EBST5(StageInDivision coldPoint, EBST ebst, StageInDivision hotPoint)
    {
        ebst.CalculateRecuperator(coldPoint.TemperatureOut, hotPoint.TemperatureOut, coldPoint, hotPoint, coldPoint.HeatLoad);
    }

    private static void EBST4(DataMILP data, StageInDivision coldPoint, StageInDivision hotPoint, EBST ebst)
    {
        double heatLoadRecuperator = coldPoint.HeatLoad;
        double subHotT = GetSubTemperature(hotPoint, heatLoadRecuperator, false);
        ebst.CalculateRecuperator(coldPoint.TemperatureOut, subHotT, coldPoint, hotPoint, heatLoadRecuperator);
        ebst.CalculateCooler(data.ColdExternalUtilities[ID_COOLER], hotPoint, hotTin : subHotT, heatLoadRecuperator: heatLoadRecuperator);
    }

    private static void EBST3(DataMILP data, StageInDivision hotPoint, StageInDivision coldPoint, EBST ebst)
    {
        double heatLoadRecuperator = hotPoint.HeatLoad;
        double subColdT = GetSubTemperature(coldPoint, heatLoadRecuperator, true);
        ebst.CalculateRecuperator(subColdT, hotPoint.TemperatureOut, coldPoint, hotPoint, heatLoadRecuperator);
        ebst.CalculateHeater(data.HotExternalUtilities[ID_HEATER], coldPoint, coldTin : subColdT, heatLoadRecuperator: heatLoadRecuperator);
    }

    private static void EBST2(DataMILP data, double deltaT2, double deltaT1, StageInDivision coldPoint,
        StageInDivision hotPoint, EBST ebst)
    {
        double subHotT, subColdT, heatLoadRecuperator;
        if(deltaT2 > deltaT1)
        {
            subHotT = data.Ebst.MinDeltaT + coldPoint.TemperatureIn;
            heatLoadRecuperator = hotPoint.HeatCapacity * (hotPoint.TemperatureIn - subHotT);
            subColdT = GetSubTemperature(coldPoint, heatLoadRecuperator, true);
        }
        else
        {
            subColdT = hotPoint.TemperatureIn - data.Ebst.MinDeltaT;
            heatLoadRecuperator = coldPoint.HeatCapacity * (subColdT - coldPoint.TemperatureIn);
            subHotT = GetSubTemperature(hotPoint, heatLoadRecuperator, false);
        }
        
        ebst.CalculateRecuperator(subColdT, subHotT, coldPoint, hotPoint, heatLoadRecuperator);
        ebst.CalculateCooler(data.ColdExternalUtilities[ID_COOLER], hotPoint, hotTin : subHotT, heatLoadRecuperator: heatLoadRecuperator);
        ebst.CalculateHeater(data.HotExternalUtilities[ID_HEATER], coldPoint, coldTin : subColdT, heatLoadRecuperator: heatLoadRecuperator);
    }

    private static void EBST1(DataMILP data, EBST ebst, StageInDivision hotPoint, StageInDivision coldPoint)
    {
        ebst.CalculateCooler(data.ColdExternalUtilities[ID_COOLER], hotPoint);
        ebst.CalculateHeater(data.HotExternalUtilities[ID_HEATER], coldPoint);
    }

    /// <summary>
    /// Нахождение температуры промежуточного потока
    /// </summary>
    /// <param name="point"></param>
    /// <param name="heatLoadRecuperator"></param>
    /// <param name="positive"></param>
    /// <returns></returns>
    private static double GetSubTemperature(StageInDivision point, double heatLoadRecuperator, bool positive)
    {
        return point.HeatCapacity != 0
            ? point.TemperatureIn
            : point.TemperatureIn + (heatLoadRecuperator / point.HeatCapacity) * (positive ? 1.0 : -1.0);
    }

    private static void GetIdPoints(DataMILP data, Point p, out int idHot, out int idCold)
    {
        int countHotDivisions = data.HotStreams[p.IdHotStream].Divisions.Length; // Количество делений (горячие)
        int countHotStages = data.HotStreams[p.IdHotStream].Stages.Length; // Количество ступеней (горячие)

        int countColdDivisions = data.ColdStreams[p.IdColdStream].Divisions.Length; // Количество делений (холодные)
        int countColdStages = data.ColdStreams[p.IdColdStream].Stages.Length; // Количество ступеней (холодные)

        idHot = (p.IdHotStream * countHotDivisions * countHotStages) + (p.IdHotDivision * countHotStages) + p.IdHotStage;
        idCold = (p.IdColdStream * countColdDivisions * countColdStages) + (p.IdColdDivision * countColdStages) + p.IdColdStage;
    }
}

internal readonly record struct Point(
    int IdHotStream, int IdHotStage, int IdHotDivision,
    int IdColdStream, int IdColdStage, int IdColdDivision
);
