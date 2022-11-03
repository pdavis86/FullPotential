using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Api.Items.SpellsAndGadgets
{
    [System.Serializable]
    public abstract class SpellOrGadgetItemBase : ItemBase, IHasTargetingAndShape
    {
        public ResourceConsumptionType ResourceConsumptionType { get; protected set; }

        public int ChargePercentage { get; set; }

        private ITargeting _targeting;
        public ITargeting Targeting
        {
            get
            {
                return _targeting;
            }
            set
            {
                _targeting = value;
                TargetingTypeName = _targeting?.TypeName;
            }
        }

        public string TargetingTypeName { get; set; }

        private IShape _shape;
        public IShape Shape
        {
            get
            {
                return _shape;
            }
            set
            {
                _shape = value;
                ShapeTypeName = _shape?.TypeName;
            }
        }

        public string ShapeTypeName { get; set; }

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
