using System;

using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Gameplay.Player
{
    public struct SlotIntraAction
    {
        public Func<UniTask> ActionAsync { get; set; }

        public float? DelayBefore { get; set; }

        public float? DelayAfter { get; set; }

        public Func<bool> AdditionalStopCondition { get; set; }
    }
}
