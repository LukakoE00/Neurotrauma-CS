using static System.Math;

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
                return NTC.GetMultiplier(C,"healingrate");
            });
            Stats["anyspecificorgandamage"] = new NTStatDouble("anyspecificorgandamage", 0, 100, 1, (C) =>
            {
                return NTC.GetMultiplier(C, "anyspecificorgandamage") 
                        + Clamp(C.GetBloodAffStrength("afthiamine"),0,1) * 4;
            });
            Stats["neworgandamage"] = new NTStatDouble("neworgandamage", 0, 100, 1, (C) => 
            {
                return (
                    C.GetBloodAffStrength("sepsis") / 300
                    + C.GetBloodAffStrength("hypoxemia") / 400
                    + Max(C.GetAffStrength("radiationsickness") - 25, 0) / 400
                   )
                    * NTC.GetMultiplier(C,"anyorgandamage")
                    * NTConfig.Get("NT_OrganDamageGain",1)
                    * NTAfflictions.DeltaTime;
            });
            Stats["clottingrate"] = new NTStatDouble("clottingrate", 0, 100, 1, (C) => 
            {
                return Clamp(1 - C.GetAffStrength("liverdamage") / 100, 0, 1)
                        * C.GetDoubleStatStrength("healingrate")
                        * Clamp(1 - C.GetAffStrength("afstreptokinase"), 0, 1)
                        * NTC.GetMultiplier(C, "clottingrate");
            });
            Stats["bloodamount"] = new NTStatDouble("bloodamount", 0, 100, 1, (C) => 
            {
                return Math.Clamp(100 - C.GetAffStrength("bloodloss"),0,100);
            });
            Stats["stasis"] = new NTStatBool("stasis",false, (C) => 
            {
                return C.GetAffStrength("stasis") > 0;
            });
            Stats["sedated"] = new NTStatBool("sedated",false, (C) => 
            {
                return C.GetAffStrength("analgesia") > 0
                        || C.GetAffStrength("anesthesia") > 10
                        || C.GetAffStrength("drunk") > 20
                        || C.GetAffStrength("stasis") > 0;
            });
            Stats["withdrawal"] = new NTStatDouble("withdrawal", 0, 100, 1, (C) => 
            {
                return Max(Max(C.GetAffStrength("opiatewithdrawal"), C.GetAffStrength("chemwithdrawal")), C.GetAffStrength("alcoholwithdrawal"));
            });
            Stats["availableoxygen"] = new NTStatDouble("availableoxygen", 0, 100, 1, (C) => 
            {
                double Res = Clamp(C.Human.Oxygen,0,100);
                // heart isnt pumping blood? no new oxygen is getting into the bloodstream, no matter how oxygen rich the air in the lungs
                Res *= (C.GetAffStrength("fibrillation") / 100);
                // and uuuh, maybe also dont let people without lungs or broken lungs use the oxygen where their lungs should be
                if (C.GetAffStrength("cardiacarrest") > 1 || C.GetAffStrength("lungdamage") == 100 || C.GetAffStrength("lungremoved") > 0.1) Res = 0;
                return Res;
            });
            Stats["speedmultiplier"] = new NTStatDouble("speedmultiplier", 0, 100, 1, (C) => 
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
            Stats["wheelchaired"] = new NTStatDouble("wheelchaired", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["bonegrowthCount"] = new NTStatDouble("bonegrowthCount", 0, 100, 1, (C) => 
            {
                return 1;
            });
            Stats["burndamage"] = new NTStatDouble("burndamage", 0, 100, 1, (C) => 
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