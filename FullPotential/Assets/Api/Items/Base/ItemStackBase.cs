using FullPotential.Api.Localization;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Items.Base
{
    public abstract class ItemStackBase : ItemBase
    {
        public int Count
        {
            get => int.Parse(Properties[nameof(Count)]);
            set => Properties[nameof(Count)] = value.ToString();
        }

        public int MaxSize => ((IItemStackType)RegistryType).MaxStackSize;

        public override string GetName(ILocalizer localizer)
        {
            return $"{localizer.Translate(RegistryType)} ({Count})";
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full)
        {
            return levelOfDetail == LevelOfDetail.Full ? GetName(localizer) : null;
        }
    }
}
