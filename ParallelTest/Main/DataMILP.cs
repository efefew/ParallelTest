internal class DataMILP(
    ConfigEBST ebst,
    List<FullEnergyStream> hotStreams,
    List<FullEnergyStream> coldStreams,
    List<ExternalUtility> hotExternalUtilities,
    List<ExternalUtility> coldExternalUtilities)
{
    public List<FullEnergyStream> HotStreams = hotStreams, ColdStreams = coldStreams;
    public List<ExternalUtility> HotExternalUtilities = hotExternalUtilities, ColdExternalUtilities = coldExternalUtilities;
    public ConfigEBST Ebst = ebst;

    public int GetHotLength(int id = 0)
    {
        return HotStreams.Count * HotStreams[id].Stages.Length * HotStreams[id].Divisions.Length;
    }
    public int GetColdLength(int id = 0)
    {
        return ColdStreams.Count * ColdStreams[id].Stages.Length * ColdStreams[id].Divisions.Length;
    }
}