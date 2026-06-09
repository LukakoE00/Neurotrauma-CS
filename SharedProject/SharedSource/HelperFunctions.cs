
using Barotrauma;
using Barotrauma.Networking;
using FluentResults;
using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
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
        public static readonly Dictionary<LimbType, string> StringLimbsToCheck = new Dictionary<LimbType, string>() { 
                                                                                                                        { LimbType.LeftArm, "LeftArm" },{ LimbType.RightArm, "RightArm" },
                                                                                                                        { LimbType.LeftArm, "LeftLeg" },{ LimbType.RightArm, "RightLeg" },
                                                                                                                        { LimbType.Torso, "Torso" },{ LimbType.Head, "Head" }};
        public static readonly Dictionary<LimbType, string> ShortHandLimbsToCheck = new Dictionary<LimbType, string>() {
                                                                                                                        { LimbType.LeftArm, "la" },{ LimbType.RightArm, "ra" },
                                                                                                                        { LimbType.LeftArm, "ll" },{ LimbType.RightArm, "rl" },
                                                                                                                        { LimbType.Torso, "t" },{ LimbType.Head, "h" }};

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
            ShortHandLimbsToCheck.TryGetValue (NormalizedLimb, out string Value);
            return (Value + "_" + Identifier) ?? FailSafe;
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
                            string Afflictions = Element.GetAttributeString("afflictiontypes","").ToLower();
                            if (Afflictions.Contains(ResistanceID))
                            {
                                return (float) Convert.ToDouble(Element.GetAttributeString("damagemultiplier", "1"));
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

        public static List<string> EndocrineTalents = [
            "aggressiveengineering",
            "crisismanagement",
            "cannedheat",
            "doubleduty",
            "firemanscarry",
            "fieldmedic",
            "multitasker",
            "aceofalltrades",
            "stillkicking",
            "drunkensailor",
            "trustedcaptain",
            "downwiththeship",
            "physicalconditioning",
            "beatcop",
            "commando",
            "justascratch",
            "intheflow",
            "collegeathletics"];
        
        public static void ApplyEndocrineBoost(Character Character, List<string> TalentList = null)
        {
            // WIP
            TalentList = TalentList ?? EndocrineTalents;

            // gee I sure love translating lua into C#
            Character TargetCharacter = Character;
            if (TargetCharacter.Info == null) { return; }
            TalentTree TalentTree = TalentTree.JobTalentTrees[Character.Info.Job.Prefab.Identifier.Value];
            if (TalentTree == null) { return; }
            List<string> DisallowedTalents = new List<string>();
        }

        public static void PrintChat(string Message)
        {
        #if SERVER
            LuaGame.SendMessage(Message, ChatMessageType.Server, null);
        #endif
        
        }

        public static void DMClient(Client Client, string Message, Color Color)
        {
        #if SERVER
            ChatMessage ChatMsg = ChatMessage.Create("",Message,ChatMessageType.Server,null);
            ChatMsg.Color = Color;
            LuaGame.SendMessage(Message, ChatMessageType.Server, null);
        #endif

        }

        public static bool Chance(float Chance)
        {
            float RandomChance = new Random().Range(0, 1);
            return RandomChance < Chance;
        }

        public static float BoolToNum(bool Value, float Out = 1)
        {
            if (Value) { return  Out; }
            return 0;
        }

        public static bool GameIsPaused()
        {
        #if SERVER
            return false;
        #elif SHARED
            return LuaGame.Paused;
        #else
            return false;
        #endif
        }

        public static void GiveItem(Character Character, string ItemIdentifier) // This took me an hour to translate btw lol
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
                LuaCsTimer.Wait((params object[] _) => { GiveItem(Character, Item); }, 35); // Thanks Evil and Misinformation Factory
                return;
            }
            LuaCsTimer.Wait((params object[] _) => 
            { 
            ItemPrefab IPrefab = ItemPrefab.GetItemPrefab(ItemID);
            Entity.Spawner.AddItemToSpawnQueue(IPrefab, Character.WorldPosition, null, null, (Item) => { Character.Inventory.TryPutItem(Item, null, [InvSlotType.Any]); });
            }, 35);
            return;
