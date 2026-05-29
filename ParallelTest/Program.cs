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
public class EnergyStream
{
    public string Name;
    public float Tin, Tout;
    public float W;

    public EnergyStream()
    {
        Name = string.Empty;
        W = 0;
        Tin = 0;
        Tout = 0;
    }

    public EnergyStream(float tin, float tout)
    {
        Name = string.Empty;
        Tin = tin;
        Tout = tout;
    }

    public EnergyStream(string name, float w, float tin, float tout)
    {
        Name = name;
        W = w;
        Tin = tin;
        Tout = tout;
    }

    public void AddT(float t)
    {
        Tin += t;
        Tout += t;
    }

    public void Copy(EnergyStream stream)
    {
        Name = stream.Name;
        W = stream.W;
        Tin = stream.Tin;
        Tout = stream.Tout;
    }
}
public class EnthalpyInterval
{
    public double EnthalpyIn, EnthalpyOut;
    public double TemperatureColdIn, TemperatureColdOut;
    public double TemperatureHotIn, TemperatureHotOut;

    public Dictionary<int, EnergyStream> HotStreams, ColdStreams;
    public double GetBetaCold(int idStream)
    {
        if (!HotStreams.ContainsKey(idStream))
            return 0;
        double wStreams = HotStreams.Sum(stream => stream.Value.W);
        return HotStreams[idStream].W / wStreams;
    }
    public double GetBetaHot(int idStream)
    {
        if (!ColdStreams.ContainsKey(idStream))
            return 0;
        double wInInterval = ColdStreams.Sum(stream => stream.Value.W);
        return ColdStreams[idStream].W / wInInterval;
    }
    public double GetAlphaHot(int idStream)
    {
        if (!HotStreams.ContainsKey(idStream))
            return 0;
        double deltaTemperatureInStream = HotStreams[idStream].Tin - HotStreams[idStream].Tout;
        double deltaTemperatureInInterval = TemperatureHotIn - TemperatureHotOut;
        return deltaTemperatureInInterval / deltaTemperatureInStream;
    }
    public double GetAlphaCold(int idStream)
    {
        if (!ColdStreams.ContainsKey(idStream))
            return 0;
        double deltaTemperatureInStream = ColdStreams[idStream].Tout - ColdStreams[idStream].Tin;
        double deltaTemperatureInInterval = TemperatureColdOut - TemperatureColdIn;
        return deltaTemperatureInInterval / deltaTemperatureInStream;
    }
}

public struct ConfigEBST
{
    public double MinDeltaT, CGamma, Years, CostExponent;
}
public class Utility
{
    /// <summary>
    /// Тепловая нагрузка
    /// </summary>
    public double HeatLoad;
    /// <summary>
    /// Капитальные затраты
    /// </summary>
    public double CapitalCost;
    /// <summary>
    /// Эксплуатационные затраты
    /// </summary>
    public double OperatingCost;
    /// <summary>
    /// Cуммарные затраты
    /// </summary>
    public double SummCost;
    /// <summary>
    /// Площадь
    /// </summary>
    public double Area;
}
public class EBST
{
    private ConfigEBST _config;
    public Utility Heater { get; private set; }
    public Utility Cooler { get; private set; }
    public Utility Recuperator { get; private set; }
    public EBST(ConfigEBST config)
    {
        _config = config;
    }
    /// <summary>
    /// Среднелогарифмический температурный напор
    /// </summary>
    /// <param name="interval">Интервал</param>
    /// <returns></returns>
    private static double GetLMTD(EnthalpyInterval interval)
    {
        double dT1 = interval.TemperatureHotOut - interval.TemperatureColdIn;
        double dT2 = interval.TemperatureHotIn - interval.TemperatureColdOut;
        return Math.Abs(dT1 - dT2) < TOLERANCE ? dT1 : (dT1 - dT2) / Math.Log(dT1 / dT2);
    }

    private const double TOLERANCE = 1e-5;

