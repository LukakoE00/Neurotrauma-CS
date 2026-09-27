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

                if (animController?.Character?.SelectedCharacter == null)
                {
                    return null;
                }

                Character character = animController.Character.SelectedCharacter;
                NTHuman? target = NTHuman.getNTHumanFromCharacter(character);

                if (target == null)
                {
                    HF.PrintError($"CPR Success error: target character {character.Name} is not NTHuman!");
                    return null;
                }

                if (!target.HasAffliction("cpr_buff_auto"))
                {
                    target.AddAffliction("cpr_buff", 2f);
                }

                // Prevent fractures during CPR
                target.AddAffliction("cpr_fracturebuff", 2f);

                return null;
            });

            LuaCsSetup.Instance.Hook.Add("character.CPRFailed", "NT.CPRFailed", (params object[] args) =>
            {
                if (args.Length < 1) return null;

                var animController = args[0] as AnimController;
                if (animController?.Character?.SelectedCharacter == null) return null;

                Character character = animController.Character.SelectedCharacter;
                NTHuman? target = NTHuman.getNTHumanFromCharacter(character);

                if (target == null)
                {
                    HF.PrintError($"CPR Success error: target character {character.Name} is not NTHuman!");
                    return null;
                }

                // Prevent fractures during CPR
                target.AddAffliction("cpr_fracturebuff", 2f);
                target.AddAfflictionLimb("blunttrauma", LimbType.Torso, 0.3f);

                float fractureChance =
                    NTConfig.Get("NT_fractureChance", 1f) *
                    NTConfig.Get("NT_CPRFractureChance", 1f) *
                    0.2f /
                    HF.GetSkillLevel(animController.Character, "medical");

                if (HF.Chance(fractureChance)) target.AddAffliction("t_fracture", 1f);
                    

                return null;
            });
        }
    }
}