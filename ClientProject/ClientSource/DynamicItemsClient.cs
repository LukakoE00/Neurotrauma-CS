// *** There's probably a way better and much shorter way to do this without the wrappers and reflection bullshittery and harmony patching workarounds, but it works

// Disable the Hook warning
#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using System.Reflection;
using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.Networking;
using HarmonyLib;

namespace Neurotrauma
{
    public static partial class DynamicItems
    {
        // Stack specific items together; if you want only the left leg to be cheaper than a right one go fuck yourself
        private static readonly Dictionary<string, string> ItemVariants = new Dictionary<string, string>
        {
            { "antibloodloss2", "NT_ItemPrice_bloodpacks" },
            { "bloodpackoplus", "NT_ItemPrice_bloodpacks" },
            { "bloodpackaminus", "NT_ItemPrice_bloodpacks" },
            { "bloodpackaplus", "NT_ItemPrice_bloodpacks" },
            { "bloodpackbminus", "NT_ItemPrice_bloodpacks" },
            { "bloodpackbplus", "NT_ItemPrice_bloodpacks" },
            { "bloodpackabminus", "NT_ItemPrice_bloodpacks" },
            { "bloodpackabplus", "NT_ItemPrice_bloodpacks" },
            { "rarm", "NT_ItemPrice_arms" },
            { "larm", "NT_ItemPrice_arms" },
            { "rleg", "NT_ItemPrice_legs" },
            { "lleg", "NT_ItemPrice_legs" },
            { "rarmp", "NT_ItemPrice_bionicarms" },
            { "larmp", "NT_ItemPrice_bionicarms" },
            { "rlegp", "NT_ItemPrice_bioniclegs" },
            { "llegp", "NT_ItemPrice_bioniclegs" }
        };

        // Fetch config multipliers
        private static float GetItemMultiplier(string identifier)
        {
            try
            {
                if (string.IsNullOrEmpty(identifier)) return 1.0f;

                // Add grouping so blood bags don't fucking kill me
                string configKey = ItemVariants.GetValueOrDefault(identifier, "NT_ItemPrice_" + identifier);

                if (!NTConfig.Entries.ContainsKey(configKey)) return 1.0f;

                float value = NTConfig.Get(configKey, 1.0f);
                return value;
            }
            catch (Exception e)
            {
                HF.Print($"Error in GetItemMultiplier for {identifier}: {e.Message}");
                return 1.0f;
            }
        }

