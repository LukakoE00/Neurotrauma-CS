using Neurotrauma;
using Barotrauma.Items.Components;

[HarmonyPatch]
public static class HarmonyPatches
{
    // ApplyDamage
    [HarmonyPatch(typeof(CharacterHealth), "ApplyDamage", new Type[] { typeof(Limb), typeof(AttackResult), typeof(bool), typeof(bool) }), HarmonyPrefix] 
    private static void ApplyDamage_Prefix(CharacterHealth __instance, Limb hitLimb, AttackResult attackResult, bool allowStacking, bool recalculateVitality) 
    { 
        OnDamaged.Override_ApplyDamage(__instance, hitLimb, attackResult, allowStacking, recalculateVitality); 
    }

    // DamageLimb
    [HarmonyPatch(typeof(Character), "DamageLimb", new Type[] { typeof(Vector2), typeof(Limb), typeof(IEnumerable<Affliction>), typeof(float), typeof(bool), typeof(Vector2), typeof(Character), typeof(float), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(bool) }), HarmonyPrefix]
    private static void DamageLimb_Prefix(Character __instance, Vector2 worldPosition, Limb hitLimb, IEnumerable<Affliction> afflictions, float stun, bool playSound, Vector2 attackImpulse, Character attacker, float damageMultiplier, bool allowStacking, float penetration, bool shouldImplode, bool ignoreDamageOverlay, bool recalculateVitality)
    {
        OnDamaged.Override_DamageLimb(__instance, worldPosition, hitLimb, afflictions, stun, playSound, attackImpulse, attacker, damageMultiplier, allowStacking, penetration, shouldImplode, ignoreDamageOverlay, recalculateVitality);
    }

    // Item Use
    [HarmonyPatch(typeof(Item), "Use", new Type[] { typeof(float), typeof(Character), typeof(Limb), typeof(Entity), typeof(Character) }), HarmonyPrefix]
    private static void Use_Prefix(Item __instance, float deltaTime, Character user, Limb targetLimb, Entity useTarget, Character userForOnUsedEvent)
    {
        NTItems.Override_Use(__instance, deltaTime, user, targetLimb, useTarget, userForOnUsedEvent);
    }

    // Apply Treatment
    [HarmonyPatch(typeof(Item), "ApplyTreatment", new Type[] { typeof(Character), typeof(Character), typeof(Limb) }), HarmonyPrefix]
    private static void ApplyTreatment_Prefix(Item __instance, Character user, Character character, Limb targetLimb)
    {
        NTItems.Override_ApplyTreatment(__instance, user, character, targetLimb);
    }

    // Bleeding Affliction
    [HarmonyPatch(typeof(AfflictionBleeding), "Update", new Type[] { typeof(CharacterHealth), typeof(Limb), typeof(float) }), HarmonyPrefix]
    private static void BleedingUpdate_Prefix(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
    {
        CharacterPatches.Override_BleedingUpdate(characterHealth, targetLimb, deltaTime);
    }

    // NPC Speak About Issues
    [HarmonyPatch(typeof(HumanAIController), "SpeakAboutIssues"), HarmonyPostfix]
    private static void SpeakAboutIssues_Postfix(HumanAIController __instance)
    {
        SpeakAboutIssuesPatch.Postfix_SpeakAboutIssues(__instance);
    }

    // Character Revive
    [HarmonyPatch(typeof(Character), nameof(Character.Revive)), HarmonyPrefix]
    private static void Revive_Prefix(Character __instance, ref bool removeAfflictions)
    {
        RevivePatch.Prefix_Revive(__instance, ref removeAfflictions);
    }

    // Surgery Table Controller Update
    [HarmonyPatch(typeof(Controller), "Update"), HarmonyPrefix] private static void SurgeryTableController_Update_Prefix(Controller __instance) 
    { 
        NTSurgeryTable.SurgeryTableControllerOverride.Prefix_Update(__instance); 
    }

    [HarmonyPatch(typeof(Controller), "Update"), HarmonyPostfix] private static void SurgeryTableController_Update_Postfix() 
    { 
        NTSurgeryTable.SurgeryTableControllerOverride.Postfix_Update(); 
    }

    // Surgery Table Character Selection
    [HarmonyPatch(typeof(Character), "SelectedCharacter", MethodType.Setter), HarmonyPrefix] private static bool SurgeryTable_SetSelectedCharacter_Prefix(Character __instance, Character value) 
    { 
        return NTSurgeryTable.SurgeryTableControllerOverride.Prefix_SetSelectedCharacter(__instance, value); 
    }

    // Adjust Aim wobble
    [HarmonyPatch(typeof(AnimController), "GetAimWobble", new Type[] { typeof(Limb), typeof(Limb), typeof(Item) }), HarmonyPrefix]
    private static bool GetAimWobble_Prefix(AnimController __instance, ref float __result)
    {
        return AimWobblePatch.Prefix_GetAimWobble(__instance, ref __result);
    }

    //// Fire Patch
    //[HarmonyPatch(typeof(Character), "ApplyStatusEffects", new Type[] { typeof(ActionType), typeof(float) }), HarmonyPostfix]
    //private static void ApplyStatusEffects_Postfix(Character __instance, ActionType actionType, float deltaTime)
    //{
    //    BurningPatch.Postfix_ApplyStatusEffects(__instance, actionType, deltaTime);
    //}
}