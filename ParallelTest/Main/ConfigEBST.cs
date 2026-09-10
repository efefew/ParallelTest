public struct ConfigEBST
{
    /// <summary>
    /// Минимально допустимая разность температур
    /// </summary>
    public double MinDeltaT;
    /// <summary>
    /// Коэффициент корреляции
    /// </summary>
    public double CGamma;
    /// <summary>
    /// Года
    /// </summary>
    public double Years;
    /// <summary>
    /// Параметр как var в matlab
    /// </summary>
    public double CostExponent;
    /// <summary>
    /// Фиксированная стоимость
    /// </summary>
    public double Cost;
    /// <summary>
    /// Cтоимостной коэффициент теплобменника($)
    /// </summary>
    public double CostCoeff;
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