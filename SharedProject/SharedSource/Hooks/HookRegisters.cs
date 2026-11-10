using Barotrauma;
using Barotrauma.Networking;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
#pragma warning disable CS0618

namespace Neurotrauma;

public static class Hooks
{
    public static void Register()
    {
        LuaCsSetup.Instance.EventService.Add("think", "NTCS.ThinkUpdate", OnThinkUpdate);
        LuaCsSetup.Instance.EventService.Add("changeFallDamage", "NTCS.FallDamage", OnFallDamageReceived);
        LuaCsSetup.Instance.EventService.Add("character.created", "NTCS.CharacterCreated", OnCharacterCreated);
        LuaCsSetup.Instance.EventService.Add("character.death", "NTCS.CharacterDeath", OnCharacterDeath);
        LuaCsSetup.Instance.EventService.Add("character.CPRSuccess", "NTCS.CPRSuccess", OnCPRSuccess);
        LuaCsSetup.Instance.EventService.Add("character.CPRFailed", "NTCS.CPRFailed", OnCPRFailure);
        LuaCsSetup.Instance.EventService.Add("NTCS.CauseScreams", "NTCS.CauseScreams", OnScreamTriggered);
        LuaCsSetup.Instance.EventService.Add("NTCS.SpawnMedicalStarterCrate", "NTCS.SpawnMedicalStarterCrate", OnMedStarterCrateSpawn);
        LuaCsSetup.Instance.EventService.Add("character.giveJobItems", "NTCS.GiveHealthscannerBatteries", OnGiveJobItems);
        LuaCsSetup.Instance.EventService.Add("NT.multiscalpel.incision", "NTCS.multiscalpel.incision", OnMultiscalpelIncision);
        LuaCsSetup.Instance.EventService.Add("NT.multiscalpel.bandage", "NTCS.multiscalpel.bandage", OnMultiscalpelBandage);
        LuaCsSetup.Instance.EventService.Add("NT.multiscalpel.speedflex", "NTCS.multiscalpel.speedflex", OnMultiscalpelSpeedflex);
        LuaCsSetup.Instance.EventService.Add("OnInsertedIntoBloodAnalyzer", "NTCS.BloodAnalyzer", OnBloodAnalyzer);
        LuaCsSetup.Instance.EventService.Add("surgerytable.update", "NTCS.SurgeryTable", OnSurgeryTableUpdate);
    }

    // All functions that should trigger every tick.
    public static object? OnThinkUpdate(params object[] args)
    {
        NTHumanUpdate.ThinkUpdate();
        return null;
    }

    // All functions that should trigger when falldamage is received.
    public static object? OnFallDamageReceived(params object[] args)
    {
        NTFallDamage.OnChangeFallDamage((float)args[0], (Character)args[1], (Vector2)args[2], (Vector2)args[3]);
        return null;
    }

    // All functions that should trigger on character creation.
    public static object? OnCharacterCreated(params object[] args)
    {
        CharacterEvents.OnCharacterCreated(args);
        NTBloodTypes.AddBloodAndImmunityToCharacter(args);
        return null;
    }

    // All functions that should trigger on character death.
    public static object? OnCharacterDeath(params object[] args)
    {
        CharacterEvents.OnCharacterDeath(args);
        return null;
    }

    // All functions that should trigger on successful CPR application.
    public static object? OnCPRSuccess(params object[] args)
    {
        CPR.OnCPRSuccess(args);
        return null;
    }

    // All functions that should trigger on failed CPR application.
    public static object? OnCPRFailure(params object[] args)
    {
        CPR.OnCPRFailure(args);
        return null;
    }

    // All functions that should trigger when Screams get triggered.
    public static object? OnScreamTriggered(params object[] args)
    {
        Screaming.DoScreaming(args);
        return null;
    }

    // All functions that should trigger when Screams get triggered.
    public static object? OnMedStarterCrateSpawn(params object[] args)
    {
        LootCrates.OnMedStarterCrateSpawn(args);
        return null;
    }

    // All functions that should trigger when Screams get triggered.
    public static object? OnGiveJobItems(params object[] args)
    {
        LootCrates.OnGiveJobItems(args);
        return null;
    }

    // All functions that should trigger when the multiscalpel performs an incision.
    public static object? OnMultiscalpelIncision(params object[] args)
    {
        var item = args[2] as Item;
        if (item != null) NTMultiscalpel.SetMultiscalpelFunction(item, "incision");
        return null;
    }

    // All functions that should trigger when the multiscalpel applies bandages.
    public static object? OnMultiscalpelBandage(params object[] args)
    {
        var item = args[2] as Item;
        if (item != null) NTMultiscalpel.SetMultiscalpelFunction(item, "bandage");
        return null;
    }

    // All functions that should trigger when the multiscalpel uses speedflex.
    public static object? OnMultiscalpelSpeedflex(params object[] args)
    {
        var item = args[2] as Item;
        if (item != null) NTMultiscalpel.SetMultiscalpelFunction(item, "speedflex");
        return null;
    }

    // All functions that should trigger when a blood analyzer is used.
    public static object? OnBloodAnalyzer(params object[] args)
    {
        NTBloodTypes.OnBloodAnalyzer(args);
        return null;
    }

    // All functions that should trigger when a surgery table updates.
    public static object? OnSurgeryTableUpdate(params object[] args)
    {
        NTSurgeryTable.OnSurgeryTableUpdate(args);
        return null;
    }
}