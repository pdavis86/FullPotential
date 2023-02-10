using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;
using UnityEngine;

namespace FullPotential.Api.Utilities
{
    public interface IModHelper
    {
        IGameManager GetGameManager();

        void SpawnShapeGameObject<T>(
            SpellOrGadgetItemBase spellOrGadget,
            IFighter sourceFighter,
            Vector3 startPosition,
            Quaternion rotation)
            where T : IShapeBehaviour;
    }
}