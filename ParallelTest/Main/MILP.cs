public class DataMILP
{
    //public int CountHotStreams, CountColdStreams, CountHotStage, CountColdStage, CountHotDivision, CountColdDivision;
    public List<FullEnergyStream> HotStreams = new(), ColdStreams = new();
    public List<ExternalUtility> HotExternalUtilities = new(), ColdExternalUtilities = new();
    public ConfigEBST configEBST;
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
internal class MILP
{
    public void Run(DataMILP data)
    {
        // ЭТАП 2 РЕШЕНИЕ ЗАДАЧИ MILP
        double summCost = 0;
        SystemParam(data);
        int idHotStream = 0, idColdStream = 0;
        int countHot = data.HotStreams.Count * data.HotStreams[idHotStream].Stages.Length * data.HotStreams[idHotStream].Divisions.Length;
        int countCold = data.ColdStreams.Count * data.ColdStreams[idColdStream].Stages.Length * data.ColdStreams[idColdStream].Divisions.Length;
        // ЭТАП 1 НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
        for (int idHot = 0; idHot < countHot; idHot++)
        {
            for (int idCold = 0; idCold < countCold; idCold++)
            {
                
            }
        }
    }

    private void SystemParam(DataMILP data)
    {
        for (int idCold = 0; idCold < data.ColdStreams.Count; idCold++)
        {

        }
    }
}