using MonoGame.Utilities;
using MoonSharp.Interpreter;

namespace Neurotrauma
{
    public partial class NeurotraumaInit : IAssemblyPlugin
    {
        // ---------------------------         Ydrec Shit         --------------------------- \\

        // These are automatically assigned by the plugin service after the Constructor is called
        public IConfigService ConfigService { get; set; }
        public IPluginManagementService PluginService { get; set; }
        public ILoggerService LoggerService { get; set; }
        public readonly ILuaScriptManagementService luaScriptManagementService = LuaCsSetup.Instance.LuaScriptManagementService;
        private Harmony harmony;

        // --------------------------- Local Variables/Classes --------------------------- \\
        private static readonly HumanUpdate HU = new HumanUpdate(); // Create our local Human Update class.

        // ---------------------------        Functions        --------------------------- \\
        // Called right after the constructor
        public void PreInitPatching()
        {
        }

        // When your plugin is loading, use this instead of the constructor for code relying on
        // the services above.
        // Put any code here that does not rely on other plugins.

        // No fucking clue what should go here for now tbh. - Lukako
        public void Initialize()
        {
            UserData.RegisterType(typeof(HF));

            if (HF.GameIsMultiplayer())
            {
                #if SERVER
                    HF.Print("Initializing for Multiplayer.");
                    InitializeServer();
                #endif
            }

            if (HF.GameIsSingleplayer())
            {
                // ServersideInit.cs
                HF.Print("Initializing for Singleplayer.");
                InitializeServer();
            }
        }

        // After all plugins have loaded
        // Put code that interacts with other plugins here.
        public void OnLoadCompleted()
        {
            // Shared Scripts
            NTConfigData.Register();
            NTConfig.LoadConfig();

            // Serverside code that ALSO runs in Singleplayer
            // Add functions in SharedSource/SharedInit.cs
            if (HF.GameIsMultiplayer())
            {
                #if SERVER
                    HF.Print("OnLoadCompleted for Multiplayer.");
                    OnLoadCompletedServerside();
                #endif
            }

            if (HF.GameIsSingleplayer())
            {
                // ServersideInit.cs
                HF.Print("OnLoadCompleted for Singleplayer.");
                OnLoadCompletedServerside();
            }

            // Clientside code
            // Add functions in ClientSource/ClientInit.cs
            #if CLIENT
                InitClientOnly();
            #endif

            // Serverside code that ONLY runs in Multiplayer
            // Add functions in ServerSource/ServerInit.cs
            #if SERVER
                InitServerOnly();
            #endif
        }

        public void Dispose()
        {
            #if SERVER
                DisposeServer();
            #endif
        }
    }
}
