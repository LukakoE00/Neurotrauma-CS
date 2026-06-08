using System.Reflection.Metadata.Ecma335;

namespace Neurotrauma;

class HumanUpdate
{
    private static int UpdateCooldown = 0;
    private static int UpdateIntervalHigh = (int) AfflictionPriority.HIGH; // 120 = 2s
    private static int UpdateIntervalMedium = (int) AfflictionPriority.MEDIUM; // 240 = 4s
    private static int UpdateIntervalLow = (int)AfflictionPriority.LOW; // 480 = 8s
    //private static int DeltaTime

    private static void UpdateMonster(Character character)
    {

    }

    private static void UpdateHuman(Character character)
    {

    }

    private static void Update(Character character)
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


    }

}