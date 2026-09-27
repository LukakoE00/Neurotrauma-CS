// Disable the Hook warning
#pragma warning disable CS0618

namespace Neurotrauma;
    public static class LootCrates
    {
        private static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            LuaCsSetup.Instance.Hook.Add("NT.medstartercrate.spawn", "NT.medstartercrate.spawn", OnMedStarterCrateSpawn);

            LuaCsSetup.Instance.Hook.Add("character.giveJobItems", "NT.giveHealthScannersBatteries", OnGiveJobItems);
        }

        private static object ?OnMedStarterCrateSpawn(object[] args)
        {
            Item ?item = args.Length > 2 ? args[2] as Item : null;
            if (item == null) return null;

            LuaCsSetup.Instance.Timer.Wait((object[] _) =>
            {
                if (item.Removed) return;
                if (item.Scale == 0.5f) return;
                item.Scale = 0.5f;

                SpawnToolbox1(item);
                SpawnToolbox2(item);
                // SpawnSurgeryToolbox(item);

                HF.SpawnItemPlusFunction("bloodanalyzer", item.OwnInventory, InvSlotType.Any, Vector2.Zero, null);

                HF.SpawnItemPlusFunction("healthscanner", item.OwnInventory, InvSlotType.Any, Vector2.Zero, p =>
                    {
                        Item ?scanner = p[^1] as Item;
                        if (scanner?.OwnInventory == null) return;

                        ItemPrefab prefab = ItemPrefab.Find(null, "batterycell");
                        if (prefab == null) return;

                        Entity.Spawner.AddItemToSpawnQueue(prefab, scanner.WorldPosition, null,  null, battery => scanner.OwnInventory.TryPutItem(battery, null));
                    }
                );

            }, 35);

            return null;
        }

        private static void SpawnToolbox1(Item item)
        {
            HF.SpawnItemPlusFunction("medtoolbox", item.OwnInventory, InvSlotType.Any, Vector2.Zero, p =>
                {
                    var inv = (p[^1] as Item)?.OwnInventory;
                    if (inv == null) return;

                    HF.SpawnItemPlusFunction("defibrillator", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("autocpr", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("tourniquet", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("tourniquet", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("ringerssolution", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("ringerssolution", inv, InvSlotType.Any, Vector2.Zero, null);
                    // HF.SpawnItemPlusFunction("surgicaldrill", inv, InvSlotType.Any, Vector2.Zero, null);
                    // HF.SpawnItemPlusFunction("surgerysaw", inv, InvSlotType.Any, Vector2.Zero, null);
                }
            );
        }

        private static void SpawnToolbox2(Item item)
        {
            HF.SpawnItemPlusFunction("medtoolbox", item.OwnInventory, InvSlotType.Any, Vector2.Zero, p =>
                {
                    var inv = (p[^1] as Item)?.OwnInventory;
                    if (inv == null) return;

                    HF.SpawnItemPlusFunction("antibleeding1", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("gypsum", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("opium", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("antibiotics", inv, InvSlotType.Any, Vector2.Zero, null);
                    HF.SpawnItemPlusFunction("ointment", inv, InvSlotType.Any, Vector2.Zero, null);
                }
            );
        }

        //private static void SpawnSurgeryToolbox(Item item)
        //{
        //    HF.SpawnItemPlusFunction("surgerytoolbox", item.OwnInventory, InvSlotType.Any, Vector2.Zero, p =>
        //        {
        //            var inv = (p[^1] as Item)?.OwnInventory;
        //            if (inv == null) return;

        //            HF.SpawnItemPlusFunction("advscalpel", inv, InvSlotType.Any, Vector2.Zero, null);
        //            HF.SpawnItemPlusFunction("advhemostat", inv, InvSlotType.Any, Vector2.Zero, null);
        //            HF.SpawnItemPlusFunction("advretractors", inv, InvSlotType.Any, Vector2.Zero, null);

        //            for (int i = 0; i < 16; i++) HF.SpawnItemPlusFunction("suture", inv, InvSlotType.Any, Vector2.Zero, null);

        //            HF.SpawnItemPlusFunction("tweezers", inv, InvSlotType.Any, Vector2.Zero, null);
        //            HF.SpawnItemPlusFunction("traumashears", inv, InvSlotType.Any, Vector2.Zero, null);
        //            HF.SpawnItemPlusFunction("drainage", inv, InvSlotType.Any, Vector2.Zero, null);
        //            HF.SpawnItemPlusFunction("needle", inv, InvSlotType.Any, Vector2.Zero, null);
        //        }
        //    );
        //}

        private static object ?OnGiveJobItems(object[] args)
        {
            Character ?character = args.Length > 0 ? args[0] as Character : null;
            if (character == null) return null;

            LuaCsSetup.Instance.Timer.Wait((object[] _) =>
            {
                foreach (Item item in character.Inventory.AllItems)
                {
                    if (item?.Prefab?.Identifier.Value != "healthscanner") continue;
                    if (item.OwnInventory == null) continue;
                    if (item.OwnInventory.GetItemAt(0) != null) continue;

                    ItemPrefab prefab = ItemPrefab.Find(null, "batterycell");
                    if (prefab == null) continue;

                    Entity.Spawner.AddItemToSpawnQueue(prefab, character.WorldPosition, null, null, battery => item.OwnInventory.TryPutItem(battery, character));
                }
            }, 1000);

            return null;
        }
}