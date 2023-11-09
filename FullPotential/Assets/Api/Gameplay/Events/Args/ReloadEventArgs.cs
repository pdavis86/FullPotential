using FullPotential.Api.Gameplay.Combat;

namespace FullPotential.Api.Gameplay.Events.Args
{
    public class ReloadEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public IFighter Fighter { get; }

        public bool IsLeftHand { get; }

        public ReloadEventArgs(IFighter fighter, bool isLeftHand)
        {
            Fighter = fighter;
            IsLeftHand = isLeftHand;
        }
    }
}
