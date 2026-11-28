using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Barotrauma;

namespace Neurotrauma
{
    public static partial class NTConfig
    {
        // This is a fucking mess :fire:
        private static readonly string PresetDirectoryPath = Path.Combine(SaveUtil.DefaultSaveFolder, "ModConfigs", "Neurotrauma Configs").Replace('\\', '/');
        private static readonly string DefaultPresetDirectory = Path.Combine(Initialization.NeurotraumaModDir, "DefaultPresets").Replace('\\', '/');
        private static readonly string DefaultPresetDirectoryFull = Path.GetFullPath(DefaultPresetDirectory).Replace('\\', '/');
        private static readonly string DefaultConfigFilePath = Path.Combine(DefaultPresetDirectory, "Intended Config Preset.json").Replace('\\', '/');
        private static readonly string SelectedPresetFilePath = Path.Combine(SaveUtil.DefaultSaveFolder, "ModConfigs", "Neurotrauma Selected Preset.json").Replace('\\', '/');

        private static string currentConfigPath = DefaultConfigFilePath;
        public static string CurrentConfigPath => currentConfigPath;
        public static string DefaultConfigName => Path.GetFileNameWithoutExtension(DefaultConfigFilePath);

        // Change the preset by selecting our beloved json.
        private static void SetActivePath(string path)
        {
            if (currentConfigPath == path) return;

            currentConfigPath = path;
            SaveSelectedPreset();
        }

        public static void SetCurrentConfigPath(string path) => SetActivePath(path);

        public static IEnumerable<string> GetDefaultPresetFiles() => Directory.GetFiles(DefaultPresetDirectory, "*.json");

        public static IEnumerable<string> GetPresetFiles() => Directory.Exists(PresetDirectoryPath) ? Directory.GetFiles(PresetDirectoryPath, "*.json") : Array.Empty<string>();

        public static bool IsDefaultPreset(string PresetPath)
        {
            string FullPresetPath = Path.GetFullPath(PresetPath).Replace('\\', '/');
            return FullPresetPath.StartsWith(DefaultPresetDirectoryFull);
        }

        public static string GetPresetFilePath(string PresetName) => Path.Combine(PresetDirectoryPath, SanitizePresetName(PresetName) + ".json").Replace('\\', '/');

        private static string SanitizePresetName(string PresetName)
        {
            foreach (char Character in Path.GetInvalidFileNameChars())
            {
                PresetName = PresetName.Replace(Character, '_');
            }

            return PresetName;
        }

        public static string ResolvePathFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Equals(DefaultConfigName))
            {
                return DefaultConfigFilePath;
            }

            return GetPresetFilePath(name);
        }

        public static void SavePreset(string PresetName)
        {
            string PresetPath = GetPresetFilePath(PresetName);
            SaveConfig(PresetPath);
            SetActivePath(PresetPath);
        }

        public static void LoadPreset(string PresetPath)
        {
            LoadConfig(PresetPath);
            SetActivePath(PresetPath);
        }

        // Collect my pages
        // This just finds the .jsons in the two locations they should be.
        public static Dictionary<string, string> CollectPresets()
        {
            var Presets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string File in GetDefaultPresetFiles())
            {
                Presets[Path.GetFileNameWithoutExtension(File)] = File;
            }

            foreach (string File in GetPresetFiles())
            {
                Presets[Path.GetFileNameWithoutExtension(File)] = File;
            }

            return Presets;
        }

        public static string? FindPresetPath(string name) => CollectPresets().TryGetValue(name, out string? path) ? path : null;

        // Load a preset by name.
        public static bool SelectPreset(string name)
        {
            string? path = FindPresetPath(name);
            if (path == null) return false;

            ResetConfig(false);
            LoadPreset(path);
            return true;
        }

        // If we've previously selected a preset, it gets saved to its own small .json; load it if it exists.
        public static void LoadStartupPreset()
        {
            LoadConfig();

            if (GameMain.NetworkMember?.IsClient == true || !System.IO.File.Exists(SelectedPresetFilePath)) return;

            try
            {
                var Data = JsonSerializer.Deserialize<Dictionary<string, string>>(System.IO.File.ReadAllText(SelectedPresetFilePath));
                if (Data == null || !Data.TryGetValue("SelectedPreset", out string? Name) || string.IsNullOrWhiteSpace(Name)) return;

                if (!SelectPreset(Name))
                {
                    HF.PrintError("Saved preset '" + Name + "' no longer exists, using default.");
                }
            }
            catch (Exception ex)
            {
                HF.PrintError("Error restoring selected preset: " + ex.Message);
            }
        }

        // Only the host / singleplayer / dedicated server remembers the selection.
        private static void SaveSelectedPreset()
        {
            if (GameMain.NetworkMember?.IsClient == true) return;

            try
            {
                string? Dir = Path.GetDirectoryName(SelectedPresetFilePath);
                if (!string.IsNullOrEmpty(Dir) && !Directory.Exists(Dir))
                {
                    Directory.CreateDirectory(Dir);
                }

                var Data = new Dictionary<string, string>
                {
                    ["SelectedPreset"] = Path.GetFileNameWithoutExtension(currentConfigPath)
                };

                System.IO.File.WriteAllText(SelectedPresetFilePath, JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
               HF.PrintError("Error saving selected preset: " + ex.Message);
            }
        }
    }
}
