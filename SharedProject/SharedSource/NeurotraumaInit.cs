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
        //Called right after the constructor
        public void PreInitPatching()
        {
            
            

            

        }

        // When your plugin is loading, use this instead of the constructor for code relying on
        // the services above.

        // Put any code here that does not rely on other plugins.
        public void Initialize()
        {
#if SERVER
            InitializeServer();
#endif
            UserData.RegisterType(typeof(HF));
        }

        // After all plugins have loaded
        // Put code that interacts with other plugins here.
        public void OnLoadCompleted()
        {
            // So, we're bringing back the init.lua classic. This will run MP serverside AND singleplayer; but not clientside.
            // It in fact does not run in singleplayer -Cookie

#if SERVER
            HF.Print("Running Server");
            OnLoadCompletedServer();
            InitLuaHooks();
            PrintNTInitInfo();
#else
            if (GameMain.IsSingleplayer)
            {
                HF.Print("Running Client");
                InitLuaHooks();
                PrintNTInitInfo();
            }
#endif
        }

        // Cleanup your plugin!
        public void Dispose()
        {
            #if SERVER
                DisposeServer();
            #endif

            // harmony.UnpatchSelf();

            
        }

        // This does what Init.lua used to do, using NTInfo.cs to hold relevant information.
        private void PrintNTInitInfo()
        {
            // New string with the first line of the init print; the $ allows the string to interpolate
            string consolePrint = $"\n\n/// Running Neurotrauma V {NTInfo.Version} ///\n";
            // Repeat the dash until the line is just as long as the line above and add 4 more to make it stand out
            consolePrint += new string('-', consolePrint.Length + 4);

            // Now check for addons and react accordingly
            bool hasAddons = NTInfo.RegisteredAddons.Count > 0;

            foreach (NTAddon addon in NTInfo.RegisteredAddons)
            {
                consolePrint += $"\n+ {addon.Name} V {addon.Version}";

                if (NTInfo.VersionNum < addon.MinNTVersionNum)
                {
                    consolePrint += $"\n-- WARNING! Neurotrauma version {addon.MinNTVersion} or higher required!";
                }
            }

            consolePrint += "\n";
            if (!hasAddons) consolePrint += "- Not running any expansions\n";

            LoggerService.Log(consolePrint);
        }

        public static void InitLuaHooks() // Based off the Traumatic Presence mod by Lenny!
        {
        HF.Print("Adding Lua Hooks");
            // This function stores our hooks we will be using for NT 2.
#pragma warning disable CS0618 // Type or member is obsolete
            LuaCsSetup.Instance.Hook.Add("think", "NTCS.ThinkUpdate", (params object[] _) => // The Hook details
            { // Start of our Function
                double DeltaTime = 1;
                HU.OnUpdate(DeltaTime);
                return null;
            }); // End of our Function
#pragma warning restore CS0618 // Type or member is obsolete
        }
        
    }
}
