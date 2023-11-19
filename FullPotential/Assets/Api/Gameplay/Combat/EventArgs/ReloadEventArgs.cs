using System;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.EventArgs
{
    public class ReloadEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public IFighter Fighter { get; }

        public bool IsLeftHand { get; }

        public Func<int> GetNewAmmoCount { get; set; }

        public ReloadEventArgs(IFighter fighter, bool isLeftHand)
        {
            Fighter = fighter;
            IsLeftHand = isLeftHand;
        }
    }
}
