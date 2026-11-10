namespace Neurotrauma
{
    public static class Screaming
    {
        public static object? DoScreaming(params object[] args)
        {
            if (!NTConfig.Get("NT_screams", true))
            {
                return null;
            }

            var character = args[2] as Character;

            if (character == null)
            {
                return null;
            }

            HF.SetAffliction(character, "screaming", 10f, null, 0f);

            return null;
        }
    }
}