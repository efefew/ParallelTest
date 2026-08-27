public class EnthalpyInterval
{
    public double EnthalpyIn, EnthalpyOut;
    public double TemperatureColdIn, TemperatureColdOut;
    public double TemperatureHotIn, TemperatureHotOut;

    public Dictionary<int, EnergyStream> HotStreams, ColdStreams;
    public double GetBetaCold(int idStream)
    {
        if (!HotStreams.ContainsKey(idStream))
            return 0;
        double wStreams = HotStreams.Sum(stream => stream.Value.W);
        return HotStreams[idStream].W / wStreams;
    }
    public double GetBetaHot(int idStream)
    {
        if (!ColdStreams.ContainsKey(idStream))
            return 0;
        double wInInterval = ColdStreams.Sum(stream => stream.Value.W);
        return ColdStreams[idStream].W / wInInterval;
    }
    public double GetAlphaHot(int idStream)
    {
        if (!HotStreams.ContainsKey(idStream))
            return 0;
        double deltaTemperatureInStream = HotStreams[idStream].Tin - HotStreams[idStream].Tout;
        double deltaTemperatureInInterval = TemperatureHotIn - TemperatureHotOut;
        return deltaTemperatureInInterval / deltaTemperatureInStream;
    }
    public double GetAlphaCold(int idStream)
    {
        if (!ColdStreams.ContainsKey(idStream))
            return 0;
        double deltaTemperatureInStream = ColdStreams[idStream].Tout - ColdStreams[idStream].Tin;
        double deltaTemperatureInInterval = TemperatureColdOut - TemperatureColdIn;
        return deltaTemperatureInInterval / deltaTemperatureInStream;
    }
}