using UnityEngine;

namespace Assets.Scripts.Spells
{
    public class Damage : SpellBehaviourBase
    {
        private void Update()
        {
            UpdateTimeAlive(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            Physics.IgnoreCollision(other, gameObject.GetComponent<Collider>());

            //todo: do damage rather than shrinking the scale
            other.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);

            Destroy(gameObject);
        }
    }
}
