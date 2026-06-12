using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using LightInject;
using MonoMod.RuntimeDetour;
using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma;

public class HumanUpdate
{
    private static int UpdateCooldown = 0;
    private static readonly int UpdateIntervalHigh = (int)AfflictionPriority.HIGH; // 120 = 2s
    private static readonly int UpdateIntervalMedium = (int)AfflictionPriority.MEDIUM; // 240 = 4s
    private static readonly int UpdateIntervalLow = (int)AfflictionPriority.LOW; // 480 = 8s
    static private List<NTHuman> UpdatingHumans = new List<NTHuman>();
    static private List<NTMonster> UpdatingMonsters = new List<NTMonster>();

    // ---------------------------------------- NT Human Update Classes -------------------------------------------------- \\

    public class NTHuman(Character Human)
    {

        public Character Human = Human; // Our Human Ref
        public CharacterStats LocalStats = new CharacterStats();
        public CharacterAfflictions LocalAfflictions = new CharacterAfflictions(Human);

        public CharacterStats? GetStats()
        {
            return LocalStats;
        }

        public void Update(List<AfflictionPriority> Priorities)
        {
            foreach (KeyValuePair<string,NTNonLimbAffliction> Pair in LocalAfflictions.UpdatingAfflictions)
            {
                if (Pair.Key != null)
                {
                    // Fetch the data of the affliction
                    Dictionary<string, double> AffStrengths = LocalAfflictions.UpdatingAffStrength;
                    string ID = Pair.Key;
                    NTNonLimbAffliction Aff = Pair.Value;
                    double CurrentAffStrength = AffStrengths[ID];

                    // Add our affliction Depedencies
                    foreach (NTAffliction Dependency in Aff.DependentAfflictions)
                    {
                        if (!LocalAfflictions.UpdatingAfflictions.ContainsValue((NTNonLimbAffliction)Dependency))
                        {

                        }
                    }

                    Aff.UpdateAction(this, ID, LimbType.Torso, LocalAfflictions.UpdatingAffStrength);
                }
            }

            foreach (KeyValuePair<string, NTLimbAffliction> Pair in LocalAfflictions.UpdatingLimbAfflictions)
            {
                if (Pair.Key != null)
                {
                }
            }

            foreach (KeyValuePair<string, NTBloodAffliction> Pair in LocalAfflictions.UpdatingBloodAfflictions)
            {
                if (Pair.Key != null)
                {
                }
            }

        }

        public class CharacterAfflictions(Character Human)
        {
            public Character Human = Human; // Our Human Ref
            public Dictionary<string,NTNonLimbAffliction> UpdatingAfflictions = new(); // Stores the ID's of our updating afflictions.
            public Dictionary<string, double> UpdatingAffStrength = new(); // Stores the ID's of our updating afflictions strength.

            public Dictionary<string, NTLimbAffliction> UpdatingLimbAfflictions = new(); // Stores the ID's of our updating (Limb) afflictions.
            public Dictionary<string, Dictionary<LimbType, double>> UpdatingLimbAffStrength = new(); // Stores the ID's of our updating afflictions strength.

            public Dictionary<string, NTBloodAffliction> UpdatingBloodAfflictions = new(); // Stores the ID's of our updating (blood) afflictions.
            public Dictionary<string, double> UpdatingBloodAffStrength = new(); // Stores the ID's of our updating (blood) afflictions strength.

            public void AddNonLimbAffliction(string ID, NTNonLimbAffliction NTNonLimbAff, double Strength)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingAfflictions[ID] = NTNonLimbAff;
                    UpdatingAffStrength[ID] = Strength;
                }
            }

