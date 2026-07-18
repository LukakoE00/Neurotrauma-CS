using static System.Math;
using static Neurotrauma.HF;

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

            Stats["specificOrganDamageHealMultiplier"] = new NTStatDouble("specificOrganDamageHealMultiplier", 0, 100, 1, (C) =>
            {
                return NTC.GetMultiplier(C, "anyspecificorgandamage") + Clamp(C.GetAffStrength("afthiamine"), 0, 1) * 4;
            });

            Stats["neworgandamage"] = new NTStatDouble("neworgandamage", 0, 100, 1, (C) => 
            {
                return (
                    C.GetBloodAffStrength("sepsis") / 300
                    + C.GetBloodAffStrength("hypoxemia") / 400
                    + Max(C.GetNonLimbAffStrength("radiationsickness") - 25, 0) / 400
                   )
                    * NTC.GetMultiplier(C,"anyorgandamage")
                    * NTConfig.Get("NT_OrganDamageGain",1)
                    * NT.DeltaTime;
            });

            Stats["clottingrate"] = new NTStatDouble("clottingrate", 0, 100, 1, (C) => 
            {
                return Clamp(1 - C.GetNonLimbAffStrength("liverdamage") / 100, 0, 1)
                        * C.GetDoubleStatStrength("healingrate")
                        * Clamp(1 - C.GetNonLimbAffStrength("afstreptokinase"), 0, 1)
                        * NTC.GetMultiplier(C, "clottingrate");
            });

            Stats["bloodamount"] = new NTStatDouble("bloodamount", 0, 100, 1, (C) => 
            {
                return Math.Clamp(100 - C.GetBloodAffStrength("bloodloss"),0,100);
            });

            Stats["stasis"] = new NTStatBool("stasis",false, (C) => 
            {
                return C.GetNonLimbAffStrength("stasis") > 0;
            });

            Stats["sedated"] = new NTStatBool("sedated",false, (C) => 
            {
                return C.GetNonLimbAffStrength("analgesia") > 0
                        || C.GetNonLimbAffStrength("anesthesia") > 10
                        || C.GetNonLimbAffStrength("drunk") > 20
                        || C.GetNonLimbAffStrength("stasis") > 0;
            });

            Stats["withdrawal"] = new NTStatDouble("withdrawal", 0, 100, 1, (C) => 
            {
                return Max(Max(C.GetNonLimbAffStrength("opiatewithdrawal"), C.GetNonLimbAffStrength("chemwithdrawal")), C.GetNonLimbAffStrength("alcoholwithdrawal"));
            });

            Stats["availableoxygen"] = new NTStatDouble("availableoxygen", 0, 100, 1, (C) => 
            {
                double Res = Clamp(C.Human.Oxygen,0,100);
                // heart isnt pumping blood? no new oxygen is getting into the bloodstream, no matter how oxygen rich the air in the lungs
                Res *= (1 - C.GetNonLimbAffStrength("fibrillation") / 100);
                // and uuuh, maybe also dont let people without lungs or broken lungs use the oxygen where their lungs should be
                if (C.GetNonLimbAffStrength("cardiacarrest") > 1 || C.GetNonLimbAffStrength("lungdamage") == 100 || C.GetNonLimbAffStrength("lungremoved") > 0.1) Res = 0;
                return Res;
            });

            Stats["speedmultiplier"] = new NTStatDouble("speedmultiplier", 0, 100, 1, (C) => 
            {
                double Res = 1;
                if (C.GetNonLimbAffStrength("spinalcordinjury") > 0) Res = -9001; // Wow, I find this to be a bit overkill
                if (C.GetSymptomStrength("vomiting") > 0) Res *= .8;
                if (C.GetSymptomStrength("nausea") > 0) Res *= .9;
                if (C.GetNonLimbAffStrength("anesthesia") > 0) Res *= .5;
                if (C.GetNonLimbAffStrength("opiateoverdose") > 50) Res *= .5;

                if (C.GetDoubleStatStrength("withdrawal") > 80)
                {
                    Res *= .5;
                }
                else if (C.GetDoubleStatStrength("withdrawal") > 40)
                {
                    Res *= .7;
                }
                else if (C.GetDoubleStatStrength("withdrawal") > 20)
                {
                    Res *= .9;
                }

                if (C.GetNonLimbAffStrength("drunk") > 80)
                {
                    Res *= .5;
                }
                else if (C.GetNonLimbAffStrength("drunk") > 40)
                {
                    Res *= .7;
                }
                else if (C.GetNonLimbAffStrength("drunk") > 20)
                {
                    Res *= .8;
                }

                Res += C.GetNonLimbAffStrength("afadrenaline") / 100;

                Res *= NTC.GetSpeed(C);

                return Res;
            });

            Stats["slowdown"] = new NTStatDouble("slowdown", 0, 100, 0, (C) =>
            {
                return Math.Clamp(100 * (1 - C.GetDoubleStatStrength("speedmultiplier")), 0, 100);
            });

            Stats["lockleftarm"] = new NTStatBool("lockleftarm",false, (C) => 
            {
                return LimbLockedInitial(C,LimbType.LeftArm,"lockleftarm");
            });

            Stats["lockrightarm"] = new NTStatBool("lockrightarm",false, (C) => 
            {
                return LimbLockedInitial(C, LimbType.RightArm, "lockrightarm");
            });

            Stats["lockleftleg"] = new NTStatBool("lockleftleg",false, (C) => 
            {
                return LimbLockedInitial(C, LimbType.LeftLeg, "lockleftleg");
            });

            Stats["lockrightleg"] = new NTStatBool("lockrightleg",false, (C) => 
            {
                return LimbLockedInitial(C, LimbType.RightLeg, "lockrightleg");
            });

            Stats["forceprone"] = new NTStatBool("forceprone", false, (C) =>
            {
                return NTC.HasSymptom(C.Human,"forceprone");
            });

            Stats["wheelchaired"] = new NTStatBool("wheelchaired", false, (C) => 
            {
                Item OutWearItem = GetItemInOuterWear(C.Human);
                bool Res = (OutWearItem != null && OutWearItem.Prefab.Identifier.Value == "wheelchair") ? true : false;

                if (Res)
                {
                    C.SetBoolStatStrength("lockleftleg", C.GetBoolStatStrength("lockleftarm"));
                    C.SetBoolStatStrength("lockrightleg", C.GetBoolStatStrength("lockrightarm"));
                }

                if (C.GetBoolStatStrength("lockleftleg") || C.GetBoolStatStrength("lockrightleg")) 
                {
                    if (C.GetNonLimbAffStrength("afadrenaline") < 0.1 || Res)
                    {
                        C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * .5);
                    }
                }

                bool IsProne = C.GetBoolStatStrength("lockleftleg") && C.GetBoolStatStrength("lockrightleg");

                if (IsProne && C.Human.IsClimbing)
                {
                    C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * .5);
                }

                if ((IsProne || Res) && C.GetBoolStatStrength("lockleftarm") && C.GetBoolStatStrength("lockrightarm"))
                {
                    C.SetDoubleStatStrength("speedmultiplier", -9001);
                }
                else if (IsProne && (C.GetBoolStatStrength("lockleftarm") || C.GetBoolStatStrength("lockrightarm")))
                {
                    C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * .8);
                }

                return Res;
            });

            Stats["bonegrowthCount"] = new NTStatDouble("bonegrowthCount", 0, 100, 0, (C) =>
            {
                double count = 0;
                foreach (LimbType Limb in HF.LimbsToCheck)
                {
                    if (GetAfflictionStrengthLimb(C.Human, Limb, "bonegrowth", 0) > 0) count++;
                }
                return count;
            });

            Stats["burndamage"] = new NTStatDouble("burndamage", 0, 100, 0, (C) =>
            {
                double total = 0;
                foreach (LimbType Limb in HF.LimbsToCheck)
                {
                    total += GetAfflictionStrengthLimb(C.Human, Limb, "burn", 0);
                }
                return total;
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

    public abstract class NTStat()
    {
        public void Get()
        {

        }

        public void Set()
        {

        }
    }

    public class NTStatDouble(string Name, double MinStrength = 0, double MaxStrength = 1, double DefaultStrength = 1, Func<HumanUpdate.NTHuman, double> Update = null) : NTStat()
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

    public class NTStatBool(string Name, bool Strength = false, Func<HumanUpdate.NTHuman, bool> Update = null) : NTStat()
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