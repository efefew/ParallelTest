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
    private double[,] _alpha;

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
        SetAlpha(stages, divisions);
        BuildStages(stages);
        BuildDivisions(divisions);
    }

    private void BuildDivisions(double[] divisions)
    {
        Divisions = new Division[divisions.Length];
        for (int idDivision = 1; idDivision < Divisions.Length; idDivision++)
        {
            Divisions[idDivision] = new Division(divisions[idDivision]);
        }
    }

    private void BuildStages(double[] stages)
    {
        {
            Stages = new Stage[stages.Length];
            Stages[0] = new Stage(
                stages[0], 
                TemperatureIn,
                TemperatureIn + Heat * stages[0] / HeatCapacity,
                this);
            for (int idStage = 0; idStage < Stages.Length; idStage++)
            {
                Stages[idStage] = new Stage(stages[idStage], 
                    GetDivT(idStage - 1),
                    GetDivT(idStage),
                    this);
            }
        }
        return;

        double GetDivT(int idStage)
        {
            //TODO  Stages[idStage] заменить

            double shiftTemperature = Heat * Stages[idStage].Beta / HeatCapacity;
            if(TemperatureIn > TemperatureOut) shiftTemperature = -shiftTemperature;
            return Stages[idStage].TemperatureIn + shiftTemperature;
        }
    }

    public double GetAlpha(int idStage, int idDivision)
    {
        return _alpha[idStage, idDivision];
    }
    public void SetAlpha(int idStage, int idDivision, double value)
    {
        _alpha[idStage, idDivision] = value;
    }
    private void SetAlpha(double[] stages, double[] divisions)
    {
        _alpha = new double[stages.Length, divisions.Length];
        for (int idStage = 0; idStage < stages.Length; idStage++)
        {
            for (int idDivision = 0; idDivision < divisions.Length; idDivision++)
                _alpha[idStage, idDivision] = divisions[idDivision];
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
    public Division(double gamma)
    {
        Gamma = gamma;
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
    /// <summary>
    /// Теплоёмкость
    /// </summary>
    public double HeatCapacity;
    /// <summary>
    /// Водяной эквивалент
    /// </summary>
    public double WaterEquivalent;
    public double Heat;
    public Stage(double beta, double tin, double tout, FullEnergyStream energyStream)
    {
        Beta = beta;
        TemperatureIn = tin;
        TemperatureOut = tout;
        //TODO держать в цикле делений
        Heat = energyStream.Heat * Beta/* * energyStream.GetAlpha()*/;
        HeatCapacity = energyStream.HeatCapacity;
        WaterEquivalent = energyStream.WaterEquivalent;
        
    }
}