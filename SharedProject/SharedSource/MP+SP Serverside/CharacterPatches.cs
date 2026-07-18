namespace Neurotrauma
{
    public class CharacterPatches
    {
        private static Harmony harmony;

        public static void InitCharacterPatches()
        {
            harmony = new Harmony("CharacterPatches");
            DisableAimingPenalties();
            ApplyBurningOnFire();
        }

        public static void Dispose()
        {
            if (harmony != null) harmony.UnpatchSelf();
        }

        public static void DisableAimingPenalties()
        {
            var Method_GetAimWobble = AccessTools.Method(typeof(AnimController), "GetAimWobble", new[] { typeof(Limb), typeof(Limb), typeof(Item) });

            harmony.Patch(Method_GetAimWobble, prefix: new HarmonyMethod(typeof(AimWobblePatch), nameof(AimWobblePatch.Prefix_GetAimWobble)));
        }

        public static void ApplyBurningOnFire()
        {
            var Method_ApplyStatusEffects = AccessTools.Method(typeof(Character), "ApplyStatusEffects", new[] { typeof(ActionType), typeof(float) });

            harmony.Patch(Method_ApplyStatusEffects, postfix: new HarmonyMethod(typeof(BurningPatch), nameof(BurningPatch.Postfix_ApplyStatusEffects)));
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
                HF.AddAfflictionLimb(__instance, "burning", __instance.AnimController.MainLimb.type, deltaTime * 5f, null);
            }
        }
    }
}