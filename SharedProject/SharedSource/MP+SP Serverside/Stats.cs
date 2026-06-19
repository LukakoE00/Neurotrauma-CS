namespace Neurotrauma
{
    public static class NTStats
    {
        public static Dictionary<string, NTStat> Stats = new Dictionary<string, NTStat>();

        public static void DefineAllStats()
        {
            // This isnt done, just a basic template.
            Stats["healingrate"] = new NTStatDouble("healingrate",0,100,1, (C) =>
            {
                return 1;
            });
            Stats["neworgandamage"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["clottingrate"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["bloodamount"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return Math.Clamp(100 - C.GetAffData()["bloodloss"].Strength,0,100);
            });
            Stats["stasis"] = new NTStatBool("stasis",false, (C) => 
            {
                return true;
            });
            Stats["sedated"] = new NTStatBool("sedated",false, (C) => 
            {
                return true;
            });
            Stats["withdrawal"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["availableoxygen"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["speedmultiplier"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["lockleftarm"] = new NTStatBool("lockleftarm",false, (C) => 
            {
                return true;
            });
            Stats["lockrightarm"] = new NTStatBool("lockrightarm",false, (C) => 
            {
                return true;
            });
            Stats["lockleftleg"] = new NTStatBool("lockleftleg",false, (C) => 
            {
                return true;
            });
            Stats["lockrightleg"] = new NTStatBool("lockrightleg",false, (C) => 
            {
                return true;
            });
            Stats["wheelchaired"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["bonegrowthCount"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["burndamage"] = new NTStatDouble("healingrate", 0, 100, 1, (C) => 
            {
                return 1;
            });
        }

        public static void RegisterStat(string id, NTStat NewStat) // Register a new stat to the NTStat Dictionary.
        {
            if (!Stats.ContainsKey(id))
            {
                Stats.Add(id, NewStat);
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} already exists! Multiple addons might be trying to register the same stat.\n" +
                    $"If you want to recalculate a stat, use CharacterStats.RecalculateSingle instead of registering it again.");
            }
        }

        public static void OverrideStat(string id, NTStat NewStat) // Override a stat in NTStat Dictionary.
        {
            if (Stats.ContainsKey(id))
            {
                Stats[id] = NewStat;
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} does not exist! You can't override a stat that doesn't exist.\n" +
                    $"If you want to register a new stat, use RegisterStat instead of trying to override it.");
            }
        }

        public static void RemoveStat(string id) // Remove a stat to the NTStat Dictionary.
        {
            if (!Stats.ContainsKey(id))
            {
                Stats.Remove(id);
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} does not exist! You can't remove a stat that doesn't exist.");
            }
        }

    }

    public abstract class NTStat(string Name)
    {
        public void Get()
        {

        }
    }

    public class NTStatDouble(string Name, double MinStrength = 0, double MaxStrength = 1, double DefaultStrength = 1, Func<HumanUpdate.NTHuman, double> Update = null) : NTStat(Name)
    {
        private double MinStrength { get; set; } = MinStrength;
        private double MaxStrength { get; set; } = MaxStrength;
        private double DefaultStrength { get; set; } = DefaultStrength;
        private bool Settable { get; set; } = false;
        public string ID = Name;

        public void Add(HumanUpdate.NTHuman C, double AddStrength)
        {
            if (Settable)
            {
                C.LocalStats.DoubleStats[ID].Strength = Math.Clamp(C.LocalStats.DoubleStats[ID].Strength + AddStrength,MinStrength,MaxStrength);
            }
        }

        public double Get(HumanUpdate.NTHuman C)
        {
            return (Update != null) ? Update.Invoke(C) : C.LocalStats.DoubleStats[ID].Strength; // C# my beloved.
        }

        public void Set(HumanUpdate.NTHuman C, double NewStrength)
        {
            if (Settable)
            {
                C.LocalStats.DoubleStats[ID].Strength = Math.Clamp(NewStrength,MinStrength,MaxStrength);
            }
        }

    }

    public class NTStatBool(string Name, bool Strength = false, Func<HumanUpdate.NTHuman, bool> Update = null) : NTStat(Name)
    {
        private bool Settable { get; set; } = false;
        public string ID = Name;

        public bool Get(HumanUpdate.NTHuman C)
        {
            return (Update != null) ? Update.Invoke(C) : Strength; // C# my beloved.
        }

        public void Set(HumanUpdate.NTHuman C, bool NewStrength)
        {
            if (Settable)
            {
                C.LocalStats.BoolStats[ID].Strength = NewStrength;
            }
        }
    }
}