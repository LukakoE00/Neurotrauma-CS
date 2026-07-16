using Barotrauma;
using MonoMod.Utils;
using static Microsoft.Xna.Framework.Graphics.VertexDeclaration;
using static Neurotrauma.HF;
using static Neurotrauma.NTC;

namespace Neurotrauma;

public static class HumanUpdate
{
    private static int UpdateCooldown = 0;
    private static readonly int UpdateIntervalHigh = (int)AfflictionPriority.HIGH; // 120 = 2s
    private static readonly int UpdateIntervalMedium = (int)AfflictionPriority.MEDIUM; // 240 = 4s
    private static readonly int UpdateIntervalLow = (int)AfflictionPriority.LOW; // 480 = 8s
    static private Dictionary<Character, NTHuman> UpdatingHumans = new();
    static private List<NTMonster> UpdatingMonsters = new();

    public static Dictionary<Character, NTHuman> GetUpdatingCharacters()
    {
        return UpdatingHumans;
    }

    public static List<NTMonster> GetUpdatingMonsters()
    {
        return UpdatingMonsters;
    }

    // ---------------------------------------- NT Human Update Classes -------------------------------------------------- \\

    /// <summary>
    /// The abstract data class we use to store Afflictions.
    /// </summary>
    public abstract class NTHumanAffData()
    {
        public NTAffliction AffTemplate; // Stores our aff template for updates and clamps.
        public double Strength;
        public double PrevStrength;
        public string ID;
    }

    /// <summary>
    /// The data class stored in NTHumans to represent our Afflictions.
    /// </summary>
    public class NTHumanNonLimbAffData : NTHumanAffData // Stores our characters Aff Data
    {
        new public NTNonLimbAffliction AffTemplate;
        public NTHumanNonLimbAffData(NTNonLimbAffliction NewAff, string NewID, double NewStrength = 0) : base()
        {
            AffTemplate = NewAff; // Stores our template. The reason we aren't just creating a new affliction for each character is performance. I'm pretty sure it's more peformance efficent to just reference our affliction.
            base.AffTemplate = NewAff;
            Strength = NewStrength;
            PrevStrength = 0;
            ID = NewID;
        }
    }

    /// <summary>
    /// The data class stored in NTHumans to represent our Limb Afflictions.
    /// </summary>
    public class NTHumanLimbAffData : NTHumanAffData // Stores our characters Aff Data
    {
        new public NTLimbAffliction AffTemplate;
        public Dictionary<LimbType, double> Strength = new(); // Gotta be a dictionary so we can store strength of limbtypes.
        public Dictionary<LimbType, double> PrevStrength = new();

        public NTHumanLimbAffData(NTLimbAffliction NewAff, string NewID, Dictionary<LimbType, double> NewStrength) : base()
        {
            AffTemplate = NewAff; // Stores our template. The reason we aren't just creating a new affliction for each character is performance. I'm pretty sure it's more peformance efficent to just reference our affliction.
            base.AffTemplate = NewAff;
            Strength = NewStrength;
            PrevStrength = NewStrength;
            ID = NewID;
        }

        public double GetLimbPrevStrength(LimbType Type) // Lua compat
        {
            if (!PrevStrength.ContainsKey(Type)) return 0;
            return PrevStrength[Type];
        }

        public double GetLimbStrength(LimbType Type) // Lua compat
        {
            if (!Strength.ContainsKey(Type)) return 0;
            return Strength[Type];
        }
    }

    /// <summary>
    /// The data class stored in NTHumans to represent our Blood Afflictions.
    /// </summary>
    public class NTHumanBloodAffData : NTHumanAffData // Stores our characters Aff Data
    {
        new public NTBloodAffliction AffTemplate;
        public NTHumanBloodAffData(NTBloodAffliction NewAff, string NewID, double NewStrength = 0) : base()
        {
            AffTemplate = NewAff; // Stores our template. The reason we aren't just creating a new affliction for each character is performance. I'm pretty sure it's more peformance efficent to just reference our affliction.
            base.AffTemplate = NewAff;
            Strength = NewStrength;
            PrevStrength = 0;
            ID = NewID;
        }
    }

    /// <summary>
    /// The data class stored in NTHumans to represent our Symptoms.
    /// </summary>
    public class NTHumanSymptomData : NTHumanAffData
    {
        public NTSymptom SymTemplate;
        public int HumanUpdateTime = 0;
        public int HumanUpdateStoptime = 0;
        public NTHumanSymptomData(NTSymptom NewAff, string NewID) : base()
        {
            AffTemplate = NewAff; // Failsafe, don't reference this when using NTSymptom.
            base.AffTemplate = NewAff;
            SymTemplate = NewAff; // Stores our template. The reason we aren't just creating a new affliction for each character is performance. I'm pretty sure it's more peformance efficent to just reference our affliction.
            PrevStrength = 0;
            ID = NewID;
        }
    }

    /// <summary>
    /// The data class stored in NTHumans to represent our Limb Specific Symptoms.
    /// </summary>
    public class NTHumanLimbSymptomData : NTHumanLimbAffData
    {
        public NTLimbSymptom SymTemplate;
        public Dictionary<LimbType, int> HumanUpdateTime = new();
        public Dictionary<LimbType, int> HumanUpdateStoptime = new();

