using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Combat.EventArgs
{
    public class DamageTakenEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public LivingEntityBase LivingEntity { get; }

        public DamageTakenEventArgs(LivingEntityBase livingEntity)
        {
            LivingEntity = livingEntity;
        }
    }
}
