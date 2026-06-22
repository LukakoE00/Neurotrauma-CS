using Barotrauma;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using LightInject;
using MonoMod.RuntimeDetour;
using MoonSharp.Interpreter;
using static Barotrauma.Networking.MessageFragment;
using static Neurotrauma.NTC;
using static Neurotrauma.HF;

namespace Neurotrauma;

public class HumanUpdate
{
    private static int UpdateCooldown = 0;
    private static readonly int UpdateIntervalHigh = (int)AfflictionPriority.HIGH; // 120 = 2s
    private static readonly int UpdateIntervalMedium = (int)AfflictionPriority.MEDIUM; // 240 = 4s
    private static readonly int UpdateIntervalLow = (int)AfflictionPriority.LOW; // 480 = 8s
    static private Dictionary<Character, NTHuman> UpdatingHumans = new();
    static private List<NTMonster> UpdatingMonsters = new();

    public Dictionary<Character, NTHuman> GetUpdatingCharacters()
    {
        return UpdatingHumans;
    }

    public List<NTMonster> GetUpdatingMonsters()
    {
        return UpdatingMonsters;
    }

    // ---------------------------------------- NT Human Update Classes -------------------------------------------------- \\

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
        public Dictionary<LimbType, double> Strength = Strength; // Gotta be a dictionary so we can store strength of limbtypes.
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

    public class NTHumanSymptomData(NTSymptom Sym, string ID)
    {
        public NTSymptom SymTemplate = Sym;
        public double Strength = 0;
        public int HumanUpdateTime = 0;
        public int HumanUpdateStoptime = 0;
        public string ID = ID;
    }

    /// <summary>
    /// The primary backbone of NT Characters. Stores all of the afflictions that our character uses. Also contains the strengths of each individual character.
    /// </summary>
    public class CharacterAfflictions
    {

        public CharacterAfflictions(Character Human2, NTHuman C)
        {
            Human = Human2;
            CharacterNT = C;

            AddAfflictions(); // ADD OUR AFFLICTIONS YEAHHHHH
        }

        public Character Human { get; set; } // Our Human Ref
        public NTHuman CharacterNT { get; set; } // Our NTHuman Ref

        public Dictionary<string, NTHumanNonLimbAffData> UpdatingAfflictions = new(); // Stores the ID's of our updating afflictions.
        public Dictionary<string, NTHumanLimbAffData> UpdatingLimbAfflictions = new(); // Stores the ID's of our updating (Limb) afflictions.
        public Dictionary<string, NTHumanBloodAffData> UpdatingBloodAfflictions = new(); // Stores the ID's of our updating (blood) afflictions.
        public Dictionary<string, NTHumanSymptomData> UpdatingSymptoms = new(); // Stores the ID's of our symptoms.

