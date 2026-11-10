namespace Neurotrauma;

public static class CPR
{
    private static bool TryGetTarget(object[] args, out Character rescuer, out NTHuman target)
    {
        rescuer = null!;
        target = null!;

        if (args.Length < 1) { return false; }
        if (args[0] is not AnimController animController) { return false; }
        if (animController.Character?.SelectedCharacter == null) { return false; }

        rescuer = animController.Character;
        Character patient = rescuer.SelectedCharacter;

        NTHuman? human = NTHuman.getNTHumanFromCharacter(patient);
        if (human == null)
        {
            HF.PrintError($"CPR error: target character {patient.Name} is not NTHuman!");
            return false;
        }

        target = human;
        return true;
    }

    public static object? OnCPRSuccess(params object[] args)
    {
        if (!TryGetTarget(args, out _, out NTHuman target)) 
        { 
            return null; 
        }

        if (!target.HasAffliction("cpr_buff_auto"))
        {
            target.AddAffliction("cpr_buff", 2f);
        }

        // Prevent fractures during CPR
        target.AddAffliction("cpr_fracturebuff", 2f);

        return null;
    }

    public static object? OnCPRFailure(params object[] args)
    {
        if (!TryGetTarget(args, out Character rescuer, out NTHuman target)) { return null; }

        // Prevent fractures during CPR
        target.AddAffliction("cpr_fracturebuff", 2f);
        target.AddAfflictionLimb("blunttrauma", LimbType.Torso, 0.3f);

        float fractureChance = NTConfig.Get("NT_fractureChance", 1f) * NTConfig.Get("NT_CPRFractureChance", 1f) * 0.2f / HF.GetSkillLevel(rescuer, "medical".ToIdentifier());

        if (HF.Chance(fractureChance)) 
        { 
            target.AddAffliction("fracturedribs", 1f); 
        }

        return null;
    }
}