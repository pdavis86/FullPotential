using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Drawing;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Persistence;
using FullPotential.Api.Spawning;
using FullPotential.Api.Ui.Services;
using FullPotential.Api.Utilities;
using FullPotential.Core.GameManagement.Inventory;
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Localization;
using FullPotential.Core.Networking;
using FullPotential.Core.Persistence;
using FullPotential.Core.Registry;
using FullPotential.Core.Spawning;
using FullPotential.Core.Utilities;

namespace FullPotential.Core.GameManagement
{
    public static class ServiceManager
    {
        public static void RegisterServices()
        {
            DependenciesContext.Dependencies.Register<IUserRepository, UserRepository>();
            DependenciesContext.Dependencies.Register<IResultFactory, ResultFactory>();
            DependenciesContext.Dependencies.Register<IInventoryDataService, InventoryDataService>();
            DependenciesContext.Dependencies.Register<IValueCalculator, ValueCalculator>();
            DependenciesContext.Dependencies.Register<ILocalizer, Localizer>();
            DependenciesContext.Dependencies.Register<ITypeRegistry, TypeRegistry>();
            DependenciesContext.Dependencies.Register<ISpawnService, SpawnService>(true);
            DependenciesContext.Dependencies.Register<IRpcService, RpcService>();
            DependenciesContext.Dependencies.Register<IEffectService, EffectService>();
            DependenciesContext.Dependencies.Register<IModHelper, ModHelper>();
            DependenciesContext.Dependencies.Register<IDrawingService, DrawingService>();
            DependenciesContext.Dependencies.Register<IUiAssistant, UiAssistant>();
            DependenciesContext.Dependencies.Register<ISettingsRepository, SettingsRepository>();
        }
    }
}
