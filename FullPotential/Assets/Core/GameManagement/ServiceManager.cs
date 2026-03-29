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
            DependenciesContext.Dependencies.Register<Api.Gameplay.Combat.ICombatService, Gameplay.Combat.CombatService>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Crafting.IResultFactory, Gameplay.Crafting.ResultFactory>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Drawing.IDrawingService, Gameplay.Drawing.DrawingService>();
            DependenciesContext.Dependencies.Register<Api.Gameplay.Events.IEventManager, Gameplay.Events.EventManager>();
            DependenciesContext.Dependencies.Register<Api.Localization.ILocalizer, Localization.Localizer>();
            DependenciesContext.Dependencies.Register<Api.Modding.IModHelper, Modding.ModHelper>();
            DependenciesContext.Dependencies.Register<Api.Networking.IRpcService, Networking.RpcService>();
            DependenciesContext.Dependencies.Register<Api.Obsolete.Networking.IFragmentedMessageReconstructorFactory, Api.Obsolete.Networking.FragmentedMessageReconstructorFactory>();
            DependenciesContext.Dependencies.Register<Api.Data.ISettingsRepository, Persistence.Local.SettingsRepository>();
            DependenciesContext.Dependencies.Register<Api.Registry.ITypeRegistry, Registry.TypeRegistry>();
            DependenciesContext.Dependencies.Register<Api.Ui.Services.IUiAssistant, Api.Ui.Services.UiAssistant>();
            DependenciesContext.Dependencies.Register<Api.Unity.Services.IShaderUtilities, Unity.Services.ShaderUtilities>();
            DependenciesContext.Dependencies.Register<Api.Unity.Services.IUnityHelperUtilities, Unity.Services.UnityHelperUtilities>();

            var settingsRepository = DependenciesContext.Dependencies.GetService<Api.Data.ISettingsRepository>();

            var settings = settingsRepository.GetOrLoad();

            // todo: register the right kind
            if (string.IsNullOrWhiteSpace(settings.ManagementApiAddress))
            {
                DependenciesContext.Dependencies.Register<Api.Data.IInstanceManagement, Persistence.Local.InstanceManagement>();
                DependenciesContext.Dependencies.Register<Api.Data.IPlayerManagement, Persistence.Local.PlayerManagement>();
                DependenciesContext.Dependencies.Register<Api.Data.IUserManagement, Persistence.Local.UserManagement>();
            }
            else
            {
                DependenciesContext.Dependencies.Register<Api.Data.IInstanceManagement, Persistence.Https.InstanceManagement>();
                DependenciesContext.Dependencies.Register<Api.Data.IPlayerManagement, Persistence.Https.PlayerManagement>();
                DependenciesContext.Dependencies.Register<Api.Data.IUserManagement, Persistence.Https.UserManagement>();
            }
        }
    }
}
