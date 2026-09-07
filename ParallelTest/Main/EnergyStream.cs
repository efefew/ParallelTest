public class EnergyStream
{
    public string Name;
    public float Tin, Tout;
    public float W;

    public EnergyStream()
    {
        Name = string.Empty;
        W = 0;
        Tin = 0;
        Tout = 0;
    }

    public EnergyStream(float tin, float tout)
    {
        Name = string.Empty;
        Tin = tin;
        Tout = tout;
    }

    public EnergyStream(string name, float w, float tin, float tout)
    {
        Name = name;
        W = w;
        Tin = tin;
        Tout = tout;
    }

    public void AddT(float t)
    {
        Tin += t;
        Tout += t;
    }

    public void Copy(EnergyStream stream)
    {
        Name = stream.Name;
        W = stream.W;
        Tin = stream.Tin;
        Tout = stream.Tout;
    }
}