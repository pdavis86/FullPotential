namespace FullPotential.Core.Gameplay.Events
{
    public interface IGeneralEventHandlerGroup
    {
        void Add(object handler);

        bool Remove(object handler);
    }
}
