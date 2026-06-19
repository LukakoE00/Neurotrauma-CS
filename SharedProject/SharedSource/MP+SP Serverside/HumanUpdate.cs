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
    static private Dictionary<Character, NTHuman> UpdatingHumans = new();
    static private List<NTMonster> UpdatingMonsters = new List<NTMonster>();

    // ---------------------------------------- NT Human Update Classes -------------------------------------------------- \\

    public class NTHuman
    {
        public NTHuman(Character NewHuman)
        {
            Human = NewHuman;
            LocalStats = new CharacterStats(this);
            LocalAfflictions = new CharacterAfflictions(NewHuman,this);
        }

        public Character Human; // Our Human Ref
        public CharacterStats LocalStats;
        public CharacterAfflictions LocalAfflictions;

        public Dictionary<string, CharacterAfflictions.NTHumanNonLimbAffData> GetAffData()
        {
            return LocalAfflictions.UpdatingAfflictions;
        }

        public Dictionary<string, CharacterAfflictions.NTHumanLimbAffData> GetLimbAffData()
        {
            return LocalAfflictions.UpdatingLimbAfflictions;
        }

        public Dictionary<string, CharacterAfflictions.NTHumanBloodAffData> GetBloodAffData()
        {
            return LocalAfflictions.UpdatingBloodAfflictions;
        }

        public CharacterAfflictions? GetAfflictions()
        {
            return LocalAfflictions;
        }

        public CharacterStats? GetStats()
        {
            return LocalStats;
        }

        public void Update(List<AfflictionPriority> Priorities)
        {
            if (Human == null) return;
            foreach (KeyValuePair<string,CharacterAfflictions.NTHumanNonLimbAffData> Pair in LocalAfflictions.UpdatingAfflictions)
            {
                if (Pair.Key != null)
                {
                    // Fetch the data of the affliction
                    string ID = Pair.Key;
                    CharacterAfflictions.NTHumanNonLimbAffData AffData = Pair.Value;
                    NTNonLimbAffliction Aff = AffData.AffTemplate;

                    if (!Priorities.Contains(Aff.Priority)) continue; // Skip to the next affliction, we don't have the same priority currently.

                    AffData.Strength = HF.GetAfflictionStrength(Human, ID);
                    double PrevStrength = AffData.Strength;
                    Aff.UpdateAction(this, ID, LimbType.Torso, AffData);
                    HF.ApplyAfflictionChange(Human, ID, (float)AffData.Strength, (float)PrevStrength, (float)AffData.AffTemplate.MinStrength, (float)AffData.AffTemplate.MaxStrength);
                }
            }

            foreach (KeyValuePair<string, CharacterAfflictions.NTHumanLimbAffData> Pair in LocalAfflictions.UpdatingLimbAfflictions)
            {
                if (Pair.Key != null)
                {
                    double TotalStrength = 0;

                    foreach (LimbType Limb in HF.LimbsToCheck)
                    {
                        // Fetch the data of the affliction
                        string ID = Pair.Key;
                        CharacterAfflictions.NTHumanLimbAffData AffData = Pair.Value;
                        NTLimbAffliction Aff = AffData.AffTemplate;

                        if (!Priorities.Contains(Aff.Priority)) continue;

                        AffData.Strength[Limb] = HF.GetAfflictionStrengthLimb(Human,Limb,ID);
                        double PrevStrength = AffData.Strength[Limb];
                        Aff.UpdateAction(this, ID, Limb, AffData);
                        HF.ApplyAfflictionChangeLimb(Human, Limb, ID, (float)AffData.Strength[Limb], (float)PrevStrength, (float)AffData.AffTemplate.MinStrength, (float)AffData.AffTemplate.MaxStrength);
                        TotalStrength += AffData.Strength[Limb];
                    }
                }
            }

            foreach (KeyValuePair<string, CharacterAfflictions.NTHumanBloodAffData> Pair in LocalAfflictions.UpdatingBloodAfflictions)
            {
                if (Pair.Key != null)
                {
                    // Fetch the data of the affliction
                    string ID = Pair.Key;
                    CharacterAfflictions.NTHumanBloodAffData AffData = Pair.Value;
                    NTBloodAffliction Aff = AffData.AffTemplate;

                    if (!Priorities.Contains(Aff.Priority)) continue;

                    AffData.Strength = HF.GetAfflictionStrength(Human, ID);
                    double PrevStrength = AffData.Strength;
                    Aff.UpdateAction(this, ID, LimbType.Torso, AffData);
                    HF.ApplyAfflictionChange(Human, ID, (float)AffData.Strength, (float)PrevStrength, (float)AffData.AffTemplate.MinStrength, (float)AffData.AffTemplate.MaxStrength);
                }
            }

        }

        public class CharacterAfflictions
        {

            public CharacterAfflictions(Character Human2, NTHuman C)
            {
                Human = Human2;
                CharacterNT = C;

                AddAfflictions(); // ADD OUR AFFLICTIONS YEAHHHHH
            }

            public class NTHumanNonLimbAffData(NTNonLimbAffliction Aff, string ID, double Strength = 0) // Stores our characters Aff Data
            {
                public NTNonLimbAffliction AffTemplate = Aff; // Stores our template. The reason we aren't just creating a new affliction for each character is performance. I'm pretty sure it's more peformance efficent to just reference our affliction.
                public double Strength = Strength;
                public double PrevStrength = 0;
                public string ID = ID;
            }

            public class NTHumanLimbAffData(NTLimbAffliction Aff, string ID, Dictionary<LimbType, double> Strength) // Stores our characters Aff Data
            {

                public NTLimbAffliction AffTemplate = Aff; // Stores our template.
                public Dictionary<LimbType,double> Strength = Strength; // Gotta be a dictionary so we can store strength of limbtypes.
                public Dictionary<LimbType, double> PrevStrength = new();
                public string ID = ID;
            }

            public class NTHumanBloodAffData(NTBloodAffliction Aff, string ID, double Strength = 0) // Stores our characters Aff Data
            {
                public NTBloodAffliction AffTemplate = Aff; // Stores our template.
                public double Strength = Strength;
                public double PrevStrength = 0;
                public string ID = ID;
            }

            public Character Human { get; set; } // Our Human Ref
            public NTHuman CharacterNT { get; set; } // Our NTHuman Ref

            public Dictionary<string, NTHumanNonLimbAffData> UpdatingAfflictions = new(); // Stores the ID's of our updating afflictions.
            public Dictionary<string, NTHumanLimbAffData> UpdatingLimbAfflictions = new(); // Stores the ID's of our updating (Limb) afflictions.
            public Dictionary<string, NTHumanBloodAffData> UpdatingBloodAfflictions = new(); // Stores the ID's of our updating (blood) afflictions.

            public void RegisterAffliction(string ID, NTAffliction Aff, double Strength)
            {
                if (Aff != null)
                {
                    if (Aff is NTNonLimbAffliction)
                    {
                        RegisterNonLimbAffliction(ID,(NTNonLimbAffliction)Aff, Strength);
                    }
                    if (Aff is NTBloodAffliction)
                    {
                        RegisterBloodAffliction(ID, (NTBloodAffliction)Aff, Strength);
                    }
                    if (Aff is NTLimbAffliction)
                    {
                        RegisterLimbAffliction(ID, (NTLimbAffliction)Aff, new Dictionary<LimbType, double>() { });
                    }
                }
            }

            public void RegisterNonLimbAffliction(string ID, NTNonLimbAffliction NTNonLimbAff, double Strength)
            {
                if (CharacterNT == null) return;
                if (NTAfflictions.HasAffliction(ID) && !UpdatingAfflictions.ContainsKey(ID))
                {

                        UpdatingAfflictions[ID] = new NTHumanNonLimbAffData(NTNonLimbAff, ID, Strength);

                }
            }

            public void RegisterLimbAffliction(string ID, NTLimbAffliction NTLimbAff, Dictionary<LimbType,double> Strength)
            {
                if (CharacterNT == null) return;
                if (NTAfflictions.HasAffliction(ID) && !UpdatingLimbAfflictions.ContainsKey(ID))
                {
                        UpdatingLimbAfflictions[ID] = new NTHumanLimbAffData(NTLimbAff, ID, Strength);
                }
            }

            public void RegisterBloodAffliction(string ID, NTBloodAffliction NTBloodAff, double Strength)
            {
                if (CharacterNT == null) return;
                if (NTAfflictions.HasAffliction(ID) && !UpdatingBloodAfflictions.ContainsKey(ID))
                {
                        UpdatingBloodAfflictions[ID] = new NTHumanBloodAffData(NTBloodAff, ID, Strength);
                }
            }

            public bool HasNonLimbAffliction(string ID)
            {
                return CharacterNT.LocalAfflictions.UpdatingAfflictions.ContainsKey(ID);
            }

            public bool HasLimbAffliction(string ID)
            {
                return CharacterNT.LocalAfflictions.UpdatingLimbAfflictions.ContainsKey(ID);
            }

            public bool HasBloodAffliction(string ID)
            {
                return CharacterNT.LocalAfflictions.UpdatingBloodAfflictions.ContainsKey(ID);
            }

            public void RemoveAffliction(string ID, NTAffliction Aff) // Should only be called at the end of a human update.
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    if (Aff is NTNonLimbAffliction)
                    {
                        UpdatingAfflictions.Remove(ID);
                        return;
                    }
                    if (Aff is NTLimbAffliction)
                    {
                        UpdatingLimbAfflictions.Remove(ID);
                        return;
                    }
                    if (Aff is NTBloodAffliction)
                    {
                        UpdatingBloodAfflictions.Remove(ID);
                        return;
                    }

                }
            }

            public void RemoveNonLimbAffliction(string ID)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingAfflictions.Remove(ID);
                }
            }

            public void RemoveLimbAffliction(string ID)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingLimbAfflictions.Remove(ID);
                }
            }

            public void RemoveBloodAffliction(string ID)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingBloodAfflictions.Remove(ID);
                }
            }

            public List<string> GetUpdatingAfflictions()
            {
                return UpdatingAfflictions.Keys.ToList();
            }

            public NTHumanNonLimbAffData GetNonLimbData(string ID)
            {
                if (UpdatingAfflictions.ContainsKey(ID))
                {
                    return UpdatingAfflictions[ID];
                }
                return null;
            }

            public NTHumanLimbAffData GetLimbData(string ID)
            {
                if (UpdatingLimbAfflictions.ContainsKey(ID))
                {
                    return UpdatingLimbAfflictions[ID];
                }
                return null;
            }

            public NTHumanBloodAffData GetBloodData(string ID)
            {
                if (UpdatingBloodAfflictions.ContainsKey(ID))
                {
                    return UpdatingBloodAfflictions[ID];
                }
                return null;
            }

            private string NTAfflictionToType(NTAffliction Aff)
            {
                if (Aff is NTNonLimbAffliction)
                {
                    return "NTNonLimbAffliction";
                }
                if (Aff is NTBloodAffliction)
                {
                    return "NTBloodAffliction";
                }
                return "NTLimbAffliction";
            }

            private void AddAfflictions()
            {
                foreach (KeyValuePair<string,NTAffliction> Pair in NTAfflictions.Afflictions)
                {
                    string ID = Pair.Key;
                    NTAffliction Aff = Pair.Value;
                    string Type = NTAfflictionToType(Aff);
                    switch (Type)
                    {
                        case "NTNonLimbAffliction":
                            RegisterNonLimbAffliction(ID, (NTNonLimbAffliction) Aff, 0);
                            break;

                        case "NTLimbAffliction":
                            RegisterLimbAffliction(ID, (NTLimbAffliction) Aff, HF.DefaultLimbAffStrengths);
                            break;

                        case "NTBloodAffliction":
                            RegisterBloodAffliction(ID, (NTBloodAffliction)Aff, 0);
                            break;
                    }
                }
            }
        }

        public class CharacterStats
        {
            public CharacterStats(NTHuman C)
            {
                foreach (KeyValuePair<string,NTStat> Pair in NTStats.Stats)
                {
                    if (Pair.Value is NTStatDouble)
                    {
                        NTStatDouble StatDouble = (NTStatDouble)Pair.Value;
                        NTHumanStatDoubleData NewData = new(StatDouble, C);
                        DoubleStats[StatDouble.ID] = NewData;
                    }
                    else if (Pair.Value is NTStatBool)
                    {
                        NTStatBool StatBool = (NTStatBool)Pair.Value;
                        NTHumanStatBoolData NewData = new(StatBool, C);
                        BoolStats[StatBool.ID] = NewData;
                    }
                }
            }

            public class NTHumanStatDoubleData(NTStatDouble Stat, NTHuman C) // Stores our characters Stat Data
            {
                NTStatDouble StatRef = Stat; // Stores our template.
                public double Strength = 0;
            }

            public class NTHumanStatBoolData(NTStatBool Stat, NTHuman C) // Stores our characters Stat Data
            {
                NTStatBool StatRef = Stat; // Stores our template.
                public bool Strength = false;
            }

            public Dictionary<string, NTHumanStatDoubleData> DoubleStats = new();
            public Dictionary<string, NTHumanStatBoolData> BoolStats = new();

        }

    }

    class NTMonster(Character Monster) // To Do
    {
        public Character Monster = Monster; // Our Monster Ref

    }


    // ---------------------------------------- The Human Update -------------------------------------------------- \\

    public static NTHuman CharacterToNTHuman(Character Character)
    {
        if (!UpdatingHumans.ContainsKey(Character)) return null;
        return UpdatingHumans[Character];
    }

    public static void AddCharacterToUpdate(CharacterPrefab prefab, Vector2 position, string seed, CharacterInfo characterInfo, ushort id, bool isRemotePlayer, bool hasAi, bool createNetworkEvent, RagdollParams ragdoll, bool spawnInitialItems)
    {
        if (characterInfo == null) { return; }
        Character NewCharacter = characterInfo.Character;
        if (NewCharacter == null) { return; }
        if (NewCharacter.IsHuman)
        {
            HF.Print($"Adding the following Character {NewCharacter.Name} !");
            AddHumanToUpdate(NewCharacter);
        }
        else
        {
            //AddMonsterToUpdate(NewCharacter); I gotta test this more.
        }
    }

    public static void RemoveCharacterFromUpdate(Character target)
    {
        if (target is Character)
        {
            Character NewCharacter = (Character)target;
            if (NewCharacter.IsHuman)
            {
                HF.Print($"Removed the following Character {NewCharacter.Name} !");
                RemoveHumanFromUpdate(NewCharacter);
            }
            else
            {
                //RemoveMonsterFromUpdate(NewCharacter);
            }
        }
    }

    public static void AddHumanToUpdate(Character AddedCharacter)
    {
        if (!UpdatingHumans.ContainsKey(AddedCharacter))
        {
            NTHuman NewNTHuman = new NTHuman(AddedCharacter); // Hopefully this wont create a memory leak.
            UpdatingHumans[AddedCharacter] = NewNTHuman;
        }
    }

    public static void RemoveHumanFromUpdate(Character RemovingCharacter) // Probably a better way to do this.
    {
        if (UpdatingHumans.ContainsKey(RemovingCharacter)) return;
        UpdatingHumans.Remove(RemovingCharacter);
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

        } else // This isnt really a fix but it works for now.
        {
            output.Add(AfflictionPriority.HIGH);
        }

        return output;
    }

    private int Interval = 120;
    private int Tick = 0;
    private double NTDeltaTime = UpdateIntervalHigh / 120;
    // Gets called 60 times a second
    public void ThinkUpdate()
    {

        // If game paused we just skip
        if (HF.GameIsPaused() || !HF.InGame()) return;

        Tick--; // Decrement our tick.
        if (!(Tick < 0)) { return; }
        else { Tick = Interval; HF.Print("Human Update Tick"); }

        // We check if timer is up
        List<AfflictionPriority> checkedPriorities = GetLowestPriority(UpdateCooldown);
        UpdateCooldown++;
        if (checkedPriorities.Count == 0) return;

        HF.Print("Actually Update");
        NTAfflictions.DeltaTime = NTDeltaTime;
        Update(checkedPriorities);
    }

    private void Update(List<AfflictionPriority> priorities)
    {
        List<Character> CHList = Character.CharacterList;

        UpdateHumans(priorities);

        UpdateMonsters(priorities);
    }

    private void UpdateHumans(List<AfflictionPriority> priorities)
    {
        foreach (KeyValuePair<Character,NTHuman> Pair in UpdatingHumans)
        {

            Pair.Value.Update(priorities);

        }
    }

    private void UpdateMonsters(List<AfflictionPriority> priorities)
    {

    }

    private static void UpdateMonster(Character character)
    {

    }
}