#endif
        }

        // ---------------------------------------- Character Related Helper Functions -------------------------------------------------- \\

        public static float GetSkillLevel(Character Character, Identifier SkillType)
        {
            return Character.GetSkillLevel(SkillType);
        }

        public static float GetBaseSkillLevel(Character Character, Identifier SkillType)
        {
            return Character.Info.Job.GetSkillLevel(SkillType);
        }
        public static bool GetSkillRequirmentMet(Character Character, Identifier SkillType, float RequiredAmount)
        {
            float SkillLevel = GetSkillLevel(Character, SkillType);
            // Need to implement our NTConfig part here.
            return Chance(Math.Clamp(SkillLevel/RequiredAmount,0,1));
        }
        public static bool GetSurgerySkillRequirmentMet(Character Character, float RequiredAmount)
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
            GiveSkill(Character, SkillType, (float) (Amount * 0.001 / Math.Max(GetSkillLevel(Character,SkillType),1)));
        }

        // ---------------------------------------- Affliction Related Helper Functions -------------------------------------------------- \\
        public static bool HasAffliction(Character Character, string Identifier = "", float MinAmount = 0)
        {
            if (Identifier == "" || Character.CharacterHealth == null) { return false; }
            Affliction Aff = GetAffliction(Character,Identifier);
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
            Affliction Aff = GetAfflictionLimb(Character,Identifier,GivenLimbType);
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

        public static float GetAfflictionStrength(Character Character, String Identifier = "", float DefaultValue = 0)
        {
            if (Identifier != "")  // Verify we have the info needed.
            {
                if (!HasAffliction(Character, Identifier)) { return DefaultValue; }
                float Strength = Character.CharacterHealth.GetAfflictionStrength(Identifier, GetCharacterLimb(Character, LimbType.Torso), false);
                return Strength;
            }
            return DefaultValue;
        }

        public static float GetAfflictionStrengthLimb(Character Character, LimbType GivenLimbType = LimbType.Torso, String Identifier = "", float DefaultValue = 0)
        {
            if (Identifier != "")  // Verify we have the info needed.
            {
                if (!HasAfflictionLimb(Character,Identifier,GivenLimbType)) { return DefaultValue; }
                float Strength = Character.CharacterHealth.GetAfflictionStrength(Identifier, GetCharacterLimb(Character, GivenLimbType), false);
                return Strength;
            }
            return DefaultValue;
        }

        public static void SetAffliction(Character Character, string Identifier, float Strength, Character Aggressor, float PreviousStrength)
        {
            SetAfflictionLimb(Character, Identifier, LimbType.Torso ,Strength, Aggressor, PreviousStrength);
        }

        public static void SetAfflictionLimb(Character Character, string Identifier, LimbType GivenLimbType, float Strength, Character Aggressor, float PreviousStrength)
        {
            dynamic Check = AfflictionPrefab.Prefabs.TryGet(Identifier, out AfflictionPrefab Result); // Most likely a better way to acheive this but basically I don't know what this will return.
            if (Result == null) {return;}
            AfflictionPrefab Prefab = Result;
            float Resistance = Character.CharacterHealth.GetResistance(Prefab, GivenLimbType);
            if (Resistance > 1) {return;}
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

        public static void AddAfflictionLimb(Character Character, string Identifier, LimbType GivenLimbType, float Strength, Character Aggresor)
        {
            if (Strength < 0)
            {
                Character.CharacterHealth.ReduceAfflictionOnLimb(
                    GetCharacterLimb(Character, GivenLimbType),
                    Identifier,
                    -Strength,
                    null,
                    Aggresor);
                return;
            }
            float PrevStrength = GetAfflictionStrengthLimb(Character, GivenLimbType, Identifier);
            SetAfflictionLimb(Character, Identifier, GivenLimbType, Strength + PrevStrength, Aggresor, PrevStrength);
        }

        public static void AddAffliction(Character Character, string Identifier, float Strength, Character Aggresor)
        {
            float PrevStrength = GetAfflictionStrength(Character, Identifier);
            SetAffliction(Character, Identifier, Strength + PrevStrength, Aggresor, PrevStrength);
        }

        public static void AddAfflictionResisted(Character Character, string Identifier, float Strength, Character Aggresor)
        {
            float PrevStrength = GetAfflictionStrength(Character, Identifier);
            Strength *= 1 - GetResistance(Character, Identifier);
            SetAffliction(Character, Identifier, Strength + PrevStrength, Aggresor, PrevStrength);
        }

        public static void ApplyAfflictionChange(Character Character, string Identifier, float Strength, float PrevStrength, float MinStrength, float MaxStrength)
        {
            Strength = Math.Clamp(Strength, MinStrength, MaxStrength);
            PrevStrength = Math.Clamp(PrevStrength, MinStrength, MaxStrength);
            if (PrevStrength != Strength)
            {
                SetAffliction(Character,Identifier, Strength, Character, Strength);
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
            return HasAfflictionLimb(Character, CreateLimbAfflictionID(GivenLimbType,"amputation","ll_amputation") + "t", GivenLimbType);
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

        public static void SurgicallyAmputateLimb(Character Character, LimbType GivenLimbType, float Strength = 100, float TraumampStrength = 0)
        {
            GivenLimbType = NormalizeLimbType(GivenLimbType);
            SetAffliction(Character, CreateLimbAfflictionID(GivenLimbType, "amputation", "ll_amputation") + "s", Strength, Character, Strength);
            SetAffliction(Character, CreateLimbAfflictionID(GivenLimbType, "amputation", "ll_amputation") + "t", TraumampStrength, Character, TraumampStrength);
            SetAffliction(Character, "gangrene", 0, Character, 0);
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
            if (!Attacker.IsHuman && !Attacker.Inventory.IsFull())
            {

            }
            else
            {

            }
        }

        public static void SurgicallyAmputateLimbAndGenerateItem(Character UsingCharacter, Character TargetCharacter, LimbType GivenLimbType) // Holy mouth full
        {
            Item PrevItem = GetItemInHeadWear(TargetCharacter);
            if (PrevItem != null && GivenLimbType == LimbType.Head) { PrevItem.Drop(UsingCharacter,true); }
            bool DropLimb = !LimbIsAmputated(TargetCharacter,GivenLimbType) || !HasAfflictionLimb(TargetCharacter,"gangrene",GivenLimbType,15);
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
    }
}
