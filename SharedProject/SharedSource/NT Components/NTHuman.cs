using static Neurotrauma.NTAfflictions;
using static Neurotrauma.NTAfflictions.NTSymptoms;

namespace Neurotrauma;

public class NTHuman
{
    public static Dictionary<Character, NTHuman> NTHumans = new Dictionary<Character, NTHuman>();

    // TODO: We might want to get our own ?
    private static IEventService EventService = LuaCsSetup.Instance.EventService;

    public Dictionary<String, bool> BoolStats { get; private set; }
    public Dictionary<String, float> FloatStats { get; private set; }
    private NTSymptomsStorage Symptoms;
    public Character Human {  get; private set; }

    public CharacterTags Tags { get; private set; }

    public NTHuman(Character Human)
    {
        this.Human = Human;
        this.Symptoms = new NTSymptomsStorage(this);

        this.BoolStats = new Dictionary<String, bool>();
        this.FloatStats = new Dictionary<String, float>();

        this.Tags = new CharacterTags();

        NTHumans.Add(Human, this);

        foreach (var item in NTStats.StatRegistry)
        {
            string name = item.Key;
            NTStats.NTStat stat = item.Value;

            if (stat is NTStats.NTStatBool)
            {
                this.BoolStats.Add(name, ((NTStats.NTStatBool)stat).DefaultValue);
            }
            else if (stat is NTStats.NTStatFloat)
            {
                this.FloatStats.Add(name, ((NTStats.NTStatFloat)stat).DefaultStrength);
            }
        }
    }

    public static NTHuman? getNTHumanFromCharacter(Character Human)
    {
        if (NTHumans.ContainsKey(Human))
        {
            return NTHumans[Human];
        }
        return null;
    }

    public static void RemoveNTHuman(NTHuman Human)
    {
        NTHumans.Remove(Human.Human);
    }

    #region Stats
    // ========== STATS    ==========

    public void UpdateStats(float deltaTime)
    {
        EventService.Call("Neurotrauma.HumanUpdate.UpdateStats", this);

        foreach (var i in BoolStats)
        {
            string id = i.Key;
            bool val = i.Value;

            var stat = NTStats.StatRegistry[id];

            BoolStats[id] = ((NTStats.NTStatBool)stat).UpdateFunction?.Invoke(this, deltaTime) ?? val;
        }

        foreach (var i in FloatStats)
        {
            string id = i.Key;
            float val = i.Value;

            var stat = NTStats.StatRegistry[id];

            FloatStats[id] = ((NTStats.NTStatFloat)stat).UpdateFunction?.Invoke(this, deltaTime) ?? val;
        }
    }

    public bool GetBoolStat(string StatID)
    {
        if (BoolStats.ContainsKey(StatID))
        {
            return BoolStats[StatID];
        }

        HF.PrintError($"Trying to get an unknown bool stat : {StatID}. Target character : {this.Human.DisplayName}");

        return false;
    }

    public float GetFloatStat(string StatID)
    {
        if (FloatStats.ContainsKey(StatID))
        {
            return FloatStats[StatID];
        }

        HF.PrintError($"Trying to get an unknown double stat : {StatID}. Target character : {this.Human.DisplayName}");

        return 0;
    }

    public void SetBoolStat(string StatID, bool val)
    {
        if (BoolStats.ContainsKey(StatID))
        {
            BoolStats[StatID] = val;
            return;
        }

        HF.PrintError($"Trying to set an unknown bool stat : {StatID}. Target character : {this.Human.DisplayName}");
    }

    public void SetFloatStat(string StatID, float val)
    {
        if (FloatStats.ContainsKey(StatID))
        {
            FloatStats[StatID] = val;
            return;
        }

        HF.PrintError($"Trying to set an unknown double stat : {StatID}. Target character : {this.Human.DisplayName}");
    }

    #endregion

    #region Afflictions

    // TODO : add debug prints

    public Affliction GetAffliction(string AfflictionID)
    {
        return this.Human.CharacterHealth.GetAffliction(AfflictionID);
    }

    public Affliction GetAfflictionLimb(string AfflictionID, LimbType Limb)
    {
        return this.Human.CharacterHealth.GetAffliction(AfflictionID, this.Human.AnimController.GetLimb(Limb));
    }

