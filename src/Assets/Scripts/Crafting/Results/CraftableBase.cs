﻿using System.Collections.Generic;

namespace Assets.Scripts.Crafting.Results
{
    public class CraftableBase
    {
        public string Name { get; set; }
        public Attributes Attributes { get; set; }
        public List<string> Effects { get; set; }
    }
}