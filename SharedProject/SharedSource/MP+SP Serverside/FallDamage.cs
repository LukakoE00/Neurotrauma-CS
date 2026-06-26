
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using FarseerPhysics.Common;
using System;
using Voronoi2;
using static Neurotrauma.HF;
using static Neurotrauma.NTC;
using static Sdl.Joystick;

namespace Neurotrauma
{
    public static class NTFallDamage
    {

        public static bool HasLungs(Character C)
        {
            return HasAffliction(C, "lungremoved");
        }

        public static double GetReduction(double Strength, Item Armor, string Type = "blunttrauma")
        {
            Wearable Wearable = (Wearable)Armor.GetComponentString("Wearable");
            IEnumerable<DamageModifier> Modifiers = Wearable.DamageModifiers;
            foreach (DamageModifier Modifier in Modifiers)
            {
                if (Modifier.AfflictionIdentifiers.Contains(Type))
                {
                    return Strength - Strength * Modifier.DamageMultiplier;
                }
            }
            return 0;
        }

        public static double GetCalculatedReductionSuit(Item Armor, double Strength, LimbType Limb)
        {
            if (Armor == null) { return 0; }
            double Reduction = 0;

            if (Armor.HasTag("deepdivinglarge") || Armor.HasTag("deepdiving"))
            {
                Reduction = GetReduction(Strength, Armor);
            }
            else if (Armor.HasTag("clothing") || Armor.HasTag("smallitem"))
            {
                Reduction = GetReduction(Strength, Armor);
            }

            return Reduction;
        }

        public static double GetCalculatedReductionClothes(Item Armor, double Strength, LimbType Limb)
        {
            if (Armor == null) { return 0; }
            double Reduction = 0;

            if (Armor.HasTag("deepdivinglarge") || Armor.HasTag("deepdiving"))
            {
                Reduction = GetReduction(Strength, Armor);
            }
            else if (Armor.HasTag("clothing") || Armor.HasTag("smallitem"))
            {
                Reduction = GetReduction(Strength, Armor);
            }

            return Reduction;
        }

        public static double GetCalculatedReductionHelmet(Item Armor, double Strength, LimbType Limb)
        {
            if (Armor == null) { return 0; }
            double Reduction = 0;

            if (Armor.HasTag("smallitem"))
            {
                Reduction = GetReduction(Strength, Armor);
            }

            return Reduction;
        }

        public static double GetCalculatedConcussionReduction(Item Armor, double Strength, LimbType Limb)
        {
            if (Armor == null) { return 0; }
            double Reduction = 0;

            if (Armor.HasTag("deepdivinglarge") || Armor.HasTag("deepdiving"))
            {
                Reduction = GetReduction(Strength, Armor, "concussion");
            }
            else if (Armor.HasTag("clothing") || Armor.HasTag("smallitem"))
            {
                Reduction = GetReduction(Strength, Armor, "concussion");
            }

            return Reduction;
        }

        public static void OnChangeFallDamage(float impactDamage, Character character, Vector2 impactPos, Vector2 velocity)
        {
            Print("Fall Damage");

            if (!NTConfig.Get("NT_Calculations", true)) return;
            
            if (!character.IsHuman) return;

            if (character.InWater) return;

            if (character.SelectedBy != null) return;

            if (HasAffliction(character, "cpr_fracturebuff") || HasAffliction(character, "stopcreatureabuse")) return;

            if (!HasAffliction(character, "luabotomy")) SetAffliction(character, "luabotomy", 1,character,0);

            double VelocityMagnitude = Math.MaxMagnitude(velocity.X, velocity.Y);
            VelocityMagnitude = Math.Pow(VelocityMagnitude,1.3);

            // apply fall damage to all limbs based on fall direction
            Vector2 MainLimbPos = character.AnimController.MainLimb.WorldPosition;

            Dictionary<LimbType,double> LimbDotResults = new();
            double MinDotRes = 1000;

            foreach (Limb Limb in character.AnimController.Limbs)
            {
                foreach (LimbType Type in LimbsToCheck)
                {
                    if (Limb.type != Type ) continue;
                    // Fetch the direction of each limb relative to the torso.
                    Vector2 LimbPosition = Limb.WorldPosition;
                    Vector2 PosDif = LimbPosition - MainLimbPos;
                    PosDif.X /= 100;
                    PosDif.Y /= 100;
                    double PosDifMagnitude = Math.MaxMagnitude(PosDif.X, PosDif.Y);
                    if (PosDifMagnitude > 1) PosDif.Normalize();

                    Vector2 NormalizedVelocity = new(velocity.X, velocity.Y);
                    NormalizedVelocity.Normalize();

                    // Compare those directions to the direction we're moving.
                    // This will later be used to hurt the limbs facing impact more than the others
                    double LimbDot = Vector2.Dot(PosDif, NormalizedVelocity);
                    LimbDotResults[Type] = LimbDot;
                    if (MinDotRes > LimbDot) MinDotRes = LimbDot;
                    break;
                }
            }

            // shift all weights out of the negatives
            // increase the weight of all limbs if speed is high
            // the effect of this is that, at higher speeds, all limbs take damage instead of mainly the ones facing the impact site
            foreach (KeyValuePair<LimbType,double> Pair in LimbDotResults)
            {
                LimbType Type = Pair.Key;
                double DotResult = Pair.Value;
                LimbDotResults[Type] = DotResult - MinDotRes + Math.Max(0, (VelocityMagnitude - 30) / 10);
            }

            double WeightSum = 0;
            foreach (KeyValuePair<LimbType, double> Pair in LimbDotResults)
            {
                WeightSum += Pair.Value;
            }

            foreach (KeyValuePair<LimbType, double> Pair in LimbDotResults)
            {
                double RelativeWeight = Pair.Value / WeightSum;

                // lets limit the numbers to the max value of blunttrauma so that resistances make sense
                double DamageInflictedToThisLimb = Math.Min(
                    RelativeWeight * Math.Max(0, Math.Pow(VelocityMagnitude - 10,1.5) * NTConfig.Get("NT_falldamage",1) * .5),
                    NTConfig.Get("NT_falldamageCeiling",1) * 60);

                CauseFallDamage(character,Pair.Key,DamageInflictedToThisLimb);
            }
        }