        public NTHumanLimbSymptomData(NTLimbSymptom NewAff, string NewID, Dictionary<LimbType, double> NewStrength, Dictionary<LimbType, int> NewUpdateTime) : base(NewAff, NewID, NewStrength)
        {
            AffTemplate = NewAff; // Failsafe, don't reference this when using NTSymptom.
            SymTemplate = NewAff; // Stores our template. The reason we aren't just creating a new affliction for each character is performance. I'm pretty sure it's more peformance efficent to just reference our affliction.
            HumanUpdateTime = NewUpdateTime;
            HumanUpdateStoptime = NewUpdateTime;
        }
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

        public Dictionary<string, NTHumanAffData> UpdatingAfflictions = new();                  // Stores the ID's of our updating afflictions.
        public Dictionary<string, NTHumanAffData> ConstantAfflictions = new();
        public Dictionary<string, NTHumanNonLimbAffData> UpdatingNonLimbAfflictions = new();    // Stores the ID's of our updating non limb afflictions.
        public Dictionary<string, NTHumanLimbAffData> UpdatingLimbAfflictions = new();          // Stores the ID's of our updating (Limb) afflictions.
        public Dictionary<string, NTHumanBloodAffData> UpdatingBloodAfflictions = new();        // Stores the ID's of our updating (blood) afflictions.
        public Dictionary<string, NTHumanSymptomData> UpdatingSymptoms = new();                 // Stores the ID's of our symptoms.
        public Dictionary<string, NTHumanLimbSymptomData> UpdatingLimbSymptoms = new();         // Stores the ID's of our limb symptoms.

        public List<Affliction> LastUpdatedAfflictions = new();

        public void RegisterAffliction(string ID, NTAffliction Aff)
        {
            if (Aff != null)
            {
                if (Aff is NTSymptom)
                {
                    RegisterSymptom(ID, (NTSymptom)Aff);
                }
                else if (Aff is NTLimbSymptom)
                {
                    RegisterLimbSymptom(ID, (NTLimbSymptom)Aff, new Dictionary<LimbType, double>(DefaultLimbAffStrengths), new Dictionary<LimbType, int>(DefaultLimbSymUpdateTime));
                }
                else if (Aff is NTNonLimbAffliction)
                {
                    RegisterNonLimbAffliction(ID, (NTNonLimbAffliction)Aff, 0);
                }
                else if (Aff is NTBloodAffliction)
                {
                    RegisterBloodAffliction(ID, (NTBloodAffliction)Aff, 0);
                }
                else if (Aff is NTLimbAffliction)
                {
                    RegisterLimbAffliction(ID, (NTLimbAffliction)Aff, new Dictionary<LimbType, double>(DefaultLimbAffStrengths));
                }
            }
        }

        public void RegisterNonLimbAffliction(string ID, NTNonLimbAffliction NTNonLimbAff, double Strength)
        {
            if (CharacterNT == null) return;
            if (NTAfflictions.HasAffliction(ID) && !UpdatingNonLimbAfflictions.ContainsKey(ID))
            {

                UpdatingNonLimbAfflictions[ID] = new NTHumanNonLimbAffData(NTNonLimbAff, ID, Strength);
                UpdatingAfflictions[ID] = UpdatingNonLimbAfflictions[ID];
                if (UpdatingNonLimbAfflictions[ID].AffTemplate.Const)
                {
                    ConstantAfflictions[ID] = UpdatingNonLimbAfflictions[ID];
                }

            }
        }

        public void RegisterLimbAffliction(string ID, NTLimbAffliction NTLimbAff, Dictionary<LimbType, double> Strength)
        {
            if (CharacterNT == null) return;
            if (NTAfflictions.HasAffliction(ID) && !UpdatingLimbAfflictions.ContainsKey(ID))
            {
                UpdatingLimbAfflictions[ID] = new NTHumanLimbAffData(NTLimbAff, ID, new Dictionary<LimbType, double>(Strength));
                UpdatingAfflictions[ID] = UpdatingLimbAfflictions[ID];
                if (UpdatingLimbAfflictions[ID].AffTemplate.Const)
                {
                    ConstantAfflictions[ID] = UpdatingLimbAfflictions[ID];
                }
            }
        }

        public void RegisterBloodAffliction(string ID, NTBloodAffliction NTBloodAff, double Strength)
        {
            if (CharacterNT == null) return;
            if (NTAfflictions.HasAffliction(ID) && !UpdatingBloodAfflictions.ContainsKey(ID))
            {
                UpdatingBloodAfflictions[ID] = new NTHumanBloodAffData(NTBloodAff, ID, Strength);
                UpdatingAfflictions[ID] = UpdatingBloodAfflictions[ID];
                if (UpdatingBloodAfflictions[ID].AffTemplate.Const)
                {
                    ConstantAfflictions[ID] = UpdatingBloodAfflictions[ID];
                }
            }
        }

        public void RegisterSymptom(string ID, NTSymptom Sym)
        {
            if (CharacterNT == null) return;
            if (NTAfflictions.HasAffliction(ID) && !UpdatingSymptoms.ContainsKey(ID))
            {
                UpdatingSymptoms[ID] = new NTHumanSymptomData(Sym, ID);
                UpdatingAfflictions[ID] = UpdatingSymptoms[ID];
                if (UpdatingSymptoms[ID].AffTemplate.Const)
                {
                    ConstantAfflictions[ID] = UpdatingSymptoms[ID];
                }
            }
        }

