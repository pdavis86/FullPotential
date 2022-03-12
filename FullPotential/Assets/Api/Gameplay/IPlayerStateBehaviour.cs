using FullPotential.Api.Registry.SpellsAndGadgets;
using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerStateBehaviour : IFighter
    {
        ulong OwnerClientId { get; }

        Transform Transform { get; }

        GameObject GameObject { get; }

        GameObject CameraGameObject { get; }

        void SpawnLootChest(Vector3 position);

        bool ConsumeResource(SpellOrGadgetItemBase spellOrGadget, bool slowDrain = false, bool isTest = false);
    }
}
