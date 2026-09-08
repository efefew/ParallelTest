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
}