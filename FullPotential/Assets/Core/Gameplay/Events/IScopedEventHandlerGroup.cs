namespace FullPotential.Core.Gameplay.Events
{
    public interface IScopedEventHandlerGroup
    {
        void RemoveByOwner(object owner);
    }
}
