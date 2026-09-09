public class DataMILP
{
    //public int CountHotStreams, CountColdStreams, CountHotStage, CountColdStage, CountHotDivision, CountColdDivision;
    public List<FullEnergyStream> HotStreams = new(), ColdStreams = new();
    public List<ExternalUtility> HotExternalUtilities = new(), ColdExternalUtilities = new();
    public ConfigEBST Ebst;
    /// <summary>
    /// Точность декомпозиции
    /// </summary>
    public const double TOLERANCE_DECOMPOSITION = 0.0000001;
    /// <summary>
    /// Точность процедуры агрегирования
    /// </summary>
    public const double TOLERANCE_AGGREGATION_PROCEDURE = 0.0000001;
    /// <summary>
    /// Точность конструкции?
    /// </summary>
    public const double TOLERANCE_CONSTR = 0.0001;
}
internal class Milp
{
    public void Run(DataMILP data)
    {
        // ЭТАП 2 РЕШЕНИЕ ЗАДАЧИ MILP
        double summCost = 0;
        // ЭТАП 1 НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
        for (int idHotStream = 0; idHotStream < data.HotStreams.Count; idHotStream++)
            for (int idHotStage = 0; idHotStage < data.HotStreams[idHotStream].Stages.Length; idHotStage++)
                for (int idHotDivision = 0; idHotDivision < data.HotStreams[idHotStream].Divisions.Length; idHotDivision++)
                    for (int idColdStream = 0; idColdStream < data.HotStreams.Count; idColdStream++)
                        for (int idColdStage = 0; idColdStage < data.HotStreams[idColdStream].Stages.Length; idColdStage++)
                            for (int idColdDivision = 0; idColdDivision < data.HotStreams[idColdStream].Divisions.Length; idColdDivision++)
                            {
                                SolveMILP(data, idHotStream, idHotStage, idHotDivision, idColdStream, idColdStage, idColdDivision, ref summCost);
                            }
    }

    private void SolveMILP(DataMILP data, int idHotStream, int idHotStage, int idHotDivision, int idColdStream, int idColdStage, int idColdDivision, ref double summCost)
    {
        StageInDivision hotPoint = data.HotStreams[idHotStream].Divisions[idHotDivision].Stages[idHotStage];
        StageInDivision coldPoint = data.ColdStreams[idColdStream].Divisions[idColdDivision].Stages[idColdStage];
        if (hotPoint.TemperatureIn - coldPoint.TemperatureIn < data.Ebst.MinDeltaT)
        {

        }
    }
}