    public float GetAfflictionStrength(string AfflictionID, float DefaultValue = 0f)
    {
        Affliction aff = this.GetAffliction(AfflictionID);

        if (aff == null)
        {
            return DefaultValue;
        }

        return HF.NormalizeFloat(aff.Strength);
        

    }

    public float GetAfflictionStrengthLimb(string AfflictionID, LimbType Limb, float DefaultValue = 0f)
    {

        Affliction aff = this.GetAfflictionLimb(AfflictionID, Limb);

        if (aff == null)
        {
            return DefaultValue;
        }

        return HF.NormalizeFloat(aff.Strength);
    }

    public void AddAffliction(string AfflictionID, float Strength, NTHuman Aggressor) => AddAffliction(AfflictionID, Strength, Aggressor.Human);

    public void AddAffliction(string AfflictionID, float Strength, Character? Aggressor = null)
    {
        float prev = this.GetAfflictionStrength(AfflictionID, 0f);
        this.SetAffliction(AfflictionID, prev + Strength, Aggressor == null ? this.Human : Aggressor);
    }

    public void AddAfflictionLimb(string AfflictionID, LimbType Limb, float Strength, NTHuman Aggressor) => AddAfflictionLimb(AfflictionID, Limb, Strength, Aggressor.Human);

    public void AddAfflictionLimb(string AfflictionID, LimbType Limb, float Strength, Character? Aggressor = null)
    {
        if (Aggressor == null)
        {
            Aggressor = this.Human;
        }

        if (Strength < 0)
        {
            this.Human.CharacterHealth.ReduceAfflictionOnLimb(
                HF.GetCharacterLimb(this.Human, Limb),
                AfflictionID,
                HF.NormalizeFloat(-Strength),
                null,
                Aggressor);

            return;
        }

        if (!AfflictionPrefab.Prefabs.TryGet(AfflictionID, out AfflictionPrefab? Prefab) || Prefab == null || this.Human == null || this.Human.CharacterHealth == null)
        {
            return;
        }

        float Resistance = this.Human.CharacterHealth.GetResistance(Prefab, Limb);

        if (Resistance >= 1)
        {
            return;
        }

        float ScaledStrength = Strength * this.Human.CharacterHealth.MaxVitality / 100 / (1 - Resistance);
        Affliction Affliction = Prefab.Instantiate(ScaledStrength, Aggressor);
        bool RecalculateVitality = NTC.AfflictionsAffectingVitality.Contains(AfflictionID);

        // No need to manually calculate strength, just stack it - Lukako
        this.Human.CharacterHealth.ApplyAffliction(
            this.Human.AnimController.GetLimb(Limb),
            Affliction,
            true,
            false,
            RecalculateVitality
        );
    }

    public void AddAfflictionResisted(string AfflictionID, float Strength, Character? Aggressor = null)
    {
        if (Aggressor == null)
        {
            Aggressor = this.Human;
        }

        float PrevStrength = GetAfflictionStrength(AfflictionID);
        Strength *= 1 - HF.GetResistance(this.Human, AfflictionID);

        SetAffliction(AfflictionID, Strength + PrevStrength, Aggressor);
    }

    public void SetAffliction(string AfflictionID, float Strength, Character? Aggressor = null)
    {
        SetAfflictionLimb(AfflictionID, LimbType.Torso, HF.NormalizeFloat(Strength), Aggressor == null ? this.Human : Aggressor);
    }

    public void SetAfflictionLimb(string AfflictionID, LimbType Limb, float Strength, Character? Aggressor = null)
    {
        // This Error was in the original but not ported for some reason?
        if (!AfflictionPrefab.Prefabs.TryGet(AfflictionID, out AfflictionPrefab? Prefab) || Prefab == null || this.Human == null || this.Human.CharacterHealth == null)
        {
            LuaCsLogger.LogError(string.Format(
                "Can't apply affliction to character limb\ncharacter = {0}, limbtype = {1}, affliction = {2}, strength = {3}",
                this.Human != null ? this.Human.Name : "nil",
                Limb.ToString(),
                Prefab != null ? $"{Prefab.Name} ({Prefab.Identifier})" : AfflictionID ?? "nil",
                Strength.ToString("F3")
            ));
            return;
        }

        float Resistance = this.Human.CharacterHealth.GetResistance(Prefab, Limb);
        if (Resistance >= 1)
        {
            return;
        }

        // Flip the resistances effects so we get the right values accounting for them
        float ScaledStrength = Strength * this.Human.CharacterHealth.MaxVitality / 100 / (1 - Resistance);
        Affliction Affliction = Prefab.Instantiate(HF.NormalizeFloat(ScaledStrength), Aggressor);
        bool RecalculateVitality = NTC.AfflictionsAffectingVitality.Contains(AfflictionID);

        this.Human.CharacterHealth.ApplyAffliction(
            this.Human.AnimController.GetLimb(Limb),
            Affliction,
            false,
            false,
            true
        );
    }

