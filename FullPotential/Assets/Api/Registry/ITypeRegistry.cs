using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Items;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Registry
{
    //todo: Remove anything from the Registry namespace that is not registerable
    public interface ITypeRegistry
    {
        IEnumerable<T> GetRegisteredTypes<T>() where T : IRegisterableType;

        T GetRegisteredByTypeId<T>(string typeIdString) where T : IRegisterableType;

        IRegisterableType GetAnyRegisteredBySlotId(string typeIdString);

        void LoadAddessable<T>(string address, Action<T> action);

        UniTask LoadAddessableAsync<T>(string address, Action<T> action);

        IItemTypeMapping GetItemTypeMapping<T>() where T : IRegisterableType;

        IItemTypeMapping GetCraftableItemTypeMapping(Type craftableType);

        IRegisterableType GetAnyRegisteredTypeById(string typeIdString);

        Type GetCraftableTypeFromName(string craftableTypeName);
    }
}
