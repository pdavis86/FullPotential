using FullPotential.Api.Ioc;

namespace FullPotential.Core.GameManagement
{
    public static class ServiceManager
    {
        public static void RegisterServices()
        {
            //Scoped
            DependenciesContext.Dependencies.Register<Api.Scenes.ISceneService, Environment.SceneService>(true);

            //Singleton
            DependenciesContext.Dependencies.Register<Inventory.IInventoryDataService, Inventory.InventoryDataService>();
            DependenciesContext.Dependencies.Register<Api.Persistence.IUserRepository, Persistence.UserRepository>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Crafting.IResultFactory, Gameplay.Crafting.ResultFactory>();
            DependenciesContext.Dependencies.Register<Api.Localization.ILocalizer, Localization.Localizer>();
            DependenciesContext.Dependencies.Register<Api.Registry.ITypeRegistry, Registry.TypeRegistry>();
            DependenciesContext.Dependencies.Register<Api.Networking.IRpcService, Networking.RpcService>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Combat.ICombatService, Gameplay.Combat.CombatService>();
            DependenciesContext.Dependencies.Register<Api.Modding.IModHelper, Modding.ModHelper>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Drawing.IDrawingService, Gameplay.Drawing.DrawingService>();
            DependenciesContext.Dependencies.Register<Api.Ui.Services.IUiAssistant, Api.Ui.Services.UiAssistant>();
            DependenciesContext.Dependencies.Register<Api.Persistence.ISettingsRepository, Persistence.SettingsRepository>();
            DependenciesContext.Dependencies.Register<Api.Unity.Services.IUnityHelperUtilities, Unity.Services.UnityHelperUtilities>();
            DependenciesContext.Dependencies.Register<Api.Unity.Services.IShaderUtilities, Unity.Services.ShaderUtilities>();
        }
    }
}
