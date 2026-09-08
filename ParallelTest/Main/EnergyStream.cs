public class EnergyStream
{
    /// <summary>
    /// Имя потока
    /// </summary>
    public string Name;
    /// <summary>
    /// Температура входная
    /// </summary>
    public double TimperatureIn;
    /// <summary>
    /// Температура выходная
    /// </summary>
    public double TemperatureOut;
    /// <summary>
    /// Водяной эквивалент
    /// </summary>
    public double WaterEquivalent;

    public EnergyStream()
    {
        Name = string.Empty;
        WaterEquivalent = 0;
        TimperatureIn = 0;
        TemperatureOut = 0;
    }

    public EnergyStream(double tin, double tout)
    {
        Name = string.Empty;
        TimperatureIn = tin;
        TemperatureOut = tout;
    }

    public EnergyStream(string name, double w, double tin, double tout)
    {
        Name = name;
        WaterEquivalent = w;
        TimperatureIn = tin;
        TemperatureOut = tout;
    }

    public void ShiftTimperature(double t)
    {
        TimperatureIn += t;
        TemperatureOut += t;
    }

    public void Copy(EnergyStream stream)
    {
        Name = stream.Name;
        WaterEquivalent = stream.WaterEquivalent;
        TimperatureIn = stream.TimperatureIn;
        TemperatureOut = stream.TemperatureOut;
    }
}