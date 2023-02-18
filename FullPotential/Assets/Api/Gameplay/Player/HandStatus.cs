using System.Collections;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Consumers;

namespace FullPotential.Api.Gameplay.Player
{
    public class HandStatus
    {
        public ItemBase EquippedItem{ get; private set; }

        public string EquippedItemDescription { get; private set; }

        #region Weapons

        public Weapon EquippedWeapon { get; private set; }

        public bool IsReloading { get; set; }

        public IEnumerator RapidFireEnumerator { get; set; }

        #endregion

        #region Consumer

        public Consumer EquippedConsumer { get; private set; }

        public IEnumerator ChargeEnumerator { get; set; }

        public IConsumerBehaviour ActiveConsumerBehaviour { get; set; }

        //public IEnumerator CooldownEnumerator { get; set; }

        #endregion

        public void SetEquippedItem(ItemBase item, string description)
        {
            if (ActiveConsumerBehaviour != null)
            {
                ActiveConsumerBehaviour.Stop();
                ActiveConsumerBehaviour = null;
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

        public bool IsConsumingMana()
        {
            return ActiveConsumerBehaviour != null && EquippedConsumer.ResourceConsumptionType == ResourceConsumptionType.Mana;
        }

        public bool IsConsumingEnergy()
        {
            return ActiveConsumerBehaviour != null && EquippedConsumer.ResourceConsumptionType == ResourceConsumptionType.Energy;
        }
    }
}