    public bool HasAffliction(string AfflictionID = "", float MinAmount = 0)
    {
        if (AfflictionID == "" || this.Human.CharacterHealth == null)
        {
            return false;
        }

        // Is the affliction null?
        Affliction Aff = GetAffliction(AfflictionID);
        if (Aff == null)
        {
            return false;
        }

        float AffStrength = Aff.Strength;
        if (AffStrength > MinAmount)
        {
            return true;
        }

        return false;
    }

    public bool HasAfflictionLimb(string AfflictionID = "", LimbType Limb = LimbType.Torso, float MinAmount = 0)
    {
        if (AfflictionID == "" || this.Human.CharacterHealth == null)
        {
            return false;
        }

        // Is the affliction null?
        Affliction Aff = GetAfflictionLimb(AfflictionID, Limb);
        if (Aff == null)
        {
            return false;
        }

        float AffStrength = Aff.Strength;
        if (AffStrength >= MinAmount)
        {
            return true;
        }

        return false;
    }

    private static readonly List<List<LimbType>> LocalLimbsToCheck = [[LimbType.LeftArm, LimbType.LeftForearm, LimbType.LeftHand],[LimbType.RightArm, LimbType.RightForearm, LimbType.RightHand],
                                                        [LimbType.LeftLeg, LimbType.LeftThigh, LimbType.LeftFoot],[LimbType.RightLeg, LimbType.RightThigh, LimbType.RightFoot]];

    public bool HasAfflictionExtremity(string AfflictionID = "", LimbType GivenLimbType = LimbType.Torso, double MinAmount = 0.5)
    {
        Affliction? Aff = null;
        
        foreach (List<LimbType> SubList in LocalLimbsToCheck)
        {
            if (HF.NormalizeLimbType(GivenLimbType) == SubList[0])
            {
                Aff = GetAfflictionLimb(AfflictionID, SubList[0]);
                if (Aff == null)
                {
                    Aff = GetAfflictionLimb(AfflictionID, SubList[1]);
                }
                if (Aff == null)
                {
                    Aff = GetAfflictionLimb(AfflictionID, SubList[2]);
                }
                break; // We can end the for loop, we found what we were looking for.
            }
        }

        bool Res = false;
        if (Aff != null)
        {
            Res = Aff.Strength >= MinAmount;
        }

        return Res;
    }

    #endregion

    #region Update
    // ==== UPDATE ====

    public void PreHook()
    {
        EventService.Call("Neurotrauma.HumanUpdate.PreHook", this);
        return;
    }

    public Dictionary<LimbType, List<String>> FetchAfflictions(List<AfflictionPriority> priorities)
    {
        EventService.Call("Neurotrauma.HumanUpdate.FetchAfflictions", this, priorities);
        IReadOnlyCollection<Affliction> afflictions = this.Human.CharacterHealth.GetAllAfflictions();

        var r = new Dictionary<LimbType, List<String>>();

        // Remember to add constant afflictions from NTAfflictions.ConstantAfflicitonPrefabs

        return r;
        
    }

    /// <summary>
    /// First calls UpdateSymptoms() then calls every NTAffliction Update function for every NTAffliction in the given list.
    /// </summary>
    /// <param name="AfflictionsList">The list of affliction IDs oredered by LimbType</param>
    public void UpdateAfflictions(Dictionary<LimbType, List<String>> AfflictionsList)
    {
        this.UpdateSymptoms();

        EventService.Call("Neurotrauma.HumanUpdate.UpdateAfflictions", this, AfflictionsList);
        foreach (var limbList in AfflictionsList)
        {
            var limb = limbList.Key;

            foreach (var id in limbList.Value)
            {

                var aff = NeurotraumaInit.NTAfflLoader.Get(id);

                if (aff == null)
                {
                    HF.PrintError($"Error getting a NTAfflictionPrefab from the ID {id} : The ID does not match any NTAfflictionPrefab!");
                    continue;
                }

                float deltaTime = ((float)NTHumanUpdate.GetUpdateInterval(aff.Priority)) / 60f;

                aff.Update(this, id, limb, deltaTime);
            }
        }

    }

