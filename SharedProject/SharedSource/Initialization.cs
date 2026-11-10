using Barotrauma;
using Barotrauma.Plugins;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Barotrauma.LuaCs.Events;
using MoonSharp.Interpreter;


namespace Neurotrauma;

public partial class Initialization : IAssemblyPlugin
{
    public IConfigService ConfigService { get; set; }
    public IEventService EventService { get; set; }
    public IPluginManagementService PluginService { get; set; }
    public ILoggerService LoggerService { get; set; }
    public ILuaScriptManagementService luaScriptManagementService = LuaCsSetup.Instance.LuaScriptManagementService;

    public static NTAfflictions.NTAfflictionsLoader NTAfflLoader = new NTAfflictions.NTAfflictionsLoader(NTInfo.Name);
    public static NTItems.NTItemFunctionLoader NTItemsLoader = new NTItems.NTItemFunctionLoader(NTInfo.Name);
    public static NTStats.NTStatLoader NTStatsLoader = new NTStats.NTStatLoader(NTInfo.Name);

    public static ContentPackage NeurotraumaContentPackage;
    public static string NeurotraumaModDir;

    private Harmony? harmony;

    public void Initialize()
    {
        PluginService.TryGetPackageForPlugin<Initialization>(out NeurotraumaContentPackage);
        NeurotraumaModDir = Path.GetDirectoryName(NeurotraumaContentPackage.Path.ToString());

        harmony = new Harmony("Neurotrauma");
        harmony.PatchAll();

        if (HF.GameIsMultiplayer())
        {
#if SERVER
            InitializeShared();
#endif
        }

        if (HF.GameIsSingleplayer())
        {
            InitializeShared();
        }
    }

    // Initialize Shared functions.
    public void InitializeShared()
    {
        NTAfflictionsToAdd.AddAfflictions();
        NTStats.DefineAllStats();
        NTItemsData.DefineAllItems();
    }

    public void PreInitPatching()
    {
    }

    // Unload certain parts of the mod.
    public void Dispose()
    {
        harmony?.UnpatchSelf();
        DisposeClient();
    }

    partial void DisposeClient();
    partial void DisposeServer();

    public void OnLoadCompleted()
    {
        // Shared Scripts
        NTConfigData.Register();
        NTConfig.LoadConfig();

        // Serverside code that ALSO runs in Singleplayer
        if (HF.GameIsMultiplayer())
        {
#if SERVER
            OnLoadCompletedShared();
#endif
        }

        if (HF.GameIsSingleplayer())
        {
            OnLoadCompletedShared();
        }

        // Client / Server specific code.
        InitProjectSpecific();
    }

    public partial void InitProjectSpecific();

    public void OnLoadCompletedShared()
    {
        // Initialize all Neurotrauma hooks.
        Hooks.Register();
        NTInfo.PrintNTInitInfo();
        DynamicItems.InitDynamicItems(); // Add the DynamicItems hooks
        OnDamaged.InitializeOnDamagedMethods(); // Add OnDamaged patches
        NTItemsData.EnsureWorkingItems(); // Runs the FixItems hooks

        //BotTreatment.InitBotTreatment(); // Bot Treatment code
    }
}