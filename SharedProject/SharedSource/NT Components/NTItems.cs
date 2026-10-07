using static Neurotrauma.NTAfflictions;

namespace Neurotrauma;

public class NTItems
{
    private static Dictionary<string, Action<ItemUpdateFunctionInfos>> NTItemsRegistry { get; } = new Dictionary<string, Action<ItemUpdateFunctionInfos>> { };


    
   
    
    /// <summary>
    /// Stores which mod defined an item last. 
    /// Key is Item ID and Value is Mod Name.
    /// </summary>
    public static Dictionary<string, string> NTItemsModDefinerRegistry { get; } = new Dictionary<string, string>(); // Stores the mod that defined the affliction

    /// <summary>
    /// When an Item action is overriden, the old Item action is stored here in case a mod needs to access the original action. 
    /// The Key is a tuple of (ModName, AfflictionID) and the Value is the old action.
    /// </summary>
    public static Dictionary<(string, string), Action<ItemUpdateFunctionInfos>> NTOldItemsRegistry { get; } = new Dictionary<(string, string), Action<ItemUpdateFunctionInfos>>();

    /// <summary>
    /// When an Item action is overriden, the replaced action is stored here so the overriding mod can call it with CallPrevious.
    /// The Key is a tuple of (OverridingModName, ItemID) and the Value is the replaced action.
    /// </summary>
    public static Dictionary<(string, string), Action<ItemUpdateFunctionInfos>> NTPreviousItemsRegistry { get; } = new Dictionary<(string, string), Action<ItemUpdateFunctionInfos>>();

    /// <summary>
    /// Contains everything required to defined and change the behavior of items in Neurotrauma.
    /// </summary>
    public class NTItemFunctionLoader
    {
        private string ModID { get; }

        /// <summary>
        /// Create a new instance of the NTItemFunctionLoader class for a specific mod. This allows you to register update functions for items associated with that mod.
        /// </summary>
        /// <param name="ModID">The name of your mod, should be the same as defined in your addon's infos.</param>
        public NTItemFunctionLoader(string ModID)
        {
            this.ModID = ModID;
        }

        /// <summary>
        /// Register a new function associated with the given item ID. This function will be called when the item is used in the game.
        /// <example>
        /// <code>
        /// NTItemFunctionLoader loader = new NTItems.NTItemFunctionLoader("MyMod");
        /// 
        /// loader.Register("MyItemID", (infos) => {
        ///     // Your item update logic here
        /// });
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <param name="UpdateFunction">A function that runs when the item is used.</param>
        /// <returns>true if the function was registered successfully, false otherwise (the item already has a function assigned).</returns>
        /// 
        public bool Register(string ItemID, Action<ItemUpdateFunctionInfos> UpdateFunction)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("NT_DEBUG_MODE", false))
            {
                HF.PrintUtility($"[{this.ModID}] Registering item: {ItemID}");
            }

            if (NTItemsRegistry.ContainsKey(ItemID))
            {
                HF.PrintError($"[{this.ModID}] Item with ID '{ItemID}' already has a registered use function.");
                return false;
            }

