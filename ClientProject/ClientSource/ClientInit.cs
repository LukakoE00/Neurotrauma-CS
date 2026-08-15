using Barotrauma.Networking;
using Neurotrauma.ClientSource.OverlayEffects;

namespace Neurotrauma
{
    // Clientside code ONLY!
    public partial class NeurotraumaInit
    {

        private static readonly bool ENABLE_SYMPTOM_EFFECTS_CLIENT = false;

        public void InitClientOnly()
        {
            ConfigurationMenu.AddConfigToPauseMenu();
            DynamicItems.InitDynamicItemsClient();

            if (ENABLE_SYMPTOM_EFFECTS_CLIENT) {SymptomsEffects.InitSymptomsEffects();}

            LuaCsSetup.Instance.Networking.Receive("NT.ConfigUpdate", (object[] args) =>
            {
                IReadMessage msg = (IReadMessage)args[0];
                NTConfig.ReceiveConfig(msg);
            });
        }
    }
}
