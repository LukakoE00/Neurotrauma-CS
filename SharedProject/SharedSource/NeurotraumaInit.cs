namespace Neurotrauma
{
    public partial class NeurotraumaInit : IAssemblyPlugin
    {
        // These are automatically assigned by the plugin service after the Constructor is called
        public IConfigService ConfigService { get; set; }
        public IPluginManagementService PluginService { get; set; }
        public ILoggerService LoggerService { get; set; }



        private void DefineAllStats()
        {
            // I wanted to write every stats already defined in LuaNT however NTC looks like complete horsedong.
            // I will then leave it for someone to write them, but here is an example of how to register a stat:
            NTStat.RegisterStat("stasis", (character) =>
            {
                return HF.HasAffliction(character.Character, "stasis") ? 1 : 0;
            });
        }

        private void DefineAllAfflictions()
        {
            
        }


        // private Harmony harmony;

        public void Initialize()
        {

            DefineAllStats();

            // When your plugin is loading, use this instead of the constructor for code relying on
            // the services above.

            // Put any code here that does not rely on other plugins.
        }

        public void OnLoadCompleted()
        {
            /* Example of Harmony patching, if you need it. Remember to unpatch -Cookie
            harmony = new Harmony("neurotrauma");
            var original = AccessTools.Method(typeof(CharacterHealth), "ApplyAffliction", [typeof(Limb), typeof(Affliction), typeof(bool), typeof(bool), typeof(bool)]);
            harmony.Patch(original, prefix: new HarmonyMethod(typeof(PatchTest), nameof(PatchTest.Override_ApplyAffiction)));
            */

            // After all plugins have loaded
            // Put code that interacts with other plugins here.

            // So, we're bringing back the init.lua classic. This will run MP serverside AND singleplayer; but not clientside.
#if SERVER
            PrintNTInitInfo();
            #else
                if (GameMain.IsSingleplayer)
                    PrintNTInitInfo();
            #endif


        }

        public void PreInitPatching()
        {
            //Called right after the constructor
        }

        public void Dispose()
        {
            //harmony.UnpatchSelf();


            // Cleanup your plugin!
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
    }
}
