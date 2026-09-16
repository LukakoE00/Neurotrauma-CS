namespace Neurotrauma;

public partial class NTStats
{
    static NTStatLoader loader = NeurotraumaInit.NTStatsLoader;

    public static void DefineAllStats()
    {

        List<NTStat> l = new List<NTStat>();

        l.Add(new NTStatFloat("healingrate", 0, 100, 1, (C, dt) =>
        {
            return NTC.GetMultiplier(C.Human, "healingrate");
        }));

        l.Add(new NTStatFloat("specificOrganDamageHealMultiplier", 0, 100, 1, (C, dt) =>
        {
            return NTC.GetMultiplier(C.Human, "anyspecificorgandamage") + Math.Clamp(C.GetAfflictionStrength("afthiamine"), 0, 1) * 4;
        }));

        l.Add(new NTStatFloat("neworgandamage", 0, 100, 1, (C,dt) =>
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

        l.Add(new NTStatFloat("clottingrate", 0, 100, 1, (C, dt) =>
        {
            return Math.Clamp(1 - C.GetAfflictionStrength("liverdamage") / 100, 0, 1)
                    * C.GetDoubleStat("healingrate")
                    * Math.Clamp(1 - C.GetAfflictionStrength("afstreptokinase"), 0, 1)
                    * NTC.GetMultiplier(C.Human, "clottingrate");
        }));

        l.Add(new NTStatFloat("bloodamount", 0, 100, 1, (C, dt) =>
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

        l.Add(new NTStatFloat("withdrawal", 0, 100, 1, (C, dt) =>
        {
            return Math.Max(Math.Max(C.GetAfflictionStrength("opiatewithdrawal"), C.GetAfflictionStrength("chemwithdrawal")), C.GetAfflictionStrength("alcoholwithdrawal"));
        }));

        l.Add(new NTStatFloat("availableoxygen", 0, 100, 1, (C, dt) =>
        {
            float Res = Math.Clamp(C.Human.Oxygen, 0, 100);
            // heart isnt pumping blood? no new oxygen is getting into the bloodstream, no matter how oxygen rich the air in the lungs
            Res *= (1 - C.GetAfflictionStrength("fibrillation") / 100);
            // and uuuh, maybe also dont let people without lungs or broken lungs use the oxygen where their lungs should be
            if (C.GetAfflictionStrength("cardiacarrest") > 1 || C.GetAfflictionStrength("lungdamage") == 100 || C.GetAfflictionStrength("lungremoved") > 0.1) Res = 0;
            return Res;
        }));

        l.Add(new NTStatFloat("speedmultiplier", 0, 100, 1, (C, dt) =>
        {
            float Res = 1;
            if (C.GetAfflictionStrength("spinalcordinjury") > 0) Res = -9001f; // Wow, I find this to be a bit overkill    // It indeed might be overkill -Cookie
            if (C.GetAfflictionStrength("vomiting") > 0) Res *= .8f;
            if (C.GetAfflictionStrength("nausea") > 0) Res *= .9f;
            if (C.GetAfflictionStrength("anesthesia") > 0) Res *= .5f;
            if (C.GetAfflictionStrength("opiateoverdose") > 50) Res *= .5f;

            if (C.GetDoubleStat("withdrawal") > 80)
            {
                Res *= .5f;
            }
            else if (C.GetDoubleStat("withdrawal") > 40)
            {
                Res *= .7f;
            }
            else if (C.GetDoubleStat("withdrawal") > 20)
            {
                Res *= .9f;
            }

            if (C.GetAfflictionStrength("drunk") > 80)
            {
                Res *= .5f;
            }
            else if (C.GetAfflictionStrength("drunk") > 40)
            {
                Res *= .7f;
            }
            else if (C.GetAfflictionStrength("drunk") > 20)
            {
                Res *= .8f;
            }

            Res += C.GetAfflictionStrength("afadrenaline") / 100;

            Res *= (float) NTC.GetSpeed(C.Human);

            return Res;
        }));

        l.Add(new NTStatFloat("slowdown", 0, 100, 0, (C, dt) =>
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

        l.Add(new NTStatFloat("bonegrowthCount", 0, 100, 0, (C, dt) =>
        {
            float count = 0;
            foreach (LimbType Limb in HF.LimbsToCheck)
            {
                if (C.GetAfflictionStrengthLimb("bonegrowth", Limb) > 0) count++;
            }
            return count;
        }));

        l.Add(new NTStatFloat("burndamage", 0, 100, 0, (C, dt) =>
        {
            float total = 0;
            foreach (LimbType Limb in HF.LimbsToCheck)
            {
                total += C.GetAfflictionStrengthLimb("burn", Limb, 0);
            }
            return total;
        }));

        loader.Registers(l);
    }
}