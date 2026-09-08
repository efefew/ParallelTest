public class FullEnergyStream : EnergyStream
{
    /// <summary>
    /// Стадии
    /// </summary>
    public double[] Stages;
    /// <summary>
    /// Делелния
    /// </summary>
    public double[] Divisions;
    /// <summary>
    /// Теплоёмкость
    /// </summary>
    public double HeatCapacity;
    public FullEnergyStream(string name, double w, double tin, double tout, double heatCapacity, double[] stages, double[] divisions) : base(name, w, tin, tout)
    {
        Stages = stages;
        Divisions = divisions;
        HeatCapacity = heatCapacity;
    }
    public FullEnergyStream(EnergyStream energyStream, double heatCapacity, double[] stages, double[] divisions) : base(energyStream.Name, energyStream.WaterEquivalent, energyStream.TimperatureIn, energyStream.TemperatureOut)
    {
        Stages = stages;
        Divisions = divisions;
        HeatCapacity = heatCapacity;
    }
    private double? _heat;
    /// <summary>
    /// Количество теплоты
    /// </summary>
    public double Heat
    {
        get
        {
            _heat ??= HeatCapacity * (TimperatureIn - TemperatureOut);
            return (double)_heat;
        }
    }
}
