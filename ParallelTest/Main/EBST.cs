using Accord;

public class EBST(ConfigEBST config)
{
    private ConfigEBST _config = config;
    public Utility? Heater { get; private set; }
    public Utility? Cooler { get; private set; }
    public Utility? Recuperator { get; private set; }
    public TypeEBST GetTypeEBST()
    {
        if (Recuperator == null) return TypeEBST.CoolerAndHeater;

        if (Heater != null && Cooler != null) return TypeEBST.All;
        if (Heater != null) return TypeEBST.RecuperatorAndHeater;
        if (Cooler != null) return TypeEBST.RecuperatorAndCooler;

        return TypeEBST.Recuperator;
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

    public Utility CalculateRecuperator(EnthalpyInterval interval, double cost, double heatLoad, double heatTransferCoeff, double heatTransferExternalCoeff)
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
        Recuperator.CapitalCost = (config.CostCoeff * Math.Pow(Recuperator.Area, _config.CGamma)) / _config.Years;
        Recuperator.SummCost = Recuperator.OperatingCost + Recuperator.CapitalCost;
        Recuperator.SummCost=(Recuperator.HeatLoad != 0) ? (Recuperator.SummCost / Math.Pow(Recuperator.HeatLoad, _config.CostExponent)) : 0.0;
        return Recuperator;

    }
    /// <summary>
    /// Рассчитать нагреватель
    /// </summary>
    /// <returns>Нагреватель</returns>
    public Utility CalculateHeater(ExternalUtility heaterData, StageInDivision coldPoint)
    {
        if (Heater == null) Heater = new Utility();
        Heater.HeatLoad = coldPoint.HeatLoad;
        EnthalpyInterval interval = new EnthalpyInterval(coldPoint.TemperatureIn, coldPoint.TemperatureOut, heaterData.TimperatureIn, heaterData.TemperatureOut);
        Heater = CalculateUtility(interval, heaterData.Cost, Heater, coldPoint.WaterEquivalent, heaterData.HeatTransferCoefficient);
        return Heater;
    }
    /// <summary>
    /// Рассчитать холодильник
    /// </summary>
    /// <returns>Холодильник</returns>
    public Utility CalculateCooler(ExternalUtility coolerData, StageInDivision hotPoint)
    {
        if (Cooler == null) Cooler = new Utility();
        Cooler.HeatLoad = hotPoint.HeatLoad;
        EnthalpyInterval interval = new EnthalpyInterval(coolerData.TemperatureOut, coolerData.TimperatureIn, hotPoint.TemperatureOut, hotPoint.TemperatureIn);
        Cooler = CalculateUtility(interval, coolerData.Cost, Cooler, hotPoint.WaterEquivalent, coolerData.HeatTransferCoefficient);
        return Cooler;
    }   
    /// <summary>
    /// Рассчитать утилиту
    /// </summary>
    /// <param name="interval">Интервал температур</param>
    /// <param name="cost">Стоимость утилиты</param>
    /// <param name="heatTransferCoeff">Коэффициент теплопередачи утилиты</param>
    /// <param name="heatTransferExternalCoeff">Коэффициент теплопередачи внешней утилиты</param>
    /// <returns>Cуммарные затраты</returns>
    private Utility CalculateUtility(EnthalpyInterval interval, double cost, Utility utility, double heatTransferCoeff, double heatTransferExternalCoeff)
    {
        double deltaT = GetLMTD(interval);
        //TODO общий коэффициент теплопередачи (без учёта сопротивления стенки R=δ/λ)
        double overallHeatTransferCoeff = 1 / (1 / heatTransferCoeff + 1 / heatTransferExternalCoeff);
        
        utility.Area = utility.HeatLoad / (deltaT * overallHeatTransferCoeff);

        utility.OperatingCost = utility.HeatLoad * cost;
        utility.CapitalCost = (config.CostCoeff * Math.Pow(utility.Area, _config.CGamma)) / _config.Years;
        utility.SummCost = utility.OperatingCost + utility.CapitalCost;
        utility.SummCost = (utility.HeatLoad != 0) ? (utility.SummCost / Math.Pow(utility.HeatLoad, _config.CostExponent)) : 0.0;
        return utility;
    }
    public double GetSummCost()
    {
        double summCost = 0;
        if (Heater != null) summCost += Heater.SummCost;
        if (Cooler != null) summCost += Cooler.SummCost;
        if (Recuperator != null) summCost += Recuperator.SummCost;
        return summCost;
    }
}
public enum TypeEBST
{
    CoolerAndHeater,
    All,
    RecuperatorAndHeater,
    RecuperatorAndCooler,
    Recuperator
}