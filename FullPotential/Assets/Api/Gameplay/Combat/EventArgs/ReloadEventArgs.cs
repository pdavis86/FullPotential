using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.EventArgs
{
    public class ReloadEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public FighterBase Fighter { get; }

        public bool IsLeftHand { get; }

        public Func<int> GetNewAmmoCount { get; set; }

        public ReloadEventArgs(FighterBase fighter, bool isLeftHand)
        {
            Fighter = fighter;
            IsLeftHand = isLeftHand;
        }
    }
}
