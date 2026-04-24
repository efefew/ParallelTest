using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System.Diagnostics;
using System.Reflection;

public static class Program
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
}
public class MaterialStream
{
    public string Name;
    public float Tin, Tout;
    public float W;

    public MaterialStream()
    {
        Name = string.Empty;
        W = 0;
        Tin = 0;
        Tout = 0;
    }

    public MaterialStream(float tin, float tout)
    {
        Name = string.Empty;
        Tin = tin;
        Tout = tout;
    }

    public MaterialStream(string name, float w, float tin, float tout)
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

    public void Copy(MaterialStream stream)
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

    public Dictionary<int, MaterialStream> HotStreams, ColdStreams;
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
    public double MinDeltaT, CGamma, Years;
}
public class EBST
{
    private static double GetLMTD(EnthalpyInterval interval)
    {
        double dT1 = interval.TemperatureHotOut - interval.TemperatureColdIn;
        double dT2 = interval.TemperatureHotIn - interval.TemperatureColdOut;
        return dT1 == dT2 ? dT1 : (dT1 - dT2) / Math.Log(dT1 / dT2);
    }

    public void GetRecuperator()
    {

    }
    public void GetCooler(EnthalpyInterval interval)
    {
        double LMTD = GetLMTD(interval);
        double square = 0;//площадь холодильника
        double operatingCost = 0.0;//эксплуатационные затраты
        double capitalCost = 0.0;//капитальные затраты
        double summCost = operatingCost + capitalCost;//суммарные затраты
        //тепловая нагрузка
    }
    public void GetHeater()
    {

    }
}
//function[summ] = CalculateRecuperator(i, j, var, dQ, T11, T12, T21, T22, isMax) % Расчет рекуператора
//    global CFI CFJ Ahe Khe CA C_gamma year dQhe Nh_streams Nq_i Nl_i Nc_streams Nq_j Nl_j;
//global Nc_sec_u Nh_sec_u Cu_sec_u;
//deltaT = INPUT.AverageLogarithmicDeltaTemperature(T11, T12, T21, T22);
//U = 1 / (1 / CFI(i) + 1 / CFJ(j));
//Ahe(i, j) = dQ / (deltaT * U); % площадь теплообмена
//        if ~isempty(isMax)
//            isMax = false;
//end
//maxLength = max((Nh_streams - Nh_sec_u) * Nq_i * Nl_i, (Nc_streams - Nc_sec_u) * Nq_j * Nl_j);
//if (~isMax && i > (Nh_streams - Nh_sec_u) * Nq_i * Nl_i || j > (Nc_streams - Nc_sec_u) * Nq_j * Nl_j) || (isMax && i > maxLength || j > maxLength)
//            if (~isMax && i > (Nh_streams - Nh_sec_u) * Nq_i * Nl_i && j > (Nc_streams - Nc_sec_u) * Nq_j * Nl_j) || (isMax && i > maxLength && j > maxLength)
//                Cu_ = realmax;
//            else
//    Cu_ = Cu_sec_u;
//end
//        else
//            Cu_ = 0;
//end
//Ehe = dQ * Cu_; % эксплуатационные затраты
//        Khe(i, j) = (CA * power(Ahe(i, j), C_gamma)) / year; % капитальные затраты
//        summ = Khe(i, j) + Ehe;
//summ = summ / (dQhe(i, j) ^ var) * (dQhe(i, j)~=0);
//end

//function[summ] = CalculateHeater(i, j, var, dQ, T11, T12, T21, T22) % Расчет нагревателя
//        global CFJ CFUh Ah Kreb Ereb CA C_gamma Chu year dQreb;
//deltaT = INPUT.AverageLogarithmicDeltaTemperature(T11, T12, T21, T22);
//U = 1 / (1 / CFJ(j) + 1 / CFUh);
//Ah(i, j) = dQ / (deltaT * U); % площадь нагревателя

//        Ereb(i, j) = dQ * Chu; % эксплуатационные затраты
//        Kreb(i, j) = (CA * power(Ah(i, j), C_gamma)) / year; % капитальные затраты
//        summ = Ereb(i, j) + Kreb(i, j);
//summ = summ / (dQreb(i, j) ^ var) * (dQreb(i, j)~=0);
//end

//function[summ] = CalculateCoooler(i, j, var, dQ, T11, T12, T21, T22) % Расчет холодильника
//        global CFI CFUc Ac Kcol Ecol CA C_gamma Ccu year dQcol;
//deltaT = INPUT.AverageLogarithmicDeltaTemperature(T11, T12, T21, T22);
//U = 1 / (1 / CFI(i) + 1 / CFUc);
//Ac(i, j) = dQ / (deltaT * U); % площадь холодильника

//        Ecol(i, j) = dQ * Ccu; % эксплуатационные затраты
//        Kcol(i, j) = (CA * power(Ac(i, j), C_gamma)) / year; % капитальные затраты
//        summ = Ecol(i, j) + Kcol(i, j);
//summ = summ / (dQcol(i, j) ^ var) * (dQcol(i, j)~=0);
//end

//function[deltaT] = AverageLogarithmicDeltaTemperature(T11, T12, T21, T22) % Среднелогарифмическая разность температур
//        dt1 = T11 - T12;
//dt2 = T21 - T22;
//deltaT = power(dt1 * dt2 * (dt1 + dt2) / 2, 1 / 3);
//end