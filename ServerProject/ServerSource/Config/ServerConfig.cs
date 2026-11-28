using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Barotrauma;

namespace Neurotrauma
{
    public static partial class NTConfig
    {
        // Get the preset name so that clients can peek at it
        static partial void AddPresetInfo(Dictionary<string, object> table)
        {
            table[ConfigMsg.ActivePresetToClient] = Path.GetFileNameWithoutExtension(CurrentConfigPath);
        }

        static partial void OnConfigReceived(Dictionary<string, JsonElement> table)
        {
            // If in-game we change the preset, make sure the server knows and changes accordingly.
            if (table.TryGetValue(ConfigMsg.PresetToServer, out JsonElement PresetElement) && PresetElement.ValueKind == JsonValueKind.String)
            {
                string? PresetName = PresetElement.GetString();
                if (!string.IsNullOrWhiteSpace(PresetName))
                {
                    SetActivePath(FindPresetPath(PresetName) ?? ResolvePathFromName(PresetName));
                    SaveConfig(CurrentConfigPath);
                }
            }

            // Originally, the UI synced on level changes; this is not required at all so just do it now.
            SendConfig();
        }

        // This one is for you "John"
        // Dedicated Servers used to just use the only NT config.json available; but with presets that became more complicated.
        // As per above, a host can join the game and swap / edit the preset that way; but we also want to give them the option to do it via the command box.
        // Since all functions already exist for this, we're mostly just printing in the console.
        // Commands needed: 1 to list the known presets, 1 to know the current one and 1 to swap the current one. I'm not hooking up settings changing to this.
        public static void RegisterPresetCommands()
        {
            LuaCsSetup.Instance.Game.AddCommand("nt_listpresets", "Lists available Neurotrauma config presets.",
                new LuaCsAction(args =>
                {
                    foreach (string Name in CollectPresets().Keys)
                    {
                        HF.Print("- " + Name);
                    }
                }));

            LuaCsSetup.Instance.Game.AddCommand("nt_currentpreset", "Shows the active Neurotrauma config preset path.",
                new LuaCsAction(args =>
                {
                    HF.Print("Current preset: " + CurrentConfigPath);
                }));

            LuaCsSetup.Instance.Game.AddCommand("nt_loadpreset", "nt_loadpreset <name>: loads a Neurotrauma config preset.",
                new LuaCsAction(args =>
                {
                    // Command arguments are a string list so we gotta yoink them individually.
                    var Parts = new List<string>();
                    if (args != null)
                    {
                        foreach (object Arg in args)
                        {
                            if (Arg is string[] Strings) Parts.AddRange(Strings);
                            else if (Arg != null) Parts.Add(Arg.ToString() ?? "");
                        }
                    }
                    string Name = string.Join(" ", Parts).Trim();

                    if (Name == "")
                    {
                        HF.Print("Usage: nt_loadpreset <name>");
                        return;
                    }

                    if (!SelectPreset(Name))
                    {
                        HF.Print("Preset not found: " + Name + " (use nt_listpresets)");
                        return;
                    }

                    // Make sure clients are in the know.
                    SendConfig();

                    HF.Print("Loaded preset: " + Name);
                }),
                new LuaCsFunc(args => new string[][] { CollectPresets().Keys.ToArray() }));
        }
    }
}