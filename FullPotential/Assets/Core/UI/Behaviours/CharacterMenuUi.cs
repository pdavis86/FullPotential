using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Ui.Behaviours
{
    public class CharacterMenuUi : MonoBehaviour
    {
        // ReSharper disable UnassignedField.Global
        // ReSharper disable MemberCanBePrivate.Global
        public GameObject[] TabContents;
        public GameObject DarkOverlay;
        // ReSharper restore UnassignedField.Global
        // ReSharper restore MemberCanBePrivate.Global

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            foreach (var tabContent in TabContents)
            {
                tabContent.SetActive(false);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            OnTabClick(0);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void OnTabClick(int index)
        {
            for (var i = 0; i < TabContents.Length; i++)
            {
                TabContents[i].SetActive(i == index);
            }
        }

    }
}
