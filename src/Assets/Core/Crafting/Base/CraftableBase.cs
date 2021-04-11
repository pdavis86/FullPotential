using Assets.ApiScripts.Crafting;

// ReSharper disable ArrangeAccessorOwnerBody

namespace Assets.Core.Crafting.Base
{
    public class CraftableBase : ItemBase
    {
        private ICraftable _craftableType;
        public ICraftable CraftableType
        {
            get
            {
                return _craftableType;
            }
            set
            {
                _craftableType = value;
                TypeId = _craftableType.TypeId.ToString();
            }
        }

        public string TypeId;
    }
}
