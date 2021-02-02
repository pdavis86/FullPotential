using Assets.Scripts.Crafting.Results;
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class SpellBehaviour : AttackBehaviourBase
{
    public Spell Spell;

    private void Awake()
    {
        var ttl = gameObject.AddComponent<TimeToLive>();
        ttl.GameObjectToDestroy = gameObject;
        ttl.AllowedTime = 3f;

        //if (ignorePhysics)
        //{
        gameObject.GetComponent<Collider>().isTrigger = true;
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            DealDamage(Spell, other.gameObject, other.ClosestPointOnBounds(gameObject.transform.position));
            Destroy(gameObject);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    try
    //    {
    //        DealDamage(Spell, collision.gameObject, collision.GetContact(0).point);
    //        Destroy(gameObject);
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError(ex);
    //    }
    //}

}
