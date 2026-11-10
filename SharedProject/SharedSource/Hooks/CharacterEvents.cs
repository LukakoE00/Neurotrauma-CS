using Barotrauma;

namespace Neurotrauma;

public static class CharacterEvents
{
    public static void OnCharacterCreated(params object[] args)
    {
        if (args.Length < 1 || args[0] is not Character character) { return; }
        if (!character.IsHuman) { return; }

        LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
        {
            var h = NTHuman.getNTHumanFromCharacter(character) ?? new NTHuman(character);

            if (h.Human.teamID == CharacterTeamType.Team1)
            {
                h.AddAffliction("luabotomy", 0.1f);
            }

        }, 1000);
    }

    public static void OnCharacterDeath(params object[] args)
    {
        if (args.Length < 1 || args[0] is not Character character) { return; }
        if (!character.IsHuman) { return; }

        NTHuman? human = NTHuman.getNTHumanFromCharacter(character);
        if (human == null) { return; }

        NTHuman.RemoveNTHuman(human);
    }
}