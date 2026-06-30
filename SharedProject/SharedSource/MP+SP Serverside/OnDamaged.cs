using Barotrauma.Items.Components;

namespace Neurotrauma;

public class OnDamaged
{
    public static readonly Dictionary<string, Action<Character, float, LimbType>> OnDamagedMethods = new();

    public static readonly List<Func<CharacterHealth, List<Affliction>, Limb, List<Affliction>>> ModifyingOnDamagedHooks = new();

    public static readonly List<Action<CharacterHealth, AttackResult, Limb>> OnDamagedHooks = new();

    private static bool HasLungs(Character C) => !(HF.HasAffliction(C, "lungremoved"));

    private static bool HasHeart(Character C) => !(HF.HasAffliction(C, "heartremoved"));

    /// <summary>
    /// Reduces Concussion amount based on worn armor.
    /// </summary>
    /// <param name="Armor">Item ID of worn armor.</param>
    /// <param name="Strength">Amount of Strength of the Concussion Affliction.</param>
    /// <returns></returns>
    public static float GetCalculatedConcussionReduction(Item Armor, float Strength)
    {
        if (Armor == null)
        {
            return 0f;
        }

        if (!Armor.HasTag("deepdiving") &&
            !Armor.HasTag("deepdivinglarge") &&
            !Armor.HasTag("smallitem"))
        {
            return 0f;
        }

        var wearable = Armor.GetComponent<Wearable>();
        if (wearable == null)
        {
            return 0f;
        }

        foreach (var modifier in wearable.DamageModifiers)
        {
            if (modifier.AfflictionIdentifiers.Contains("concussion"))
            {
                return Strength - (Strength * modifier.DamageMultiplier);
            }
        }

        return 0f;
    }

    public static void Override_DamageLimb(
    Character Character,
    Vector2 WorldPosition,
    Limb HitLimb,
    IEnumerable<Affliction> Afflictions,
    float StunAmount,
    bool PlaySound,
    Vector2 AttackImpulse,
    Character Attacker = null,
    float DamageMultiplier = 1f,
    bool AllowStacking = true,
    float Penetration = 0f,
    bool ShouldImplode = false,
    bool IgnoreDamageOverlay = false,
    bool RecalculateVitality = true)
    {
        // Confirm the attack data is valid.
        if (Character == null || Character.IsDead || !(Character.IsHuman) ||
            Afflictions == null ||
            HitLimb == null || HitLimb.IsSevered ||
            Attacker == null ||
            !(NTConfig.Get<bool>("NT_Calculations", true)))
        {
            return;
        }

        // Pull the Evil Falldamage abusing creatures from config.
        var CreatureCategory = NTConfig.Get<IEnumerable<string>>("NT_creatureNoFallDamage", Enumerable.Empty<string>());

        // If one of these critters caused the attack, counteract the additional damage.
        foreach (string Species in CreatureCategory)
        {
            if (Attacker.SpeciesName == Species)
            {
                HF.AddAffliction(Character, "stopcreatureabuse", 2f);
                break;
            }
        }
    }

    public static void Override_ApplyDamage(
    CharacterHealth characterHealth,
    Limb HitLimb,
    AttackResult AttackResult,
    bool AllowStacking = true,
    bool RecalculateVitality = true)
    {
        // Confirm the attack data is valid.
        if (characterHealth == null || characterHealth.Character == null || characterHealth.Character.IsDead || !(characterHealth.Character.IsHuman) ||
            AttackResult.Afflictions == null || !(AttackResult.Afflictions.Any()) ||
            HitLimb == null || HitLimb.IsSevered ||
            !NTConfig.Get<bool>("NT_Calculations", true))
        {
            return;
        }

        // Check for Luabotomy.
        if (!HF.HasAffliction(characterHealth.Character, "luabotomy"))
        {
            HF.SetAffliction(characterHealth.Character, "luabotomy", 1f);
        }

        List<Affliction> Afflictions = AttackResult.Afflictions;

        // NT Compatibility Modifying OnDamaged Hooks
        foreach (var hook in OnDamaged.ModifyingOnDamagedHooks)
        {
            Afflictions = hook(characterHealth, Afflictions, HitLimb);
        }

        // Run the method corresponding to the identifier (if it exists)
        foreach (Affliction affliction in Afflictions)
        {
            string Identifier = affliction.Prefab.Identifier.Value;

            if (OnDamaged.OnDamagedMethods.TryGetValue(Identifier, out var method))
            {
                float Resistance = HF.GetResistance(characterHealth.Character, Identifier, HitLimb.type);
                float Strength = affliction.Strength * (1f - Resistance);
                method(characterHealth.Character, Strength, HitLimb.type);
            }
        }

        // NT Compatibility OnDamaged Hooks
        foreach (var hook in OnDamaged.OnDamagedHooks)
        {
            hook(characterHealth, AttackResult, HitLimb);
        }
    }

