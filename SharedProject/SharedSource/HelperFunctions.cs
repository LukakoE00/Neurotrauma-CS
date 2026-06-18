
using Barotrauma;
using Barotrauma.Networking;
using FluentResults;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Text;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
// What shit was Tina/Mannatu smoking. How do you write this many functions. - GreenBean
// The majority of these functions are direct translations into C#. Due to the fact that most of the C# methods were already exposed in lua, we can easily do this.

// Also due to this fact of being in C#, many functions are now irrelevant.
// Lerp (Rest in Peace lil bro, you were the king).
// Clamp (My goat).
// Minimum
// Maximum
// Distance Between


namespace Neurotrauma
{

    public static class HF // The HelperFunctions class
    {
        // ---------------------------------------- Limb Related Helper Functions -------------------------------------------------- \\

        public static readonly List<LimbType> ArmsLegsToCheck = [LimbType.LeftArm, LimbType.RightArm, LimbType.LeftLeg, LimbType.RightLeg];
        public static readonly List<LimbType> LegsToCheck = [LimbType.LeftLeg, LimbType.RightLeg];
        public static readonly List<LimbType> ArmsToCheck = [LimbType.LeftArm, LimbType.RightArm];
        public static readonly List<LimbType> LimbsToCheck = [LimbType.LeftArm, LimbType.RightArm, LimbType.LeftLeg, LimbType.RightLeg, LimbType.Torso, LimbType.Head];
        
        // GreenBean fucked this one up! - Lukako
        public static readonly Dictionary<LimbType, string> StringLimbsToCheck = new Dictionary<LimbType, string>() 
        {
            { LimbType.LeftArm, "LeftArm" }, { LimbType.RightArm, "RightArm" },
            { LimbType.LeftLeg, "LeftLeg" }, { LimbType.RightLeg, "RightLeg" },
            { LimbType.Torso, "Torso" }, { LimbType.Head, "Head" }
        };

        public static readonly Dictionary<LimbType, string> ShortHandLimbsToCheck = new Dictionary<LimbType, string>() 
        {
            { LimbType.LeftArm, "la" }, { LimbType.RightArm, "ra" },
            { LimbType.LeftLeg, "ll" }, { LimbType.RightLeg, "rl" },
            { LimbType.Torso, "t" }, { LimbType.Head, "h" }
        };

        public static readonly Dictionary<LimbType, double> DefaultLimbAffStrengths = new Dictionary<LimbType, double>() 
        { 
            { LimbType.Head, 0 }, { LimbType.Torso, 0 }, 
            { LimbType.LeftArm, 0 }, { LimbType.RightArm, 0 }, 
            { LimbType.LeftLeg, 0 }, { LimbType.RightLeg, 0 } 
        };

        public static Limb GetCharacterLimb(Character Character, LimbType GivenLimbType)
        {
            return Character.AnimController.GetLimb(GivenLimbType);
        }

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

        public static string LimbToString(LimbType GivenLimbType)
        {
            LimbType NormalizedLimb = NormalizeLimbType(GivenLimbType);
            StringLimbsToCheck.TryGetValue(NormalizedLimb, out string Value);
            return Value;
        }

        public static string CreateLimbAfflictionID(LimbType GivenLimbType, string Identifier, string FailSafe)
        {
            LimbType NormalizedLimb = NormalizeLimbType(GivenLimbType);
            ShortHandLimbsToCheck.TryGetValue(NormalizedLimb, out string Value);
            return (Value + "_" + Identifier) ?? FailSafe;
        }

        public static bool LimbIsExtremity(LimbType GivenLimbType)
        {
            return GivenLimbType != LimbType.Torso && GivenLimbType != LimbType.Head;
        }

        public static void ForceArmLock(Character Character, string Identifier) // This took me an hour to translate btw lol
        {
            // HostSide Only
#if SHARED
#if CLIENT
            if (LuaGame.IsMultiplayer())
            {
                return;
            }
#endif
            if (Entity.Spawner == null)
            {
                LuaCsTimer.Wait((params object[] _) => { ForceArmLock(Character, Identifier); }, 35);
                return;
            }

            int HandIndex = 6;
            if (Identifier == "armlock2") {HandIndex = 5;}
            Item PrevItem = Character.Inventory.GetItemAt(HandIndex)
            if (PrevItem != null) { PrevItem.Drop(Character,true);}

            LuaCsTimer.Wait((params object[] _) => 
            { 
            ItemPrefab IPrefab = ItemPrefab.GetItemPrefab(Identifier);
            Entity.Spawner.AddItemToSpawnQueue(IPrefab, Character.WorldPosition, null, null, (Item) => {
                                                                                                            if (Character.Inventory != null && Identifier == "armlock1") 
                                                                                                            {
                                                                                                                Character.Inventory.TryPutItem(Item, null, [InvSlotType.RightHand]); 
                                                                                                            }
                                                                                                            if (Character.Inventory != null && Identifier == "armlock2") 
                                                                                                            {
                                                                                                                Character.Inventory.TryPutItem(Item, null, [InvSlotType.LeftHand]); 
                                                                                                            }
                                                                                                        }
            );
            }, 35);
            return;
#endif
        }

