using System.Collections;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Gameplay.Player
{
    public class HandStatus
    {
        public ItemBase EquippedItem { get; private set; }

        public string EquippedItemDescription { get; private set; }

        #region Weapons

        public Weapon EquippedWeapon { get; private set; }

        public bool IsReloading { get; set; }

        public IEnumerator RapidFireEnumerator { get; set; }

        #endregion

        #region Consumer

        public Consumer EquippedConsumer { get; private set; }

        public IEnumerator ChargeEnumerator { get; set; }

        public Consumer ActiveConsumer { get; set; }

        //public IEnumerator CooldownEnumerator { get; set; }

        #endregion

        public void SetEquippedItem(ItemBase item, string description)
        {
            if (ActiveConsumer != null)
            {
                ActiveConsumer.StopStoppables();
                ActiveConsumer = null;
            }

            EquippedItem = item;
            EquippedItemDescription = description;

            switch (item)
            {
                case Weapon weapon:
                    EquippedWeapon = weapon;
                    EquippedConsumer = null;
                    break;

                case Consumer consumer:
                    EquippedWeapon = null;
                    EquippedConsumer = consumer;
                    break;

                default:
                    EquippedWeapon = null;
                    EquippedConsumer = null;
                    break;
            }
        }

        public bool IsConsumingResource(ResourceType type)
        {
            return ActiveConsumer != null && EquippedConsumer.ResourceType == type;
        }
    }
}