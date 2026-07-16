using Barotrauma.Networking;
using System.Xml.Linq;

namespace Neurotrauma
{

    public static class HF // The HelperFunctions class
    {
        private static readonly Random Random = new Random();

        // Lists & Dictionaries
        // List containing Arm + Leg LimbTypes.
        public static readonly List<LimbType> ArmsLegsToCheck = [LimbType.LeftArm, LimbType.RightArm, LimbType.LeftLeg, LimbType.RightLeg];

        // List containing Leg LimbTypes.
        public static readonly List<LimbType> LegsToCheck = [LimbType.LeftLeg, LimbType.RightLeg];

        // List containing Arm LimbTypes.
        public static readonly List<LimbType> ArmsToCheck = [LimbType.LeftArm, LimbType.RightArm];

        // All HealthUI Limbs
        public static readonly List<LimbType> LimbsToCheck = [LimbType.LeftArm, LimbType.RightArm, LimbType.LeftLeg, LimbType.RightLeg, LimbType.Torso, LimbType.Head];

        // Limb Dictionaries
        public static readonly Dictionary<LimbType, string> StringLimbsToCheck = new Dictionary<LimbType, string>()
        {
            { LimbType.LeftArm, "LeftArm" },
            { LimbType.RightArm, "RightArm" },
            { LimbType.LeftLeg, "LeftLeg" },
            { LimbType.RightLeg, "RightLeg" },
            { LimbType.Torso, "Torso" },
            { LimbType.Head, "Head" }
        };

        public static readonly Dictionary<LimbType, string> ShortHandLimbsToCheck = new Dictionary<LimbType, string>()
        {
            { LimbType.LeftArm, "la" },
            { LimbType.RightArm, "ra" },
            { LimbType.LeftLeg, "ll" },
            { LimbType.RightLeg, "rl" },
            { LimbType.Torso, "t" },
            { LimbType.Head, "h" }
        };

        public static readonly Dictionary<LimbType, double> DefaultLimbAffStrengths = new Dictionary<LimbType, double>()
        {
            { LimbType.Head, 0 },
            { LimbType.Torso, 0 },
            { LimbType.LeftArm, 0 },
            { LimbType.RightArm, 0 },
            { LimbType.LeftLeg, 0 },
            { LimbType.RightLeg, 0 }
        };

        public static readonly Dictionary<LimbType, int> DefaultLimbSymUpdateTime = new Dictionary<LimbType, int>()
        {
            { LimbType.Head, 0 },
            { LimbType.Torso, 0 },
            { LimbType.LeftArm, 0 },
            { LimbType.RightArm, 0 },
            { LimbType.LeftLeg, 0 },
            { LimbType.RightLeg, 0 }
        };

        /// <summary>
        /// Returns the limb of a character given the LimbType.
        /// </summary>
        /// <param name="Character">Character to check.</param>
        /// <param name="GivenLimbType">LimbType to check.</param>
        /// <returns>The given limb on a character, else null.</returns>
        public static Limb GetCharacterLimb(Character Character, LimbType GivenLimbType)
        {
            return Character.AnimController.GetLimb(GivenLimbType);
        }

        /// <summary>
        /// Converts a limbtype into it's more common type. I.E LeftHand becomes LeftArm and RightFoot becomes RightArm. So on so forth.
        /// </summary>
        /// <param name="GivenLimbType">LimbType to check.</param>
        /// <returns>One of the Left/Right, Arm/Leg or Head/Torso Limbs.</returns>
        public static LimbType NormalizeLimbType(LimbType GivenLimbType) // Our beloved one and only normalize limb type.
        {
            if (LimbsToCheck.Contains(GivenLimbType)) { return GivenLimbType; }

            if (GivenLimbType == LimbType.LeftHand || GivenLimbType == LimbType.LeftForearm)
            {
                return LimbType.LeftArm;
            }

            if (GivenLimbType == LimbType.RightHand || GivenLimbType == LimbType.RightForearm)
            {
                return LimbType.RightArm;
            }

            if (GivenLimbType == LimbType.LeftFoot || GivenLimbType == LimbType.LeftThigh)
            {
                return LimbType.LeftLeg;
            }

            if (GivenLimbType == LimbType.RightFoot || GivenLimbType == LimbType.RightThigh)
            {
                return LimbType.RightLeg;
            }

            if (GivenLimbType == LimbType.Waist)
            {
                return LimbType.Torso;
            }

            return GivenLimbType;
        }

        /// <summary>
        /// Converts our limbtype into a string.
        /// </summary>
        /// <param name="GivenLimbType">LimbType to check.</param>
        /// <returns>A LimbType as a string, else null.</returns>
        public static string LimbToString(LimbType GivenLimbType)
        {
            LimbType NormalizedLimb = NormalizeLimbType(GivenLimbType);
            StringLimbsToCheck.TryGetValue(NormalizedLimb, out string Value);
            return Value;
        }

        /// <summary>
        /// Validates if a string Identifier is valid for a search.
        /// </summary>
        /// <param name="Identifier">Any identifier that needs to be checked.</param>
        /// <returns>True if the Identifier is valid, else False.</returns>
        public static bool IsValidIdentifier(string Identifier)
        {
            return !(Identifier == null || Identifier == "");
        }

        /// <summary>
        /// Converts our affliction ID into an affliction with our limb prefix.
        /// </summary>
        /// <param name="GivenLimbType">LimbType to convert to a prefix.</param>
        /// <param name="Identifier">Affliction Identifier to append to a limb prefix.</param>
        /// <returns>A concatenated string composed of the limb and the affliction identifier, else null.</returns>
        public static string CreateLimbAfflictionID(LimbType GivenLimbType, string Identifier)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);

            if (!ShortHandLimbsToCheck.TryGetValue(GivenLimbType, out string Value))
            {
                return null;
            }

