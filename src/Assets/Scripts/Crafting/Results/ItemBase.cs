using System.Collections.Generic;

namespace Assets.Scripts.Crafting.Results
{
    [System.Serializable]
    public class ItemBase
    {
        //todo: consumables

        public string Id;
        public string Name;
        public Attributes Attributes;
        public List<string> Effects;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 101;
                hash = hash * 103 + Id.GetHashCode();
                hash = hash * 107 + Name.GetHashCode();
                hash = hash * 109 + Attributes.GetHashCode();
                hash = hash * 113 + string.Join(null, Effects).GetHashCode();
                return hash;
            }
        }

    }
}