        public void RegisterLimbSymptom(string ID, NTLimbSymptom Sym, Dictionary<LimbType,double> Strength, Dictionary<LimbType, int> UpdateTime)
        {
            if (CharacterNT == null) return;
            if (NTAfflictions.HasAffliction(ID) && !UpdatingLimbSymptoms.ContainsKey(ID))
            {
                UpdatingLimbSymptoms[ID] = new NTHumanLimbSymptomData(Sym, ID, new Dictionary<LimbType, double>(Strength), new Dictionary<LimbType, int>(UpdateTime));
                UpdatingAfflictions[ID] = UpdatingLimbSymptoms[ID];
                if (UpdatingLimbSymptoms[ID].AffTemplate.Const)
                {
                    ConstantAfflictions[ID] = UpdatingLimbSymptoms[ID];
                }
            }
        }

        public bool HasNonLimbAffliction(string ID)
        {
            return CharacterNT.LocalAfflictions.UpdatingNonLimbAfflictions.ContainsKey(ID);
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
                UpdatingAfflictions.Remove(ID);
                if (Aff is NTSymptom)
                {
                    UpdatingSymptoms.Remove(ID);
                    return;
                }
                else if (Aff is NTLimbSymptom)
                {
                    UpdatingLimbSymptoms.Remove(ID);
                    return;
                }
                else if (Aff is NTNonLimbAffliction)
                {
                    UpdatingNonLimbAfflictions.Remove(ID);
                    return;
                }
                else if (Aff is NTLimbAffliction)
                {
                    UpdatingLimbAfflictions.Remove(ID);
                    return;
                }
                else if (Aff is NTBloodAffliction)
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
                UpdatingNonLimbAfflictions.Remove(ID);
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

        public void RemoveSymptom(string ID)
        {
            if (NTAfflictions.HasAffliction(ID))
            {
                UpdatingSymptoms.Remove(ID);
            }
        }

        public void RemoveLimbSymptom(string ID)
        {
            if (NTAfflictions.HasAffliction(ID))
            {
                UpdatingSymptoms.Remove(ID);
            }
        }

        public List<string> GetUpdatingNonLimbAfflictions()
        {
            return UpdatingNonLimbAfflictions.Keys.ToList();
        }

        public NTHumanNonLimbAffData GetNonLimbData(string ID)
        {
            if (UpdatingNonLimbAfflictions.ContainsKey(ID))
            {
                return UpdatingNonLimbAfflictions[ID];
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

        public string NTAfflictionToType(NTAffliction Aff)
        {
            if (Aff is NTSymptom)
            {
                return "NTSymptom";
            }
            else if (Aff is NTLimbSymptom)
            {
                return "NTLimbSymptom";
            }
            else if (Aff is NTNonLimbAffliction)
            {
                return "NTNonLimbAffliction";
            }
            else if (Aff is NTLimbAffliction)
            {
                return "NTLimbAffliction";
            }
            else if (Aff is NTBloodAffliction)
            {
                return "NTBloodAffliction";
            }
            return "NTNonLimbAffliction";
        }

        private void AddAfflictions()
        {
            foreach (KeyValuePair<string, NTAffliction> Pair in NTAfflictions.Afflictions)
            {
                RegisterAffliction(Pair.Key,Pair.Value);
            }
        }
    }

    /// <summary>
    /// The stats of out NTHumans, used to read and write data.
    /// </summary>
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
            public string ID = Stat.ID;
        }

        public class NTHumanStatBoolData(NTStatBool Stat, NTHuman C) // Stores our characters Stat Data
        {
            public NTStatBool StatRef = Stat; // Stores our template.
            public bool Strength = false;
            public string ID = Stat.ID;
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
            SetDefaults(this);
        }

        public Character Human; // Our Human Ref
        public CharacterStats LocalStats;
        public CharacterAfflictions LocalAfflictions;
        public CharacterTags LocalTags;


        // -------------------------------- Start of afflictions -------------------------------- \\

        public Dictionary<string, NTHumanAffData> GetAffDatas()
        {
            return LocalAfflictions.UpdatingAfflictions;
        }

        public NTHumanAffData GetAffData(string Identifier)
        {
            if (!LocalAfflictions.UpdatingAfflictions.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingAfflictions!");

            return LocalAfflictions.UpdatingAfflictions[Identifier];
        }

        public double GetAffStrength(string Identifier) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            if (!LocalAfflictions.UpdatingAfflictions.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingAfflictions!");

            return (LocalAfflictions.UpdatingAfflictions.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingAfflictions[Identifier].Strength : 0;
        }

        public Dictionary<string, NTHumanNonLimbAffData> GetNonLimbAffDatas()
        {
            return LocalAfflictions.UpdatingNonLimbAfflictions;
        }

        public NTHumanNonLimbAffData GetNonLimbAffData(string Identifier)
        {
            if (!LocalAfflictions.UpdatingNonLimbAfflictions.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingNonLimbAfflictions!");

            return LocalAfflictions.UpdatingNonLimbAfflictions[Identifier];
        }

        public double GetNonLimbAffStrength(string Identifier) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            if (!LocalAfflictions.UpdatingNonLimbAfflictions.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingNonLimbAfflictions!");

            return (LocalAfflictions.UpdatingNonLimbAfflictions.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingNonLimbAfflictions[Identifier].Strength : 0;
        }

        public Dictionary<string, NTHumanLimbAffData> GetLimbAffDatas()
        {
            return LocalAfflictions.UpdatingLimbAfflictions;
        }

        public NTHumanLimbAffData GetLimbAffData(string Identifier)
        {
            if (!LocalAfflictions.UpdatingLimbAfflictions.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingLimbAfflictions!");

            return LocalAfflictions.UpdatingLimbAfflictions[Identifier];
        }

        public double GetLimbAffStrength(string Identifier, LimbType Limb) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            if (!LocalAfflictions.UpdatingLimbAfflictions.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingLimbAfflictions!");

            return (LocalAfflictions.UpdatingLimbAfflictions.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingLimbAfflictions[Identifier].Strength[Limb] : 0;
        }

        public Dictionary<string, NTHumanBloodAffData> GetBloodAffDatas()
        {
            return LocalAfflictions.UpdatingBloodAfflictions;
        }

        public NTHumanBloodAffData GetBloodAffData(string Identifier)
        {
            if (!LocalAfflictions.UpdatingBloodAfflictions.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingBloodAfflictions!");

            return LocalAfflictions.UpdatingBloodAfflictions[Identifier];
        }

        public double GetBloodAffStrength(string Identifier) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            if (!LocalAfflictions.UpdatingBloodAfflictions.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingBloodAfflictions!");

            return (LocalAfflictions.UpdatingBloodAfflictions.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingBloodAfflictions[Identifier].Strength : 0;
        }

        public Dictionary<string,NTHumanSymptomData> GetSymptomAffDatas()
        {

            return LocalAfflictions.UpdatingSymptoms;
        }

        public NTHumanSymptomData GetSymptomAffData(string Identifier)
        {
            if (!LocalAfflictions.UpdatingSymptoms.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingSymptoms!");

            return LocalAfflictions.UpdatingSymptoms[Identifier];
        }

        public double GetSymptomStrength(string Identifier) // SHOULD ONLY BE USED FOR READING. NOT SETTING.
        {
            if (!LocalAfflictions.UpdatingSymptoms.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingSymptoms!");

            return (LocalAfflictions.UpdatingSymptoms.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingSymptoms[Identifier].Strength : 0;
        }

        public Dictionary<string,NTHumanLimbSymptomData> GetLimbSymptomDatas()
        {
            return LocalAfflictions.UpdatingLimbSymptoms;
        }

        public NTHumanLimbSymptomData GetLimbSymptomData(string Identifier)
        {
            if (!LocalAfflictions.UpdatingLimbSymptoms.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingLimbSymptoms!");

            return LocalAfflictions.UpdatingLimbSymptoms[Identifier];
        }

        public double GetLimbSymptomStrength(string Identifier, LimbType Limb)
        {
            if (!LocalAfflictions.UpdatingLimbSymptoms.ContainsKey(Identifier)) PrintError($"The following identifier of {Identifier} wasn't found in UpdatingLimbSymptoms!");

            return (LocalAfflictions.UpdatingLimbSymptoms.ContainsKey(Identifier)) ? LocalAfflictions.UpdatingLimbSymptoms[Identifier].Strength[Limb] : 0;
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

        /// <summary>
        /// Can return NTHumanStatDoubleData or NTHumanStatBoolData.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Identifier"></param>
        /// <returns></returns>
        public T GetStat<T>(string Identifier) // Not my best work.
        {
            object ReturnType = null;
            if (HasBoolStat(Identifier)) ReturnType = GetBoolStat(Identifier);
            else ReturnType = GetDoubleStat(Identifier);
            return (T) Convert.ChangeType(ReturnType, typeof(T));
        }

        public T GetStatStrength<T>(string Identifier) // Not my best work.
        {
            object ReturnType = null;
            if (HasBoolStat(Identifier)) ReturnType = GetBoolStat(Identifier).Strength;
            else ReturnType = GetDoubleStat(Identifier).Strength;
            return (T)Convert.ChangeType(ReturnType, typeof(T));
        }

        public bool HasBoolStat(string Identifier)
        {
            if (LocalStats.BoolStats.ContainsKey(Identifier)) return true;
            return false;
        }

        public CharacterStats.NTHumanStatBoolData GetBoolStat(string Identifier)
        {
            return LocalStats.BoolStats[Identifier];
        }

        public bool GetBoolStatUpdate(NTHuman C, string Identifier) // SHOULDNT BE USED I REGRET WRITING THIS FUNCTION.
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

        public bool HasDoubleStat(string Identifier)
        {
            if (LocalStats.DoubleStats.ContainsKey(Identifier)) return true;
            return false;
        }

        public CharacterStats.NTHumanStatDoubleData GetDoubleStat(string Identifier)
        {
            return LocalStats.DoubleStats[Identifier];
        }

        public double GetDoubleStatUpdate(NTHuman C, string Identifier) // SHOULDNT BE USED I REGRET WRITING THIS FUNCTION.
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

        private void SetDefaults(NTHuman C)
        {
            foreach (KeyValuePair<string,NTHumanAffData> Pair in C.LocalAfflictions.ConstantAfflictions)
            {
                if (Pair.Key == null || Pair.Value == null) continue;
                NTAfflictionType AffType = Pair.Value.AffTemplate.Type;
                SetAfflictionDefaultStrength(AffType, Pair.Key, Pair.Value);
            }
        }

        public void Update(List<AfflictionPriority> Priorities) // THHHHEEEE UPPPDATTEEEEE
        {

            if (Human == null) return;

            if (!(Human.IsHuman && Human.TeamID == CharacterTeamType.Team1 || Human.TeamID == CharacterTeamType.Team2 && !Human.IsDead))
            {
                if (!HasAffliction(Human, "luabotomy")) return;
            }

            UpdatePreHumanHooks();

            // ----------------------------------------- Stat updates ----------------------------------------- \\

            UpdateStats();

            // ----------------------------------------- Affliction updates ----------------------------------------- \\

            UpdateAfflictions(Priorities);

            SetAfflictionStrengths();

            // ----------------------------------------- Clearing ----------------------------------------- \\

            UpdatePost();

        }

        public void ClearSpeedMultiplier()
        {
            CharacterSpeedMultipliers.Remove(this);
        }

        public void UpdatePreHumanHooks()
        {
            foreach (Action<NTHuman> Hook in PreHumanUpdateHooks) // Pre hooks.
            {
                Hook.Invoke(this);
            }

            if (UsingLuaAddons())
            {
                HumanUpdateLuaSync.SyncPreHumanUpdateHooks(this.Human);
            }
        }

        private void UpdatePost()
        {
            UpdatePostHumanHooks();

            HF.SetAffliction(Human, "slowdown", Math.Clamp(100 * (1 - (float)GetDoubleStatStrength("speedmultiplier")), 0, 100));

            if (UsingLuaAddons()) HumanUpdateLuaSync.SyncCharacterSpeed(Human, GetDoubleStatStrength("speedmultiplier")); // If we have lua addons sync our character speed.

            else
            {
                SetDoubleStatStrength("speedmultiplier", 1);
                ClearSpeedMultiplier();
            }
        }

        public void UpdatePostHumanHooks()
        {
            if (!UsingLuaAddons())
            {
                NTC.TickCharacterTags(this);
                foreach (Action<NTHuman> Hook in PostHumanUpdateHooks) // Post hooks.
                {
                    Hook.Invoke(this);
                }
            }
        }

        private void UpdateStats()
        {
            foreach (KeyValuePair<string, CharacterStats.NTHumanStatDoubleData> Pair in LocalStats.DoubleStats) // Update all of our double stats
            {
                string ID = Pair.Key;
                CharacterStats.NTHumanStatDoubleData StatData = Pair.Value;
                SetDoubleStatStrength(Pair.Key, GetDoubleStatUpdate(this, ID));
            }

            foreach (KeyValuePair<string, CharacterStats.NTHumanStatBoolData> Pair in LocalStats.BoolStats) // Update all of our boolean stats
            {
                string ID = Pair.Key;
                CharacterStats.NTHumanStatBoolData StatData = Pair.Value;
                SetBoolStatStrength(Pair.Key, GetBoolStatUpdate(this, ID));
            }
        }

        private void UpdateAfflictions(List<AfflictionPriority> Priorities)
        {
            IReadOnlyCollection<Affliction> CurrentAfflictions = Human.CharacterHealth.GetAllAfflictions();
            IEnumerable<Affliction> FilteredAfflictions = CurrentAfflictions.Where(aff => { return LocalAfflictions.UpdatingAfflictions.ContainsKey(aff.Identifier.ToString()); });
            List < Affliction > SortedAfflictions = FilteredAfflictions.OrderBy(
                                aff => LocalAfflictions.UpdatingAfflictions[aff.Identifier.ToString()].AffTemplate.AffSortID
                            ).ToList();

            SortedAfflictions = SortedAfflictions.Union(LocalAfflictions.LastUpdatedAfflictions).ToList(); // We merge our last updated afflictions with our new afflictions.
            List<string> UpdatedAfflictions = new List<string>(); // We store this so we don't update a duplicate.

            if (SortedAfflictions != null && SortedAfflictions.Count() > 0)
            {
                // Our Current Afflictions Update
                foreach (Affliction RealAff in SortedAfflictions)
                {
                    if (RealAff == null) continue;
                    if (UpdatedAfflictions.Contains(RealAff.Identifier.ToString())) continue;
                    if (!LocalAfflictions.UpdatingAfflictions.ContainsKey(RealAff.Identifier.ToString())) continue;
                    NTHumanAffData Data = LocalAfflictions.UpdatingAfflictions[RealAff.Identifier.ToString()];
                    if (Data.AffTemplate.Const) continue;
                    NTAfflictionType AffType = Data.AffTemplate.Type;
                    UpdateAffliction(AffType, Priorities, RealAff.Identifier.ToString(), Data);
                    UpdatedAfflictions.Add(RealAff.Identifier.ToString());
                }
            }

            if (LocalAfflictions.ConstantAfflictions != null && LocalAfflictions.ConstantAfflictions.Count > 0)
            {
                // Our Constant Afflictions Update
                foreach (KeyValuePair<string, NTHumanAffData> Pair in LocalAfflictions.ConstantAfflictions)
                {
                    if (Pair.Key == null || Pair.Value == null) continue;
                    NTAfflictionType AffType = Pair.Value.AffTemplate.Type;
                    UpdateAffliction(AffType,Priorities,Pair.Key, Pair.Value);
                }
            }

            LocalAfflictions.LastUpdatedAfflictions = SortedAfflictions.Where(aff => { return Human.CharacterHealth.GetAllAfflictions().Contains(aff); }).ToList();
        }


        private static bool PreSymptomCheck(NTHumanAffData Data)
        {
            if (!(Data is NTHumanSymptomData)) return false; // Is this a symptom lol
            NTHumanSymptomData AffData = (NTHumanSymptomData)Data;
            NTSymptom Aff = AffData.SymTemplate;
            if ((!Aff.Const) && AffData.Strength == 0 && (AffData.HumanUpdateTime <= 0 || AffData.HumanUpdateStoptime > 0)) return true;
            return false;
        }

        private static bool PreSymptomCheck(NTHumanLimbAffData Data, LimbType Limb)
        {
            if (!(Data is NTHumanLimbSymptomData)) return false; // Is this a symptom lol
            NTHumanLimbSymptomData AffData = (NTHumanLimbSymptomData)Data;
            NTLimbSymptom Aff = AffData.SymTemplate;
            if ((!Aff.Const) && AffData.Strength[Limb] == 0 && (AffData.HumanUpdateTime[Limb] <= 0 || AffData.HumanUpdateStoptime[Limb] > 0)) return true;
            return false;
        }

        private static void PostSymptomCheck(NTHumanSymptomData SymData)
        {
            if (SymData.HumanUpdateTime > 0)
            {
                SymData.Strength = 100;
                SymData.HumanUpdateTime--;

                if (SymData.HumanUpdateTime <= 0)
                {
                    SymData.Strength = 0;
                }
            }

            if (SymData.HumanUpdateStoptime > 0)
            {
                SymData.Strength = 0;
                SymData.HumanUpdateStoptime--;

                if (SymData.HumanUpdateStoptime <= 0)
                {
                    SymData.Strength = 0;
                }
            }
        }

        private static void PostSymptomCheck(NTHumanLimbSymptomData SymData, LimbType Limb)
        {
            if (SymData.HumanUpdateTime[Limb] > 0)
            {
                SymData.Strength[Limb] = 100;
                SymData.HumanUpdateTime[Limb]--;

                if (SymData.HumanUpdateTime[Limb] <= 0)
                {
                    SymData.Strength[Limb] = 0;
                }
            }

            if (SymData.HumanUpdateStoptime[Limb] > 0)
            {
                SymData.Strength[Limb] = 0;
                SymData.HumanUpdateStoptime[Limb]--;

                if (SymData.HumanUpdateStoptime[Limb] <= 0)
                {
                    SymData.Strength[Limb] = 0;
                }
            }
        }

        private void UpdateAffliction(NTAfflictionType AffType, List<AfflictionPriority> Priorities, string Key, NTHumanAffData Data)
        {
            switch (AffType)
            {
                case NTAfflictionType.NONLIMB:
                case NTAfflictionType.BLOOD:
                case NTAfflictionType.SYMPTOM:
                    // Fetch the data of the affliction
                    string ID = Key;
                    NTHumanAffData AffData = Data;
                    NTAffliction Aff = AffData.AffTemplate;

                    if (!Priorities.Contains(Aff.Priority))
                    {
                        return; // Skip to the next affliction, we don't have the same priority currently.
                    }

                    // Store the previous strength before reading the current value.
                    double PrevStrength = AffClamp(AffData.Strength,Aff);
                    double CurrentStrength = Aff.Real ? GetAfflictionStrength(Human, ID) : PrevStrength; // If real, use the prefab strength, else use custom.

                    AffData.PrevStrength = PrevStrength;
                    AffData.Strength = CurrentStrength;

                    if (AffType == NTAfflictionType.SYMPTOM)
                    {
                        if (PreSymptomCheck(AffData))
                        {
                            return;
                        }
                    }

                    Aff.Update(this, ID, LimbType.Torso, AffData);

                    if (AffType == NTAfflictionType.SYMPTOM)
                    {
                        PostSymptomCheck((NTHumanSymptomData)AffData);
                    }

                    break;

                case NTAfflictionType.LIMB:
                case NTAfflictionType.LIMBSYMPTOM:

                    foreach (LimbType Limb in LimbsToCheck)
                    {
                        // Fetch the data of the affliction
                        string LimbID = Key;
                        NTHumanLimbAffData LimbAffData = (NTHumanLimbAffData)Data;
                        NTLimbAffliction LimbAff = LimbAffData.AffTemplate;

                        if (!Priorities.Contains(LimbAff.Priority) || ((!LimbAff.IgnoreStasis) && GetBoolStatStrength("stasis")))
                        {
                            continue;
                        }

                        double LimbPrevStrength = AffClamp(LimbAffData.Strength[Limb], LimbAff);
                        double LimbCurrentStrength = LimbAff.Real ?  GetAfflictionStrengthLimb(Human, Limb, LimbID) : LimbPrevStrength; // If real, use the prefab strength, else use custom.

                        LimbAffData.PrevStrength[Limb] = LimbPrevStrength;
                        LimbAffData.Strength[Limb] = LimbCurrentStrength;

                        if (PreSymptomCheck(LimbAffData, Limb))
                        {
                            return;
                        }

                        LimbAff.Update(this, LimbID, Limb, LimbAffData);

                        if (AffType == NTAfflictionType.LIMBSYMPTOM)
                        {
                            PostSymptomCheck((NTHumanLimbSymptomData)LimbAffData, Limb);
                        }
                        
                    }

                    break;
            }
        }

        private void SetAfflictionDefaultStrength(NTAfflictionType AffType, string Key, NTHumanAffData Data)
        {
            switch (AffType)
            {
                case NTAfflictionType.SYMPTOM:
                case NTAfflictionType.BLOOD:
                case NTAfflictionType.NONLIMB:

                    // Fetch the data of the affliction
                    string ID = Key;
                    NTHumanAffData AffData = (NTHumanAffData)Data;
                    NTAffliction Aff = AffData.AffTemplate;

                    if (!Aff.Real) return;

                    HF.SetAffliction(Human, ID, (float)Aff.DefaultStrength);
                    break;

                case NTAfflictionType.LIMBSYMPTOM:
                case NTAfflictionType.LIMB:

                    foreach (LimbType Limb in LimbsToCheck)
                    {
                        // Fetch the data of the affliction
                        string LimbID = Key;
                        NTHumanLimbAffData LimbAffData = (NTHumanLimbAffData)Data;
                        NTLimbAffliction LimbAff = LimbAffData.AffTemplate;
                        if (!LimbAff.Real) return;

                        HF.SetAfflictionLimb(Human, LimbID, Limb, (float)LimbAff.DefaultStrength);
                    }
                    break;
            }
        }

        private void SetAfflictionStrengths()
        {
            foreach (KeyValuePair<string,NTHumanAffData> Pair in LocalAfflictions.UpdatingAfflictions)
            {
                NTAfflictionType AffType = Pair.Value.AffTemplate.Type;
                SetAfflictionStrength(AffType, Pair.Key, Pair.Value);
            }
        }

        private void SetAfflictionStrength(NTAfflictionType AffType, string ID, NTHumanAffData Data)
        {

            switch (AffType)
            {
                case NTAfflictionType.NONLIMB:
                case NTAfflictionType.BLOOD:
                case NTAfflictionType.SYMPTOM:

                    // Fetch the data of the affliction

                    NTHumanAffData AffData = (NTHumanAffData)Data;
                    NTAffliction Template = AffData.AffTemplate;

                    if (AffData.Strength == 0 || (!Template.Real)) return;

                    ApplyAfflictionChange(Human, ID, (float)AffData.Strength, (float)AffData.PrevStrength, (float)Template.MinStrength, (float)Template.MaxStrength);

                    break;

                case NTAfflictionType.LIMBSYMPTOM:
                case NTAfflictionType.LIMB:

                    foreach (LimbType Limb in LimbsToCheck)
                    {
                        // Fetch the data of the affliction
                        NTHumanLimbAffData LimbAffData = (NTHumanLimbAffData)Data;
                        NTLimbAffliction LimbTemplate = LimbAffData.AffTemplate;

                        if (LimbAffData.Strength[Limb] == 0 || (!LimbTemplate.Real)) return;

                        ApplyAfflictionChangeLimb(Human, Limb, ID, (float)LimbAffData.Strength[Limb], (float)LimbAffData.PrevStrength[Limb], (float)LimbTemplate.MinStrength, (float)LimbTemplate.MaxStrength);
                    }

                    break;
            }
        }

    }

    /// <summary>
    /// The Neurotrauma version of a Monster Character.
    /// </summary>
    public class NTMonster(Character Monster)
    {
        public Character Monster = Monster; // Our Monster Ref
        
        public void Update()
        {
            double BloodLoss = GetAfflictionStrength(Monster, "bloodloss", 0);
            double OxygenLow = GetAfflictionStrength(Monster, "oxygenlow", 0);

            if (BloodLoss > 0)
            {
                AddAffliction(Monster, "organdamage", (float)BloodLoss * 2, Monster);
                SetAffliction(Monster, "bloodloss", 0, Monster, (float)BloodLoss);
            }
            else if (OxygenLow > 50)
            {
                AddAffliction(Monster, "organdamage", (float) (OxygenLow - 50) * 2, Monster);
                SetAffliction(Monster, "oxygenlow", 50, Monster, (float) OxygenLow);
            }
        }
    }


    // ---------------------------------------- The Human Update -------------------------------------------------- \\

    public static NTHuman CharacterToNTHuman(Character Character)
    {
        if (!UpdatingHumans.ContainsKey(Character)) return null;
        return UpdatingHumans[Character];
    }

    public static void AddCharacterToUpdate(Character character)
    {
        if (character != null)
        {
            if (UpdatingHumans.ContainsKey(character)) return;
            if (character.IsHuman)
            {
                AddHumanToUpdate(character);
                //CleanBotomy(character); This breaks everything for some reason. May somebody investigate?
            }
            else
            {
                AddMonsterToUpdate(character);
            }
        }
    }

    public static void RemoveCharacterFromUpdate(Character target)
    {
        if (target is Character)
        {
            Character NewCharacter = target;
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
        if (!UpdatingHumans.ContainsKey(AddedCharacter))
        {
            NTHuman NewNTHuman = new NTHuman(AddedCharacter); // Hopefully this wont create a memory leak.
            UpdatingHumans[AddedCharacter] = NewNTHuman;
        }
    }

    public static void RemoveHumanFromUpdate(Character RemovingCharacter) // Probably a better way to do this.
    {
        if (!UpdatingHumans.ContainsKey(RemovingCharacter)) return;
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
    private static  List<AfflictionPriority> GetLowestPriority(List<AfflictionPriority> CurrentPriorities)
    {
        List<AfflictionPriority> NewPriorities = CurrentPriorities;

        switch (CurrentPriorities.Count)
        {
            case 0:
                NewPriorities.Add(AfflictionPriority.HIGH);
                break;

            case 1:
                NewPriorities.Add(AfflictionPriority.MEDIUM);
                break;

            case 2:
                NewPriorities.Add(AfflictionPriority.LOW);
                break;

            case 3:
                NewPriorities.Clear();
                NewPriorities.Add(AfflictionPriority.HIGH);
                break;

            default:
                NewPriorities.Clear();
                NewPriorities.Add(AfflictionPriority.HIGH);
                break;
        }

        return NewPriorities;
    }

    private static int Interval = 120;
    private static int Tick = 0;
    private static double NTDeltaTime = UpdateIntervalHigh / 120;
    private static List<AfflictionPriority> Priorities = new();
    // Gets called 60 times a second
    public static void ThinkUpdate()
    {
        // If game paused we just skip
        if ((!HF.InGame()) || HF.GameIsPaused()) return;

        Tick--; // Decrement our tick.
        if (!(Tick < 0)) { return; }
        else { Tick = Interval; }

        if (!NTConfig.Get("NT_Calculations", true)) return; // Check the config.

        Priorities = GetLowestPriority(Priorities);

        NT.DeltaTime = NTDeltaTime;
        Update(Priorities);
    }

    private static void Update(List<AfflictionPriority> priorities)
    {
        // Our Single Player check for fetching humans.
        if (GameIsSingleplayer() && (UpdatingHumans.Count + UpdatingMonsters.Count > Character.CharacterList.Count))
        {
            UpdatingHumans.Clear();
            UpdatingMonsters.Clear();

            foreach (Character character in Character.CharacterList)
            {
                if (character.IsHuman)
                {
                    AddHumanToUpdate(character);
                }
                else if (!character.IsHuman)
                {
                    AddMonsterToUpdate(character);
                }
            }
        }

        if (UpdatingMonsters.Count > 0)
        {
            //Task MonsterUpdateTask = new(UpdateMonsters); // We create a new task to run monsters along humans. This should help greatly with mods such as barotraumatic.
            // MonsterUpdateTask.Start();
            UpdateMonsters();
            UpdateHumans(priorities);
            //MonsterUpdateTask.Wait();
        }
        else 
        {
            UpdateHumans(priorities);
        }

        if (UsingLuaAddons()) HumanUpdateLuaSync.Update(UpdatingHumans.Values.ToList(),priorities);
    }
    
    private static void UpdateHumans(List<AfflictionPriority> priorities)
    {
        List<Character> QueuedCharacters = new();
        int index = 1; // thanks lua

        foreach (KeyValuePair<Character, NTHuman> Pair in UpdatingHumans)
        {

            if (Pair.Key?.IsDead == true || Pair.Key == null || Pair.Value == null || Pair.Key.IdFreed)
            {
                QueuedCharacters.Add(Pair.Key);
                continue;
            }

            double Delay = (((index + 1) / UpdatingHumans.Count) * NT.DeltaTime * 1000); // Delay our update to prevent sutters.

            LuaCsSetup.Instance.Timer.Wait((params object[] _) => {
                Pair.Value.Update(priorities);
            }, (int)Delay);

            index++;
        }

        foreach (Character character in QueuedCharacters)
        {
            RemoveHumanFromUpdate(character);
        }
    }

    private static void UpdateMonsters()
    {
        List<NTMonster> QueuedCharacters = new();
        int index = 1; // thanks lua

        foreach (NTMonster Monster in UpdatingMonsters)
        {

            if (Monster.Monster?.IsDead == true || Monster == null || Monster.Monster == null || Monster.Monster.IdFreed)
            {
                QueuedCharacters.Add(Monster);
                continue;
            }

            double Delay = (((index + 1) / UpdatingMonsters.Count) * NT.DeltaTime * 1000); // Delay our update to prevent sutters.

            LuaCsSetup.Instance.Timer.Wait((params object[] _) => {
                Monster.Update();
            }, (int)Delay);

            index++;
        }

        foreach (NTMonster character in QueuedCharacters)
        {
            RemoveMonsterFromUpdate(character.Monster);
        }

        return;
    }

    public static void CleanBotomy(Character C)
    {
        if (HasAffliction(C, "surgeryincision")) SetAffliction(C, "tshocktimeout", 15, C, 0);

        if (C.TeamID == CharacterTeamType.Team1 || C.TeamID == CharacterTeamType.Team2)
        {
            SetAffliction(C, "luabotomypurger", 2, C, 0);
            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                SetAffliction(C,"luabotomy",0.1f,C,0);
            }, 8000);
        }
    }

}