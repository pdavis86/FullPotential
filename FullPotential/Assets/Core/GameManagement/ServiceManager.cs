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
            DependenciesContext.Dependencies.Register<Api.GameManagement.IGameManager>(GameManager.Instance);
            DependenciesContext.Dependencies.Register<Api.Gameplay.Combat.ICombatService, Gameplay.Combat.CombatService>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Crafting.IResultFactory, Gameplay.Crafting.ResultFactory>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Drawing.IDrawingService, Gameplay.Drawing.DrawingService>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Events.IEventBus, Gameplay.Events.EventBus>();
            DependenciesContext.Dependencies.Register<Api.Localization.ILocalizer, Localization.Localizer>();
            DependenciesContext.Dependencies.Register<Api.Networking.IRpcService, Networking.RpcService>();
            DependenciesContext.Dependencies.Register<Api.Data.ISettingsRepository, Persistence.Local.SettingsRepository>();
            DependenciesContext.Dependencies.Register<Api.Registry.ITypeRegistry, Registry.TypeRegistry>();
            DependenciesContext.Dependencies.Register<Api.Ui.IUiAssistant, Ui.UiAssistant>();
            DependenciesContext.Dependencies.Register<Api.Unity.IShaderUtilities, Unity.ShaderUtilities>();
            DependenciesContext.Dependencies.Register<Api.Unity.IUnityHelperUtilities, Unity.UnityHelperUtilities>();
            DependenciesContext.Dependencies.Register<Api.Data.ISaveManager, Persistence.SaveManager>();

            var settingsRepository = DependenciesContext.Dependencies.GetService<Api.Data.ISettingsRepository>();

            var settings = settingsRepository.Get();

            // todo: uncomment dependencies
            //if (string.IsNullOrWhiteSpace(settings.ManagementApiAddress))
            //{
            //    DependenciesContext.Dependencies.Register<Api.Data.IUserManagement, Persistence.Local.UserManagement>();
            DependenciesContext.Dependencies.Register<Api.Data.IDataLoader, Persistence.Local.DataLoader>();
            //    DependenciesContext.Dependencies.Register<Api.Data.IDataSaver, Persistence.Local.DataSaver>();
            //}
            //else
            //{
            DependenciesContext.Dependencies.Register<Api.Data.IUserManagement, Persistence.Https.UserManagement>();
            //    DependenciesContext.Dependencies.Register<Api.Data.IDataLoader, Persistence.Https.DataLoader>();
            DependenciesContext.Dependencies.Register<Api.Data.IDataSaver, Persistence.Https.DataSaver>();
            //}
        }
    }
}