        public static void CauseFallDamage(Character character, LimbType limbtype, double strength)
        {
            Item Armor1 = GetItemInOuterWear(character);
            Item Armor2= GetItemInInnerWear(character);
            if (limbtype != LimbType.Head)
            {
                strength = Math.Max(
                    strength
                        - GetCalculatedReductionSuit(Armor1, strength, limbtype)
                        - GetCalculatedReductionHelmet(Armor2, strength, limbtype),
                    0
                );
            }
            else
            {
                Armor2 = GetItemInHeadWear(character);
                strength = Math.Max(
                    strength
                        - GetCalculatedReductionSuit(Armor1, strength, limbtype)
                        - GetCalculatedReductionHelmet(Armor2, strength, limbtype),
                    0
                );
            }

            // additionally calculate the affliction reduced damage
            AfflictionPrefab prefab = AfflictionPrefab.Prefabs["blunttrauma"];
            double resistance = character.CharacterHealth.GetResistance(prefab, limbtype);
            if (resistance >= 1) return;
            strength *= (1 - resistance);
            AddAfflictionLimb(character, "blunttrauma", limbtype, (float)strength, character);

            // return earlier if the strength value is not high enough for damage checks
            if (strength < 1) return;

            bool FractureImmune = false;

            double InjuryChanceMultiplier = NTConfig.Get("NT_falldamageSeriousInjuryChance", 1);

            HumanUpdate.NTHuman NTCharacter = HumanUpdate.CharacterToNTHuman(character);

            // torso
            if ((!FractureImmune) && strength >= 1 && limbtype == LimbType.Torso)
            {
                if (Chance((float)(
                        (strength - 15)
                            / 100
                            * GetMultiplier(NTCharacter, "anyfracturechance")
                            * NTConfig.Get("NT_fractureChance", 1)
                            * InjuryChanceMultiplier
                        )))
                {
                    BreakLimb(character, limbtype);
                    if (HasLungs(character) && strength >= 5 && Chance((float)(strength/70*GetMultiplier(NTCharacter, "pneumothoraxchance") * NTConfig.Get("NT_pneumothoraxChance", 1))))
                    {
                        AddAffliction(character, "pneumothorax", 5, character);
                    }
                }
            }

            // head
            if ((!FractureImmune) && strength >= 1 && limbtype == LimbType.Head)
            {
                if (strength >= 15 && Chance((float)Math.Min(strength / 100, .7)))
                {
                    AddAfflictionResisted(
                        character,
                        "concussion",
                        (float)
                        Math.Max(
                        Math.Max(
                            10,
                                -GetCalculatedConcussionReduction(Armor1, 10, limbtype)
                                - GetCalculatedConcussionReduction(Armor2, 10, limbtype)),
                        0), 
                    character);
                } 

                if (strength >= 15 
                    && Chance((float)(
                        Math.Min((strength - 15) / 100, .7)
                            * GetMultiplier(NTCharacter, "anyfracturechance")
                            * NTConfig.Get("NT_fractureChance", 1)
                            * InjuryChanceMultiplier)))
                {
                    BreakLimb(character, limbtype);
                }
                if (strength >= 55
                    && Chance((float)(
                        Math.Min((strength - 15) / 100, .7)
                            * GetMultiplier(NTCharacter, "anyfracturechance")
                            * NTConfig.Get("NT_fractureChance", 1)
                            * InjuryChanceMultiplier)))
                {
                    AddAffliction(character, "fracturedneck", 5, character);
                }
                if (strength >= 5 && Chance(.7f)) ;
                {
                    AddAffliction(character, "neurotrauma", (float) (strength * Rand.Range(0.1, 0.4)), character);
                }
            }

            // extremeties
            if ((!FractureImmune) && strength >= 1 && LimbIsExtremity(limbtype))
            {
                if (Chance((float)(
                        (strength - 15)
                            / 100
                            * NTC.GetMultiplier(NTCharacter, "anyfracturechance")
                            * NTConfig.Get("NT_fractureChance", 1)
                            * InjuryChanceMultiplier)))
                {
                    BreakLimb(character, limbtype);
                    if (Chance((float) (strength - 2) / 60))
                    {
                        //this is here to simulate open fractures
                        ArteryCutLimb(character, limbtype);
                    }
                }

                if (Chance((float)(
                    (strength - 15)
                        / 100
                        * NTC.GetMultiplier(NTCharacter, "anyfracturechance")
                        * NTConfig.Get("NT_fractureChance", 1)
                        * InjuryChanceMultiplier
                        )))
                {
                    DislocateLimb(character, limbtype);
                }
            }
        }

    }
}
