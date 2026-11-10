using Barotrauma;
using Barotrauma.Networking;
using Barotrauma.Plugins;
using Neurotrauma.ClientSource;
using Neurotrauma.ClientSource.OverlayEffects;

namespace Neurotrauma;

public partial class Initialization
{
    private static readonly bool ENABLE_SYMPTOM_EFFECTS_CLIENT = false;

    public partial void InitProjectSpecific()
    {
        ConfigurationMenu.InitNTConfig();
        DynamicItems.InitDynamicItemsClient();
        ButtonsHUI.InitClient();

        if (ENABLE_SYMPTOM_EFFECTS_CLIENT)
        {
            SymptomsEffects.InitSymptomsEffects();
        }

        LuaCsSetup.Instance.Networking.Receive("NT.ConfigUpdate", (object[] args) =>
        {
            IReadMessage msg = (IReadMessage)args[0];
            NTConfig.ReceiveConfig(msg);
        });
    }

    partial void DisposeClient()
    {
        ButtonsHUI.RemoveNTButtons();
        ConfigurationMenu.RemoveConfigButtonFromPauseMenu();
    }
}