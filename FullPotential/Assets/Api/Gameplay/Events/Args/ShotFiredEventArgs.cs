using FullPotential.Api.Gameplay.Combat;

namespace FullPotential.Api.Gameplay.Events.Args
{
    public class ShotFiredEventArgs : IEventHandlerArgs
    {
        //todo: ShotFired can be cancelled?
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
