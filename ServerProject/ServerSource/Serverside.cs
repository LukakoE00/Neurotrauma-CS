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
            DefineAllStats();
            DefineAllAfflictions();
        }
    }
}