        // ---------------------------------------- Utility Related Helper Functions -------------------------------------------------- \\

        public static float GetResistance(Character Character, string Identifier, LimbType GivenLimbType = LimbType.Torso) // Only returns health Resistance
        {
            AfflictionPrefab Prefab = AfflictionPrefab.Prefabs[Identifier];
            return Character.CharacterHealth.GetResistance(Prefab, GivenLimbType);
        }

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

        public static float FindDepth(Barotrauma.Item Item) // I butchered this function lol
        {
            return Item.WorldPosition.Y * Physics.DisplayToRealWorldRatio;
        }

        public static Barotrauma.Item GetItemInRightHand(Character Character)
        {
            return GetCharacterInventorySlot(Character, 6);
        }

        public static Barotrauma.Item GetItemInLeftHand(Character Character)
        {
            return GetCharacterInventorySlot(Character, 5);
        }

        public static Barotrauma.Item GetItemInOuterWear(Character Character)
        {
            return GetCharacterInventorySlot(Character, 4);
        }

        public static Barotrauma.Item GetItemInInnerWear(Character Character)
        {
            return GetCharacterInventorySlot(Character, 3);
        }

        public static Barotrauma.Item GetItemInHeadWear(Character Character)
        {
            return GetCharacterInventorySlot(Character, 2);
        }

        public static Barotrauma.Item GetCharacterInventorySlot(Character Character, int Slot)
        {
            return Character.Inventory.GetItemAt(Slot);
        }

        public static string GetCharacterInventorySlotIdentifer(Character Character, int Slot)
        {
            Barotrauma.Item Item = GetCharacterInventorySlot(Character, Slot);
            if (Item == null) { return "null"; }
            return Item.Prefab.Identifier.Value;
        }

        public static string GetOuterWearIdentifier(Character Character)
        {
            return GetCharacterInventorySlotIdentifer(Character, 4);
        }

        public static string GetInnerWearIdentifier(Character Character)
        {
            return GetCharacterInventorySlotIdentifer(Character, 3);
        }

        public static string GetHeadWearIdentifier(Character Character)
        {
            return GetCharacterInventorySlotIdentifer(Character, 2);
        }

        // You have no fucking clue how long it took me to figure this one out. - Lukako
        public static void DMClient(Client client, string message, Color? color)
        {
            var chatMessage = ChatMessage.Create("", message, ChatMessageType.Server, null);
            if (color.HasValue) chatMessage.Color = color.Value;

#if SERVER
            if (client == null) return;
            LuaGame.SendMessage(chatMessage.Text, ChatMessageType.Server, null);
#else
            if (GameMain.GameSession?.CrewManager != null)
            GameMain.GameSession.CrewManager.AddSinglePlayerChatMessage("", chatMessage.Text, ChatMessageType.Server, Character.Controlled);
#endif
        }

        // Pulls a ConfigData entry that holds an RGB value, and turns it into a color.
        // It's in here since the Hematology Analyzer and Health Scanner in Items ánd in Blood.cs use it.
        // It's like the table.concat but not; there has to be a better way for this. - Lukako
        public static Color GetColor(string key)
        {
            var rgbList = NTConfig.Get<List<string>>(key, null);
            if (rgbList == null || rgbList.Count == 0) return new Color(127, 255, 255);

            var rgbString = rgbList[0];
            var rgb = rgbString.Split(',');

            if (rgb.Length < 3) return new Color(127, 255, 255);

            return new Color(
                byte.Parse(rgb[0]),
                byte.Parse(rgb[1]),
                byte.Parse(rgb[2])
            );
        }

        public static bool Chance(float Chance)
        {
            float RandomChance = new Random().Range(0, 1);
            return RandomChance < Chance;
        }

        public static float BoolToNum(bool Value, float Out = 1)
        {
            if (Value) { return Out; }
            return 0;
        }

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

        public static bool InGame()
        {
#if SERVER
            return GameMain.Server.GameStarted;
            //return GameMain.GameSession.IsRunning;
#elif SHARED
            return LuaGame.RoundStarted;
#endif

            return GameMain.GameSession.IsRunning;
        }

        public static bool GameIsMultiplayer()
        {
            return GameMain.IsMultiplayer;
        }

        public static bool GameIsSingleplayer()
        {
            return GameMain.IsSingleplayer;
        }

        // This took me an hour to translate btw lol
        // Yeah add another hour cause it was BROKEN!!! - Lukako
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

