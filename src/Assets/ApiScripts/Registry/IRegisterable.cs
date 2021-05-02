using System;

namespace Assets.ApiScripts.Registry
{
    public interface IRegisterable
    {
        /// <summary>
        /// The ID for this type of registerable
        /// </summary>
        Guid TypeId { get; }

        /// <summary>
        /// The friendly name for this type of registerable
        /// </summary>
        string TypeName { get; }
    }
}
