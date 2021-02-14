using Assets.Scripts.Crafting.Results;
using System;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class SpellBehaviour : AttackBehaviourBase
{
    public Spell Spell;

    [SyncVar]
    public uint PlayerNetworkId;

    private void Awake()
    {
        Destroy(gameObject, 3f);
    }

    private void Start()
    {
        if (isServer)
        {
            SourcePlayer = NetworkServer.FindLocalObject(new NetworkInstanceId(PlayerNetworkId));
        }
        else
        {
            SourcePlayer = ClientScene.FindLocalObject(new NetworkInstanceId(PlayerNetworkId));
        }

        Physics.IgnoreCollision(GetComponent<Collider>(), SourcePlayer.GetComponent<Collider>());

        if (!isServer)
        {
            //Debug.LogError("Player tried to deal damage instead of server");
            return;
        }

        //var test2 = ClientScene.FindLocalObject(new NetworkInstanceId(PlayerNetworkId));

        //if (SourcePlayer == null)
        //{
        //    Debug.LogError("I don't have a SourcePlayer :'(");
        //}

        var playerController = SourcePlayer.GetComponent<PlayerController>();
        Spell = playerController.GetPlayerActiveSpell();

        //if (Spell == null)
        //{
        //    Debug.LogError("I don't have a Spell :'(");
        //}

        var castSpeed = Spell.Attributes.Speed / 50f;
        if (castSpeed < 0.5)
        {
            castSpeed = 0.5f;
        }

        var spellRb = GetComponent<Rigidbody>();
        spellRb.AddForce(playerController.PlayerCamera.transform.forward * 20f * castSpeed, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            //Debug.Log("Collided with " + other.gameObject.name);

            if (other.gameObject.CompareTag("Spell"))
            {
                //Debug.Log("You hit another spell");
                return;
            }

            //Don't Destroy(). Need it alive for RPC calls
            gameObject.SetActive(false);

            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
            {
                DealDamage(Spell, gameObject, other.gameObject, other.ClosestPointOnBounds(gameObject.transform.position));
                return;
            }

            //Debug.Log("You hit something not damageable");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