    public static void InitializeOnDamagedMethods()
    {
        OnDamagedMethods["gunshotwound"] = GunshotWound;
        OnDamagedMethods["explosiondamage"] = ExplosionDamage;
        OnDamagedMethods["bitewounds"] = BiteWounds;
        OnDamagedMethods["lacerations"] = Lacerations;
        OnDamagedMethods["blunttrauma"] = BluntTrauma;
        OnDamagedMethods["internaldamage"] = InternalDamage;
    }

    // Causes Foreign Bodies, Rib Fractures, Pneumothorax, Tamponade, Internal Bleeding, Fractures, Neurotrauma
    public static void GunshotWound(Character Character, float Strength, LimbType LimbType)
    {
        // Normalize just in case
        LimbType = HF.NormalizeLimbType(LimbType);

        bool CauseFullForeignBody = false;

        // Torso-specific injuries
        if (Strength >= 1 && LimbType == LimbType.Torso)
        {
            bool hitOrgan = false;

            // Rib Injuries
            if (HF.Chance(Math.Clamp(Strength * 0.02f, 0f, 0.3f)))
            {
                HF.BreakLimb(Character, LimbType);
                CauseFullForeignBody = true;
            }

            // Lung Injuries
            if (HasLungs(Character) && HF.Chance(0.3f))
            {
                HF.AddAffliction(Character, "pneumothorax", 5);
                HF.AddAffliction(Character, "lungdamage", Strength);
                HF.AddAffliction(Character, "organdamage", Strength / 4);
                hitOrgan = true;
            }

            // Heart Injuries
            if (HasHeart(Character) && !hitOrgan && Strength >= 5 && HF.Chance(Strength / 50f))
            {
                HF.AddAffliction(Character, "tamponade", 5);
                HF.AddAffliction(Character, "heartdamage", Strength);
                HF.AddAffliction(Character, "organdamage", Strength / 4);
                hitOrgan = true;
            }

            // Liver + Kidney Injuries
            if (!hitOrgan && Strength >= 2 && HF.Chance(0.5f))
            {
                HF.AddAfflictionLimb(Character, "organdamage", LimbType, Strength / 4);
                HF.AddAffliction(Character, "infectedcavity", 5);

                if (HF.Chance(0.5f))
                {
                    HF.AddAffliction(Character, "liverdamage", Strength);
                }
                else
                {
                    HF.AddAffliction(Character, "kidneydamage", Strength);
                }
            }

            // Internal Bleeding
            if (Strength >= 5)
            {
                HF.AddAffliction(Character, "internalbleeding", Strength * HF.RandomRange(0.3f, 0.6f));
            }
        }

        // Head-specific injuries
        if (Strength >= 1 && LimbType == LimbType.Head)
        {
            if (HF.Chance(Strength / 90f))
            {
                HF.BreakLimb(Character, LimbType);
                CauseFullForeignBody = true;
            }

            if (Strength >= 5 && HF.Chance(0.7f))
            {
                HF.AddAffliction(Character, "neurotrauma", Strength * HF.RandomRange(0.1f, 0.4f));
            }
        }

        // Extremity-specific injuries
        if (Strength >= 1 && HF.LimbIsExtremity(LimbType))
        {
            if (HF.LimbIsBroken(Character, LimbType, false) && !(HF.LimbIsAmputated(Character, LimbType)) && HF.Chance(Strength / 60f))
            {
                HF.TraumamputateLimb(Character, LimbType, null);
            }

            if (HF.Chance(Strength / 60f))
            {
                HF.BreakLimb(Character, LimbType);
                CauseFullForeignBody = true;
            }
        }

        // Foreign Bodies
        if (CauseFullForeignBody)
        {
            HF.AddAfflictionLimb(Character, "foreignbody", LimbType, Math.Clamp(Strength, 0, 30));
        }
        else if (HF.Chance(0.75f))
        {
            HF.AddAfflictionLimb(Character, "foreignbody", LimbType, Math.Clamp(Strength / 4f, 0, 20));
        }
    }

