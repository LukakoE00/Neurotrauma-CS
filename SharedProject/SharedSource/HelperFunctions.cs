
using Barotrauma;

namespace Neurotrauma
{
    public class HF // The HelperFunctions class
    {
        static public Limb GetCharacterLimb(Character Character, LimbType GivenLimbType)
        {
            if (Character == null) { return Character!.AnimController.GetLimb(LimbType.Torso); }
            return Character!.AnimController.GetLimb(GivenLimbType);
        }

        static public bool HasAffliction(Character Character, string Identifier = "", float MinAmount = 0)
        {
            if (Character == null || Identifier == "") { return false; }
            Affliction Aff = Character.CharacterHealth.GetAffliction(Identifier);
            if (Aff == null) { return false; } // Is the affliction null?
            float AffStrength = Aff.Strength;
            if (AffStrength > MinAmount)
            {
                return true;
            }
            return false;
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
    }
}
