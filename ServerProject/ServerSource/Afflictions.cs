namespace Neurotrauma
{
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

    // The priority defines at wich frequency the affliction gets updated. 
    public enum AfflictionPriority: int
    {
        LOW = 8*60,  // Every 8s
        MEDIUM = 4*60, // Every 4s
        HIGH = 2*60 // Every 2s
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
    }
}

// wtf am i doing with my life