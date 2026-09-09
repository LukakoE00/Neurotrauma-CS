
using Barotrauma.LuaCs.Compatibility;

namespace Neurotrauma;

public class NTHumanUpdate
{
    public static IEventService EventService = LuaCsSetup.Instance.EventService;


    private static void PreHook(NTHuman human)
    {
        EventService.Call("Neurotrauma.HumanUpdate.PreHook", human);
        return;
    }

    private static void UpdateStats(NTHuman human)
    {
        return;
    }

    private static List<String> FetchAfflictions (NTHuman human, NTAfflictions.AfflictionPriority minimumPriority)
    {
        return new List<String> ();
    }

    private static void UpdateAfflictions(NTHuman human, List<String> afflictions)
    {
        EventService.Call("Neurotrauma.HumanUpdate.Update", human, afflictions);
        return;
    }

    private static void SetAfflictions(NTHuman human)
    {
        return;
    }

    private static void PostHook(NTHuman human)
    {
        EventService.Call("Neurotrauma.HumanUpdate.PostHook", human);
        return;
    }


    private static void UpdateHumans()
    {
        


        PreHook();
        UpdateStats();
        FetchAfflictions();
        UpdateAfflictions();
        SetAfflictions();
        PostHook();
    }
}