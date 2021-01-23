using Assets.Scripts.Attacks;
using Assets.Scripts.Crafting.Results;
using UnityEngine;

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

    //Damage
    private void OnTriggerEnter(Collider other)
    {
        DealDamage(Spell, other.gameObject, other.ClosestPointOnBounds(gameObject.transform.position));
        Destroy(gameObject);
    }

    //Impact
    private void OnCollisionEnter(Collision collision)
    {
        DealDamage(Spell, collision.gameObject, collision.GetContact(0).point);
        Destroy(gameObject);
    }

}
