
namespace Neurotrauma;

public class ItemMethods
{

    public static void Override_ApplyTreatment(Item __instance, Character user, Character character, Limb targetLimb)
    {
        LuaCsLogger.Log($"applytreatment {__instance.Prefab.Identifier}");
    }

    public static void Override_Use(Item __instance, float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
    {
        LuaCsLogger.Log("use");
    }
}
