using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using MonoMod.RuntimeDetour;
using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma;

class HumanUpdate
{
    private static int UpdateCooldown = 0;
    private static readonly int UpdateIntervalHigh = (int)AfflictionPriority.HIGH; // 120 = 2s
    private static readonly int UpdateIntervalMedium = (int)AfflictionPriority.MEDIUM; // 240 = 4s
    private static readonly int UpdateIntervalLow = (int)AfflictionPriority.LOW; // 480 = 8s
    private List<NTHuman> UpdatingHumans = new List<NTHuman>();
    private List<NTMonster> UpdatingMonsters = new List<NTMonster>();

    // ---------------------------------------- NT Human Update Classes -------------------------------------------------- \\

    class NTHuman(Character Human)
    {
        public Character Human = Human; // Our Human Ref
        private static CharacterStats LocalStats = new CharacterStats();
        private static CharacterAfflictions LocalAfflictions = new CharacterAfflictions();

        public static CharacterStats? GetStats()
        {
            return LocalStats;
        }

        public class CharacterAfflictions
        {
            private static List<string> UpdatingAfflictions = new List<string>(); // Stores the ID's of our updating afflictions.

            public static void RegisterAffliction(string ID)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingAfflictions.Add(ID);
                }
            }

            public static void RemoveAffliction(string ID)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingAfflictions.Remove(ID);
                }
            }

            public static List<string> GetUpdatingAfflictons()
            {
                return UpdatingAfflictions;
            }
        }

        public class CharacterStats
        {
            private Dictionary<string, int> Stats { get; }

            public CharacterStats()
            {
                Stats = new Dictionary<string, int>();
            }

            // If you want to recalculate a single stat
            public void RecalculateSingle(string id, NTUpdateFunctionInfos character)
            {

                if (NTStats.Stats[id] != null)
                {
                    NTStats.Stats[id].Recalculate(character);
                }
            }

            // If we need to recalculate every stats for a character we can call this
            public void RecalculateAll(NTUpdateFunctionInfos character)
            {
                foreach (var stat in NTStats.Stats)
                {
                    stat.Value.Recalculate(character);
                }
            }
        }

    }

    class NTMonster(Character Monster) // To Do
    {
        public Character Monster = Monster; // Our Monster Ref

    }


    // ---------------------------------------- The Human Update -------------------------------------------------- \\

    // Returns a list 
    private static List<AfflictionPriority> GetLowestPriority(int cd)
    {
        List<AfflictionPriority> output = [];

        if (cd % UpdateIntervalLow == 0)
        {
            output.Add(AfflictionPriority.LOW);
            output.Add(AfflictionPriority.MEDIUM);
            output.Add(AfflictionPriority.HIGH);
            UpdateCooldown = 0;

        } else if (cd % UpdateIntervalMedium == 0)
        {
            output.Add(AfflictionPriority.MEDIUM);
            output.Add(AfflictionPriority.HIGH);

        } else if (cd % UpdateIntervalHigh == 0)
        {
            output.Add(AfflictionPriority.HIGH);
        }

        return output;
    }

    private int Interval = 120;
    private int Tick = 0;
    private double NTDeltaTime = UpdateIntervalHigh / 120;
    // Gets called 60 times a second
    public void ThinkUpdate(double fixedDeltaTime)
    {
        // If game paused we just skip
        if (HF.GameIsPaused()) return;

        Tick--; // Decrement our tick.
        if (!(Tick < 0)) { return; }
        else { Tick = Interval; HF.Print("Human Update Tick"); }

        // We check if timer is up
        List<AfflictionPriority> checkedPriorities = GetLowestPriority(UpdateCooldown);
        if (checkedPriorities.Count == 0) return;

        Update(checkedPriorities);

        UpdateCooldown++;
    }

    private void Update(List<AfflictionPriority> priorities)
    {
        List<Character> CHList = Character.CharacterList;

        foreach (Character c in CHList)
        {
            if (c.isDead) continue; // Skip to next iteration

            if (c.IsHuman && c.Enabled)
            {
                NTHuman NewNTHuman = new NTHuman(c); // Hopefully this wont create a memory leak.
                if (!UpdatingHumans.Contains(NewNTHuman))
                {
                    UpdatingHumans.Add(NewNTHuman);
                }
            }
            else
            {
                if (!c.IsHuman)
                {
                    NTMonster NewNTMonster = new NTMonster(c);
                    if (!UpdatingMonsters.Contains(NewNTMonster))
                    {
                        UpdatingMonsters.Add(NewNTMonster);
                    }
                }
            }
        }

        UpdateHumans(priorities);

        UpdateMonsters(priorities);
    }

    private void UpdateHumans(List<AfflictionPriority> priorities)
    {
        foreach (NTHuman Human in UpdatingHumans)
        {

            UpdateHuman(Human, priorities);

        }
    }

    private static void UpdateHuman(NTHuman Character, List<AfflictionPriority> priorities)
    {
        LuaCsLogger.Log(Character.Human.Prefab.Identifier.ToString());
    }

    private void UpdateMonsters(List<AfflictionPriority> priorities)
    {

    }

    private static void UpdateMonster(Character character)
    {

    }
}