using System;
using System.Collections.Generic;
using FullPotential.Api.Items.Base;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Registry
{
    //todo: zzz v0.6 - Remove anything from the Registry namespace that is no registerable
    public interface ITypeRegistry
    {
        IEnumerable<T> GetRegisteredTypes<T>() where T : IRegisterable;

        T GetRegisteredByTypeId<T>(string typeIdString) where T : IRegisterable;

        IRegisterable GetAnyRegisteredBySlotId(string typeIdString);

        IRegisterable GetRegistryTypeForItem(ItemBase item);

        void LoadAddessable<T>(string address, Action<T> action);
    }
}
