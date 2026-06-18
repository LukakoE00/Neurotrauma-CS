// Disable the Hook warning
#pragma warning disable CS0618
namespace Neurotrauma
{
    public static class CauseScreams
    {
        public static void InitScreamsHook()
        {
            LuaCsSetup.Instance.Hook.Add("NT.causeScreams", "NT.causeScreams", (params object[] args) =>
            {
                if (!NTConfig.Get("NT_screams", true)) return null;

                var character = args[2] as Character;
                if (character == null) return null;

                HF.SetAffliction(character, "screaming", 10f, null, 0f);
                return null;
            });
        }
    }
}