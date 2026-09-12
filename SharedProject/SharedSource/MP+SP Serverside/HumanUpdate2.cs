
using Barotrauma.LuaCs.Compatibility;

namespace Neurotrauma;

public class NTHumanUpdate
{
    /*
     * NTConfig.Get("NT_UpdateInterval_High", 120)
     * NTConfig.Get("NT_UpdateInterval_Medium", 240)
     * NTConfig.Get("NT_UpdateInterval_Low", 360)
     */

    private static int UpdateIntervalHigh = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_High", NTAfflictions.AfflictionPriority.HIGH));
    private static int UpdateIntervalMedium = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_Medium", NTAfflictions.AfflictionPriority.MEDIUM));
    private static int UpdateIntervalLow = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_Low", NTAfflictions.AfflictionPriority.LOW));

    private static int Interval = (int) Math.Round(NTConfig.Get("NT_UpdateInterval", 120));
    private static int Tick = 0;
    private static int UpdateTick = 0;

    private static void RefreshIntervals()
    {
        UpdateIntervalHigh = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_High", NTAfflictions.AfflictionPriority.HIGH));
        UpdateIntervalMedium = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_Medium", NTAfflictions.AfflictionPriority.MEDIUM));
        UpdateIntervalLow = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_Low", NTAfflictions.AfflictionPriority.LOW));
    }


    public static void ThinkUpdate()
    {
        // If game paused we just skip
        if ((!HF.InGame()) || HF.GameIsPaused()) return;

        Tick--; // Decrement our tick.
        if (!(Tick < 0)) { return; }
        else { Tick = Interval; }

        if (!NTConfig.Get("NT_Calculations", true)) return; // Check the config.

        Priorities = GetLowestPriority(UpdateTick);
        UpdateTick++;

        Update(Priorities);
    }



    private static void UpdateHumans()
    {
        



    }
}