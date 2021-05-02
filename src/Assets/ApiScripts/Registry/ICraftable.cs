namespace Assets.ApiScripts.Registry
{
    public interface ICraftable : IRegisterable
    {
        //todo: requirements e.g. strength, speed, accuracy, 6 scrap or less

        /// <summary>
        /// The address of the prefab to load for this weapon
        /// </summary>
        string PrefabAddress { get; }
    }
}
