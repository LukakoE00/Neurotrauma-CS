using Barotrauma.LuaCs.Data;
using Barotrauma.Networking;
using MoonSharp.Interpreter;
using static Neurotrauma.NTVariants;

namespace Neurotrauma;

// This takes the place of the init.lua information and can be used during Initialization (see NeurotraumaInit)
// Addons should add their own file like this which then gets used to do the old print-in-console thing

/// <summary>
/// Stores the ID's of NT variants.
/// </summary>
public struct NTVariants
{
    readonly public static string NTOld = "3190189044";
    readonly public static string NTLegacy = "3705482890";
    readonly public static string NTDev = "3439141713";
    readonly public static string NTLite = "3369418643";
    readonly public static string NTAi = "3741432181";
    readonly public static string NTOptimized = "3622155470";
    readonly public static string NTRussianFix = "3531774267";
}

public struct NTLuaAddonIDs
{
    public static string NTSP = "3478084070";
}


/// <summary>
/// Stores information about the current version of NT C# and the currently used Addons.
/// </summary>
public static class NTInfo
{
    public const string Name = "Neurotrauma C#";
    public const string Version = "A2.0.0";
    public const int VersionNum = 02000000;

    public static bool NTSPEnabled = false;

    public static bool ShowIncompatibleModsPopup = false;

    // Make a new list (like a table! but not!) that only holds NTAddon objects.
    // 'get' means we can read the list, but not replace it and 'new' means it get's created on loading.
    public static Dictionary<string,NTAddon> RegisteredAddons { get; } = new();
    public static Dictionary<string, Table> LuaRegisteredAddons = new();

    // This is the NTC.RegisterExpansion function from NTCompat.
    // NTAddon (defined below!) is an object, or like a 'blueprint' from which other Addon objects are made!
    // Each one has some things it needs to function, which will get passed along.
    // Object Oriented Coding!!!!!!

    public static void RegisterAddon(NTAddon addon)
    {
        RegisteredAddons[addon.Name] = addon;
    }
    public static void RegisterAddon(Table addon)
    {
        LuaRegisteredAddons[addon.Get("Name").String] = addon;
    }

    public static void PrintNTInitInfo(ImmutableArray<ILuaScriptResourceInfo> executionOrder, bool enableSandbox)
    {
        LuaCsSetup.Instance.Timer.Wait((params object[] _) => {
            // New string with the first line of the init print; the $ allows the string to interpolate
            string consolePrint = $"\n\n/// Running Neurotrauma V {NTInfo.Version} ///\n";
            // Repeat the dash until the line is just as long as the line above and add 4 more to make it stand out
            consolePrint += new string('-', consolePrint.Length + 4);

            // Now check for addons and react accordingly
            bool hasCSAddons = RegisteredAddons.Count > 0;

            bool hasLuaAddons = LuaRegisteredAddons.Count() > 0;

            consolePrint += "\n";

            if (hasCSAddons) consolePrint += "- C# Addons:";
            else consolePrint += "- Not running any C# Addons";
            foreach (KeyValuePair<string, NTAddon> kvp in RegisteredAddons)
            {
                NTAddon addon = kvp.Value;
                consolePrint += $"\n+ {addon.Name} V {addon.Version}";

                if (VersionNum < addon.MinNTVersionNum)
                {
                    consolePrint += $"\n-- WARNING! Neurotrauma version {addon.MinNTVersion} or higher required!";
                }
            }

            consolePrint += "\n";

            if (hasLuaAddons) consolePrint += "\n- Lua Addons:";
            else consolePrint += "\n- Not running any Lua Addons\n";
            foreach (KeyValuePair< string, Table > kvp in LuaRegisteredAddons)
            {
                Table addon = kvp.Value;
                consolePrint += $"\n+ {addon.Get("Name").String} V {addon.Get("Version").String}";

                if (VersionNum < addon.Get("MinNTVersionNum").Number)
                {
                    consolePrint += $"\n-- WARNING! Neurotrauma version {addon.Get("MinNTVersion").Number} or higher required!";
                }
            }

            consolePrint += "\n";

            // Checking for incompatible mods

            List<String> incompatibleMods = [NTLegacy, NTDev, NTOld, NTLite, NTAi, NTOptimized, NTRussianFix];
            List<string> WorkingAddons = ["Eyes","Thermal","Cybernetics","Nanite Integration","Grafting","Lobotomy","Pharmacy","NT Surgery Plus"]; // Addons we can be sure that work. (Presuming you use the patches)
            string detectedMods = "";
            string warnedMods = "";

            foreach (var item in ContentPackageManager.EnabledPackages.All)
            {

                if (item.UgcId.value != null)
                {
                    if (incompatibleMods.Contains(item.UgcId.value.ToString()))
                    {
                        detectedMods += " - " + item.Name + "\n";
                    }

                    if (item.UgcId.value.ToString() == NTLuaAddonIDs.NTSP)
                    {
                        NTSPEnabled = true;
                    }
                }
            }

            foreach (KeyValuePair<string, Table> kvp in LuaRegisteredAddons)
            {
                Table addon = kvp.Value;
                if (!WorkingAddons.Contains(addon.Get("Name").String))
                {
                    warnedMods += " - NT " + addon.Get("Name").String + "\n";
                }
            }

            if (NTSPEnabled) consolePrint += "\nNT Surgery Plus Enabled!\n";

            HF.Print(consolePrint);

            if (detectedMods != "")
            {
                HF.PrintError("Incompatible mods detected! This will cause many errors and you should disable them before playing!\n" + detectedMods);

                // Works for MP
                IReadOnlyList<Client> clients = GameMain.NetworkMember?.ConnectedClients;
                if (clients != null)
                {
                    foreach (Client client in clients)
                    {
                        HF.SendTextBox("Incompatible mods detected!", "Incompatible mods detected! This will cause many errors and you should disable them before playing!\n" + detectedMods, client);
                    }
                } else
                {
                    // TODO: Works for solo
#if CLIENT
                    
#endif
                }

            }

            if (warnedMods != "")
            {
                HF.PrintWarning("Possibly incompatible addons detected! These may causes issues when using NT C#!\n" + warnedMods);
            }

            NTLua.Call("OverrideC#Afflictions");
            NTLua.Call("OverrideC#Items");
            NTLua.Call("SyncLegacyTables");
        }, 1000);
    }
}

// This is the same information currently present in NTCompat.lua; it can be added in the Addon's OnLoadCompleted block:
// Yes, it's addon now. No-one calls it expansion like, ever.

// NTInfo.RegisterAddon(new NTAddon
//    {
//      Name = "My Addon",
//      Version = "1.0.0",
//      VersionNum = 01000000
//      MinNTVersion = "A1.17.4",
//      MinNTVersionNum = 1170400
//    });

public class NTAddon
{
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required int VersionNum { get; set; }
    public required string MinNTVersion { get; set; }
    public required int MinNTVersionNum { get; set; }
}
