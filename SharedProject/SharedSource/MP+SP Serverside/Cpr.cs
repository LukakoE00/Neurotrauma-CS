// Disable the Hook warning
#pragma warning disable CS0618

namespace Neurotrauma
{
    public static class CPRHooks
    {
        public static void InitCPRHooks()
        {
            LuaCsSetup.Instance.Hook.Add("character.CPRSuccess", "NT.CPRSuccess", (params object[] args) =>
            {
                if (args.Length < 1) return null;

                var animController = args[0] as AnimController;
                if (animController?.Character?.SelectedCharacter == null) return null;

                Character character = animController.Character.SelectedCharacter;

                var debugAff = HF.GetAffliction(character, "cpr_buff_auto");

                if (!HF.HasAffliction(character, "cpr_buff_auto")) HF.AddAffliction(character, "cpr_buff", 2f, null);

                // Prevent fractures during CPR
                HF.AddAffliction(character, "cpr_fracturebuff", 2f, null);

                return null;
            });

            LuaCsSetup.Instance.Hook.Add("character.CPRFailed", "NT.CPRFailed", (params object[] args) =>
            {
                if (args.Length < 1) return null;

                var animController = args[0] as AnimController;
                if (animController?.Character?.SelectedCharacter == null) return null;

                Character character = animController.Character.SelectedCharacter;

                // Prevent fractures during CPR
                HF.AddAffliction(character, "cpr_fracturebuff", 2f, null);
                HF.AddAfflictionLimb(character, "blunttrauma", LimbType.Torso, 0.3f, null);

                float fractureChance =
                    NTConfig.Get("NT_fractureChance", 1f) *
                    NTConfig.Get("NT_CPRFractureChance", 1f) *
                    0.2f /
                    HF.GetSkillLevel(animController.Character, "medical");

                if (HF.Chance(fractureChance)) HF.AddAffliction(character, "t_fracture", 1f, null);

                return null;
            });
        }
    }
}