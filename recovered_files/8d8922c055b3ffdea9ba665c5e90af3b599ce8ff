namespace Neurotrauma;

public class NTItems
{

    private static Dictionary<string, Action<ItemUpdateFunctionInfos>> NTItemsRegistry { get; } = new Dictionary<string, Action<ItemUpdateFunctionInfos>> { };

    /// <summary>
    /// Contains everything required to defined and change the behavior of items in Neurotrauma.
    /// </summary>
    public class NTItemFunctionLoader
    {
        private string ModID { get; }

        /// <summary>
        /// Create a new instance of the NTItemFunctionLoader class for a specific mod. This allows you to register update functions for items associated with that mod.
        /// </summary>
        /// <param name="ModID">The name of your mod, helps with debugging and organization</param>
        public NTItemFunctionLoader(string ModID)
        {
            this.ModID = ModID;
        }

        /// <summary>
        /// Register a new function associated with the given item ID. This function will be called when the item is used in the game.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <param name="UpdateFunction">A function that runs when the item is used.</param>
        /// <returns>true if the function was registered successfully, false otherwise (the item already has a function assigned).</returns>
        /// <example>
        /// <code>
        /// var NTItemFunctionLoader = new NTItems.NTItemFunctionLoader("MyMod");
        /// NTItemFunctionLoader.Register("MyItemID", (infos) => {
        ///     // Your item update logic here
        /// });
        /// </code>
        /// </example>
        public bool Register(string ItemID, Action<ItemUpdateFunctionInfos> UpdateFunction)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("debug_mode", true))
            {
                HF.PrintUtility($"[{this.ModID}] Registering item: {ItemID}");
            }

            if (NTItemsRegistry.ContainsKey(ItemID))
            {
                HF.PrintError($"[{this.ModID}] Item with ID '{ItemID}' already has a registered use function.");
                return false;
            }

            NTItemsRegistry.Add(ItemID, UpdateFunction);
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

            if (!NTItemsRegistry.ContainsKey(ItemID))
            {
                if (RegisterInstead)
                {
                    HF.PrintWarning($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to override. Will Register instead.");
                    return Register(ItemID, UpdateFunction);
                }

                HF.PrintError($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to override.");
                return false;

            }

            NTItemsRegistry[ItemID] = UpdateFunction;
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


            if (!NTItemsRegistry.ContainsKey(ItemID))
            {
                HF.PrintError($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to remove.");
                return false;
            }

            NTItemsRegistry.Remove(ItemID);
            return true;
        }

        /// <summary>
        /// Check if the given item ID has a corresponding function registered.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <returns>true if the item has a registered use function, false otherwise.</returns>
        public bool Has(string ItemID)
        {
            return NTItemsRegistry.ContainsKey(ItemID);
        }


        /// <summary>
        /// Returns the function associated with the given item ID, or null if no function is registered for that item.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <returns>The function associated with the item ID, or null if not found.</returns>
        public Action<ItemUpdateFunctionInfos>? Get(string ItemID)
        {
            if (NTItemsRegistry.ContainsKey(ItemID))
            {
                return NTItemsRegistry[ItemID];
            }
            return null;
        }
    }

    public class ItemUpdateFunctionInfos
    {
        public Item item { get; }
        public Character user { get; }
        public Character target { get; }
        public Limb targetLimb { get; }

        public ItemUpdateFunctionInfos(Item item, Character user, Character target, Limb targetLimb)
        {
            this.item = item;
            this.user = user;
            this.target = target;
            this.targetLimb = targetLimb;
        }
    }

    /// <summary>
    /// Contains all the data necessary to add an Affliction to DrainageAfflictions.
    /// </summary>
    public class ItemsAfflictionInfos
    {

        /// <summary>
        /// The ID defined in the XML. <strong>The affliction CANNOT BE Limb-Specific.</strong>
        /// </summary>
        public string AfflictionID { get; }

        /// <summary>
        /// The amount of XP given to the surgery or medical skill when the item is applied successfully.
        /// </summary>
        public int XPGain { get; }

        ///<summary>This function will be run to know if the affliction can be cured by the drainage.</summary>
        /// <example>
        /// <code>
        /// bool conditionFunction(ItemUpdateFunctionInfos infos)
        /// {
        ///     return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
        /// }
        /// </code>
        /// </example>
        public string Case { get; } = "";
        public Func<ItemUpdateFunctionInfos, bool> Conditions { get; }

        /// <summary>
        /// This function will be called when the item is used successfully. Useful for removing symptoms.
        /// </summary>
        public Action<ItemUpdateFunctionInfos>? Used { get; }
        public LuaCsAction LuaConditions { get; }

        public ItemsAfflictionInfos(string affID, int xpGain, Func<ItemUpdateFunctionInfos, bool> conditions, string newCase = "", Action<ItemUpdateFunctionInfos>? used = null)
        {
            AfflictionID = affID;
            XPGain = xpGain;
            Conditions = conditions;
            Case = newCase;
            Used = used;
        }
    }

    /// <summary>
    /// A List containing Identifiers for all afflictions curable by using the Drainage item.
    /// </summary>
    public static Dictionary<string, ItemsAfflictionInfos> DrainageAfflictions { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all afflictions removable by either Trauma Shears or Diving Knives.
    /// </summary>
    public static List<string> CuttableAfflictions { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all afflictions removable by Trauma Shears.
    /// </summary>
    public static List<string> TraumaShearsAfflictions { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all afflictions healable by Sutures.
    /// </summary>
    public static Dictionary<string, ItemsAfflictionInfos> SutureAfflictions { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all afflictions detectable by the Blood Analyzer.
    /// </summary>
    public static List<string> HematologyDetectable { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all items with Wrench functionality.
    /// </summary>
    public static List<string> WrenchItems { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all Blood Pack items.
    /// </summary>
    public static List<string> BloodPacks { get; } = [];


    /// <summary>
    /// The function patching the base game Item.ApplyTreatment
    /// </summary>
    public static void Override_ApplyTreatment(Barotrauma.Item __instance, Character user, Character character, Limb targetLimb)
    {

        string itemID = __instance.Prefab.Identifier.ToString();
        if (NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry[itemID].Invoke(new ItemUpdateFunctionInfos(__instance, user, character, targetLimb));
        }
    }

    /// <summary>
    /// The function patching the base game Item.Use
    /// </summary>
    public static void Override_Use(Barotrauma.Item __instance, float deltaTime, Character? user = null, Limb? targetLimb = null, Entity? useTarget = null, Character? userForOnUsedEvent = null)
    {
        // LuaCsLogger.Log("use");
    }
}