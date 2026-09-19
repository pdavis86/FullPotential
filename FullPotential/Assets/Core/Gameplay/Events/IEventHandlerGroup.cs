namespace FullPotential.Core.Gameplay.Events
{
    public interface IEventHandlerGroup
    {
        void Add(object handler);

        bool Remove(object handler);
    }
}
