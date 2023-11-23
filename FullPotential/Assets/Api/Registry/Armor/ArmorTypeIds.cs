using System;

namespace FullPotential.Api.Registry.Armor
{
    public static class ArmorTypeIds
    {
        private const string HelmId = "bd6f655b-6fed-42e5-8797-e9cb3f675696";
        public static readonly Guid Helm = new Guid(HelmId);

        private const string ChestId = "2419fcac-217e-48d8-9770-76c5ff27c9f8";
        public static readonly Guid Chest = new Guid(ChestId);
        
        private const string LegsId = "b4ec0616-8a9c-4052-a318-482a305bb263";
        public static readonly Guid Legs = new Guid(LegsId);
        
        private const string FeetId = "645b2a9b-02df-4fb7-bea2-9b7a2a9c620b";
        public static readonly Guid Feet = new Guid(FeetId);
    }
}
