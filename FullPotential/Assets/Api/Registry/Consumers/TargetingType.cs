namespace FullPotential.Api.Registry.Consumers
{
    //todo: not an enum. each of these needs to be a class that has props for all of the current ITargeting props
    public enum TargetingType
    {
        InRange,
        CloseProximity,
        LineOfSight,
        PointToPoint,
        Self
    }
}
