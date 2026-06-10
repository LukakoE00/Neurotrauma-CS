using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using MonoMod.RuntimeDetour;

namespace Neurotrauma;

class HumanUpdate
{
    private static int UpdateCooldown = 0;
    private static readonly int UpdateIntervalHigh = (int) AfflictionPriority.HIGH; // 120 = 2s
    private static readonly int UpdateIntervalMedium = (int) AfflictionPriority.MEDIUM; // 240 = 4s
    private static readonly int UpdateIntervalLow = (int) AfflictionPriority.LOW; // 480 = 8s
    //private static int DeltaTime

    

    private static void UpdateMonster(Character character)
    {
        
    }

    private static void UpdateHuman(Character character, List<AfflictionPriority> priorities)
    {
        LuaCsLogger.Log(character.Prefab.Identifier.ToString());
    }

    


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
    public void OnUpdate(double fixedDeltaTime)
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

        UpdateCooldown += 1;
    }

    private static void Update(List<AfflictionPriority> priorities)
    {
        List<Character> HumanList = new List<Character>();
        List<Character> MonsterList = new List<Character>();

        Character.CharacterList.ForEach(c =>
        {
            if (c.isDead) return;

            if (c.IsHuman && c.Enabled)
            {
                HumanList.Add(c);
            }
            else
            {
                MonsterList.Add(c);
            }
        });

        foreach (var human in HumanList)
        {

            Thread UpdateHumanT = new Thread(() => UpdateHuman(human, priorities));

            UpdateHumanT.Start();

        }

        foreach (var monster in MonsterList)
        {

        }

    }
}