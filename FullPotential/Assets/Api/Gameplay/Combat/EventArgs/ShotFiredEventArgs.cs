using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.EventArgs
{
    public class ShotFiredEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public IFighter Fighter { get; }

        public bool IsLeftHand { get; }

        public ShotFiredEventArgs(IFighter fighter, bool isLeftHand)
        {
            Fighter = fighter;
            IsLeftHand = isLeftHand;
        }
    }
}
