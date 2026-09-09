public class EnergyStream(string name, double w, double tin, double tout)
{
    /// <summary>
    /// Имя потока
    /// </summary>
    public string Name = name;
    /// <summary>
    /// Температура входная
    /// </summary>
    public double TemperatureIn = tin;
    /// <summary>
    /// Температура выходная
    /// </summary>
    public double TemperatureOut = tout;
    /// <summary>
    /// Водяной эквивалент
    /// </summary>
    public double WaterEquivalent = w;

    public EnergyStream() : this(string.Empty, 0, 0, 0)
    {
    }

    public EnergyStream(double tin, double tout) : this(string.Empty, 0, tin, tout)
    {
    }

    public void ShiftTemperature(double t)
    {
        TemperatureIn += t;
        TemperatureOut += t;
    }

    public void Copy(EnergyStream stream)
    {
        Name = stream.Name;
        WaterEquivalent = stream.WaterEquivalent;
        TemperatureIn = stream.TemperatureIn;
        TemperatureOut = stream.TemperatureOut;
    }
}