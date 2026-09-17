// ReSharper disable InconsistentNaming

using Accord.Math;
using static ConfigEBST;

internal static class Milp
{
    public static double RunParallel2(DataMILP data)
    {
        int countTasks = 100;
        Task[] tasks = new Task[countTasks];
        int hotCount = data.GetHotLength();
        int coldCount = data.GetColdLength();
        int totalIterations = hotCount * coldCount;

        int sizePerTask = totalIterations / countTasks;
        int remainder = totalIterations % countTasks;

        EBST[,] ebsts = new EBST[hotCount, coldCount];
        
        for (int idTask = 0; idTask < countTasks; idTask++)
        {
            tasks[idTask] = new Task(Chunk(data, ebsts, idTask, remainder, sizePerTask));
            tasks[idTask].Start();
        }


        Task.WaitAll(tasks);
        double summCost = 0;
        /*for (int x = 0; x < ebsts.GetLength(0); x++)
            for (int y = 0; y < ebsts.GetLength(1); y++)
                summCost += ebsts[x, y].GetSummCost();*/
        return summCost;
    }
private static Action Chunk(DataMILP data, EBST[,] ebsts, int taskId, int remainder, int sizePerTask)
{
    int maxHotStage = data.HotStreams[0].Stages.Length;
    int maxHotDivision = data.HotStreams[0].Divisions.Length;
    int maxColdStream = data.ColdStreams.Length;
    int maxColdStage = data.ColdStreams[0].Stages.Length;
    int maxColdDivision = data.ColdStreams[0].Divisions.Length;

    int startIdx = taskId * sizePerTask + Math.Min(taskId, remainder);
    int endIdx = startIdx + sizePerTask + (taskId < remainder ? 1 : 0);

    return () =>
    {
        // 5. Итерируемся по выделенному линейному диапазону
        for (long i = startIdx; i < endIdx; i++)
        {
            // Восстанавливаем 6D-координаты из линейного индекса `i`
            long rem = i;

            int idColdDivision = (int)(rem % maxColdDivision); 
            rem /= maxColdDivision;
            int idColdStage    = (int)(rem % maxColdStage);    
            rem /= maxColdStage;
            int idColdStream   = (int)(rem % maxColdStream);   
            rem /= maxColdStream;
            int idHotDivision  = (int)(rem % maxHotDivision);  
            rem /= maxHotDivision;
            int idHotStage     = (int)(rem % maxHotStage);     
            rem /= maxHotStage;
            int idHotStream    = (int)rem;

            // 6. Выполняем полезную работу
            Point p = new(idHotStream, idHotStage, idHotDivision, idColdStream, idColdStage, idColdDivision);
            GetIdPoints(data, p, out int idHot, out int idCold);
            
            EBST result = CalculateOptimalHeatTransfer(data, p);
            ebsts[idHot, idCold] = result;
        }
    };
}
    private static Action Chunk(DataMILP data, EBST[,] ebsts, int countTasks)
    {
        return () =>
        {
            Point min = new();//инициализируй относительно countTasks
            Point max = new();//инициализируй относительно countTasks
            
            for (int idHotStream = min.HotStream; idHotStream < max.HotStream; idHotStream++)
            for (int idHotStage = min.HotStage; idHotStage < max.HotStage; idHotStage++)
            for (int idHotDivision = min.HotDivision; idHotDivision < max.HotDivision; idHotDivision++)
            for (int idColdStream = min.ColdStream; idColdStream < max.ColdStream; idColdStream++)
            for (int idColdStage = min.ColdStage; idColdStage < max.ColdStage; idColdStage++)
            for (int idColdDivision = min.ColdDivision; idColdDivision < max.ColdDivision; idColdDivision++)
            {
                Point p = new(idHotStream, idHotStage, idHotDivision, idColdStream, idColdStage, idColdDivision);
                GetIdPoints(data, p, out int idHot, out int idCold);
                ebsts[idHot, idCold] = CalculateOptimalHeatTransfer(data, p);
            }
        };
    }
    public static double RunParallel1(DataMILP data)
    {
        int hotCount = data.HotStreams.Length;
        int coldCount = data.ColdStreams.Length;
        int totalPairs = hotCount * coldCount;
    
        object lockObj = new object();
        double totalCost = 0;
        //Environment.ProcessorCount
    
        // Параллельный цикл по парам потоков (HotStream x ColdStream)
        Parallel.For(0, totalPairs,
            () => 0.0, // Инициализация локальной суммы для каждого потока CPU
            (pairId, _, localSum) =>
            {
                // Декодируем плоский индекс обратно в ID потоков
                int hStreamId = pairId / coldCount;
                int cStreamId = pairId % coldCount;
    
                FullEnergyStream hotStream = data.HotStreams[hStreamId];
                FullEnergyStream coldStream = data.ColdStreams[cStreamId];
    
                int hStagesLen = hotStream.Stages.Length;
                int hDivsLen = hotStream.Divisions.Length;
                int cStagesLen = coldStream.Stages.Length;
                int cDivsLen = coldStream.Divisions.Length;
    
                // Вложенные циклы по стадиям и делениям выполняются последовательно внутри потока
                for (int hStageId = 0; hStageId < hStagesLen; hStageId++)
                {
                    for (int hDivId = 0; hDivId < hDivsLen; hDivId++)
                    {
                        for (int cStageId = 0; cStageId < cStagesLen; cStageId++)
                        {
                            for (int cDivId = 0; cDivId < cDivsLen; cDivId++)
                            {
                                Point point = new(hStreamId, hStageId, hDivId, cStreamId, cStageId, cDivId);
                                
                                // Вычисляем и сразу суммируем результат, не сохраняя его в список
                                EBST result = CalculateOptimalHeatTransfer(data, point);
                                localSum += result.GetSummCost();
                            }
                        }
                    }
                }
    
                return localSum; // Возвращаем накопленную сумму потока
            },
            localSum =>
            {
                // Безопасно суммируем результаты потоков в общую переменную (вызывается редко)
                lock (lockObj)
                {
                    totalCost += localSum;
                }
            });
    
        return totalCost;
    }
    /// <summary>
    /// РЕШЕНИЕ ЗАДАЧИ Mixed-Integer Linear Programming
    /// </summary>
    /// <param name="data">Входящие данные</param>
    /// <returns></returns>
    public static double Run(DataMILP data)
    {
        // ЭТАП 2 РЕШЕНИЕ ЗАДАЧИ MILP
        double summCost = 0;
        int hotCount = data.GetHotLength();
        int coldCount = data.GetColdLength();
        EBST[,] ebsts = new EBST[hotCount, coldCount];
        for (int idHotStream = 0; idHotStream < data.HotStreams.Length; idHotStream++)
        for (int idHotStage = 0; idHotStage < data.HotStreams[idHotStream].Stages.Length; idHotStage++)
        for (int idHotDivision = 0; idHotDivision < data.HotStreams[idHotStream].Divisions.Length; idHotDivision++)
        for (int idColdStream = 0; idColdStream < data.ColdStreams.Length; idColdStream++)
        for (int idColdStage = 0; idColdStage < data.ColdStreams[idColdStream].Stages.Length; idColdStage++)
        for (int idColdDivision = 0; idColdDivision < data.ColdStreams[idColdStream].Divisions.Length; idColdDivision++)
        {
            Point p = new (idHotStream, idHotStage, idHotDivision, idColdStream, idColdStage, idColdDivision);
            GetIdPoints(data, p, out int idHot, out int idCold);
            ebsts[idHot, idCold] = CalculateOptimalHeatTransfer(data, p);
            double cost = ebsts[idHot, idCold].GetSummCost();
            if (double.IsNaN(cost) || double.IsInfinity(cost))
                Message.Error($"Error: cost of {p} is {cost}");
            summCost += cost;
        }
        return summCost;
    }
    
