
using Barotrauma;
// What shit was Tina/Mannatu smoking. How do you write this many functions.


namespace Neurotrauma
{
    static public class HF // The HelperFunctions class
    {
        static public Limb GetCharacterLimb(Character Character, LimbType GivenLimbType)
        {
            if (Character == null) { return Character!.AnimController.GetLimb(LimbType.Torso); }
            return Character!.AnimController.GetLimb(GivenLimbType);
        }

        static public bool HasAffliction(Character Character, string Identifier = "", float MinAmount = 0)
        {
            if (Character == null || Identifier == "" || Character.CharacterHealth == null) { return false; }
            Affliction Aff = GetAffliction(Character,Identifier);
            if (Aff == null) { return false; } // Is the affliction null?
            float AffStrength = Aff.Strength;
            if (AffStrength > MinAmount)
            {
                return true;
            }
            return false;
        }

        static public bool HasAfflictionLimb(Character Character, string Identifier = "", LimbType GivenLimbType = LimbType.Torso, float MinAmount = 0)
        {
            if (Character == null || Identifier == "" || Character.CharacterHealth == null) { return false; }
            Affliction Aff = GetAfflictionLimb(Character,Identifier,GivenLimbType);
            if (Aff == null) { return false; } // Is the affliction null?
            float AffStrength = Aff.Strength;
            if (AffStrength > MinAmount)
            {
                return true;
            }
            return false;
        }

        static public Affliction GetAffliction(Character Character, String Identifier = "")
        {
            return Character.CharacterHealth.GetAffliction(Identifier); // No error handling on this one, gonna need someone smarter to do that.
        }

        static public Affliction GetAfflictionLimb(Character Character, String Identifier = "", LimbType GivenLimbType = LimbType.Torso)
        {
            return Character.CharacterHealth.GetAffliction(Identifier, GetCharacterLimb(Character, GivenLimbType)); // No error handling on this one, gonna need someone smarter to do that.
        }

        static public float GetAfflictionStrength(Character Character, String Identifier = "", float DefaultValue = 0)
        {
            if (Character != null & Identifier != "")  // Verify we have the info needed.
            {
                if (!HasAffliction(Character, Identifier)) { return DefaultValue; }
                float Strength = Character.CharacterHealth.GetAfflictionStrength(Identifier, GetCharacterLimb(Character, LimbType.Torso), false);
                return Strength;
            }
            return DefaultValue;
        }

        static public float GetAfflictionStrengthLimb(Character Character, LimbType GivenLimbType = LimbType.Torso, String Identifier = "", float DefaultValue = 0)
        {
            if (Character != null & Identifier != "")  // Verify we have the info needed.
            {
                if (!HasAfflictionLimb(Character,Identifier,GivenLimbType)) { return DefaultValue; }
                float Strength = Character.CharacterHealth.GetAfflictionStrength(Identifier, GetCharacterLimb(Character, GivenLimbType), false);
                return Strength;
            }
            return DefaultValue;
        }
    }
}
