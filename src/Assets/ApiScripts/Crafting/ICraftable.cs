namespace Assets.ApiScripts.Crafting
{
    public interface ICraftable
    {
        /// <summary>
        /// The name used to identify this type
        /// </summary>
        string TypeName { get; }


        //todo: requirements e.g. strength, speed, accuracy, 6 scrap or less
    }
}
