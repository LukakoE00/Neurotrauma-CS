
using Barotrauma;
using FluentResults;
using System;
using System.Xml.Linq;
// What shit was Tina/Mannatu smoking. How do you write this many functions. - GreenBean
// The majority of these functions are direct translations into C#. Due to the fact that most of the C# methods were already exposed in lua, we can easily do this.

namespace Neurotrauma
{

    public static class HF // The HelperFunctions class
    {
        // ---------------------------------------- Utility Related Helper Functions -------------------------------------------------- \\
        public static Limb GetCharacterLimb(Character Character, LimbType GivenLimbType)
        {
            return Character.AnimController.GetLimb(GivenLimbType);
        }

        public static float GetResistance(Character Character, string Identifier, LimbType GivenLimbType = LimbType.Torso) // Only returns health Resistance
        {
            AfflictionPrefab Prefab = AfflictionPrefab.Prefabs[Identifier];
            return Character.CharacterHealth.GetResistance(Prefab, GivenLimbType);
        }

        public static float GetItemAfflictionResistance(Item Item, string ResistanceID) // I thought this was a useful helper function. Credit to Antinous for the original method.
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
            return 0; // Womp Womp
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


    }
}
