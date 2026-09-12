internal class DataMILP(
    ConfigEBST ebst,
    FullEnergyStream[] hotStreams,
    FullEnergyStream[] coldStreams,
    ExternalUtility[] hotExternalUtilities,
    ExternalUtility[] coldExternalUtilities)
{
    public FullEnergyStream[] HotStreams = hotStreams, ColdStreams = coldStreams;
    public ExternalUtility[] HotExternalUtilities = hotExternalUtilities, ColdExternalUtilities = coldExternalUtilities;
    public ConfigEBST Ebst = ebst;

    public int GetHotLength(int id = 0)
    {
        return HotStreams.Length * HotStreams[id].Stages.Length * HotStreams[id].Divisions.Length;
    }
    public int GetColdLength(int id = 0)
    {
        return ColdStreams.Length * ColdStreams[id].Stages.Length * ColdStreams[id].Divisions.Length;
    }
}