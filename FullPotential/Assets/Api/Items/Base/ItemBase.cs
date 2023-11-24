using System;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Items.Base
{
    [Serializable]
    public abstract class ItemBase
    {
        public string Id;
        public string RegistryTypeId;
        public string Name;

        private IRegisterable _registryType;
        public IRegisterable RegistryType
        {
            get => _registryType;
            set
            {
                _registryType = value;
                RegistryTypeId = _registryType.TypeId.ToString();
            }
        }

        public virtual string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            return Name;
        }
    }
}
