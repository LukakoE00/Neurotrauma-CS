// Disable the Hook warning
#pragma warning disable CS0618

namespace Neurotrauma
{
    public static partial class DynamicItems
    {
        // Based on config settings, which items ought to be destroyed
        private static void DynamicRemoveItems()
        {
            var blockedItems = HF.DynamicUnavailableItems();

            foreach (Item item in Item.ItemList.ToList())
            {
                string id = item.Prefab.Identifier.Value;

                if (blockedItems.ContainsKey(id))
                {
                    item.Remove();
                }
            }
        }

        public static void InitDynamicItems()
        {
            // On level swap, remove any items that shouldn't be there
            LuaCsSetup.Instance.Hook.Add("roundEnd", "nt_dynamicremoveitems",
                (params object[] args) =>
                {
                    DynamicRemoveItems();
                    return null;
                });

            // Recreate stores command to make sure newly added items are actually in stores
            LuaCsSetup.Instance.Game.AddCommand("nt_recreatestores", "Recreate all stores.",
                new LuaCsAction(args =>
                {
                    if (GameMain.GameSession.Map != null)
                    {
                        foreach (var location in GameMain.GameSession.Map.Locations)
                        {
                            location.CreateStores(true);
                        }
                    }
                    else
                    {
                        HF.Print("nt_recreatestores: Tried to recreate stores in the campaign map, but there is none!");
                    }
                }));
        }
    }
}