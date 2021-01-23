using Assets.Scripts.Crafting.Results;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Attacks
{
    public abstract class AttackBehaviourBase : MonoBehaviour
    {
        public GameObject Player;
        public Camera PlayerCamera;
        public GameObject HitTextUiPrefab;
        public Transform DamageNumbersParent;

        internal void DealDamage(CraftableBase source, GameObject target, Vector3 position)
        {
            //todo: check target is damagable

            //todo: calc damage
            var damageDealt = 3;

            Debug.Log($"Source '{source.Name}' attacked target '{target.name}' for {damageDealt} damage");

            ShowDamage(position, damageDealt.ToString());
        }

        private void ShowDamage(Vector3 position, string damage)
        {
            var hit = Instantiate(HitTextUiPrefab);
            hit.transform.SetParent(DamageNumbersParent, false);
            hit.gameObject.SetActive(true);

            var tmp = hit.GetComponent<TextMeshProUGUI>();
            tmp.text = damage.ToString();

            var stwp = hit.GetComponent<StickToWorldPosition>();
            stwp.PlayerCamera = PlayerCamera;
            stwp.WorldPosition = position;

            var ttl = hit.gameObject.AddComponent<TimeToLive>();
            ttl.GameObjectToDestroy = hit.gameObject;
            ttl.AllowedTime = 1f;
        }

    }
}
