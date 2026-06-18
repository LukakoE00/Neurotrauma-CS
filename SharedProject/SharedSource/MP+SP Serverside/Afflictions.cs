using MoonSharp.Interpreter;
using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma
{

    // The priority defines at wich frequency the affliction gets updated. 
    public enum AfflictionPriority : int
    {
        LOW = 8 * 60,  // Every 8s
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

    /// <summary>
    /// The abstract template of our Afflictions. This class and any of it's descendants are never instantiated for a player class. Rather, we use the outline of this affliction class
    /// determine the results of the affliction. The strength is stored seperately in the NTHuman character class!
    /// </summary>
    public abstract class NTAffliction // Added to NTHuman updatingAfflictions
    {
        /// <summary>
        /// Should this affliction always be running? If on, is always added to the updating afflictions list. Off by default.
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
        /// The priority of our affliction, higher intervals meaner more updates.
        /// </summary>
        public AfflictionPriority Priority { get; set; }
        /// <summary>
        /// The ID of the affliction.
        /// </summary>
        public string ID = "";
        /// <summary>
        /// The main update function of our affliction.
        /// </summary>
        public Action<HumanUpdate.NTHuman,string,LimbType> UpdateAction = 
            (HumanUpdate.NTHuman C, string ID, LimbType Limb) => 
            { 
                // Insert your Affliction Update in here.
            };
        public NTAffliction(double NewMinStrength, double NewMaxStrength,
                                        AfflictionPriority NewPriority = AfflictionPriority.HIGH)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            Priority = NewPriority;
        }
    }

    public class NTNonLimbAffliction : NTAffliction
    {
        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanNonLimbAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanNonLimbAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };
        
        public NTNonLimbAffliction(double NewMinStrength, double NewMaxStrength, AfflictionPriority NewPriority = AfflictionPriority.HIGH) : 
                                        base(NewMinStrength, NewMaxStrength, NewPriority)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            Priority = NewPriority;
        }
    }

    public class NTLimbAffliction : NTAffliction
    {
        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanLimbAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanLimbAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTLimbAffliction(double NewMinStrength, double NewMaxStrength, AfflictionPriority NewPriority = AfflictionPriority.HIGH) :
                                        base(NewMinStrength, NewMaxStrength, NewPriority)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            Priority = NewPriority;
        }

        public List<LimbType> AllowedLimbs { get; set; } = HF.LimbsToCheck; // I'll add this one later.
    }

    public class NTBloodAffliction : NTAffliction
    {
        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanBloodAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanBloodAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTBloodAffliction(double NewMinStrength, double NewMaxStrength, AfflictionPriority NewPriority = AfflictionPriority.HIGH) :
                                        base(NewMinStrength, NewMaxStrength, NewPriority)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            Priority = NewPriority;
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
    }


    public class  NTAfflictionsToAdd : AfflictionsPackage
    {

        // Human Updates update functons have 
        // Param 1: NTHuman (The character we updating) [C]
        // Param 2: String (The affliction Identifier) [I]
        // Param 3: LimbType (The limb the aff is on) [L]
        // Param 4: Type???? Idk I forgot what this one is.

        Dictionary<string, NTNonLimbAffliction> AfflictionsToAdd =
                                new Dictionary<string, NTNonLimbAffliction>();
        Dictionary<string, NTLimbAffliction> LimbAfflictionsToAdd =
                                new Dictionary<string, NTLimbAffliction>();
        Dictionary<string, NTBloodAffliction> BloodAfflictionsToAdd =
                                new Dictionary<string, NTBloodAffliction>();

        public void Initialize() // Initalize the afflictions.
        {
            AddAfflictions();
            AddLimbAfflictions();
            AddBloodAfflictions();
        }

        private void AddAfflictions() // Create your afflictions in here.
        {
            AfflictionsToAdd["Example1"] = new(0, 100); // Create the new affliction.
            AfflictionsToAdd["Example1"].ID = "Example1"; // Set the ID.
            AfflictionsToAdd["Example1"].UpdateAction = // Set the update function.
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanNonLimbAffData AffData) =>
            {
                AffData.Strength = 100; // Access the affliction strength like so.
                C.GetAffData()["Example2"].Strength = 0; // Access other afflictions like this.
            };

            AfflictionsToAdd["Example2"] = new(0, 100); // This affliction now has "Example1" affliction as a dependency.
            AfflictionsToAdd["Example2"].ID = "Example2";
            AfflictionsToAdd["Example2"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanNonLimbAffData AffData) =>
            {
                // Same idea here.
            };

            foreach (KeyValuePair<string,NTNonLimbAffliction> Pair in AfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddLimbAfflictions()
        {
            LimbAfflictionsToAdd["Example1Limb"] = new(0, 100);
            LimbAfflictionsToAdd["Example1Limb"].ID = "Example1Limb";
            LimbAfflictionsToAdd["Example1Limb"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanLimbAffData AffData) =>
            {
                // Bit different here but same idea.
                AffData.Strength[Limb] = 100; // Since this is a limb affliction we gotta access the limb specific strength.
            };

            foreach (KeyValuePair<string, NTLimbAffliction> Pair in LimbAfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddBloodAfflictions()
        {
            BloodAfflictionsToAdd["Example1Blood"] = new(0, 100);
            BloodAfflictionsToAdd["Example1Blood"].ID = "Example1Blood";
            BloodAfflictionsToAdd["Example1Blood"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanBloodAffData AffData) =>
            {
                // Same as non limb.
            };

            foreach (KeyValuePair<string, NTBloodAffliction> Pair in BloodAfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }
    }
}


// wtf am i doing with my life