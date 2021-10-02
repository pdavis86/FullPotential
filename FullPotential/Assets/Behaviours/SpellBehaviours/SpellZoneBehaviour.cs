using FullPotential.Assets.Behaviours.SpellBehaviours;
using FullPotential.Assets.Core.Constants;
using FullPotential.Assets.Core.Helpers;
using FullPotential.Assets.Core.Registry.Types;
using FullPotential.Assets.Extensions;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

public class SpellZoneBehaviour : NetworkBehaviour, ISpellBehaviour
{
    public NetworkVariable<ulong> PlayerClientId;
    public NetworkVariable<string> SpellId;

    private GameObject _sourcePlayer;
    private Spell _spell;
    private float _timeSinceLastEffective;
    private float _timeBetweenEffects;

    private void Start()
    {
        if (!IsServer)
        {
            //No need to Debug.LogError(). We only want this behaviour on the server
            return;
        }

        //todo: for how long does this live?
        Destroy(gameObject, 10f);

        _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId.Value].PlayerObject.gameObject;

        _spell = _sourcePlayer.GetComponent<PlayerState>().Inventory.GetItemWithId<Spell>(SpellId.Value);

        if (_spell == null)
        {
            Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
            return;
        }

        _timeBetweenEffects = 0.5f;
        _timeSinceLastEffective = _timeBetweenEffects;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (_timeSinceLastEffective < _timeBetweenEffects)
        {
            _timeSinceLastEffective += Time.deltaTime;
            return;
        }

        _timeSinceLastEffective = 0;

        if (!other.gameObject.CompareTagAny(Tags.Player, Tags.Enemy))
        {
            //Debug.Log("You hit something not damageable");
            return;
        }

        ApplySpellEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
    }

    public void ApplySpellEffects(GameObject target, Vector3? position)
    {
        //throw new System.NotImplementedException();
        Debug.Log("Applying spell effects to " + target.name);

        var adjustedPosition = position + new Vector3(0, 1);
        AttackHelper.DealDamage(_sourcePlayer, _spell, target, adjustedPosition);
    }
}
