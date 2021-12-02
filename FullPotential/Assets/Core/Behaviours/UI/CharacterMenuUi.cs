using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Global

namespace FullPotential.Core.Behaviours.Ui
{
    public class CharacterMenuUi : MonoBehaviour
    {
        public GameObject Equipment;
        public GameObject Crafting;

        private void Awake()
        {
            Equipment.SetActive(false);
            Crafting.SetActive(false);
        }

        private void OnEnable()
        {
            OnTabClick(0);
        }

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
