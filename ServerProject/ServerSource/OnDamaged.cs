namespace Neurotrauma;

public class OnDamaged
{
    public static void Override_ApplyDamage(Limb hitLimb, AttackResult attackResult, bool allowStacking = true, bool recalculateVitality = true)
    {
        LuaCsLogger.Log("ApplyDamage triggerred");
    }

    public static void Override_DamageLimb(Vector2 worldPosition, Limb hitLimb, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, 
        Character attacker = null, float damageMultiplier = 1, bool allowStacking = true, float penetration = 0f, bool shouldImplode = false, 
        bool ignoreDamageOverlay = false, bool recalculateVitality = true)
    {
        LuaCsLogger.Log("DamageLimb triggerred");
    }
}