using FarseerPhysics.Collision;
using MonoMod.RuntimeDetour;
using MoonSharp.Interpreter;
using Neurotrauma;

namespace Neurotrauma
{
    public class CharacterPatches
    {
        private static Harmony harmony;

        public static void InitCharacterPatches()
        {
            harmony = new Harmony("CharacterPatches");
            DisableAimingPenalties();
        }

        public static void DisableAimingPenalties()
        {
            var Method_GetAimWobble = AccessTools.Method(typeof(AnimController), "GetAimWobble", new[] { typeof(Limb), typeof(Limb), typeof(Item) });

            harmony.Patch(Method_GetAimWobble, prefix: new HarmonyMethod(typeof(AimWobblePatch), nameof(AimWobblePatch.Prefix_GetAimWobble)));
        }
    }

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
}