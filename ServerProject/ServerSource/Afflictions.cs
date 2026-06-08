namespace Neurotrauma
{
    public class NTUpdateFunctionInfos
    {
        // We can edit NTUpdateFunctionInfos to change what affliction update functions have access to
        public float DeltaTime { get; }

        public Character Character { get; }

        public CharacterStats Stats { get; }

        public NTUpdateFunctionInfos(Character character, CharacterStats stats)
        {
            this.Character = character;
            this.Stats = stats;
        }
    }

    // The priority defines at wich frequency the affliction gets updated. 
    public enum AfflictionPriority
    {
        LOW,  // Every 8s
        MEDIUM, // Every 4s
        HIGH // Every 2s
    }

    public class NTAfflictionInfos
    {
        private AfflictionPriority Priority { get; }
        // They return an int because i can't return void. so do whatever you want with it 
        private Func<NTUpdateFunctionInfos, int> UpdateFunction { get; } 

        public NTAfflictionInfos(AfflictionPriority priority, Func<NTUpdateFunctionInfos, int> updateFunction)
        {
            this.Priority = priority;
            this.UpdateFunction = updateFunction;
        }

        public AfflictionPriority GetPriority() { return this.Priority; }
        public Func<NTUpdateFunctionInfos, int> GetUpdateFunction() { return this.UpdateFunction; }

    }




    // Contains the list of every affliction defined by Neurotrauma. Addons should add their afflictions there.
    // We should provide functions to Lua to add Afflictions here. 

    //TODO ADD A WAY TO IMPLEMENT LIMB SPECIFIC AFFLICTIONS, do we really need a special way ?
    public static class NTAfflictions
    {

        public static Dictionary<string, NTAfflictionInfos> Afflictions { get; } = new Dictionary<string, NTAfflictionInfos>();

        public static void RegisterAffliciton(string id, NTAfflictionInfos affliction)
        {
            if (!Afflictions.ContainsKey(id))
            {
                Afflictions.Add(id, affliction);
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
    }
}

// wtf am i doing with my life