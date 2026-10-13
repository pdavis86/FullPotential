using System.Collections.Generic;

using FullPotential.Api.Gameplay;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Items
{
    public interface IItem
    {
        string Id { get; }

        string CharacterId { get; }

        IRegisterableType RegistryType { get; }

        string Name { get; }

        Dictionary<string, string> Attributes { get; }

        Dictionary<string, string> Properties { get; }

        bool IsDirty { get; set; }

        string GetName(ILocalizer localizer);

        string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full);

        IEnumerable<string> ValidateCraftingAttempt(List<ItemBase> components);

        string GetTextForUiComponent(string id, IPlayerFighter playerFighter);
    }
}