        public static void InitDynamicItemsClient()
        {
            // Manual harmony patch for fabricators cause it decided to say fuck you, function at the bottom
            try
            {
                var harmony = new Harmony("com.neurotrauma.dynamicitems");
                var targetMethod = typeof(Fabricator).GetMethod("FilterEntities", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (targetMethod != null)
                {
                    var postfix = typeof(DynamicItems).GetMethod(nameof(FilterEntitiesPostfix), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                    if (postfix != null)
                    {
                        harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfix));
                    }
                    else
                    {
                        LuaCsLogger.LogError($"NT Dynamic Items Harmony patch failed at step 3.");
                    }
                }
                else
                {
                    LuaCsLogger.LogError($"NT Dynamic Items Harmony patch failed at step 2.");
                }
            }
            catch (Exception ex)
            {
                LuaCsLogger.LogError($"NT Dynamic Items Harmony patch failed at step 1, Exception: {ex}");
            }

            // PRICE CHANGING
            // Hook into the price-determining function and add a multiplier
            LuaCsSetup.Instance.Hook.Patch(
                "NT.GetAdjustedItemBuyPrice",       // identifier for our Patch
                "Barotrauma.Location+StoreInfo",    // className
                "GetAdjustedItemBuyPrice",          // methodName
                null,                               // parameterTypes (this one has none)

                new LuaCsPatchFunc((object instance, LuaPatcherService.ParameterTable ptable) =>
                {
                    try
                    {
                        // Get the item from the parameter table with null checks to avoid crashes
                        object itemObj = ptable["item"];
                        if (itemObj == null) return null;

                        ItemPrefab? itemPrefab = itemObj as ItemPrefab;
                        if (itemPrefab == null)
                        {
                            Item? item = itemObj as Item;
                            if (item != null) itemPrefab = item.Prefab;
                        }

                        if (itemPrefab == null || itemPrefab.Identifier == null) return null;

                        string id = itemPrefab.Identifier.Value;

                        float mult = GetItemMultiplier(id);

                        // Don't do extra math if the item value is unchanged
                        if (mult == 1.0f) return null;

                        // Get the 'actual price' after the game is done with it's calculations
                        var baseVal = ptable.OriginalReturnValue;
                        if (baseVal == null) return null;

                        // Convert baseVal to int
                        int price = Convert.ToInt32(baseVal);

                        // Apply config-determined multiplier
                        int result = (int)Math.Floor(price * mult + 0.5f);

                        ptable.ReturnValue = result;
                    }
                    catch (Exception e)
                    {
                        HF.Print($"Error in GetAdjustedItemBuyPrice patch: {e.Message}");
                    }

                    return null;
                }),
                ILuaCsHook.HookMethodType.After
            );

            // STORE CHANGES
            // You cannot buy the item; we simply hook into store availability and hide the item.
            // Ensure the items CANNOT be specials.
            LuaCsSetup.Instance.Hook.Patch(
                "NT.Store.FilterStoreItems",                                        // identifier for our Patch
                "Barotrauma.Store",                                                 // className
                "FilterStoreItems",                                                 // methodName
                new string[] { "Barotrauma.MapEntityCategory", "System.String" },   // parameterTypes

                new LuaCsPatchFunc((object instance, LuaPatcherService.ParameterTable ptable) =>
                {
                    try
                    {
                        var store = instance as Barotrauma.Store;
                        if (store == null) return null;

                        LuaCsSetup.Instance.Timer.Wait((params object[] args) =>
                        {
                            // Fetch items to hide
                            var blockedItems = HF.DynamicUnavailableItems();

                            // Get private UI items via reflection
                            var storeBuyListPrivate = typeof(Barotrauma.Store).GetField("storeBuyList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                            var storeList = storeBuyListPrivate?.GetValue(store) as GUIListBox;

                            foreach (var child in storeList?.Content?.Children ?? new List<GUIComponent>()) // Loop execution null check for stupid CS8602
                            {
                                var item = child.UserData;

                                if (item != null)
                                {
                                    // Get private UI items via reflection, fallback if property can't be found (fields)
                                    var itemElem = item.GetType().GetProperty("ItemPrefab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? item.GetType().GetField("ItemPrefab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) as MemberInfo; // bigass line lmfoa

                                    // Get ItemPrefab from either property or field
                                    ItemPrefab? itemPrefab = null;
                                    if (itemElem is PropertyInfo pInfo) itemPrefab = pInfo.GetValue(item) as ItemPrefab;
                                    else if (itemElem is FieldInfo fInfo) itemPrefab = fInfo.GetValue(item) as ItemPrefab;

                                    if (itemPrefab != null)
                                    {
                                        string id = itemPrefab.Identifier.Value;
                                        // Hide items within the table every refresh
                                        if (blockedItems.ContainsKey(id)) child.Visible = false;
                                    }
                                }
                            }
                        }, 10); // Wait 10ms for stuff to fully load
                    }
                    catch (Exception e)
                    {
                        HF.Print($"Error in Store.FilterStoreItems patch: {e.Message}");
                    }

                    return null;
                }),
                ILuaCsHook.HookMethodType.After
            );

            // Force config sync on round start to ensure everything syncs cleanly
            LuaCsSetup.Instance.Hook.Add("roundStart", "forcesyncconfig", (params object[] args) =>
            {
                try
                {
                    if (GameMain.IsMultiplayer)
                    {
                        IWriteMessage msg = LuaCsSetup.Instance.Networking.Start("NT.ConfigRequest");
                        LuaCsSetup.Instance.Networking.Send(msg);
                    }
                }
                catch (Exception e)
                {
                    HF.Print($"Error in roundStart forcesyncconfig hook: {e.Message}");
                }

                return null;
            });
        }

        // Manual Harmony Postfix for type-reflections, preventing crash when opening a fabricator etc
        private static void FilterEntitiesPostfix(Fabricator __instance)
        {
            try
            {
                if (__instance == null) return;

                LuaCsSetup.Instance.Timer.Wait((params object[] args) =>
                {
                    try
                    {
                        var blockedItems = HF.DynamicUnavailableItems();

                        // Get private UI items via reflection
                        var itemListPrivate = typeof(Fabricator).GetField("itemList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        var itemList = itemListPrivate?.GetValue(__instance) as GUIListBox;

                        if (itemList?.Content?.Children != null)
                        {
                            foreach (var child in itemList.Content.Children)
                            {
                                var recipe = child.UserData as FabricationRecipe;
                                if (recipe != null)
                                {
                                    // Check if the recipe's target item is in the blocked items list and hides it if so
                                    string id = recipe.TargetItem.Identifier.Value;
                                    if (blockedItems.ContainsKey(id)) child.Visible = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LuaCsLogger.LogError($"NT Dynamic Items Harmony patch filter entities failed at step 2, Exception; {ex}");
                    }
                }, 1); // Wait 1ms for UI to fully load
            }
            catch (Exception ex)
            {
                LuaCsLogger.LogError($"NT Dynamic Items Harmony patch filter entities failed at step 1, Exception; {ex}");
            }
        }
    }
}