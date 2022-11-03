using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Items.Weapons
{
    //todo: split out data into the sub-classes the remove this attribute and make the class abstract
    [System.Serializable]
    public class WeaponItemBase : GearBase
    {
        //todo: move all below to RangedWeapon

        public int Ammo;

        public int GetAmmoMax()
        {
            var ammoCap = Attributes.IsAutomatic ? 100 : 20;
            var returnValue = (int)(Attributes.Efficiency / 100f * ammoCap);
            //Debug.Log("GetAmmoMax: " + returnValue);
            return returnValue;
        }

        public float GetReloadTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Recovery, 0.5f, 5);
            //Debug.Log("GetReloadTime: " + returnValue);
            return returnValue;
        }

        public float GetFireRate()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Speed, 0.05f, 0.5f);
            //Debug.Log("GetWeaponFireRate: " + returnValue);
            return returnValue;
        }

        public float GetRange()
        {
            var returnValue = Attributes.Range / 100f * 15 + 5;
            //Debug.Log("GetProjectileRange: " + returnValue);
            return returnValue;
        }
    }
}
