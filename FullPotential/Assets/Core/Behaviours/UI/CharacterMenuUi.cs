using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.Ui
{
    public class CharacterMenuUi : MonoBehaviour
    {
        // ReSharper disable UnassignedField.Global
        public GameObject Equipment;
        public GameObject Crafting;
        // ReSharper enable UnassignedField.Global

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            Equipment.SetActive(false);
            Crafting.SetActive(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            OnTabClick(0);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void OnTabClick(int index)
        {
            switch (index)
            {
                case 0:
                    Crafting.SetActive(false);
                    Equipment.SetActive(true);
                    break;

                case 2:
                    Equipment.SetActive(false);
                    Crafting.SetActive(true);
                    break;
            }
        }

    }
}