    private const int ID_COOLER = 0;
    private const int ID_HEATER = 0;
    /// <summary>
    /// НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
    /// </summary>
    /// <param name="data">Входящие данные</param>
    /// <param name="p">Точка, потенциально, для рекуператора</param>
    /// <returns></returns>
    private static EBST CalculateOptimalHeatTransfer(DataMILP data, Point p)
    {
        StageInDivision hotPoint = data.HotStreams[p.HotStream].Divisions[p.HotDivision].Stages[p.HotStage];
        StageInDivision coldPoint = data.ColdStreams[p.ColdStream].Divisions[p.ColdDivision].Stages[p.ColdStage];
        EBST ebst = new (data.Ebst);
        
        CalculateOptimalHeatTransfer(data, hotPoint, coldPoint, ebst);
        return ebst;
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
            ? point.TemperatureIn + (heatLoadRecuperator / point.HeatCapacity) * (positive ? 1.0 : -1.0)
            : point.TemperatureIn;
    }

    private static void GetIdPoints(DataMILP data, Point p, out int idHot, out int idCold)
    {
        int countHotDivisions = data.HotStreams[p.HotStream].Divisions.Length; // Количество делений (горячие)
        int countHotStages = data.HotStreams[p.HotStream].Stages.Length; // Количество ступеней (горячие)

        int countColdDivisions = data.ColdStreams[p.ColdStream].Divisions.Length; // Количество делений (холодные)
        int countColdStages = data.ColdStreams[p.ColdStream].Stages.Length; // Количество ступеней (холодные)

        idHot = (p.HotStream * countHotDivisions * countHotStages) + (p.HotDivision * countHotStages) + p.HotStage;
        idCold = (p.ColdStream * countColdDivisions * countColdStages) + (p.ColdDivision * countColdStages) + p.ColdStage;
    }
}