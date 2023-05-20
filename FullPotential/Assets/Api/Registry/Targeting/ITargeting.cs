using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;

namespace FullPotential.Api.Registry.Targeting
{
    public interface ITargeting : IRegisterable
    {
        //todo: implement
        bool CanHaveShape { get; }

        bool IsContinuous { get; }

        bool VisualsParentedToSource { get; }

        string VisualsFallbackPrefabAddress { get; }

        IEnumerable<ViableTarget> GetTargets(IFighter sourceFighter, Consumer consumer);
    }
}
