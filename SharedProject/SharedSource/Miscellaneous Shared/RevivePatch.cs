using Barotrauma;
using HarmonyLib;
using Neurotrauma;
using System;
using System.Collections.Generic;
using System.Linq;

public static class RevivePatch
{
    private static Harmony harmony;

    public static void Init()
    {
        harmony = new Harmony("NTRevivePatch");

        var Revive = AccessTools.Method(typeof(Character), nameof(Character.Revive));

        harmony.Patch(Revive, prefix: new HarmonyMethod(typeof(RevivePatch), nameof(Prefix_Revive)));
    }

    public static void Prefix_Revive(Character __instance, ref bool removeAfflictions)
    {
        if (!removeAfflictions) 
        {
            return; 
        }

        var CharacterHealth = __instance.CharacterHealth;
        if (CharacterHealth == null) 
        { 
            return; 
        }

        // Dont blanket-remove all the afflictions since we want to exclude our own.
        removeAfflictions = false;

        var AfflictionsToKeep = new HashSet<string>(NTConfig.Get("NT_ReviveExcluded", Enumerable.Empty<string>()) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

        // Take all afflictions on a character, subtract the exclusions then NUKE THEM!!!!!!!!!!!!!!!!!!!
        var AfflictionsToRemove = CharacterHealth.GetAllAfflictions().Where(Affliction => Affliction != null && !AfflictionsToKeep.Contains(Affliction.Identifier.Value)).Select(Affliction => Affliction.Identifier).Distinct().ToList();

        foreach (var id in AfflictionsToRemove)
        {
            CharacterHealth.ReduceAfflictionOnAllLimbs(id, 100000f);
        }

        // It does this normally on revive, so we pass it along as well :3
        __instance.SetAllDamage(0f, 0f, 0f);
        __instance.Bloodloss = 0f;
        __instance.SetStun(0f, true);
    }
}