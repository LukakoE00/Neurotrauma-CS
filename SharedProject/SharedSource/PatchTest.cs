namespace Neurotrauma
{
    public class PatchTest
    {

        // Example of patch for https://evilfactory.github.io/LuaCsForBarotrauma/cs-docs/baro-server/html/_character_health_8cs_source.html line 456
        public static void Override_ApplyAffiction(Limb targetLimb, Affliction affliction, bool allowStacking = true, bool ignoreUnkillability = false, bool recalculateVitality = true)
        {
            LuaCsLogger.Log($"Applying affliction: {affliction.Prefab.Identifier}");
        }
    }
}