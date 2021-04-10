using Assets.ApiScripts.Crafting;

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
                TypeName = _craftableType.TypeName;
            }
        }

        public string TypeName;
    }
}