    public void PostHook()
    {
        EventService.Call("Neurotrauma.HumanUpdate.PostHook", this);

        if (this.Human != null && this.Human.IdFreed == false)
        {
            this.Human.SetStun((float)GetAfflictionStrength("stun"));

            if (this.Human.Health < 0)
            {
                SetSymptomTrue("unconsciousness", 2);
                SetAffliction("unconsciousness", 100);
            }
        }

        return;
    }

    #endregion

    #region Symptoms

    // ========== SYMPTOMS ==========

    /// <summary>
    /// Make a symptom visible for the given Duration on the given Character. The duration is in amount of HumanUpdate cycles (should be 2s by default); the recommended Duration is 2. The LimbType is only relevant if the affliction is limbspecific.
    /// </summary>
    public void SetSymptomTrue(string AfflictionID, int Duration, LimbType Limb = LimbType.None)
    {
        NTAfflictionPrefab? aff = NeurotraumaInit.NTAfflLoader.Get(AfflictionID);

        if (aff == null)
        {
            HF.PrintError($"Error getting a NTAfflictionPrefab from the ID {AfflictionID} : The ID does not match any NTAfflictionPrefab!");
            return;
        }

        this.SetSymptomTrue(aff, Duration, Limb);
    }

    /// <summary>
    /// Make a symptom visible for the given Duration on the given Character. The duration is in amount of HumanUpdate cycles (should be 2s by default); the recommended Duration is 2. The LimbType is only relevant if the affliction is limbspecific.
    /// </summary>
    public void SetSymptomTrue(NTAfflictionPrefab Affliction, int Duration, LimbType Limb = LimbType.None)
    {
        if (!Affliction.Symptom)
        {
            HF.PrintError($"Trying to interact with the Affliction {Affliction.ID} but it isn't a symptom!");
            return;
        }

        this.Symptoms.AddSymptom(Affliction, Duration, Limb);
        
    }

    /// <summary>
    /// Remove a the given symptom from the given Character. The LimbType is only relevant if the affliction is limbspecific.
    /// </summary>
    public void SetSymptomFalse(string AfflictionID, LimbType Limb = LimbType.None)
    {
        NTAfflictionPrefab? aff = NeurotraumaInit.NTAfflLoader.Get(AfflictionID);

        if (aff == null)
        {
            HF.PrintError("Error getting a NTAfflictionPrefab from the ID : The ID does not match any NTAfflictionPrefab!");
            return;
        }

        this.SetSymptomFalse(aff, Limb);
    }

    /// <summary>
    /// Remove the given symptom from the given Character. The LimbType is only relevant if the affliction is limbspecific.
    /// </summary>
    public void SetSymptomFalse(NTAfflictionPrefab Affliction, LimbType Limb = LimbType.None)
    {
        if (!Affliction.Symptom)
        {
            HF.PrintError($"Trying to interact with the Affliction {Affliction.ID} but it isn't a symptom!");
            return;
        }

        this.Symptoms.RemoveSymptom(Affliction, Limb);

    }

    public bool HasSymptom(String AfflictionID, LimbType Limb = LimbType.None)
    {

        NTAfflictionPrefab? aff = NeurotraumaInit.NTAfflLoader.Get(AfflictionID);

        if (aff == null)
        {
            HF.PrintError($"Error getting a NTAfflictionPrefab from the ID {AfflictionID} : The ID does not match any NTAfflictionPrefab!");
            return false;
        }

        return this.HasSymptom(aff, Limb);
    }

    public bool HasSymptom(NTAfflictionPrefab Affliction, LimbType Limb = LimbType.None)
    {
        if (!Affliction.Symptom)
        {
            HF.PrintError($"Trying to interact with the Affliction {Affliction.ID} but it isn't a symptom!");
            return false;
        }

        if (Affliction.LimbSpecific == false) Limb = LimbType.None;

        return this.Symptoms.HasSymptom(Affliction, Limb);

    }

