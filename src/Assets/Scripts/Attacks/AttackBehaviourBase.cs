using TMPro;
using UnityEngine;

namespace Assets.Scripts.Attacks
{
    public abstract class AttackBehaviourBase : MonoBehaviour
    {
        public GameObject Player;
        public Camera PlayerCamera;
        public GameObject HitTextUiPrefab;
        public GameObject UiCanvas;

        internal void ShowDamage(Vector3 position, string damage)
        {
            var hit = Instantiate(HitTextUiPrefab);
            hit.transform.SetParent(UiCanvas.transform, false);
            hit.gameObject.SetActive(true);

            var tmp = hit.GetComponent<TextMeshProUGUI>();
            tmp.text = damage.ToString();

            var stwp = hit.GetComponent<StickToWorldPosition>();
            stwp.PlayerCamera = PlayerCamera;
            stwp.WorldPosition = position;

            //todo:
            //var ttl = hit.gameObject.AddComponent<TimeToLive>();
            //ttl.GameObjectToDestroy = hit.gameObject;
            //ttl.AllowedTime = 1f;




            //var damageIndicator = Instantiate(new TextMeshPro(), position, rotation);
            //damageIndicator.text = damage;

            //var hit = Instantiate(HitTextPrefab, position, Quaternion.identity); //new Vector3(0,0,6)

            //var rect = hit.GetComponent<RectTransform>();
            //var adjusted = position + new Vector3(rect.rect.width / -2, rect.rect.height / -2);

            //Debug.Log("TextAdjusted: " + adjusted);

            ////Rotate hit to show facing the player
            //hit.transform.LookAt(2 * transform.position - Caster.transform.position);

            //hit.transform.position = adjusted;

            //hit.gameObject.SetActive(true);

            ////todo: do not use Find where possible. If you have to, put it in Awake()
            //var canvas = GameObject.Find("Canvas");
            //var hit = Instantiate(canvas.transform.Find("Attacks"), transform.position, Quaternion.identity);
            //hit.transform.SetParent(canvas.transform, false);
            //hit.transform.position = transform.position;
            //hit.gameObject.SetActive(true);

            //var hitText = hit.gameObject.AddComponent<Text>();
            //hitText.color = Color.red;
            //hitText.text = damage;
        }

    }
}
