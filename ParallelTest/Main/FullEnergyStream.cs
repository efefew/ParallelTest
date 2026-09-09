using Accord.Math;

public class FullEnergyStream : EnergyStream
{
    /// <summary>
    /// Стадии
    /// </summary>
    public Stage[] Stages;

    /// <summary>
    /// Теплоёмкость входная
    /// </summary>
    public double HeatCapacity;
    /// <summary>
    /// Потоки делелния
    /// </summary>
    public Division[] Divisions;

    public FullEnergyStream(string name, double w, double tin, double tout, double heatCapacity, double[] stages, double[] divisions) : base(name, w, tin, tout)
    {
        HeatCapacity = heatCapacity;
        Build(stages, divisions);
    }
    public FullEnergyStream(EnergyStream energyStream, double heatCapacity, double[] stages, double[] divisions) : base(energyStream.Name, energyStream.WaterEquivalent, energyStream.TemperatureIn, energyStream.TemperatureOut)
    {
        HeatCapacity = heatCapacity;
        Build(stages, divisions);
    }

    private void Build(double[] stages, double[] divisions)
    {
        BuildStages(stages);
        BuildDivisions(divisions);
    }

    private void BuildDivisions(double[] divisions)
    {
        Divisions = new Division[divisions.Length];
        for (int idDivision = 1; idDivision < Divisions.Length; idDivision++)
        {
            Divisions[idDivision] = new Division(divisions[idDivision], Stages, this);
        }
    }

    private void BuildStages(double[] stages)
    {
        {
            Stages = new Stage[stages.Length];
            double shiftTemperature = Heat * stages[0] / HeatCapacity;
            if (TemperatureIn > TemperatureOut) shiftTemperature = -shiftTemperature;
            Stages[0] = new Stage(stages[0], TemperatureIn, TemperatureIn + shiftTemperature);
            for (int idStage = 0; idStage < Stages.Length; idStage++)
            {
                double tin = GetDivT(idStage - 1, stages[idStage - 1], Stages[idStage - 1].TemperatureIn);
                Stages[idStage] = new Stage(stages[idStage], tin, GetDivT(idStage, stages[idStage], tin));
            }
        }
        return;

        double GetDivT(int idStage, double beta, double tin)
        {
            double shiftTemperature = Heat * beta / HeatCapacity;
            if(TemperatureIn > TemperatureOut) shiftTemperature = -shiftTemperature;
            return tin + shiftTemperature;
        }
    }


    private double? _heat;
    /// <summary>
    /// Количество теплоты
    /// </summary>
    public double Heat
    {
        get
        {
            _heat ??= HeatCapacity * Math.Abs(TemperatureIn - TemperatureOut);
            return (double)_heat;
        }
    }
}

public class Division
{
    public double Gamma;
    public StageInDivision[] Stages;
    public Division(double gamma, Stage[] stages, FullEnergyStream stream)
    {
        Gamma = gamma;
        Stages = new StageInDivision[stages.Length];
        for(int idStage = 0; idStage < stages.Length; idStage++)
        {
            Stages[idStage] = new(stages[idStage], this, stream);
        }
    }
}
public class StageInDivision
{
    /// <summary>
    /// Теплоёмкость
    /// </summary>
    public double HeatCapacity;
    /// <summary>
    /// Водяной эквивалент
    /// </summary>
    public double WaterEquivalent;
    /// <summary>
    /// Температура входная
    /// </summary>
    public double TemperatureIn;
    /// <summary>
    /// Температура выходная
    /// </summary>
    public double TemperatureOut;
    public double Heat;
    public Stage Stage { get; private set; }
    public Division Division { get; private set; }
    public double Alpha;

    public StageInDivision(Stage stage, Division division, FullEnergyStream stream)
    {
        Stage = stage;
        Division = division;
        Alpha = division.Gamma;

        Heat = stream.Heat * stage.Beta * Alpha;
        HeatCapacity = stream.HeatCapacity * Alpha;
        WaterEquivalent = stream.WaterEquivalent;
        TemperatureIn = stage.TemperatureIn;
        if (HeatCapacity == 0)
            TemperatureOut = TemperatureIn;
        else
        {
            double shiftTemperature = Heat / HeatCapacity;
            if (stream.TemperatureIn > stream.TemperatureOut) shiftTemperature = -shiftTemperature;
            TemperatureOut = TemperatureIn + shiftTemperature;
        }

    }
}
public class Stage
{
    public double Beta;
    /// <summary>
    /// Температура входная
    /// </summary>
    public double TemperatureIn;
    /// <summary>
    /// Температура выходная
    /// </summary>
    public double TemperatureOut;
    public Stage(double beta, double tin, double tout)
    {
        Beta = beta;
        TemperatureIn = tin;
        TemperatureOut = tout;
    }
}