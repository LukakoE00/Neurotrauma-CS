namespace Neurotrauma;

public partial class NTStats
{
    static NTStatLoader loader = NeurotraumaInit.NTStatsLoader;

    public static void DefineAllStats()
    {

        List<NTStat> l = new List<NTStat>();

        l.Add(new NTStatDouble("healingrate", 0, 100, 1, (C, dt) =>
        {
            return NTC.GetMultiplier(C.Human, "healingrate");
        }));

        l.Add(new NTStatDouble("specificOrganDamageHealMultiplier", 0, 100, 1, (C, dt) =>
        {
            return NTC.GetMultiplier(C.Human, "anyspecificorgandamage") + Math.Clamp(C.GetAfflictionStrength("afthiamine"), 0, 1) * 4;
        }));

        l.Add(new NTStatDouble("neworgandamage", 0, 100, 1, (C,dt) =>
        {
            return (
                C.GetAfflictionStrength("sepsis") / 300
                + C.GetAfflictionStrength("hypoxemia") / 400
                + Math.Max(C.GetAfflictionStrength("radiationsickness") - 25, 0) / 400
               )
                * NTC.GetMultiplier(C.Human, "anyorgandamage")
                * NTConfig.Get("NT_OrganDamageGain", 1)
                * dt;
        }));

        l.Add(new NTStatDouble("clottingrate", 0, 100, 1, (C, dt) =>
        {
            return Math.Clamp(1 - C.GetAfflictionStrength("liverdamage") / 100, 0, 1)
                    * C.GetDoubleStat("healingrate")
                    * Math.Clamp(1 - C.GetAfflictionStrength("afstreptokinase"), 0, 1)
                    * NTC.GetMultiplier(C.Human, "clottingrate");
        }));

        l.Add(new NTStatDouble("bloodamount", 0, 100, 1, (C, dt) =>
        {
            return Math.Clamp(100 - C.GetAfflictionStrength("bloodloss"), 0, 100);
        }));

        l.Add(new NTStatBool("stasis", false, (C, dt) =>
        {
            return C.GetAfflictionStrength("stasis") > 0;
        }));

        l.Add(new NTStatBool("sedated", false, (C, dt) =>
        {
            return C.GetAfflictionStrength("analgesia") > 0
                    || C.GetAfflictionStrength("anesthesia") > 10
                    || C.GetAfflictionStrength("drunk") > 20
                    || C.GetAfflictionStrength("stasis") > 0;
        }));

        l.Add(new NTStatDouble("withdrawal", 0, 100, 1, (C, dt) =>
        {
            return Math.Max(Math.Max(C.GetAfflictionStrength("opiatewithdrawal"), C.GetAfflictionStrength("chemwithdrawal")), C.GetAfflictionStrength("alcoholwithdrawal"));
        }));

        l.Add(new NTStatDouble("availableoxygen", 0, 100, 1, (C, dt) =>
        {
            double Res = Math.Clamp(C.Human.Oxygen, 0, 100);
            // heart isnt pumping blood? no new oxygen is getting into the bloodstream, no matter how oxygen rich the air in the lungs
            Res *= (1 - C.GetAfflictionStrength("fibrillation") / 100);
            // and uuuh, maybe also dont let people without lungs or broken lungs use the oxygen where their lungs should be
            if (C.GetAfflictionStrength("cardiacarrest") > 1 || C.GetAfflictionStrength("lungdamage") == 100 || C.GetAfflictionStrength("lungremoved") > 0.1) Res = 0;
            return Res;
        }));

        l.Add(new NTStatDouble("speedmultiplier", 0, 100, 1, (C, dt) =>
        {
            double Res = 1;
            if (C.GetAfflictionStrength("spinalcordinjury") > 0) Res = -9001; // Wow, I find this to be a bit overkill    // It indeed might be overkill -Cookie
            if (C.GetAfflictionStrength("vomiting") > 0) Res *= .8;
            if (C.GetAfflictionStrength("nausea") > 0) Res *= .9;
            if (C.GetAfflictionStrength("anesthesia") > 0) Res *= .5;
            if (C.GetAfflictionStrength("opiateoverdose") > 50) Res *= .5;

            if (C.GetDoubleStat("withdrawal") > 80)
            {
                Res *= .5;
            }
            else if (C.GetDoubleStat("withdrawal") > 40)
            {
                Res *= .7;
            }
            else if (C.GetDoubleStat("withdrawal") > 20)
            {
                Res *= .9;
            }

            if (C.GetAfflictionStrength("drunk") > 80)
            {
                Res *= .5;
            }
            else if (C.GetAfflictionStrength("drunk") > 40)
            {
                Res *= .7;
            }
            else if (C.GetAfflictionStrength("drunk") > 20)
            {
                Res *= .8;
            }

            Res += C.GetAfflictionStrength("afadrenaline") / 100;

            Res *= NTC.GetSpeed(C.Human);

            return Res;
        }));

        l.Add(new NTStatDouble("slowdown", 0, 100, 0, (C, dt) =>
        {
            return Math.Clamp(100 * (1 - C.GetDoubleStat("speedmultiplier")), 0, 100);
        }));

        l.Add(new NTStatBool("lockleftarm", false, (C, dt) =>
        {
            return HF.LimbLockedInitial(C, LimbType.LeftArm, "lockleftarm");
        }));

        l.Add(new NTStatBool("lockrightarm", false, (C, dt) =>
        {
            return HF.LimbLockedInitial(C, LimbType.RightArm, "lockrightarm");
        }));

        l.Add(new NTStatBool("lockleftleg", false, (C, dt) =>
        {
            return HF.LimbLockedInitial(C, LimbType.LeftLeg, "lockleftleg");
        }));

        l.Add(new NTStatBool("lockrightleg", false, (C, dt) =>
        {
            return HF.LimbLockedInitial(C, LimbType.RightLeg, "lockrightleg");
        }));

        l.Add(new NTStatBool("forceprone", false, (C, dt) =>
        {
            return C.HasAffliction("forceprone");
        }));

        l.Add(new NTStatBool("wheelchired", false, (C, dt) =>
        {
            Item OutWearItem = HF.GetItemInOuterWear(C.Human);
            bool Res = (OutWearItem != null && OutWearItem.Prefab.Identifier.Value == "wheelchair") ? true : false;

            if (Res)
            {
                C.SetBoolStat("lockleftleg", C.GetBoolStat("lockleftarm"));
                C.SetBoolStat("lockrightleg", C.GetBoolStat("lockrightarm"));
            }

            if (C.GetBoolStat("lockleftleg") || C.GetBoolStat("lockrightleg"))
            {
                if (C.GetAfflictionStrength("afadrenaline") < 0.1 || Res)
                {
                    C.SetDoubleStat("speedmultiplier", C.GetDoubleStat("speedmultiplier") * .5);
                }
            }

            bool IsProne = C.GetBoolStat("lockleftleg") && C.GetBoolStat("lockrightleg");

            if (IsProne && C.Human.IsClimbing)
            {
                C.SetDoubleStat("speedmultiplier", C.GetDoubleStat("speedmultiplier") * .5);
            }

            if ((IsProne || Res) && C.GetBoolStat("lockleftarm") && C.GetBoolStat("lockrightarm"))
            {
                C.SetDoubleStat("speedmultiplier", -9001);
            }
            else if (IsProne && (C.GetBoolStat("lockleftarm") || C.GetBoolStat("lockrightarm")))
            {
                C.SetDoubleStat("speedmultiplier", C.GetDoubleStat("speedmultiplier") * .8);
            }

            return Res;
        }));

        l.Add(new NTStatDouble("bonegrowthCount", 0, 100, 0, (C, dt) =>
        {
            double count = 0;
            foreach (LimbType Limb in HF.LimbsToCheck)
            {
                if (C.GetAfflictionStrengthLimb("bonegrowth", Limb) > 0) count++;
            }
            return count;
        }));

        l.Add(new NTStatDouble("burndamage", 0, 100, 0, (C, dt) =>
        {
            double total = 0;
            foreach (LimbType Limb in HF.LimbsToCheck)
            {
                total += C.GetAfflictionStrengthLimb("burn", Limb, 0);
            }
            return total;
        }));

        loader.Registers(l);
    }
}