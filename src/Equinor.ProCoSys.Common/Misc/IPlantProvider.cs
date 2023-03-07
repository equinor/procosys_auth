namespace Equinor.ProCoSys.Common.Misc
{
    public interface IPlantProvider
    {
        string Plant { get; }
        bool IsCrossPlantQuery { get; }
    }
}
