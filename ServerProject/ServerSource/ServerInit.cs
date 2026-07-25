using Barotrauma.Networking;

namespace Neurotrauma
{
    // Serverside (and thus MULTIPLAYER) code ONLY!
    public partial class NeurotraumaInit
    {
        public void InitServerOnly()
        {
           // gulp
            LuaCsSetup.Instance.Networking.Receive("NT.ConfigUpdate", (object[] args) =>
            {
                IReadMessage msg = (IReadMessage)args[0];
                Client ?sender = args.Length > 1 ? args[1] as Client : null;

                if (sender == null || !sender.HasPermission(ClientPermissions.ManageSettings))
                {
                    return;
                }

                NTConfig.ReceiveConfig(msg);
                NTConfig.SaveConfig();
            });

            LuaCsSetup.Instance.Networking.Receive("NT.ConfigRequest", (object[] args) =>
            {
                Client ?sender = args.Length > 1 ? args[1] as Client : null;

                if (sender == null)
                {
                    return;
                }
                
                NTConfig.SendConfig();
            });
        }
    }
}
