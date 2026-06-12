using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Barotrauma;
using Barotrauma.Networking;

namespace Neurotrauma
{
    public enum ConfigEntryType
    {
        Category,
        Float,
        Bool,
        String
    }

    public class ConfigEntry
    {
        public string Name;
        public ConfigEntryType Type;
        public object Default;
        public object Value;
        public float[] Range;
        public bool Group;
        public bool Resettable;
        public string Description;
        public string Style;
        public float Boxsize;
        public bool NoMLTB;
        public string Page;
        public string Expansion;
    }

    public class ConfigExpansion
    {
        public string Name;
        public Dictionary<string, ConfigEntry> ConfigData;
    }

    public static class NTConfig
    {
        public static Dictionary<string, ConfigEntry> Entries = new Dictionary<string, ConfigEntry>();
        public static List<ConfigExpansion> Expansions = new List<ConfigExpansion>();

        private static readonly string ConfigDirectoryPath = Path.Combine(SaveUtil.DefaultSaveFolder, "ModConfigs").Replace('\\', '/');
        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectoryPath, "Neurotrauma.json").Replace('\\', '/');

        public static void AddConfigOptions(ConfigExpansion expansion)
        {
            if (Expansions.Any(e => e.Name == expansion.Name)) return;

            Expansions.Add(expansion);
            foreach (KeyValuePair<string, ConfigEntry> kvp in expansion.ConfigData)
            {
                ConfigEntry entry = kvp.Value;
                entry.Value = entry.Default;
                entry.Expansion = expansion.Name;
                Entries[kvp.Key] = entry;
            }
        }

        public static void SaveConfig()
        {
            if (GameMain.NetworkMember != null &&
                GameMain.NetworkMember.IsClient)
            {
                return;
            }

            Dictionary<string, object> tableToSave = new Dictionary<string, object>();
            foreach (KeyValuePair<string, ConfigEntry> kvp in Entries)
            {
                if (kvp.Value.Type != ConfigEntryType.Category)
                {
                    tableToSave[kvp.Key] = kvp.Value.Value;
                }
            }

            try
            {
                if (!Directory.Exists(ConfigDirectoryPath)) Directory.CreateDirectory(ConfigDirectoryPath);
                string json = JsonSerializer.Serialize(tableToSave, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                LuaCsLogger.LogError("[Neurotrauma] Error saving config: " + ex.Message);
            }
        }

        public static void LoadConfig()
        {
            if (!File.Exists(ConfigFilePath)) return;

            try
            {
                string jsonContent = File.ReadAllText(ConfigFilePath);
                var readConfig = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);

                if (readConfig == null) return;

                foreach (KeyValuePair<string, JsonElement> kvp in readConfig)
                {
                    if (Entries.ContainsKey(kvp.Key))
                    {
                        ConfigEntry entry = Entries[kvp.Key];
                        if (entry.Type == ConfigEntryType.Bool) entry.Value = kvp.Value.GetBoolean();
                        else if (entry.Type == ConfigEntryType.Float) entry.Value = (float)kvp.Value.GetDouble();
                        else if (entry.Type == ConfigEntryType.String)
                        {
                            if (entry.Default is List<string> || kvp.Value.ValueKind == JsonValueKind.Array)
                            {
                                entry.Value = kvp.Value.EnumerateArray()
                                    .Select(e => e.GetString())
                                    .ToList();
                            }
                            else
                            {
                                entry.Value = kvp.Value.GetString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LuaCsLogger.LogError("[Neurotrauma] Error loading config: " + ex.Message);
            }
        }

        public static void ResetConfig()
        {
            foreach (var kvp in Entries)
            {
                ConfigEntry entry = kvp.Value;

                if (entry.Type == ConfigEntryType.Category)
                    continue;

                entry.Value = entry.Default;
            }
        }

        public static T Get<T>(string key, T defaultValue)
        {
            if (Entries.TryGetValue(key, out ConfigEntry entry))
            {
                if (entry.Value == null) return defaultValue;

                if (entry.Value is T directMatch) return directMatch;

                try
                {
                    return (T)Convert.ChangeType(entry.Value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public static void Set(string key, object value)
        {
            if (Entries.ContainsKey(key))
            {
                Entries[key].Value = value;
            }
        }

        public static void SendConfig()
        {
            Dictionary<string, object> tableToSend =
                new Dictionary<string, object>();

            foreach (var kvp in Entries)
            {
                if (kvp.Value.Type != ConfigEntryType.Category)
                {
                    tableToSend[kvp.Key] = kvp.Value.Value;
                }
            }

            IWriteMessage msg =
                LuaCsSetup.Instance.Networking.Start("NT.ConfigUpdate");

            msg.WriteString(JsonSerializer.Serialize(tableToSend));

            LuaCsSetup.Instance.Networking.Send(msg);
        }

        public static void ReceiveConfig(IReadMessage msg)
        {
            try
            {
                string json = msg.ReadString();
                var receivedTable = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (receivedTable == null) return;

                foreach (KeyValuePair<string, JsonElement> kvp in receivedTable)
                {
                    if (Entries.ContainsKey(kvp.Key))
                    {
                        ConfigEntry entry = Entries[kvp.Key];
                        if (entry.Type == ConfigEntryType.Bool) entry.Value = kvp.Value.GetBoolean();
                        else if (entry.Type == ConfigEntryType.Float) entry.Value = (float)kvp.Value.GetDouble();
                        else if (entry.Type == ConfigEntryType.String)
                        {
                            if (entry.Default is List<string> || kvp.Value.ValueKind == JsonValueKind.Array)
                            {
                                entry.Value = kvp.Value.EnumerateArray()
                                    .Select(e => e.GetString())
                                    .ToList();
                            }
                            else
                            {
                                entry.Value = kvp.Value.GetString();
                            }
                        }
                    }
                }

                SaveConfig();
            }
            catch (Exception ex)
            {
                LuaCsLogger.LogError("[Neurotrauma] Error receiving network config: " + ex.Message);
            }
        }
    }
}