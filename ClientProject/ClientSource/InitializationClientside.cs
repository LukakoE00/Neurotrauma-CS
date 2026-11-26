using Barotrauma;
using Barotrauma.Networking;
using Barotrauma.Plugins;
using Neurotrauma.ClientSource;
using Neurotrauma.ClientSource.OverlayEffects;
using MoonSharp.Interpreter;

namespace Neurotrauma;

public partial class Initialization
{
    private static readonly bool ENABLE_SYMPTOM_EFFECTS_CLIENT = false;

    public partial void InitProjectSpecific()
    {

        // Eyes Compatibility
        UserData.RegisterType(typeof(ButtonData));

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

        LuaCsSetup.Instance.Networking.Receive("NT.RegenLimb", (object[] args) =>
        {
            IReadMessage msg = (IReadMessage)args[0];
            ushort CharacterID = msg.ReadUInt16();
            LimbType Limb = (LimbType)msg.ReadInt32();

            if (Entity.FindEntityByID(CharacterID) is Character C)
            {
                Amputations.RegenerateLimb(C, Limb, ReceivedFromServer: true);
            }
        });
    }

    partial void DisposeClient()
    {
        ButtonsHUI.RemoveNTButtons();
        ConfigurationMenu.RemoveConfigButtonFromPauseMenu();
    }
}