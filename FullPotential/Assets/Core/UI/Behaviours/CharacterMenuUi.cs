using UnityEngine;

namespace FullPotential.Core.Ui.Behaviours
{
    public class CharacterMenuUi : MonoBehaviour
    {
        public GameObject[] TabContents;
        public GameObject DarkOverlay;

        private void Awake()
        {
            foreach (var tabContent in TabContents)
            {
                tabContent.SetActive(false);
            }
        }

        private void OnEnable()
        {
            OnTabClick(0);
        }

        public void OnTabClick(int index)
        {
            for (var i = 0; i < TabContents.Length; i++)
            {
                TabContents[i].SetActive(i == index);
            }
        }

    }
}
