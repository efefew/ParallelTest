namespace HenSQP
{
    public class HenOptimizationData(EBST ebst, DataMILP milp, StageInDivision hotPoint, StageInDivision coldPoint)
    {
        public StageInDivision HotPoint = hotPoint;
        public StageInDivision ColdPoint = coldPoint;
        public EBST Ebst = ebst;
        public DataMILP Milp = milp;
    }
}
