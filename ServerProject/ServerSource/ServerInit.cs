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
                Client? sender = args.Length > 1 ? args[1] as Client : null;

                if (sender == null || !sender.HasPermission(ClientPermissions.ManageSettings))
                {
                    return;
                }

                NTConfig.ReceiveConfig(msg);
                NTConfig.SaveConfig();
            });

            LuaCsSetup.Instance.Networking.Receive("NT.ConfigRequest", (object[] args) =>
            {
                Client? sender = args.Length > 1 ? args[1] as Client : null;

                if (sender == null)
                {
                    return;
                }

                NTConfig.SendConfig();
            });

            LuaCsSetup.Instance.Networking.Receive("NT.HUIButtonTreatment", (object[] args) =>
            {
                // Get the data sent over by client buttons and use it to do treatment!
                IReadMessage msg = (IReadMessage)args[0];

                ushort userId = msg.ReadUInt16();
                ushort targetId = msg.ReadUInt16();
                int selectedLimbIndex = msg.ReadInt32();
                string itemIdentifier = msg.ReadString();

                Character user = Character.CharacterList.First(character => character.ID == userId);
                Character target = Character.CharacterList.First(character => character.ID == targetId);
                Limb targetLimb = target.AnimController.Limbs.First(limb => limb.HealthIndex == selectedLimbIndex);
                NTItemMethods.NTItemsRegistry.TryGetValue(itemIdentifier, out var itemFunction);

                ItemPrefab prefab = ItemPrefab.FindByIdentifier(itemIdentifier) as ItemPrefab;

                var infos = new NTItemMethods.ItemUpdateFunctionInfos(prefab, user, target, targetLimb);
                itemFunction(infos);
            });
        }
    }
}
