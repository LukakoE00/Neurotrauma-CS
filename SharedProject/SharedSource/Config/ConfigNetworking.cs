using System;
using System.Collections.Generic;
using System.Text.Json;
using Barotrauma;
using Barotrauma.Networking;

namespace Neurotrauma
{
    internal static class ConfigMsg
    {
        public const string Update = "NT.ConfigUpdate";
        public const string Request = "NT.ConfigRequest";
        public const string PresetToServer = "__preset";
        public const string ActivePresetToClient = "__activepreset";
    }

    public static partial class NTConfig
    {
        static partial void AddPresetInfo(Dictionary<string, object> table);
        static partial void OnConfigReceived(Dictionary<string, JsonElement> table);

        public static void SendConfig(Client? target = null)
        {
            var Table = new Dictionary<string, object>();

            foreach (var kvp in Entries)
            {
                if (kvp.Value.Type != ConfigEntryType.Category && kvp.Value.IsClientside != true)
                {
                    Table[kvp.Key] = kvp.Value.Value;
                }
            }

            AddPresetInfo(Table);

            IWriteMessage msg = LuaCsSetup.Instance.Networking.Start(ConfigMsg.Update);
            msg.WriteString(JsonSerializer.Serialize(Table));

#if SERVER
            if (target != null)
            {
                LuaCsSetup.Instance.Networking.Send(msg, target.Connection);
                return;
            }
#endif
            LuaCsSetup.Instance.Networking.Send(msg);
        }

        public static void ReceiveConfig(IReadMessage msg)
        {
            try
            {
                var Table = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(msg.ReadString());
                if (Table == null) return;

                foreach (var kvp in Table)
                {
                    if (Entries.TryGetValue(kvp.Key, out ConfigEntry? entry) && entry.IsClientside != true)
                    {
                        ConvertJsonValues(entry, kvp.Value);
                    }
                }

                OnConfigReceived(Table);
            }
            catch (Exception ex)
            {
                HF.PrintError("Error receiving network config: " + ex.Message);
            }
        }
    }
}