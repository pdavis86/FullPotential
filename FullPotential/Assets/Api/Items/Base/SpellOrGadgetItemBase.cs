using FullPotential.Api.Gameplay.Items;

namespace FullPotential.Api.Items.Base
{
    [System.Serializable]
    public abstract class SpellOrGadgetItemBase : ItemWithTargetingAndShapeBase
    {
        public const string AliasSegmentSog = "sog";

        public ResourceConsumptionType ResourceConsumptionType { get; protected set; }

        public int ChargePercentage { get; set; }


        public int GetResourceCost()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Efficiency, 5, 50);
            //Debug.Log("GetResourceCost: " + returnValue);
            return (int)returnValue;
        }

        public float GetContinuousRange()
        {
            var returnValue = Attributes.Range / 100f * 10;
            //Debug.Log("GetContinuousRange: " + returnValue);
            return returnValue;
        }

        public float GetProjectileSpeed()
        {
            var castSpeed = Attributes.Speed / 50f;
            var returnValue = castSpeed < 0.5
                ? 0.5f
                : castSpeed;
            //Debug.Log("GetProjectileSpeed: " + returnValue);
            return returnValue;
        }

        public float GetChargeTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Speed, 0, 2);
            //Debug.Log("GetSogChargeTime: " + returnValue);
            return returnValue;
        }

        public float GetCooldownTime()
        {
            var returnValue = GetValueInRangeHighLow(Attributes.Recovery, 0, 2);
            //Debug.Log("GetSogChargeTime: " + returnValue);
            return returnValue;
        }

    }
}
