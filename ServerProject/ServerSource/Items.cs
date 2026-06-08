
namespace Neurotrauma;



public class NTItemMethods
{

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

    private static Dictionary<string, Func<ItemUpdateFunctionInfos, int>> NTItemUpdateFunctions = new Dictionary<string, Func<ItemUpdateFunctionInfos, int>> { };

    public static Dictionary<string, Func<ItemUpdateFunctionInfos, int>> GetNTItemUpdateFunctions ()
    {
        return NTItemUpdateFunctions;
    }

    public static void RegisterItemUpdateFunction(string itemID, Func<ItemUpdateFunctionInfos, int> function)
    {
        if (!NTItemUpdateFunctions.ContainsKey(itemID))
        {
            NTItemUpdateFunctions.Add(itemID, function);
        }
    }

    public static void OverrideItemUpdateFunction(string itemID, Func<ItemUpdateFunctionInfos, int> function)
    {
        if (NTItemUpdateFunctions.ContainsKey(itemID))
        {
            NTItemUpdateFunctions.Add(itemID, function);
        }
    }


    public static void Override_ApplyTreatment(Item __instance, Character user, Character character, Limb targetLimb)
    {

        string itemID = __instance.Prefab.Identifier.ToString();
        if (NTItemUpdateFunctions.ContainsKey(itemID))
        {
            NTItemUpdateFunctions[itemID].Invoke(new ItemUpdateFunctionInfos(__instance, user, character, targetLimb));
        }
    }

    public static void Override_Use(Item __instance, float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
    {
        LuaCsLogger.Log("use");
    }
}