            return $"{Value}_{Identifier}";
        }

        /// <summary>
        /// Checks our limb to see if it's an extremity.
        /// </summary>
        /// <param name="GivenLimbType">LimbType to check.</param>
        /// <returns>True if it is an extremity, else False.</returns>
        public static bool LimbIsExtremity(LimbType GivenLimbType)
        {
            return GivenLimbType != LimbType.Torso && GivenLimbType != LimbType.Head;
        }

        /// <summary>
        /// Locks an Arm by force-occupying that inventory slot.
        /// </summary>
        /// <param name="Character">The character whose arm to lock.</param>
        /// <param name="ArmToLock">The arm to lock, either "LeftArm" or "RightArm".</param>
        public static void ForceArmLock(Character Character, string ArmToLock)
        {
            // In Multiplayer, only run on the host.
#if CLIENT
                if (GameMain.IsMultiplayer)
                {
                    return;
                }
#endif

            // Safety check!
            if (ArmToLock != "LeftArm" && ArmToLock != "RightArm")
            {
                return;
            }

            // If the Entity Spawner isn't initialized yet, skip a game tick and try again!
            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) => { ForceArmLock(Character, ArmToLock); }, 35);
                return;
            }

            // Determine which Arm should be locked, then set the Inventory Slot accordingly.
            bool IsLeft = ArmToLock == "LeftArm";
            int HandIndex = IsLeft ? 5 : 6;
            InvSlotType Slot = IsLeft ? InvSlotType.LeftHand : InvSlotType.RightHand;

            // Drop the previously held item.
            Item PrevItem = Character.Inventory.GetItemAt(HandIndex);

            if (PrevItem != null)
            {
                PrevItem.Drop(Character, true);
            }

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                ItemPrefab ArmLock = ItemPrefab.Find(null, "armlock");

                Entity.Spawner.AddItemToSpawnQueue(ArmLock, Character.WorldPosition, null, null, (Item spawnedItem) =>
                {
                    if (Character.Inventory != null)
                    {
                        Character.Inventory.TryPutItem(spawnedItem, null, [Slot]);
                    }
                });
            }, 35);

            return;
        }

        /// <summary>
        /// Returns the damage resistance to an affliction for a Character.
        /// </summary>
        /// <param name="Character">Character to check for resistances.</param>
        /// <param name="Identifier">Affliction Identifier whose resistance we want checked.</param>
        /// <param name="GivenLimbType">LimbType to check in case of limb-specific resistances.</param>
        /// <returns></returns>
        public static float GetResistance(Character Character, string Identifier, LimbType GivenLimbType = LimbType.Torso) // Only returns health Resistance
        {
            AfflictionPrefab Prefab = AfflictionPrefab.Prefabs[Identifier];
            return Character.CharacterHealth.GetResistance(Prefab, GivenLimbType);
        }

        /// <summary>
        /// Returns the damage resistance to an affliction present on an Item.
        /// </summary>
        /// <param name="Item">The item to check for resistances.</param>
        /// <param name="ResistanceID">The identifier of the Resistance we're checking for.</param>
        /// <returns>Actual damage multiplier for a type of resistance, else 1.</returns>
        public static float GetItemAfflictionResistance(Barotrauma.Item Item, string ResistanceID) // I thought this was a useful helper function. Credit to Antinous for the original method.
        {
            IEnumerable<ContentXElement> ItemElements = Item.Prefab.ConfigElement.Elements();
            foreach (ContentXElement ItemElement in ItemElements) // Iterate through our elements to find "Wearable"
            {
                if (ItemElement.Name == "Wearable")
                {
                    foreach (XElement Element in ItemElement.Elements())
                    {
                        if (Element.Name == "damagemodifier")
                        {
                            string Afflictions = Element.GetAttributeString("afflictiontypes", "").ToLower();
                            if (Afflictions.Contains(ResistanceID))
                            {
                                return (float)Convert.ToDouble(Element.GetAttributeString("damagemultiplier", "1"));
                            }
                        }
                    }
                }
            }
            return 1; // Womp Womp
        }

        /// <summary>
        /// Returns the depth of an item in the world; not to be confused with Sprite Depth.
        /// </summary>
        /// <param name="Item">Item to check.</param>
        /// <returns>Y-coordinate value of the given item</returns>
        public static float FindDepth(Barotrauma.Item Item)
        {
            return Level.Loaded.GetRealWorldDepth(Item.WorldPosition.Y);
        }

        /// <summary>
        /// Checks a character's inventory for a Barotrauma Item in a specific slot returns it.
        /// <para>If you want the Item Identifier, use GetCharacterInventorySlotIdentifier instead.</para>
        /// <para>Premade functions exist for Outer-, Head- and Innerwear; GetItemIn(LeftHand/RightHand/OuterWear/InnerWear/HeadWear/HeadSet) respectively.</para>
        /// </summary>
        /// <param name="Character">Character whose inventory to check.</param>
        /// <param name="Slot">Inventory slot index number.</param>
        /// <returns>The Item if present, else null.</returns>
        public static Barotrauma.Item GetCharacterInventorySlot(Character Character, int Slot)
        {
            return Character.Inventory.GetItemAt(Slot);
        }

        /// <summary>
        /// Checks the Right Hand inventory slot of a character for an item.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The item if present, else null.</returns>
        public static Barotrauma.Item GetItemInRightHand(Character Character)
        {
            return GetCharacterInventorySlot(Character, 6);
        }

        /// <summary>
        /// Checks the Left Hand inventory slot of a character for an item.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The item if present, else null.</returns>
        public static Barotrauma.Item GetItemInLeftHand(Character Character)
        {
            return GetCharacterInventorySlot(Character, 5);
        }

        /// <summary>
        /// Checks the Outerwear inventory slot of a character for an item.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The item if present, else null.</returns>
        public static Barotrauma.Item GetItemInOuterWear(Character Character)
        {
            return GetCharacterInventorySlot(Character, 4);
        }

        /// <summary>
        /// Checks the InnerWear inventory slot of a character for an item.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The item if present, else null.</returns>
        public static Barotrauma.Item GetItemInInnerWear(Character Character)
        {
            return GetCharacterInventorySlot(Character, 3);
        }

        /// <summary>
        /// Checks the HeadWear inventory slot of a character for an item.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The item if present, else null.</returns>
        public static Barotrauma.Item GetItemInHeadWear(Character Character)
        {
            return GetCharacterInventorySlot(Character, 2);
        }

        /// <summary>
        /// Checks the HeadSet inventory slot of a character for an item.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The item if present, else null.</returns>
        public static Barotrauma.Item GetItemInHeadSet(Character Character)
        {
            return GetCharacterInventorySlot(Character, 1);
        }

        /// <summary>
        /// Checks a character's inventory for a Barotrauma Item in a specific slot and returns the Identifier.
        /// <para>If you want the item itself, use GetCharacterInventorySlot instead.</para>
        /// <para>Premade functions exist for Outer-, Head- and Innerwear; Get(Outer/Head/Inner)WearIdentifier respectively.</para>
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <param name="Slot">Inventory slot index number.</param>
        /// <returns>Item Identifier, else 'null' as a string.</returns>
        public static string GetCharacterInventorySlotIdentifier(Character Character, int Slot)
        {
            Barotrauma.Item Item = GetCharacterInventorySlot(Character, Slot);
            if (Item == null)
            {
                return "null";
            }

            return Item.Prefab.Identifier.Value;
        }

        /// <summary>
        /// Checks the OuterWear inventory slot of a character for an item and pulls its identifier.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The Item's Identifier if present, else 'null' as a string.</returns>
        public static string GetOuterWearIdentifier(Character Character)
        {
            return GetCharacterInventorySlotIdentifier(Character, 4);
        }

        /// <summary>
        /// Checks the InnerWear inventory slot of a character for an item and pulls its identifier.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The Item's Identifier if present, else 'null' as a string.</returns>
        public static string GetInnerWearIdentifier(Character Character)
        {
            return GetCharacterInventorySlotIdentifier(Character, 3);
        }

        /// <summary>
        /// Checks the HeadWear inventory slot of a character for an item and pulls its identifier.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <returns>The Item's Identifier if present, else 'null' as a string.</returns>
        public static string GetHeadWearIdentifier(Character Character)
        {
            return GetCharacterInventorySlotIdentifier(Character, 2);
        }

        /// <summary>
        /// Prints a message in the chatbox for a client.
        /// </summary>
        /// <param name="Client">The client to whom the message should be sent.</param>
        /// <param name="Message">The actual message to send.</param>
        /// <param name="Color">Color that the message should be.</param>
        public static void DMClient(Client Client, string Message, Color? Color)
        {
            var chatMessage = ChatMessage.Create("", Message, ChatMessageType.Server, null);
            if (Color.HasValue) chatMessage.Color = Color.Value;

#if SERVER
            if (Client == null) return;

            LuaGame.SendDirectChatMessage(chatMessage, Client);

#else
            if (GameMain.GameSession?.CrewManager != null)
            {
                GameMain.GameSession.CrewManager.AddSinglePlayerChatMessage("", chatMessage.Text, ChatMessageType.Server, Character.Controlled);
            }
#endif
        }

        /// <summary>
        /// // Pulls a ConfigData entry that holds an RGB value, and turns it into a color.
        /// </summary>
        /// <param name="ConfigKey">Identifier corresponding to a config entry, specifically one with comma-seperated RGB values.</param>
        /// <returns>RGB color value.</returns>
        public static Color GetColorFromConfigEntry(string ConfigEntry)
        {
            List<string> rgbList = NTConfig.Get(ConfigEntry, []);
            if (rgbList == null || rgbList.Count == 0) return new Color(127, 255, 255);

            var rgbString = rgbList[0];
            var rgb = rgbString.Split(',');

            if (rgb.Length < 3)
            {
                return new Color(127, 255, 255);
            }

            return new Color(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2])
            );
        }

        /// <summary>
        /// Returns the Length / Magnitude of a Vector2.
        /// </summary>
        /// <param name="Vector">Vector whose length needs to be known.</param>
        /// <returns>Length / Magnitude value, else null.</returns>
        public static double Magnitude(Vector2 Vector)
        {
            return (double)Vector.Length();
        }

        /// <summary>
        /// Returns an unrounded, random number within a specific range.
        /// </summary>
        /// <param name="Min">Minimum value</param>
        /// <param name="Max">Maximum value</param>
        /// <returns>Float within the given range.</returns>
		public static float RandomRange(float Min, float Max)
        {
            return Min + (float)Random.NextDouble() * (Max - Min);
        }

        /// <summary>
        /// Determines whether or not a 'chance' check passed.
        /// </summary>
        /// <param name="Chance">Numerical value to denote chance; i.e. 0.5 = 50%.</param>
        /// <returns>True if the chance check passed; else False.</returns>
		public static bool Chance(float Chance)
        {
            return Random.NextDouble() < Chance;
        }

        /// <summary>
        /// Converts a boolean into the Out parameter if true, else 0.
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Out"></param>
        /// <returns></returns>
		public static float BoolToNum(bool Value, float Out = 1)
        {
            if (Value)
            {
                return Out;
            }

            return 0;
        }

        /// <summary>
        /// Checks if the game is currently paused. Multiplayer instances can never be paused.
        /// </summary>
        /// <returns>True if the game is paused, else False.</returns>
        public static bool GameIsPaused()
        {
#if SERVER
            return false;
#elif SHARED
#if !CLIENT
            return LuaGame.Paused;
#endif
#else
            return false;
#endif
        }

        /// <summary>
        /// Checkes to see if a level is running.
        /// </summary>
        /// <returns>True if in a level, else false.</returns>
        public static bool InGame()
        {
#if SERVER
            if (GameMain.Server == null) return false;
            return GameMain.Server.GameStarted;
#elif SHARED
            return LuaGame.RoundStarted;
#endif
            if (GameMain.GameSession == null) return false;
            return GameMain.GameSession.IsRunning;
        }

        /// <summary>
        /// Checks to see if a game is Multiplayer.
        /// </summary>
        /// <returns>True if Multiplayer, else False.</returns>
        public static bool GameIsMultiplayer()
        {
            return GameMain.IsMultiplayer;
        }

        /// <summary>
        /// Checks to see if a game is Singleplayer.
        /// </summary>
        /// <returns>True if Singleplayer, else False.</returns>
        public static bool GameIsSingleplayer()
        {
            return GameMain.IsSingleplayer;
        }

        /// <summary>
        /// Gives an item to a character at a specific condition.
        /// </summary>
        /// <param name="Character">The character who will recieve the item.</param>
        /// <param name="ItemIdentifier">The identifier for the item to give.</param>
        /// <param name="Condition">The condition at which the item should be given.</param>
        public static void GiveItem(Character Character, string ItemIdentifier, float Condition = 100)
        {
            // HostSide Only
#if CLIENT
                if (HF.GameIsMultiplayer()) return;
#endif

            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    GiveItem(Character, ItemIdentifier, Condition);
                }, 35);

                return;
            }

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                ItemPrefab prefab = ItemPrefab.Find(null, ItemIdentifier);
                Entity.Spawner.AddItemToSpawnQueue(prefab, Character.WorldPosition, null, null, item =>
                {
                    item.Condition = Condition;
                    Character.Inventory.TryPutItem(item, null, [InvSlotType.Any]);
                });
            }, 35);

            return;
        }

        /// <summary>
        /// Spawns an item that runs a function on spawn-in.
        /// </summary>
        /// <param name="ItemIdentifier">The identifier for the item to spawn.</param>
        /// <param name="Inventory">The inventory in which to spawn the item.</param>
        /// <param name="Slot">The slot in which to put the item.</param>
        /// <param name="Position">The position in the world where to spawn the item.</param>
        /// <param name="Function">The function to run when the item is spawned.</param>
        /// <param name="Parameters">The parameters for the function used by the item.</param>
        public static void SpawnItemPlusFunction(string ItemIdentifier, Inventory Inventory, InvSlotType Slot, Vector2 Position, LuaCsAction Function, params object[] Parameters)
        {
#if CLIENT
                if (HF.GameIsMultiplayer()) return;
#endif

            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    SpawnItemPlusFunction(ItemIdentifier, Inventory, Slot, Position, Function, Parameters);
                }, 35);

                return;
            }

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                ItemPrefab prefab = ItemPrefab.Find(null, ItemIdentifier);

                Entity.Spawner.AddItemToSpawnQueue(prefab, Position, null, null, item =>
                {
                    if (Inventory != null)
                    {
                        Inventory.TryPutItem(item, null, [Slot], true, true);
                    }

                    // Not so sure on this part tbh - Lukako
                    object[] args = Parameters
                        .Concat(new object[] { item })
                        .ToArray();
                    Function?.Invoke(args);
                });

            }, 35);
        }

        /// <summary>
        /// Gives an item to a specified character, and takes a function to run on spawn.
        /// </summary>
        /// <param name="ItemIdentifier">The identifier for the item to give.</param>
        /// <param name="Character">The character to which the item should be given.</param>
        /// <param name="Function">The function to run when the item is spawned.</param>
        /// <param name="Parameters">The parameters for the function used by the item.</param>
        public static void GiveItemPlusFunction(string ItemIdentifier, Character Character, LuaCsAction Function, params object[] Parameters)
        {
#if CLIENT
                if (HF.GameIsMultiplayer()) return;
#endif

            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    GiveItemPlusFunction(ItemIdentifier, Character, Function, Parameters);
                }, 35);

                return;
            }

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                ItemPrefab prefab = ItemPrefab.Find(null, ItemIdentifier);

                Entity.Spawner.AddItemToSpawnQueue(prefab, Character.WorldPosition, null, null, item =>
                {
                    if (Character?.Inventory != null)
                    {
                        Character.Inventory.TryPutItem(item, null, [InvSlotType.Any]);
                    }

                    object[] args = Parameters
                        .Concat(new object[] { item })
                        .ToArray();

                    Function?.Invoke(args);
                });

            }, 35);
        }

        /// <summary>
        /// Spawns an item at a given position.
        /// </summary>
        /// <param name="ItemIdentifier">The identifier for the item to spawn.</param>
        /// <param name="Position">The position in the world where the item will be spawned.</param>
        public static void SpawnItemAt(string ItemIdentifier, Vector2 Position)
        {
#if CLIENT
                if (HF.GameIsMultiplayer()) return;
#endif

            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    SpawnItemAt(ItemIdentifier, Position);
                }, 35);

                return;
            }

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                ItemPrefab prefab = ItemPrefab.Find(null, ItemIdentifier);

                Entity.Spawner.AddItemToSpawnQueue(prefab, Position, null, null, null);
            }, 35);
        }

        /// <summary>
        /// Removes an item from the session.
        /// </summary>
        /// <param name="Item">A specific Item to be removed.</param>
        public static void RemoveItem(Item Item)
        {
#if CLIENT
                if (HF.GameIsMultiplayer()) return;
#endif

            if (Item == null || Item.Removed) return;

            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    RemoveItem(Item);
                }, 35);

                return;
            }

            Entity.Spawner.AddEntityToRemoveQueue(Item);
        }

        /// <summary>
        /// Checks an item to see if it has a specified tag.
        /// </summary>
        /// <param name="Item">The item to check for a tag.</param>
        /// <param name="Tag">The tag that gets checked for.</param>
        /// <returns></returns>
        public static bool ItemHasTag(Item Item, string Tag)
        {
            return Item.HasTag(Tag);
        }

        /// <summary>
        /// Moves an item into a specified container.
        /// </summary>
        /// <param name="Container">The container in which the item shall be put.</param>
        /// <param name="Identifier">The identifier of the item that should be put into the container.</param>
        /// <param name="Index">The inventory slot index into which the item gets put.</param>
        public static void PutItemInContainer(Item Container, string Identifier, int Index = 0)
        {
#if CLIENT
                if (HF.GameIsMultiplayer()) return;
#endif

            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    PutItemInContainer(Container, Identifier, Index);
                }, 35);

                return;
            }

            Inventory inv = Container?.OwnInventory;
            if (inv == null) return;

            Item prevItem = inv.GetItemAt(Index);
            if (prevItem != null)
            {
                inv.RemoveItem(prevItem);
                prevItem.Drop(null, true, true);
            }

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                ItemPrefab prefab = ItemPrefab.Find(null, Identifier);

                Entity.Spawner.AddItemToSpawnQueue(prefab, Container.WorldPosition, null, null,
                    item =>
                    {
                        inv.TryPutItem(item, null, new InvSlotType[] { (InvSlotType)Index }, true, true);
                    });

            }, 35);
        }

        /// <summary>
        /// Removes a character from the session.
        /// </summary>
        /// <param name="Character">The character that gets removed.</param>
        public static void RemoveCharacter(Character Character)
        {
#if SHARED
#if CLIENT
            if (LuaGame.IsMultiplayer())
            {
                return;
            }
#endif
            if (Character == null || Character.Removed) {return;}    

            if (Entity.Spawner == null)
            {
                LuaCsTimer.Wait((params object[] _) => { RemoveCharacter(Character); }, 35); // Thanks Evil and Misinformation Factory
                return;
            }
            
            Entity.Spawner.AddEntityToRemoveQueue(Character)
#endif
        }

        /// <summary>
        /// Converts our cause of death to a string.
        /// </summary>
        /// <param name="COD">The Cause-Of-Death.</param>
        /// <returns>A string containing the cause-of-death</returns>
        public static string CauseOfDeathToString(CauseOfDeath COD) // COD Zombies?????
        {
            string Res;

            if (COD.Affliction != null && COD.Affliction.CauseOfDeathDescription != null)
            {
                Res = Convert.ToString(COD.Affliction.CauseOfDeathDescription) ?? "";
            }
            else
            {
                Res = Convert.ToString(COD.Type) ?? "";
            }
            return Res ?? "";
        }

        /// <summary>
        /// Explodes a person into itty bitty pieces.
        /// </summary>
        /// <param name="GivenEntity">Entity to explode.</param>
        /// <param name="Range">Range of the explosion.</param>
        /// <param name="Force">Force of the explosion; used for knockback.</param>
        /// <param name="Damage">Amount of damage the explosion does to characters.</param>
        /// <param name="StructureDamage">Amount of damage the explosion does to structures.</param>
        /// <param name="ItemDamage">Amount of damage the explosion does to items.</param>
        /// <param name="EmpStrength">Strength of the EMP caused by the explosion.</param>
        /// <param name="BallastFloraStrength">Amount of damage the explosion does to Ballast Flora</param>
        public static void Explode(Entity GivenEntity, float Range = 0, float Force = 0, float Damage = 0, float StructureDamage = 0, float ItemDamage = 0, float EmpStrength = 0, float BallastFloraStrength = 0)
        {
            LuaGame.Explode(GivenEntity.WorldPosition, Range, Force, Damage, StructureDamage, ItemDamage, EmpStrength, BallastFloraStrength);
        }

        /// <summary>
        /// Converts an Identifier type into a String.
        /// </summary>
        /// <param name="Identifier">The identifier that should be turned into a string.</param>
        /// <returns></returns>
        public static string GetText(Identifier Identifier)
        {
            LocalizedString Text = TextManager.Get(Identifier);
            return Convert.ToString(Text) ?? "";
        }

        /// <summary>
        /// Returns a count of alive characters that have a certain type of job.
        /// </summary>
        /// <param name="JobIdentifier">The identifier of the job that will get its members tallied.</param>
        /// <returns></returns>
        public static int JobMemberCount(string JobIdentifier)
        {
            int Res = 0;
            foreach (Character Character in Character.CharacterList)
            {
                if (Character.IsHuman && !Character.IsDead && Character.Info.Job != null)
                {
                    if (Character.Info.Job.Prefab.Identifier.Value == JobIdentifier) { Res++; }
                }
            }
            return Res;
        }

        /// <summary>
        /// Sends a message to a client that pops into the GUI.
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="Msg"></param>
        /// <param name="CharacterClient"></param>
        public static void SendTextBox(string Header, string Msg, Client CharacterClient)
        {
#if SERVER
            LuaGame.SendDirectChatMessage(Header, Msg, null, ChatMessageType.MessageBox, CharacterClient);
#elif CLIENT
            GUI.AddMessage(Msg, Color.White);
#endif
        }

        /// <summary>
        /// Shorthand for LuaCsLogger; prints a message into the console. Prefixed by [Neurotrauma C#].
        /// </summary>
        /// <param name="Message">The message to print.</param>
        public static void Print(string Message)
        {
            LuaCsLogger.Log("[Neurotrauma C#] " + Message);
        }

        /// <summary>
        /// Shorthand for LuaCsLogger; prints a red message into the console. Prefixed by [Neurotrauma C#].
        /// </summary>
        /// <param name="Message">The message to print.</param>
        public static void PrintError(string Message)
        {
            LuaCsLogger.Log("[Neurotrauma C#] " + Message, Color.Red);
        }

        /// <summary>
        /// Shorthand for LuaCsLogger; prints an orange message into the console. Prefixed by [Neurotrauma C#].
        /// </summary>
        /// <param name="Message">The message to print.</param>
        public static void PrintWarning(string Message)
        {
            LuaCsLogger.Log("[Neurotrauma C#] " + Message, Color.Orange);
        }

        /// <summary>
        /// Shorthand for LuaCsLogger; prints a blue message into the console. Prefixed by [Neurotrauma C#].
        /// </summary>
        /// <param name="Message">The message to print.</param>
        public static void PrintUtility(string Message)
        {
            LuaCsLogger.Log("[Neurotrauma C#] " + Message, Color.SkyBlue);
        }

        /// <summary>
        /// Gets the skill level of a character for a certain skill-type.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <param name="SkillType">The skill type to check.</param>
        /// <returns>The skill level if present, else 0.</returns>
        public static float GetSkillLevel(Character Character, Identifier SkillType)
        {
            return Character.GetSkillLevel(SkillType);
        }

        /// <summary>
        /// Gets the default skill level of a character for a certain skill-type.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <param name="SkillType">The skill type to check.</param>
        /// <returns>The skill level's default value.</returns>
        public static float GetBaseSkillLevel(Character Character, Identifier SkillType)
        {
            return Character.Info.Job.GetSkillLevel(SkillType);
        }

        /// <summary>
        /// Rums a chance function to see if a skillcheck is passed.
        /// </summary>
        /// <param name="Character">The character to check.</param>
        /// <param name="SkillType">The skill type to check.</param>
        /// <param name="RequiredAmount">The required amount of skill to guarantee a passed skillcheck.</param>
        /// <returns>True if the skill-check was passed, else False.</returns>
        public static bool GetSkillRequirementMet(Character Character, Identifier SkillType, float RequiredAmount)
        {
            float SkillLevel = GetSkillLevel(Character, SkillType);

            if (NTConfig.Get("NT_VanillaSkillCheck", false))
            {
                return Chance(Math.Clamp((100 - (RequiredAmount - SkillLevel)) / 100, 0, 1));
            }

            return Chance(Math.Clamp(SkillLevel / RequiredAmount, 0, 1));
        }

        // For performances reason the check is cached in NTInfo to avoid looping all the time
        // If you dont like it idc change it yourself
        public static bool IsNTSPEnabled()
        {
            return NTInfo.NTSPEnabled;
        }

        public static float GetSurgerySkill(Character Character)
        {
            // TODO: NTSP integration
            // if (NTSP != null && NTConfig.Get("NTSP_enableSurgerySkill", false))
            //     return Math.Max(5, GetSkillLevel(Character, "surgery"), GetSkillLevel(Character, "medical") / 4);

            if (IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", false)) 
            { 
                return Math.Max(GetSkillLevel(Character, "surgery"), GetSkillLevel(Character, "medical") / 4);
            }
            
            return GetSkillLevel(Character, "medical");

        }

        public static bool GetSurgerySkillRequirementMet(Character Character, float RequiredAmount)
        {
            float SkillLevel = GetSurgerySkill(Character);

            if (NTConfig.Get("NT_vanillaSkillCheck", false))
                return Chance(Math.Clamp((100 - (RequiredAmount - SkillLevel)) / 100, 0, 1));

            return Chance(Math.Clamp(SkillLevel / RequiredAmount, 0, 1));
        }

        public static void GiveSkill(Character Character, Identifier SkillType, float Amount)
        {
            Character.Info.IncreaseSkillLevel(SkillType, Amount);
        }

        public static void GiveSurgerySkill(Character Character, float Amount)
        {
            Character.Info.IncreaseSkillLevel("surgery", Amount);
        }

        public static void GiveSkillScaled(Character Character, Identifier SkillType, float Amount)
        {
            GiveSkill(Character, SkillType, (float)(Amount * 0.001 / Math.Max(GetSkillLevel(Character, SkillType), 1)));
        }

        public static bool HasAbilityFlag(Character Character, AbilityFlags FlagType)
        {
            return Character.HasAbilityFlag(FlagType);
        }

        public static Vector2 GetVelocity(Character Character)
        {
            if (Character == null || Character.AnimController == null || Character.AnimController.MainLimb == null || Character.AnimController.MainLimb.body == null) { return new Vector2(0, 0); }
            return Character.AnimController.MainLimb.body.LinearVelocity;
        }

        public static bool HasTalent(Character Character, string Talent)
        {
            return Character.HasTalent(Talent);
        }

        public static float CharacterDistance(Character Character1, Character Character2)
        {
            Vector2 Pos1 = Character1.WorldPosition;
            Vector2 Pos2 = Character2.WorldPosition;
            return Vector2.Distance(Pos1, Pos2);
        }


        public static string FirstCharToUpper(string Input)
        {
            string UpperFirst = Input[0].ToString().ToUpper();
            string LowerLast = Input[1..];
            return UpperFirst + LowerLast;
        }

        public static bool UsingAddons()
        {
            return UsingLuaAddons() || UsingCSAddons();
        }

        public static bool UsingLuaAddons()
        {
            return NTInfo.LuaRegisteredAddons.Count > 0;
        }

        public static bool UsingLuaAddon(string ModName)
        {
            return NTInfo.LuaRegisteredAddons.ContainsKey(ModName);
        }

        public static bool UsingCSAddons()
        {
            return NTInfo.RegisteredAddons.Count > 0;
        }

        public static bool UsingCSAddon(string ModName)
        {
            return NTInfo.RegisteredAddons.ContainsKey(ModName);
        }

        public static double AffClamp(double Value, NTAffliction Aff)
        {
            return Math.Clamp(Value, Aff.MinStrength, Aff.MaxStrength);
        }

        public static double AffClamp(double Value, NTNonLimbAffliction Aff)
        {
            return Math.Clamp(Value,Aff.MinStrength,Aff.MaxStrength);
        }

        public static double AffClamp(double Value, NTLimbAffliction Aff)
        {
            return Math.Clamp(Value, Aff.MinStrength, Aff.MaxStrength);
        }

        public static double AffClamp(double Value, NTBloodAffliction Aff)
        {
            return Math.Clamp(Value, Aff.MinStrength, Aff.MaxStrength);
        }

        public static double AffClamp(double Value, NTSymptom Aff)
        {
            return Math.Clamp(Value, Aff.MinStrength, Aff.MaxStrength);
        }

        public static double AffClamp(double Value, NTLimbSymptom Aff)
        {
            return Math.Clamp(Value, Aff.MinStrength, Aff.MaxStrength);
        }

        // ---------------------------------------- Affliction Related Helper Functions -------------------------------------------------- \\
        public static bool HasAffliction(Character Character, string Identifier = "", float MinAmount = 0)
        {
            if (Identifier == "" || Character.CharacterHealth == null) { return false; }
            Affliction Aff = GetAffliction(Character, Identifier);
            if (Aff == null) { return false; } // Is the affliction null?
            float AffStrength = Aff.Strength;
            if (AffStrength > MinAmount)
            {
                return true;
            }
            return false;
        }

        public static bool HasAfflictionLimb(Character Character, string Identifier = "", LimbType GivenLimbType = LimbType.Torso, float MinAmount = 0)
        {
            if (Identifier == "" || Character.CharacterHealth == null) { return false; }
            Affliction Aff = GetAfflictionLimb(Character, Identifier, GivenLimbType);
            if (Aff == null) { return false; } // Is the affliction null?
            float AffStrength = Aff.Strength;
            if (AffStrength >= MinAmount)
            {
                return true;
            }
            return false;
        }

        public static bool HasAfflictionExtremity(Character Character, string Identifier = "", LimbType GivenLimbType = LimbType.Torso, double MinAmount = 0.5)
        {
            Affliction Aff = null;
            List<List<LimbType>> LocalLimbsToCheck = [[LimbType.LeftArm, LimbType.LeftForearm, LimbType.LeftHand],[LimbType.RightArm, LimbType.RightForearm, LimbType.RightHand],
                                                        [LimbType.LeftLeg, LimbType.LeftThigh, LimbType.LeftFoot],[LimbType.RightLeg, LimbType.RightThigh, LimbType.RightFoot]];

            foreach (List<LimbType> SubList in LocalLimbsToCheck)
            {
                if (NormalizeLimbType(GivenLimbType) == SubList[0])
                {
                    Aff = GetAfflictionLimb(Character, Identifier, SubList[0]);
                    if (Aff == null)
                    {
                        Aff = GetAfflictionLimb(Character, Identifier, SubList[1]);
                    }
                    if (Aff == null)
                    {
                        Aff = GetAfflictionLimb(Character, Identifier, SubList[2]);
                    }
                    break; // We can end the for loop, we found what we were looking for.
                }
            }
            bool Res = false;
            if (Aff != null)
            {
                Res = Aff.Strength >= MinAmount;
            }
            return Res;
        }

        public static Affliction GetAffliction(Character Character, String Identifier = "")
        {
            return Character.CharacterHealth.GetAffliction(Identifier); // No error handling on this one, gonna need someone smarter to do that.
        }

        public static Affliction GetAfflictionLimb(Character Character, String Identifier = "", LimbType GivenLimbType = LimbType.Torso)
        {
            if (Character == null || Character.CharacterHealth == null) return null;
            return Character.CharacterHealth.GetAffliction(Identifier, GetCharacterLimb(Character, GivenLimbType));
        }

        // Previous iteration was broken - Lukako
        public static float GetAfflictionStrength(Character Character, string Identifier = "", float DefaultValue = 0)
        {
            Affliction aff = GetAffliction(Character, Identifier);

            if (aff == null)
            {
                return DefaultValue;
            }

            return aff.Strength;
        }

        // Previous iteration was broken - Greabb
        public static float GetAfflictionStrengthLimb(Character Character, LimbType GivenLimbType = LimbType.Torso, string Identifier = "", float DefaultValue = 0)
        {
            Affliction aff = GetAfflictionLimb(Character, Identifier, GivenLimbType);

            if (aff == null)
            {
                return DefaultValue;
            }

            return aff.Strength;
        }

        public static void AddAffliction(Character Character, string Identifier, float Strength, Character Aggressor = null)
        {
            if (Aggressor == null)
            {
                Aggressor = Character;
            }

            float PrevStrength = GetAfflictionStrength(Character, Identifier, 0);
            SetAffliction(Character, Identifier, Strength + PrevStrength, Aggressor, PrevStrength);
        }

        public static void SetAffliction(Character Character, string Identifier, float Strength, Character Aggressor = null, float PreviousStrength = 0)
        {
            if (Aggressor == null)
            {
                Aggressor = Character;
            }
                
            SetAfflictionLimb(Character, Identifier, LimbType.Torso, Strength, Aggressor, PreviousStrength);
        }

        public static void SetAfflictionLimb(Character Character, string Identifier, LimbType GivenLimbType, float Strength, Character Aggressor = null, float PrevStrength = 0)
        {
            // This Error was in the original but not ported for some reason?
            if (!AfflictionPrefab.Prefabs.TryGet(Identifier, out AfflictionPrefab Prefab) || Prefab == null || Character == null)
            {
                LuaCsLogger.LogError(string.Format(
                    "Can't apply affliction to character limb\ncharacter = {0}, limbtype = {1}, affliction = {2}, strength = {3}",
                    Character != null ? Character.Name : "nil",
                    GivenLimbType.ToString(),
                    Prefab != null ? $"{Prefab.Name} ({Prefab.Identifier})" : Identifier ?? "nil",
                    Strength.ToString("F3")
                ));
                return;
            }

            float Resistance = Character.CharacterHealth.GetResistance(Prefab, GivenLimbType);
            if (Resistance >= 1) 
            { 
                return; 
            }

            // Flip the resistances effects so we get the right values accounting for them
            float ScaledStrength = Strength * Character.CharacterHealth.MaxVitality / 100 / (1 - Resistance);
            Affliction Affliction = Prefab.Instantiate(ScaledStrength, Aggressor);
            bool RecalculateVitality = NTC.AfflictionsAffectingVitality.Contains(Identifier);

            Character.CharacterHealth.ApplyAffliction(
                Character.AnimController.GetLimb(GivenLimbType),
                Affliction,
                false,
                false,
                true
            );
        }

        public static void AddAfflictionLimb(Character Character, string Identifier, LimbType GivenLimbType, float Strength, Character Aggressor = null)
        {
            if (Aggressor == null) Aggressor = Character;

            if (Strength < 0)
            {
                Character.CharacterHealth.ReduceAfflictionOnLimb(
                    GetCharacterLimb(Character, GivenLimbType),
                    Identifier,
                    -Strength,
                    null,
                    Aggressor);
                return;
            }

            if (!AfflictionPrefab.Prefabs.TryGet(Identifier, out AfflictionPrefab Prefab) || Prefab == null) { return; }

            float Resistance = Character.CharacterHealth.GetResistance(Prefab, GivenLimbType);

            if (Resistance >= 1) { return; }

            float ScaledStrength = Strength * Character.CharacterHealth.MaxVitality / 100 / (1 - Resistance);
            Affliction Affliction = Prefab.Instantiate(ScaledStrength, Aggressor);
            bool RecalculateVitality = NTC.AfflictionsAffectingVitality.Contains(Identifier);

            // No need to manually calculate strength, just stack it - Lukako
            Character.CharacterHealth.ApplyAffliction(
                Character.AnimController.GetLimb(GivenLimbType),
                Affliction,
                true,
                false,
                RecalculateVitality
            );
        }

        

        public static void AddAfflictionResisted(Character Character, string Identifier, float Strength, Character Aggressor = null)
        {
            if (Aggressor == null) Aggressor = Character;

            float PrevStrength = GetAfflictionStrength(Character, Identifier);
            Strength *= 1 - GetResistance(Character, Identifier);
            SetAffliction(Character, Identifier, Strength + PrevStrength, Aggressor, PrevStrength);
        }

        public static void ApplyAfflictionChange(Character Character, string Identifier, float Strength, float PrevStrength, float MinStrength, float MaxStrength)
        {
            Strength = Math.Clamp(Strength, MinStrength, MaxStrength);
            PrevStrength = Math.Clamp(PrevStrength, MinStrength, MaxStrength);
            if (PrevStrength != Strength)
            {
                SetAffliction(Character, Identifier, Strength, Character, Strength);
            }
        }

        public static void ApplyAfflictionChangeLimb(Character Character, LimbType GivenLimbType, string Identifier, float Strength, float PrevStrength, float MinStrength, float MaxStrength)
        {
            Strength = Math.Clamp(Strength, MinStrength, MaxStrength);
            PrevStrength = Math.Clamp(PrevStrength, MinStrength, MaxStrength);
            if (PrevStrength != Strength)
            {
                SetAfflictionLimb(Character, Identifier, GivenLimbType, Strength, Character, Strength);
            }
        }

        public static void ApplySymptom(Character Character, string Identifier, bool HasSymptom, bool RemoveIfNot)
        {
            if (!HasSymptom && !RemoveIfNot)
            {
                return;
            }

            float Strength = 0;
            if (HasSymptom) { Strength = 100; }
            if (RemoveIfNot || HasSymptom)
            {
                SetAffliction(Character, Identifier, Strength, Character, Strength);
            }
        }
        public static void ApplySymptomLimb(Character Character, LimbType GivenLimbType, string Identifier, bool HasSymptom, bool RemoveIfNot)
        {
            if (!HasSymptom && !RemoveIfNot)
            {
                return;
            }

            float Strength = 0;
            if (HasSymptom) { Strength = 100; }
            if (RemoveIfNot || HasSymptom)
            {
                SetAfflictionLimb(Character, Identifier, GivenLimbType, Strength, Character, Strength);
            }
        }

        // Needed for some slow-acting Consumables. - Lukako
        /// <summary>
        /// Applies a given Affliction over a given Duration.
        /// </summary>
        /// <param name="Target">Character to apply the affliction to.</param>
        /// <param name="Identifier">Identifier of the affliction to apply.</param>
        /// <param name="TotalAmount">Total combined strength of applications.</param>
        /// <param name="Duration">Amount of time in seconds over which the application happens.</param>
        /// <param name="Aggressor">Character that's considered the one applying the affliction.</param>
        public static void ApplyAfflictionOverTime(Character Target, string Identifier, float TotalAmount, float Duration, Character Aggressor = null)
        {
            if (Aggressor == null) Aggressor = Target;

            int Applications = (int)Duration;
            if (Applications <= 0 || Target == null) return;

            float AfflictionAmountPerApplication = TotalAmount / Applications;

            void ApplyAfflictionAgain(int RemainingApplications)
            {
                if (RemainingApplications <= 0 || Target == null || Target.Removed) return;

                HF.AddAffliction(Target, Identifier, AfflictionAmountPerApplication, Aggressor);

                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    ApplyAfflictionAgain(RemainingApplications - 1);
                }, 1000);
            }
            // Start the loop
            ApplyAfflictionAgain(Applications);
        }

        // ---------------------------------------- Specific Affliction Helper Functions -------------------------------------------------- \\

        public static void DislocateLimb(Character Character, LimbType GivenLimbType, float Strength = 1) // UNUSED?
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            AddAfflictionLimb(Character, "dislocation", GivenLimbType, Strength, Character);
        }

        public static void BreakLimb(Character Character, LimbType GivenLimbType, float Strength = 1)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);

            if (GivenLimbType == LimbType.Head)
            {
                AddAfflictionLimb(Character, "fracturedskull", GivenLimbType, Strength, Character);
            } else if (GivenLimbType == LimbType.Torso) {
                AddAfflictionLimb(Character, "fracturedribs", GivenLimbType, Strength, Character);
            } else
            {
                AddAfflictionLimb(Character, "fracturedextremity", GivenLimbType, Strength, Character);

                if (Strength > 0 && NTConfig.Get("NT_fracturesRemoveCasts", true))
                {
                    SetAfflictionLimb(Character, "plastercast", GivenLimbType, 0);
                }
            }
        }
        public static void ArteryCutLimb(Character Character, LimbType GivenLimbType, float Strength = 1)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            AddAfflictionLimb(Character, "arterialcut", GivenLimbType, Strength, Character);
        }

        public static bool LimbIsDislocated(Character Character, LimbType GivenLimbType, bool IsArm)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            if (IsArm) { return HasAfflictionLimb(Character, "dislocation", GivenLimbType, 100); }
            return HasAfflictionLimb(Character, "dislocation", GivenLimbType);
        }

        public static bool LimbIsBroken(Character Character, LimbType GivenLimbType, bool IsArm)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            if (IsArm) { return HasAfflictionLimb(Character, "fracturedextremity", GivenLimbType, 100); }
            return HasAfflictionLimb(Character, "fracturedextremity", GivenLimbType);
        }

        public static bool LimbIsArterialCut(Character Character, LimbType GivenLimbType)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            return HasAfflictionLimb(Character, "arterialcut", GivenLimbType);
        }

        public static bool LimbIsTraumaticallyAmputated(Character character, LimbType givenLimbType)
        {
            givenLimbType = NormalizeLimbType(givenLimbType);

            return HasAffliction(character, "t" + CreateLimbAfflictionID(givenLimbType, "amputation"));
        }

        public static bool LimbIsSurgicallyAmputated(Character character, LimbType givenLimbType)
        {
            givenLimbType = NormalizeLimbType(givenLimbType);

            return HasAffliction(character, "s" + CreateLimbAfflictionID(givenLimbType, "amputation"));
        }

        public static bool LimbIsAmputated(Character Character, LimbType GivenLimbType)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            return LimbIsSurgicallyAmputated(Character, GivenLimbType) || LimbIsTraumaticallyAmputated(Character, GivenLimbType);
        }

        public static void TraumamputateLimbAndGenerateItem(Character Character, LimbType GivenLimbType, Character Attacker)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            if (!LimbsToCheck.Contains(GivenLimbType)) { return; }
            Dictionary<LimbType, string> LimbToAffliction = new Dictionary<LimbType, string>() { {LimbType.RightLeg,"gate_ta_rl" }, { LimbType.LeftLeg, "gate_ta_ll" },
                                                                                                { LimbType.RightArm, "gate_ta_ra" },{ LimbType.LeftArm, "gate_ta_la" },
                                                                                                { LimbType.Head, "gate_ta_h" } };
            Dictionary<LimbType, string> LimbToItem = new Dictionary<LimbType, string>() { {LimbType.RightLeg,"rleg" }, { LimbType.LeftLeg, "lleg" },
                                                                                                { LimbType.RightArm, "rarm" },{ LimbType.LeftArm, "larm" },
                                                                                                { LimbType.Head, "headta" } };
            LimbToAffliction.TryGetValue(GivenLimbType, out string Value);
            LimbToItem.TryGetValue(GivenLimbType, out string Value2);
            string Aff = Value;
            string LimbItem = Value2;
            if (!Attacker.IsHuman && !Attacker.Inventory.IsFull())
            {
                GiveItem(Attacker, LimbToAffliction[GivenLimbType]);
                AddAfflictionLimb(Character, LimbToAffliction[GivenLimbType] + "_2", GivenLimbType, 10, Attacker);
            }
            else
            {
                AddAfflictionLimb(Character, LimbToAffliction[GivenLimbType] + "_2", GivenLimbType, 10, Character);
            }
        }

        public static void TraumamputateLimb(Character Character, LimbType GivenLimbType, Character Attacker)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            if (!LimbsToCheck.Contains(GivenLimbType)) { return; }
            Dictionary<LimbType, string> LimbToAffliction = new Dictionary<LimbType, string>() { {LimbType.RightLeg,"gate_ta_rl" }, { LimbType.LeftLeg, "gate_ta_ll" },
                                                                                                { LimbType.RightArm, "gate_ta_ra" },{ LimbType.LeftArm, "gate_ta_la" },
                                                                                                { LimbType.Head, "gate_ta_h" } };
            Dictionary<LimbType, string> LimbToItem = new Dictionary<LimbType, string>() { {LimbType.RightLeg,"rleg" }, { LimbType.LeftLeg, "lleg" },
                                                                                                { LimbType.RightArm, "rarm" },{ LimbType.LeftArm, "larm" },
                                                                                                { LimbType.Head, "headta" } };
            LimbToAffliction.TryGetValue(GivenLimbType, out string Value);
            LimbToItem.TryGetValue(GivenLimbType, out string Value2);
            string Aff = Value;
            string LimbItem = Value2;
            AddAfflictionLimb(Character, LimbToAffliction[GivenLimbType] + "_2", GivenLimbType, 10, Character);
        }

        /// <summary>
        /// Removes an extremity by applying the Surgical Amputation affliction respective to that limb and spawning the respective transplant item.
        /// </summary>
        /// <param name="usingCharacter">Character performing the surgery.</param>
        /// <param name="targetCharacter">Character recieving the surgery.</param>
        /// <param name="limbType">Limb being surgically removed.</param>
        public static void SurgicallyAmputateLimbAndGenerateItem(Character usingCharacter, Character targetCharacter, LimbType limbType)
        {
            limbType = NormalizeLimbType(limbType);

            Item prevItem = GetItemInHeadWear(targetCharacter);

            if (prevItem != null && limbType == LimbType.Head) prevItem.Drop(usingCharacter, true);

            bool dropLimb = !LimbIsAmputated(targetCharacter, limbType) && !HasAfflictionLimb(targetCharacter, "gangrene", limbType, 15);

            SurgicallyAmputateLimb(targetCharacter, limbType);

            if (!dropLimb) return;

            Dictionary<LimbType, string> limbToItem = new()
            {
                { LimbType.RightLeg, "rleg" },
                { LimbType.LeftLeg, "lleg" },
                { LimbType.RightArm, "rarm" },
                { LimbType.LeftArm, "larm" },
                { LimbType.Head, "headsa" }
            };

            if (limbToItem.TryGetValue(limbType, out string itemIdentifier))
            {
                GiveItem(usingCharacter, itemIdentifier);
                GiveSurgerySkill(usingCharacter, 0.5f);
            }
        }

        /// <summary>
        /// Removes an extremity by applying the Surgical Amputation affliction respective to that limb.
        /// </summary>
        /// <param name="usingCharacter">Character performing the surgery.</param>
        /// <param name="targetCharacter">Character recieving the surgery.</param>
        /// <param name="limbType">Limb being surgically removed.</param>
        public static void SurgicallyAmputateLimb(Character character, LimbType GivenLimbType, float strength = 100f, float traumampStrength = 0f)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);

            string baseId = CreateLimbAfflictionID(GivenLimbType, "amputation");

            SetAffliction(character, "s" + baseId, strength, null, 0);

            if (traumampStrength > 0f)
            {
                SetAffliction(character, "t" + baseId, traumampStrength, null, 0);
            }

            SetAfflictionLimb(character, "gangrene", GivenLimbType, 0, null, 0);
        }

        /// <summary>
        /// Checks if Surgery can be performed on a character.
        /// </summary>
        /// <param name="Character">The character that gets checked for Surgery Viability.</param>
        /// <returns>'True' if Surgery can be performed; otherwise 'False'.</returns>
        public static bool CanPerformSurgeryOn(Character Character)
        {
            return HasAffliction(Character, "analgesia", 1) || HasAffliction(Character, "unconsciousness", (float).1);
        }

        public static void Fibrillate(Character Character, float Amount)
        {
            // tachycardia (increased heartrate) ->
            // fibrillation(irregular heartbeat)->

            // cardiacarrest

            float Tachycardia = GetAfflictionStrength(Character, "tachycardia");
            float Fibrillation = GetAfflictionStrength(Character, "fibrillation");
            float CardiacArrest = GetAfflictionStrength(Character, "cardiacarrest");

            // Already in cardiac arrest? Don't do anything.
            if (CardiacArrest > 0) { return; }
            float PreviousAmount = Tachycardia / 5;
            if (Fibrillation > 0) { PreviousAmount = 20 + Fibrillation; }
            float NewAmount = PreviousAmount + Amount;

            //0 - 20: 0 - 100 % tachycardia
            // 20 - 120: 0 - 100 % fibrillation
            // > 120: cardiac arrest

            if (NewAmount < 20)
            {
                Tachycardia = NewAmount * 5;
                Fibrillation = 0;
            }
            else
            {
                if (NewAmount < 120)
                {
                    Tachycardia = 0;
                    Fibrillation = NewAmount - 20;
                }
                else
                {
                    Tachycardia = 0;
                    Fibrillation = 0;
                    SetAffliction(Character, "cardiacarrest", 10, Character, CardiacArrest);
                }
            }

            SetAffliction(Character, "tachycardia", Tachycardia, Character, 0);
            SetAffliction(Character, "fibrillation", Fibrillation, Character, 0);
        }

        public static bool LimbLockedInitial(HumanUpdate.NTHuman C, LimbType Limb, string Key)
        {
            return (!NTC.HasSymptomFalse(C, Key))
                   && (
                       NTC.HasSymptom(C, Key)
                || LimbIsAmputated(C.Human, Limb)
                       || (GetAfflictionStrengthLimb(C.Human, Limb, "bandaged") <= 0 && GetAfflictionStrengthLimb(
                           C.Human,
                           Limb,
                           "bandageddirty"
                           ) <= 0 && GetAfflictionStrength(C.Human, "afadrenaline") <= 0 && LimbIsDislocated(
                                C.Human,
                                Limb,
                                Limb == LimbType.LeftArm || Limb == LimbType.RightArm
                           )
                )
                || (
                            GetAfflictionStrengthLimb(C.Human, Limb, "gypsumcast") <= 0
                            && GetAfflictionStrength(C.Human, "afadrenaline") <= 0
                            && LimbIsBroken(
                                C.Human,
                                Limb,
                                Limb == LimbType.LeftArm || Limb == LimbType.RightArm
                         )
                    )
                );
        }

        public static double OrganDamageCalc(HumanUpdate.NTHuman C, double DamageValue, bool NoMaxStrength = false)
        {
            if (DamageValue >= 99 && !(NoMaxStrength)) return 100;
            return DamageValue - 0.01 * C.GetDoubleStatStrength("healingrate") * C.GetDoubleStatStrength("specificOrganDamageHealMultiplier") * NT.DeltaTime;
        }

        public static double KidneyDamageCalc(HumanUpdate.NTHuman C, double DamageValue)
        {
            if (DamageValue >= 99) return 100;
            if (DamageValue >= 50)
            {
                if (DamageValue <= 51) return DamageValue;
                return DamageValue - 0.01 * C.GetDoubleStatStrength("healingrate") * C.GetDoubleStatStrength("specificOrganDamageHealMultiplier") * NT.DeltaTime;
            }
            return DamageValue - 0.02 * C.GetDoubleStatStrength("healingrate") * C.GetDoubleStatStrength("specificOrganDamageHealMultiplier") * NT.DeltaTime;
        }

        // ---------------------------------------- Client Related Helper Functions -------------------------------------------------- \\

        // Both these functions were returning null. - Lukako
        public static Client CharacterToClient(Character character)
        {
#if SERVER
            foreach (Client client in GameMain.Server.ConnectedClients)
            {
                if (client.Character == character)
                {
                    return client;
                }
            }
#endif

            return null;
        }

        public static Client ClientFromName(string Name)
        {
#if SERVER
            foreach (Client client in GameMain.Server.ConnectedClients)
            {
                if (client.Name == Name)
                {
                    return client;
                }
            }
#endif

            return null;
        }

        public static Dictionary<string, bool> DynamicUnavailableItems()
        {
            Dictionary<string, bool> blockedItems = new Dictionary<string, bool>();

            // Hardmode Aortic Rupture; enable/disable stent + balloon
            if (!NTConfig.Get("NT_HardmodeAorticRupture", false))
            {
                blockedItems["medstent"] = true;
                blockedItems["endovascballoon"] = true;
            }

            // Sodium Nitroprusside
            if (!NTConfig.Get("NT_DoNitroprusside", false))
            {
                blockedItems["pressuremeds"] = true;
            }

            // Organ scalpels
            if (!NTConfig.Get("NT_DoOrganScalpels", false))
            {
                blockedItems["organscalpel_liver"] = true;
                blockedItems["organscalpel_kidneys"] = true;
                blockedItems["organscalpel_heart"] = true;
                blockedItems["organscalpel_lungs"] = true;
                blockedItems["organscalpel_brain"] = true;
                blockedItems["surgerytoolboxsetscalpel"] = true;
            }

            return blockedItems;
        }
    }
}