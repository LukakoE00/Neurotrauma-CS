namespace Neurotrauma
{

    public partial class NeurotraumaInit
    {
        // Server-specific code

        private void DefineAllAfflictions()
        {

        }

        private void DefineAllItems()
        {
            // Azathioprine
            NTItemMethods.RegisterItemUseFunction("immunosuppressant", infos =>
            {
                bool success = HF.GetSkillRequirmentMet(infos.user, "Medical", 10);
                HF.AddAffliction(infos.target, "afimmunosuppressant", success ? 5 : 3, infos.user);
            });

            // Antibiotic Ointment
            NTItemMethods.RegisterItemUseFunction("ointment", (infos) =>
            {

                bool success = HF.GetSkillRequirmentMet(infos.user, "medical", 10);

                HF.AddAfflictionLimb(infos.target, "ointmented", infos.targetLimb.type, success ? 120 : 60, infos.user);
                HF.AddAfflictionLimb(infos.target, "infectedwound", infos.targetLimb.type, success ? -72 : -24, infos.user);

                // Check for third degree burn might not be working correctly
                if (HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "burn", 0) < 50)
                {
                    HF.AddAfflictionLimb(infos.target, "burn", infos.targetLimb.type, success ? -12 : (float) -7.2, infos.user);
                }

                HF.GiveItem(infos.target, "ntsfx_ointment");

            });

            // Scalpel
            NTItemMethods.RegisterItemUseFunction("advscalpel", infos =>
            {
                // Not in stasis
                if (HF.HasAffliction(infos.target, "stasis", (float) 0.1)) { return; }

                if (!HF.CanPerformSurgeryOn(infos.target) || HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 1)) { return; }

                bool success = HF.GetSurgerySkillRequirmentMet(infos.user, 30);

                if (success)
                {
                    HF.AddAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 1 + HF.GetSkillLevel(infos.user, "medical")/2, infos.user); // TODO change this to using surgery instead of medical

                    HF.SetAfflictionLimb(infos.target, "suturedi", infos.targetLimb.type, 0, infos.user, 0);
                    HF.SetAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, 0, infos.user, 0);
                    HF.SetAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, 0, infos.user, 0);

                } else
                {
                    HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                    HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 10, infos.user);
                }

                HF.GiveItem(infos.target, "ntsfx_slash");
            });
        }

        public void InitializeServer()
        {
            LuaCsLogger.Log("Running InitializeServer");
            DefineAllAfflictions();
            DefineAllItems();
        }

        public void OnLoadCompletedServer()
        {
            LuaCsLogger.Log("Running OnLoadCompletedServers");
            harmony = new Harmony("neurotrauma.server");

            var originalApplyDamage = AccessTools.Method(typeof(CharacterHealth), "ApplyDamage", [typeof(Limb), typeof(AttackResult), typeof(bool), typeof(bool)]);
            var originalDamageLimb = AccessTools.Method(typeof(Character), "DamageLimb", [typeof(Vector2), typeof(Limb), typeof(IEnumerable<Affliction>), typeof(float), typeof(bool), typeof(Vector2),
                typeof(Character), typeof(float), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(bool)]);
            var originalUse = AccessTools.Method(typeof(Item), "Use", [typeof(float), typeof(Character), typeof(Limb), typeof(Entity), typeof(Character)]);
            var originalApplyTreatment = AccessTools.Method(typeof(Item), "ApplyTreatment", [typeof(Character), typeof(Character), typeof(Limb)]);

            harmony.Patch(originalApplyDamage, prefix: new HarmonyMethod(typeof(OnDamaged), nameof(OnDamaged.Override_ApplyDamage)));
            harmony.Patch(originalDamageLimb, prefix: new HarmonyMethod(typeof(OnDamaged), nameof(OnDamaged.Override_DamageLimb)));
            harmony.Patch(originalUse, prefix: new HarmonyMethod(typeof(NTItemMethods), nameof(NTItemMethods.Override_Use)));
            harmony.Patch(originalApplyTreatment, prefix: new HarmonyMethod(typeof(NTItemMethods), nameof(NTItemMethods.Override_ApplyTreatment)));
        }

        public void DisposeServer()
        {
            harmony.UnpatchSelf();
        }
    }
}