    // Causes Foreign Bodies, Rib Fractures, Pneumothorax, Internal Bleeding, Fractures
    public static void ExplosionDamage(Character Character, float Strength, LimbType LimbType)
    {
        // Normalize just in case
        LimbType = HF.NormalizeLimbType(LimbType);

        // Deal with Multipliers
        var NTCharacter = HumanUpdate.CharacterToNTHuman(Character);

        // Possible Foreign Bodies
        if (HF.Chance(0.75f))
        {
            HF.AddAfflictionLimb(Character, "foreignbody", LimbType, Strength / 2f * (float)(NTC.GetMultiplier(NTCharacter, "foreignbodymultiplier")));
        }

        // Torso-specific injuries
        if (Strength >= 1 && LimbType == LimbType.Torso)
        {
            // Fractures
            if (Strength >= 10 && HF.Chance(Strength / 50f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Pneumothorax
            if (HasLungs(Character) && Strength >= 5 && HF.Chance(Strength / 50f * (float)NTC.GetMultiplier(NTCharacter, "pneumothoraxchance") * NTConfig.Get("NT_pneumothoraxChance", 1f)))
            {
                HF.AddAffliction(Character, "pneumothorax", 5);
            }

            // Internal Bleeding
            if (Strength >= 5)
            {
                HF.AddAffliction(Character, "internalbleeding", Strength * HF.RandomRange(0.2f, 0.5f));
            }
        }

        // Head-specific injuries
        if (Strength >= 1 && LimbType == LimbType.Head)
        {
            // Concussion (Armor-Reduced)
            if (Strength >= 15 && HF.Chance(Math.Min(Strength / 60f, 0.7f)))
            {
                var Armor1 = Character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes);
                var Armor2 = Character.Inventory.GetItemInLimbSlot(InvSlotType.Head);

                float Reduction = Math.Max(10f - GetCalculatedConcussionReduction(Armor1, 10f) - GetCalculatedConcussionReduction(Armor2, 10f), 0f);

                HF.AddAfflictionResisted(Character, "concussion", Reduction);
            }

            // Fractured Skull
            if (Strength >= 15 && HF.Chance(Math.Min(Strength / 60f, 0.7f) * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Neck Fracture
            if (Strength >= 15 && HF.Chance(Math.Min(Strength / 60f, 0.7f) * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.AddAffliction(Character, "fracturedneck", 5);
            }

            // Remove the head
            if (Strength >= 75 && HF.Chance(0.25f))
            {
                var previtem = HF.GetItemInHeadWear(Character);

                if (previtem != null)
                {
                    previtem.Drop(Character, true);
                }

                HF.TraumamputateLimb(Character, LimbType, null);
            }
        }

        // Extremities
        if (Strength >= 1 && HF.LimbIsExtremity(LimbType))
        {
            // Traumatic Amputations
            if (HF.LimbIsBroken(Character, LimbType, false) 
                && !HF.LimbIsAmputated(Character, LimbType) 
                && HF.Chance(Strength / 60f * (float)NTC.GetMultiplier(NTCharacter, "traumamputatechance") * NTConfig.Get("NT_traumaticAmputationChance", 1f)))
            {
                HF.TraumamputateLimb(Character, LimbType, null);
            }

            // Fractures
            if (HF.Chance(Strength / 60f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Dislocations
            if (HF.Chance(0.35f * (float)NTC.GetMultiplier(NTCharacter, "dislocationchance") * NTConfig.Get("NT_dislocationChance", 1f)) && !HF.LimbIsAmputated(Character, LimbType))
            {
                HF.DislocateLimb(Character, LimbType);
            }
        }
    }

    // Causes Rib Fractures, Pneumothorax, Internal Bleeding, Concussion, Fractures
    public static void BiteWounds(Character Character, float Strength, LimbType LimbType)
    {
        // Normalize just in case
        LimbType = HF.NormalizeLimbType(LimbType);

        // Deal with Multipliers
        var NTCharacter = HumanUpdate.CharacterToNTHuman(Character);

        // Torso-specific injuries
        if (Strength >= 1 && LimbType == LimbType.Torso)
        {
            // Rib fractures
            if (Strength >= 10 && HF.Chance((Strength - 10f) / 50f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Pneumothorax
            if (HasLungs(Character) && Strength >= 5 && HF.Chance((Strength - 5f) / 50f * (float)NTC.GetMultiplier(NTCharacter, "pneumothoraxchance") * NTConfig.Get("NT_pneumothoraxChance", 1f)))
            {
                HF.AddAffliction(Character, "pneumothorax", 5);
            }

            // Internal Bleeding
            if (Strength >= 5)
            {
                HF.AddAffliction(Character, "internalbleeding", Strength * HF.RandomRange(0.2f, 0.5f));
            }
        }

        // Head-specific injuries
        if (Strength >= 1 && LimbType == LimbType.Head)
        {
            // Concussion (Armor-Reduced)
            if (Strength >= 15 && HF.Chance(Math.Min(Strength / 60f, 0.7f)))
            {
                var Armor1 = Character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes);
                var Armor2 = Character.Inventory.GetItemInLimbSlot(InvSlotType.Head);

                float Reduction = Math.Max(10f - GetCalculatedConcussionReduction(Armor1, 10f) - GetCalculatedConcussionReduction(Armor2, 10f), 0f);

                HF.AddAfflictionResisted(Character, "concussion", Reduction);
            }

            // Fractured Skull
            if (Strength >= 15 && HF.Chance(Math.Min((Strength - 10f) / 60f, 0.7f) * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }
        }

        // Extremities
        if (Strength >= 1 && HF.LimbIsExtremity(LimbType))
        {
            // Traumatic Amputation
            if (HF.LimbIsBroken(Character, LimbType, false) 
                && !(HF.LimbIsAmputated(Character, LimbType)) 
                && HF.Chance((Strength - 5f) / 60f * (float)NTC.GetMultiplier(NTCharacter, "traumamputatechance") * NTConfig.Get("NT_traumaticAmputationChance", 1f)))
            {
                HF.TraumamputateLimb(Character, LimbType, Character.LastAttacker);
            }

            // Fractures
            if (HF.Chance((Strength - 5f) / 60f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }
        }
    }

    // Causes Rib Fractures, Pneumothorax, Tamponade, Internal Bleeding, Fractures
    public static void Lacerations(Character Character, float Strength, LimbType LimbType)
    {
        // Normalize just in case
        LimbType = HF.NormalizeLimbType(LimbType);

        // Deal with Multipliers
        var NTCharacter = HumanUpdate.CharacterToNTHuman(Character);

        // Torso-specific injuries
        if (Strength >= 1 && LimbType == LimbType.Torso)
        {
            // Rib fractures
            if (Strength >= 10 && HF.Chance((Strength - 10f) / 50f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Pneumothorax
            if (HasLungs(Character) && Strength >= 5 && HF.Chance((Strength - 5f) / 50f * (float)NTC.GetMultiplier(NTCharacter, "pneumothoraxchance") * NTConfig.Get("NT_pneumothoraxChance", 1f)))
            {
                HF.AddAffliction(Character, "pneumothorax", 5);
            }

            // Tamponade
            if (HasHeart(Character) && Strength >= 5 && HF.Chance((Strength - 5f) / 50f * (float)NTC.GetMultiplier(NTCharacter, "tamponadechance") * NTConfig.Get("NT_tamponadeChance", 1f)))
            {
                HF.AddAffliction(Character, "tamponade", 5);
            }

            // Internal bleeding
            if (Strength >= 5)
            {
                HF.AddAffliction(Character, "internalbleeding", Strength * HF.RandomRange(0.2f, 0.5f));
            }
        }

        // Head-specific injuries
        if (Strength >= 1 && LimbType == LimbType.Head)
        {
            if (Strength >= 15 && HF.Chance(Math.Min((Strength - 15f) / 60f, 0.7f) * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }
        }

        // Extremities
        if (Strength >= 1 && HF.LimbIsExtremity(LimbType))
        {
            // Traumatic amputation
            if (HF.LimbIsBroken(Character, LimbType, false)
                && !HF.LimbIsAmputated(Character, LimbType)
                && HF.Chance(Strength / 60f * (float)NTC.GetMultiplier(NTCharacter, "traumamputatechance") * NTConfig.Get("NT_traumaticAmputationChance", 1f)))
            {
                HF.TraumamputateLimb(Character, LimbType, null);
            }

            // Fractures
            if (HF.Chance((Strength - 5f) / 60f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }
        }
    }

    // Causes Rib Fractures, Organ Damage, Pneumothorax, Concussion, Fractures, Neurotrauma
    public static void BluntTrauma(Character Character, float Strength, LimbType LimbType)
    {
        // Normalize just in case
        LimbType = HF.NormalizeLimbType(LimbType);

        // Deal with Multipliers
        var NTCharacter = HumanUpdate.CharacterToNTHuman(Character);

        bool FractureImmune = HF.HasAffliction(Character, "cpr_fracturebuff");

        // Torso
        if (!FractureImmune && Strength >= 1 && LimbType == LimbType.Torso)
        {
            // Fractured Ribs
            if (HF.Chance(Strength / 50f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Specific Organ Damage
            HF.AddAffliction(Character, "lungdamage", Strength * HF.RandomRange(0f, 1f));
            HF.AddAffliction(Character, "heartdamage", Strength * HF.RandomRange(0f, 1f));
            HF.AddAffliction(Character, "liverdamage", Strength * HF.RandomRange(0f, 1f));
            HF.AddAffliction(Character, "kidneydamage", Strength * HF.RandomRange(0f, 1f));
            HF.AddAffliction(Character, "organdamage", Strength * HF.RandomRange(0f, 1f));

            // Pneumothorax
            if (HasLungs(Character) && Strength >= 5 && HF.Chance(
                Strength / 50f * (float)NTC.GetMultiplier(NTCharacter, "pneumothoraxchance") * NTConfig.Get("NT_pneumothoraxChance", 1f)))
            {
                HF.AddAffliction(Character, "pneumothorax", 5);
            }
        }

        // Head
        if (!FractureImmune && Strength >= 1 && LimbType == LimbType.Head)
        {
            // Concussion (Armor-Reduced)
            if (Strength >= 15 && HF.Chance(Math.Min(Strength / 60f, 0.7f)))
            {
                var Armor1 = Character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes);
                var Armor2 = Character.Inventory.GetItemInLimbSlot(InvSlotType.Head);

                float Reduction = Math.Max(10f - GetCalculatedConcussionReduction(Armor1, 10f) - GetCalculatedConcussionReduction(Armor2, 10f), 0f);

                HF.AddAfflictionResisted(Character, "concussion", Reduction);
            }

            // Fractured Skull
            if (Strength >= 15 && HF.Chance(Math.Min((Strength - 10f) / 60f, 0.7f) * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Fractured Neck
            if (Strength >= 15 && HF.Chance(Math.Min((Strength - 10f) / 60f, 0.7f) * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.AddAffliction(Character, "fracturedneck", 5);
            }

            // Neurotrauma
            if (Strength >= 5 && HF.Chance(0.7f))
            {
                HF.AddAffliction(Character, "neurotrauma", Strength * HF.RandomRange(0.1f, 0.4f));
            }
        }

        // Extremities
        if (!FractureImmune && Strength >= 1 && HF.LimbIsExtremity(LimbType))
        {
            // Traumatic Amputation
            if (Strength > 15
                && HF.LimbIsBroken(Character, LimbType, false)
                && !(HF.LimbIsAmputated(Character, LimbType))
                && HF.Chance(Strength / 100f * (float)NTC.GetMultiplier(NTCharacter, "traumamputatechance") * NTConfig.Get("NT_traumaticAmputationChance", 1f)))
            {
                HF.TraumamputateLimb(Character, LimbType, null);
            }

            // Fractures
            if (HF.Chance((Strength - 2f) / 60f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Dislocation
            if (HF.Chance(Math.Clamp((Strength - 2f) / 80f, 0f, 0.5f)
                * (float)NTC.GetMultiplier(NTCharacter, "dislocationchance")
                * NTConfig.Get("NT_dislocationChance", 1f))
                && !(HF.LimbIsAmputated(Character, LimbType)))
            {
                HF.DislocateLimb(Character, LimbType);
            }
        }
    }

    // Causes Rib Fractures, Organ Damage, Pneumothorax, Concussion, Fractures
    public static void InternalDamage(Character Character, float Strength, LimbType LimbType)
    {
        // Normalize just in case
        LimbType = HF.NormalizeLimbType(LimbType);

        // Deal with Multipliers
        var NTCharacter = HumanUpdate.CharacterToNTHuman(Character);

        // Torso
        if (Strength >= 1 && LimbType == LimbType.Torso)
        {
            // Fractured Ribs
            if (HF.Chance((Strength - 5f) / 50f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Specific Organ Damage
            HF.AddAffliction(Character, "lungdamage", Strength * HF.RandomRange(0f, 1f));
            HF.AddAffliction(Character, "heartdamage", Strength * HF.RandomRange(0f, 1f));
            HF.AddAffliction(Character, "liverdamage", Strength * HF.RandomRange(0f, 1f));
            HF.AddAffliction(Character, "kidneydamage", Strength * HF.RandomRange(0f, 1f));
            HF.AddAffliction(Character, "organdamage", Strength * HF.RandomRange(0f, 1f));

            // Pneumothorax
            if (HasLungs(Character) && Strength >= 5 && HF.Chance(
                (Strength - 5f) / 50f * (float)NTC.GetMultiplier(NTCharacter, "pneumothoraxchance") * NTConfig.Get("NT_pneumothoraxChance", 1f)))
            {
                HF.AddAffliction(Character, "pneumothorax", 5);
            }
        }

        // Head
        if (Strength >= 1 && LimbType == LimbType.Head)
        {
            // Concussion (Armor-Reduced)
            if (Strength >= 15 && HF.Chance(Math.Min(Strength / 60f, 0.7f)))
            {
                var Armor1 = Character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes);
                var Armor2 = Character.Inventory.GetItemInLimbSlot(InvSlotType.Head);

                float Reduction = Math.Max(10f - GetCalculatedConcussionReduction(Armor1, 10f) - GetCalculatedConcussionReduction(Armor2, 10f), 0f);

                HF.AddAfflictionResisted(Character, "concussion", Reduction);
            }

            // Fractured Skull
            if (Strength >= 15 && HF.Chance(Math.Min((Strength - 5f) / 60f, 0.7f) * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Fractured Neck
            if (Strength >= 15 && HF.Chance(Math.Min((Strength - 5f) / 60f, 0.7f) * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.AddAffliction(Character, "fracturedneck", 5);
            }
        }

        // Extremities
        if (Strength >= 1 && HF.LimbIsExtremity(LimbType))
        {
            // Traumatic Amputation
            if (Strength > 10
                && HF.LimbIsBroken(Character, LimbType, false)
                && !(HF.LimbIsAmputated(Character, LimbType))
                && HF.Chance((Strength - 10f) / 60f * (float)NTC.GetMultiplier(NTCharacter, "traumamputatechance") * NTConfig.Get("NT_traumaticAmputationChance", 1f)))
            {
                HF.TraumamputateLimb(Character, LimbType, null);
            }

            // Fractures
            if (HF.Chance((Strength - 5f) / 60f * (float)NTC.GetMultiplier(NTCharacter, "anyfracturechance") * NTConfig.Get("NT_fractureChance", 1f)))
            {
                HF.BreakLimb(Character, LimbType);
            }

            // Dislocations
            if (HF.Chance(0.25f * (float)NTC.GetMultiplier(NTCharacter, "dislocationchance") * NTConfig.Get("NT_dislocationChance", 1f)) && !(HF.LimbIsAmputated(Character, LimbType)))
            {
                HF.DislocateLimb(Character, LimbType);
            }
        }
    }
}