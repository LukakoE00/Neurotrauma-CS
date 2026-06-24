using MoonSharp.Interpreter;
using static Barotrauma.LuaCs.NetworkingService;
using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma
{

    /// <summary>
    /// Determines how often an affliction gets updated; Low every 6 seconds, Medium every 4 seconds and High every 2 seconds.
    /// </summary>
    public enum AfflictionPriority : int
    {
        LOW = 6 * 60,  // Every 6s
        MEDIUM = 4 * 60, // Every 4s
        HIGH = 2 * 60 // Every 2s
    }

    // Contains the list of every affliction defined by Neurotrauma. Addons should add their afflictions there.
    // We should provide functions to Lua to add Afflictions here. 
    public static class NTAfflictions
    {
        public static Dictionary<string, NTAffliction> Afflictions { get; } = new Dictionary<string, NTAffliction>(); // Stores all of our registered afflictions (regardless of categeory)

        private static List<string> AfflictionsLOW = [];
        private static List<string> AfflictionsMEDIUM = [];
        private static List<string> AfflictionsHIGH = [];

        public static void DefineAllAfflictions()
        {
            NTAfflictionsToAdd AffsToAdd = new();
        }

        public static void RegisterAffliction(string id, NTAffliction affliction)
        {
            if (!Afflictions.ContainsKey(id))
            {
                Afflictions.Add(id, affliction);
                switch (affliction.Priority)
                {
                    case AfflictionPriority.LOW:
                        AfflictionsLOW.Add(id);
                        break;
                    case AfflictionPriority.MEDIUM:
                        AfflictionsMEDIUM.Add(id);
                        break;
                    case AfflictionPriority.HIGH:
                        AfflictionsHIGH.Add(id);
                        break;
                }
            } else
            {
                LuaCsLogger.LogError($"Affliction with id {id} already exists! Multiple addons might be trying to register the same affliction.\n" +
                    $"If you're trying to override an affliction update function, use OverrideAffliction instead.");
            }
            
        }

        // If an addon needs to change how an affliction works. Could create some incompatibility if multiple addons override the same shit but that's on them, not my problem
        public static void OverrideAffliction(string id, NTAffliction affliction)
        {
            if (Afflictions.ContainsKey(id))
            {
                Afflictions[id] = affliction;
            }
            else
            {
                LuaCsLogger.LogError($"Affliction with id {id} does not exist! Can't override it.");
            }
        }


        // I recommend running this function only once OnLoadCompleted as it could be perf inducing.
        public static Dictionary<string, NTAffliction> GetAfflictionsByPriority(AfflictionPriority priority)
        {
            Dictionary<string, NTAffliction> output = [];

            switch (priority)
            {
                case AfflictionPriority.LOW:
                    AfflictionsLOW.ForEach(affID =>
                    {
                        output.Add(affID, Afflictions[affID]);
                    });
                    break;
                case AfflictionPriority.MEDIUM:
                    AfflictionsMEDIUM.ForEach(affID =>
                    {
                        output.Add(affID, Afflictions[affID]);
                    });
                    break;
                case AfflictionPriority.HIGH:
                    AfflictionsHIGH.ForEach(affID =>
                    {
                        output.Add(affID, Afflictions[affID]);
                    });
                    break;
            }

            return output;
        }

        public static bool HasAffliction(string id)
        {
            return Afflictions.ContainsKey(id);
        }

        public static NTAffliction IDToNTAff(string id)
        {
            if (Afflictions.ContainsKey(id))
            {
                return Afflictions[id];
            }
            return null;
        }

    }

    public enum NTAfflictionType : int
    {
        ABSTRACT = 1,
        NONLIMB = 2,
        LIMB = 3,
        BLOOD = 4,
        SYMPTOM = 5,
        LIMBSYMPTOM = 6
    }

    /// <summary>
    /// The abstract template of our Afflictions. This class and any of it's descendants are never instantiated for a player class. Rather, we use the outline of this affliction class
    /// determine the results of the affliction. The strength is stored seperately in the NTHuman character class!
    /// </summary>
    public abstract class NTAffliction // Added to NTHuman updatingAfflictions
    {
        /// <summary>
        /// Should this affliction always be running? If on, regardless of current affliction strength, this will update.
        /// </summary>
        public bool Const = false;

        /// <summary>
        /// The minimum strength the affliction can have.
        /// </summary>
        public double MinStrength { get; set; }

        /// <summary>
        /// The maximum strength the affliction can have.
        /// </summary>
        public double MaxStrength { get; set; }


        /// <summary>
        /// The strength of the affliction on creation.
        /// </summary>
        public double DefaultStrength { get; set; }

        /// <summary>
        /// The priority of our affliction, higher intervals mean more updates.
        /// </summary>
        public AfflictionPriority Priority { get; set; }

        /// <summary>
        /// If false, doesnt update on stasis.
        /// </summary>
        public bool IgnoreStasis { get; set; } = true;

        /// <summary>
        /// The ID of the affliction.
        /// </summary>
        public string ID = "";

        public NTAfflictionType Type = NTAfflictionType.ABSTRACT;
        public int AffSortID = 0;
        /// <summary>
        /// The main update function of our affliction.
        /// </summary>
        public Action<HumanUpdate.NTHuman,string,LimbType> UpdateAction = 
            (HumanUpdate.NTHuman C, string ID, LimbType Limb) => 
            { 
                // Insert your Affliction Update in here.
            };
        public NTAffliction(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0,
                                        AfflictionPriority NewPriority = AfflictionPriority.HIGH)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength,NewMinStrength,NewMaxStrength);
            Priority = NewPriority;
        }
    }

    public class NTNonLimbAffliction : NTAffliction
    {

        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanNonLimbAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };
        
        public NTNonLimbAffliction(string NewID, double NewMinStrength =0, double NewMaxStrength= 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH) : 
                                        base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);
            Priority = NewPriority;
            AffSortID = 1;
            Type = NTAfflictionType.NONLIMB;
        }
    }

    public class NTLimbAffliction : NTAffliction
    {
        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanLimbAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTLimbAffliction(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH) :
                                        base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);
            Priority = NewPriority;
            IgnoreStasis = false;
            AffSortID = 2;
            Type = NTAfflictionType.LIMB;
        }

        public List<LimbType> AllowedLimbs { get; set; } = HF.LimbsToCheck; // I'll add this one later.
    }

    public class NTBloodAffliction : NTAffliction
    {

        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanBloodAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTBloodAffliction(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH, bool NewAddToHematology = true) :
                                        base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);   
            Priority = NewPriority;
            AffSortID = 3;
            Type = NTAfflictionType.BLOOD;
            if (NewAddToHematology)
            {
                NTC.AddHematologyAffliction(ID);
            }
        }
    }

    public class NTSymptom : NTNonLimbAffliction
    {

        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanSymptomData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData SymData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTSymptom(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH) 
                            : base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);
            Priority = NewPriority;
            Type = NTAfflictionType.SYMPTOM;
            AffSortID = 4;
    }
    }

    public class NTLimbSymptom : NTLimbAffliction
    {
        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanLimbSymptomData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbSymptomData SymData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTLimbSymptom(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH)
                            : base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);
            Priority = NewPriority;
            AffSortID = 5;
            Type = NTAfflictionType.LIMBSYMPTOM;
        }
    }

    public abstract class AfflictionsPackage 
    {
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> AfflictionsToAdd =
                        new Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>>();
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> LimbAfflictionsToAdd =
                                new Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>>();
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> BloodAfflictionsToAdd =
                                new Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>>();
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> SymptomsToAdd =
                                new Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>>();

        private void AddAfflictions() // Create your afflictions in here.
        {
            throw new NotImplementedException();
        }

        private void AddLimbAfflictions() // Create your afflictions in here.
        {
            throw new NotImplementedException();
        }

        private void AddBloodAfflictions() // Create your afflictions in here.
        {
            throw new NotImplementedException();
        }

        private void AffSymptoms()
        {
            throw new NotImplementedException();
        }

        private void AddLimbSymptoms()
        {
            throw new NotImplementedException();
        }
    }


    public class  NTAfflictionsToAdd : AfflictionsPackage
    {

        // Human Updates update functions have 
        // Param 1: NTHuman (The character we updating) [C]
        // Param 2: String (The affliction Identifier) [I]
        // Param 3: LimbType (The limb the aff is on) [L]
        // Param 4: AfflictionData

        Dictionary<string, NTNonLimbAffliction> AfflictionsToAdd =
                                new Dictionary<string, NTNonLimbAffliction>();
        Dictionary<string, NTLimbAffliction> LimbAfflictionsToAdd =
                                new Dictionary<string, NTLimbAffliction>();
        Dictionary<string, NTBloodAffliction> BloodAfflictionsToAdd =
                                new Dictionary<string, NTBloodAffliction>();
        Dictionary<string, NTSymptom> SymptomsToAdd =
                                new Dictionary<string, NTSymptom>();
        Dictionary<string, NTSymptom> LimbSymptomsToAdd =
                                new Dictionary<string, NTSymptom>();

        public NTAfflictionsToAdd() // Initalize the afflictions.
        {
            AddAfflictions();
            AddLimbAfflictions();
            AddBloodAfflictions();
            AddSymptoms();
            AddLimbSymptoms();
        }

        private void AddAfflictions() // Create your afflictions in here.
        {
            // EXAMPLE AFFLICTION

            AfflictionsToAdd["respiratoryarrest"] = new("respiratoryarrest", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["respiratoryarrest"].Const = true; // This affliction should always run
            AfflictionsToAdd["respiratoryarrest"].UpdateAction = // The update function of the affliction, like how it is in Lua
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    AffData.Strength -= (0.05 + HF.BoolToNum(C.GetSymptomAffData("unconsciousness").Strength < .1, .45f)) * NT.DeltaTime;
                    if
                        (!NTC.HasSymptomFalse(C, "triggersym_respiratoryarrest")
                        && (NTC.HasSymptomFalse(C, "triggersym_respiratoryarrest")
                        || C.GetBoolStatStrength("stasis")
                        || C.GetNonLimbAffData("lungremoved").Strength > 0
                        || C.GetNonLimbAffData("brainremoved").Strength > 0
                        || C.GetNonLimbAffData("opiateoverdose").Strength > 50
                        || C.GetNonLimbAffData("lungdamage").Strength > 99 && HF.Chance(.8f)
                        || C.GetNonLimbAffData("traumaticshock").Strength > 30 && HF.Chance(.2f)
                        || (
                            (C.GetNonLimbAffData("neurotrauma").Strength > 100 || C.GetNonLimbAffData("neurotrauma").Strength > 70
                            && HF.Chance(.05f))
                        )
                      )
                      )
                    {
                        AffData.Strength += 10;
                    }
                };

            // Rib Fractures
            AfflictionsToAdd["fracturedribs"] = new("fracturedribs", 0, 100, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["fracturedribs"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    AffData.Strength += 2 * NT.DeltaTime;
                };

            // Neck Fracture
            AfflictionsToAdd["fracturedneck"] = new("fracturedneck", 0, 100, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["fracturedneck"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    AffData.Strength += 2 * NT.DeltaTime;
                };

            // Skull Fracture
            AfflictionsToAdd["fracturedskull"] = new("fracturedskull", 0, 100, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["fracturedskull"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    AffData.Strength += 2 * NT.DeltaTime;
                };


            // =============== Drugs =============== //
            // Opiate Overdose
            AfflictionsToAdd["opiateoverdose"] = new("opiateoverdose");

            // =============== Organs =============== //
            // Lung Damage
            AfflictionsToAdd["lungdamage"] = new("lungdamage");

            // Lung Removed
            AfflictionsToAdd["lungremoved"] = new("lungremoved");

            // Lung Swap
            AfflictionsToAdd["lungswap"] = new("lungswap");

            // Brain Removed
            AfflictionsToAdd["brainremoved"] = new("brainremoved");

            // Brain Swap
            AfflictionsToAdd["brainswap"] = new("brainswap");

            // Cardiac Tamponade
            AfflictionsToAdd["tamponade"] = new("tamponade");

            // Increased Heartrate
            AfflictionsToAdd["increasedheartrate"] = new("increasedheartrate");

            // Fibrillation
            AfflictionsToAdd["fibrillation"] = new("fibrillation");

            // Cardiac Arrest
            AfflictionsToAdd["cardiacarrest"] = new("cardiacarrest");

            // Heart Attack
            AfflictionsToAdd["heartattack"] = new("heartattack");

            // Heart Damage
            AfflictionsToAdd["heartdamage"] = new("heartdamage");

            // Heart Removed
            AfflictionsToAdd["heartremoved"] = new("heartremoved");

            // Heart Swap
            AfflictionsToAdd["heartswap"] = new("heartswap");

            // Kidney Damage
            AfflictionsToAdd["kidneydamage"] = new("kidneydamage");

            // Kidney Removed
            AfflictionsToAdd["kidneyremoved"] = new("kidneyremoved");

            // Kidney Swap
            AfflictionsToAdd["kidneyswap"] = new("kidneyswap");

            // Liver Damage
            AfflictionsToAdd["liverdamage"] = new("liverdamage");

            // Liver Removed
            AfflictionsToAdd["liverremoved"] = new("liverremoved");

            // Liver Swap
            AfflictionsToAdd["liverswap"] = new("liverswap");

            // Hyperventilation
            AfflictionsToAdd["hyperventilation"] = new("hyperventilation");

            // Hypoventilation
            AfflictionsToAdd["hypoventilation"] = new("hypoventilation");

            // Pneumothorax
            AfflictionsToAdd["pneumothorax"] = new("pneumothorax");

            // =============== Limbs =============== //
            // Traumatic Right Arm Amputation
            AfflictionsToAdd["tra_amputation"] = new("tra_amputation");

            // Traumatic Left Arm Amputation
            AfflictionsToAdd["tla_amputation"] = new("tla_amputation");

            // Traumatic Right Leg Amputation
            AfflictionsToAdd["trl_amputation"] = new("trl_amputation");

            // Traumatic Left Leg Amputation
            AfflictionsToAdd["tll_amputation"] = new("tll_amputation");

            // Traumatic Head Amputation
            AfflictionsToAdd["th_amputation"] = new("th_amputation");

            // Surgical Right Arm Amputation
            AfflictionsToAdd["sra_amputation"] = new("sra_amputation");

            // Surgical Left Arm Amputation
            AfflictionsToAdd["sla_amputation"] = new("sla_amputation");

            // Surgical Right Leg Amputation
            AfflictionsToAdd["srl_amputation"] = new("srl_amputation");

            // Surgical Left Leg Amputation
            AfflictionsToAdd["sll_amputation"] = new("sll_amputation");

            // Surgical Head Amputation
            AfflictionsToAdd["sh_amputation"] = new("sh_amputation");

            // =============== Utility =============== //
            // Luabotomy
            AfflictionsToAdd["luabotomy"] = new("luabotomy");

            // Luabotomy Purger
            AfflictionsToAdd["luabotomypurger"] = new("luabotomypurger");

            // StopCreatureAbuse
            AfflictionsToAdd["stopcreatureabuse"] = new("stopcreatureabuse");

            // ModConflict
            AfflictionsToAdd["modconflict"] = new("modconflict");

            // TShockTimeout
            AfflictionsToAdd["tshocktimeout"] = new("tshocktimeout");

            // GiveIn
            AfflictionsToAdd["givein"] = new("givein");

            // CPR Buff
            AfflictionsToAdd["cpr_buff"] = new("cpr_buff");

            // CPR Buff AutoPulse
            AfflictionsToAdd["cpr_buff_auto"] = new("cpr_buff_auto");

            // CPR Fracture Buff
            AfflictionsToAdd["cpr_fracturebuff"] = new("cpr_fracturebuff");

            // Force Prone
            AfflictionsToAdd["forceprone"] = new("forceprone");

            // On Wheelchair
            AfflictionsToAdd["onwheelchair"] = new("onwheelchair");

            // Stasis Bag Overlay
            AfflictionsToAdd["stasisbagoverlay"] = new("stasisbagoverlay");

            // BodyBag Overlay
            AfflictionsToAdd["bodybagoverlay"] = new("bodybagoverlay");

            // Stretchers
            AfflictionsToAdd["stretchers"] = new("stretchers");

            // Stasis
            AfflictionsToAdd["stasis"] = new("stasis");

            // Locked Hands
            AfflictionsToAdd["lockedhands"] = new("lockedhands");

            // Slowdown
            AfflictionsToAdd["slowdown"] = new("slowdown");

            // TraumaticAmputating + Item
            AfflictionsToAdd["gate_ta_ll"] = new("gate_ta_ll");

            // TraumaticAmputating + Item
            AfflictionsToAdd["gate_ta_rl"] = new("gate_ta_rl");

            // TraumaticAmputating + Item
            AfflictionsToAdd["gate_ta_la"] = new("gate_ta_la");

            // TraumaticAmputating + Item
            AfflictionsToAdd["gate_ta_ra"] = new("gate_ta_ra");

            // TraumaticAmputating + Item
            AfflictionsToAdd["gate_ta_h"] = new("gate_ta_h");

            // TraumaticAmputating
            AfflictionsToAdd["gate_ta_ll_2"] = new("gate_ta_ll_2");

            // TraumaticAmputating
            AfflictionsToAdd["gate_ta_rl_2"] = new("gate_ta_rl_2");

            // TraumaticAmputating
            AfflictionsToAdd["gate_ta_la_2"] = new("gate_ta_la_2");

            // TraumaticAmputating
            AfflictionsToAdd["gate_ta_ra_2"] = new("gate_ta_ra_2");

            // TraumaticAmputating
            AfflictionsToAdd["gate_ta_h_2"] = new("gate_ta_h_2");

            // Opioids in Blood
            AfflictionsToAdd["afopioid"] = new("afopioid");

            // Anaesthetic in Blood
            AfflictionsToAdd["afanaesthetic"] = new("afanaesthetic");

            // Safe Surgery (via Surgery Table)
            AfflictionsToAdd["safesurgery"] = new("safesurgery");

            // Artificial Ventilation (via Surgery Table)
            AfflictionsToAdd["artificialventilation"] = new("artificialventilation");

            // Alcohol Addiction
            AfflictionsToAdd["alcoholaddiction"] = new("alcoholaddiction");

            // Alcohol Withdrawal
            AfflictionsToAdd["alcoholwithdrawal"] = new("alcoholwithdrawal");

            // On Fire!
            AfflictionsToAdd["onfire"] = new("onfire");

            // Screaming
            AfflictionsToAdd["screaming"] = new("screaming");

            // Severe Pain
            AfflictionsToAdd["severepain"] = new("severepain");

            // Pain
            AfflictionsToAdd["pain"] = new("pain");

            // Shock Pain
            AfflictionsToAdd["shockpain"] = new("shockpain");

            // Analgesia
            AfflictionsToAdd["analgesia"] = new("analgesia");

            // Anesthesia
            AfflictionsToAdd["anesthesia"] = new("anesthesia");

            // =============== Head =============== //
            // Stroke
            AfflictionsToAdd["stroke"] = new("stroke");

            // Neurotrauma
            AfflictionsToAdd["neurotrauma"] = new("neurotrauma");

            // Seizure
            AfflictionsToAdd["seizure"] = new("seizure");

            // Coma
            AfflictionsToAdd["coma"] = new("coma");

            // Spinal Cord Injury
            AfflictionsToAdd["spinalcordinjury"] = new("spinalcordinjury");

            // Carotid Arterial Cut
            AfflictionsToAdd["carotidarterialcut"] = new("carotidarterialcut");

            // =============== Item Derived =============== //
            // Adrenaline in Blood
            AfflictionsToAdd["afadrenaline"] = new("afadrenaline");

            // Needle in Chest
            AfflictionsToAdd["needlec"] = new("needlec");

            // Saline in Blood
            AfflictionsToAdd["afsaline"] = new("afsaline");

            // Ringers Solution in Blood
            AfflictionsToAdd["afringerssolution"] = new("afringerssolution");

            // Mannitol in Blood
            AfflictionsToAdd["afmannitol"] = new("afmannitol");

            // Immunosuppressants in Blood
            AfflictionsToAdd["afimmunosuppressant"] = new("afimmunosuppressant");

            // Pressure-increasing drugs in Blood
            AfflictionsToAdd["afpressuredrug"] = new("afpressuredrug");

            // Thiamine in Blood
            AfflictionsToAdd["afthiamine"] = new("afthiamine");

            // Streptokinase in Blood
            AfflictionsToAdd["afstreptokinase"] = new("afstreptokinase");

            // Antibiotics in Blood
            AfflictionsToAdd["afantibiotics"] = new("afantibiotics");

            // =============== Surgical =============== //
            // Surgical Incision
            AfflictionsToAdd["surgeryincision"] = new("surgeryincision");

            // Clamped Bleeding
            AfflictionsToAdd["clampedbleeding"] = new("clampedbleeding");

            // Drilled Bones
            AfflictionsToAdd["drilledbones"] = new("drilledbones");

            // Retracted Skin
            AfflictionsToAdd["retractedskin"] = new("retractedskin");

            // Sutured Incision
            AfflictionsToAdd["suturedi"] = new("suturedi");

            // Sutured Wound
            AfflictionsToAdd["suturedw"] = new("suturedw");

            // Sawed Bones
            AfflictionsToAdd["sawedbones"] = new("sawedbones");

            // Traumatic Shock
            AfflictionsToAdd["traumaticshock"] = new("traumaticshock");

            // Ballooned Aorta
            AfflictionsToAdd["balloonedaorta"] = new("balloonedaorta");

            // =============== Torso =============== //
            // Aortic Rupture
            AfflictionsToAdd["aorticrupture"] = new("aorticrupture");

            // =============== Bones =============== //
            // Bone Damage
            AfflictionsToAdd["bonedamage"] = new("bonedamage");
            AfflictionsToAdd["bonedamage"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    if (C.GetBoolStatStrength("stasis")) return;
                };


            // Now add these afflictions.
            foreach (KeyValuePair<string,NTNonLimbAffliction> Pair in AfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddLimbAfflictions()
        {
            // Bleeding
            LimbAfflictionsToAdd["bleeding"] = new("bleeding");
            LimbAfflictionsToAdd["bleeding"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    AffData.Strength[Limb] -= (C.GetDoubleStatStrength("clottingrate") * .1 * NT.DeltaTime);
                    NTC.SetSymptomFalse(C,"vomitingblood",10);
                };

            // Stimulated Bone Growth
            LimbAfflictionsToAdd["stimulatedbonegrowth"] = new("stimulatedbonegrowth");

            // Fractures
            // Should only update when actually present; get applied OnDamaged or via ItemApplication.
            // Arm + Leg Fractures
            LimbAfflictionsToAdd["fracturedextremity"] = new("fracturedextremity");
            LimbAfflictionsToAdd["fracturedextremity"].Const = false;
            LimbAfflictionsToAdd["fracturedextremity"].UpdateAction = 
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                };

            // Arm + Leg Dislocation
            LimbAfflictionsToAdd["dislocation"] = new("dislocation");

            // Tourniquet around Extremity
            LimbAfflictionsToAdd["tourniqueted"] = new("tourniqueted");

            // Plaster Cast
            LimbAfflictionsToAdd["plastercast"] = new("plastercast");

            // Arterial Cut on Extremity
            LimbAfflictionsToAdd["arterialcut"] = new("arterialcut");

            // Gangrene
            LimbAfflictionsToAdd["gangrene"] = new("gangrene");

            // Bandage applied to Limb
            LimbAfflictionsToAdd["bandaged"] = new("bandaged");

            // Dirty Bandage around Limb
            LimbAfflictionsToAdd["bandageddirty"] = new("bandageddirty");

            // Gel Coolant Pack applied to Limb
            LimbAfflictionsToAdd["iced"] = new("iced");

            // Antibiotic Ointment applied to Limb
            LimbAfflictionsToAdd["ointmented"] = new("ointmented");

            // Infected Wound
            LimbAfflictionsToAdd["infectedwound"] = new("infectedwound");

            // Foreign Body
            LimbAfflictionsToAdd["foreignbody"] = new("foreignbody");

            // First-degree Burn
            LimbAfflictionsToAdd["firstdegreeburn"] = new("firstdegreeburn");

            // Second-degree Burn
            LimbAfflictionsToAdd["seconddegreeburn"] = new("seconddegreeburn");

            // Third-degree Burn
            LimbAfflictionsToAdd["thirddegreeburn"] = new("thirddegreeburn");

            LimbAfflictionsToAdd["lacerations"] = new("lacerations");
            LimbAfflictionsToAdd["lacerations"].UpdateAction  =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (AffData.Strength[Limb] < 50)
                    {
                        AffData.Strength[Limb] -= (
                                C.GetBloodAffData("immunity").PrevStrength / 3000
                                + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * .1
                                + Math.Clamp(C.GetLimbAffStrength("skinointmented", Limb), 0, 1) * .12
                                )
                                * C.GetDoubleStatStrength("healingrate")
                                * NT.DeltaTime;
                    }
                };


            // I did it ... I added all the template afflictions. Good luck Lukako, this is all you. :peace: :crying emoji:

            foreach (KeyValuePair<string, NTLimbAffliction> Pair in LimbAfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddBloodAfflictions()
        {
            // Blood afflictions are literally the same to write as NonLimbAfflictions, they're just here for organization purposes.

            // Blood Pressure
            BloodAfflictionsToAdd["bloodpressure"] = new("bloodpressure");

            // Hypoxemia
            BloodAfflictionsToAdd["hypoxemia"] = new("hypoxemia");

            // Alkalosis
            BloodAfflictionsToAdd["alkalosis"] = new("alkalosis");

            // Acidosis
            BloodAfflictionsToAdd["acidosis"] = new("acidosis");

            // Hemotransfusion Shock
            BloodAfflictionsToAdd["hemotransfusionshock"] = new("hemotransfusionshock");

            // Sepsis
            BloodAfflictionsToAdd["sepsis"] = new("sepsis");

            // Immunity
            BloodAfflictionsToAdd["immunity"] = new("immunity");

            foreach (KeyValuePair<string, NTBloodAffliction> Pair in BloodAfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddSymptoms()
        {
            // Cough
            SymptomsToAdd["cough"] = new("cough", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["cough"].Const = true; // This affliction should always run
            SymptomsToAdd["cough"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    AffData.Strength = HF.BoolToNum(
                        !NTC.HasSymptomFalse(C, ID)
                        && C.GetSymptomAffData("unconsciousness").Strength <= 0
                        && C.GetNonLimbAffData("lungremoved").Strength <= 0
                        && (
                            NTC.HasSymptom(C, ID)
                            || C.GetNonLimbAffData("lungdamage").Strength > 50
                            || C.GetNonLimbAffData("heartdamage").Strength > 50
                            || C.GetNonLimbAffData("tamponade").Strength > 20
                        ),
                        100
                    );
                };

            // Pale Skin
            SymptomsToAdd["paleskin"] = new("paleskin", 0, 100, 0, AfflictionPriority.HIGH);

            // Lightheadedness
            SymptomsToAdd["lightheadedness"] = new("lightheadedness", 0, 100, 0, AfflictionPriority.HIGH);

            // Blurred Vision
            SymptomsToAdd["blurredvision"] = new("blurredvision", 0, 100, 0, AfflictionPriority.HIGH);

            // Confusion
            SymptomsToAdd["confusion"] = new("confusion", 0, 100, 0, AfflictionPriority.HIGH);

            // Headache
            SymptomsToAdd["headache"] = new("headache", 0, 100, 0, AfflictionPriority.HIGH);

            // Leg Swelling
            SymptomsToAdd["legswelling"] = new("legswelling", 0, 100, 0, AfflictionPriority.HIGH);

            // Weakness
            SymptomsToAdd["weakness"] = new("weakness", 0, 100, 0, AfflictionPriority.HIGH);

            // Wheezing
            SymptomsToAdd["wheezing"] = new("wheezing", 0, 100, 0, AfflictionPriority.HIGH);

            // Vomiting
            SymptomsToAdd["vomiting"] = new("vomiting", 0, 100, 0, AfflictionPriority.HIGH);

            // Vomiting Blood
            SymptomsToAdd["vomitingblood"] = new("vomitingblood", 0, 100, 0, AfflictionPriority.HIGH);

            // Fever
            SymptomsToAdd["fever"] = new("fever", 0, 100, 0, AfflictionPriority.HIGH);

            // Abdominal Discomfort
            SymptomsToAdd["abdominaldiscomfort"] = new("abdominaldiscomfort", 0, 100, 0, AfflictionPriority.HIGH);

            // Bloating
            SymptomsToAdd["bloating"] = new("bloating", 0, 100, 0, AfflictionPriority.HIGH);

            // Jaundice
            SymptomsToAdd["jaundice"] = new("jaundice", 0, 100, 0, AfflictionPriority.HIGH);

            // Sweating
            SymptomsToAdd["sweating"] = new("sweating", 0, 100, 0, AfflictionPriority.HIGH);

            // Palpitations
            SymptomsToAdd["palpitations"] = new("palpitations", 0, 100, 0, AfflictionPriority.HIGH);

            // Unconsciousness
            SymptomsToAdd["unconsciousness"] = new("unconsciousness", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["unconsciousness"].Const = true;
            SymptomsToAdd["unconsciousness"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    bool IsUnc = !NTC.HasSymptomFalse(C, ID)
                        && (NTC.HasSymptom(C, ID)
                        || C.GetBoolStatStrength("stasis")
                        || C.GetAffStrength("brainremoved") > 0
                        || C.GetAffStrength("neurotrauma") > 100
                        || C.GetAffStrength("coma") > 15
                        || C.Human.Vitality <= 0
                        || C.GetAffStrength("hypoxemia") > 80
                        || C.GetAffStrength("t_arterialcut") > 0
                        || C.GetAffStrength("seizure") > 0.1
                        || C.GetAffStrength("opiateoverdose") > 60
                    )
                    && !C.Human.HasAbilityFlag(AbilityFlags.AlwaysStayConscious);
                    AffData.Strength = HF.BoolToNum(IsUnc, 100);

                };

            // Craving
            SymptomsToAdd["craving"] = new("craving", 0, 100, 0, AfflictionPriority.HIGH);

            // Nausea
            SymptomsToAdd["nausea"] = new("nausea", 0, 100, 0, AfflictionPriority.HIGH);

            // Chest Pain
            SymptomsToAdd["chestpain"] = new("chestpain", 0, 100, 0, AfflictionPriority.HIGH);

            // Abdominal Pain
            SymptomsToAdd["abdominalpain"] = new("abdominalpain", 0, 100, 0, AfflictionPriority.HIGH);

            // Intense Pain
            SymptomsToAdd["intensepain"] = new("intensepain", 0, 100, 0, AfflictionPriority.HIGH);

            // Shortness of Breath
            SymptomsToAdd["shortnessofbreath"] = new("shortnessofbreath", 0, 100, 0, AfflictionPriority.HIGH);

            SymptomsToAdd["lockleftarm"] = new("lockleftarm", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["lockrightarm"] = new("lockleftarm", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["lockleftleg"] = new("lockleftarm", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["lockrightleg"] = new("lockleftarm", 0, 100, 0, AfflictionPriority.HIGH);

            SymptomsToAdd["triggersym_respiratoryarrest"] = new("triggersym_respiratoryarrest", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["triggersym_respiratoryarrest"].Const = true;

            foreach (KeyValuePair<string, NTSymptom> Pair in SymptomsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddLimbSymptoms()
        {

            // Inflammation
            LimbSymptomsToAdd["inflammation"] = new("inflammation", 0, 100, 0, AfflictionPriority.HIGH);

            // Spasms
            LimbSymptomsToAdd["spasm"] = new("spasm", 0, 100, 0, AfflictionPriority.HIGH);

            foreach (KeyValuePair<string, NTSymptom> Pair in SymptomsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }
    }
}