            NTItemsModDefinerRegistry[ItemID] = this.ModID;
            NTItemsRegistry.Add(ItemID, UpdateFunction);
            return true;
        }

        public bool Register(string ItemID, LuaCsAction UpdateFunction)
        {
            return Register(ItemID, (Action<ItemUpdateFunctionInfos>) ((ItemUpdateFunctionInfos) =>
            {
                try
                {
                    UpdateFunction(ItemUpdateFunctionInfos);
                }
                catch (Exception e)
                {
                    HF.PrintError($"[Lua] Error when updating item {ItemID} : {e}");
                }

            }));
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

            if (NTConfig.Get("NT_DEBUG_MODE", false)) HF.PrintUtility($"[{this.ModID}] Overriding item: {ItemID}");


            if (!NTItemsRegistry.ContainsKey(ItemID))
            {
                if (RegisterInstead)
                {
                    if (NTConfig.Get("NT_DEBUG_MODE", false)) HF.PrintUtility($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to override. Will Register instead.");
                    return Register(ItemID, UpdateFunction);
                }

                HF.PrintError($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to override.");
                return false;

            }

            NTOldItemsRegistry[(NTItemsModDefinerRegistry[ItemID], ItemID)] = NTItemsRegistry[ItemID];

            // Same mod overriding twice keeps the function it originally replaced, otherwise CallPrevious would call itself forever.
            if (NTItemsModDefinerRegistry[ItemID] != this.ModID)
            {
                NTPreviousItemsRegistry[(this.ModID, ItemID)] = NTItemsRegistry[ItemID];
            }

            NTItemsRegistry[ItemID] = UpdateFunction;
            NTItemsModDefinerRegistry[ItemID] = this.ModID;
            return true;
        }

        public bool Override(string ItemID, LuaCsAction UpdateFunction, bool RegisterInstead = true)
        {
            return Override(ItemID, (Action<ItemUpdateFunctionInfos>)((ItemUpdateFunctionInfos) =>
            {
                try
                {
                    UpdateFunction(ItemUpdateFunctionInfos);
                }
                catch (Exception e)
                {
                    HF.PrintError($"[Lua] Error when updating item {ItemID}: {e}");
                }
            }), RegisterInstead);
        }

        /// <summary>
        /// Removes the update function associated with the given item ID.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <returns>true if there was a function to remove, false otherwise.</returns>
        public bool Remove(string ItemID)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("NT_DEBUG_MODE", false))
            {
                HF.PrintUtility($"[{this.ModID}] Removing item: {ItemID}");
            }


            if (!NTItemsRegistry.ContainsKey(ItemID))
            {
                HF.PrintError($"[{this.ModID}] Item with ID '{ItemID}' does not have a registered use function to remove.");
                return false;
            }

            NTOldItemsRegistry[(NTItemsModDefinerRegistry[ItemID], ItemID)] = NTItemsRegistry[ItemID];
            NTItemsRegistry.Remove(ItemID);
            return true;
        }

        /// <summary>
        /// Check if the given item ID has a corresponding function registered.
        /// </summary>
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
            return NTItemsRegistry.GetValueOrDefault(ItemID);
        }

        public void Call(string ItemID, ItemUpdateFunctionInfos infos)
        {
            if (NTItemsRegistry.TryGetValue(ItemID, out var function))
            {
                function.Invoke(infos);
            }
        }

        /// <summary>
        /// Calls the old function associated with the given item ID and mod name after an override.
        /// 
        /// /// <example>
        /// <code>
        /// NTItemFunctionLoader loader = new NTItems.NTItemFunctionLoader("MyMod");
        /// 
        /// loader.Override("MyItemID", (infos) => {
        ///     // Your item update logic here
        ///     
        ///     loader.CallOld("MyItemID", "OtherMod", infos); // Call the old function defined by OtherMod
        /// });
        /// </code>
        /// </example>
        /// 
        /// ```lua
        /// local loader = NTCS.ItemFunctionLoader("MyMod")
        /// 
        /// loader:Override("MyItemID", function (infos) 
        ///     -- do your things
        ///     
        ///     loader:CallOld("MyItemID", "OtherMod", infos) -- Call the old function defined by OtherMod
        /// end)
        /// ```
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <param name="ModName">The name of the mod that originally defined the item. Should be the same as defined in the addon's infos.</param>
        public void CallOld(string ItemID, string ModName, ItemUpdateFunctionInfos infos)
        {
            if (NTOldItemsRegistry.TryGetValue((ModName, ItemID), out var old))
            {
                old.Invoke(infos);
            }
        }

        /// <summary>
        /// Check if there is an old function associated with the given item ID and mod name after an override.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        /// <param name="ModName">The name of the mod that originally defined the item. Should be the same as defined in the addon's infos.</param>
        public bool HasOld(string ItemID, string ModName)
        {
            return NTOldItemsRegistry.ContainsKey((ModName, ItemID));
        }

        /// <summary>
        /// Calls the function this mod replaced when it overrode the given item. Works through any number of overriding mods,
        /// without needing to know which mod defined the item before.
        ///
        /// <example>
        /// <code>
        /// loader.Override("MyItemID", (infos) => {
        ///     // Your item update logic here
        ///
        ///     loader.CallPrevious("MyItemID", infos);
        /// });
        /// </code>
        /// </example>
        ///
        /// ```lua
        /// loader:Override("MyItemID", function (infos)
        ///     -- do your things
        ///
        ///     loader:CallPrevious("MyItemID", infos)
        /// end)
        /// ```
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        public void CallPrevious(string ItemID, ItemUpdateFunctionInfos infos)
        {
            if (NTPreviousItemsRegistry.TryGetValue((this.ModID, ItemID), out var previous))
            {
                previous.Invoke(infos);
            }
        }

        /// <summary>
        /// Check if this mod replaced a function when it overrode the given item.
        /// </summary>
        /// <param name="ItemID">The ID of the item defined in the XML.</param>
        public bool HasPrevious(string ItemID)
        {
            return NTPreviousItemsRegistry.ContainsKey((this.ModID, ItemID));
        }
    }

    public class ItemUpdateFunctionInfos
    {
        public Item item { get; }
        public NTHuman user { get; }
        public NTHuman target { get; }
        public Limb targetLimb { get; }

        public ItemUpdateFunctionInfos(Item item, NTHuman user, NTHuman target, Limb targetLimb)
        {
            this.item = item;
            this.user = user;
            this.target = target;
            this.targetLimb = targetLimb;
        }
    }

    /// <summary>
    /// Contains all the data necessary to add an Affliction to DrainageAfflictions or SutureAfflictions.
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

        /// <summary>
        /// The affliction ID the target must have (strength 1 or more) for the item to treat it.
        /// </summary>
        public string Case { get; } = "";

        ///<summary>This function will be run to know if the affliction can be cured by the item.</summary>
        /// <example>
        /// <code>
        /// bool conditionFunction(ItemUpdateFunctionInfos infos)
        /// {
        ///     return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
        /// }
        /// </code>
        /// </example>
        public Func<ItemUpdateFunctionInfos, bool> Conditions { get; }

        /// <summary>
        /// This function will be called when the item is used successfully. Useful for removing symptoms.
        /// </summary>
        public Action<ItemUpdateFunctionInfos>? Used { get; }

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
        if (NTItemsRegistry.TryGetValue(itemID, out var function))
        {

            if (user == null)
            {
                HF.PrintError($"Error trying to ApplyTreatment of item {__instance.Name} : user is null");
                return;
            }

            if (character == null)
            {
                HF.PrintError($"Error trying to ApplyTreatment of item {__instance.Name} : target character is null");
                return;
            }

            NTHuman? u = NTHuman.getNTHumanFromCharacter(user);
            NTHuman? t = NTHuman.getNTHumanFromCharacter(character);

            if (u == null)
            {
                HF.PrintError($"Error trying to ApplyTreatment of item {__instance.Name} : {user.Name} is not an NTHuman!");
                return;
            }

            if (t == null)
            {
                HF.PrintError($"Error trying to ApplyTreatment of item {__instance.Name} : {character.Name} is not an NTHuman!");
                return;
            }

            if (!t.HasAffliction("luabotomy"))
            {
                t.AddAffliction("luabotomy", 0.1f);
            }

            try
            {
                function.Invoke(new ItemUpdateFunctionInfos(__instance, u, t, targetLimb));
            }
            catch (Exception e)
            {
                HF.PrintError($"[{NTItemsModDefinerRegistry.GetValueOrDefault(itemID)}] Error when updating item {itemID} : {e}");
            }
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