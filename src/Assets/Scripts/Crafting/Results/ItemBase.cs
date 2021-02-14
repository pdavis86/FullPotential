using System;
using System.Collections.Generic;

namespace Assets.Scripts.Crafting.Results
{
    [System.Serializable]
    public class ItemBase
    {
        public Guid Id;
        public string Name;
        public Attributes Attributes;
        public List<string> Effects;
    }
}
