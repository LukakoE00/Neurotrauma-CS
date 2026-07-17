using MoonSharp.Interpreter;
using System.Reflection.Metadata;
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
                Afflictions[id] = affliction;
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
            return NTAfflictions.Afflictions.ContainsKey(id);
        }

        public static void PrintIDs()
        {
            foreach (KeyValuePair<string,NTAffliction> kvp in Afflictions)
            {
                HF.Print(kvp.Key);
            }
        }

        public static NTAffliction GetAffliction(string id)
        {
            return Afflictions[id];
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
        /// Does this affliction actually have an XML prefab? If true, gets/sets the affliction prefab, else uses the custom strength only.
        /// </summary>
        public bool Real = true;

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
        public Action<HumanUpdate.NTHuman,string,LimbType,HumanUpdate.NTHumanAffData> UpdateAction = 
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanAffData AffData) => 
            { 
                // Insert your Affliction Update in here.
            };

        public virtual void Update(HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanAffData Data)
        {
            UpdateAction.Invoke(C, ID, Limb, Data);
        }

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

        public override void Update(HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanAffData Data)
        {
            UpdateAction.Invoke(C, ID, Limb, (HumanUpdate.NTHumanNonLimbAffData)Data);
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

        public override void Update(HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanAffData Data)
        {
            UpdateAction.Invoke(C, ID, Limb, (HumanUpdate.NTHumanLimbAffData)Data);
        }

        public List<LimbType> AllowedLimbs { get; set; } = HF.LimbsToCheck;
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

        public override void Update(HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanAffData Data)
        {
            UpdateAction.Invoke(C, ID, Limb, (HumanUpdate.NTHumanBloodAffData)Data);
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

        public override void Update(HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanAffData Data)
        {
            UpdateAction.Invoke(C, ID, Limb, (HumanUpdate.NTHumanSymptomData)Data);
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

        public override void Update(HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanAffData Data)
        {
            UpdateAction.Invoke(C, ID, Limb, (HumanUpdate.NTHumanLimbSymptomData)Data);
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
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> LimbSymptomsToAdd =
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
        // Param 4: AfflictionData (the affliction data of the aff) [AffData]

        Dictionary<string, NTNonLimbAffliction> AfflictionsToAdd =
                                new Dictionary<string, NTNonLimbAffliction>();
        Dictionary<string, NTLimbAffliction> LimbAfflictionsToAdd =
                                new Dictionary<string, NTLimbAffliction>();
        Dictionary<string, NTBloodAffliction> BloodAfflictionsToAdd =
                                new Dictionary<string, NTBloodAffliction>();
        Dictionary<string, NTSymptom> SymptomsToAdd =
                                new Dictionary<string, NTSymptom>();
        Dictionary<string, NTLimbSymptom> LimbSymptomsToAdd =
                                new Dictionary<string, NTLimbSymptom>();

        public NTAfflictionsToAdd() // Initalize the afflictions.
        {
            AddAfflictions();
            AddLimbAfflictions();
            AddBloodAfflictions();
            AddSymptoms();
            AddLimbSymptoms();
        }

        private void AddAfflictions()
        {
            // Oxygen Low
            // Not constant; gets applied by other sources
            // Type: Non-Limb Specific, Vanilla Override
            // Caused By: Lack of Oxygen, Respiratory Arrest
            // Effects: Hypoxemia
            AfflictionsToAdd["oxygenlow"] = new("oxygenlow", 0, 200, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["oxygenlow"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    if (C.GetAffStrength("respiratoryarrest") > 0)
                    {
                        AffData.Strength += 30f;
                    }
                };


            // Drunk
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Vanilla Override
            // Caused By: ROOOTT BEEERRRRRR.
            // Effects: idk.
            AfflictionsToAdd["drunk"] = new("drunk", 0, 200, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["drunk"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                };

            // Radiation Sickness
            // Not constant; gets applied by other sources.
            // Type: Damage, Vanilla Override
            // Caused By: Health Scanner, Radiotoxin, Radiation, Certain Damage.
            // Effects: Burns (XML), Screen Grain (XML), Specific Organ Damage, Bone Damage.
            AfflictionsToAdd["radiationsickness"] = new("radiationsickness", 0, 200, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["radiationsickness"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Regeneration
                    AffData.Strength -= NT.DeltaTime * 0.02;

                    // Effects:   
                    if (AffData.Strength > 25)
                    {
                        // Additional Lung Damage
                        HF.AddAffliction(C.Human, "lungdamage", (float)(Math.Max(AffData.Strength - 25, 0) / 800 * NT.DeltaTime), null);

                        // Bone Damage
                        HF.AddAffliction(C.Human, "bonedamage", (float)(Math.Max(AffData.Strength - 25, 0) / 600 * NT.DeltaTime), null);
                    }

                    // Heart Damage (in NewOrganDamage)
                    // Liver Damage (in NewOrganDamage)
                    // Kidney Damage (in NewOrganDamage)

                    // Seizures
                    double RadSicknessAbove50 = AffData.Strength >= 50 ? AffData.Strength : 0;
                    if (HF.Chance((float)(RadSicknessAbove50 / 200 * 0.1)))
                    {
                        HF.AddAffliction(C.Human, "seizure", 10, null);
                    }

                    // Nausea
                    if (AffData.Strength > 80)
                    {
                        NTC.SetSymptomTrue(C, "nausea", 2);
                    }
                };

            // Respiratory Arrest
            // Not constant; gets applied by other sources, removes itself however.
            // Type: Non-Limb Specific, Interrim
            // Caused By: Lung Damage, TraumaShock, Neurotrauma, Hypoxemia, Opiate Overdose, Stasis, Morbusine Poisoning.
            // Effects: Oxygen Low, Acidosis.
            AfflictionsToAdd["respiratoryarrest"] = new("respiratoryarrest", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["respiratoryarrest"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                    {
                        // Removal Conditions
                        if ((!C.GetBoolStatStrength("stasis")) // Not in Stasis
                            && C.GetAffData("lungremoved").Strength <= 0 // No Lungs Removed
                            && C.GetAffData("brainremoved").Strength <= 0 // No Brain Removed
                            && C.GetAffData("opiateoverdose").Strength <= 60 // Below Opiate Overdose Threshold
                            && C.GetAffData("lungdamage").Strength <= 99 // Below Lung Damage Threshold
                            && C.GetAffData("traumaticshock").Strength <= 30 // Below Traumatic Shock Threshold
                            && C.GetAffData("neurotrauma").Strength <= 100 // Below Neurotrauma Threshold
                            && C.GetAffData("hypoxemia").Strength <= 70 // Below Hypoxemia Threshold
                            )
                        {
                            // Passive Regeneration
                            AffData.Strength -= (5f + HF.BoolToNum(C.GetAffStrength("unconsciousness") < 0.1f, 45f)) * NT.DeltaTime;
                        }

                        // Effects:
                        // Acidosis
                        // Shares increase with Cardiac Arrest
                        double AcidosisIncrease = HF.BoolToNum(C.GetAffData("cardiacarrest").Strength <= 0 
                            && C.GetAffData("respiratoryarrest").Strength > 0 
                            && C.GetAffData("artificialventilation").Strength <= 0.1) 
                        * 0.18 * NT.DeltaTime;

                        HF.AddAffliction(C.Human, "acidosis", (float)AcidosisIncrease, null);
                    };

            // Rib Fractures
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Internal Wounds (DMG), Open Wounds (DMG), Bone Death.
            // Effects: Pneumothorax if not Bandaged (XML), Chest Pain.
            AfflictionsToAdd["fracturedribs"] = new("fracturedribs", 0, 100, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["fracturedribs"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Increase
                    AffData.Strength += 4 * NT.DeltaTime;

                    // Effects:
                    // Chest Pain
                    if (AffData.Strength > 0 && C.GetSymptomAffData("unconsciousness").Strength <= 0 && (!C.GetBoolStatStrength("sedated")))
                    {
                        NTC.SetSymptomTrue(C, "chestpain", 2);
                    }
                };

            // Neck Fracture
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Internal Wounds (DMG), Open Wounds (DMG), Bone Death.
            // Effects: Spinal Cord Injury if not Bandaged (XML), Internal Damage if not Bandaged (XML).
            AfflictionsToAdd["fracturedneck"] = new("fracturedneck", 0, 100, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["fracturedneck"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Increase
                    AffData.Strength += 4 * NT.DeltaTime;
                };

            // Skull Fracture
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Internal Wounds (DMG), Open Wounds (DMG), Bone Death.
            // Effects: Headache, prevents Neurotrauma Regeneration.
            AfflictionsToAdd["fracturedskull"] = new("fracturedskull", 0, 100, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["fracturedskull"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Increase
                    AffData.Strength += 4 * NT.DeltaTime;

                    // Effects:
                    // Headache
                    if (AffData.Strength > 0 && C.GetAffData("unconsciousness").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "headache", 2);
                    }

                    // Neurotrauma Regeneration (in Neurotrauma itself)
                };

            // =============== Drugs =============== //

            // Opiate Overdose
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Drug, Vanilla Override
            // Caused By: Application of Opiates.
            // Effects: Respiratory Arrest, Unconsciousness, Seizures, Death.
            AfflictionsToAdd["opiateoverdose"] = new("opiateoverdose", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["opiateoverdose"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Effects:
                    if (AffData.Strength > 60)
                    {
                        // Respiratory Arrest
                        HF.AddAffliction(C.Human, "respiratoryarrest", 200, null);

                        // Unconsciousness
                        NTC.SetSymptomTrue(C, "unconsciousness", 2);
                        
                        // Seizures
                        if (HF.Chance((float)AffData.Strength / 500f))
                        {
                            HF.AddAffliction(C.Human, "seizure", 10, null);
                        }  
                    }
                };

            // =============== Organs =============== //

            // Lung Damage
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Organ Damage
            // Caused By: ABX, LiOxy, Ambubag, HemoTransShock, RadSickness, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG).
            // Effects: Cough, Shortness of Breath, Respiratory Arrest
            AfflictionsToAdd["lungdamage"] = new("lungdamage", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["lungdamage"].Const = true;
            AfflictionsToAdd["lungdamage"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress while in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double LungDamage = HF.OrganDamageCalc(C, AffData.Strength + NTC.GetMultiplier(C, "lungdamagegain") * C.GetDoubleStatStrength("neworgandamage"));

                    // Passive Regeneration / Increase
                    AffData.Strength = LungDamage;

                    // Effects:
                    // Shortness of Breath
                    if (AffData.Strength > 45)
                    {
                        if (C.GetAffData("respiratoryarrest").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                        }

                        // Cough
                        if (AffData.Strength > 50 && C.GetAffData("unconsciousness").Strength <= 0 && C.GetAffData("lungremoved").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "cough", 2);
                        }

                        // Respiratory Arrest
                        if (AffData.Strength > 99 && HF.Chance(0.8f))
                        {
                            HF.AddAffliction(C.Human, "respiratoryarrest", 200, null);
                        }
                    }
                };

            // Lung Removed
            // Not constant; gets applied by other sources.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 2x.
            // Effects: Respiratory Arrest, Unconsciousness, eventual Death.
            AfflictionsToAdd["lungremoved"] = new("lungremoved", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["lungremoved"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // State check; strength is 1 if Retracted Skin is present, else 100.
                    AffData.Strength = 1 + HF.BoolToNum(HF.HasAfflictionLimb(C.Human, "retractedskin", LimbType.Head, 99), 99);

                    // Effects:
                    // Respiratory Arrest
                    HF.AddAffliction(C.Human, "respiratoryarrest", 200f, null);

                    // Unconsciousness
                    NTC.SetSymptomTrue(C, "unconsciousness", 2);
                };

            // Lung Swap
            // Not constant; gets applied by other sources, removed on surgery end.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 1x.
            // Effects: None.
            AfflictionsToAdd["lungswap"] = new("lungswap", 0, 100, 0, AfflictionPriority.LOW);

            // Brain Removed
            // Not constant; gets applied by other sources.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 2x.
            // Effects: Cardiac Arrest, Respiratory Arrest, Unconsciousness, eventual Death.
            AfflictionsToAdd["brainremoved"] = new("brainremoved", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["brainremoved"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // State check; strength is 1 if Retracted Skin is present, else 100.
                    AffData.Strength = 1 + HF.BoolToNum(HF.HasAfflictionLimb(C.Human, "retractedskin", LimbType.Head, 99), 99);
                        
                    // Effects:
                    // Cardiac Arrest
                    HF.AddAffliction(C.Human, "cardiacarrest", 200, null);

                    // Respiratory Arrest
                    HF.AddAffliction(C.Human, "respiratoryarrest", 200, null);

                    // Unconsciousness
                    NTC.SetSymptomTrue(C, "unconsciousness", 2);

                    // Neurotrauma
                    float NeurotraumaGain = 2.4f;
                    if (C.GetAffData("afmannitol").Strength <= 0.5)
                    {
                        NeurotraumaGain += 1.6f;
                    }

                    HF.AddAffliction(C.Human, "neurotrauma", NeurotraumaGain, null);
                };

            // Brain Swap
            // Not constant; gets applied by other sources, removed on surgery end.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 1x.
            // Effects: None.
            AfflictionsToAdd["brainswap"] = new("brainswap", 0, 100, 0, AfflictionPriority.LOW);

            // Cardiac Tamponade
            // Type: Non-Limb Specific
            // Not constant; gets applied by other sources.
            // Caused By: Open Wounds (DMG) to Torso.
            // Effects: Decreases Blood Pressure, Weakness, Cough, Shortness of Breath.
            // Additional interaction with: Needle.
            AfflictionsToAdd["tamponade"] = new("tamponade", 0, 100, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["tamponade"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Cannot have Cardiac Tamponade without a heart.
                    if (C.GetAffData("heartremoved").Strength > 0)
                    {
                        AffData.Strength = 0;
                    }

                    // Passive Regeneration / Increase
                    // Increases if there is no needle until 100%; else decreases until 5%.
                    AffData.Strength = Math.Clamp(AffData.Strength + NT.DeltaTime * (0.5f - HF.BoolToNum(AffData.Strength > 5) * Math.Clamp(C.GetAffData("needlec").Strength, 0, 1)),
                        0,
                        100
                    );

                    // Effects:
                    // Shortness of Breath
                    if (AffData.Strength > 10)
                    {
                        if (C.GetAffData("respiratoryarrest").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                        }

                        // Cough
                        if (AffData.Strength > 20 && C.GetAffData("unconsciousness").Strength <= 0 && C.GetAffData("lungremoved").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "cough", 2);
                        }

                        // Weakness
                        if (AffData.Strength > 30)
                        {
                            NTC.SetSymptomTrue(C, "weakness", 2);
                        }
                    }
                };

            // Increased Heartrate (previously Tachycardia)
            // Type: Non-Limb Specific
            // Constant; too complicated otherwise.
            // Harmless Causes: Sepsis, Blood Loss, Acidosis, Pneumothorax, Adrenaline, Alcohol Withdrawal.
            // Harmful Causes: Aortic Rupture, Acidosis, Hypotension, Hypoxemia, Traumatic Shock.
            AfflictionsToAdd["increasedheartrate"] = new("increasedheartrate", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["increasedheartrate"].Const = true;
            AfflictionsToAdd["increasedheartrate"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {

                    // Fibrillation cannot occur without a (beating) heart
                    if (C.GetAffData("cardiacarrest").Strength > 0 || C.GetAffData("heartremoved").Strength > 0)
                    {
                        C.GetNonLimbAffData("fibrillation").Strength = 0;
                        AffData.Strength = 0;
                        return;
                    }


                    // Harmless symptom (does not lead to Fibrillation)
                    bool hasSymHarmless =
                        C.GetAffData("sepsis").Strength > 20
                        || C.GetDoubleStatStrength("bloodamount") < 60
                        || C.GetAffData("acidosis").Strength > 20
                        || C.GetAffData("pneumothorax").Strength > 30
                        || C.GetAffData("afadrenaline").Strength > 1
                        || C.GetAffData("alcoholwithdrawal").Strength > 75;

                    AffData.Strength = Math.Max(AffData.Strength, HF.BoolToNum(hasSymHarmless, 2));


                    // Fibrillation speed calculation
                    double fibrillationSpeed = -0.1
                        + Math.Clamp(C.GetNonLimbAffData("aorticrupture").Strength, 0, 2)
                        + Math.Clamp(C.GetAffData("acidosis").Strength / 200, 0, 0.5)
                        + Math.Clamp(
                            0.9 - ((C.GetAffData("bloodpressure").Strength + Math.Clamp(C.GetAffData("afpressuredrug").Strength * 5, 0, 20)) / 90),
                            0, 1
                        ) * 2
                        + Math.Clamp(C.GetAffData("hypoxemia").Strength / 100, 0, 1) * 1.5
                        + Math.Clamp((C.GetAffData("traumaticshock").Strength - 5) / 40, 0, 3)
                        - Math.Clamp(C.GetAffData("afadrenaline").Strength, 0, 0.9);

                    // Adrenaline halves Fibrillation speed
                    if (fibrillationSpeed > 0 && C.GetAffData("afadrenaline").Strength > 0)
                    {
                        fibrillationSpeed /= 2;
                    }


                    // Apply Fibrillation multipliers only when progressing
                    if (fibrillationSpeed > 0)
                    {
                        fibrillationSpeed *= NTC.GetMultiplier(C, "fibrillation") * NTConfig.Get("NT_fibrillationSpeed", 1);
                    }


                    // Progress IncreasedHeartrate or Fibrillation
                    if (C.GetNonLimbAffData("fibrillation").Strength <= 0)
                    {
                        AffData.Strength += fibrillationSpeed * 5 * NT.DeltaTime;

                        if (AffData.Strength >= 100)
                        {
                            C.GetNonLimbAffData("fibrillation").Strength = 5;
                            AffData.Strength = 0;
                        }
                    }
                    else
                    {
                        C.GetNonLimbAffData("fibrillation").Strength += fibrillationSpeed * NT.DeltaTime;
                        AffData.Strength = 0;
                    }

                };

            // Fibrillation
            // Type: Non-Limb Specific, Mechanic
            // Not constant; gets applied by other sources.
            // Effects: Cardiac Arrest.
            AfflictionsToAdd["fibrillation"] = new("fibrillation", 0, 100, 0, AfflictionPriority.MEDIUM);
            AfflictionsToAdd["fibrillation"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Fibrillation cannot occur without a (beating) heart
                    if (C.GetAffData("cardiacarrest").Strength >= 1 || C.GetAffData("heartremoved").Strength >= 1)
                    {
                        AffData.Strength = 0;
                        return;
                    }

                    // Cardiac Arrest
                    if (AffData.Strength > 20 && HF.Chance((float)Math.Pow(AffData.Strength / 100f, 4f)))
                    {
                        HF.AddAffliction(C.Human, "cardiacarrest", 200, null);
                    }
                };

            // Cardiac Arrest
            // Type: Non-Limb Specific, Lethal
            // Not constant; gets applied by other sources.
            // Caused By: Heart Removed, Brain Removed, Heart Damage, Traumatic Shock, Coma, Hypoxemia, Fibrillation, Stasis.
            // Effects: Coma, Acidosis, Hypotension, Hypoxemia.
            AfflictionsToAdd["cardiacarrest"] = new("cardiacarrest", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["cardiacarrest"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Removal Conditions
                    if ((!C.GetBoolStatStrength("stasis")) // Not in Stasis
                        && C.GetAffData("heartremoved").Strength <= 0 // Heart not removed
                        && C.GetAffData("brainremoved").Strength <= 0 // Brain not removed
                        && C.GetAffData("heartdamage").Strength <= 99 // Below Heart Damage threshold
                        && C.GetAffData("traumaticshock").Strength <= 40 // Below Traumatic Shock threshold
                        && C.GetAffData("coma").Strength <= 40 // Below Coma threshold
                        && C.GetAffData("hypoxemia").Strength <= 80 // Below Hypoxemia threshold
                        && C.GetNonLimbAffData("fibrillation").Strength <= 20) // Below Fibrillation threshold
                    {
                        AffData.Strength -= 50 * NT.DeltaTime;
                    }

                    // Effects:
                    // Acidosis
                    // Shares increase with Respiratory Arrest
                    double AcidosisIncrease = 0.18 * NT.DeltaTime;
                    HF.AddAffliction(C.Human, "acidosis", (float)AcidosisIncrease, null);

                    // Coma
                    if (AffData.Strength > 1 && HF.Chance(0.05f))
                    {
                        HF.AddAffliction(C.Human, "coma", 14, null);
                    }

                    // Hypotension (in BloodPressure constant itself)
                    // Hypoxemia (in Hypoxemia constant itself)
                };

            // Heart Attack
            // Type: Non-Limb Specific, Lethal
            // Not constant; gets applied by other sources.
            // Caused By: Hypertension, Antibiotic Glue (XML).
            // Effects: Sweating, Shortness of Breath, Heart Damage.
            AfflictionsToAdd["heartattack"] = new("heartattack", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["heartattack"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Cannot have a heart attack without a heart.
                    if (C.GetAffData("heartremoved").Strength > 0) 
                    { 
                        AffData.Strength = 0; 
                        return; 
                    }

                    // Passive Regeneration
                    AffData.Strength -= NT.DeltaTime;

                    // Effects:
                    // Sweating
                    NTC.SetSymptomTrue(C, "sweating", 2);
                        
                    // Shortness of Breath
                    if (C.GetAffData("respiratoryarrest").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                    }
                    
                    // Heart Damage
                    HF.AddAffliction(C.Human, "heartdamage", (float)(Math.Clamp(C.GetAffData("heartattack").Strength, 0, 0.5) * NT.DeltaTime), null);
                };

            // Heart Damage
            // Type: Non-Limb Specific, Organ Damage
            // Constant for Regeneration
            // Caused By: Heart Attack, ABX, LiOxy, HemoTransShock, Mannitol, RadSickness, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
            // Effects: Cough, Leg Swelling, Shortness of Breath, Cardiac Arrest.
            AfflictionsToAdd["heartdamage"] = new("heartdamage", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["heartdamage"].Const = true;
            AfflictionsToAdd["heartdamage"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress while in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double HeartDamage = HF.OrganDamageCalc(C, AffData.Strength + NTC.GetMultiplier(C, "heartdamagegain") * C.GetDoubleStatStrength("neworgandamage"));

                    // Passive Regeneration / Increase
                    AffData.Strength = HeartDamage;

                    // Effects:
                    // Cough
                    if (AffData.Strength > 50)
                    {
                        if (C.GetAffData("unconsciousness").Strength <= 0 && C.GetAffData("lungremoved").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "cough", 2);
                        }

                        // Leg Swelling & Shortness of Breath
                        if (AffData.Strength > 80)
                        {
                            if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                            {
                                NTC.SetSymptomTrue(C, "legswelling", 2);
                            }

                            if (C.GetAffData("respiratoryarrest").Strength <= 0)
                            {
                                NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                            }

                            // Cardiac Arrest
                            if (AffData.Strength > 99 && HF.Chance(0.3f))
                            {
                                HF.AddAffliction(C.Human, "cardiacarrest", 200, null);
                            }
                        }
                    }
                };

            // Heart Removed
            // Not constant; gets applied by other sources.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 2x.
            // Effects: Cardiac Arrest.
            AfflictionsToAdd["heartremoved"] = new("heartremoved", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["heartremoved"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // State check; strength is 1 if Retracted Skin is present, else 100.
                    AffData.Strength = 1 + HF.BoolToNum(HF.HasAfflictionLimb(C.Human, "retractedskin", LimbType.Torso, 99), 99);

                    // Effects:
                    // Cardiac Arrest
                    HF.AddAffliction(C.Human, "cardiacarrest", 200, null);
                };

            // Heart Swap
            // Not constant; gets applied by other sources, removed on surgery end.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 1x.
            // Effects: None.
            AfflictionsToAdd["heartswap"] = new("heartswap", 0, 100, 0, AfflictionPriority.LOW);

            // Kidney Damage
            // Type: Non-Limb Specific, Organ Damage
            // Constant for Regeneration
            // Caused By: ABX, LiOxy, HemoTransShock, Mannitol, RadSickness, Hypertension, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
            // Effects: Acidosis, Leg Swelling, Hypertension, Bone Damage, Vomiting, Neurotrauma, Nausea.
            AfflictionsToAdd["kidneydamage"] = new("kidneydamage", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["kidneydamage"].Const = true;
            AfflictionsToAdd["kidneydamage"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress while in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double KidneyDamage = HF.KidneyDamageCalc(C, AffData.Strength
                        + NTC.GetMultiplier(C, "kidneydamagegain") * (C.GetDoubleStatStrength("neworgandamage")
                        + Math.Clamp((C.GetBloodAffData("bloodpressure").Strength - 120) / 160, 0, 0.5) * NT.DeltaTime * 0.5));

                    // Passive Regeneration / Increase
                    AffData.Strength = KidneyDamage;

                    // Effects:
                    // Acidosis
                    double AcidosisIncrease = Math.Max(0, AffData.Strength - 80) / 20.0 * 0.1 * NT.DeltaTime;
                    HF.AddAffliction(C.Human, "acidosis", (float)AcidosisIncrease, null);

                    // Neurotrauma
                    double NeurotraumaIncrease = AffData.Strength / 1000.0 * NT.DeltaTime
                        * NTC.GetMultiplier(C, "neurotraumagain")
                        * NTConfig.Get("NT_neurotraumaGain", 1)
                        * (1 - Math.Clamp(C.GetAffData("afmannitol").Strength, 0, 0.5));

                    HF.AddAffliction(C.Human, "neurotrauma", (float)NeurotraumaIncrease, null);

                    // Hypertension (in BloodPressure constant)

                    // Nausea & Leg Swelling
                    if (AffData.Strength > 60)
                    {
                        NTC.SetSymptomTrue(C, "nausea", 2);

                        if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                        {
                            NTC.SetSymptomTrue(C, "legswelling", 2);
                        }

                        // Vomiting
                        if (!NTC.HasSymptom(C, "vomiting") && HF.Chance((float)(AffData.Strength - 60) / 40f * 0.07f))
                        {
                            NTC.SetSymptomTrue(C, "vomiting", Rand.Range(3, 11));
                        }

                        // Bone Damage
                        if (AffData.Strength > 70)
                        {
                            HF.AddAffliction(C.Human, "bonedamage", (float)((AffData.Strength - 70) / 30 * 0.15 * NT.DeltaTime), null);
                        }
                    }
                };

            // Kidney Removed
            // Not constant; gets applied by other sources.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 2x.
            // Effects: None; Kidney Damage 100% via Removal Surgery causes effects.
            AfflictionsToAdd["kidneyremoved"] = new("kidneyremoved", 0, 100, 0);

            // Kidney Swap
            // Not constant; gets applied by other sources, removed on surgery end.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 1x.
            // Effects: None.
            AfflictionsToAdd["kidneyswap"] = new("kidneyswap", 0, 100, 0);

            // Liver Damage
            // Type: Non-Limb Specific, Organ Damage
            // Constant for Regeneration
            // Caused By: ABX, LiOxy, HemoTransShock, RadSickness, Sepsis, Hypoxemia, Drunk, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
            // Effects: Leg Swelling, Internal Bleeding, Vomiting Blood, Hypertension, Neurotrauma, AbdomDiscomfort, Jaundice, Bloating.
            AfflictionsToAdd["liverdamage"] = new("liverdamage", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["liverdamage"].Const = true;
            AfflictionsToAdd["liverdamage"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double LiverDamage = HF.OrganDamageCalc(C, AffData.Strength + NTC.GetMultiplier(C, "liverdamagegain") * C.GetDoubleStatStrength("neworgandamage"));

                    // Passive Regeneration / Increase
                    AffData.Strength = LiverDamage;

                    // Effects:
                    // Neurotrauma
                    double NeurotraumaIncrease = (AffData.Strength / 800.0 * NT.DeltaTime)
                        * NTC.GetMultiplier(C, "neurotraumagain")
                        * NTConfig.Get("NT_neurotraumaGain", 1)
                        * (1 - Math.Clamp(C.GetAffData("afmannitol").Strength, 0, 0.5));

                    HF.AddAffliction(C.Human, "neurotrauma", (float)NeurotraumaIncrease, null);

                    // Hypertension (in BloodPressure constant itself)

                    // Leg Swelling
                    if (AffData.Strength > 40)
                    {
                        if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                        {
                            NTC.SetSymptomTrue(C, "legswelling", 2);
                        }

                        if (AffData.Strength > 50)
                        {
                            // Bloating
                            NTC.SetSymptomTrue(C, "bloating", 2);

                            if (AffData.Strength > 65)
                            {
                                // Abdominal Discomfort
                                if (C.GetAffData("unconsciousness").Strength <= 0)
                                {
                                    NTC.SetSymptomTrue(C, "abdominaldiscomfort", 2);
                                }

                                if (AffData.Strength > 80)
                                {
                                    // Jaundice
                                    NTC.SetSymptomTrue(C, "jaundice", 2);

                                    if (AffData.Strength >= 99 && HF.Chance(0.05f))
                                    {
                                        // Internal Bleeding & Vomiting Blood
                                        NTC.SetSymptomTrue(C, "vomitingblood", Random.Shared.Next(3, 10));
                                        HF.AddAffliction(C.Human, "internalbleeding", 2, null);
                                    }
                                }
                            }
                        }
                    }
                };

            // Liver Removed
            // Not constant; gets applied by other sources.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 2x.
            // Effects: None; Liver Damage 100% via Removal Surgery causes effects.
            AfflictionsToAdd["liverremoved"] = new("liverremoved", 0, 100, 0);

            // Liver Swap
            // Not constant; gets applied by other sources, removed on surgery end.
            // Type: Surgical Action
            // Caused By: Organ Removal Scalpel action 1x.
            // Effects: None.
            AfflictionsToAdd["liverswap"] = new("liverswap", 0, 100, 0);
       
            // Pneumothorax
            // Type: Non-Limb Specific
            // Not constant; gets applied by other sources.
            // Caused By: Rib Fracture, Trauma to the Torso, Needle application.
            // Effects: Shortness of Breath, Hyperventilation, Increased Heartrate
            // Additional interaction with: Needle.
            AfflictionsToAdd["pneumothorax"] = new("pneumothorax", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["pneumothorax"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Regeneration / Increase
                    // Increases if there is no needle until 100%; else decreases until 5%.
                    AffData.Strength = Math.Clamp(AffData.Strength + NT.DeltaTime * (0.5 - HF.BoolToNum(AffData.Strength > 15) * Math.Clamp(C.GetAffData("needlec").Strength, 0, 1)),
                        0, 
                        100
                    );

                    // Effects:
                    // Increased Heartrate (in IncreasedHeartrate constant)

                    // Hyperventilation
                    if (AffData.Strength > 15)
                    {
                        HF.AddAffliction(C.Human, "hyperventilation", 100, null);

                        // Shortness of Breath
                        if (AffData.Strength > 40 && C.GetAffData("respiratoryarrest").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                        }
                    }
                };

            // =============== Limbs =============== //
            // Traumatic Right Arm Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator
            // Effects: None.
            // Applied via Damage Sustained. Does nothing.
            AfflictionsToAdd["tra_amputation"] = new("tra_amputation", 0, 100, 0);

            // Traumatic Left Arm Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator
            // Effects: None.
            // Applied via Damage Sustained. Does nothing.
            AfflictionsToAdd["tla_amputation"] = new("tla_amputation", 0, 100, 0);

            // Traumatic Right Leg Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator
            // Effects: None.
            // Applied via Damage Sustained. Does nothing.
            AfflictionsToAdd["trl_amputation"] = new("trl_amputation", 0, 100, 0);

            // Traumatic Left Leg Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator
            // Effects: None.
            // Applied via Damage Sustained. Does nothing.
            AfflictionsToAdd["tll_amputation"] = new("tll_amputation", 0, 100, 0);

            // Traumatic Head Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator
            // Effects: None.
            // Applied via Damage Sustained. Does nothing. Act of removing the head kills instantly.
            AfflictionsToAdd["th_amputation"] = new("th_amputation", 0, 100, 0);

            // Surgical Right Arm Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator, Surgery
            // Effects: None.
            // Result of Surgical Amputation. Does nothing.
            AfflictionsToAdd["sra_amputation"] = new("sra_amputation", 0, 100, 0);

            // Surgical Left Arm Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator, Surgery
            // Effects: None.
            // Result of Surgical Amputation. Does nothing.
            AfflictionsToAdd["sla_amputation"] = new("sla_amputation", 0, 100, 0);

            // Surgical Right Leg Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator, Surgery
            // Effects: None.
            // Result of Surgical Amputation. Does nothing.
            AfflictionsToAdd["srl_amputation"] = new("srl_amputation", 0, 100, 0);

            // Surgical Left Leg Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator, Surgery
            // Effects: None.
            // Result of Surgical Amputation. Does nothing.
            AfflictionsToAdd["sll_amputation"] = new("sll_amputation", 0, 100, 0);

            // Surgical Head Amputation
            // Not constant; gets applied by other sources.
            // Type: Indicator, Surgery
            // Effects: None.
            // Result of Surgical Amputation. Does nothing. Act of removing the head kills instantly.
            AfflictionsToAdd["sh_amputation"] = new("sh_amputation", 0, 100, 0);

            // =============== Utility =============== //
            // Luabotomy
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: None.
            // Used to determine whether or not someone should be updated.
            AfflictionsToAdd["luabotomy"] = new("luabotomy", 0, 15, 0, AfflictionPriority.LOW);
            AfflictionsToAdd["luabotomy"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Keep it low if everything works properly. Else, increase until it shows on the UI.
                    AffData.Strength = 0.1f;
                };

            // Luabotomy Purger
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Removes Luabotomy, then itself.
            AfflictionsToAdd["luabotomypurger"] = new("luabotomypurger", 0, 2, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["luabotomypurger"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Removes Luabotomy and itself; originally done in XML
                    HF.SetAffliction(C.Human, "luabotomy", 0, null, 0);
                    AffData.Strength = 0;
                };

            // StopCreatureAbuse
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: None.
            // Used to decrease additional fall damage for certain creatures.
            AfflictionsToAdd["stopcreatureabuse"] = new("stopcreatureabuse", 0, 2, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["stopcreatureabuse"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Decreases itself by 1 per 2 seconds, removing itself after 4 seconds; originally done in XML
                    AffData.Strength -= 1;
                };

            // TShockTimeout
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Removes Traumatic Shock.
            // Applied during level change to prevent Traumatic Shock from taking place.
            AfflictionsToAdd["tshocktimeout"] = new("tshocktimeout", 0, 300, 0, AfflictionPriority.LOW);
            AfflictionsToAdd["tshocktimeout"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Ticks every 6 seconds for 6 strength. Max strength is 300 so it lasts for 5 minutes unless removed early.
                    AffData.Strength -= 6;
                };

            // GiveIn
            // Type: Functionality
            // Effects: Enables give-in button.
            // Allows you to die while stuck in certain afflictions like Spinal Cord Injury, which are not lethal yet prevent character use.
            AfflictionsToAdd["givein"] = new("givein", 0, 1, 0);

            // CPR Buff
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Decreases Cardiac Arrest and Fibrillation while increasing Blood Pressure.
            // Originally done in XML.
            AfflictionsToAdd["cpr_buff"] = new("cpr_buff", 0, 2, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["cpr_buff"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    AffData.Strength -= 1;

                    // Effects:
                    // Reduce Cardiac Arrest
                    HF.SetAffliction(C.Human, "cardiacarrest", (float)Math.Max(0, C.GetAffData("cardiacarrest").Strength - 4), null, 0);

                    // Reduce Fibrillation
                    HF.SetAffliction(C.Human, "fibrillation", (float)Math.Max(0, C.GetNonLimbAffData("fibrillation").Strength - 2), null, 0);

                    // Increase Blood Pressure
                    HF.AddAffliction(C.Human, "bloodpressure", 8, null);

                    // If Cardiac Arrest is above 0 and below or equal to 0.5, clear it and apply Fibrillation
                    double CardiacArrest = C.GetAffData("cardiacarrest").Strength;
                    if (CardiacArrest > 0 && CardiacArrest <= 0.5)
                    {
                        HF.SetAffliction(C.Human, "cardiacarrest", 0, null, 0);
                        HF.AddAffliction(C.Human, "fibrillation", 20, null);
                    }
                };

            // CPR Buff AutoPulse
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Decreases Cardiac Arrest and Fibrillation while increasing Blood Pressure.
            // Originally done in XML.
            AfflictionsToAdd["cpr_buff_auto"] = new("cpr_buff_auto", 0, 2, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["cpr_buff_auto"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    AffData.Strength -= 1;

                    // Effects:
                    // Reduce Cardiac Arrest
                    HF.SetAffliction(C.Human, "cardiacarrest", (float)Math.Max(0, C.GetAffData("cardiacarrest").Strength - 3), null, 0);

                    // Reduce Fibrillation
                    HF.SetAffliction(C.Human, "fibrillation", (float)Math.Max(0, C.GetNonLimbAffData("fibrillation").Strength - 2), null, 0);

                    // Increase Blood Pressure
                    HF.AddAffliction(C.Human, "bloodpressure", 10, null);

                    // Reduce Oxygen Low
                    HF.SetAffliction(C.Human, "oxygenlow", (float)Math.Max(0, C.GetAffData("oxygenlow").Strength - 6), null, 0);

                    // If Cardiac Arrest is above 0 and below or equal to 0.5, clear it and apply Fibrillation
                    double CardiacArrest = C.GetAffData("cardiacarrest").Strength;
                    if (CardiacArrest > 0 && CardiacArrest <= 0.5)
                    {
                        HF.SetAffliction(C.Human, "cardiacarrest", 0, null, 0);
                        HF.AddAffliction(C.Human, "fibrillation", 20, null);
                    }
                };

            // CPR Fracture Buff
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Prevents Fractures from fall damage during CPR.
            // Originally done in XML.
            AfflictionsToAdd["cpr_fracturebuff"] = new("cpr_fracturebuff", 0, 2, 0);
            AfflictionsToAdd["cpr_fracturebuff"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Decreases itself by 1 per 2 seconds, removing itself after 4 seconds; originally done in XML
                    AffData.Strength -= 1;
                };

            // On Wheelchair
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Changes the animations of a character to one in a wheelchair.
            // Applied via Stats.
            AfflictionsToAdd["onwheelchair"] = new("onwheelchair", 0, 2, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["onwheelchair"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Removal Conditions
                    AffData.Strength = HF.BoolToNum(
                        !(NTC.HasSymptomFalse(C, "onwheelchair"))
                        && C.GetSymptomAffData("unconsciousness").Strength <= 0
                        && (NTC.HasSymptom(C, "onwheelchair") || HF.GetOuterWearIdentifier(C.Human) == "wheelchair"
                        ),
                        2
                    );
                };

            // Stasis Bag Overlay
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Overlays the stasis bag sprite.
            // Applied via Stasis Bag item.
            AfflictionsToAdd["stasisbagoverlay"] = new("stasisbagoverlay", 0, 2, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["stasisbagoverlay"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Removal Conditions
                    // Originally removed itself every second in XML only to be added again every tick. 
                    AffData.Strength = HF.BoolToNum(HF.GetOuterWearIdentifier(C.Human) == "stasisbag", 2);
                };

            // BodyBag Overlay
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Overlays the body bag sprite.
            // Applied via Body Bag item.
            AfflictionsToAdd["bodybagoverlay"] = new("bodybagoverlay", 0, 2, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["bodybagoverlay"].UpdateAction =
               (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
               {
                   // Removal Conditions
                   // Originally removed itself every second in XML only to be added again every tick. 
                   AffData.Strength = HF.BoolToNum(HF.GetOuterWearIdentifier(C.Human) == "bodybag", 2);
               };

            // Stasis
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Enables Stasis stattype.
            // Applied via Stasis Bag item.
            AfflictionsToAdd["stasis"] = new("stasis", 0, 3, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["stasis"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    AffData.Strength -= 2;

                    // Reduce Husk Infection if below 100
                    if (HF.HasAffliction(C.Human, "huskinfection") && C.GetAffData("huskinfection").Strength < 100)
                    {
                        HF.AddAffliction(C.Human, "huskinfection", -0.15f, null);

                        // Additional reduction if no Husk Infection Resistance
                        if (!HF.HasAffliction(C.Human, "huskinfectionresistance") || C.GetAffData("huskinfectionresistance").Strength <= 0)
                        {
                            HF.AddAffliction(C.Human, "huskinfection", -0.15f, null);
                        }
                    }
                };

            // Locked Hands
            // Constant; too complicated otherwise.
            // Type: Functionality
            // Effects: Prevent usage of the left/right arms when triggered.
            AfflictionsToAdd["lockedhands"] = new("lockedhands", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["lockedhands"].Const = true;
            AfflictionsToAdd["lockedhands"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Arm lock items
                    Item LeftLockItem = HF.GetItemInLeftHand(C.Human);
                    if (LeftLockItem?.Prefab.Identifier.Value != "armlock")
                    {
                        LeftLockItem = null;
                    }

                    Item RightLockItem = HF.GetItemInRightHand(C.Human);
                    if (RightLockItem?.Prefab.Identifier.Value != "armlock")
                    {
                        RightLockItem = null;
                    }

                    // Handcuffs Check
                    Item Handcuffs = C.Human.Inventory.FindItemByIdentifier("handcuffs", false);
                    bool Handcuffed = Handcuffs != null && C.Human.Inventory.FindIndex(Handcuffs) <= 6;

                    if (Handcuffed)
                    {
                        // Drop non-handcuff items
                        Item leftHandItem = HF.GetItemInLeftHand(C.Human);
                        Item rightHandItem = HF.GetItemInRightHand(C.Human);

                        if (leftHandItem != null && leftHandItem != Handcuffs && LeftLockItem == null)
                        {
                            leftHandItem.Drop(C.Human);
                        }
                            
                        if (rightHandItem != null && rightHandItem != Handcuffs && RightLockItem == null)
                        { 
                            rightHandItem.Drop(C.Human);
                        }
                    }

                    bool LeftArmLocked = LeftLockItem != null && !Handcuffed;
                    bool RightArmLocked = RightLockItem != null && !Handcuffed;

                    if (LeftArmLocked && !C.GetBoolStatStrength("lockleftarm")) 
                    { 
                        HF.RemoveItem(LeftLockItem);
                    }

                    if (RightArmLocked && !C.GetBoolStatStrength("lockrightarm"))
                    {
                        HF.RemoveItem(RightLockItem);
                    }

                    if (!LeftArmLocked && C.GetBoolStatStrength("lockleftarm"))
                    {
                        HF.ForceArmLock(C.Human, "LeftArm");
                    }

                    if (!RightArmLocked && C.GetBoolStatStrength("lockrightarm"))
                    {
                        HF.ForceArmLock(C.Human, "RightArm");
                    }

                    AffData.Strength = HF.BoolToNum((C.GetBoolStatStrength("lockleftarm") && C.GetBoolStatStrength("lockrightarm")) || Handcuffed, 100);
                };

            // TraumaticAmputating Left Leg + Item
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_ll"] = new("gate_ta_ll");

            // TraumaticAmputating Right Leg + Item
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_rl"] = new("gate_ta_rl");

            // TraumaticAmputating Left Arm + Item
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_la"] = new("gate_ta_la");

            // TraumaticAmputating Right Arm + Item
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_ra"] = new("gate_ta_ra");

            // TraumaticAmputating Head + Item
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Kills you ontop of spawning a severed head.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_h"] = new("gate_ta_h");

            // TraumaticAmputating Left Leg
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_ll_2"] = new("gate_ta_ll_2");

            // TraumaticAmputating Right Leg
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_rl_2"] = new("gate_ta_rl_2");

            // TraumaticAmputating Left Arm
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_la_2"] = new("gate_ta_la_2");

            // TraumaticAmputating Right Arm
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_ra_2"] = new("gate_ta_ra_2");

            // TraumaticAmputating Head
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Effects: Kills you.
            // Uses XML to cause TraumaAmputations.
            AfflictionsToAdd["gate_ta_h_2"] = new("gate_ta_h_2");

            // Opioids in Blood
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Opioids
            // Effects: Hypoventilation.
            AfflictionsToAdd["afopioid"] = new("afopioid", 0, 200, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afopioid"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Decreases itself by 0.3/s or 0.6/2s; originally done in XML
                    AffData.Strength -= 0.6f;

                    // Effects:
                    // Hypoventilation
                    if (AffData.Strength > 1)
                    {
                        HF.AddAffliction(C.Human, "hypoventilation", 100, null);
                    }

                };

            // Anaesthetic in Blood
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Propofol
            // Effects: Hypoventilation.
            AfflictionsToAdd["afanaesthetic"] = new("afanaesthetic", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afanaesthetic"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Decreases itself by 0.3/s or 0.6/2s; originally done in XML
                    AffData.Strength -= 0.6f;

                    // Effects:
                    // Hypoventilation
                    if (AffData.Strength > 1)
                    {
                        HF.AddAffliction(C.Human, "hypoventilation", 100, null);
                    }

                };

            // Safe Surgery (via Surgery Table)
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Surgery Table / Hospital Bed
            // Effects: Reduces / Prevents Traumatic Shock.
            AfflictionsToAdd["safesurgery"] = new("safesurgery", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["safesurgery"].UpdateAction =
               (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
               {
                   // Passive Decrease
                   // Originally had a maxstrength of 3, and reduced by 1 per second in XML.
                   // Adjusted, that became 60 per 2 seconds (so technically, it takes 4 seconds to fully vanish).
                   AffData.Strength -= 60;
               };

            // Artificial Ventilation (via Surgery Table)
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Surgery Table / Hospital Bed
            // Effects: Reduces Oxygen Low
            AfflictionsToAdd["artificialventilation"] = new("artificialventilation", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["artificialventilation"].UpdateAction =
               (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
               {
                   // Passive Decrease
                   // Originally had a maxstrength of 100, and reduced by 20 per second in XML.
                   // Adjusted, that became 40 per 2 seconds (so technically, it takes 6 seconds to fully vanish).
                   AffData.Strength -= 40;

                   // Reduce Oxygen Low if lungs are present
                   if (C.GetAffData("lungremoved").Strength <= 0)
                   {
                       HF.AddAffliction(C.Human, "oxygenlow", -100, null);
                   }
               };


            AfflictionsToAdd["chemwithdrawal"] = new("chemwithdrawal");
            AfflictionsToAdd["chemwithdrawal"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                };


            AfflictionsToAdd["opiatewithdrawal"] = new("opiatewithdrawal");
            AfflictionsToAdd["opiatewithdrawal"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                };

            // Alcohol Addiction
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Consuming alcohol.
            // Effects: Alcohol Withdrawal if not eternally drinking (XML).
            AfflictionsToAdd["alcoholaddiction"] = new("alcoholaddiction", 0, 100, 0);

            // Alcohol Withdrawal
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Not consuming Alcohol with an addiction (applies via XML).
            // Effects: Craving, Sweating, Nausea, Fever, Vomiting, Headache, Confusion, Increased Heartrate, Seizure, Hypertension. 
            AfflictionsToAdd["alcoholwithdrawal"] = new("alcoholwithdrawal");
            AfflictionsToAdd["alcoholwithdrawal"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Effects:
                    // Hypertension (in BloodPressure constant)
                    // Increased Heartrate (in IncreasedHeartrate constant)

                    // Craving
                    if (AffData.Strength > 20)
                    {
                        if (C.GetAffData("unconsciousness").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "craving", 2);
                        }

                        // Sweating
                        if (AffData.Strength > 30)
                        {
                            NTC.SetSymptomTrue(C, "sweating", 2);

                            // Nausea
                            if (AffData.Strength > 40)
                            {
                                NTC.SetSymptomTrue(C, "nausea", 2);

                                if (AffData.Strength > 50)
                                {
                                    // Headache
                                    if (C.GetAffData("unconsciousness").Strength <= 0)
                                    {
                                        NTC.SetSymptomTrue(C, "headache", 2);
                                    }

                                    // Seizure
                                    if (HF.Chance((float)AffData.Strength / 1000f))
                                    {
                                        HF.AddAffliction(C.Human, "seizure", 10, null);
                                    }

                                    // Vomiting
                                    if (AffData.Strength > 60)
                                    {
                                        NTC.SetSymptomTrue(C, "vomiting", 2);

                                        // Confusion
                                        if (AffData.Strength > 80 && C.GetAffData("unconsciousness").Strength <= 0)
                                        {
                                            NTC.SetSymptomTrue(C, "confusion", 2);

                                            // Fever
                                            if (AffData.Strength > 90)
                                            {
                                                NTC.SetSymptomTrue(C, "fever", 2);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

            // On Fire!
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Being on fire too long.
            // Effects: Visibly on fire (XML), burns (XML).
            AfflictionsToAdd["onfire"] = new("onfire", 0, 1, 0);

            // Screaming
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Fractures, Amputations, Dislocations.
            // Effects: Character screams (XML).
            AfflictionsToAdd["screaming"] = new("screaming", 0, 1, 0);

            // Severe Pain
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Fractures, Amputations, Dislocations.
            // Effects: Character screams (XML), gets momentarily stunned (XML).
            AfflictionsToAdd["severepain"] = new("severepain", 0, 2, 0);

            // Pain
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Damage.
            // Effects: Damage Sounds (XML), Slowdown (XML). Removes self via XML.
            AfflictionsToAdd["pain"] = new("pain", 0, 2, 0);

            // Shock Pain
            // Not constant; gets applied by other sources.
            // Type: Functionality
            // Caused By: Traumatic Shock.
            // Effects: Damage Sounds (XML), Slowdown (XML). Removes self via XML.
            AfflictionsToAdd["shockpain"] = new("shockpain", 0, 2, 0);

            // Analgesia
            // Not constant; gets applied by other sources.
            // Type: Functionality, Surgery, Buff
            // Caused By: Painkillers.
            // Effects: Damage resistance (XML), allows surgery, reduces Pain (XML), applies screen changes (XML). Removes self via XML.
            AfflictionsToAdd["analgesia"] = new("analgesia", 0, 100, 0);

            // Anesthesia
            // Not constant; gets applied by other sources.
            // Type: Functionality, Surgery
            // Caused By: Propofol.
            // Effects: Applies Analgesia (XML) and has side effects. Increases and removes self via XML.
            AfflictionsToAdd["anesthesia"] = new("anesthesia", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["anesthesia"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Apply random side-effects.
                    if (!HF.Chance(0.06f)) return;

                    double casecount = 7;
                    double case_ = Random.Shared.NextDouble();

                    if (case_ < 1 / casecount)
                    {
                        NTC.SetSymptomTrue(C, "vomitingblood", (int)(5 + Random.Shared.NextDouble() * 10));
                    }
                    else if (case_ < 2 / casecount)
                    {
                        if (C.GetAffData("unconsciousness").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "blurredvision", (int)(5 + Random.Shared.NextDouble() * 10));
                        }
                    }
                    else if (case_ < 3 / casecount)
                    {
                        if (C.GetAffData("unconsciousness").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "confusion", (int)(5 + Random.Shared.NextDouble() * 10));
                        }
                    }
                    else if (case_ < 4 / casecount)
                    {
                        NTC.SetSymptomTrue(C, "fever", (int)(5 + Random.Shared.NextDouble() * 10));
                    }
                    else if (case_ < 5 / casecount)
                    {
                        NTC.SetSymptomTrue(C, "triggersym_seizure", (int)(1 + Random.Shared.NextDouble() * 2));
                    }
                    else if (case_ < 6 / casecount)
                    {
                        HF.Fibrillate(C.Human, (float)(5 + Random.Shared.NextDouble() * 30));
                    }
                    else
                    { 
                        HF.AddAffliction(C.Human, "psychosis", 10, null); 
                    }
                };

            // =============== Head =============== //

            // Stroke
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Hypertension.
            // Effects: Headache, Coma, Seizure, Neurotrauma.
            AfflictionsToAdd["stroke"] = new("stroke", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["stroke"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    // Passive Regeneration
                    AffData.Strength -= (1.0 / 20) * C.GetDoubleStatStrength("clottingrate") * NT.DeltaTime;

                    // Effects:
                    // Neurotrauma
                    double NeurotraumaGain = Math.Clamp(AffData.Strength, 0, 20) * 0.1 * NT.DeltaTime
                        * NTC.GetMultiplier(C, "neurotraumagain")
                        * NTConfig.Get("NT_neurotraumaGain", 1)
                        * (1 - Math.Clamp(C.GetAffData("afmannitol").Strength, 0, 0.5));

                    HF.AddAffliction(C.Human, "neurotrauma", (float)NeurotraumaGain, null);

                    // Headache
                    if (AffData.Strength > 1 && C.GetAffData("unconsciousness").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "headache", 2);
                    }

                    // Coma & Seizure
                    if (AffData.Strength > 1 && HF.Chance(0.05f))
                    {
                        HF.AddAffliction(C.Human, "coma", 14, null);
                        HF.AddAffliction(C.Human, "seizure", 10, null);
                    }
                };

            // Neurotrauma
            // Constant for Regeneration
            // Type: Non-Limb Specific, Organ Damage, Lethal
            // Caused By: Stroke, Liver Damage, Kidney Damage, Sepsis, Hypoxemia, Items, Traumatic Shock, Cyanide Poisoning, GSW (DMG).
            // Effects: Unconsciousness, Respiratory Arrest
            AfflictionsToAdd["neurotrauma"] = new("neurotrauma", 0, 200, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["neurotrauma"].Const = true;
            AfflictionsToAdd["neurotrauma"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    // Does not regenerate with a Skull Fracture
                    bool HasFracture = HF.HasAfflictionLimb(C.Human, "fracturedskull", LimbType.Head, 1);
                    double FractureModifier = HasFracture ? 0 : 1;

                    double PassiveRegeneration = -0.1 * C.GetDoubleStatStrength("healingrate") * FractureModifier * NT.DeltaTime;

                    if (PassiveRegeneration < -0.08 * NT.DeltaTime)
                    {
                        PassiveRegeneration *= 2.5;
                    }

                    AffData.Strength = Math.Clamp(AffData.Strength + PassiveRegeneration, 0, 200);

                    // Effects:
                    // Unconsciousness & Respiratory Arrest
                    if (AffData.Strength > 100)
                    {
                        NTC.SetSymptomTrue(C, "unconsciousness", 2);
                        if (HF.Chance(0.05f)) 
                        {
                            HF.AddAffliction(C.Human, "respiratoryarrest", 200, null);
                        }
                    }
                };

            // Seizure
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Stroke, Acidosis, Alkalosis, Withdrawal, Opiate Overdose, Anesthesia, Radiation Sickness
            // Effects: Unconsciousness, Spasms
            AfflictionsToAdd["seizure"] = new("seizure", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["seizure"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Regeneration:
                    AffData.Strength -= NT.DeltaTime;

                    // Effects:
                    // Spasms
                    if (AffData.Strength > 0.1)
                    {
                        NTC.SetSymptomTrue(C, "unconsciousness", 2);

                        foreach (LimbType type in Enum.GetValues<LimbType>())
                        {
                            HF.AddAfflictionLimb(C.Human, "spasm", type, 10, null);
                        }
                    }
                };

            // Coma
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Stroke, Cardiac Arrest, High Acidosis, Morbusine Poisoning, Naloxone fail.
            // Effects: Cardiac Arrest, Unconsciousness.
            AfflictionsToAdd["coma"] = new("coma", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["coma"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    // Passive Regeneration
                    if (C.GetAffData("acidosis").Strength < 20
                        && C.GetAffData("alkalosis").Strength < 20
                        && C.GetAffData("heartdamage").Strength < 30
                        && C.GetAffData("lungdamage").Strength < 40
                        && C.GetDoubleStatStrength("availableoxygen") > 60)
                    {
                        AffData.Strength -= NT.DeltaTime / 2;
                    }
                    else
                    {
                        AffData.Strength -= NT.DeltaTime / 5;
                    }

                    // Effects:
                    

                    // Unconsciousness
                    if (AffData.Strength > 15)
                    {
                        NTC.SetSymptomTrue(C, "unconsciousness", 2);

                        // Cardiac Arrest
                        if (AffData.Strength > 40 && HF.Chance(0.03f))
                        {
                            HF.AddAffliction(C.Human, "cardiacarrest", 200, null);
                        }
                    }
                };

            // Spinal Cord Injury
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Unstable Neck Fractures.
            // Effects: Paralysis (XML), Analgesia (XML).
            AfflictionsToAdd["spinalcordinjury"] = new("spinalcordinjury", 0, 100, 0);

            // Carotid Arterial Cut
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Damage.
            // Effects: Blood Loss (XML), Internal Bleeding (XML). Increases self via XML.
            AfflictionsToAdd["carotidarterialcut"] = new("carotidarterialcut", 0, 100, 0);

            // =============== Item Derived =============== //

            // Adrenaline in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Adrenaline item.
            // Effects: Melee Damage increased (XML), Analgesia (XML).
            AfflictionsToAdd["afadrenaline"] = new("afadrenaline", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afadrenaline"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Originally had a maxstrength of 100, and reduced by 1 per second in XML.
                    // Adjusted, that became 2 per 2 seconds.
                    AffData.Strength -= 2;
                };

            // Needle in Chest
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Needle item.
            // Effects: Reduced Pneumothorax / Cardiac Tamponade.
            AfflictionsToAdd["needlec"] = new("needlec", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["needlec"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    AffData.Strength -= 0.15 * NT.DeltaTime;
                };

            // Saline in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Saline item.
            // Effects: Increased Acidosis, Blood Pressure.
            AfflictionsToAdd["afsaline"] = new("afsaline", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afsaline"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                    // Adjusted, that became 0.5 per 2 seconds.
                    AffData.Strength -= 0.5;

                    // Effects:
                    // Acidosis
                    HF.AddAffliction(C.Human, "acidosis", 0.2f, null);

                    // Blood Pressure (in BloodPressure constant)
                };

            // Ringers Solution in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Ringer's Solution item.
            // Effects: Increased Alkalosis, Blood Pressure.
            AfflictionsToAdd["afringerssolution"] = new("afringerssolution", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afringerssolution"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                    // Adjusted, that became 0.5 per 2 seconds.
                    AffData.Strength -= 0.5;

                    // Effects:
                    // Alkalosis
                    HF.AddAffliction(C.Human, "alkalosis", 0.2f, null);

                    // Blood Pressure (in BloodPressure constant)
                };


            // Mannitol in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Mannitol Item.
            // Effects: Reduce Neurotrauma.
            AfflictionsToAdd["afmannitol"] = new("afmannitol", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afmannitol"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Originally had a maxstrength of 100, and reduced by 0.5 per second in XML.
                    // Adjusted, that became 1 per 2 seconds.
                    AffData.Strength -= 1;

                    // Effects:
                    // Reduce Neurotrauma if Blood Pressure and Hypoxemia conditions are met.
                    if (C.GetAffData("bloodpressure").Strength >= 70 && C.GetAffData("hypoxemia").Strength <= 30)
                    {
                        HF.AddAffliction(C.Human, "neurotrauma", -2, null);
                    }
                };

            // Immunosuppressants in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Azathioprine Item.
            // Effects: Reduce Immunity.
            AfflictionsToAdd["afimmunosuppressant"] = new("afimmunosuppressant", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afimmunosuppressant"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                    // Adjusted, that became 0.5 per 2 seconds.
                    AffData.Strength -= 0.5;

                    // Effects:
                    // Reduce Immunity
                    if (C.GetAffData("immunity").Strength >= 2.5)
                    {
                        HF.AddAffliction(C.Human, "immunity", -8, null);
                    }
                };

            // Pressure-increasing drugs in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Nitroglycerin, Sodium Nitroprusside Items.
            // Effects: Increase target Blood Pressure.
            AfflictionsToAdd["afpressuredrug"] = new("afpressuredrug", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afpressuredrug"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Passive Decrease
                    // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                    // Adjusted, that became 0.5 per 2 seconds.
                    AffData.Strength -= 0.5;

                    // Effects:
                    // Blood Pressure (in BloodPressure constant)
                };

            // Thiamine in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Thiamine Item.
            // Effects: Increase specific organ damage healing.
            AfflictionsToAdd["afthiamine"] = new("afthiamine", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afthiamine"].UpdateAction =
               (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
               {
                   // Passive Decrease
                   // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                   // Adjusted, that became 0.5 per 2 seconds.
                   AffData.Strength -= 0.5;

                   // Effects:
                   // Additional Healing (in HF.NewOrganDamage)
               };

            // Streptokinase in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Streptokinase Item.
            // Effects: Increase stroke chance, cure Heart Attack / Hemotransfusion shock.
            AfflictionsToAdd["afstreptokinase"] = new("afstreptokinase", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afstreptokinase"].UpdateAction =
               (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
               {
                   // Passive Decrease
                   // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                   // Adjusted, that became 0.5 per 2 seconds.
                   AffData.Strength -= 0.5;

                   // Effects:
                   // Cures Heart Attack / HemoTransShock in ItemFunctions
                   // Hypertension Stroke (in BloodPressure constant)
               };

            // Antibiotics in Blood
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Item Derived
            // Caused By: Broad-Spectrum Antibiotics Item.
            // Effects: Decreases Sepsis, extra Organ Damage, decreased Husk Infection.
            AfflictionsToAdd["afantibiotics"] = new("afantibiotics", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["afantibiotics"].UpdateAction =
               (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
               {
                   // Passive Decrease
                   // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                   // Adjusted, that became 0.5 per 2 seconds.
                   AffData.Strength -= 0.5;

                   // Effects:
                   // Organ Damage
                   HF.AddAffliction(C.Human, "organdamage", 0.4f, null);
                   HF.AddAffliction(C.Human, "kidneydamage", 0.35f, null);
                   HF.AddAffliction(C.Human, "liverdamage", 0.35f, null);
                   HF.AddAffliction(C.Human, "heartdamage", 0.2f, null);
                   HF.AddAffliction(C.Human, "lungdamage", 0.2f, null);

                   // Reduce Husk Infection
                   if (HF.HasAffliction(C.Human, "huskinfection") && C.GetAffData("huskinfection").Strength < 75)
                   {
                       HF.AddAffliction(C.Human, "huskinfection", -1, null);
                   }

                   // Sepsis
                   if (C.GetAffData("sepsis").Strength > 0)
                   {
                       HF.AddAffliction(C.Human, "sepsis", -2, null);
                   }
               };

            // =============== Surgical =============== //

            // Traumatic Shock
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific, Lethal
            // Caused By: Unsafe Surgery
            // Effects: Hypotension, Cardiac Arrest, Respiratory Arrest, Neurotrauma, Psychosis, Pain.
            AfflictionsToAdd["traumaticshock"] = new("traumaticshock", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["traumaticshock"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Removes on TShockTimeout
                    if (C.GetAffData("tshocktimeout").Strength > 0)
                    {
                        AffData.Strength = 0;
                        return;
                    }

                    // Passive Decrease
                    bool IsSedated = C.GetBoolStatStrength("sedated");
                    bool IsSafeSurgery = C.GetAffData("safesurgery").Strength > 0;
                    bool IsAnesthesized = C.GetAffData("anesthesia").Strength > 15;

                    HF.Print($"IsSedated: {IsSedated}");
                    HF.Print($"SafeSurgery: {IsSafeSurgery}");
                    HF.Print($"Anesthesized: {IsAnesthesized}");

                    bool ShouldReduce = (IsSedated && IsSafeSurgery || IsAnesthesized);

                    AffData.Strength -= (0.5 + HF.BoolToNum(ShouldReduce, 1.5f)) * NT.DeltaTime;

                    // Effects:
                    // Pain & Psychosis
                    if (AffData.Strength > 5)
                    {
                        if (C.GetSymptomAffData("unconsciousness").Strength < 0.1)
                        {
                            HF.AddAffliction(C.Human, "shockpain", (float)(10 * NT.DeltaTime), null);
                            HF.AddAffliction(C.Human, "psychosis", (float)(AffData.Strength / 100 * NT.DeltaTime), null);
                        }

                        // Respiratory Arrest
                        if (AffData.Strength > 30 && HF.Chance(0.2f))
                        {
                            HF.AddAffliction(C.Human, "respiratoryarrest", 200, null);
                        }

                        // Cardiac Arrest
                        if (AffData.Strength > 40 && HF.Chance(0.1f))
                        {
                            HF.AddAffliction(C.Human, "cardiacarrest", 200, null);
                        }
                    }
                };

            // Ballooned Aorta
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Endovascular Balloon item.
            // Effects: Gangrene in extremities (XML), Organ Damage, Specific Organ Damage, Reduced Bleeding in extremities (XML).
            AfflictionsToAdd["balloonedaorta"] = new("balloonedaorta", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["balloonedaorta"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Effects:
                    // Vanilla Organ Damage
                    HF.AddAffliction(C.Human, "organdamage", 1, null);

                    // Specific Organ Damage
                    HF.AddAffliction(C.Human, "liverdamage", 1, null);
                    HF.AddAffliction(C.Human, "kidneydamage", 1, null);
                };

            // =============== Torso =============== //
            // Aortic Rupture
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Damage.
            // Effects: Blood Loss (XML), Internal Bleeding (XML), Chest Pain, Abdominal Pain, Unconsciousness.
            AfflictionsToAdd["aorticrupture"] = new("aorticrupture", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["aorticrupture"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Chest Pain & Abdominal Pain & Unconsciousness
                    if (AffData.Strength > 0)
                    {
                        if (C.GetSymptomAffData("unconsciousness").Strength <= 0 && (!C.GetBoolStatStrength("sedated")))
                        {
                            NTC.SetSymptomTrue(C, "chestpain", 2);
                            NTC.SetSymptomTrue(C, "abdominalpain", 2);
                        }
                    }
                };

            // Internal Bleeding
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Damage.
            // Effects: Blood Loss (XML), Internal Bleeding (XML), Chest Pain, Abdominal Pain, Unconsciousness.
            AfflictionsToAdd["internalbleeding"] = new("internalbleeding", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["internalbleeding"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    // Passive Regeneration
                    AffData.Strength -= NT.DeltaTime * 0.02 * C.GetDoubleStatStrength("clottingrate");

                    // Effects:
                    // Blood Loss
                    if (AffData.Strength > 0)
                    {
                        HF.AddAffliction(C.Human, "bloodloss", (float)(AffData.Strength * (1f / 40f) * NT.DeltaTime), null);

                        // Vomiting Blood
                        if (AffData.Strength > 50)
                        {
                            NTC.SetSymptomTrue(C, "vomitingblood", 2);
                        }
                    }
                };

            // =============== Bones =============== //

            // Bone Damage
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Kidney Damage, Radiation Sickness, Sepsis, Hypoxemia.
            // Effects: Bone Death, Fractures.
            AfflictionsToAdd["bonedamage"] = new("bonedamage", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["bonedamage"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    // Passive Regeneration
                    AffData.Strength = HF.OrganDamageCalc(C, AffData.Strength);

                    // Bone Regeneration
                    if (AffData.Strength < 90)
                    {
                        AffData.Strength -= C.GetDoubleStatStrength("bonegrowthCount") * 0.3 * NT.DeltaTime;
                    }
                    else if (C.GetDoubleStatStrength("bonegrowthCount") >= 6)
                    {
                        AffData.Strength -= 2 * NT.DeltaTime;
                    }

                    // Fractures
                    if (AffData.Strength > 90 && HF.Chance(0.01f))
                    {
                        HF.BreakLimb(C.Human, Limb);
                    }
                };

            // =============== MUST RUN AFTER EVERYTHING ELSE =============== //
            // Probably needs even special treatment than this.

            // Slowdown 
            // Constant; too complicated otherwise.
            // Type: Functionality
            // Effects: Decreases character by a percentage proportional to the affliction strength.
            AfflictionsToAdd["slowdown"] = new("slowdown", 0, 100, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["slowdown"].Const = true;
            AfflictionsToAdd["slowdown"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    AffData.Strength = C.GetDoubleStatStrength("slowdown");
                };

            // Stun 
            // Constant; too complicated otherwise.
            // Type: Functionality
            // Effects: Used to stun the character.
            AfflictionsToAdd["stun"] = new("stun", 0, 30, 0, AfflictionPriority.HIGH);
            AfflictionsToAdd["stun"].Const = true;
            AfflictionsToAdd["stun"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    if (C.GetNonLimbAffStrength("spinalcordinjury") > 0
                        || C.GetNonLimbAffStrength("anesthesia") > 15
                        || NTC.HasSymptom(C,"unconsciousness"))
                    {
                        AffData.Strength = Math.Max(5,AffData.Strength);
                    }
                };

            // Now add these afflictions.
            foreach (KeyValuePair<string,NTNonLimbAffliction> Pair in AfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddLimbAfflictions()
        {
            // Surgical Incision
            // Not constant; gets applied by other sources.
            // Type: Limb Specific, Surgical
            // Caused By: Scalpel.
            // Effects: Blood Loss, Traumatic Shock, increases self in XML.
            LimbAfflictionsToAdd["surgeryincision"] = new("surgeryincision", 0, 100, 0);

            // Clamped Bleeding
            // Not constant; gets applied by other sources.
            // Type: Limb Specific, Surgical
            // Caused By: Hemostat.
            // Effects: Prevents Surgery Incision Blood Loss (Scalpel XML).
            LimbAfflictionsToAdd["clampedbleeding"] = new("clampedbleeding", 0, 100, 0);

            // Drilled Bones
            // Not constant; gets applied by other sources.
            // Type: Limb Specific, Surgical
            // Caused By: Surgical Drill.
            // Effects: Applies Traumatic Shock (XML).
            LimbAfflictionsToAdd["drilledbones"] = new("drilledbones", 0, 100, 0);

            // Retracted Skin
            // Not constant; gets applied by other sources.
            // Type: Limb Specific, Surgical
            // Caused By: Skin Retractors.
            // Effects: Applies Traumatic Shock (XML).
            LimbAfflictionsToAdd["retractedskin"] = new("retractedskin", 0, 100, 0);

            // Sutured Incision
            // Not constant; gets applied by other sources.
            // Type: Limb Specific, Surgical
            // Caused By: Stitching a Surgical Incision.
            // Effects: None.
            LimbAfflictionsToAdd["suturedi"] = new("suturedi", 0, 100, 0, AfflictionPriority.MEDIUM);
            LimbAfflictionsToAdd["suturedi"].UpdateAction =
               (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
               {
                   // Passive Decrease
                   // Originally had a maxstrength of 100, and reduced by 1 per second in XML.
                   // Adjusted, that became 4 per 4 seconds.
                   AffData.Strength[Limb] -= 4;
               };

            // Sutured Wound
            // Not constant; gets applied by other sources.
            // Type: Limb Specific, Surgical
            // Caused By: Stitching an Open Wound.
            // Effects: Vitality damage proportional to affliction strength.
            LimbAfflictionsToAdd["suturedw"] = new("suturedw", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["suturedi"].UpdateAction =
               (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
               {
                   // Passive Decrease
                   // Originally had a maxstrength of 100, and reduced by 1 per second in XML.
                   // Adjusted, that became 0.44 per 2 seconds.
                   AffData.Strength[Limb] -= 0.4;
               };

            // Sawed Bones
            // Not constant; gets applied by other sources.
            // Type: Limb Specific, Surgical
            // Caused By: Surgical Saw.
            // Effects: Applies Traumatic Shock (XML).
            LimbAfflictionsToAdd["sawedbones"] = new("sawedbones", 0, 100, 0);

            // Bleeding
            // Not constant; gets applied by other sources.
            // Type: Limb Specific, Basegame Override
            // Caused By: Damage, failed skill checks.
            // Effects: Blood Loss (Hardcoded?)
            LimbAfflictionsToAdd["bleeding"] = new("bleeding", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["bleeding"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Regeneration
                    AffData.Strength[Limb] -= (C.GetDoubleStatStrength("clottingrate") * 0.1
                        + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 0.5
                        + Math.Clamp(C.GetLimbAffStrength("bandageddirty", Limb), 0, 1) * 0.25
                    ) * NT.DeltaTime;
                };

            // Stimulated Bone Growth
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Damage, failed skill checks.
            // Effects: Decreases bone damage.
            LimbAfflictionsToAdd["stimulatedbonegrowth"] = new("stimulatedbonegrowth", 0, 100, 0, AfflictionPriority.MEDIUM);
            LimbAfflictionsToAdd["stimulatedbonegrowth"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Regeneration
                    // Originally had a maxstrength of 100, and reduced by 0.5 per second in XML.
                    // Adjusted, that became 1 per 2 seconds.
                    AffData.Strength[Limb] -= 1;
                };

            // Arm + Leg Fractures
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Damage, failed skill checks.
            // Effects: Pain, lost ability of limb, Internal Damage.
            LimbAfflictionsToAdd["fracturedextremity"] = new("fracturedextremity", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["fracturedextremity"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    bool HasCast = HF.HasAfflictionLimb(C.Human, "plastercast", Limb);
                    bool HasBandage = HF.HasAfflictionLimb(C.Human, "bandaged", Limb) || HF.HasAfflictionLimb(C.Human, "bandageddirty", Limb);

                    // Arms: halt progression between 90-100 if bandaged
                    if (Limb == LimbType.LeftArm || Limb == LimbType.RightArm)
                    {
                        if (AffData.Strength[Limb] > 90 && AffData.Strength[Limb] < 100 && HasBandage)
                        {
                            return;
                        }
                    }

                    // Passive Increase if no cast
                    AffData.Strength[Limb] += 2 * HF.BoolToNum(!HasCast) * NT.DeltaTime;

                    // Legs: adrenaline causes Bleeding if no cast and not ragdolled
                    if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
                    {
                        if (!HasCast && HF.HasAffliction(C.Human, "afadrenaline", 1) && !C.Human.IsRagdolled)
                        {
                            HF.AddAfflictionLimb(C.Human, "bleeding", Limb, 15, null);
                        }
                    }

                    // Internal Damage if no cast
                    if (!HasCast && !C.GetBoolStatStrength("sedated") && (HF.LimbIsExtremity(Limb) || !HasBandage))
                    {
                        HF.AddAfflictionLimb(C.Human, "internaldamage", Limb, (float)(0.1 * NT.DeltaTime), null);
                    }
                };

            // Arm + Leg Dislocation
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Damage, failed skill checks.
            // Effects: Pain (XML), lost ability of limb, Internal Damage.
            LimbAfflictionsToAdd["dislocation"] = new("dislocation", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["dislocation"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // If painlessness is present, don't cause problems
                    if (C.GetBoolStatStrength("sedated")) return;

                    if (C.GetLimbAffStrength("plastercast", Limb) <= 0 && C.GetLimbAffStrength("bandaged", Limb) <= 0 && C.GetLimbAffStrength("bandageddirty", Limb) <= 0)
                    {
                        HF.AddAfflictionLimb(C.Human, "internaldamage", Limb, (float)(0.1 * NT.DeltaTime), null);
                        if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
                        {
                            C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.8); // slow the character down.
                        }
                    }
                };

            // Tourniquet around Extremity
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Tourniquet item.
            // Effects: Reduces Bleeding (XML), Gangrene.
            LimbAfflictionsToAdd["tourniqueted"] = new("tourniqueted", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["tourniqueted"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Effects:
                    // Gangrene
                    HF.AddAfflictionLimb(C.Human, "gangrene", Limb, (float)(HF.BoolToNum(HF.Chance(0.1f)) * 0.5 * NTConfig.Get("NT_gangrenespeed", 1) * NT.DeltaTime), null);
                };

                    
            // Plaster Cast
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Gypsum item.
            // Effects: Heals fractures, slows character.
            LimbAfflictionsToAdd["plastercast"] = new("plastercast", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["plastercast"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Effects:
                    // Leg slowdown
                    if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
                    {
                        C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.8);
                    }
                    
                    // Heal Fracture
                    HF.BreakLimb(C.Human, Limb, (float)(-(100.0 / 300.0) * NT.DeltaTime));
                };

            // Arterial Cut on Extremity
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Damage.
            // Effects: Blood Loss (XML).
            LimbAfflictionsToAdd["arterialcut"] = new("arterialcut", 0, 100, 0);

            // Gangrene
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Tourniquets, Sepsis, Aortic Balloon (XML).
            // Effects: Blood Loss (XML).
            LimbAfflictionsToAdd["gangrene"] = new("gangrene", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["gangrene"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Limb must be an extremity
                    if (!HF.LimbIsExtremity(Limb)) return;

                    // Surgical amputation prevents Gangrene on that stump
                    if (HF.LimbIsSurgicallyAmputated(C.Human, Limb))
                    {
                        AffData.Strength[Limb] = 0;
                        return;
                    }

                    // Passive Regeneration below 15
                    if (AffData.Strength[Limb] < 15)
                    {
                        AffData.Strength[Limb] -= 0.01 * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                    }
                };

            // Bandage applied to Limb
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Bandage items.
            // Effects: Reduces bleeding, heals wounds, reduces infection.
            LimbAfflictionsToAdd["bandaged"] = new("bandaged", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["bandaged"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    double WoundDamage = C.GetLimbAffStrength("firstdegreeburn", Limb)
                        + C.GetLimbAffStrength("seconddegreeburn", Limb)
                        + C.GetLimbAffStrength("thirddegreeburn", Limb)
                        + C.GetLimbAffStrength("lacerations", Limb)
                        + C.GetLimbAffStrength("foreignbody", Limb)
                        + C.GetLimbAffStrength("arterialcut", Limb)
                        + C.GetLimbAffStrength("infectedwound", Limb);

                    double BandageDirtifySpeed = 0.1
                        + Math.Clamp(WoundDamage / 100, 0, 0.4)
                        + C.GetLimbAffStrength("bleeding", Limb) / 20;

                    // Dirtify bandage over time
                    AffData.Strength[Limb] -= BandageDirtifySpeed * NT.DeltaTime;

                    float DirtyBandageStrength = (float)C.GetLimbAffStrength("bandageddirty", Limb);

                    // Transition to dirty bandage
                    if (AffData.Strength[Limb] <= 0.5f)
                    {
                        HF.SetAfflictionLimb(C.Human, "bandageddirty", Limb, (float)Math.Max(DirtyBandageStrength, 1), null);
                        AffData.Strength[Limb] = 0f;
                    }

                    if (DirtyBandageStrength > 0)
                    {
                        HF.AddAfflictionLimb(C.Human, "bandageddirty", Limb, (float)(BandageDirtifySpeed * NT.DeltaTime), null);
                    }
                        
                    // Effects:
                    // Slowdown
                    C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.9);

                    // Wound Healing
                    HF.AddAfflictionLimb(C.Human, "lacerations", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.1 * NT.DeltaTime), null);
                    HF.AddAfflictionLimb(C.Human, "firstdegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.1 * NT.DeltaTime), null);
                    HF.AddAfflictionLimb(C.Human, "seconddegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.1 * NT.DeltaTime), null);
                    HF.AddAfflictionLimb(C.Human, "thirddegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.1 * NT.DeltaTime), null);

                    // Infection Healing
                    if (C.GetLimbAffStrength("infectedwound", Limb) > 0)
                    {
                        HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 1.5 * NT.DeltaTime), null);
                    }
                };

            // Dirty Bandage around Limb
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Dirtyfication.
            // Effects: Reduces bleeding, heals wounds, causes infection.
            LimbAfflictionsToAdd["bandageddirty"] = new("bandageddirty", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["bandageddirty"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    float BandagedStrength = (float)C.GetLimbAffStrength("bandaged", Limb);
                    if (BandagedStrength > 0)
                    {
                        HF.SetAfflictionLimb(C.Human, "bandaged", Limb, 0);
                    }

                    double WoundDamage = C.GetLimbAffStrength("firstdegreeburn", Limb)
                        + C.GetLimbAffStrength("seconddegreeburn", Limb)
                        + C.GetLimbAffStrength("thirddegreeburn", Limb)
                        + C.GetLimbAffStrength("lacerations", Limb)
                        + C.GetLimbAffStrength("foreignbody", Limb)
                        + C.GetLimbAffStrength("arterialcut", Limb)
                        + C.GetLimbAffStrength("infectedwound", Limb);

                    double BandageDirtifySpeed = 0.1
                        + Math.Clamp(WoundDamage / 100, 0, 0.4)
                        + C.GetLimbAffStrength("bleeding", Limb) / 20;

                    AffData.Strength[Limb] += BandageDirtifySpeed * NT.DeltaTime;

                    // Effects:
                    // Slowdown
                    C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.9);

                    // Wound Healing
                    HF.AddAfflictionLimb(C.Human, "lacerations", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.05 * NT.DeltaTime), null);
                    HF.AddAfflictionLimb(C.Human, "firstdegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.05 * NT.DeltaTime), null);
                    HF.AddAfflictionLimb(C.Human, "seconddegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.05 * NT.DeltaTime), null);
                    HF.AddAfflictionLimb(C.Human, "thirddegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.05 * NT.DeltaTime), null);
                };

            // Gel Coolant Pack applied to Limb
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Gel Coolant Pack.
            // Effects: Amplifies healing, slows character.
            LimbAfflictionsToAdd["iced"] = new("iced", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["iced"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Decrease
                    AffData.Strength[Limb] -= 1.7 * NT.DeltaTime;

                    // Effects:
                    // Slowdown (5% per limb)
                    C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.95);

                    // Effects:
                    // Reduce Internal Bleeding if on Torso
                    if (Limb == LimbType.Torso)
                    {
                        HF.AddAffliction(C.Human, "internalbleeding", (float)(-0.2 * NT.DeltaTime), null);
                    }

                    // Heal Blunt Force Trauma
                    HF.AddAfflictionLimb(C.Human, "blunttrauma", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.3 * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime), null);
                };

            // Antibiotic Ointment applied to Limb
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Antibiotic Ointment.
            // Effects: Amplifies healing.
            LimbAfflictionsToAdd["ointmented"] = new("ointmented", 0, 100, 0, AfflictionPriority.MEDIUM);
            LimbAfflictionsToAdd["ointmented"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Decrease
                    AffData.Strength[Limb] -= 1.2 * NT.DeltaTime;

                    // Effects:
                    // Reduce Infected Wounds
                    if (C.GetLimbAffStrength("infectedwound", Limb) <= 60) 
                    {
                        HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(-3 * NT.DeltaTime), null);
                    }   
                };

            // Infected Wound
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Burns, Foreign Bodies, Lacerations, Explosive Damage, Gunshot Wounds.
            // Effects: Inflammation.
            LimbAfflictionsToAdd["infectedwound"] = new("infectedwound", 0, 100, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["infectedwound"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    // Passive decrease from immunity, bandaged, ointmented
                    double InfectIndex = (-C.GetBloodAffData("immunity").PrevStrength / 200
                        - Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 1.5
                        - C.GetLimbAffStrength("ointmented", Limb) * 3
                    ) * NT.DeltaTime;

                    // Dirty bandage :skull:
                    if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                    {
                        InfectIndex += (C.GetLimbAffStrength("bandageddirty", Limb) / 20) * NT.DeltaTime;
                    }
                        
                    if (InfectIndex > 0)
                    {
                        InfectIndex *= NTConfig.Get("NT_infectionRate", 1) * Math.Clamp(C.GetLimbAffStrength("iced", Limb), 1, 10);
                    }
                        
                    AffData.Strength[Limb] += InfectIndex;

                    // Effects:
                    // Inflammation
                    if (AffData.Strength[Limb] > 10)
                    {
                        HF.AddAfflictionLimb(C.Human, "inflammation", Limb, (float)(0.15 * NT.DeltaTime), null);
                    }
                };

            // Foreign Body
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Damage, fractures.
            // Effects: Inflammation, Sepsis.
            LimbAfflictionsToAdd["foreignbody"] = new("foreignbody");
            LimbAfflictionsToAdd["foreignbody"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Decrease
                    if (AffData.Strength[Limb] < 15)
                    {
                        AffData.Strength[Limb] -= 0.05 * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                    }

                    // Arterial Cut chance
                    double ForeignBodyAbove20 = AffData.Strength[Limb] >= 20 ? AffData.Strength[Limb] : 0;
                    double ForeignBodyCutChance = Math.Pow(ForeignBodyAbove20 / 100, 6) * 0.5;

                    if (C.GetLimbAffStrength("bleeding", Limb) > 80 || HF.Chance((float)ForeignBodyCutChance))
                    {
                        HF.ArteryCutLimb(C.Human, Limb);
                    }

                    // Effects:
                    // Sepsis
                    double GangreneAbove15 = C.GetLimbAffStrength("gangrene", Limb) >= 15 ? C.GetLimbAffStrength("gangrene", Limb) : 0;
                    double InfectedAbove50 = C.GetLimbAffStrength("infectedwound", Limb) >= 50 ? C.GetLimbAffStrength("infectedwound", Limb) : 0;

                    double SepsisChance = GangreneAbove15 / 400
                        + InfectedAbove50 / 1000
                        + ForeignBodyCutChance;

                    if (HF.Chance((float)SepsisChance))
                    {
                        HF.AddAffliction(C.Human, "sepsis", (float)(NT.DeltaTime * NTConfig.Get("NT_SepsisRate", 1)), null);
                    }

                    // Inflammation
                    if (AffData.Strength[Limb] > 15)
                    {
                        HF.AddAfflictionLimb(C.Human, "inflammation", Limb, (float)(0.15 * NT.DeltaTime), null);
                    }

                    // Infected Wounds
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double ForeignBodyInfectIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                    HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(ForeignBodyInfectIndex / 5), null);
                    HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(ForeignBodyInfectIndex / 3, 0, 10), null);

                    if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                    {
                        double ForeignBodyDirtyIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                        HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(ForeignBodyDirtyIndex / 5), null);
                        HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(ForeignBodyDirtyIndex / 3, 0, 10), null);
                    }
                };

            // Burn
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Fire, items, damage.
            // Effects: Specific Burns, infection.
            LimbAfflictionsToAdd["burn"] = new("burn", 0, 200, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["burn"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Decrease
                    if (AffData.Strength[Limb] < 50)
                    {
                        AffData.Strength[Limb] -= (C.GetBloodAffData("immunity").PrevStrength / 3000
                            + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 0.1
                            + Math.Clamp(C.GetLimbAffStrength("ointmented", Limb), 0, 1) * 0.12) 
                        * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                    }
                        
                    double burn = AffData.Strength[Limb];

                    // Conversion:
                    // First-degree Burns
                    HF.SetAfflictionLimb(C.Human, "firstdegreeburn", Limb, (float)((burn < 1 || burn > 20) ? 0 : burn * 5), null, 0);

                    // Second-degree Burns
                    HF.SetAfflictionLimb(C.Human, "seconddegreeburn", Limb, (float)((burn <= 20 || burn > 50) ? 0 : Math.Max(5, (burn - 20) / 30 * 100)), null, 0);

                    // Third-degree Burns
                    HF.SetAfflictionLimb(C.Human, "thirddegreeburn", Limb, (float)(burn <= 50 ? 0 : Math.Clamp((burn - 50) / 50 * 100, 5, 100)), null, 0);

                    // Effects:
                    // Infected Wounds
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double BurnInfectIndex = AffData.Strength[Limb] / 20 * NT.DeltaTime;
                    HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(BurnInfectIndex / 5), null);
                    HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(BurnInfectIndex / 3, 0, 10), null);

                    // Dirty Bandage results
                    if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                    {
                        double BurnDirtyIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                        HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(BurnDirtyIndex / 5), null);
                        HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(BurnDirtyIndex / 3, 0, 10), null);
                    }
                };

            // First-degree Burns
            LimbAfflictionsToAdd["firstdegreeburn"] = new("firstdegreeburn", 0, 100, 0);

            // Second-degree Burns
            LimbAfflictionsToAdd["seconddegreeburn"] = new("seconddegreeburn", 0, 100, 0);

            // Third-degree Burns
            LimbAfflictionsToAdd["thirddegreeburn"] = new("thirddegreeburn", 0, 100, 0);

            // Lacerations
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Damage, failed skill checks.
            // Effects: Damage, infection.
            LimbAfflictionsToAdd["lacerations"] = new("lacerations");
            LimbAfflictionsToAdd["lacerations"].UpdateAction  =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Regeneration
                    if (AffData.Strength[Limb] < 50)
                    {
                        AffData.Strength[Limb] -= (
                                C.GetBloodAffData("immunity").PrevStrength / 3000
                                + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * .1
                                + Math.Clamp(C.GetLimbAffStrength("ointmented", Limb), 0, 1) * .12
                                )
                                * C.GetDoubleStatStrength("healingrate")
                                * NT.DeltaTime;
                    }

                    // Effects:
                    // Infected Wounds
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double LacerationInfectIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                    HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(LacerationInfectIndex / 5), null);
                    HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(LacerationInfectIndex / 3, 0, 10), null);

                    if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                    {
                        double LacerationDirtyIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                        HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(LacerationDirtyIndex / 5), null);
                        HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(LacerationDirtyIndex / 3, 0, 10), null);
                    }
                };

            // Gunshot Wound
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Getting shot.
            // Effects: Damage, infection.
            LimbAfflictionsToAdd["gunshotwound"] = new("gunshotwound", 0, 200, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["gunshotwound"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Regeneration
                    if (AffData.Strength[Limb] < 50)
                    {
                        AffData.Strength[Limb] -= (
                            C.GetBloodAffData("immunity").PrevStrength / 3000
                            + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 0.1
                            + Math.Clamp(C.GetLimbAffStrength("ointmented", Limb), 0, 1) * 0.12
                        ) * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                    }

                    // Effects:
                    // Infected Wounds
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double GSWInfectIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                    HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(GSWInfectIndex / 5), null);
                    HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(GSWInfectIndex / 3, 0, 10), null);

                    if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                    {
                        double GSWDirtyIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                        HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(GSWDirtyIndex / 5), null);
                        HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(GSWDirtyIndex / 3, 0, 10), null);
                    }
                };

            // Explosion Damage
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Being near explosions.
            // Effects: Damage, infection.
            LimbAfflictionsToAdd["explosiondamage"] = new("explosiondamage", 0, 200, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["explosiondamage"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Regeneration
                    if (AffData.Strength[Limb] < 50)
                    {
                        AffData.Strength[Limb] -= (
                            C.GetBloodAffData("immunity").PrevStrength / 3000
                            + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 0.1
                            + Math.Clamp(C.GetLimbAffStrength("ointmented", Limb), 0, 1) * 0.12
                        ) * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                    }

                    // Effects:
                    // Infected Wounds
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double ExplosionDamageInfectIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                    HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(ExplosionDamageInfectIndex / 5), null);
                    HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(ExplosionDamageInfectIndex / 3, 0, 10), null);

                    if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                    {
                        double ExplosionDamageDirtyIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                        HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(ExplosionDamageDirtyIndex / 5), null);
                        HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(ExplosionDamageDirtyIndex / 3, 0, 10), null);
                    }
                };

            // Bite Wounds
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Being chomped.
            // Effects: Damage, infection.
            LimbAfflictionsToAdd["bitewounds"] = new("bitewounds", 0, 200, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["bitewounds"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Regeneration
                    if (AffData.Strength[Limb] < 100)
                    {
                        AffData.Strength[Limb] -= (
                            C.GetBloodAffData("immunity").PrevStrength / 3000
                            + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 0.1
                            + Math.Clamp(C.GetLimbAffStrength("ointmented", Limb), 0, 1) * 0.12
                        ) * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                    }

                    // Effects:
                    // Infected Wounds
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double BitesInfectIndex = AffData.Strength[Limb] / 30 * NT.DeltaTime;
                    HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(BitesInfectIndex / 5), null);
                    HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(BitesInfectIndex / 3, 0, 10), null);

                    if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                    {
                        double BitesDirtyIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                        HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(BitesDirtyIndex / 5), null);
                        HF.AddAffliction(C.Human, "immunity", (float)-Math.Clamp(BitesDirtyIndex / 3, 0, 10), null);
                    }
                };

            // Blunt Force Trauma
            // Not constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Fall damage or weapons.
            // Effects: Damage.
            LimbAfflictionsToAdd["blunttrauma"] = new("blunttrauma", 0, 200, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["blunttrauma"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Regeneration
                    if (AffData.Strength[Limb] < 100)
                    {
                        AffData.Strength[Limb] -= (
                            C.GetBloodAffData("immunity").PrevStrength / 8000
                            + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 0.1
                            + Math.Clamp(C.GetLimbAffStrength("iced", Limb), 0, 1) * 0.3
                            + Math.Clamp(C.GetLimbAffStrength("ointmented", Limb), 0, 1) * 0.12
                        ) * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                    }
                };

            // Internal Damage
            // Not Constant; gets applied by other sources.
            // Type: Limb Specific
            // Caused By: Dislocations, Fractures, Neck Fractures, sustaining damage.
            // Effects: Damage.
            LimbAfflictionsToAdd["internaldamage"] = new("internaldamage", 0, 200, 0, AfflictionPriority.HIGH);
            LimbAfflictionsToAdd["internaldamage"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                    if (!(AffData.Strength[Limb] > 0)) return;

                    // Passive Regeneration
                    if (AffData.Strength[Limb] < 50)
                    {
                        AffData.Strength[Limb] -= 0.05f * (float)C.GetDoubleStatStrength("healingrate") * (float)NT.DeltaTime;
                    }
                };

            foreach (KeyValuePair<string, NTLimbAffliction> Pair in LimbAfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddBloodAfflictions()
        {
            // Blood afflictions are literally the same to write as NonLimbAfflictions, they're just here for organization purposes.

            // Blood Loss
            // Not constant; gets applied by other sources.
            // Type: Non-Limb Specific
            // Caused By: Bleeding, Damage.
            // Effects: Changes Blood Pressure.
            BloodAfflictionsToAdd["bloodloss"] = new("bloodloss", 0, 200, 0);

            // Blood Pressure
            // Constant; too complicated otherwise.
            // Type: Vital Mechanic
            // Handles the entire blood pressure system and application of effects.
            BloodAfflictionsToAdd["bloodpressure"] = new("bloodpressure", 0, 200, 100, AfflictionPriority.HIGH);
            BloodAfflictionsToAdd["bloodpressure"].Const = true;
            BloodAfflictionsToAdd["bloodpressure"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
                {
                    // If a character has no blood pressure, apply it.
                    if (!(HF.HasAffliction(C.Human, "bloodpressure")))
                    {
                        AffData.Strength = 100f;
                    }

                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    double desiredBloodPressure = (
                        C.GetDoubleStatStrength("bloodamount")
                        - C.GetNonLimbAffData("tamponade").Strength / 2
                        - Math.Clamp(C.GetAffData("afpressuredrug").Strength * 5, 0, 45)
                        - Math.Clamp(C.GetAffData("anesthesia").Strength, 0, 15)
                        + Math.Clamp(C.GetAffData("afadrenaline").Strength * 10, 0, 30)
                        + Math.Clamp(C.GetAffData("afsaline").Strength * 5, 0, 30)
                        + Math.Clamp(C.GetAffData("afringerssolution").Strength * 5, 0, 30)
                    )
                        * (1 + 0.5 * Math.Pow(C.GetAffData("liverdamage").Strength / 100, 2))
                        * (1 + 0.5 * Math.Pow(C.GetAffData("kidneydamage").Strength / 100, 2))
                        * (1 + C.GetAffData("alcoholwithdrawal").Strength / 200)
                        * Math.Clamp((100 - C.GetAffData("traumaticshock").Strength * 2) / 100, 0, 1)
                        * ((100 - C.GetNonLimbAffData("fibrillation").Strength) / 100)
                        * (1 - Math.Min(1, C.GetNonLimbAffData("cardiacarrest").Strength))
                        * NTC.GetMultiplier(C, "bloodpressure");

                    double bloodPressureLerp = 0.2 * NTC.GetMultiplier(C, "bloodpressurerate");
                    
                    if (desiredBloodPressure > AffData.Strength)
                    {
                        bloodPressureLerp /= 3;
                    }
                    
                    // Move to desired amount
                    AffData.Strength = Math.Clamp(Double.Lerp(AffData.Strength, desiredBloodPressure, bloodPressureLerp), 5, 200);

                    // Effects:
                    // Confusion
                    if (AffData.Strength < 60)
                    {
                        if (C.GetAffData("unconsciousness").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "lightheadedness", 2);
                            NTC.SetSymptomTrue(C, "headache", 2);
                        }

                        // Blurred Vision
                        if (AffData.Strength < 55)
                        {
                            if (C.GetAffData("unconsciousness").Strength <= 0)
                            {
                                NTC.SetSymptomTrue(C, "blurredvision", 2);
                            }

                            // Pale Skin
                            if (AffData.Strength < 50)
                            {
                                NTC.SetSymptomTrue(C, "paleskin", 2);

                                // Confusion
                                if (AffData.Strength < 30)
                                {
                                    if (C.GetAffData("unconsciousness").Strength <= 0)
                                    {
                                        NTC.SetSymptomTrue(C, "confusion", 2);
                                    }
                                }
                            }
                        }
                    }

                    // Heart Attack + Stroke
                    if (AffData.Strength > 150)
                    {
                        if (C.GetAffData("afstreptokinase").Strength <= 0 && C.GetAffData("heartremoved").Strength <= 0 && HF.Chance((float)(NTConfig.Get("NT_heartattackChance", 1f) * ((AffData.Strength - 150) / 50 * 0.02f))))
                        {
                            HF.AddAffliction(C.Human, "heartattack", 50, null);
                        }

                        if (HF.Chance((float)(NTConfig.Get("NT_strokeChance", 1) * ((AffData.Strength - 150) / 50 * 0.02 + Math.Clamp(C.GetAffData("afstreptokinase").Strength, 0, 1) * 0.05))))
                        {
                            HF.AddAffliction(C.Human, "stroke", 5, null);
                        }
                    }
                };

            // Hypoxemia
            // Constant, too complicated otherwise
            // Type: Blood Affliction
            // Caused By: Oxygen Low, Blood Loss.
            // Effects: Changes Blood Pressure, Specific Organ Damage (requires them to be Constant).
            BloodAfflictionsToAdd["hypoxemia"] = new("hypoxemia");
            BloodAfflictionsToAdd["hypoxemia"].Const = true;
            BloodAfflictionsToAdd["hypoxemia"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    C.SetDoubleStatStrength("availableoxygen", Math.Min(C.GetDoubleStatStrength("availableoxygen"), 100 - C.GetNonLimbAffData("pneumothorax").Strength / 2));

                    double HypoxemiaGain = NTC.GetMultiplier(C, "hypoxemiagain");
                    double RegularHypoxemiaChange = (-C.GetDoubleStatStrength("availableoxygen") + 50) / 8;

                    if (RegularHypoxemiaChange > 0)
                    {
                        RegularHypoxemiaChange *= HypoxemiaGain;
                    }
                    else
                    {
                        RegularHypoxemiaChange = Double.Lerp(RegularHypoxemiaChange * 2, 0, Math.Clamp((50 - C.GetDoubleStatStrength("bloodamount")) / 50, 0, 1));
                    }
                        
                    // Passively Increase / Decrease
                    AffData.Strength = Math.Clamp(
                        AffData.Strength + (
                            -Math.Min(0, (C.GetBloodAffData("bloodpressure").Strength - 70) / 7) * HypoxemiaGain
                            - Math.Min(0, (C.GetDoubleStatStrength("bloodamount") - 60) / 4) * HypoxemiaGain
                            + RegularHypoxemiaChange
                        ) * NT.DeltaTime,
                        0, 100
                    );

                    // Effects:
                    // Neurotrauma
                    double NeurotraumaGain = AffData.Strength / 100 * NT.DeltaTime
                        * NTC.GetMultiplier(C, "neurotraumagain")
                        * NTConfig.Get("NT_neurotraumaGain", 1)
                        * (1 - Math.Clamp(C.GetAffData("afmannitol").Strength, 0, 0.5));

                    HF.AddAffliction(C.Human, "neurotrauma", (float)NeurotraumaGain, null);

                    // Bone Damage
                    HF.AddAffliction(C.Human, "bonedamage", (float)(AffData.Strength / 1000 * NTC.GetMultiplier(C, "bonedamagegain") * NT.DeltaTime), null);

                    // Shortness of Breath
                    if (AffData.Strength > 20)
                    {
                        if (C.GetAffData("respiratoryarrest").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                        }

                        // Headache
                        if (AffData.Strength > 40)
                        {
                            if (C.GetAffData("unconsciousness").Strength <= 0)
                            {
                                NTC.SetSymptomTrue(C, "headache", 2);
                            }

                            // Confusion
                            if (AffData.Strength > 50)
                            {
                                if (C.GetAffData("unconsciousness").Strength <= 0)
                                {
                                    NTC.SetSymptomTrue(C, "confusion", 2);
                                }

                                // Respiratory Arrest
                                if (AffData.Strength > 70 && HF.Chance(0.05f))
                                {
                                    HF.AddAffliction(C.Human, "respiratoryarrest", 200, null);
                                }

                                // Unconsciousness & Cardiac Arrest
                                if (AffData.Strength > 80)
                                {
                                    NTC.SetSymptomTrue(C, "unconsciousness", 2);

                                    if (HF.Chance(0.01f))
                                    {
                                        HF.AddAffliction(C.Human, "cardiacarrest", 200, null);
                                    }
                                }
                            }
                        }
                    }
                };

            // Alkalosis
            // Not constant; gets applied by other sources.
            // Type: Blood
            // Caused By: Hyperventilation, Vomiting, Blood Pack items.
            // Effects: Seizures, Palpitations.
            BloodAfflictionsToAdd["alkalosis"] = new("alkalosis");
            BloodAfflictionsToAdd["alkalosis"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    // Passive Increase / Decrease
                    AffData.Strength += (-NT.DeltaTime * 0.03);

                    // Alkalosis Interaction
                    if (C.GetBloodAffData("acidosis").Strength > 1 && AffData.Strength > 1)
                    {
                        double min = Math.Min(C.GetBloodAffData("acidosis").Strength, AffData.Strength);
                        C.GetBloodAffData("acidosis").Strength -= min;
                        AffData.Strength -= min;
                    }

                    // Effects:
                    // Palpitations
                    if (AffData.Strength > 20)
                    {
                        if (C.GetAffData("cardiacarrest").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "palpitations", 2);
                        }

                        // Seizures
                        if (AffData.Strength > 60 && HF.Chance(0.05f))
                        {
                            HF.AddAffliction(C.Human, "seizure", 10, null);
                        }
                    }
                };

            // Acidosis
            // Not constant; gets applied by other sources.
            // Type: Blood
            // Caused By: Hypoventilation, Respiratory Arrest, Cardiac Arrest, Kidney Damage, Saline, Blood Pack items.
            // Effects: Seizures, Coma, Increased Heartrate, Fibrillation, Headache, Confusion, Weakness.
            BloodAfflictionsToAdd["acidosis"] = new("acidosis");
            BloodAfflictionsToAdd["acidosis"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    AffData.Strength += HF.BoolToNum(
                            C.GetSymptomAffData("hypoventilation").Strength > 0
                            && C.GetAffData("artificialventilation").Strength <= 0.1
                        ) * 0.09 * NT.DeltaTime
                        + Math.Max(0, C.GetAffData("kidneydamage").Strength - 80) / 20 * 0.1 * NT.DeltaTime
                        - NT.DeltaTime * 0.03;

                    // Effects:
                    

                    // Fibrillation (in IncreasedHeartrate constant)
                    // Increased Heartrate (in IncreasedHeartrate constant)

                    // Confusion
                    if (AffData.Strength > 15)
                    {
                        bool IsConscious = C.GetAffData("unconsciousness").Strength <= 0;

                        if (IsConscious)
                        {
                            NTC.SetSymptomTrue(C, "confusion", 2);

                            // Headache
                            if (AffData.Strength > 20)
                            {
                                NTC.SetSymptomTrue(C, "headache", 2);
                            }
                        }

                        // Weakness
                        if (AffData.Strength > 35)
                        {
                            NTC.SetSymptomTrue(C, "weakness", 2);

                            if (AffData.Strength > 60)
                            {
                                // Coma
                                if (HF.Chance(0.05f + (float)(AffData.Strength - 60f) / 100f))
                                {
                                    HF.AddAffliction(C.Human, "coma", 14, null);
                                }

                                // Seizures
                                if (HF.Chance(0.05f))
                                {
                                    HF.AddAffliction(C.Human, "seizure", 10, null);
                                }
                            }
                        }
                    }
                };

            // Hemotransfusion Shock
            // Not constant; gets applied by other sources.
            // Type: Blood
            // Caused By: Wrong Blood Type, Bozo.
            // Effects: Vomiting, Chest Pain, Blood Loss (XML), Vanilla Organ Damage (XML), Liver Damage (XML), Heart Damage (XML), Kidney Damage (XML), Lung Damage (XML), Shortness of Breath, Abdominal Pain, Wheezing.
            BloodAfflictionsToAdd["hemotransfusionshock"] = new("hemotransfusionshock", 0, 100, 0, AfflictionPriority.HIGH);
            BloodAfflictionsToAdd["hemotransfusionshock"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
                {
                    // Effects:
                    // Wheezing
                    if (AffData.Strength < 90)
                    {
                        bool IsConscious = C.GetAffData("unconsciousness").Strength <= 0;
                        bool IsSedated = C.GetBoolStatStrength("sedated");

                        if (C.GetAffData("respiratoryarrest").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "wheezing", 2);
                        }

                        // Abdominal Pain
                        if (AffData.Strength < 80)
                        {
                            if (IsConscious && !IsSedated)
                            {
                                NTC.SetSymptomTrue(C, "abdominalpain", 2);
                            }

                            // Shortness of Breath
                            if (AffData.Strength < 70)
                            {
                                if (C.GetAffData("respiratoryarrest").Strength <= 0)
                                {
                                    NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                                }

                                // Chest Pain
                                if (AffData.Strength < 60)
                                {
                                    if (IsConscious && !IsSedated)
                                    {
                                        NTC.SetSymptomTrue(C, "chestpain", 2);
                                    }

                                    // Vomiting
                                    if (AffData.Strength < 40)
                                    {
                                        NTC.SetSymptomTrue(C, "vomiting", 2);
                                    }
                                }
                            }
                        }
                    }
                };

            // Sepsis
            // Not constant; gets applied by other sources.
            // Type: Blood
            // Caused By: Blood Pack Items, Gangrene, Foreign Bodies, Infected Wounds, Azathioprine failed skillcheck.
            // Effects: Organ Damage (via Stats), Gangrene, Fever, Hyperventilation, Increased Heartrate (Constant), Neurotrauma, Confusion, Bone Damage.
            BloodAfflictionsToAdd["sepsis"] = new("sepsis");
            BloodAfflictionsToAdd["sepsis"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
                {
                    // Does not progress in Stasis
                    if (C.GetBoolStatStrength("stasis")) return;

                    // Passive Increase
                    if (AffData.Strength > 0.1)
                    {
                        AffData.Strength += 0.05 * NT.DeltaTime;
                    }

                    // Effects:
                    // Neurotrauma
                    double NeurotraumaGain = AffData.Strength / 100 * 0.4 * NT.DeltaTime
                        * NTC.GetMultiplier(C, "neurotraumagain")
                        * NTConfig.Get("NT_neurotraumaGain", 1)
                        * (1 - Math.Clamp(C.GetAffData("afmannitol").Strength, 0, 0.5));

                    HF.AddAffliction(C.Human, "neurotrauma", (float)NeurotraumaGain, null);

                    // Bone Damage
                    HF.AddAffliction(C.Human, "bonedamage", (float)(AffData.Strength / 500 * NTC.GetMultiplier(C, "bonedamagegain") * NT.DeltaTime), null);

                    // Fever
                    if (AffData.Strength > 5)
                    {
                        NTC.SetSymptomTrue(C, "fever", 2);

                        // Gangrene
                        if (HF.Chance(0.04f))
                        {
                            foreach (LimbType AllLimbs in HF.LimbsToCheck)
                            {
                                if (HF.LimbIsExtremity(Limb))
                                {
                                    HF.AddAfflictionLimb(C.Human, "gangrene", Limb, (float)((0.5 + AffData.Strength / 150) * NTConfig.Get("NT_gangrenespeed", 1) * NT.DeltaTime), null);
                                }
                            }
                        }

                        // Confusion
                        if (AffData.Strength > 40 && C.GetAffData("unconsciousness").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "confusion", 2);
                        }
                    }
                };

            // Immunity
            // Constant; else too complicated.
            // Type: Blood, Mechanic
            // Caused By: Existing.
            // Effects: Increased regeneration for Burns and Wounds.
            BloodAfflictionsToAdd["immunity"] = new("immunity", 0, 100, 100, AfflictionPriority.HIGH);
            BloodAfflictionsToAdd["immunity"].Const = true;
            BloodAfflictionsToAdd["immunity"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
                {
                    if (AffData.Strength == -1)
                    {
                        if (NTBloodTypes.HasBloodType(C.Human))
                        {
                            AffData.Strength = 100;
                        }
                        else
                        {
                            AffData.Strength = 100;
                            NTBloodTypes.TryRandomizeBlood(C.Human);
                        }
                    }

                    if (C.GetBoolStatStrength("stasis")) return;

                    AffData.Strength = Math.Clamp(
                        AffData.Strength + (0.5 + AffData.Strength / 100) * NT.DeltaTime,
                        5, 100
                    );
                };

            foreach (KeyValuePair<string, NTBloodAffliction> Pair in BloodAfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddSymptoms()
        {
            // Cough
            // Type: Symptom, Mental
            // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
            SymptomsToAdd["cough"] = new("cough", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["cough"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Pale Skin
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["paleskin"] = new("paleskin", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["paleskin"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Lightheadedness
            // Type: Symptom, Mental
            // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
            SymptomsToAdd["lightheadedness"] = new("lightheadedness", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["lightheadedness"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Blurred Vision
            // Type: Symptom, Mental
            // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
            SymptomsToAdd["blurredvision"] = new("blurredvision", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["blurredvision"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Confusion
            // Type: Symptom, Mental
            // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
            SymptomsToAdd["confusion"] = new("confusion", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["confusion"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Headache
            // Type: Symptom, Mental, Pain
            // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious. Removed if under Painkillers.
            SymptomsToAdd["headache"] = new("headache", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["headache"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Leg Swelling
            // Type: Symptom, Organic
            // Removes itself when conditions are NOT met. Applied by other afflictions. Not present on Cybernetics.
            SymptomsToAdd["legswelling"] = new("legswelling", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["legswelling"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Weakness
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["weakness"] = new("weakness", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["weakness"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Wheezing
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["wheezing"] = new("wheezing", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["wheezing"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Vomiting
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            // Effects: Alkalosis
            SymptomsToAdd["vomiting"] = new("vomiting", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["vomiting"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Vomiting Blood
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["vomitingblood"] = new("vomitingblood", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["vomitingblood"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Fever
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["fever"] = new("fever", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["fever"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Abdominal Discomfort
            // Type: Symptom, Mental
            // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
            SymptomsToAdd["abdominaldiscomfort"] = new("abdominaldiscomfort", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["abdominaldiscomfort"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Bloating
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["bloating"] = new("bloating", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["bloating"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Jaundice
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["jaundice"] = new("jaundice", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["jaundice"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Sweating
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["sweating"] = new("sweating", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["sweating"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Palpitations
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["palpitations"] = new("palpitations", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["palpitations"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Unconsciousness
            // Type: Symptom, Talent Interaction
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["unconsciousness"] = new("unconsciousness", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["unconsciousness"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    // Effects:
                    // Give In
                    if (AffData.Strength > 0)
                    {
                        HF.AddAffliction(C.Human, "givein", 1, null);
                    }
                };

            // Craving
            // Type: Symptom, Mental
            // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
            SymptomsToAdd["craving"] = new("craving", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["craving"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Nausea
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            // Effects: Alkalosis
            SymptomsToAdd["nausea"] = new("nausea", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["nausea"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    // Effects:
                    // Alkalosis
                    if (AffData.Strength > 0)
                    {
                        HF.AddAffliction(C.Human, "alkalosis", (float)(Math.Clamp(AffData.Strength, 0, 1) * 0.1 * NT.DeltaTime), null);
                    }
                };

            // Chest Pain
            // Type: Symptom, Mental
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["chestpain"] = new("chestpain", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["chestpain"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Abdominal Pain
            // Type: Symptom, Mental, Pain
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["abdominalpain"] = new("abdominalpain", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["abdominalpain"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Intense Pain
            // Type: Symptom, Mental, Pain
            SymptomsToAdd["intensepain"] = new("intensepain", 0, 100, 0);
            SymptomsToAdd["intensepain"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Shortness of Breath
            // Type: Symptom
            // Removes itself when conditions are NOT met. Applied by other afflictions.
            SymptomsToAdd["shortnessofbreath"] = new("shortnessofbreath", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["shortnessofbreath"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                };

            // Force Prone
            // Constant; too complicated otherwise.
            // Type: Functionality
            // Effects: Changes the animations of a character to be unable to walk.
            SymptomsToAdd["forceprone"] = new("forceprone", 0, 2, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["forceprone"].Const = true;
            SymptomsToAdd["forceprone"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    // Application Conditions
                    AffData.Strength = HF.BoolToNum(
                        (!NTC.HasSymptomFalse(C, "forceprone"))
                        && C.GetSymptomAffData("unconsciousness").Strength <= 0
                        && (!C.Human.IsClimbing)
                        && (
                            NTC.HasSymptom(C, "forceprone")
                            || (C.GetBoolStatStrength("lockleftleg") && C.GetBoolStatStrength("lockrightleg") && (!C.GetBoolStatStrength("wheelchaired")))
                        ),
                        2
                    );
                    if (AffData.Strength > 0)
                    {
                        AffData.HumanUpdateTime = 2;
                    }
                };

            // Hyperventilation
            // Not constant; gets applied by other sources, removes itself however.
            // Type: Non-Limb Specific
            // Caused By: Hypotension, Hypoxemia, Pneumothorax, Sepsis, Adrenaline
            // Effects: Alkalosis.
            SymptomsToAdd["hyperventilation"] = new("hyperventilation", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["hyperventilation"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    // Effects:
                    // Alkalosis
                    HF.AddAffliction(C.Human, "alkalosis", (float)(Math.Clamp(AffData.Strength, 0, 1) * 0.09 * NT.DeltaTime), null);
                };

            // Hypoventilation
            // Not constant; gets applied by other sources, removes itself however.
            // Type: Non-Limb Specific
            // Caused By: Opiate Overdose, Opiods, Anesthesia
            // Effects: Acidosis.
            SymptomsToAdd["hypoventilation"] = new("hypoventilation", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["hypoventilation"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    // Counteracting with Hyperventilation
                    if (C.GetSymptomAffData("hyperventilation").Strength > 0 && AffData.Strength > 0)
                    {
                        C.GetSymptomAffData("hyperventilation").Strength = 0;
                        AffData.Strength = 0;
                    }

                    // Effects:
                    // Acidosis
                    if (AffData.Strength > 0 && C.GetAffData("artificialventilation").Strength <= 0.1)
                    {
                        HF.AddAffliction(C.Human, "acidosis", (float)(0.09 * NT.DeltaTime), null);
                    }
                };

            // Ponder later
            SymptomsToAdd["lockleftarm"] = new("lockleftarm", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["lockrightarm"] = new("lockrightarm", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["lockleftleg"] = new("lockleftleg", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["lockrightleg"] = new("lockrightleg", 0, 100, 0, AfflictionPriority.HIGH);

            SymptomsToAdd["triggersym_respiratoryarrest"] = new("triggersym_respiratoryarrest", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["triggersym_respiratoryarrest"].Real = false;
            SymptomsToAdd["triggersym_respiratoryarrest"].Const = true;
            SymptomsToAdd["triggersym_respiratoryarrest"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    if (AffData.Strength <= 0) return;
                    HF.SetAffliction(C.Human, "respiratoryarrest", 100);
                };
            SymptomsToAdd["triggersym_seizure"] = new("triggersym_seizure", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["triggersym_seizure"].Real = false;
            SymptomsToAdd["triggersym_seizure"].Const = true;
            SymptomsToAdd["triggersym_seizure"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    if (AffData.Strength <= 0) return;
                    HF.SetAffliction(C.Human, "seizure", 100);
                };
            SymptomsToAdd["triggersym_stroke"] = new("triggersym_stroke", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["triggersym_stroke"].Real = false;
            SymptomsToAdd["triggersym_stroke"].Const = true;
            SymptomsToAdd["triggersym_stroke"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    if (AffData.Strength <= 0) return;
                    HF.SetAffliction(C.Human, "stroke", 100);
                };
            SymptomsToAdd["triggersym_coma"] = new("triggersym_coma", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["triggersym_coma"].Real = false;
            SymptomsToAdd["triggersym_coma"].Const = true;
            SymptomsToAdd["triggersym_coma"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    if (AffData.Strength <= 0) return;
                    HF.SetAffliction(C.Human, "coma", 100);
                };
            SymptomsToAdd["triggersym_cardiacarrest"] = new("triggersym_cardiacarrest", 0, 100, 0, AfflictionPriority.HIGH);
            SymptomsToAdd["triggersym_cardiacarrest"].Real = false;
            SymptomsToAdd["triggersym_cardiacarrest"].Const = true;
            SymptomsToAdd["triggersym_cardiacarrest"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData AffData) =>
                {
                    if (AffData.Strength <= 0) return;
                    HF.SetAffliction(C.Human, "cardiacarrest", 100);
                };

            foreach (KeyValuePair<string, NTSymptom> Pair in SymptomsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddLimbSymptoms()
        {
            // Inflammation
            // Type: Limb-specific
            // Caused by: Foreign Bodies, Infected Wounds
            // Effects: Fever
            LimbSymptomsToAdd["inflammation"] = new("inflammation", 0, 100, 0, AfflictionPriority.HIGH);
            LimbSymptomsToAdd["inflammation"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbSymptomData AffData) =>
                {
                    // Passive Decrease
                    AffData.Strength[Limb] -= 0.1 * NT.DeltaTime;
                };

            // Spasms
            // Type: Symptom
            // Caused By: Seizure
            // Effects: Makes character twitch on the ground via XML.
            LimbSymptomsToAdd["spasm"] = new("spasm", 0, 100, 0, AfflictionPriority.HIGH);
            LimbSymptomsToAdd["spasm"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbSymptomData AffData) =>
                {
                    // Passive Decrease
                    AffData.Strength[Limb] -= 100f;
                };

            foreach (KeyValuePair<string, NTLimbSymptom> Pair in LimbSymptomsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }
    }
}