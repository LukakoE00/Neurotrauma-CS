namespace Neurotrauma
{
    public class CharacterPatches
    {
        public static bool Override_BleedingUpdate(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
        {
            return false;
        }
    }

    // Formerly CharacterPatches.lua
    public static class AimWobblePatch
    {
        public static bool Prefix_GetAimWobble(AnimController __instance, ref float __result)
        {
            if (HF.HasAffliction(__instance.Character, "analgesia", 20f))
            {
                __result = 0f;
                return false;
            }

            return true;
        }
    }

    // Formerly OnFire.lua
    public static class BurningPatch
    {
        public static void Postfix_ApplyStatusEffects(Character __instance, ActionType actionType, float deltaTime)
        {
            if (actionType != ActionType.OnFire) return;
            if (!HF.IsCharacterValid(__instance) || __instance.IsDead || __instance.Removed) return;

            void ApplyBurn(Character character, LimbType limbType)
            {
                HF.AddAfflictionLimb(character, "burning", limbType, deltaTime * 3f, null);
            }

            if (__instance.IsHuman)
            {
                if (!HF.HasAffliction(__instance, "luabotomy")) HF.SetAffliction(__instance, "luabotomy", 1f, null, 0);

                ApplyBurn(__instance, LimbType.Torso);
                ApplyBurn(__instance, LimbType.Head);
                ApplyBurn(__instance, LimbType.LeftArm);
                ApplyBurn(__instance, LimbType.RightArm);
                ApplyBurn(__instance, LimbType.LeftLeg);
                ApplyBurn(__instance, LimbType.RightLeg);
            }
            else
            {
                Limb mainLimb = __instance.AnimController?.MainLimb;
                if (mainLimb == null) return;

                HF.AddAfflictionLimb(__instance, "burning", mainLimb.type, deltaTime * 5f, null);
            }
        }
    }
}