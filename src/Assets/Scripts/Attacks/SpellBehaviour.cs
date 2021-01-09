using Assets.Scripts;
using Assets.Scripts.Attacks;
using UnityEngine;

public class SpellBehaviour : AttackBehaviourBase
{
    private void Awake()
    {
        var ttl = gameObject.AddComponent<TimeToLive>();
        ttl.GameObjectToDestroy = gameObject;
        ttl.AllowedTime = 3f;

        //todo: when is this false?
        var ignorePhysics = true;
        if (ignorePhysics)
        {
            gameObject.GetComponent<Collider>().isTrigger = true;
        }
    }

    //Damage
    private void OnTriggerEnter(Collider other)
    {
        //todo: check is damagable

        //var rb = GetComponent<Rigidbody>();
        //var negativeVelocity = rb.velocity * -1;

        //var cp = other.ClosestPoint(gameObject.transform.position);
        //var cp2 = other.ClosestPointOnBounds(gameObject.transform.position);

        //var direction = other.transform.position - transform.position;
        //float angle = Vector3.Angle(transform.forward, direction);

        //var centreOfHitObject = other.ClosestPoint(gameObject.transform.position);
        //var rect = other.GetComponent<RectTransform>();
        //var adjusted = centreOfHitObject; // + negativeVelocity; // new Vector3(0, 0, other.transform.localScale.z * -1);


        ////var worldPosSpell = transform.TransformPoint(transform.position);
        ////var worldPosCube = other.transform.TransformPoint(other.transform.position);

        //Vector3 startPoint = Origin;
        //bool foundBody = false;
        //RaycastHit hit;

        //foundBody = Physics.Raycast(startPoint, negativeVelocity, out hit);
        //startPoint = hit.point;

        //foundBody = Physics.Raycast(startPoint, negativeVelocity, out hit);
        //startPoint = hit.point;

        //foundBody = Physics.Raycast(startPoint, negativeVelocity, out hit);
        //startPoint = hit.point;

        //foundBody = Physics.Raycast(startPoint, negativeVelocity, out hit);
        //startPoint = hit.point;

        ////if (Physics.Raycast(transform.position, negativeVelocity, out hit))
        ////{
        ////    Debug.Log("Raycast: " + hit.point);
        ////}

        //var face = transform.TransformPoint(hit.point);

        //var cp3 = other.ClosestPoint(hit.point);
        //var cp4 = other.ClosestPointOnBounds(hit.point);

        Debug.Log($"Spell position: {gameObject.transform.position}");
        Debug.Log($"Other position: {other.transform.position}");
        Debug.Log($"Local closest: {other.ClosestPointOnBounds(gameObject.transform.position)}");
        Debug.Log($"World closest: {other.ClosestPointOnBounds(transform.TransformVector(gameObject.transform.position))}");

        var hitPos = other.ClosestPointOnBounds(gameObject.transform.position);

        //todo: calc damage
        ShowDamage(hitPos, "30");

        Destroy(gameObject);
    }

    //Impact
    private void OnCollisionEnter(Collision collision)
    {
        //collision.rigidbody.AddExplosionForce(400f, collision.transform.position, 5f);

        //todo: calc damage
        ShowDamage(collision.GetContact(0).point, "3");

        Destroy(gameObject);
    }

}
