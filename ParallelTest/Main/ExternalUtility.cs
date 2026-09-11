/// <summary>
/// Внешний энергоноситель (утилита)
/// </summary>
public class ExternalUtility(
    string name,
    double temperatureIn,
    double temperatureOut,
    double heatTransferCoefficient,
    double cost)
{
    /// <summary>
    /// Имя утилиты
    /// </summary>
    public string Name = name;
    /// <summary>
    /// Температура входная
    /// </summary>
    public double TemperatureIn = temperatureIn;
    /// <summary>
    /// Температура выходная
    /// </summary>
    public double TemperatureOut = temperatureOut;
    /// <summary>
    /// Коэффициент теплопередачи
    /// </summary>
    public double HeatTransferCoefficient = heatTransferCoefficient;
    /// <summary>
    /// Стоимость
    /// </summary>
    public double Cost = cost;
}