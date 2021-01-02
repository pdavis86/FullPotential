using Assets.Scripts.Spells;
using UnityEngine;

namespace Assets.Scripts.Spells
{
    public class Impact : SpellBehaviourBase
    {
        private void Update()
        {
            UpdateTimeAlive(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            collision.rigidbody.AddExplosionForce(400f, collision.transform.position, 5f);
            Destroy(gameObject);
        }
    }
}
