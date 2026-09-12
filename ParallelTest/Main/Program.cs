using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

internal static class Program
{
    public static void Main()
    {
        ConfigEBST ebst  = new(5, 0.65, 1, 0, 380, 0);

        double[] hotDivisions = [0.24, 0.34, 0.3, 0.12];
        FullEnergyStream[] hotStreams =
        [
            new(0.81, 503, 308, 66.4, [0.33, 0.67], hotDivisions),
            new(1.78, 426, 425, 33020, [0.51, 0.49], hotDivisions),
            new(1.62, 382, 381, 12870, [0.62, 0.38], hotDivisions)
        ];
        
        double[] coldDivisions = [0.3, 0.44, 0.26];
        FullEnergyStream[]  coldStreams =
        [
            new(0.72, 323, 503, 49.1, [0.43, 0.57], coldDivisions),
            new(1.91, 408, 409, 18413.1, [0.59, 0.41], coldDivisions),
            new(1.76, 391, 392, 18498.4, [0.65, 0.35], coldDivisions),
            new(1.84, 353, 354, 16347.9, [0.66, 0.34], coldDivisions)
        ];
        
        ExternalUtility[] hotExternalUtilities = [new("Hot", 627, 626, 2.5, 100)];
        ExternalUtility[] coldExternalUtilities = [new("Cold", 303, 315, 1, 10)];

        DataMILP data = new (ebst, hotStreams, coldStreams, hotExternalUtilities, coldExternalUtilities);

        Tests.CompareTestTimes(Parallel1, Mono, 50000);
        return;

        void Mono() => Milp.Run(data);

        void Parallel1() => Milp.RunParallel1(data);
    }
}