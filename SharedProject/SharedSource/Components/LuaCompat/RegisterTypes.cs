using MoonSharp.Interpreter;

namespace Neurotrauma
{
    public static class LuaCompat
    {
        // Register commonly used types so Lua can interact with them.
        public static void RegisterTypes()
        {
            UserData.RegisterType(typeof(Initialization));
            UserData.RegisterType(typeof(NTInfo));
            UserData.RegisterType(typeof(NTAddon));

            // Common
            UserData.RegisterType(typeof(HF));
            UserData.RegisterType(typeof(NTC));

            // Components
            UserData.RegisterType(typeof(NTBloodTypes));
            UserData.RegisterType(typeof(NTAfflictions));
            UserData.RegisterType(typeof(NTAfflictions.AfflictionPriority));
            UserData.RegisterType(typeof(NTAfflictions.NTAfflictionPrefabBuilder));
            UserData.RegisterType(typeof(NTAfflictions.NTAfflictionsLoader));
            UserData.RegisterType(typeof(NTAfflictions.NTAfflictionPrefab));
            UserData.RegisterType(typeof(NTHuman));
            UserData.RegisterType(typeof(NTHuman.CharacterTags));
            UserData.RegisterType(typeof(NTStats));
            UserData.RegisterType(typeof(NTStats.NTStat));
            UserData.RegisterType(typeof(NTStats.NTStatBool));
            UserData.RegisterType(typeof(NTStats.NTStatFloat));
            UserData.RegisterType(typeof(NTStats.NTStatLoader));
            UserData.RegisterType(typeof(NTItems));
            UserData.RegisterType(typeof(NTItemsData));
            UserData.RegisterType(typeof(NTItems.ItemsAfflictionInfos));
            UserData.RegisterType(typeof(NTItems.ItemUpdateFunctionInfos));
            UserData.RegisterType(typeof(NTItems.NTItemFunctionLoader));

            // Config
            UserData.RegisterType(typeof(NTConfig));
            UserData.RegisterType(typeof(NTConfigData));
            UserData.RegisterType(typeof(ConfigExpansion));
            UserData.RegisterType(typeof(ConfigEntry));
            UserData.RegisterType(typeof(ConfigEntryType));

            // HarmonyPatches
            UserData.RegisterType(typeof(SpeakAboutIssuesPatch));
            UserData.RegisterType(typeof(OnDamaged));

            // MiscellaneousShared
            UserData.RegisterType(typeof(NTHumanUpdate));
        }
    }
}