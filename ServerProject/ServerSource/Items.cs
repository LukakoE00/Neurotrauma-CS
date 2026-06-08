
namespace Neurotrauma;



public class NTItemMethods
{

    /**
     * <summary>
     * Contains all the data necessary for an item update function.
     * 
     * </summary>
     */
    public class ItemUpdateFunctionInfos
    {
        public Item item { get; }
        public Character user {  get; }
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

    private static Dictionary<string, Func<ItemUpdateFunctionInfos, int>> NTItemRegistry = new Dictionary<string, Func<ItemUpdateFunctionInfos, int>> { };

    public static Dictionary<string, Func<ItemUpdateFunctionInfos, int>> GetNTItemRegistry ()
    {
        return NTItemRegistry;
    }

    /**<summary>
     * Register a new identifier => function to the NTItemRegistry.
     * The function will trigger when the item matching the identifier is used.
     * </summary>
     * <param name="itemID">The identifier described in the XML.</param>
     * <param name="function">The item update function.</param>
     * <returns>Returns true if the item was defined correctly (if it was not already defined). Returns false otherwise.</returns>
     */
    public static bool RegisterItemUpdateFunction(string itemID, Func<ItemUpdateFunctionInfos, int> function)
    {
        if (!NTItemRegistry.ContainsKey(itemID))
        {
            NTItemRegistry.Add(itemID, function);
            return true;
        }

        return false;
    }

    /**<summary>
     * Overrides an already defined function matching the itemID.
     * Does nothing if the itemID isn't defined in the registry.
     * </summary>
     * <param name="itemID">The identifier described in the XML.</param>
     * <param name="function">The item update function.</param>
     * <returns>Returns true if the override was succesful, false otherwise.</returns>
     */
    public static bool OverrideItemUpdateFunction(string itemID, Func<ItemUpdateFunctionInfos, int> function)
    {
        if (NTItemRegistry.ContainsKey(itemID))
        {
            NTItemRegistry.Add(itemID, function);
            return true;
        }

        return false;
    }


    /**
     * <summary>
     * The function patching the base game Item.ApplyTreatment
     * </summary>
     */
    public static void Override_ApplyTreatment(Item __instance, Character user, Character character, Limb targetLimb)
    {

        string itemID = __instance.Prefab.Identifier.ToString();
        if (NTItemRegistry.ContainsKey(itemID))
        {
            NTItemRegistry[itemID].Invoke(new ItemUpdateFunctionInfos(__instance, user, character, targetLimb));
        }
    }

    /**
     * <summary>
     * The function patching the base game Item.Use
     * </summary>
     */
    public static void Override_Use(Item __instance, float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
    {
        LuaCsLogger.Log("use");
    }
}


