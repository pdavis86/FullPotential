using System;
using FullPotential.Api.Registry.Spells;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Standard.Spells.Behaviours;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Standard.Spells.Targeting
{
    public class Projectile : ISpellTargeting
    {
        public Guid TypeId => new Guid("6e41729e-bb21-44f8-8fb9-b9ad48c0e680");

        public string TypeName => nameof(Projectile);

        public bool HasShape => true;

        public bool IsContinuous => false;

        public GameObject SpawnGameObject(Spell activeSpell, Vector3 startPosition, Vector3 targetDirection, ulong senderClientId, bool isLeftHand = false, Transform parentTransform = null)
        {
            //todo: prefab should be an addressable
            var spellObject = UnityEngine.Object.Instantiate(GameManager.Instance.Prefabs.Combat.SpellProjectile, startPosition, Quaternion.identity);

            var spellScript = spellObject.GetComponent<SpellProjectileBehaviour>();
            spellScript.PlayerClientId = senderClientId;
            spellScript.SpellId = activeSpell.Id;
            spellScript.SpellDirection = targetDirection;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();

            return spellObject;
        }
    }
}