    public Utility CalculateRecuperator(EnthalpyInterval interval, double costRecuperator, double cost, double heatLoad, double heatTransferCoeff, double heatTransferExternalCoeff)
    {
        if (Recuperator == null) Recuperator = new Utility();
        Recuperator.HeatLoad = heatLoad;
        double deltaT = GetLMTD(interval);
        double overallHeatTransferCoeff = 1 / (1 / heatTransferCoeff + 1 / heatTransferExternalCoeff);

        Recuperator.Area = heatLoad / (deltaT * overallHeatTransferCoeff);

        /*if ~isempty(isMax)
        isMax = false;
        end
            maxLength = max((Nh_streams-Nh_sec_u)*Nq_i*Nl_i,(Nc_streams-Nc_sec_u)*Nq_j*Nl_j);
        if (~isMax && i > (Nh_streams-Nh_sec_u)*Nq_i*Nl_i || j > (Nc_streams-Nc_sec_u)*Nq_j*Nl_j) || (isMax && i > maxLength || j > maxLength)
        if (~isMax && i > (Nh_streams-Nh_sec_u)*Nq_i*Nl_i && j > (Nc_streams-Nc_sec_u)*Nq_j*Nl_j) || (isMax && i > maxLength && j > maxLength)
        Cu_ = realmax;
        else
        Cu_ = Cu_sec_u;
        end
        else
        Cu_ = 0;
        end*/

        Recuperator.OperatingCost = Recuperator.HeatLoad * cost;
        Recuperator.CapitalCost = (costRecuperator * Math.Pow(Recuperator.Area, _config.CGamma)) / _config.Years;
        Recuperator.SummCost = Recuperator.OperatingCost + Recuperator.CapitalCost;
        Recuperator.SummCost=(Recuperator.HeatLoad != 0) ? (Recuperator.SummCost / Math.Pow(Recuperator.HeatLoad, _config.CostExponent)) : 0.0;
        return Recuperator;

    }
    /// <summary>
    /// Рассчитать нагреватель
    /// </summary>
    /// <param name="interval">Интервал температур</param>
    /// <param name="costRecuperator">Стоимость рекуператора</param>
    /// <param name="costHeater">Стоимость нагревателя</param>
    /// <param name="heatLoad">Тепловая нагрузка</param>
    /// <param name="heatTransferCoeff">Коэффициент теплопередачи горячей утилиты</param>
    /// <param name="heatTransferExternalCoeff">Коэффициент теплопередачи внешней горячей утилиты</param>
    /// <returns>Cуммарные затраты</returns>
    public Utility CalculateHeater(EnthalpyInterval interval, double costRecuperator, double costHeater, double heatLoad, double heatTransferCoeff, double heatTransferExternalCoeff)
    {
        if (Heater == null) Heater = new Utility();
        Heater.HeatLoad = heatLoad;
        return CalculateUtility(interval, costRecuperator, costHeater, Heater, heatTransferCoeff, heatTransferExternalCoeff);
    }
    /// <summary>
    /// Рассчитать холодильник
    /// </summary>
    /// <param name="interval">Интервал температур</param>
    /// <param name="costRecuperator">Стоимость рекуператора</param>
    /// <param name="costCooler">Стоимость холодильника</param>
    /// <param name="heatLoad">Тепловая нагрузка</param>
    /// <param name="heatTransferCoeff">Коэффициент теплопередачи холодной утилиты</param>
    /// <param name="heatTransferExternalCoeff">Коэффициент теплопередачи внешней холодной утилиты</param>
    /// <returns>Cуммарные затраты</returns>
    public Utility CalculateCooler(EnthalpyInterval interval, double costRecuperator, double costCooler, double heatLoad, double heatTransferCoeff, double heatTransferExternalCoeff)
    {
        if (Cooler == null) Cooler = new Utility();
        Cooler.HeatLoad = heatLoad;
        return CalculateUtility(interval, costRecuperator, costCooler, Cooler, heatTransferCoeff, heatTransferExternalCoeff);
    }   
    /// <summary>
    /// Рассчитать утилиту
    /// </summary>
    /// <param name="interval">Интервал температур</param>
    /// <param name="costRecuperator">Стоимость рекуператора</param>
    /// <param name="cost">Стоимость утилиты</param>
    /// <param name="heatLoad">Тепловая нагрузка</param>
    /// <param name="heatTransferCoeff">Коэффициент теплопередачи утилиты</param>
    /// <param name="heatTransferExternalCoeff">Коэффициент теплопередачи внешней утилиты</param>
    /// <returns>Cуммарные затраты</returns>
    private Utility CalculateUtility(EnthalpyInterval interval, double costRecuperator, double cost, Utility utility, double heatTransferCoeff, double heatTransferExternalCoeff)
    {
        double deltaT = GetLMTD(interval);
        //TODO общий коэффициент теплопередачи (без учёта сопротивления стенки R=δ/λ)
        double overallHeatTransferCoeff = 1 / (1 / heatTransferCoeff + 1 / heatTransferExternalCoeff);
        
        utility.Area = utility.HeatLoad / (deltaT * overallHeatTransferCoeff);

        utility.OperatingCost = utility.HeatLoad * cost;
        utility.CapitalCost = (costRecuperator * Math.Pow(utility.Area, _config.CGamma)) / _config.Years;
        utility.SummCost = utility.OperatingCost + utility.CapitalCost;
        utility.SummCost = (utility.HeatLoad != 0) ? (utility.SummCost / Math.Pow(utility.HeatLoad, _config.CostExponent)) : 0.0;
        return utility;
    }

}