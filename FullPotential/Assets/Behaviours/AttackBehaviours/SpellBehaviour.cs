using FullPotential.Assets.Core.Constants;
using FullPotential.Assets.Core.Registry.Types;
using FullPotential.Assets.Helpers;
using MLAPI;
using MLAPI.NetworkVariable;
using System;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class SpellBehaviour : AttackBehaviourBase
{
    private Spell _spell;

    public NetworkVariable<ulong> PlayerClientId;
    public NetworkVariable<string> SpellId;
    public NetworkVariable<Vector3> SpellDirection;

    private void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Destroy(gameObject, 3f);
        }
    }

    private void Start()
    {
        if (!IsServer)
        {
            //No need to Debug.LogError(). We only want this behaviour on the server
            return;
        }

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

        GetComponent<Rigidbody>().AddForce(SpellDirection.Value * 20f * castSpeed, ForceMode.VelocityChange);
    }

    //private void Update()
    //{
    //    Debug.Log($"Active '{gameObject.activeInHierarchy}', position '{gameObject.transform.position}'");
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        try
        {
            //Debug.Log("Collided with " + other.gameObject.name);

            if (other.gameObject.CompareTag(Tags.Projectile))
            {
                //Debug.Log("You hit a Projectile");
                return;
            }

            DealDamage(_spell, gameObject, other.gameObject, other.ClosestPointOnBounds(gameObject.transform.position));

            if (!GameObjectHelper.IsDestroyed(gameObject))
            {
                Destroy(gameObject);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
