using static Neurotrauma.NTAfflictions;

namespace Neurotrauma;

public class NTHumanUpdate
{

    private static int UpdateIntervalHigh = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_High", (float)NTAfflictions.AfflictionPriority.HIGH));
    private static int UpdateIntervalMedium = UpdateIntervalHigh * 2;
    private static int UpdateIntervalLow = UpdateIntervalHigh * 3;
    private static int UpdateIntervalMonster = UpdateIntervalHigh * 3;                                 //(int)Math.Round(NTConfig.Get("NT_UpdateInterval_Monster", 120f));

    public static int GetUpdateInterval(NTAfflictions.AfflictionPriority priority)
    {
        return priority switch
        {
            NTAfflictions.AfflictionPriority.HIGH => UpdateIntervalHigh,
            NTAfflictions.AfflictionPriority.MEDIUM => UpdateIntervalMedium,
            NTAfflictions.AfflictionPriority.LOW => UpdateIntervalLow,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null)
        };
    }

    private static void RefreshIntervals()
    {
        UpdateIntervalHigh = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_High", (float) NTAfflictions.AfflictionPriority.HIGH));
        UpdateIntervalMedium = UpdateIntervalHigh * 2;
        UpdateIntervalLow = UpdateIntervalHigh * 3;
        UpdateIntervalMonster = UpdateIntervalHigh * 3;
        //UpdateIntervalMonster = (int)Math.Round(NTConfig.Get("NT_UpdateInterval_Monster", 120f));
    }

    // Each priority needs to have its own counter because they can now have intervals that are not multiples of the High priority one. update: nvmd but we'll keep this cause idk i've might have fucked things up
    private static int TickHigh = 0;
    private static int TickMedium = 0;
    private static int TickLow = 0;
    private static int TickMonster = 0;

    public static void ThinkUpdate()
    {
        // If game paused we just skip
        if ((!HF.InGame()) || HF.GameIsPaused()) return;

        List<NTAfflictions.AfflictionPriority> UpdatePriorities = new List<NTAfflictions.AfflictionPriority>();

        TickHigh++;
        TickMedium++;
        TickLow++;
        TickMonster++;

        if (TickHigh >= UpdateIntervalHigh)
        {
            UpdatePriorities.Add(NTAfflictions.AfflictionPriority.HIGH);
            TickHigh = 0;
        }

        if (TickMedium >= UpdateIntervalMedium)
        {
            UpdatePriorities.Add(NTAfflictions.AfflictionPriority.MEDIUM);
            TickMedium = 0;
        }

        if (TickLow >= UpdateIntervalLow)
        {
            UpdatePriorities.Add(NTAfflictions.AfflictionPriority.LOW);
            TickLow = 0;
        }

        bool updateMonsters = false;

        if (TickMonster >= UpdateIntervalMonster)
        {
            updateMonsters = true;
            TickMonster = 0;
        }

        if (UpdatePriorities.Count == 0) return;

        if (!NTConfig.Get("NT_Calculations", true)) return; // Check the config.

        // reset the intervals as what they are in the config. for optimization we could only refresh them on level load?

        int index = 1;
        foreach (Character character in Character.CharacterList)
        {
            if (character == null || character.isDead || character.IdFreed) continue;

            double Delay = (((index + 1) / NTHuman.NTHumans.Count) * UpdateIntervalHigh * 1000);

            if (character.IsHuman)
            { 
                UpdateHuman(UpdatePriorities, character, Delay);

            } else if (updateMonsters)
            {
                UpdateMonster(character, Delay);
            }

            index++;
        }

        RefreshIntervals();

    }

    private static void UpdateHuman(List<NTAfflictions.AfflictionPriority> priorities, Character character, double delay)
    {

        NTHuman? ntHuman = NTHuman.getNTHumanFromCharacter(character);

        if (ntHuman == null)
        {
            new NTHuman(character);
            return;
        }

        if (!ntHuman.HasAffliction("luabotomy")) return;

        // Delay our update to prevent sutters.

        LuaCsSetup.Instance.Timer.Wait((params object[] _) => {
            if (ntHuman != null && HF.IsCharacterValid(ntHuman.Human)) // Verify this character exists.
            {
                ntHuman.PreHook();
                ntHuman.UpdateStats(((float)NTHumanUpdate.GetUpdateInterval(AfflictionPriority.HIGH)) / 60f);
                List<KeyValuePair<string, LimbType>> aff = ntHuman.FetchAfflictions(priorities);
                ntHuman.UpdateAfflictions(aff);
                ntHuman.PostHook();
            }
        }, (int)delay);
    }



    private static void UpdateMonster(Character Monster, double delay)
    {

        LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
        {
        if (Monster != null && HF.IsCharacterValid(Monster)) // Verify this character exists.
        {
            double BloodLoss = HF.GetAfflictionStrength(Monster, "bloodloss", 0);
            double OxygenLow = HF.GetAfflictionStrength(Monster, "oxygenlow", 0);

            if (BloodLoss > 0)
            {
                HF.AddAffliction(Monster, "organdamage", (float)BloodLoss * 2, Monster);
                HF.SetAffliction(Monster, "bloodloss", 0, Monster, (float)BloodLoss);
            }
            else if (OxygenLow > 50)
            {
                HF.AddAffliction(Monster, "organdamage", (float)(OxygenLow - 50) * 2, Monster);
                HF.SetAffliction(Monster, "oxygenlow", 50, Monster, (float)OxygenLow);


            }
        }
        }, (int)delay);



        
    }
}