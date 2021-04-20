﻿namespace Assets.ApiScripts.Crafting
{
    public interface ILoot : IRegisterable
    {
        public enum LootCategory
        {
            Technology,
            Magic
        }

        /// <summary>
        /// The category of loot
        /// </summary>
        LootCategory Category { get; }
    }
}