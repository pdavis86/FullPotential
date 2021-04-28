using Assets.Core.Constants;
using Assets.Core.Registry.Types;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class SpellBehaviour : AttackBehaviourBase
{
    private Spell _spell;

    [SyncVar]
    public uint PlayerNetworkId;

    [SyncVar]
    public string SpellId;

    private void Awake()
    {
        Destroy(gameObject, 3f);
    }

    private void Start()
    {
        SourcePlayer = isServer
            ? NetworkServer.FindLocalObject(new NetworkInstanceId(PlayerNetworkId))
            : ClientScene.FindLocalObject(new NetworkInstanceId(PlayerNetworkId));

        if (SourcePlayer == null)
        {
            Debug.LogError("No SourcePlayer found");
            return;
        }

        Physics.IgnoreCollision(GetComponent<Collider>(), SourcePlayer.GetComponent<Collider>());

        _spell = SourcePlayer.GetComponent<PlayerInventory>().Items.FirstOrDefault(x => x.Id == SpellId) as Spell;

        if (_spell == null)
        {
            Debug.LogError("No spell set");
            return;
        }

        var castSpeed = _spell.Attributes.Speed / 50f;
        if (castSpeed < 0.5)
        {
            castSpeed = 0.5f;
        }

        var playerController = SourcePlayer.GetComponent<PlayerController>();

        var spellRb = GetComponent<Rigidbody>();
        spellRb.AddForce(playerController.PlayerCamera.transform.forward * 20f * castSpeed, ForceMode.VelocityChange);
    }

    //private void Update()
    //{
    //    Debug.Log($"Active '{gameObject.activeInHierarchy}', position '{gameObject.transform.position}'");
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
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

            //Debug.Log("You hit something not damageable");

            NetworkServer.Destroy(gameObject);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
