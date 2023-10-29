using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;

namespace FullPotential.Api.Registry.Targeting
{
    public interface ITargeting : IRegisterable
    {
        bool CanHaveShape { get; }

        bool IsContinuous { get; }

        string NetworkPrefabAddress { get; }

        string VisualsFallbackPrefabAddress { get; }

        IEnumerable<ViableTarget> GetTargets(IFighter sourceFighter, Consumer consumer);
    }
}
