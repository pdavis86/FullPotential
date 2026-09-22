using System;
using System.Collections.Generic;
using System.Text;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Input
{
    [RegisterEvent]
    public struct AttackReleaseEventArgs : IEventArgs
    {
        public FighterBase Fighter { get; }

        public string SlotId {get;}

        public AttackReleaseEventArgs(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }
    }
}
