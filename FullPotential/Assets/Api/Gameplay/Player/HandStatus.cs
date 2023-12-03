using System.Collections;

namespace FullPotential.Api.Gameplay.Player
{
    public class HandStatus
    {
        public string EquippedItemId { get; set; }

        #region Weapon

        public bool IsReloading { get; set; }

        public IEnumerator RapidFireEnumerator { get; set; }

        #endregion

        #region Consumer

        public IEnumerator ChargeEnumerator { get; set; }

        public string ActiveConsumerId { get; set; }

        //todo: zzz v0.4.1 - cooldown instead of charge for some consumers?
        //public IEnumerator CooldownEnumerator { get; set; }

        #endregion
    }
}