    /// <summary>
    /// This counts down the duration of every symptoms and remove the ones where duration reaches 0.
    /// </summary>
    public void UpdateSymptoms()
    {
        EventService.Call("Neurotrauma.HumanUpdate.UpdateSymptoms", this);
        this.Symptoms.UpdateSymptoms();
    }


    // TODO: find a better name and maybe make a better system but this will have to do for now
    private class NTSymptomsStorage
    {
        private NTHuman Human;


        private Dictionary<String, NTSymptomData> HeadAfflictions;
        private Dictionary<String, NTSymptomData> TorsoAfflictions;
        private Dictionary<String, NTSymptomData> RightArmAfflictions;
        private Dictionary<String, NTSymptomData> LeftArmAfflictions;
        private Dictionary<String, NTSymptomData> RightLegAfflictions;
        private Dictionary<String, NTSymptomData> LeftLegAfflictions;
        private Dictionary<String, NTSymptomData> NonLimbSpecificAfflictions;

        public NTSymptomsStorage(NTHuman Human)
        {
            this.Human = Human;

            this.HeadAfflictions = new Dictionary<String, NTSymptomData>();
            this.TorsoAfflictions = new Dictionary<String, NTSymptomData>();
            this.RightArmAfflictions = new Dictionary<String, NTSymptomData>();
            this.LeftArmAfflictions = new Dictionary<String, NTSymptomData>();
            this.RightLegAfflictions = new Dictionary<String, NTSymptomData>();
            this.LeftLegAfflictions = new Dictionary<String, NTSymptomData>();
            this.NonLimbSpecificAfflictions = new Dictionary<String, NTSymptomData>();
        }

        /// <summary>
        /// Returns the Dict containing every symptoms on this limb. Use LimbType.None for the non limb specific Dict.
        /// </summary>
        public Dictionary<String, NTSymptomData> getDictFromLimb(LimbType limb)
        {
            switch(limb)
            {
                case LimbType.Head: return this.HeadAfflictions;
                case LimbType.Torso: return this.TorsoAfflictions;
                case LimbType.RightArm: return this.RightArmAfflictions;
                case LimbType.LeftArm: return this.LeftArmAfflictions;
                case LimbType.RightLeg: return this.RightLegAfflictions;
                case LimbType.LeftLeg: return this.LeftLegAfflictions;
                default: return this.NonLimbSpecificAfflictions;
            }

        }

        public void AddSymptom(NTAfflictionPrefab Aff, int Duration, LimbType limb)
        {
            Dictionary<String, NTSymptomData> l = this.getDictFromLimb(limb);

            foreach (var e in l)
            {
                String ID = e.Key;
                NTSymptomData Symptom = e.Value;

                // If the symptom is already present, we set the duration as the highest between the new duration and the one already set.
                if (ID == Aff.ID) Symptom.Duration = Symptom.Duration > Duration ? Symptom.Duration : Duration;
                return;
            }

            NTSymptomData d = new NTSymptomData(Aff, Duration, Aff.LimbSpecific ? limb : LimbType.None);

            l.Add(Aff.ID, d);

            if (Aff.LimbSpecific)
            {
                this.Human.SetAfflictionLimb(Aff.ID, limb, (float) Aff.MaxStrength);
            } else
            {
                this.Human.SetAffliction(Aff.ID, (float) Aff.MaxStrength);
            }

        }

        public void RemoveSymptom(NTAfflictionPrefab Aff, LimbType limb)
        {
            if (this.HasSymptom(Aff, limb))
            {
                Dictionary<String, NTSymptomData> l = this.getDictFromLimb(limb);
                l.Remove(Aff.ID);

                if (Aff.LimbSpecific)
                {
                    this.Human.SetAfflictionLimb(Aff.ID, limb, 0);
                }
                else
                {
                    this.Human.SetAffliction(Aff.ID, 0);
                }
            }

        }

