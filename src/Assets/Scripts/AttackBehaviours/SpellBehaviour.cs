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
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            CmdDealDamage(Spell, gameObject, other.gameObject, other.ClosestPointOnBounds(gameObject.transform.position));
            
            //todo: maybe don't sync position. Use and RPC to spawn it in the world instead

            //Don't Destroy(). Need it alive for RPC calls
            gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
