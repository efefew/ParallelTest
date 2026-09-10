public class DataMILP
{
    public List<FullEnergyStream> HotStreams = new(), ColdStreams = new();
    public List<ExternalUtility> HotExternalUtilities = new(), ColdExternalUtilities = new();
    public ConfigEBST Ebst;

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

    public void Run(DataMILP data)
    {
        // ЭТАП 2 РЕШЕНИЕ ЗАДАЧИ MILP
        double summCost = 0;

        // ЭТАП 1 НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
        var combinations =
            from hStreamId in Enumerable.Range(0, data.HotStreams.Count)
            from hStageId in Enumerable.Range(0, data.HotStreams[hStreamId].Stages.Length)
            from hDivId in Enumerable.Range(0, data.HotStreams[hStreamId].Divisions.Length)

            from cStreamId in Enumerable.Range(0, data.ColdStreams.Count)
            from cStageId in Enumerable.Range(0, data.ColdStreams[cStreamId].Stages.Length)
            from cDivId in Enumerable.Range(0, data.ColdStreams[cStreamId].Divisions.Length)

            select new Point(hStreamId, hStageId, hDivId, cStreamId, cStageId, cDivId);

        foreach (var c in combinations)
        {
            summCost += SolveMILP(data, c);
        }
    }

    public void RunParallel(DataMILP data)
    {
        double totalCost = GetTotalCost(data);
    }

    private double GetTotalCost(DataMILP data)
    {
        // Генерируем комбинации и сразу запускаем их параллельную обработку
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
        .Select(point => SolveMILP(data, point)) // Передаем структуру в метод
        .Sum(); // Потокобезопасное сложение результатов
    }

    private double SolveMILP(DataMILP data, Point p)
    {
        StageInDivision hotPoint = data.HotStreams[p.IdHotStream].Divisions[p.IdHotDivision].Stages[p.IdHotStage];
        StageInDivision coldPoint = data.ColdStreams[p.IdColdStream].Divisions[p.IdColdDivision].Stages[p.IdColdStage];

        int hotCount = data.GetHotLength();
        int coldCount = data.GetColdLength();
        EBST[,] ebsts = new EBST[hotCount, coldCount];

        GetIdPoints(data, p, out int idHot, out int idCold);

        const int ID_COOLER = 0;
        const int ID_HEATER = 0;

        if (hotPoint.TemperatureIn - coldPoint.TemperatureIn < data.Ebst.MinDeltaT)
        {
            ebsts[idHot, idCold] = new(data.Ebst);
            ebsts[idHot, idCold].CalculateCooler(data.ColdExternalUtilities[ID_COOLER], hotPoint);
            ebsts[idHot, idCold].CalculateHeater(data.HotExternalUtilities[ID_HEATER], coldPoint);
        }
        else
        {
            double heatLoadRecuperator = Math.Min(hotPoint.HeatLoad, coldPoint.HeatLoad); // выбор минимального количества теплоты, затраченного на охлаждение/нагревание
            double intermediateColdTemperature = // нахождение температуры промежуточного холодного потока
                coldPoint.HeatCapacity != 0 ? 
                coldPoint.TemperatureIn : 
                coldPoint.TemperatureIn + (heatLoadRecuperator / coldPoint.HeatCapacity);
            double intermediateHotTemperature =  // нахождение температуры промежуточного горячего потока
                hotPoint.HeatCapacity != 0 ?
                hotPoint.TemperatureIn :
                hotPoint.TemperatureIn - (heatLoadRecuperator / hotPoint.HeatCapacity);

            double deltaT1 = intermediateHotTemperature - coldPoint.TemperatureIn; // разность промежуточного горячего и входного холодного
            double deltaT2 = hotPoint.TemperatureIn - intermediateColdTemperature; // разность промежуточного горячего и входного холодного
            if(deltaT1 < data.Ebst.MinDeltaT || deltaT2 < data.Ebst.MinDeltaT)
            {
                // ЭБСТ2 Полноструктурный блок
                if(deltaT2 > deltaT1) // ЭБСТ2 Случай 1
                {
                    intermediateHotTemperature = data.Ebst.MinDeltaT + coldPoint.TemperatureIn; // нахождение температуры промежуточного горячего потока
                }
            }
        }
        return 0;
    }
    private static void GetIdPoints(DataMILP data, Point p, out int idHot, out int idCold)
    {
        int CountHotDivisions = data.HotStreams[p.IdHotStream].Divisions.Length; // Количество делений (горячие)
        int CountHotStages = data.HotStreams[p.IdHotStream].Stages.Length; // Количество ступеней (горячие)

        int CountColdDivisions = data.ColdStreams[p.IdColdStream].Divisions.Length; // Количество делений (холодные)
        int CountColdStages = data.ColdStreams[p.IdColdStream].Stages.Length; // Количество ступеней (холодные)

        idHot = (p.IdHotStream * CountHotDivisions * CountHotStages) + (p.IdHotDivision * CountHotStages) + p.IdHotStage;
        idCold = (p.IdColdStream * CountColdDivisions * CountColdStages) + (p.IdColdDivision * CountColdStages) + p.IdColdStage;
    }
}
public readonly record struct Point(
    int IdHotStream, int IdHotStage, int IdHotDivision,
    int IdColdStream, int IdColdStage, int IdColdDivision
);
