// ReSharper disable once InconsistentNaming
/// <summary>
/// Данные для ЭБСТ
/// </summary>
/// <param name="minDeltaT">Минимально допустимая разность температур</param>
/// <param name="cGamma">Коэффициент корреляции</param>
/// <param name="years">Года</param>
/// <param name="costExponent">Параметр как var в matlab</param>
/// <param name="costCoeff">Cтоимостной коэффициент теплобменника($)</param>
/// <param name="cost">Фиксированная стоимость</param>
public struct ConfigEBST(double minDeltaT, double cGamma, int years, double costExponent, double costCoeff, double cost)
{
    /// <summary>
    /// Минимально допустимая разность температур
    /// </summary>
    public double MinDeltaT = minDeltaT;
    /// <summary>
    /// Коэффициент корреляции
    /// </summary>
    public double CGamma = cGamma;
    /// <summary>
    /// Года
    /// </summary>
    public int Years = years;
    /// <summary>
    /// Параметр как var в matlab
    /// </summary>
    public double CostExponent = costExponent;
    /// <summary>
    /// Фиксированная стоимость
    /// </summary>
    public double Cost = cost;
    /// <summary>
    /// Cтоимостной коэффициент теплобменника($)
    /// </summary>
    public double CostCoeff = costCoeff;

    /// <summary>
    /// Точность декомпозиции
    /// </summary>
    public const double TOLERANCE_DECOMPOSITION = 0.0000001;
    /// <summary>
    /// Точность процедуры агрегирования
    /// </summary>
    public const double TOLERANCE_AGGREGATION_PROCEDURE = 0.0000001;
    /// <summary>
    /// Точность конструкции
    /// </summary>
    public const double TOLERANCE_CONSTR = 0.0001;
}