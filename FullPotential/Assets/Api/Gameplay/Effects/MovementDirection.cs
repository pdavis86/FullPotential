namespace FullPotential.Api.Gameplay.Effects
{
    // todo: Should MovementDirection be generalised? Or all types moved into Core?
    public enum MovementDirection
    {
        LeftFromSource,
        RightFromSource,
        Up,
        Down,
        Forwards,
        Backwards,
        TowardSource,
        AwayFromSource,
        MaintainDistance
    }
}
