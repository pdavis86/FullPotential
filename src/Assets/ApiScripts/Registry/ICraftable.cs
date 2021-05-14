namespace Assets.ApiScripts.Registry
{
    public interface ICraftable : IRegisterable
    {
        /// <summary>
        /// The address of the prefab to load for this weapon
        /// </summary>
        string PrefabAddress { get; }
    }
}