        public void RegisterAffliction(string ID, NTAffliction Aff, double Strength)
        {
            if (Aff != null)
            {
                if (Aff is NTNonLimbAffliction)
                {
                    RegisterNonLimbAffliction(ID, (NTNonLimbAffliction)Aff, Strength);
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

        public void RegisterLimbAffliction(string ID, NTLimbAffliction NTLimbAff, Dictionary<LimbType, double> Strength)
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

        public void RegisterSymptom(string ID, NTSymptom Sym)
        {
            if (CharacterNT == null) return;
            if (NTAfflictions.HasAffliction(ID) && !UpdatingSymptoms.ContainsKey(ID))
            {
                UpdatingSymptoms[ID] = new NTHumanSymptomData(Sym, ID);
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
            else if (Aff is NTBloodAffliction)
            {
                return "NTBloodAffliction";
            }
            else if (Aff is NTBloodAffliction)
            {
                return "NTBloodAffliction";
            }
            return "NTSymptom";
        }

        private void AddAfflictions()
        {
            foreach (KeyValuePair<string, NTAffliction> Pair in NTAfflictions.Afflictions)
            {
                string ID = Pair.Key;
                NTAffliction Aff = Pair.Value;
                string Type = NTAfflictionToType(Aff);
                switch (Type)
                {
                    case "NTNonLimbAffliction":
                        RegisterNonLimbAffliction(ID, (NTNonLimbAffliction)Aff, 0);
                        break;

                    case "NTLimbAffliction":
                        RegisterLimbAffliction(ID, (NTLimbAffliction)Aff, HF.DefaultLimbAffStrengths);
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
            foreach (KeyValuePair<string, NTStat> Pair in NTStats.Stats)
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
            public NTStatDouble StatRef = Stat; // Stores our template.
            public double Strength = 0;
        }

        public class NTHumanStatBoolData(NTStatBool Stat, NTHuman C) // Stores our characters Stat Data
        {
            public NTStatBool StatRef = Stat; // Stores our template.
            public bool Strength = false;
        }

        public Dictionary<string, NTHumanStatDoubleData> DoubleStats = new();
        public Dictionary<string, NTHumanStatBoolData> BoolStats = new();

    }

    /// <summary>
    /// Stores the Tags that our character has. Used by NTCompat for adding/setting tags and for giving speed multipliers.
    /// This acts as a replacement for NT's old Data system it used.
    /// </summary>
    public class CharacterTags
    {
        public Dictionary<string, double> Tags = new();

        public void SetTag(string Prefix, string TagID, double Amount = 1)
        {
            Tags[Prefix + "_" + TagID] = Amount;
        }

        public void SetTagsByPrefix(string Prefix, double Amount)
        {
            foreach (KeyValuePair<string, double> Pair in Tags) // Why is this read only?????
            {
                if (Pair.Key.StartsWith(Prefix))
                {
                    Tags[Pair.Key] = Amount;
                }
            }
        }

        public void SetTagsByTagID(string TagID, double Amount)
        {
            foreach (KeyValuePair<string, double> Pair in Tags)
            {
                if (Pair.Key.EndsWith(TagID))
                {
                    Tags[Pair.Key] = Amount;
                }
            }
        }

        public void RemoveTag(string Prefix, string TagID)
        {
            if (!HasTag(Prefix, TagID)) return;
            Tags.Remove(Prefix + "_" + TagID);
        }

        public bool HasTag(string Prefix, string TagID)
        {
            return Tags.ContainsKey(Prefix + "_" + TagID);
        }

        public double GetTag(string Prefix, string TagID)
        {
            if (!HasTag(Prefix, TagID)) return 1;
            return Tags[Prefix + "_" + TagID];
        }
    }

    /// <summary>
    /// The Neurotrauma version of a Human Character. Stores crucial info required for NT to work.
    /// </summary>
    public class NTHuman
    {
        public NTHuman(Character NewHuman)
        {
            Human = NewHuman;
            LocalStats = new CharacterStats(this);
            LocalAfflictions = new CharacterAfflictions(NewHuman, this);
            LocalTags = new CharacterTags();
            SetSpeed(this,1);
        }

        public Character Human; // Our Human Ref
        public CharacterStats LocalStats;
        public CharacterAfflictions LocalAfflictions;
        public CharacterTags LocalTags;

        // -------------------------------- Start of afflictions -------------------------------- \\

        public Dictionary<string, NTHumanNonLimbAffData> GetAffDatas()
        {
            return LocalAfflictions.UpdatingAfflictions;
        }

        public NTHumanNonLimbAffData GetAffData(string Identifier)
        {
            return LocalAfflictions.UpdatingAfflictions[Identifier];
        }

        public double GetAffStrength(string Identifier) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            return (LocalAfflictions.UpdatingAfflictions.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingAfflictions[Identifier].Strength : 0;
        }

        public Dictionary<string, NTHumanLimbAffData> GetLimbAffDatas()
        {
            return LocalAfflictions.UpdatingLimbAfflictions;
        }

        public NTHumanLimbAffData GetLimbAffData(string Identifier)
        {
            return LocalAfflictions.UpdatingLimbAfflictions[Identifier];
        }

        public double GetLimbAffStrength(string Identifier, LimbType Limb) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            return (LocalAfflictions.UpdatingLimbAfflictions.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingLimbAfflictions[Identifier].Strength[Limb] : 0;
        }

        public Dictionary<string, NTHumanBloodAffData> GetBloodAffDatas()
        {
            return LocalAfflictions.UpdatingBloodAfflictions;
        }

        public NTHumanBloodAffData GetBloodAffData(string Identifier)
        {
            return LocalAfflictions.UpdatingBloodAfflictions[Identifier];
        }

        public double GetBloodAffStrength(string Identifier) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            return (LocalAfflictions.UpdatingBloodAfflictions.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingBloodAfflictions[Identifier].Strength : 0;
        }

        public Dictionary<string,NTHumanSymptomData> GetSymptomAffDatas()
        {
            return LocalAfflictions.UpdatingSymptoms;
        }

        public NTHumanSymptomData GetSymptomAffData(string Identifier)
        {
            return LocalAfflictions.UpdatingSymptoms[Identifier];
        }

        public double GetSymptomStrength(string Identifier) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            return (LocalAfflictions.UpdatingSymptoms.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingSymptoms[Identifier].Strength : 0;
        }

        public CharacterAfflictions? GetAfflictions()
        {
            return LocalAfflictions;
        }

        // -------------------------------- Start of stats -------------------------------- \\

        public CharacterStats? GetStats()
        {
            return LocalStats;
        }

        public CharacterStats.NTHumanStatBoolData GetBoolStat(string Identifier)
        {
            return LocalStats.BoolStats[Identifier];
        }

        public bool GetBoolStatUpdate(NTHuman C, string Identifier)
        {
            return (LocalStats.BoolStats.ContainsKey(Identifier)) ? LocalStats.BoolStats[Identifier].StatRef.Get(C) : false;
        }

        public bool GetBoolStatStrength(string Identifier)
        {
            return (LocalStats.BoolStats.ContainsKey(Identifier)) ? LocalStats.BoolStats[Identifier].Strength: false;
        }

        public void SetBoolStatStrength(string Identifier, bool Strength)
        {
            if (LocalStats.BoolStats.ContainsKey(Identifier))
            {
                LocalStats.BoolStats[Identifier].Strength = Strength;
            }
        }

        public CharacterStats.NTHumanStatDoubleData GetDoubleStat(string Identifier)
        {
            return LocalStats.DoubleStats[Identifier];
        }

        public double GetDoubleStatUpdate(NTHuman C, string Identifier)
        {
            return (LocalStats.DoubleStats.ContainsKey(Identifier)) ? LocalStats.DoubleStats[Identifier].StatRef.Get(C) : 0;
        }

        public double GetDoubleStatStrength(string Identifier)
        {
            return (LocalStats.DoubleStats.ContainsKey(Identifier)) ? LocalStats.DoubleStats[Identifier].Strength: 0;
        }

        public void SetDoubleStatStrength(string Identifier, double Strength)
        {
            if (LocalStats.DoubleStats.ContainsKey(Identifier))
            {
                LocalStats.DoubleStats[Identifier].Strength = Strength;
            }
        }

        // -------------------------------- Start of tags -------------------------------- \\

        public CharacterTags GetTags()
        {
            return LocalTags;
        }

        // -------------------------------- Start of cursed update stuff -------------------------------- \\

        public void Update(List<AfflictionPriority> Priorities) // THHHHEEEE UPPPDATTEEEEE
        {
            if (Human == null) return;

            foreach (Action<NTHuman> Hook in PreHumanUpdateHooks) // Pre hooks.
            {
                Hook.Invoke(this);
            }

            // ----------------------------------------- Stat updates ----------------------------------------- \\

            foreach (KeyValuePair<string,CharacterStats.NTHumanStatDoubleData> Pair in LocalStats.DoubleStats) // Update all of out 
            {
                string ID = Pair.Key;
                CharacterStats.NTHumanStatDoubleData StatData = Pair.Value;
                SetDoubleStatStrength(Pair.Key, GetDoubleStatUpdate(this, ID));
            }

            foreach (KeyValuePair<string, CharacterStats.NTHumanStatBoolData> Pair in LocalStats.BoolStats) // Update all of out 
            {
                string ID = Pair.Key;
                CharacterStats.NTHumanStatBoolData StatData = Pair.Value;
                SetBoolStatStrength(Pair.Key, GetBoolStatUpdate(this, ID));
            }

            // ----------------------------------------- Affliction updates ----------------------------------------- \\

            foreach (KeyValuePair<string, NTHumanNonLimbAffData> Pair in LocalAfflictions.UpdatingAfflictions) // Update Non Limb Afflictions
            {
                if (Pair.Key != null)
                {
                    // Fetch the data of the affliction
                    string ID = Pair.Key;
                    NTHumanNonLimbAffData AffData = Pair.Value;
                    NTNonLimbAffliction Aff = AffData.AffTemplate;

                    if (!Priorities.Contains(Aff.Priority)) continue; // Skip to the next affliction, we don't have the same priority currently.

                    double CurrentStrength = GetAfflictionStrength(Human, ID);
                    AffData.Strength = CurrentStrength;

                    if (!Aff.Const && CurrentStrength < Aff.MinStrength) continue; // Our second check to see if we should run this affliction. Basically, if this affliction isn't active on the limb, and not constant, don't update.

                    double PrevStrength = AffData.Strength;
                    Aff.UpdateAction(this, ID, LimbType.Torso, AffData);
                    ApplyAfflictionChange(Human, ID, (float)AffData.Strength, (float)PrevStrength, (float)AffData.AffTemplate.MinStrength, (float)AffData.AffTemplate.MaxStrength);
                }
            }

            foreach (KeyValuePair<string, NTHumanLimbAffData> Pair in LocalAfflictions.UpdatingLimbAfflictions) // Update Limb Afflictions
            {
                if (Pair.Key != null)
                {

                    foreach (LimbType Limb in LimbsToCheck)
                    {
                        // Fetch the data of the affliction
                        string ID = Pair.Key;
                        NTHumanLimbAffData AffData = Pair.Value;
                        NTLimbAffliction Aff = AffData.AffTemplate;

                        if (!Priorities.Contains(Aff.Priority)) continue;

                        double CurrentStrength = GetAfflictionStrengthLimb(Human, Limb, ID);
                        AffData.Strength[Limb] = CurrentStrength;

                        if (!Aff.Const && CurrentStrength < Aff.MinStrength) continue; // Our second check to see if we should run this affliction. Basically, if this affliction isn't active on the limb, and not constant, don't update.

                        double PrevStrength = AffData.Strength[Limb];
                        Aff.UpdateAction(this, ID, Limb, AffData);
                        ApplyAfflictionChangeLimb(Human, Limb, ID, (float)AffData.Strength[Limb], (float)PrevStrength, (float)AffData.AffTemplate.MinStrength, (float)AffData.AffTemplate.MaxStrength);
                    }
                }
            }

            foreach (KeyValuePair<string, NTHumanBloodAffData> Pair in LocalAfflictions.UpdatingBloodAfflictions) // Update Blood Afflictions
            {
                if (Pair.Key != null)
                {
                    // Fetch the data of the affliction
                    string ID = Pair.Key;
                    NTHumanBloodAffData AffData = Pair.Value;
                    NTBloodAffliction Aff = AffData.AffTemplate;

                    if (!Priorities.Contains(Aff.Priority)) continue;

                    double CurrentStrength = GetAfflictionStrength(Human, ID);
                    AffData.Strength = CurrentStrength;

                    if (!Aff.Const && CurrentStrength < Aff.MinStrength) continue; // Our second check to see if we should run this affliction. Basically, if this affliction isn't active on the limb, and not constant, don't update.

                    double PrevStrength = AffData.Strength;
                    Aff.UpdateAction(this, ID, LimbType.Torso, AffData);
                    ApplyAfflictionChange(Human, ID, (float)AffData.Strength, (float)PrevStrength, (float)AffData.AffTemplate.MinStrength, (float)AffData.AffTemplate.MaxStrength);
                }
            }

            foreach (KeyValuePair<string, NTHumanSymptomData> Pair in LocalAfflictions.UpdatingSymptoms) // Update symptoms
            {
                if (Pair.Key != null)
                {
                    // Fetch the data of the affliction
                    string ID = Pair.Key;
                    NTHumanSymptomData SymData = Pair.Value;
                    NTSymptom Sym = SymData.SymTemplate;

                    if (!Priorities.Contains(Sym.Priority)) continue;

                    double CurrentStrength = GetAfflictionStrength(Human, ID);
                    SymData.Strength = CurrentStrength;

                    if (!Sym.Const && CurrentStrength < Sym.MinStrength) continue; // Our second check to see if we should run this affliction. Basically, if this affliction isn't active on the limb, and not constant, don't update.

                    if (SymData.HumanUpdateTime <= 0) SymData.Strength = 0;
                    else SymData.Strength = 100; SymData.HumanUpdateTime--;

                    if (SymData.HumanUpdateStoptime > 0) SymData.Strength = 0;
                    else SymData.HumanUpdateStoptime--;

                    double PrevStrength = SymData.Strength;
                    Sym.UpdateAction(this, ID, LimbType.Torso, SymData);
                    ApplyAfflictionChange(Human, ID, (float)SymData.Strength, (float)PrevStrength, (float)SymData.SymTemplate.MinStrength, (float)SymData.SymTemplate.MaxStrength);
                }
            }

            // ----------------------------------------- Clearing ----------------------------------------- \\

            foreach (Action<NTHuman> Hook in PostHumanUpdateHooks) // Post hooks.
            {
                Hook.Invoke(this);
            }

            Human.SpeedMultiplier = (float) GetSpeed(this);
            CharacterSpeedMultipliers.Remove(this);

        }
    }

    public class NTMonster(Character Monster) // To Do
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