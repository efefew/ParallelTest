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
        double wStreams = HotStreams.Sum(stream => stream.Value.WaterEquivalent);
        return HotStreams[idStream].WaterEquivalent / wStreams;
    }
    public double GetBetaHot(int idStream)
    {
        if (!ColdStreams.ContainsKey(idStream))
            return 0;
        double wInInterval = ColdStreams.Sum(stream => stream.Value.WaterEquivalent);
        return ColdStreams[idStream].WaterEquivalent / wInInterval;
    }
    public double GetAlphaHot(int idStream)
    {
        if (!HotStreams.ContainsKey(idStream))
            return 0;
        double deltaTemperatureInStream = HotStreams[idStream].TimperatureIn - HotStreams[idStream].TemperatureOut;
        double deltaTemperatureInInterval = TemperatureHotIn - TemperatureHotOut;
        return deltaTemperatureInInterval / deltaTemperatureInStream;
    }
    public double GetAlphaCold(int idStream)
    {
        if (!ColdStreams.ContainsKey(idStream))
            return 0;
        double deltaTemperatureInStream = ColdStreams[idStream].TemperatureOut - ColdStreams[idStream].TimperatureIn;
        double deltaTemperatureInInterval = TemperatureColdOut - TemperatureColdIn;
        return deltaTemperatureInInterval / deltaTemperatureInStream;
    }
}