        public bool HasSymptom(NTAfflictionPrefab Aff, LimbType limb)
        {
            Dictionary<String, NTSymptomData> l = this.getDictFromLimb(limb);

            return l.ContainsKey(Aff.ID);
        }

        
        /// <summary>
        /// This counts down the duration of every symptoms and remove the ones where duration reaches 0.
        /// </summary>
        public void UpdateSymptoms()
        {
/* ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⡴⠞⠛⠉⠀⠊⡐⠳⣣⡀⠀⣠⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⠶⠁⠄⠀⠀⠀⠀⠀⠈⢶⠰⠞⠃⣐⣞⠛⠲⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡠⣪⣋⠀⠤⠀⠀⠀⠀⠀⠀⠀⠀⠠⠒⠩⢥⠒⢝⠬⣪⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⣰⠁⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠘⠭⣳⣺⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣱⠃⠀⠀⠀⠀⠀⠀⠀⠀⠠⡀⠀⠀⠀⠀⢀⣀⣛⡋⢉⣉⡓⢺⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡿⡆⠀⠀⠀⠀⠀⠀⡠⢈⠀⡀⠂⠈⣁⣀⡉⠭⠠⡶⣶⣷⣿⣿⣅⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢳⠀⡀⠀⠀⣀⢤⣠⣉⣨⡙⠒⣶⠚⢿⣉⣂⣡⣄⡀⠿⣿⣿⣿⣝⢏⢆⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣿⡠⢐⣩⠕⢊⢽⣭⢽⣿⣷⣿⠈⢛⣫⢭⣥⢈⠉⠢⣫⣹⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢈⣥⣾⣿⠏⠠⣿⡌⠉⠀⢩⢿⠊⠀⣮⡀⠀⠉⠀⢳⣤⣻⠛⣻⣿⣿⠏⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡞⣿⠉⣿⠀⠀⠈⠀⣠⠒⣍⣼⠀⠀⡢⠙⠢⡀⠀⠁⠀⡻⣍⢡⢿⡛⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⢸⣿⡄⢠⢄⠢⢀⠊⠑⠐⢻⠞⠒⠒⣇⠁⠢⢀⠀⢠⢜⠧⣌⠃⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢣⢻⢷⡌⡉⢺⣦⠀⠀⠀⢄⣮⣦⣴⢫⠆⠀⠐⠑⣵⣒⠩⠀⠀⠀⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠑⠢⠿⠬⢴⡿⠀⢀⣴⡿⠏⠄⠜⠷⣎⠢⡀⠈⣻⠿⠋⠀⠀⠀⡃⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣵⡂⢳⣀⠴⠶⠷⠾⠦⣌⠃⠘⢊⠿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢹⠃⢙⡷⢸⣶⢶⣞⠉⠈⡣⠀⣸⡇⠀⠀⠀⠀⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⢧⡘⠈⠀⠀⠀⠀⠈⠙⠄⣀⠝⢧⠄⠀⠀⠀⢠⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡴⢣⢎⠳⢦⣀⣠⣦⣀⣔⡤⠞⠁⠀⣹⡨⣄⡂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⠹⡝⣹⠁⠀⠀⠐⠫⠿⠛⠋⠀⠀⠀⢠⢿⣏⠿⣮⡆⡤⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡆⢀⠞⣸⡇⠋⠀⠀⠀⠐⢄⠀⠀⠀⠀⠀⠀⠎⣾⠸⣄⣪⡽⢧⡁⢀⡀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢂⣠⡞⡀⠊⡐⢹⡰⡀⠀⠀⢀⠀⠀⠀⠀⠀⠀⠀⣎⢲⢸⢆⠫⡁⠫⡢⣳⡄⢢⣴⠄⣀⡀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⣴⣼⣿⡏⣠⡥⡸⢆⣚⣧⠡⡀⠀⢀⡀⠀⠀⠀⡀⠀⢔⠿⠇⣼⠒⢾⡮⣾⠋⠹⢷⠀⠱⣍⢸⠽⢷⣄⠀
⠀⠀⠀⠀⠀⢀⣠⣄⢄⠲⣿⣄⣱⣾⣒⠇⡹⢳⣰⢚⠉⠡⠉⠚⠃⠈⠢⠦⠊⠀⠀⠀⠀⠉⢈⡄⠀⢠⠈⠲⣄⣷⣇⢀⢛⡃⠀⣤⢜⣵
⠀⠀⠀⣀⣤⢳⢾⣦⣰⠀⡿⣝⠣⢳⡀⠀⣷⠈⠂⢸⠀⠀⠡⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⢹⠀⠀⢸⡆⠀⡟⢻⣗⣾⢦⢀⡄⢸⣷⣜
⠀⠀⠴⣿⣻⢯⡎⡇⢪⡦⠿⣿⣿⠿⣯⡇⠈⡇⠘⠀⡀⠀⠀⠠⡀⠀⠀⠀⠀⠀⠀⠀⡀⠀⣸⠄⠀⡏⣿⠀⣿⣿⡿⢫⣼⡟⢻⣾⣿⣿
⠀⡮⡸⠯⣿⣫⠿⡈⡌⣿⡅⢹⣿⣆⠘⣇⢠⠘⡄⡄⡇⠀⠀⠀⠐⡄⠀⠀⠀⠀⠀⠔⠀⠀⣿⠀⣼⣷⢿⠆⢻⡿⡇⠘⣟⣷⠬⣯⣿⣿
⠈⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠈⠉⠈⠁⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⠁⠉⠉⠉⠉⠉⠉⠁⠀⠉⠉⠉⠉⠉⠉
probably the most disgusting code i've ever written
*/


            foreach (var item in HeadAfflictions)
            {
                item.Value.Duration--;
                if (item.Value.Duration <= 0) 
                { 
                    this.Human.SetAfflictionLimb(item.Value.Affliction.ID, LimbType.Head, 0);
                    HeadAfflictions.Remove(item.Key);
                }
            }

            foreach (var item in TorsoAfflictions)
            {
                item.Value.Duration--;
                if (item.Value.Duration <= 0) 
                { 
                    this.Human.SetAfflictionLimb(item.Value.Affliction.ID, LimbType.Torso, 0);
                    TorsoAfflictions.Remove(item.Key);
                }
            }

            foreach (var item in RightArmAfflictions)
            {
                item.Value.Duration--;
                if (item.Value.Duration <= 0) 
                { 
                    this.Human.SetAfflictionLimb(item.Value.Affliction.ID, LimbType.RightArm, 0);
                    RightArmAfflictions.Remove(item.Key);
                }
            }

            foreach (var item in LeftArmAfflictions)
            {
                item.Value.Duration--;
                if (item.Value.Duration <= 0) 
                { 
                    this.Human.SetAfflictionLimb(item.Value.Affliction.ID, LimbType.LeftArm, 0);
                    LeftArmAfflictions.Remove(item.Key);
                }
            }

            foreach (var item in RightLegAfflictions)
            {
                item.Value.Duration--;
                if (item.Value.Duration <= 0) 
                { 
                    this.Human.SetAfflictionLimb(item.Value.Affliction.ID, LimbType.RightLeg, 0); 
                    RightLegAfflictions.Remove(item.Key);
                }
            }

            foreach (var item in LeftLegAfflictions)
            {
                item.Value.Duration--;
                if (item.Value.Duration <= 0) 
                { 
                    this.Human.SetAfflictionLimb(item.Value.Affliction.ID, LimbType.LeftLeg, 0);
                    LeftLegAfflictions.Remove(item.Key);
                }
            }

            foreach (var item in NonLimbSpecificAfflictions)
            {
                item.Value.Duration--;
                if (item.Value.Duration <= 0) { 
                    this.Human.SetAffliction(item.Value.Affliction.ID, 0);
                    NonLimbSpecificAfflictions.Remove(item.Key);
                }
            }
        }
 
    }
    #endregion

    #region Tags

    /// <summary>
    /// Stores the Tags that our character has. Used by NTCompat for adding/setting tags and for giving speed multipliers.
    /// This acts as a replacement for NT's old Data system it used.
    /// </summary>
    public class CharacterTags
    {
        public Dictionary<string, float> Tags = new();

        public void SetTag(string Prefix, string TagID, float Amount = 1)
        {
            Tags[Prefix + "_" + TagID] = Amount;
        }

        public void SetTagsByPrefix(string Prefix, float Amount)
        {
            foreach (KeyValuePair<string, float> Pair in Tags) // Why is this read only?????
            {
                if (Pair.Key.StartsWith(Prefix))
                {
                    Tags[Pair.Key] = Amount;
                }
            }
        }

        public void SetTagsByTagID(string TagID, float Amount)
        {
            foreach (KeyValuePair<string, float> Pair in Tags)
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

        public float GetTag(string Prefix, string TagID)
        {
            if (!HasTag(Prefix, TagID)) return 1;
            return Tags[Prefix + "_" + TagID];
        }
    }

    #endregion

}