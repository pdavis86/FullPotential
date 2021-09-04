using FullPotential.Assets.Behaviours.SpellBehaviours;
using FullPotential.Assets.Core.Constants;
using FullPotential.Assets.Core.Helpers;
using FullPotential.Assets.Core.Registry.Types;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class SpellProjectileBehaviour : NetworkBehaviour, ISpellBehaviour
{
    public NetworkVariable<ulong> PlayerClientId;
    public NetworkVariable<string> SpellId;
    public NetworkVariable<Vector3> SpellDirection;

    private Spell _spell;
    private GameObject _sourcePlayer;

    private void Start()
    {
        if (!IsServer)
        {
            //No need to Debug.LogError(). We only want this behaviour on the server
            return;
        }

        Destroy(gameObject, 3f);

        _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId.Value].PlayerObject.gameObject;

        Physics.IgnoreCollision(GetComponent<Collider>(), _sourcePlayer.GetComponent<Collider>());

        _spell = _sourcePlayer.GetComponent<PlayerState>().Inventory.GetItemWithId<Spell>(SpellId.Value);

        if (_spell == null)
        {
            Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
            return;
        }

        var castSpeed = _spell.Attributes.Speed / 50f;
        if (castSpeed < 0.5)
        {
            castSpeed = 0.5f;
        }

        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.AddForce(SpellDirection.Value * 20f * castSpeed, ForceMode.VelocityChange);

        if (_spell.Shape != null)
        {
            rigidBody.useGravity = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        //Debug.Log("Collided with " + other.gameObject.name);

        if (other.gameObject.CompareTag(Tags.Projectile))
        {
            //Debug.Log("You hit a Projectile");
            return;
        }

        //todo: if there is a shape, apply the wall or zone

        ApplySpellEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
    }

    public void ApplySpellEffects(GameObject target, Vector3? position)
    {
        AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
        Destroy(gameObject);
    }

}