            public void AddLimbAffliction(string ID, NTLimbAffliction NTLimbAff, double Strength, LimbType Limb)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingLimbAfflictions[ID] = NTLimbAff;
                    UpdatingLimbAffStrength[ID][Limb] = Strength;
                }
            }

            public void AddBloodAffliction(string ID, NTBloodAffliction NTBloodAff, double Strength)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingBloodAfflictions[ID] = NTBloodAff;
                    UpdatingBloodAffStrength[ID] = Strength;
                }
            }

            public void RemoveAffliction(string ID, NTAffliction Aff, LimbType Limb = LimbType.Torso)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    if (Aff is NTNonLimbAffliction)
                    {
                        UpdatingAfflictions.Remove(ID);
                        UpdatingAffStrength.Remove(ID);
                        return;
                    }
                    if (Aff is NTLimbAffliction)
                    {
                        UpdatingLimbAfflictions.Remove(ID);
                        UpdatingLimbAffStrength[ID].Remove(Limb);
                        return;
                    }
                    if (Aff is NTBloodAffliction)
                    {
                        UpdatingBloodAfflictions.Remove(ID);
                        UpdatingBloodAffStrength.Remove(ID);
                        return;
                    }

                }
            }

            public List<string> GetUpdatingAfflictons()
            {
                return UpdatingAfflictions.Keys.ToList();
            }

            public NTAffliction GetAff(string ID)
            {
                if (UpdatingAfflictions.ContainsKey(ID))
                {
                    return UpdatingAfflictions[ID];
                }
                return UpdatingLimbAfflictions[ID];
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
            public void RecalculateSingle(string id, HumanUpdate.NTHuman character)
            {

                if (NTStats.Stats[id] != null)
                {
                    NTStats.Stats[id].Recalculate(character);
                }
            }

            // If we need to recalculate every stats for a character we can call this
            public void RecalculateAll(HumanUpdate.NTHuman character)
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

    public static void AddCharacterToUpdate(CharacterPrefab prefab, Vector2 position, string seed, CharacterInfo characterInfo, ushort id, bool isRemotePlayer, bool hasAi, bool createNetworkEvent, RagdollParams ragdoll, bool spawnInitialItems)
    {
        HF.Print("Add Character");
        if (prefab == null || characterInfo == null) { return; }
        Character NewCharacter = characterInfo.Character;
        if (NewCharacter == null) { return; }
        if (NewCharacter.IsHuman)
        {
            HF.Print($"Adding the following Character {NewCharacter.Name} !");
            AddHumanToUpdate(NewCharacter);
        }
        else
        {
            AddMonsterToUpdate(NewCharacter);
        }
    }

    public static void RemoveCharacterFromUpdate(Character target)
    {
        HF.Print("Removing Character");
        if (target is Character)
        {
            Character NewCharacter = (Character)target;
            if (NewCharacter.IsHuman)
            {
                RemoveHumanFromUpdate(NewCharacter);
            }
            else
            {
                RemoveMonsterFromUpdate(NewCharacter);
            }
        }
    }

    public static void AddHumanToUpdate(Character AddedCharacter)
    {
        NTHuman NewNTHuman = new NTHuman(AddedCharacter); // Hopefully this wont create a memory leak.
        if (!UpdatingHumans.Contains(NewNTHuman))
        {
            UpdatingHumans.Add(NewNTHuman);
        }
    }

    public static void RemoveHumanFromUpdate(Character RemovingCharacter) // Probably a better way to do this.
    {
        NTHuman HumanToRemove = null; // We store the index of what to remove so we don't remove while iterating.
        foreach (NTHuman Human in UpdatingHumans)
        {
            if (Human.Human == RemovingCharacter)
            {
                HumanToRemove = Human;
                break;
            }
        }
        if (HumanToRemove != null)
        {
            UpdatingHumans.Remove(HumanToRemove);
        }
    }

    public static void AddMonsterToUpdate(Character AddedMonster)
    {
        if (!AddedMonster.IsHuman)
        {
            NTMonster NewNTMonster = new NTMonster(AddedMonster);
            if (!UpdatingMonsters.Contains(NewNTMonster))
            {
                UpdatingMonsters.Add(NewNTMonster);
            }
        }
    }

    public static void RemoveMonsterFromUpdate(Character RemovingMonster) // Probably a better way to do this.
    {
        NTMonster MonsterToRemove = null; // We store the index of what to remove so we don't remove while iterating.
        foreach (NTMonster Monster in UpdatingMonsters)
        {
            if (Monster.Monster == RemovingMonster)
            {
                MonsterToRemove = Monster;
                break;
            }
        }
        if (MonsterToRemove != null)
        {
            UpdatingMonsters.Remove(MonsterToRemove);
        }
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

        //foreach (Character c in CHList) // This is the old fetching Character for Update system. We're now using a hook method instead. Leaving this here so we can go back incase it breaks.
        //{
            //if (c.isDead) continue; // Skip to next iteration

            //if (c.IsHuman && c.Enabled)
            //{
                //AddHumanToUpdate(c);
            //}
            //else
            //{
                //AddMonsterToUpdate(c);
            //}
        //}

        UpdateHumans(priorities);

        UpdateMonsters(priorities);
    }

    private void UpdateHumans(List<AfflictionPriority> priorities)
    {
        foreach (NTHuman Human in UpdatingHumans)
        {

            Human.Update(priorities);

        }
    }

    private void UpdateMonsters(List<AfflictionPriority> priorities)
    {

    }

    private static void UpdateMonster(Character character)
    {

    }
}