        public static void GiveItemAtCondition(Character Character, string ItemIdentifier, float Condition) // DEPRECATED: Shouldn't be used, use GiveItem instead and the condition paramter.
        {
            GiveItem(Character, ItemIdentifier, Condition);
        }

        public static void SpawnItemPlusFunction(string ItemIdentifier, CharacterInventory Inventory, InvSlotType Slot, Vector2 Position, LuaCsAction Function, params object[] Parameters)
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

        public static void RemoveItem(Item item)
        {
            #if CLIENT
                if (HF.GameIsMultiplayer()) return;
            #endif

            if (item == null || item.Removed) return;

            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    RemoveItem(item);
                }, 35);

                return;
            }

            Entity.Spawner.AddEntityToRemoveQueue(item);
        }

        public static bool ItemHasTag(Item Item, string Tag)
        {
            return Item.HasTag(Tag);
        }

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

        public static void Explode(Entity GivenEntity, float Range = 0, float Force = 0, float Damage = 0, float StructureDamage = 0, float ItemDamage = 0, float EmpStrength = 0, float BallastFloraStrength = 0)
        {
            LuaGame.Explode(GivenEntity.WorldPosition, Range, Force, Damage, StructureDamage, ItemDamage, EmpStrength, BallastFloraStrength);
        }

        public static string GetText(Identifier Identifier)
        {
            LocalizedString Text = TextManager.Get(Identifier);
            return Convert.ToString(Text) ?? "";
        }

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

        public static void SendTextBox(string Header, string Msg, Client CharacterClient)
        {
#if SERVER
            LuaGame.SendDirectChatMessage(Header, Msg, null, ChatMessageType.MessageBox, CharacterClient);
#elif CLIENT
            GUI.AddMessage(Msg, Color.White);
#endif
        }

        public static void Print(string Message) // Yes I'm lazy
        {
            LuaCsLogger.Log("[Neurotrauma 2] " + Message);
        }

        public static void PrintError(string Message)
        {
            LuaCsLogger.Log("[Neurotrauma 2] " + Message, Color.Red);
        }

        public static void PrintWarning(string Message)
        {
            LuaCsLogger.Log("[Neurotrauma 2] " + Message, Color.Orange);
        }

        // ---------------------------------------- Character Related Helper Functions -------------------------------------------------- \\

        public static float GetSkillLevel(Character Character, Identifier SkillType)
        {
            return Character.GetSkillLevel(SkillType);
        }

        public static float GetSurgerySkill(Character Character)
        {
            return Character.GetSkillLevel("surgery");
        }

        public static float GetBaseSkillLevel(Character Character, Identifier SkillType)
        {
            return Character.Info.Job.GetSkillLevel(SkillType);
        }
        public static bool GetSkillRequirementMet(Character Character, Identifier SkillType, float RequiredAmount)
        {
            float SkillLevel = GetSkillLevel(Character, SkillType);
            // Need to implement our NTConfig part here.
            return Chance(Math.Clamp(SkillLevel / RequiredAmount, 0, 1));
        }
        public static bool GetSurgerySkillRequirementMet(Character Character, float RequiredAmount)
        {
            float SkillLevel = GetSkillLevel(Character, "surgery");
            // Need to implement our NTConfig part here.
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
            if (AffStrength > MinAmount)
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
            return Character.CharacterHealth.GetAffliction(Identifier, GetCharacterLimb(Character, GivenLimbType)); // No error handling on this one, gonna need someone smarter to do that.
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

        public static float GetAfflictionStrengthLimb(Character Character, LimbType GivenLimbType = LimbType.Torso, String Identifier = "", float DefaultValue = 0)
        {
            if (Identifier != "")  // Verify we have the info needed.
            {
                if (!HasAfflictionLimb(Character, Identifier, GivenLimbType)) { return DefaultValue; }
                float Strength = Character.CharacterHealth.GetAfflictionStrength(Identifier, GetCharacterLimb(Character, GivenLimbType), false);
                return Strength;
            }
            return DefaultValue;
        }

        public static void SetAffliction(Character Character, string Identifier, float Strength, Character Aggressor, float PreviousStrength)
        {
            SetAfflictionLimb(Character, Identifier, LimbType.Torso, Strength, Aggressor, PreviousStrength);
        }

        public static void SetAfflictionLimb(Character Character, string Identifier, LimbType GivenLimbType, float Strength, Character Aggressor, float PreviousStrength)
        {
            dynamic Check = AfflictionPrefab.Prefabs.TryGet(Identifier, out AfflictionPrefab Result); // Most likely a better way to acheive this but basically I don't know what this will return.
            if (Result == null) { return; }
            AfflictionPrefab Prefab = Result;

            float Resistance = Character.CharacterHealth.GetResistance(Prefab, GivenLimbType);
            if (Resistance > 1) { return; }

            Strength = Strength * Character.CharacterHealth.MaxVitality / 100 / (1 - Resistance);
            Affliction Affliction = Prefab.Instantiate(Strength, Aggressor);

            bool RecalculateVitality = NTC.AfflictionsAffectingVitality.Contains(Identifier);
            Character.CharacterHealth.ApplyAffliction(
                Character.AnimController.GetLimb(GivenLimbType),
                Affliction,
                false,
                false,
                RecalculateVitality
            );
        }

        public static void AddAfflictionLimb(Character Character, string Identifier, LimbType GivenLimbType, float Strength, Character Aggressor)
        {
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
            if (Resistance > 1) { return; }

            float ScaledStrength = Strength * Character.CharacterHealth.MaxVitality / 100 / (1 - Resistance);
            Affliction Affliction = Prefab.Instantiate(ScaledStrength, Aggressor);
            bool RecalculateVitality = NTC.AfflictionsAffectingVitality.Contains(Identifier);

            // No need to manually calculate strength, just stack it - Lukako
            Character.CharacterHealth.ApplyAffliction(
                Character.AnimController.GetLimb(GivenLimbType),
                Affliction,
                true, // allowStacking: true https://evilfactory.github.io/LuaCsForBarotrauma/lua-docs/code/characterhealth/#ApplyAffliction
                false,
                RecalculateVitality
            );
        }

        public static void AddAffliction(Character Character, string Identifier, float Strength, Character Aggressor)
        {
            AddAfflictionLimb(Character, Identifier, LimbType.Torso, Strength, Aggressor);
        }

        public static void AddAfflictionResisted(Character Character, string Identifier, float Strength, Character Aggressor)
        {
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
        public static void ApplyAfflictionOverTime(Character Target, string Identifier, float TotalAmount, float Duration, Character Aggressor)
        {
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
            AddAfflictionLimb(Character, "fracture", GivenLimbType, Strength, Character);
            // Implement the gypsum cast thing here.
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
            if (IsArm) { return HasAfflictionLimb(Character, "fracture", GivenLimbType, 100); }
            return HasAfflictionLimb(Character, "fracture", GivenLimbType);
        }

        public static bool LimbIsArterialCut(Character Character, LimbType GivenLimbType)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            return HasAfflictionLimb(Character, "arterialcut", GivenLimbType);
        }

        public static bool LimbIsTraumaticallyAmputated(Character Character, LimbType GivenLimbType)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            return HasAfflictionLimb(Character, CreateLimbAfflictionID(GivenLimbType, "amputation", "ll_amputation") + "t", GivenLimbType);
        }

        public static bool LimbIsSurgicallyAmputated(Character Character, LimbType GivenLimbType)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            return HasAfflictionLimb(Character, CreateLimbAfflictionID(GivenLimbType, "amputation", "ll_amputation") + "s", GivenLimbType);
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

        public static void SurgicallyAmputateLimbAndGenerateItem(Character UsingCharacter, Character TargetCharacter, LimbType GivenLimbType) // Holy mouth full
        {
            Item PrevItem = GetItemInHeadWear(TargetCharacter);
            if (PrevItem != null && GivenLimbType == LimbType.Head) { PrevItem.Drop(UsingCharacter, true); }
            bool DropLimb = !LimbIsAmputated(TargetCharacter, GivenLimbType) || !HasAfflictionLimb(TargetCharacter, "gangrene", GivenLimbType, 15);
            if (DropLimb)
            {
                Dictionary<LimbType, string> LimbToItem = new Dictionary<LimbType, string>() { {LimbType.RightLeg,"rleg" }, { LimbType.LeftLeg, "lleg" },
                                                                                                { LimbType.RightArm, "rarm" },{ LimbType.LeftArm, "larm" },
                                                                                                { LimbType.Head, "headsa" } };
                if (LimbToItem[GivenLimbType] != null)
                {
                    GiveItem(UsingCharacter, LimbToItem[GivenLimbType]);
                }
            }
        }

        public static void SurgicallyAmputateLimb(Character Character, LimbType GivenLimbType, float Strength = 100, float TraumampStrength = 0)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            SetAffliction(Character, CreateLimbAfflictionID(GivenLimbType, "amputation", "ll_amputation") + "s", Strength, Character, Strength);
            SetAffliction(Character, CreateLimbAfflictionID(GivenLimbType, "amputation", "ll_amputation") + "t", TraumampStrength, Character, TraumampStrength);
            SetAffliction(Character, "gangrene", 0, Character, 0);
        }

        public static bool CanPerformSurgeryOn(Character Character)
        {
            return HasAffliction(Character, "analgesia", 1) || HasAffliction(Character, "sym_unconsciousness", (float).1);
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
    }
}
