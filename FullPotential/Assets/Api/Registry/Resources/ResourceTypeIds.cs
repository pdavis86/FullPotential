using System;

namespace FullPotential.Api.Registry.Resources
{
    public static class ResourceTypeIds
    {
        public const string HealthId = "9b754336-6795-4ed7-a588-7ecaacae0de1";
        public static readonly Guid Health = new Guid(HealthId);

        public const string StaminaId = "1947b057-7b63-4737-8535-59ff613b2dd8";
        public static readonly Guid Stamina = new Guid(StaminaId);

        public const string ManaId = "378443ee-7942-4cd5-977d-818ee03333e9";
        public static readonly Guid Mana = new Guid(ManaId);

        public const string EnergyId = "89ec3ecf-badb-4e55-91b0-b288ca358010";
        public static readonly Guid Energy = new Guid(EnergyId);
    }
}
