namespace Neurotrauma
{
    // Server-side AND Singleplayer code ONLY!
    public partial class NeurotraumaInit
    {
        // Server-specific code

        private void DefineAllAfflictions()
        {

        }

        public void InitializeServer()
        {
            DefineAllAfflictions();
            NTItemMethods.DefineAllItems();
        }

        public void OnLoadCompletedServerside()
        {
            HF.Print("Running OnLoadCompletedServerside");
            NTInfo.PrintNTInitInfo(); // Prints the current Neurotrauma information in the console.
            NTBloodTypes.InitializeBloodHooks(); // Initializes LuaHooks needed for Blood.cs
            InitLuaHooks(); // Initializes the Lua hooks at the bottom of this file


            // What a mess. - Lukako
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

            // Character Patches
            var characterCreation = AccessTools.Method(typeof(Character), "Create",
                [typeof(CharacterPrefab), typeof(Vector2), typeof(string), typeof(CharacterInfo), typeof(ushort), typeof(bool), typeof(bool), typeof(bool), typeof(RagdollParams), typeof(bool)]);
            harmony.Patch(characterCreation, postfix: new HarmonyMethod(typeof(HumanUpdate), nameof(HumanUpdate.AddCharacterToUpdate))); // The Character Created hook.

            var characterDeath = AccessTools.Method(typeof(Character), "RecordKill",
                [typeof(Character)]);
            harmony.Patch(characterDeath, prefix: new HarmonyMethod(typeof(HumanUpdate), nameof(HumanUpdate.RemoveCharacterFromUpdate))); // The Character died hook.
        }

        public void DisposeServer()
        {
            harmony.UnpatchSelf();
        }

        public static void InitLuaHooks() // Based off the Traumatic Presence mod by Lenny!
        {
            HF.Print("Adding Lua Hooks");
            // This function stores our hooks we will be using for NT 2.
#pragma warning disable CS0618 // Type or member is obsolete

            LuaCsSetup.Instance.Hook.Add("think", "NTCS.ThinkUpdate", (params object[] _) => // The Hook details (TODO, make this in C#)
            { // Start of our Function

                HU.ThinkUpdate();
                return null;
            }); // End of our Function

#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
