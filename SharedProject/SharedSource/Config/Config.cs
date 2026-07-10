using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Barotrauma;
using Barotrauma.Networking;
using MoonSharp.Interpreter;

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
        public LocalizedString Name;
        public ConfigEntryType Type;
        public object Default;
        public object Value;
        public float[] Range;
        public bool Group;
        public bool Resettable;
        public LocalizedString Description;
        public LocalizedString Style;
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

        private static ConfigEntryType StringToConfigEntry(string Type)
        {
            switch (Type)
            {
                case "category":
                    return ConfigEntryType.Category;
                case "float":
                    return ConfigEntryType.Float;
                case "bool":
                    return ConfigEntryType.Bool;
                case "string":
                    return ConfigEntryType.String;
                default:
                    return ConfigEntryType.Category;
            }
        }

        // This translates the Lua config options so the C# GUI can use them.
        public static void AddConfigOptions(Table Expansion)
        {
            ConfigExpansion LuaExpansion = new() { Name = Expansion.Get("Name").IsNil() ? "Unknown" : Expansion.Get("Name").String, ConfigData = new Dictionary<string, ConfigEntry>() };
            DynValue ConfigDataValue = Expansion.Get("ConfigData");

            if (ConfigDataValue.IsNil() || ConfigDataValue.Type != DataType.Table) return;

            foreach (TablePair Kvp in ConfigDataValue.Table.Pairs)
            {
                if (Kvp.Value.Type != DataType.Table) continue;

                ConfigEntry Entry = new();
                Table SubInfo = Kvp.Value.Table;

                // So, Lua tables get wrapped by DynValue (as far as I understand it) because Lua tables are just 'throw whatever the fuck we want' in there type beats.
                // To actually use it, we have to force-convert the entries into the fields we have up above so the client's GUI script can interpret them.
                foreach (TablePair Pair in SubInfo.Pairs)
                {
                    if (Pair.Key == null) continue;

                    string Key = Pair.Key.String;

                    if (Key.Equals("type", StringComparison.OrdinalIgnoreCase) || Key.Equals("page", StringComparison.OrdinalIgnoreCase)) continue;

                    FieldInfo Field = typeof(ConfigEntry).GetFields().FirstOrDefault(F => string.Equals(F.Name, Key, StringComparison.OrdinalIgnoreCase));
                    if (Field == null) continue;

                    DynValue Dyn = Pair.Value;

                    // Localized String!
                    if (Field.FieldType == typeof(LocalizedString))
                    {
                        Field.SetValue(Entry, TextManager.ContainsTag(Dyn.String) ? TextManager.Get(Dyn.String) : (LocalizedString)Dyn.String);
                    }
                    // String!
                    else if (Field.FieldType == typeof(string))
                    {
                        Field.SetValue(Entry, Dyn.String);
                    }
                    // Bool!
                    else if (Field.FieldType == typeof(bool))
                    {
                        Field.SetValue(Entry, Dyn.Boolean);
                    }
                    // Float / Scalar!
                    else if (Field.FieldType == typeof(float))
                    {
                        Field.SetValue(Entry, (float)Dyn.Number);
                    }
                    // Floats but in a table! (Think the Ranges)
                    else if (Field.FieldType == typeof(float[]) && Dyn.Type == DataType.Table)
                    {
                        Field.SetValue(Entry, Dyn.Table.Pairs.Select(P => (float)P.Value.Number).ToArray());
                    }
                    // Whatever else may appear!
                    else if (Field.FieldType == typeof(object))
                    {
                        Field.SetValue(Entry, Dyn.Type switch
                        {
                            DataType.Number => (float)Dyn.Number,
                            DataType.Boolean => Dyn.Boolean,
                            DataType.String => Dyn.String,
                            DataType.Table => Dyn.Table.Pairs.Select(P => P.Value.ToObject()).ToList(),
                            _ => null
                        });
                    }
                }

                // Types! (Think Categories)
                DynValue TypeValue = SubInfo.Get("type");
                if (!TypeValue.IsNil())
                {
                    Entry.Type = StringToConfigEntry(TypeValue.String);
                }

                Entry.Value = Entry.Default;

                // Page!
                DynValue PageValue = SubInfo.Get("page");
                if (!PageValue.IsNil())
                {
                    Entry.Page = PageValue.String;
                }

                // Name!
                if (!Expansion.Get("Name").IsNil())
                {
                    Entry.Expansion = Expansion.Get("Name").String;
                }
                
                LuaExpansion.ConfigData.Add(Kvp.Key.String, Entry);
                Entries[Kvp.Key.String] = Entry;
            }

            Expansions.Add(LuaExpansion);
        }

        public static void AddConfigOptions(ConfigExpansion Expansion)
        {
            if (Expansions.Any(e => e.Name == Expansion.Name)) return;

            Expansions.Add(Expansion);
            foreach (KeyValuePair<string, ConfigEntry> kvp in Expansion.ConfigData)
            {
                ConfigEntry entry = kvp.Value;
                entry.Value = entry.Default;
                entry.Expansion = Expansion.Name;
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
                    if (Entries.TryGetValue(kvp.Key, out var value))
                    {
                        ConfigEntry entry = Entries[kvp.Key];
                        switch (entry.Type)
                        {
                            case ConfigEntryType.Bool:
                                entry.Value = kvp.Value.GetBoolean();
                                break;

                            case ConfigEntryType.Float:
                                entry.Value = (float)kvp.Value.GetDouble();
                                break;

                            case ConfigEntryType.String:
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
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LuaCsLogger.LogError("[NT] Error loading config: " + ex.Message);
            }
            HF.Print("Config has been loaded!");
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

        //public static T Get<T>(string key, T defaultValue)
        //{
            //HF.Print(key);
            //HF.Print($"{defaultValue}");
            //if (Entries.TryGetValue(key, out ConfigEntry entry))
            //{
                //if (entry.Value == null) return defaultValue;

                //if (entry.Value is T directMatch) return directMatch;

                //try
                //{
                    //return (T)Convert.ChangeType(entry.Value, typeof(T));
                //}
                //catch
                //{
                    //return defaultValue;
               // }
            //}
            ///return defaultValue;
        //}

        public static bool Get(string key, bool defaultValue)
        {
            if (Entries.TryGetValue(key, out ConfigEntry entry))
            {
                if (entry.Value == null) return defaultValue;
                if (entry.Value is bool) return (bool)entry.Value;
            }
            return defaultValue;
        }

        public static float Get(string key, float defaultValue)
        {
            if (Entries.TryGetValue(key, out ConfigEntry entry))
            {
                if (entry.Value == null) return defaultValue;
                if (entry.Value is float) return (float)entry.Value;
            }
            return defaultValue;
        }

        public static string Get(string key, string defaultValue)
        {
            if (Entries.TryGetValue(key, out ConfigEntry entry))
            {
                if (entry.Value == null) return defaultValue;
                if (entry.Value is string) return (string)entry.Value;
            }
            return defaultValue;
        }

        public static double Get(string key, double defaultValue)
        {
            if (Entries.TryGetValue(key, out ConfigEntry entry))
            {
                if (entry.Value == null) return defaultValue;
                if (entry.Value is double) return (double)entry.Value;
            }
            return defaultValue;
        }

        public static List<string> Get(string key, List<string> defaultValue)
        {
            if (Entries.TryGetValue(key, out ConfigEntry entry))
            {
                if (entry.Value == null) return defaultValue;
                if (entry.Value is List<string>) return (List<string>)entry.Value;
            }
            return defaultValue;
        }

        public static IEnumerable<string> Get(string key, IEnumerable<string> defaultValue)
        {
            if (Entries.TryGetValue(key, out ConfigEntry entry))
            {
                if (entry.Value == null) return defaultValue;
                if (entry.Value is IEnumerable<string>) return (IEnumerable<string>)entry.Value;
            }
            return defaultValue;
        }
        public static object Get(string key)
        {
            if (Entries.TryGetValue(key, out ConfigEntry entry))
            {
                if (entry.Value == null) return null;
                if (entry.Value is object) return (object)entry.Value;
            }
            return null;
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