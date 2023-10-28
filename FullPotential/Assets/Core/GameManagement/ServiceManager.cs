using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Drawing;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Modding;
using FullPotential.Api.Persistence;
using FullPotential.Api.Scenes;
using FullPotential.Api.Ui.Services;
using FullPotential.Core.Environment;
using FullPotential.Core.GameManagement.Inventory;
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Gameplay.Drawing;
using FullPotential.Core.Localization;
using FullPotential.Core.Modding;
using FullPotential.Core.Networking;
using FullPotential.Core.Persistence;
using FullPotential.Core.Registry;

namespace FullPotential.Core.GameManagement
{
    public static class ServiceManager
    {
        public static void RegisterServices()
        {
            //Scoped
            DependenciesContext.Dependencies.Register<ISceneService, SceneService>(true);

            //Singleton
            DependenciesContext.Dependencies.Register<IUserRepository, UserRepository>();
            DependenciesContext.Dependencies.Register<IResultFactory, ResultFactory>();
            DependenciesContext.Dependencies.Register<IInventoryDataService, InventoryDataService>();
            DependenciesContext.Dependencies.Register<ILocalizer, Localizer>();
            DependenciesContext.Dependencies.Register<ITypeRegistry, TypeRegistry>();
            DependenciesContext.Dependencies.Register<IRpcService, RpcService>();
            DependenciesContext.Dependencies.Register<ICombatService, CombatService>();
            DependenciesContext.Dependencies.Register<IModHelper, ModHelper>();
            DependenciesContext.Dependencies.Register<IDrawingService, DrawingService>();
            DependenciesContext.Dependencies.Register<IUiAssistant, UiAssistant>();
            DependenciesContext.Dependencies.Register<ISettingsRepository, SettingsRepository>();
        }
    }
}
