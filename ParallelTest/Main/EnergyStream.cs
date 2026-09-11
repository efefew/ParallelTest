public class EnergyStream(double w, double tin, double tout, string? name = null)
{
    /// <summary>
    /// Имя потока
    /// </summary>
    public string Name = name ?? $"{tin} - {tout}";
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

    public EnergyStream() : this( 0, 0, 0)
    {
    }

    public EnergyStream(double tin, double tout) : this(0, tin, tout)
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