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

    public class NTUpdateFunctionInfos
    {
        // We can edit NTUpdateFunctionInfos to change what affliction update functions have access to
        public float DeltaTime { get; }

        public Character Character { get; }


        public NTUpdateFunctionInfos(Character character)
        {
            this.Character = character;
        }
    }

    public class NTAfflictionInfos
    {
        public AfflictionPriority Priority { get; }
        // They return an int because i can't return void. so do whatever you want with it 
        public Action<NTUpdateFunctionInfos> UpdateFunction { get; } 

        public NTAfflictionInfos(Action<NTUpdateFunctionInfos> updateFunction, AfflictionPriority priority = AfflictionPriority.HIGH)
        {
            this.Priority = priority;
            this.UpdateFunction = updateFunction;
        }

    }

    // Contains the list of every affliction defined by Neurotrauma. Addons should add their afflictions there.
    // We should provide functions to Lua to add Afflictions here. 

    //TODO ADD A WAY TO IMPLEMENT LIMB SPECIFIC AFFLICTIONS, do we really need a special way ?
    public static class NTAfflictions
    {

        public static Dictionary<string, NTAfflictionInfos> Afflictions { get; } = new Dictionary<string, NTAfflictionInfos>();

        private static List<string> AfflictionsLOW = [];
        private static List<string> AfflictionsMEDIUM = [];
        private static List<string> AfflictionsHIGH = [];

        public static void RegisterAffliciton(string id, NTAfflictionInfos affliction)
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
                LuaCsLogger.LogError($"Affliction with id {id} already exists! Multiple addons might be trying to register the same affliciton.\n" +
                    $"If you're trying to override an affliction update function, use OverrideAffliciton instead.");
            }
            
        }

        // If an addon needs to change how an affliction works. Could create some incompatibility if multiple addons override the same shit but that's on them, not my problem
        public static void OverrideAffliction(string id, NTAfflictionInfos affliction)
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
        public static Dictionary<string, NTAfflictionInfos> GetAfflictionsByPriority(AfflictionPriority priority)
        {
            Dictionary<string, NTAfflictionInfos> output = [];

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


    }

    public abstract class NTAffliction // Added to NTHuman updatingAfflictions
    {
        public double Strength { get; set; } = 0;
        public double MinStrength { get; set; }
        public double MaxStrength { get; set; }

        public string Identifier { get; set; }
        public List<string> DependentAfflictions { get; set; }
        public AfflictionPriority Priority { get; set; }

        public NTAffliction(double NewMinStrength, double NewMaxStrength,
                                        string NewIdentifier, List<string> NewDependentAfflictions, AfflictionPriority NewPriority = AfflictionPriority.HIGH)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            Identifier = NewIdentifier;
            DependentAfflictions = NewDependentAfflictions;
            Priority = NewPriority;
        }
    }

    public class NTNonLimbAffliction : NTAffliction
    {
        public NTNonLimbAffliction(double NewMinStrength, double NewMaxStrength,
                                        string NewIdentifier, List<string> NewDependentAfflictions, AfflictionPriority NewPriority = AfflictionPriority.HIGH) : 
                                        base(NewMinStrength, NewMaxStrength, NewIdentifier, NewDependentAfflictions, NewPriority)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            Identifier = NewIdentifier;
            DependentAfflictions = NewDependentAfflictions ;
            Priority = NewPriority;
        }
    }

    public class NTLimbAffliction : NTAffliction
    {
        public NTLimbAffliction(double NewMinStrength, double NewMaxStrength,
                                        string NewIdentifier, List<string> NewDependentAfflictions, AfflictionPriority NewPriority = AfflictionPriority.HIGH) :
                                        base(NewMinStrength, NewMaxStrength, NewIdentifier, NewDependentAfflictions, NewPriority)
        {
        }

        public List<LimbType> AllowedLimbs { get; set; } = HF.LimbsToCheck; // I'll add this one later.
    }

    public class  NTAfflictionsToAdd
    {

        // Human Updates update functons have 
        // Param 1: NTHuman (The character we updating) [C]
        // Param 2: String (The affliction Identifier) [I]
        // Param 3: LimbType (The limb the aff is on) [L]
        // Param 4: Type???? Idk I forgot what this one is.
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> AfflictionsToAdd =
                                new Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>>();

        public void Initialize() // Initalize the afflictions.
        {
            AddAfflictions();
        }

        private void AddAfflictions() // Create your afflictions in here.
        {
            AfflictionsToAdd["example_aff"] = (C, ID, Limb) => 
            {

                NTAffliction Self = C.LocalAfflictions.RegisterGetAffliction(ID,0,100, null); // Add the affliction to the character with it's params. Essentially a condensed version of what you would normally do.
                C.GetStats(); // Now we can get the players stats!!!!!1
                Self.Strength += 5; // Or increase the strength!!

            };

            AfflictionsToAdd["example_aff2"] = (C, ID, Limb) =>
            {

                NTAffliction Self = C.LocalAfflictions.RegisterGetAffliction(ID, 0, 10, ["example_aff"]); // We now tell the Update that we must register "example_aff" too!
                NTAffliction ExampleAff = C.LocalAfflictions.RegisterGetAffliction("example_aff", 0, 10, null); // Lets get our ExampleAff!!
                C.GetStats(); // Now we can get the players stats!!!!!1
                Self.Strength += C.LocalAfflictions.GetAff("example_aff").Strength + 10;
                ExampleAff.Strength += Self.Strength;
            };
        }
    }
}


// wtf am i doing with my life