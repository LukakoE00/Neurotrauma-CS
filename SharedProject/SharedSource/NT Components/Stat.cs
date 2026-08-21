using static Neurotrauma.NTItems;

namespace Neurotrauma;

public class Stats
{
    public abstract class NTStat()
    {
        public void Get()
        {

        }

        public void Set()
        {

        }
    }

    public class NTStatDouble(string Name, double MinStrength = 0, double MaxStrength = 1, double DefaultStrength = 1, Func<HumanUpdate.NTHuman, double>? Update = null) : NTStat()
    {
        private double MinStrength { get; set; } = MinStrength;
        private double MaxStrength { get; set; } = MaxStrength;
        private double DefaultStrength { get; set; } = DefaultStrength;
        private bool Settable { get; set; } = false;
        public string ID = Name;

        public void Add(HumanUpdate.NTHuman C, double AddStrength)
        {
            if (Settable)
            {
                C.LocalStats.DoubleStats[ID].Strength = Math.Clamp(C.LocalStats.DoubleStats[ID].Strength + AddStrength, MinStrength, MaxStrength);
            }
        }

        public double Get(HumanUpdate.NTHuman C)
        {
            return (Update != null) ? Update.Invoke(C) : C.LocalStats.DoubleStats[ID].Strength; // C# my beloved.
        }

        public void Set(HumanUpdate.NTHuman C, double NewStrength)
        {
            if (Settable)
            {
                C.LocalStats.DoubleStats[ID].Strength = Math.Clamp(NewStrength, MinStrength, MaxStrength);
            }
        }

    }

    public class NTStatBool(string Name, bool Strength = false, Func<HumanUpdate.NTHuman, bool> Update = null) : NTStat()
    {
        private bool Settable { get; set; } = false;
        public string ID = Name;

        public bool Get(HumanUpdate.NTHuman C)
        {
            return (Update != null) ? Update.Invoke(C) : Strength; // C# my beloved.
        }

        public void Set(HumanUpdate.NTHuman C, bool NewStrength)
        {
            if (Settable)
            {
                C.LocalStats.BoolStats[ID].Strength = NewStrength;
            }
        }
    }


    public static Dictionary<string, NTStat> StatRegistry = new Dictionary<string, NTStat>();

    public class NTStatLoader
    {
        private string ModID { get; }

        /// <summary>
        /// Create a new instance of the NTStatLoader class for a specific mod. This allows you to register update functions for items associated with that mod.
        /// </summary>
        /// <param name="ModID">The name of your mod, helps with debugging and organization</param>
        public NTStatLoader(string ModID)
        {
            this.ModID = ModID;
        }

        /// <summary>
        /// Register a new function associated with the given item ID. This function will be called when the item is used in the game.
        /// </summary>
        /// <param name="StatID">The ID of the item defined in the XML.</param>
        /// <param name="stat">A function that runs when the item is used.</param>
        /// <returns>true if the function was registered successfully, false otherwise (the item already has a function assigned).</returns>
        /// <example>
        /// <code>
        /// var NTItemFunctionLoader = new NTItems.NTItemFunctionLoader("MyMod");
        /// NTItemFunctionLoader.Register("MyItemID", (infos) => {
        ///     // Your item update logic here
        /// });
        /// </code>
        /// </example>
        public bool Register(string StatID, NTStat Stat)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("debug_mode", true))
            {
                HF.PrintUtility($"[{this.ModID}] Registering stat: {StatID}");
            }

            if (StatRegistry.ContainsKey(StatID))
            {
                HF.PrintError($"[{this.ModID}] Stat with ID '{StatID}' already has a registered use function.");
                return false;
            }

            StatRegistry.Add(StatID, Stat);
            return true;
        }


        /// <summary>
        /// Overrides the update function for an existing item. If the item does not have a registered update function, it will register it instead.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <param name="UpdateFunction">A function that runs when the item is used.</param>
        /// <param name="RegisterInstead">If true, the given function will be registered if the given item has no function to override. If false it will do nothing.</param>
        /// <returns>true if the function was overridden or registered successfully, false otherwise.</returns>
        public bool Override(string ItemID, Action<ItemUpdateFunctionInfos> UpdateFunction, bool RegisterInstead = true)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("debug_mode", true))
            {
                HF.PrintUtility($"[{this.ModID}] Overriding item: {ItemID}");
            }

            if (!StatRegistry.ContainsKey(ItemID))
            {
                if (RegisterInstead)
                {
                    HF.PrintWarning($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to override. Will Register instead.");
                    return Register(ItemID, UpdateFunction);
                }

                HF.PrintError($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to override.");
                return false;

            }

            StatRegistry[ItemID] = UpdateFunction;
            return true;
        }

        /// <summary>
        /// Removes the update function associated with the given item ID.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <returns>true if there was a function to remove, false otherwise.</returns>
        public bool Remove(string ItemID)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("debug_mode", true))
            {
                HF.PrintUtility($"[{this.ModID}] Removing item: {ItemID}");
            }


            if (!StatRegistry.ContainsKey(ItemID))
            {
                HF.PrintError($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to remove.");
                return false;
            }

            StatRegistry.Remove(ItemID);
            return true;
        }

        /// <summary>
        /// Check if the given item ID has a corresponding function registered.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <returns>true if the item has a registered use function, false otherwise.</returns>
        public bool Has(string ItemID)
        {
            return StatRegistry.ContainsKey(ItemID);
        }


        /// <summary>
        /// Returns the function associated with the given item ID, or null if no function is registered for that item.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <returns>The function associated with the item ID, or null if not found.</returns>
        public Action<ItemUpdateFunctionInfos>? Get(string ItemID)
        {
            if (StatRegistry.ContainsKey(ItemID))
            {
                return StatRegistry[ItemID];
            }
            return null;
        }
    }
}