namespace Neurotrauma
{

    public partial class NeurotraumaInit
    {
        // Server-specific code

        private void DefineAllStats()
        {
            // I wanted to write every stats already defined in LuaNT however NTC looks like complete horsedong.
            // I will then leave it for someone to write them, but here is an example of how to register a stat:
            NTStat.RegisterStat("stasis", (character) =>
            {
                return HF.HasAffliction(character.Character, "stasis") ? 1 : 0;
            });
        }

        private void DefineAllAfflictions()
        {

        }

        public void InitializeServer()
        {
            LuaCsLogger.Log("Running InitializeServer");
            DefineAllStats();
            DefineAllAfflictions();
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
            harmony.Patch(originalUse, prefix: new HarmonyMethod(typeof(ItemMethods), nameof(ItemMethods.Override_Use)));
            harmony.Patch(originalApplyTreatment, prefix: new HarmonyMethod(typeof(ItemMethods), nameof(ItemMethods.Override_ApplyTreatment)));
        }

        public void DisposeServer()
        {
            harmony.UnpatchSelf();
        }
    }
}
