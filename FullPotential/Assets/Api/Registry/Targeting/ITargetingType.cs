using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;

// ReSharper disable UnusedParameter.Global

namespace FullPotential.Api.Registry.Targeting
{
    public interface ITargetingType : IRegisterableType, IHasNetworkPrefab
    {
        bool CanHaveShape { get; }

        bool IsContinuous { get; }

        IEnumerable<ViableTarget> GetTargets(FighterBase sourceFighter, Consumer consumer);
    }
}
