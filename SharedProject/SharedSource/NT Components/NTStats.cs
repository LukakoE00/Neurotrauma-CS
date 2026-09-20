using static Neurotrauma.NTItems;

namespace Neurotrauma;

public partial class NTStats
{

    public class NTStat
    {
        public string Name { get; private set; }

        public NTStat(string name) 
        {
            this.Name = name;
        }
    }


    public class NTStatFloat : NTStat
    {
        public float MinStrength { get; private set; }
        public float MaxStrength { get; private set; }
        public float DefaultStrength { get; private set; }
        public string ID;

        public Func<NTHuman, float, float>? UpdateFunction { get; private set; }

        public NTStatFloat(string Name, float MinStrength, float MaxStrength, float DefaultStrength, Func<NTHuman, float, float>? Update) : base(Name)
        {
            this.MinStrength = MinStrength;
            this.MaxStrength = MaxStrength;
            this.DefaultStrength = DefaultStrength;
            this.UpdateFunction = Update;
            this.ID = Name;
        }

        public float Get(NTHuman C, float deltaTime, float defaultStrength = 0)
        {
            return (UpdateFunction != null) ? UpdateFunction.Invoke(C, deltaTime) : defaultStrength;
        }

    }

    public class NTStatBool : NTStat
    {

        public bool DefaultValue { get; private set; }
        public Func<NTHuman, float, bool>? UpdateFunction { get; private set; }

        public NTStatBool(string Name, bool DefaultValue, Func<NTHuman, float, bool>? Update) : base(Name)
        {
            this.DefaultValue = DefaultValue;
            this.UpdateFunction = Update;
        }

        public bool Get(NTHuman C, float deltaTime, bool defaultValue = false)
        {
            return (UpdateFunction != null) ? UpdateFunction.Invoke(C, deltaTime) : defaultValue;
        }

    }


    public static Dictionary<string, NTStat> StatRegistry = new Dictionary<string, NTStat>();

    public class NTStatLoader
    {
        public string ModID { get; private set; }

        /// <summary>
        /// Create a new instance of the NTStatLoader class for a specific mod. This allows you to register new Stats and interact with the StatsRegistry.
        /// </summary>
        /// <param name="ModID">The name of your mod, helps with debugging and organization</param>
        public NTStatLoader(string ModID)
        {
            this.ModID = ModID;
        }

        /// <summary>
        /// Adds a new Stat to the StatRegistry. If a Stat with the same ID already exists, it will not be added and an error will be printed to the console.
        /// </summary>
        /// <returns>true if the stat was registered successfully, false otherwise (the id already has a Stat assigned to it).</returns>
        /// <example>
        /// <code>
        /// NTStatLoader loader = new NTStats.NTStatLoader("MyMod");
        /// loader.Register(new NTStatDouble("MyDoubleStat", 0, 100, 50));
        /// loader.Register(new NTStatBool("MyBoolStat", false));
        /// </code>
        /// </example>
        public bool Register(NTStat Stat)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("debug_mode", true))
            {
                HF.PrintUtility($"[{this.ModID}] Registering stat: {Stat.Name}");
            }

            if (StatRegistry.ContainsKey(Stat.Name))
            {
                HF.PrintError($"[{this.ModID}] Stat with ID '{Stat.Name}' already is registered.");
                return false;
            }

            StatRegistry.Add(Stat.Name, Stat);
            return true;
        }

        public bool Registers(List<NTStat> Stats)
        {
            bool r = true;

            foreach (var stat in Stats)
            {
                if (!Register(stat)) r = false;

            }

            return r;
        }

        /// <summary>
        /// Overrides the Stat matching the given Stat Name. If RegisterInstead is set to true, it will register the given function if the given item has no function to override.
        /// </summary>
        /// <returns>true if the function was overridden or registered successfully, false otherwise.</returns>
        public bool Override(string StatName, NTStat Stat, bool RegisterInstead = false)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("debug_mode", true))
            {
                HF.PrintUtility($"[{this.ModID}] Overriding stat: {StatName}");
            }

            if (!StatRegistry.ContainsKey(StatName))
            {
                if (RegisterInstead)
                {
                    HF.PrintWarning($"[{this.ModID}] Stat with ID '{StatName}' is not registered. Will Register instead.");
                    return Register(Stat);
                }

                HF.PrintError($"[{this.ModID}] Stat with ID '{StatName}' is not registered.");
                return false;

            }

            StatRegistry[StatName] = Stat;
            return true;
        }

        /// <summary>
        /// Removes the Stat associated with the given Stat Name.
        /// </summary>
        /// <returns>true if there was a function to remove, false otherwise.</returns>
        public bool Remove(string StatName)
        {
            // TODO: set debug mode to false when going public to avoid spamming console like retards
            if (NTConfig.Get("debug_mode", true))
            {
                HF.PrintUtility($"[{this.ModID}] Removing stat: {StatName}");
            }


            if (!StatRegistry.ContainsKey(StatName))
            {
                HF.PrintError($"[{this.ModID}] Stat with ID '{StatName}' is not registered.");
                return false;
            }

            StatRegistry.Remove(StatName);
            return true;
        }

        /// <summary>
        /// Check if the given stat name is present in the Stat Registry.
        /// </summary>
        /// <param name="StatName">The Name of the Stat you want to check.</param>
        /// <returns>true if the stat is registered, false otherwise.</returns>
        public bool Exists(string StatName)
        {
            return StatRegistry.ContainsKey(StatName);
        }


        /// <summary>
        /// Returns the function associated with the given item ID, or null if no function is registered for that item.
        /// </summary>
        /// <param name="StatName">The Name of the Stat you want to retrieve.</param>
        /// <returns>The function associated with the stat name, or null if not found.</returns>
        public NTStat? Get(string StatName)
        {
            if (StatRegistry.ContainsKey(StatName))
            {
                return StatRegistry[StatName];
            }
            return null;
        }
    }
}