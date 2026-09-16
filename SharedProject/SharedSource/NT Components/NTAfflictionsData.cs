using static Neurotrauma.NTAfflictions;

namespace Neurotrauma;

public class NTAfflictionsToAdd
{

    private static List<NTAfflictionPrefab> AfflictionsToAdd = new List<NTAfflictionPrefab>();

    public static void AddAfflictions()
    {
        NTAfflictionPrefabBuilder builder = new NTAfflictionPrefabBuilder();

        // Oxygen Low
        // Not constant; gets applied by other sourcess
        // Type: Non-Limb Specific, Vanilla Override
        // Caused By: Lack of Oxygen, Respiratory Arrest
        // Effects: Hypoxemia
        NTAfflictionPrefab OxygenLow = builder.New("oxygenlow")
            .SetStrengths(0, 200, 0)
            .SetUpdateAction((NTHuman C, string ID, LimbType Limb, float DeltaTime) =>
            {
                if (C.GetAfflictionStrength("respiratoryarrest") > 0)
                {
                    C.AddAffliction(ID, 15f * DeltaTime);
                }
            })
            .Build();

        AfflictionsToAdd.Add(OxygenLow);

        NeurotraumaInit.NTAfflLoader.Registers(AfflictionsToAdd);

        // Drunk
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Vanilla Override
        // Caused By: ROOOTT BEEERRRRRR.
        // Effects: idk.
        NTAfflictionPrefab Drunk = builder.New("drunk")
            .SetStrengths(0, 200, 0)
            .Build();

        AfflictionsToAdd.Add(Drunk);

        // Psychosis
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Vanilla Override
        // Caused By: no root beer.
        // Effects: Psychosis.
        AfflictionsToAdd.Add(
            builder.New("psychosis")
            .SetStrengths(0, 200, 0)
            .SetPriority(AfflictionPriority.MEDIUM)
            .Build());

        
        // Radiation Sickness
        // Not constant; gets applied by other sources.
        // Type: Damage, Vanilla Override
        // Caused By: Health Scanner, Radiotoxin, Radiation, Certain Damage.
        // Effects: Burns (XML), Screen Grain (XML), Specific Organ Damage, Bone Damage.
        AfflictionsToAdd.Add(
            builder.New("radiationsickness")
            .SetStrengths(0, 200, 0)
            .SetUpdateAction((NTHuman C, string ID, LimbType Limb, float DeltaTime) =>
            {
                float strength = C.GetAfflictionStrength(ID);

                // Passive Regeneration
                C.AddAffliction(ID, -0.02f * DeltaTime);

                // Effects:   
                if (strength > 25)
                {
                    // Additional Lung Damage
                    C.SetAffliction("lungdamage", (Math.Max(C.GetAfflictionStrength("lungdamage") - 25, 0) / 800 * DeltaTime));

                    // Bone Damage
                    C.SetAffliction("bonedamage", (Math.Max(C.GetAfflictionStrength("bonedamage") - 25, 0) / 600 * DeltaTime));
                }

                // Heart Damage (in NewOrganDamage)
                // Liver Damage (in NewOrganDamage)
                // Kidney Damage (in NewOrganDamage)

                // Seizures
                double RadSicknessAbove50 = strength >= 50 ? strength : 0;
                if (HF.Chance((float)(RadSicknessAbove50 / 200 * 0.1)))
                {
                    C.AddAffliction("seizure", 10);
                }

                // Nausea
                if (strength > 80)
                {
                    C.SetSymptomTrue("nausea", 2);
                }
            })
            .Build()); 

        // Respiratory Arrest
        // Not constant; gets applied by other sources, removes itself however.
        // Type: Non-Limb Specific, Interrim
        // Caused By: Lung Damage, TraumaShock, Neurotrauma, Hypoxemia, Opiate Overdose, Stasis, Morbusine Poisoning.
        // Effects: Oxygen Low, Acidosis.

        AfflictionsToAdd.Add(
            builder.New("respiratoryarrest")
            .SetUpdateAction((NTHuman C, string ID, LimbType Limb, float DeltaTime) =>
            {
                // Removal Conditions
                if ((!C.GetBoolStatStrength("stasis")) // Not in Stasis
                        && C.GetAffData("lungremoved").Strength <= 0 // No Lungs Removed
                        && C.GetAffData("brainremoved").Strength <= 0 // No Brain Removed
                        && C.GetAffData("opiateoverdose").Strength <= 60 // Below Opiate Overdose Threshold
                        && C.GetAffData("lungdamage").Strength <= 99 // Below Lung Damage Threshold
                        && C.GetAffData("traumaticshock").Strength <= 30 // Below Traumatic Shock Threshold
                        && C.GetAffData("neurotrauma").Strength <= 100 // Below Neurotrauma Threshold
                        && C.GetAffData("hypoxemia").Strength <= 70 // Below Hypoxemia Threshold
                        )
                {
                    // Passive Regeneration
                    AffData.Strength -= (5f + HF.BoolToNum(C.GetAffStrength("unconsciousness") < 0.1f, 45f)) * NT.DeltaTime;
                }

                // Effects:
                // Acidosis
                // Shares increase with Cardiac Arrest
                double AcidosisIncrease = HF.BoolToNum(C.GetAffData("cardiacarrest").Strength <= 0
                        && C.GetAffData("respiratoryarrest").Strength > 0
                        && C.GetAffData("artificialventilation").Strength <= 0.1)
                    * 0.18 * NT.DeltaTime;

                C.GetAffData("acidosis").Strength += AcidosisIncrease;

                C.SetSymptomFalse("hypoventilation");
                C.SetSymptomFalse("hyperventilation");
                C.SetSymptomFalse("shortnessofbreath");
            })
            .Build());

        // Rib Fractures
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Internal Wounds (DMG), Open Wounds (DMG), Bone Death.
        // Effects: Pneumothorax if not Bandaged (XML), Chest Pain.
        AfflictionsToAdd["fracturedribs"] = new("fracturedribs", 0, 100, 0, AfflictionPriority.MEDIUM);
        AfflictionsToAdd["fracturedribs"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                if (AffData.Strength > 0)
                {
                    // Passive Increase
                    AffData.Strength += 4 * NT.DeltaTime;
                }

                // Effects:
                // Chest Pain
                if (AffData.Strength > 0 && C.GetSymptomAffData("unconsciousness").Strength <= 0 && (!C.GetBoolStatStrength("sedated")))
                {
                    NTC.SetSymptomTrue(C, "chestpain", 3);
                }
            };

        // Neck Fracture
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Internal Wounds (DMG), Open Wounds (DMG), Bone Death.
        // Effects: Spinal Cord Injury if not Bandaged (XML), Internal Damage if not Bandaged (XML).
        AfflictionsToAdd["fracturedneck"] = new("fracturedneck", 0, 100, 0, AfflictionPriority.MEDIUM);
        AfflictionsToAdd["fracturedneck"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                if (AffData.Strength > 0)
                {
                    // Passive Increase
                    AffData.Strength += 4 * NT.DeltaTime;
                }
            };

        // Skull Fracture
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Internal Wounds (DMG), Open Wounds (DMG), Bone Death.
        // Effects: Headache, prevents Neurotrauma Regeneration.
        AfflictionsToAdd["fracturedskull"] = new("fracturedskull", 0, 100, 0, AfflictionPriority.MEDIUM);
        AfflictionsToAdd["fracturedskull"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                if (AffData.Strength > 0)
                {
                    // Passive Increase
                    AffData.Strength += 4 * NT.DeltaTime;
                }

                // Effects:
                // Headache
                if (AffData.Strength > 0 && C.GetAffData("unconsciousness").Strength <= 0)
                {
                    NTC.SetSymptomTrue(C, "headache", 3);
                }

                // Neurotrauma Regeneration (in Neurotrauma itself)
            };

        // =============== Drugs =============== //

        // Opiate Overdose
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Drug, Vanilla Override
        // Caused By: Application of Opiates.
        // Effects: Respiratory Arrest, Unconsciousness, Seizures, Death.
        AfflictionsToAdd["opiateoverdose"] = new("opiateoverdose", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["opiateoverdose"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Effects:
                if (AffData.Strength > 60)
                {
                    // Respiratory Arrest
                    C.GetAffData("respiratoryarrest").Strength += 200;

                    // Unconsciousness
                    NTC.SetSymptomTrue(C, "unconsciousness", 2);

                    // Seizures
                    if (HF.Chance((float)AffData.Strength / 500f))
                    {
                        C.GetAffData("seizure").Strength += 10;
                    }
                }
            };

        // =============== Organs =============== //

        // Lung Damage
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Organ Damage
        // Caused By: ABX, LiOxy, Ambubag, HemoTransShock, RadSickness, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG).
        // Effects: Cough, Shortness of Breath, Respiratory Arrest
        AfflictionsToAdd["lungdamage"] = new("lungdamage", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["lungdamage"].Const = true;
        AfflictionsToAdd["lungdamage"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress while in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                double LungDamage = HF.OrganDamageCalc(C, AffData.Strength + NTC.GetMultiplier(C, "lungdamagegain") * C.GetDoubleStatStrength("neworgandamage"));

                // Passive Regeneration / Increase
                AffData.Strength = LungDamage;

                // Effects:
                // Shortness of Breath
                if (AffData.Strength > 45)
                {
                    if (C.GetAffData("respiratoryarrest").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                    }

                    // Cough
                    if (AffData.Strength > 50 && C.GetAffData("unconsciousness").Strength <= 0 && C.GetAffData("lungremoved").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "cough", 2);
                    }

                    // Respiratory Arrest
                    if (AffData.Strength > 99 && HF.Chance(0.8f))
                    {
                        C.GetAffData("respiratoryarrest").Strength += 200;
                    }
                }
            };

        // Lung Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: Respiratory Arrest, Unconsciousness, eventual Death.
        AfflictionsToAdd["lungremoved"] = new("lungremoved", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["lungremoved"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                if (AffData.Strength <= 0) return;
                // State check; strength is 1 if Retracted Skin is present, else 100.
                AffData.Strength = 1 + HF.BoolToNum(HF.HasAfflictionLimb(C.Human, "retractedskin", LimbType.Head, 99), 99);

                // Effects:
                // Respiratory Arrest
                C.GetAffData("respiratoryarrest").Strength += 200;

                // Unconsciousness
                NTC.SetSymptomTrue(C, "unconsciousness", 2);
            };

        // Lung Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd["lungswap"] = new("lungswap", 0, 100, 0, AfflictionPriority.LOW);

        // Brain Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: Cardiac Arrest, Respiratory Arrest, Unconsciousness, eventual Death.
        AfflictionsToAdd["brainremoved"] = new("brainremoved", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["brainremoved"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                if (AffData.Strength <= 0) return;
                // State check; strength is 1 if Retracted Skin is present, else 100.
                AffData.Strength = 1 + HF.BoolToNum(HF.HasAfflictionLimb(C.Human, "retractedskin", LimbType.Head, 99), 99);

                // Effects:
                // Cardiac Arrest
                C.GetAffData("cardiacarrest").Strength += 200;

                // Respiratory Arrest
                C.GetAffData("respiratoryarrest").Strength += 200;

                // Unconsciousness
                NTC.SetSymptomTrue(C, "unconsciousness", 2);

                // Neurotrauma
                float NeurotraumaGain = 2.4f;
                if (C.GetAffData("afmannitol").Strength <= 0.5)
                {
                    NeurotraumaGain += 1.6f;
                }

                C.GetAffData("neurotrauma").Strength += NeurotraumaGain;
            };

        // Brain Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd["brainswap"] = new("brainswap", 0, 100, 0, AfflictionPriority.LOW);

        // Cardiac Tamponade
        // Type: Non-Limb Specific
        // Not constant; gets applied by other sources.
        // Caused By: Open Wounds (DMG) to Torso.
        // Effects: Decreases Blood Pressure, Weakness, Cough, Shortness of Breath.
        // Additional interaction with: Needle.
        AfflictionsToAdd["tamponade"] = new("tamponade", 0, 100, 0, AfflictionPriority.MEDIUM);
        AfflictionsToAdd["tamponade"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Cannot have Cardiac Tamponade without a heart.
                if (C.GetAffData("heartremoved").Strength > 0)
                {
                    AffData.Strength = 0;
                }

                // Passive Regeneration / Increase
                // Increases if there is no needle until 100%; else decreases until 5%.
                if (AffData.Strength > 0)
                {
                    AffData.Strength = Math.Clamp(AffData.Strength + NT.DeltaTime * (0.5f - HF.BoolToNum(AffData.Strength > 5) * Math.Clamp(C.GetAffData("needlec").Strength, 0, 1)),
                        0,
                        100
                    );
                }

                // Effects:
                // Shortness of Breath
                if (AffData.Strength > 10)
                {
                    if (C.GetAffData("respiratoryarrest").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "shortnessofbreath", 3);
                    }

                    // Cough
                    if (AffData.Strength > 20 && C.GetAffData("unconsciousness").Strength <= 0 && C.GetAffData("lungremoved").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "cough", 3);
                    }

                    // Weakness
                    if (AffData.Strength > 30)
                    {
                        NTC.SetSymptomTrue(C, "weakness", 3);
                    }
                }
            };

        // Increased Heartrate (previously Tachycardia)
        // Type: Non-Limb Specific
        // Constant; too complicated otherwise.
        // Harmless Causes: Sepsis, Blood Loss, Acidosis, Pneumothorax, Adrenaline, Alcohol Withdrawal.
        // Harmful Causes: Aortic Rupture, Acidosis, Hypotension, Hypoxemia, Traumatic Shock.
        AfflictionsToAdd["increasedheartrate"] = new("increasedheartrate", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["increasedheartrate"].Delay = 2; // Delay the first update a little.
        AfflictionsToAdd["increasedheartrate"].Const = true;
        AfflictionsToAdd["increasedheartrate"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {

                // Fibrillation cannot occur without a (beating) heart
                if (C.GetAffData("cardiacarrest").Strength > 0 || C.GetAffData("heartremoved").Strength > 0)
                {
                    C.GetNonLimbAffData("fibrillation").Strength = 0;
                    AffData.Strength = 0;
                    return;
                }


                // Harmless symptom (does not lead to Fibrillation)
                bool hasSymHarmless =
                    C.GetAffData("sepsis").Strength > 20
                    || C.GetDoubleStatStrength("bloodamount") < 60
                    || C.GetAffData("acidosis").Strength > 20
                    || C.GetAffData("pneumothorax").Strength > 30
                    || C.GetAffData("afadrenaline").Strength > 1
                    || C.GetAffData("alcoholwithdrawal").Strength > 75;

                AffData.Strength = Math.Max(AffData.Strength, HF.BoolToNum(hasSymHarmless, 2));


                // Fibrillation speed calculation
                double fibrillationSpeed = -0.1
                    + Math.Clamp(C.GetNonLimbAffData("aorticrupture").Strength, 0, 2)
                    + Math.Clamp(C.GetAffData("acidosis").Strength / 200, 0, 0.5)
                    + Math.Clamp(
                        0.9 - ((C.GetAffData("bloodpressure").Strength + Math.Clamp(C.GetAffData("afpressuredrug").Strength * 5, 0, 20)) / 90),
                        0, 1
                    ) * 2
                    + Math.Clamp(C.GetAffData("hypoxemia").Strength / 100, 0, 1) * 1.5
                    + Math.Clamp((C.GetAffData("traumaticshock").Strength - 5) / 40, 0, 3)
                    - Math.Clamp(C.GetAffData("afadrenaline").Strength, 0, 0.9);

                // Adrenaline halves Fibrillation speed
                if (fibrillationSpeed > 0 && C.GetAffData("afadrenaline").Strength > 0)
                {
                    fibrillationSpeed /= 2;
                }


                // Apply Fibrillation multipliers only when progressing
                if (fibrillationSpeed > 0)
                {
                    fibrillationSpeed *= NTC.GetMultiplier(C, "fibrillation") * NTConfig.Get("NT_fibrillationSpeed", 1);
                }


                // Progress IncreasedHeartrate or Fibrillation
                if (C.GetNonLimbAffData("fibrillation").Strength <= 0)
                {
                    AffData.Strength += fibrillationSpeed * 5 * NT.DeltaTime;

                    if (AffData.Strength >= 100)
                    {
                        C.GetNonLimbAffData("fibrillation").Strength = 5;
                        AffData.Strength = 0;
                    }
                }
                else
                {
                    C.GetNonLimbAffData("fibrillation").Strength += fibrillationSpeed * NT.DeltaTime;
                    AffData.Strength = 0;
                }

            };

        // Fibrillation
        // Type: Non-Limb Specific, Mechanic
        // Not constant; gets applied by other sources.
        // Effects: Cardiac Arrest.
        AfflictionsToAdd["fibrillation"] = new("fibrillation", 0, 100, 0, AfflictionPriority.MEDIUM);
        AfflictionsToAdd["fibrillation"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Fibrillation cannot occur without a (beating) heart
                if (C.GetAffData("cardiacarrest").Strength >= 1 || C.GetAffData("heartremoved").Strength >= 1)
                {
                    AffData.Strength = 0;
                    return;
                }

                // Cardiac Arrest
                if (AffData.Strength > 20 && HF.Chance((float)Math.Pow(AffData.Strength / 100f, 4f)))
                {
                    C.GetAffData("cardiacarrest").Strength += 200;
                }
            };

        // Cardiac Arrest
        // Type: Non-Limb Specific, Lethal
        // Not constant; gets applied by other sources.
        // Caused By: Heart Removed, Brain Removed, Heart Damage, Traumatic Shock, Coma, Hypoxemia, Fibrillation, Stasis.
        // Effects: Coma, Acidosis, Hypotension, Hypoxemia.
        AfflictionsToAdd["cardiacarrest"] = new("cardiacarrest", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["cardiacarrest"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Removal Conditions
                if ((!C.GetBoolStatStrength("stasis")) // Not in Stasis
                    && C.GetAffData("heartremoved").Strength <= 0 // Heart not removed
                    && C.GetAffData("brainremoved").Strength <= 0 // Brain not removed
                    && C.GetAffData("heartdamage").Strength <= 99 // Below Heart Damage threshold
                    && C.GetAffData("traumaticshock").Strength <= 40 // Below Traumatic Shock threshold
                    && C.GetAffData("coma").Strength <= 40 // Below Coma threshold
                    && C.GetAffData("hypoxemia").Strength <= 80 // Below Hypoxemia threshold
                    && C.GetNonLimbAffData("fibrillation").Strength <= 20) // Below Fibrillation threshold
                {
                    AffData.Strength -= 50 * NT.DeltaTime;
                }

                // Effects:
                // Acidosis
                // Shares increase with Respiratory Arrest
                double AcidosisIncrease = 0.18 * NT.DeltaTime;

                C.GetAffData("acidosis").Strength += AcidosisIncrease;

                // Coma
                if (AffData.Strength > 1 && HF.Chance(0.05f))
                {
                    C.GetAffData("coma").Strength += 14;
                }

                // Hypotension (in BloodPressure constant itself)
                // Hypoxemia (in Hypoxemia constant itself)
            };

        // Infected Cavity
        // Type: Non-Limb Specific, Lethal
        // Not constant; gets applied by other sources.
        // Caused By: Damage.
        // Effects: Sepsis.
        AfflictionsToAdd["infectedcavity"] = new("infectedcavity", 0, 100, 0, AfflictionPriority.MEDIUM);
        AfflictionsToAdd["infectedcavity"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress in Stasis
                if ((C.GetBoolStatStrength("stasis"))) return;

                if (AffData.Strength > 0)
                {
                    C.GetAffData("immunity").Strength -= NT.DeltaTime * (Math.Min(1.4, Math.Max(1, .8 + AffData.Strength / 100))); // Lose Immunity

                    if (C.GetAffData("afantibiotics").Strength < 0.1 || AffData.Strength > 20)
                    {
                        if (C.GetAffData("combatstimulant").Strength > 0) return;

                        AffData.Strength += NT.DeltaTime * (.65 - .0125 * Math.Max(.44 * C.GetAffData("immunity").PrevStrength, 20)); // Gain infection
                    }
                    else
                    {
                        AffData.Strength -= NT.DeltaTime * .8; // Lose infection
                    }
                }
            };

        // Heart Attack
        // Type: Non-Limb Specific, Lethal
        // Not constant; gets applied by other sources.
        // Caused By: Hypertension, Antibiotic Glue (XML).
        // Effects: Sweating, Shortness of Breath, Heart Damage.
        AfflictionsToAdd["heartattack"] = new("heartattack", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["heartattack"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Cannot have a heart attack without a heart.
                if (C.GetAffData("heartremoved").Strength > 0)
                {
                    AffData.Strength = 0;
                    return;
                }

                // Passive Regeneration
                AffData.Strength -= NT.DeltaTime;

                // Effects:
                // Sweating
                NTC.SetSymptomTrue(C, "sweating", 2);

                // Shortness of Breath
                if (C.GetAffData("respiratoryarrest").Strength <= 0)
                {
                    NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                }

                // Heart Damage
                C.GetAffData("heartdamage").Strength += (Math.Clamp(C.GetAffData("heartattack").Strength, 0, 0.5) * NT.DeltaTime);
            };

        // Heart Damage
        // Type: Non-Limb Specific, Organ Damage
        // Constant for Regeneration
        // Caused By: Heart Attack, ABX, LiOxy, HemoTransShock, Mannitol, RadSickness, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
        // Effects: Cough, Leg Swelling, Shortness of Breath, Cardiac Arrest.
        AfflictionsToAdd["heartdamage"] = new("heartdamage", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["heartdamage"].Const = true;
        AfflictionsToAdd["heartdamage"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress while in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                double HeartDamage = HF.OrganDamageCalc(C, AffData.Strength + NTC.GetMultiplier(C, "heartdamagegain") * C.GetDoubleStatStrength("neworgandamage"));

                // Passive Regeneration / Increase
                AffData.Strength = HeartDamage;

                // Effects:
                // Cough
                if (AffData.Strength > 50)
                {
                    if (C.GetAffData("unconsciousness").Strength <= 0 && C.GetAffData("lungremoved").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "cough", 2);
                    }

                    // Leg Swelling & Shortness of Breath
                    if (AffData.Strength > 80)
                    {
                        if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                        {
                            NTC.SetSymptomTrue(C, "legswelling", 2);
                        }

                        if (C.GetAffData("respiratoryarrest").Strength <= 0)
                        {
                            NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                        }

                        // Cardiac Arrest
                        if (AffData.Strength > 99 && HF.Chance(0.3f))
                        {
                            C.GetAffData("cardiacarrest").Strength += 200;
                        }
                    }
                }
            };

        // Heart Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: Cardiac Arrest.
        AfflictionsToAdd["heartremoved"] = new("heartremoved", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["heartremoved"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                if (AffData.Strength <= 0) return;
                // State check; strength is 1 if Retracted Skin is present, else 100.
                AffData.Strength = 1 + HF.BoolToNum(HF.HasAfflictionLimb(C.Human, "retractedskin", LimbType.Torso, 99), 99);

                // Effects:
                // Cardiac Arrest
                C.GetAffData("cardiacarrest").Strength += 200;
            };

        // Heart Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd["heartswap"] = new("heartswap", 0, 100, 0, AfflictionPriority.LOW);

        // Kidney Damage
        // Type: Non-Limb Specific, Organ Damage
        // Constant for Regeneration
        // Caused By: ABX, LiOxy, HemoTransShock, Mannitol, RadSickness, Hypertension, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
        // Effects: Acidosis, Leg Swelling, Hypertension, Bone Damage, Vomiting, Neurotrauma, Nausea.
        AfflictionsToAdd["kidneydamage"] = new("kidneydamage", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["kidneydamage"].Const = true;
        AfflictionsToAdd["kidneydamage"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress while in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                double KidneyDamage = HF.KidneyDamageCalc(C, AffData.Strength
                    + NTC.GetMultiplier(C, "kidneydamagegain") * (C.GetDoubleStatStrength("neworgandamage")
                    + Math.Clamp((C.GetBloodAffData("bloodpressure").Strength - 120) / 160, 0, 0.5) * NT.DeltaTime * 0.5));

                // Passive Regeneration / Increase
                AffData.Strength = KidneyDamage;

                // Effects:
                // Acidosis
                double AcidosisIncrease = Math.Max(0, AffData.Strength - 80) / 20.0 * 0.1 * NT.DeltaTime;
                C.GetAffData("acidosis").Strength += AcidosisIncrease;

                // Neurotrauma
                double NeurotraumaIncrease = AffData.Strength / 1000.0 * NT.DeltaTime
                    * NTC.GetMultiplier(C, "neurotraumagain")
                    * NTConfig.Get("NT_neurotraumaGain", 1)
                    * (1 - Math.Clamp(C.GetAffData("afmannitol").Strength, 0, 0.5));

                C.GetAffData("neurotrauma").Strength += NeurotraumaIncrease;

                // Hypertension (in BloodPressure constant)

                // Nausea & Leg Swelling
                if (AffData.Strength > 60)
                {
                    NTC.SetSymptomTrue(C, "nausea", 2);

                    if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                    {
                        NTC.SetSymptomTrue(C, "legswelling", 2);
                    }

                    // Vomiting
                    if (!NTC.HasSymptom(C, "vomiting") && HF.Chance((float)(AffData.Strength - 60) / 40f * 0.07f))
                    {
                        NTC.SetSymptomTrue(C, "vomiting", Rand.Range(3, 11));
                    }

                    // Bone Damage
                    if (AffData.Strength > 70)
                    {
                        C.GetAffData("bonedamage").Strength += ((AffData.Strength - 70) / 30 * 0.15 * NT.DeltaTime);
                    }
                }
            };

        // Kidney Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: None; Kidney Damage 100% via Removal Surgery causes effects.
        AfflictionsToAdd["kidneyremoved"] = new("kidneyremoved", 0, 100, 0);

        // Kidney Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd["kidneyswap"] = new("kidneyswap", 0, 100, 0);

        // Liver Damage
        // Type: Non-Limb Specific, Organ Damage
        // Constant for Regeneration
        // Caused By: ABX, LiOxy, HemoTransShock, RadSickness, Sepsis, Hypoxemia, Drunk, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
        // Effects: Leg Swelling, Internal Bleeding, Vomiting Blood, Hypertension, Neurotrauma, AbdomDiscomfort, Jaundice, Bloating.
        AfflictionsToAdd["liverdamage"] = new("liverdamage", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["liverdamage"].Const = true;
        AfflictionsToAdd["liverdamage"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                double LiverDamage = HF.OrganDamageCalc(C, AffData.Strength + NTC.GetMultiplier(C, "liverdamagegain") * C.GetDoubleStatStrength("neworgandamage"));

                // Passive Regeneration / Increase
                AffData.Strength = LiverDamage;

                // Effects:
                // Neurotrauma
                double NeurotraumaIncrease = (AffData.Strength / 800.0 * NT.DeltaTime)
                    * NTC.GetMultiplier(C, "neurotraumagain")
                    * NTConfig.Get("NT_neurotraumaGain", 1)
                    * (1 - Math.Clamp(C.GetAffData("afmannitol").Strength, 0, 0.5));

                C.GetAffData("neurotrauma").Strength += NeurotraumaIncrease;

                // Hypertension (in BloodPressure constant itself)

                // Leg Swelling
                if (AffData.Strength > 40)
                {
                    if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                    {
                        NTC.SetSymptomTrue(C, "legswelling", 2);
                    }

                    if (AffData.Strength > 50)
                    {
                        // Bloating
                        NTC.SetSymptomTrue(C, "bloating", 2);

                        if (AffData.Strength > 65)
                        {
                            // Abdominal Discomfort
                            if (C.GetAffData("unconsciousness").Strength <= 0)
                            {
                                NTC.SetSymptomTrue(C, "abdominaldiscomfort", 2);
                            }

                            if (AffData.Strength > 80)
                            {
                                // Jaundice
                                NTC.SetSymptomTrue(C, "jaundice", 2);

                                if (AffData.Strength >= 99 && HF.Chance(0.05f))
                                {
                                    // Internal Bleeding & Vomiting Blood
                                    NTC.SetSymptomTrue(C, "vomitingblood", Random.Shared.Next(3, 10));
                                    C.GetAffData("internalbleeding").Strength += 2;
                                }
                            }
                        }
                    }
                }
            };

        // Organ Damage
        // Type: Non-Limb Specific, Organ Damage
        // Not constant; gets applied by other sources.
        // Caused By: Many things.
        // Effects: Many things.
        AfflictionsToAdd["organdamage"] = new("organdamage", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["organdamage"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
            };

        // Liver Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: None; Liver Damage 100% via Removal Surgery causes effects.
        AfflictionsToAdd["liverremoved"] = new("liverremoved", 0, 100, 0);

        // Liver Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd["liverswap"] = new("liverswap", 0, 100, 0);

        // Pneumothorax
        // Type: Non-Limb Specific
        // Not constant; gets applied by other sources.
        // Caused By: Rib Fracture, Trauma to the Torso, Needle application.
        // Effects: Shortness of Breath, Hyperventilation, Increased Heartrate
        // Additional interaction with: Needle.
        AfflictionsToAdd["pneumothorax"] = new("pneumothorax", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["pneumothorax"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Regeneration / Increase
                // Increases if there is no needle until 100%; else decreases until 5%.
                if (AffData.Strength > 0)
                {
                    AffData.Strength = Math.Clamp(AffData.Strength + NT.DeltaTime * (0.5 - HF.BoolToNum(AffData.Strength > 15) * Math.Clamp(C.GetAffData("needlec").Strength, 0, 1)),
                        0,
                        100
                    );
                }

                // Effects:
                // Increased Heartrate (in IncreasedHeartrate constant)

                // Hyperventilation
                if (AffData.Strength > 15)
                {
                    NTC.SetSymptomTrue(C.Human, "hyperventilation", 2);

                    // Shortness of Breath
                    if (AffData.Strength > 40 && C.GetAffData("respiratoryarrest").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "shortnessofbreath", 2);
                    }
                }
            };

        // =============== Limbs =============== //
        // Traumatic Right Arm Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing.
        AfflictionsToAdd["tra_amputation"] = new("tra_amputation", 0, 100, 0);

        // Traumatic Left Arm Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing.
        AfflictionsToAdd["tla_amputation"] = new("tla_amputation", 0, 100, 0);

        // Traumatic Right Leg Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing.
        AfflictionsToAdd["trl_amputation"] = new("trl_amputation", 0, 100, 0);

        // Traumatic Left Leg Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing.
        AfflictionsToAdd["tll_amputation"] = new("tll_amputation", 0, 100, 0);

        // Traumatic Head Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing. Act of removing the head kills instantly.
        AfflictionsToAdd["th_amputation"] = new("th_amputation", 0, 100, 0);

        // Surgical Right Arm Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing.
        AfflictionsToAdd["sra_amputation"] = new("sra_amputation", 0, 100, 0);

        // Surgical Left Arm Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing.
        AfflictionsToAdd["sla_amputation"] = new("sla_amputation", 0, 100, 0);

        // Surgical Right Leg Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing.
        AfflictionsToAdd["srl_amputation"] = new("srl_amputation", 0, 100, 0);

        // Surgical Left Leg Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing.
        AfflictionsToAdd["sll_amputation"] = new("sll_amputation", 0, 100, 0);

        // Surgical Head Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing. Act of removing the head kills instantly.
        AfflictionsToAdd["sh_amputation"] = new("sh_amputation", 0, 100, 0);

        // =============== Utility =============== //
        // Luabotomy
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: None.
        // Used to determine whether or not someone should be updated.
        AfflictionsToAdd["luabotomy"] = new("luabotomy", 0, 15, 0, AfflictionPriority.LOW);
        AfflictionsToAdd["luabotomy"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Keep it low if everything works properly. Else, increase until it shows on the UI.
                AffData.Strength = 0.1f;
            };

        // Luabotomy Purger
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Removes Luabotomy, then itself.
        AfflictionsToAdd["luabotomypurger"] = new("luabotomypurger", 0, 2, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["luabotomypurger"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Removes Luabotomy and itself; originally done in XML
                C.GetAffData("luabotomy").Strength = 0;
                AffData.Strength = 0;
            };

        // StopCreatureAbuse
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: None.
        // Used to decrease additional fall damage for certain creatures.
        AfflictionsToAdd["stopcreatureabuse"] = new("stopcreatureabuse", 0, 2, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["stopcreatureabuse"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Decreases itself by 1 per 2 seconds, removing itself after 4 seconds; originally done in XML
                AffData.Strength -= 1;
            };

        // TShockTimeout
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Removes Traumatic Shock.
        // Applied during level change to prevent Traumatic Shock from taking place.
        AfflictionsToAdd["tshocktimeout"] = new("tshocktimeout", 0, 300, 0, AfflictionPriority.LOW);
        AfflictionsToAdd["tshocktimeout"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Ticks every 6 seconds for 6 strength. Max strength is 300 so it lasts for 5 minutes unless removed early.
                AffData.Strength -= 6;
            };

        // GiveIn
        // Type: Functionality
        // Effects: Enables give-in button.
        // Allows you to die while stuck in certain afflictions like Spinal Cord Injury, which are not lethal yet prevent character use.
        AfflictionsToAdd["givein"] = new("givein", 0, 2, 0);
        AfflictionsToAdd["givein"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                AffData.Strength -= 1;
            };

        // CPR Buff
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Decreases Cardiac Arrest and Fibrillation while increasing Blood Pressure.
        // Originally done in XML.
        AfflictionsToAdd["cpr_buff"] = new("cpr_buff", 0, 2, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["cpr_buff"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                AffData.Strength -= 1;

                // Effects:
                // Reduce Cardiac Arrest
                C.GetAffData("cardiacarrest").Strength = Math.Max(0, C.GetAffData("cardiacarrest").Strength - 4);

                // Reduce Fibrillation
                C.GetAffData("fibrillation").Strength = Math.Max(0, C.GetAffData("fibrillation").Strength - 2);

                // Increase Blood Pressure
                C.GetAffData("bloodpressure").Strength += 8;

                // If Cardiac Arrest is above 0 and below or equal to 0.5, clear it and apply Fibrillation
                double CardiacArrest = C.GetAffData("cardiacarrest").Strength;
                if (CardiacArrest > 0 && CardiacArrest <= 0.5)
                {
                    C.GetAffData("cardiacarrest").Strength = 0;
                    C.GetAffData("fibrillation").Strength += 20;
                }
            };

        // CPR Buff AutoPulse
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Decreases Cardiac Arrest and Fibrillation while increasing Blood Pressure.
        // Originally done in XML.
        AfflictionsToAdd["cpr_buff_auto"] = new("cpr_buff_auto", 0, 2, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["cpr_buff_auto"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                AffData.Strength -= 1;

                // Effects:
                // Reduce Cardiac Arrest
                C.GetAffData("cardiacarrest").Strength = Math.Max(0, C.GetAffData("cardiacarrest").Strength - 3);

                // Reduce Fibrillation
                C.GetAffData("cardiacarrest").Strength = Math.Max(0, C.GetNonLimbAffData("fibrillation").Strength - 2);

                // Increase Blood Pressure
                C.GetAffData("bloodpressure").Strength += 10;

                // Reduce Oxygen Low
                C.GetAffData("oxygenlow").Strength -= Math.Max(0, C.GetAffData("oxygenlow").Strength - 6);

                // If Cardiac Arrest is above 0 and below or equal to 0.5, clear it and apply Fibrillation
                double CardiacArrest = C.GetAffData("cardiacarrest").Strength;
                if (CardiacArrest > 0 && CardiacArrest <= 0.5)
                {
                    C.GetAffData("cardiacarrest").Strength = 0;
                    C.GetAffData("fibrillation").Strength += 20;
                }
            };

        // CPR Fracture Buff
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Prevents Fractures from fall damage during CPR.
        // Originally done in XML.
        AfflictionsToAdd["cpr_fracturebuff"] = new("cpr_fracturebuff", 0, 2, 0);
        AfflictionsToAdd["cpr_fracturebuff"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Decreases itself by 1 per 2 seconds, removing itself after 4 seconds; originally done in XML
                AffData.Strength -= 1;
            };

        // Stasis Bag Overlay
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Overlays the stasis bag sprite.
        // Applied via Stasis Bag item.
        AfflictionsToAdd["stasisbagoverlay"] = new("stasisbagoverlay", 0, 2, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["stasisbagoverlay"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Removal Conditions
                // Originally removed itself every second in XML only to be added again every tick. 
                AffData.Strength = HF.BoolToNum(HF.GetOuterWearIdentifier(C.Human) == "stasisbag", 2);
            };

        // BodyBag Overlay
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Overlays the body bag sprite.
        // Applied via Body Bag item.
        AfflictionsToAdd["bodybagoverlay"] = new("bodybagoverlay", 0, 2, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["bodybagoverlay"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
           {
               // Removal Conditions
               // Originally removed itself every second in XML only to be added again every tick. 
               AffData.Strength = HF.BoolToNum(HF.GetOuterWearIdentifier(C.Human) == "bodybag", 2);
           };

        // Stasis
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Enables Stasis stattype.
        // Applied via Stasis Bag item.
        AfflictionsToAdd["stasis"] = new("stasis", 0, 3, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["stasis"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                AffData.Strength -= 2;

                // Reduce Husk Infection if below 100
                if (HF.HasAffliction(C.Human, "huskinfection") && C.GetAffData("huskinfection").Strength < 100)
                {
                    C.GetAffData("huskinfection").Strength -= 0.15;

                    // Additional reduction if no Husk Infection Resistance
                    if (!HF.HasAffliction(C.Human, "huskinfectionresistance") || C.GetAffData("huskinfectionresistance").Strength <= 0)
                    {
                        C.GetAffData("huskinfection").Strength -= 0.15;
                    }
                }
            };

        // Locked Hands
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Prevent usage of the left/right arms when triggered.
        AfflictionsToAdd["lockedhands"] = new("lockedhands", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["lockedhands"].Const = true;
        AfflictionsToAdd["lockedhands"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Arm lock items
                Item? LeftLockItem = HF.GetItemInLeftHand(C.Human);
                if (LeftLockItem?.Prefab.Identifier.Value != "armlock")
                {
                    LeftLockItem = null;
                }

                Item? RightLockItem = HF.GetItemInRightHand(C.Human);
                if (RightLockItem?.Prefab.Identifier.Value != "armlock")
                {
                    RightLockItem = null;
                }

                // Handcuffs Check
                Item Handcuffs = C.Human.Inventory.FindItemByIdentifier("handcuffs", false);
                bool Handcuffed = Handcuffs != null && C.Human.Inventory.FindIndex(Handcuffs) <= 6;

                if (Handcuffed)
                {
                    // Drop non-handcuff items
                    Item LeftHandItem = HF.GetItemInLeftHand(C.Human);
                    Item RightHandItem = HF.GetItemInRightHand(C.Human);

                    if (RightHandItem != null && LeftHandItem != Handcuffs && LeftLockItem == null)
                    {
                        LeftHandItem.Drop(C.Human);
                    }

                    if (RightHandItem != null && RightHandItem != Handcuffs && RightLockItem == null)
                    {
                        RightHandItem.Drop(C.Human);
                    }
                }

                bool LeftArmLocked = LeftLockItem != null && !Handcuffed;
                bool RightArmLocked = RightLockItem != null && !Handcuffed;

                if (LeftArmLocked && !C.GetBoolStatStrength("lockleftarm"))
                {
                    HF.RemoveItem(LeftLockItem);
                }

                if (RightArmLocked && !C.GetBoolStatStrength("lockrightarm"))
                {
                    HF.RemoveItem(RightLockItem);
                }

                if (!LeftArmLocked && C.GetBoolStatStrength("lockleftarm"))
                {
                    HF.ForceArmLock(C.Human, "LeftArm");
                }

                if (!RightArmLocked && C.GetBoolStatStrength("lockrightarm"))
                {
                    HF.ForceArmLock(C.Human, "RightArm");
                }

                AffData.Strength = HF.BoolToNum((C.GetBoolStatStrength("lockleftarm") && C.GetBoolStatStrength("lockrightarm")) || Handcuffed, 100);
            };

        // TraumaticAmputating Left Leg + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_ll"] = new("gate_ta_ll");

        // TraumaticAmputating Right Leg + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_rl"] = new("gate_ta_rl");

        // TraumaticAmputating Left Arm + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_la"] = new("gate_ta_la");

        // TraumaticAmputating Right Arm + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_ra"] = new("gate_ta_ra");

        // TraumaticAmputating Head + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Kills you ontop of spawning a severed head.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_h"] = new("gate_ta_h");

        // TraumaticAmputating Left Leg
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_ll_2"] = new("gate_ta_ll_2");

        // TraumaticAmputating Right Leg
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_rl_2"] = new("gate_ta_rl_2");

        // TraumaticAmputating Left Arm
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_la_2"] = new("gate_ta_la_2");

        // TraumaticAmputating Right Arm
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_ra_2"] = new("gate_ta_ra_2");

        // TraumaticAmputating Head
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Kills you.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd["gate_ta_h_2"] = new("gate_ta_h_2");

        // Opioids in Blood
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Opioids
        // Effects: Hypoventilation.
        AfflictionsToAdd["afopioid"] = new("afopioid", 0, 200, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afopioid"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Decreases itself by 0.3/s or 0.6/2s; originally done in XML
                AffData.Strength -= 0.6f;

                // Effects:
                // Hypoventilation
                if (AffData.Strength > 1)
                {
                    NTC.SetSymptomTrue(C.Human, "hypoventilation", 2);
                }

            };

        // Anaesthetic in Blood
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Propofol
        // Effects: Hypoventilation.
        AfflictionsToAdd["afanaesthetic"] = new("afanaesthetic", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afanaesthetic"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Decreases itself by 0.3/s or 0.6/2s; originally done in XML
                AffData.Strength -= 0.6f;

                // Effects:
                // Hypoventilation
                if (AffData.Strength > 1)
                {
                    NTC.SetSymptomTrue(C.Human, "hypoventilation", 2);
                }

            };

        // Safe Surgery (via Surgery Table)
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Surgery Table / Hospital Bed
        // Effects: Reduces / Prevents Traumatic Shock.
        AfflictionsToAdd["safesurgery"] = new("safesurgery", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["safesurgery"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
           {
               // Passive Decrease
               // Originally had a maxstrength of 3, and reduced by 1 per second in XML.
               // Adjusted, that became 60 per 2 seconds (so technically, it takes 4 seconds to fully vanish).
               AffData.Strength -= 60;
           };

        // Artificial Ventilation (via Surgery Table)
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Surgery Table / Hospital Bed
        // Effects: Reduces Oxygen Low
        AfflictionsToAdd["artificialventilation"] = new("artificialventilation", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["artificialventilation"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
           {
               // Passive Decrease
               // Originally had a maxstrength of 100, and reduced by 20 per second in XML.
               // Adjusted, that became 40 per 2 seconds (so technically, it takes 6 seconds to fully vanish).
               AffData.Strength -= 40;

               // Reduce Oxygen Low if lungs are present
               if (C.GetAffData("lungremoved").Strength <= 0)
               {
                   C.GetAffData("oxygenlow").Strength -= 100;
               }
           };


        AfflictionsToAdd["chemwithdrawal"] = new("chemwithdrawal");
        AfflictionsToAdd["chemwithdrawal"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
            };


        AfflictionsToAdd["opiatewithdrawal"] = new("opiatewithdrawal");
        AfflictionsToAdd["opiatewithdrawal"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
            };

        // Alcohol Addiction
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Consuming alcohol.
        // Effects: Alcohol Withdrawal if not eternally drinking (XML).
        AfflictionsToAdd["alcoholaddiction"] = new("alcoholaddiction", 0, 100, 0);

        // Alcohol Withdrawal
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Not consuming Alcohol with an addiction (applies via XML).
        // Effects: Craving, Sweating, Nausea, Fever, Vomiting, Headache, Confusion, Increased Heartrate, Seizure, Hypertension. 
        AfflictionsToAdd["alcoholwithdrawal"] = new("alcoholwithdrawal");
        AfflictionsToAdd["alcoholwithdrawal"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Effects:
                // Hypertension (in BloodPressure constant)
                // Increased Heartrate (in IncreasedHeartrate constant)

                // Craving
                if (AffData.Strength > 20)
                {
                    if (C.GetAffData("unconsciousness").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "craving", 2);
                    }

                    // Sweating
                    if (AffData.Strength > 30)
                    {
                        NTC.SetSymptomTrue(C, "sweating", 2);

                        // Nausea
                        if (AffData.Strength > 40)
                        {
                            NTC.SetSymptomTrue(C, "nausea", 2);

                            if (AffData.Strength > 50)
                            {
                                // Headache
                                if (C.GetAffData("unconsciousness").Strength <= 0)
                                {
                                    NTC.SetSymptomTrue(C, "headache", 2);
                                }

                                // Seizure
                                if (HF.Chance((float)AffData.Strength / 1000f))
                                {
                                    C.GetAffData("seizure").Strength += 10;
                                }

                                // Vomiting
                                if (AffData.Strength > 60)
                                {
                                    NTC.SetSymptomTrue(C, "vomiting", 2);

                                    // Confusion
                                    if (AffData.Strength > 80 && C.GetAffData("unconsciousness").Strength <= 0)
                                    {
                                        NTC.SetSymptomTrue(C, "confusion", 2);

                                        // Fever
                                        if (AffData.Strength > 90)
                                        {
                                            NTC.SetSymptomTrue(C, "fever", 2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

        // On Fire!
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Being on fire too long.
        // Effects: Visibly on fire (XML), burns (XML).
        AfflictionsToAdd["onfire"] = new("onfire", 0, 1, 0);

        // Screaming
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Fractures, Amputations, Dislocations.
        // Effects: Character screams (XML).
        AfflictionsToAdd["screaming"] = new("screaming", 0, 1, 0);

        // Severe Pain
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Fractures, Amputations, Dislocations.
        // Effects: Character screams (XML), gets momentarily stunned (XML).
        AfflictionsToAdd["severepain"] = new("severepain", 0, 2, 0);

        // Pain
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Damage.
        // Effects: Damage Sounds (XML), Slowdown (XML). Removes self via XML.
        AfflictionsToAdd["pain"] = new("pain", 0, 2, 0);

        // Shock Pain
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Traumatic Shock.
        // Effects: Damage Sounds (XML), Slowdown (XML). Removes self via XML.
        AfflictionsToAdd["shockpain"] = new("shockpain", 0, 2, 0);

        // Analgesia
        // Not constant; gets applied by other sources.
        // Type: Functionality, Surgery, Buff
        // Caused By: Painkillers.
        // Effects: Damage resistance (XML), allows surgery, reduces Pain (XML), applies screen changes (XML). Removes self via XML.
        AfflictionsToAdd["analgesia"] = new("analgesia", 0, 100, 0);

        // Anesthesia
        // Not constant; gets applied by other sources.
        // Type: Functionality, Surgery
        // Caused By: Propofol.
        // Effects: Applies Analgesia (XML) and has side effects. Increases and removes self via XML.
        AfflictionsToAdd["anesthesia"] = new("anesthesia", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["anesthesia"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Apply random side-effects.
                if (!HF.Chance(0.06f)) return;

                double casecount = 7;
                double case_ = Random.Shared.NextDouble();

                if (case_ < 1 / casecount)
                {
                    NTC.SetSymptomTrue(C, "vomitingblood", (int)(5 + Random.Shared.NextDouble() * 10));
                }
                else if (case_ < 2 / casecount)
                {
                    if (C.GetAffData("unconsciousness").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "blurredvision", (int)(5 + Random.Shared.NextDouble() * 10));
                    }
                }
                else if (case_ < 3 / casecount)
                {
                    if (C.GetAffData("unconsciousness").Strength <= 0)
                    {
                        NTC.SetSymptomTrue(C, "confusion", (int)(5 + Random.Shared.NextDouble() * 10));
                    }
                }
                else if (case_ < 4 / casecount)
                {
                    NTC.SetSymptomTrue(C, "fever", (int)(5 + Random.Shared.NextDouble() * 10));
                }
                else if (case_ < 5 / casecount)
                {
                    NTC.SetSymptomTrue(C, "triggersym_seizure", (int)(1 + Random.Shared.NextDouble() * 2));
                }
                else if (case_ < 6 / casecount)
                {
                    HF.Fibrillate(C.Human, (float)(5 + Random.Shared.NextDouble() * 30));
                }
                else
                {
                    C.GetAffData("psychosis").Strength += 10;
                }
            };

        // =============== Head =============== //

        // Stroke
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Hypertension.
        // Effects: Headache, Coma, Seizure, Neurotrauma.
        AfflictionsToAdd["stroke"] = new("stroke", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["stroke"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                // Passive Regeneration
                AffData.Strength -= (1.0 / 20) * C.GetDoubleStatStrength("clottingrate") * NT.DeltaTime;

                // Effects:
                // Neurotrauma
                double NeurotraumaGain = Math.Clamp(AffData.Strength, 0, 20) * 0.1 * NT.DeltaTime
                    * NTC.GetMultiplier(C, "neurotraumagain")
                    * NTConfig.Get("NT_neurotraumaGain", 1)
                    * (1 - Math.Clamp(C.GetAffData("afmannitol").Strength, 0, 0.5));

                C.GetAffData("neurotrauma").Strength += NeurotraumaGain;

                // Headache
                if (AffData.Strength > 1 && C.GetAffData("unconsciousness").Strength <= 0)
                {
                    NTC.SetSymptomTrue(C, "headache", 2);
                }

                // Coma & Seizure
                if (AffData.Strength > 1 && HF.Chance(0.05f))
                {
                    C.GetAffData("coma").Strength += 14;
                    C.GetAffData("seizure").Strength += 10;
                }
            };

        // Neurotrauma
        // Constant for Regeneration
        // Type: Non-Limb Specific, Organ Damage, Lethal
        // Caused By: Stroke, Liver Damage, Kidney Damage, Sepsis, Hypoxemia, Items, Traumatic Shock, Cyanide Poisoning, GSW (DMG).
        // Effects: Unconsciousness, Respiratory Arrest
        AfflictionsToAdd["neurotrauma"] = new("neurotrauma", 0, 200, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["neurotrauma"].Const = true;
        AfflictionsToAdd["neurotrauma"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                // Does not regenerate with a Skull Fracture
                bool HasFracture = HF.HasAfflictionLimb(C.Human, "fracturedskull", LimbType.Head, 1);
                double FractureModifier = HasFracture ? 0 : 1;

                double PassiveRegeneration = -0.1 * C.GetDoubleStatStrength("healingrate") * FractureModifier * NT.DeltaTime;

                if (PassiveRegeneration < -0.08 * NT.DeltaTime)
                {
                    PassiveRegeneration *= 2.5;
                }

                AffData.Strength = Math.Clamp(AffData.Strength + PassiveRegeneration, 0, 200);

                // Effects:
                // Unconsciousness & Respiratory Arrest
                if (AffData.Strength > 100)
                {
                    NTC.SetSymptomTrue(C, "unconsciousness", 2);
                    if (HF.Chance(0.05f))
                    {
                        C.GetAffData("respiratoryarrest").Strength += 200;
                    }
                }
            };

        // Seizure
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Stroke, Acidosis, Alkalosis, Withdrawal, Opiate Overdose, Anesthesia, Radiation Sickness
        // Effects: Unconsciousness, Spasms
        AfflictionsToAdd["seizure"] = new("seizure", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["seizure"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Regeneration:
                AffData.Strength -= NT.DeltaTime;

                // Effects:
                // Spasms
                if (AffData.Strength > 0.1)
                {
                    NTC.SetSymptomTrue(C, "unconsciousness", 2);

                    foreach (LimbType type in Enum.GetValues<LimbType>())
                    {
                        HF.AddAfflictionLimb(C.Human, "spasm", type, 10, null);
                    }
                }
            };

        // Coma
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Stroke, Cardiac Arrest, High Acidosis, Morbusine Poisoning, Naloxone fail.
        // Effects: Cardiac Arrest, Unconsciousness.
        AfflictionsToAdd["coma"] = new("coma", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["coma"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                // Passive Regeneration
                if (C.GetAffData("acidosis").Strength < 20
                    && C.GetAffData("alkalosis").Strength < 20
                    && C.GetAffData("heartdamage").Strength < 30
                    && C.GetAffData("lungdamage").Strength < 40
                    && C.GetDoubleStatStrength("availableoxygen") > 60)
                {
                    AffData.Strength -= NT.DeltaTime / 2;
                }
                else
                {
                    AffData.Strength -= NT.DeltaTime / 5;
                }

                // Effects:


                // Unconsciousness
                if (AffData.Strength > 15)
                {
                    NTC.SetSymptomTrue(C, "unconsciousness", 2);

                    // Cardiac Arrest
                    if (AffData.Strength > 40 && HF.Chance(0.03f))
                    {
                        C.GetAffData("cardiacarrest").Strength += 200;
                    }
                }
            };

        // Spinal Cord Injury
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Unstable Neck Fractures.
        // Effects: Paralysis (XML), Analgesia (XML).
        AfflictionsToAdd["spinalcordinjury"] = new("spinalcordinjury", 0, 100, 0);

        // Carotid Arterial Cut
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Damage.
        // Effects: Blood Loss (XML), Internal Bleeding (XML). Increases self via XML.
        AfflictionsToAdd["carotidarterialcut"] = new("carotidarterialcut", 0, 100, 0);

        // =============== Item Derived =============== //

        // Adrenaline in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Adrenaline item.
        // Effects: Melee Damage increased (XML), Analgesia (XML).
        AfflictionsToAdd["afadrenaline"] = new("afadrenaline", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afadrenaline"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 1 per second in XML.
                // Adjusted, that became 2 per 2 seconds.
                AffData.Strength -= 2;
            };

        // Needle in Chest
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Needle item.
        // Effects: Reduced Pneumothorax / Cardiac Tamponade.
        AfflictionsToAdd["needlec"] = new("needlec", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["needlec"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                AffData.Strength -= 0.15 * NT.DeltaTime;
            };

        // Saline in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Saline item.
        // Effects: Increased Acidosis, Blood Pressure.
        AfflictionsToAdd["afsaline"] = new("afsaline", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afsaline"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                AffData.Strength -= 0.5;

                // Effects:
                // Acidosis
                C.GetAffData("acidosis").Strength += 0.2;

                // Blood Pressure (in BloodPressure constant)
            };

        // Ringers Solution in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Ringer's Solution item.
        // Effects: Increased Alkalosis, Blood Pressure.
        AfflictionsToAdd["afringerssolution"] = new("afringerssolution", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afringerssolution"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                AffData.Strength -= 0.5;

                // Effects:
                // Alkalosis
                C.GetAffData("acidosis").Strength += 0.2;

                // Blood Pressure (in BloodPressure constant)
            };


        // Mannitol in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Mannitol Item.
        // Effects: Reduce Neurotrauma.
        AfflictionsToAdd["afmannitol"] = new("afmannitol", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afmannitol"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.5 per second in XML.
                // Adjusted, that became 1 per 2 seconds.
                AffData.Strength -= 1;

                // Effects:
                // Reduce Neurotrauma if Blood Pressure and Hypoxemia conditions are met.
                if (C.GetAffData("bloodpressure").Strength >= 70 && C.GetAffData("hypoxemia").Strength <= 30)
                {
                    C.GetAffData("neurotrauma").Strength -= 2;
                }
            };

        // Immunosuppressants in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Azathioprine Item.
        // Effects: Reduce Immunity.
        AfflictionsToAdd["afimmunosuppressant"] = new("afimmunosuppressant", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afimmunosuppressant"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                AffData.Strength -= 0.5;

                // Effects:
                // Reduce Immunity
                if (C.GetAffData("immunity").Strength >= 2.5)
                {
                    C.GetAffData("immunity").Strength -= 8;
                }
            };

        // Pressure-increasing drugs in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Nitroglycerin, Sodium Nitroprusside Items.
        // Effects: Increase target Blood Pressure.
        AfflictionsToAdd["afpressuredrug"] = new("afpressuredrug", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afpressuredrug"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                AffData.Strength -= 0.5;

                // Effects:
                // Blood Pressure (in BloodPressure constant)
            };

        // Thiamine in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Thiamine Item.
        // Effects: Increase specific organ damage healing.
        AfflictionsToAdd["afthiamine"] = new("afthiamine", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afthiamine"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
           {
               // Passive Decrease
               // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
               // Adjusted, that became 0.5 per 2 seconds.
               AffData.Strength -= 0.5;

               // Effects:
               // Additional Healing (in HF.NewOrganDamage)
           };

        // Streptokinase in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Streptokinase Item.
        // Effects: Increase stroke chance, cure Heart Attack / Hemotransfusion shock.
        AfflictionsToAdd["afstreptokinase"] = new("afstreptokinase", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afstreptokinase"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
           {
               // Passive Decrease
               // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
               // Adjusted, that became 0.5 per 2 seconds.
               AffData.Strength -= 0.5;

               // Effects:
               // Cures Heart Attack / HemoTransShock in ItemFunctions
               // Hypertension Stroke (in BloodPressure constant)
           };

        // Antibiotics in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Broad-Spectrum Antibiotics Item.
        // Effects: Decreases Sepsis, extra Organ Damage, decreased Husk Infection.
        AfflictionsToAdd["afantibiotics"] = new("afantibiotics", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["afantibiotics"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
           {
               // Passive Decrease
               // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
               // Adjusted, that became 0.5 per 2 seconds.
               AffData.Strength -= 0.5;

               // Effects:
               // Specific Organ Damage
               C.GetAffData("organdamage").Strength += 0.4;
               C.GetAffData("kidneydamage").Strength += 0.35;
               C.GetAffData("liverdamage").Strength += 0.35;
               C.GetAffData("heartdamage").Strength += 0.2;
               C.GetAffData("lungdamage").Strength += 0.2;

               // Reduce Husk Infection
               if (HF.HasAffliction(C.Human, "huskinfection") && C.GetAffData("huskinfection").Strength < 75)
               {
                   C.GetAffData("huskinfection").Strength -= 1;
               }

               // Sepsis
               if (C.GetAffData("sepsis").Strength > 0)
               {
                   C.GetAffData("sepsis").Strength -= 2;
               }
           };

        // =============== Surgical =============== //
        // Cavity Cleaning
        // Not constant; gets applied by other sources
        // Type: Surgery, Non-Limb Specific
        // Caused By: Antiseptic Sprayer + Saline
        // Effects: Cures Infected Cavity
        AfflictionsToAdd["caviclean"] = new("caviclean", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["caviclean"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
           {
               // Once it hits 100, remove itself and infected cavity.
               if (AffData.Strength == 100)
               {
                   AffData.Strength = 0;
                   C.GetAffData("infectedcavity").Strength = 0;
                   return;
               }
           };


        // Traumatic Shock
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Lethal
        // Caused By: Unsafe Surgery
        // Effects: Hypotension, Cardiac Arrest, Respiratory Arrest, Neurotrauma, Psychosis, Pain.
        AfflictionsToAdd["traumaticshock"] = new("traumaticshock", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["traumaticshock"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Removes on TShockTimeout
                if (C.GetAffData("tshocktimeout").Strength > 0)
                {
                    AffData.Strength = 0;
                    return;
                }

                // Passive Decrease
                bool IsSedated = C.GetBoolStatStrength("sedated");
                bool IsSafeSurgery = C.GetAffData("safesurgery").Strength > 0;
                bool IsAnesthesized = C.GetAffData("anesthesia").Strength > 15;

                bool ShouldReduce = (IsSedated && IsSafeSurgery || IsAnesthesized);

                AffData.Strength -= (0.5 + HF.BoolToNum(ShouldReduce, 1.5f)) * NT.DeltaTime;

                // Effects:
                // Pain & Psychosis
                if (AffData.Strength > 5)
                {
                    if (C.GetSymptomAffData("unconsciousness").Strength < 0.1)
                    {
                        C.GetAffData("shockpain").Strength += (10 * NT.DeltaTime);
                        C.GetAffData("psychosis").Strength += (AffData.Strength / 100 * NT.DeltaTime);
                    }

                    // Respiratory Arrest
                    if (AffData.Strength > 30 && HF.Chance(0.2f))
                    {
                        C.GetAffData("respiratoryarrest").Strength += 200;
                    }

                    // Cardiac Arrest
                    if (AffData.Strength > 40 && HF.Chance(0.1f))
                    {
                        C.GetAffData("cardiacarrest").Strength += 200;
                    }
                }
            };

        // Ballooned Aorta
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Endovascular Balloon item.
        // Effects: Gangrene in extremities (XML), Organ Damage, Specific Organ Damage, Reduced Bleeding in extremities (XML).
        AfflictionsToAdd["balloonedaorta"] = new("balloonedaorta", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["balloonedaorta"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Effects:
                // Vanilla Organ Damage
                C.GetAffData("organdamage").Strength += 1;

                // Specific Organ Damage
                C.GetAffData("liverdamage").Strength += 1;
                C.GetAffData("kidneydamage").Strength += 1;
            };

        // =============== Torso =============== //
        // Aortic Rupture
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Damage.
        // Effects: Blood Loss (XML), Internal Bleeding (XML), Chest Pain, Abdominal Pain, Unconsciousness.
        AfflictionsToAdd["aorticrupture"] = new("aorticrupture", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["aorticrupture"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Chest Pain & Abdominal Pain & Unconsciousness
                if (AffData.Strength > 0)
                {
                    if (C.GetSymptomAffData("unconsciousness").Strength <= 0 && (!C.GetBoolStatStrength("sedated")))
                    {
                        NTC.SetSymptomTrue(C, "chestpain", 2);
                        NTC.SetSymptomTrue(C, "abdominalpain", 2);
                    }
                }
            };

        // Internal Bleeding
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Damage.
        // Effects: Blood Loss (XML), Internal Bleeding (XML), Chest Pain, Abdominal Pain, Unconsciousness.
        AfflictionsToAdd["internalbleeding"] = new("internalbleeding", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["internalbleeding"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                // Passive Regeneration
                AffData.Strength -= NT.DeltaTime * 0.02 * C.GetDoubleStatStrength("clottingrate");

                // Effects:
                // Blood Loss
                if (AffData.Strength > 0)
                {
                    C.GetAffData("bloodloss").Strength += (AffData.Strength * (1f / 40f) * NT.DeltaTime);

                    // Vomiting Blood
                    if (AffData.Strength > 50)
                    {
                        NTC.SetSymptomTrue(C, "vomitingblood", 2);
                    }
                }
            };

        // =============== Bones =============== //

        // Bone Damage
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Kidney Damage, Radiation Sickness, Sepsis, Hypoxemia.
        // Effects: Bone Death, Fractures.
        AfflictionsToAdd["bonedamage"] = new("bonedamage", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["bonedamage"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                // Passive Regeneration
                AffData.Strength = HF.OrganDamageCalc(C, AffData.Strength);

                // Bone Regeneration
                if (AffData.Strength < 90)
                {
                    AffData.Strength -= C.GetDoubleStatStrength("bonegrowthCount") * 0.3 * NT.DeltaTime;
                }
                else if (C.GetDoubleStatStrength("bonegrowthCount") >= 6)
                {
                    AffData.Strength -= 2 * NT.DeltaTime;
                }

                // Fractures
                if (AffData.Strength > 90 && HF.Chance(0.01f))
                {
                    HF.BreakLimb(C.Human, Limb);
                }
            };

        // =============== MUST RUN AFTER EVERYTHING ELSE =============== //
        // Probably needs even special treatment than this.

        // Slowdown 
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Decreases character by a percentage proportional to the affliction strength.
        AfflictionsToAdd["slowdown"] = new("slowdown", 0, 100, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["slowdown"].Const = true;
        AfflictionsToAdd["slowdown"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                AffData.Strength = C.GetDoubleStatStrength("slowdown");
            };

        // Stun 
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Used to stun the character.
        AfflictionsToAdd["stun"] = new("stun", 0, 30, 0, AfflictionPriority.HIGH);
        AfflictionsToAdd["stun"].Const = true;
        AfflictionsToAdd["stun"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanNonLimbAffData AffData) =>
            {
                if (C.GetNonLimbAffStrength("spinalcordinjury") > 0
                    || C.GetNonLimbAffStrength("anesthesia") > 15
                    || NTC.HasSymptom(C, "unconsciousness"))
                {
                    AffData.Strength = Math.Max(5, AffData.Strength);
                }
                else
                {
                    AffData.Strength = 0;
                }
            };

        // Stun 
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Used to stun the character.
        AfflictionsToAdd["combatstimulant"] = new("combatstimulant", 0, 100, 0, AfflictionPriority.HIGH);

        // Now add these afflictions.
        foreach (KeyValuePair<string, NTNonLimbAffliction> Pair in AfflictionsToAdd)
        {
            NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
        }

        // Surgical Incision
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Scalpel.
        // Effects: Blood Loss, Traumatic Shock, increases self in XML.
        LimbAfflictionsToAdd["surgeryincision"] = new("surgeryincision", 0, 100, 0);

        // Clamped Bleeding
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Hemostat.
        // Effects: Prevents Surgery Incision Blood Loss (Scalpel XML).
        LimbAfflictionsToAdd["clampedbleeding"] = new("clampedbleeding", 0, 100, 0);

        // Drilled Bones
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Surgical Drill.
        // Effects: Applies Traumatic Shock (XML).
        LimbAfflictionsToAdd["drilledbones"] = new("drilledbones", 0, 100, 0);

        // Retracted Skin
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Skin Retractors.
        // Effects: Applies Traumatic Shock (XML).
        LimbAfflictionsToAdd["retractedskin"] = new("retractedskin", 0, 100, 0);

        // Sutured Incision
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Stitching a Surgical Incision.
        // Effects: None.
        LimbAfflictionsToAdd["suturedi"] = new("suturedi", 0, 100, 0, AfflictionPriority.MEDIUM);
        LimbAfflictionsToAdd["suturedi"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
           {
               // Passive Decrease
               // Originally had a maxstrength of 100, and reduced by 1 per second in XML.
               // Adjusted, that became 4 per 4 seconds.
               AffData.Strength[Limb] -= 4;
           };

        // Sutured Wound
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Stitching an Open Wound.
        // Effects: Vitality damage proportional to affliction strength.
        LimbAfflictionsToAdd["suturedw"] = new("suturedw", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["suturedi"].UpdateAction =
           (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
           {
               // Passive Decrease
               // Originally had a maxstrength of 100, and reduced by 1 per second in XML.
               // Adjusted, that became 0.44 per 2 seconds.
               AffData.Strength[Limb] -= 0.4;
           };

        // Sawed Bones
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Surgical Saw.
        // Effects: Applies Traumatic Shock (XML).
        LimbAfflictionsToAdd["sawedbones"] = new("sawedbones", 0, 100, 0);

        // Bleeding
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Basegame Override
        // Caused By: Damage, failed skill checks.
        // Effects: Blood Loss (Hardcoded?)
        LimbAfflictionsToAdd["bleeding"] = new("bleeding", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["bleeding"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Passive Regeneration
                AffData.Strength[Limb] -= (C.GetDoubleStatStrength("clottingrate") * 0.1
                    + Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 0.5
                    + Math.Clamp(C.GetLimbAffStrength("bandageddirty", Limb), 0, 1) * 0.25
                ) * NT.DeltaTime;
            };

        // Stimulated Bone Growth
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, failed skill checks.
        // Effects: Decreases bone damage.
        LimbAfflictionsToAdd["stimulatedbonegrowth"] = new("stimulatedbonegrowth", 0, 100, 0, AfflictionPriority.MEDIUM);
        LimbAfflictionsToAdd["stimulatedbonegrowth"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Passive Regeneration
                // Originally had a maxstrength of 100, and reduced by 0.5 per second in XML.
                // Adjusted, that became 1 per 2 seconds.
                AffData.Strength[Limb] -= 1;
            };

        // Arm + Leg Fractures
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, failed skill checks.
        // Effects: Pain, lost ability of limb, Internal Damage.
        LimbAfflictionsToAdd["fracturedextremity"] = new("fracturedextremity", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["fracturedextremity"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                bool HasCast = HF.HasAfflictionLimb(C.Human, "plastercast", Limb);
                bool HasBandage = HF.HasAfflictionLimb(C.Human, "bandaged", Limb) || HF.HasAfflictionLimb(C.Human, "bandageddirty", Limb);

                // Arms: halt progression between 90-100 if bandaged
                if (Limb == LimbType.LeftArm || Limb == LimbType.RightArm)
                {
                    if (AffData.Strength[Limb] > 90 && AffData.Strength[Limb] < 100 && HasBandage)
                    {
                        return;
                    }
                }

                // Passive Increase if no cast
                AffData.Strength[Limb] += 2 * HF.BoolToNum(!HasCast) * NT.DeltaTime;

                // Legs: adrenaline causes Bleeding if no cast and not ragdolled
                if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
                {
                    if (!HasCast && HF.HasAffliction(C.Human, "afadrenaline", 1) && !C.Human.IsRagdolled)
                    {
                        HF.AddAfflictionLimb(C.Human, "bleeding", Limb, 15, null);
                    }
                }

                // Internal Damage if no cast
                if (!HasCast && !C.GetBoolStatStrength("sedated") && (HF.LimbIsExtremity(Limb) || !HasBandage))
                {
                    HF.AddAfflictionLimb(C.Human, "internaldamage", Limb, (float)(0.1 * NT.DeltaTime), null);
                }
            };

        // Arm + Leg Dislocation
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, failed skill checks.
        // Effects: Pain (XML), lost ability of limb, Internal Damage.
        LimbAfflictionsToAdd["dislocation"] = new("dislocation", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["dislocation"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // If painlessness is present, don't cause problems
                if (C.GetBoolStatStrength("sedated")) return;

                if (C.GetLimbAffStrength("plastercast", Limb) <= 0 && C.GetLimbAffStrength("bandaged", Limb) <= 0 && C.GetLimbAffStrength("bandageddirty", Limb) <= 0)
                {
                    HF.AddAfflictionLimb(C.Human, "internaldamage", Limb, (float)(0.1 * NT.DeltaTime), null);
                    if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
                    {
                        C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.8); // slow the character down.
                    }
                }
            };

        // Tourniquet around Extremity
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Tourniquet item.
        // Effects: Reduces Bleeding (XML), Gangrene.
        LimbAfflictionsToAdd["tourniqueted"] = new("tourniqueted", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["tourniqueted"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Effects:
                // Gangrene
                HF.AddAfflictionLimb(C.Human, "gangrene", Limb, (float)(HF.BoolToNum(HF.Chance(0.1f)) * 0.5 * NTConfig.Get("NT_gangrenespeed", 1) * NT.DeltaTime), null);
            };


        // Plaster Cast
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Gypsum item.
        // Effects: Heals fractures, slows character.
        LimbAfflictionsToAdd["plastercast"] = new("plastercast", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["plastercast"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Effects:
                // Leg slowdown
                if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
                {
                    C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.8);
                }

                // Heal Fracture
                HF.BreakLimb(C.Human, Limb, (float)(-(100.0 / 300.0) * NT.DeltaTime));
            };

        // Arterial Cut on Extremity
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage.
        // Effects: Blood Loss (XML).
        LimbAfflictionsToAdd["arterialcut"] = new("arterialcut", 0, 100, 0);

        // Gangrene
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Tourniquets, Sepsis, Aortic Balloon (XML).
        // Effects: Blood Loss (XML).
        LimbAfflictionsToAdd["gangrene"] = new("gangrene", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["gangrene"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Limb must be an extremity
                if (!HF.LimbIsExtremity(Limb)) return;

                // Surgical amputation prevents Gangrene on that stump
                if (HF.LimbIsSurgicallyAmputated(C.Human, Limb))
                {
                    AffData.Strength[Limb] = 0;
                    return;
                }

                // Passive Regeneration below 15
                if (AffData.Strength[Limb] < 15)
                {
                    AffData.Strength[Limb] -= 0.01 * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                }
            };

        // Bandage applied to Limb
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Bandage items.
        // Effects: Reduces bleeding, heals wounds, reduces infection.
        LimbAfflictionsToAdd["bandaged"] = new("bandaged", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["bandaged"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                double WoundDamage = C.GetLimbAffStrength("firstdegreeburn", Limb)
                    + C.GetLimbAffStrength("seconddegreeburn", Limb)
                    + C.GetLimbAffStrength("thirddegreeburn", Limb)
                    + C.GetLimbAffStrength("lacerations", Limb)
                    + C.GetLimbAffStrength("foreignbody", Limb)
                    + C.GetLimbAffStrength("arterialcut", Limb)
                    + C.GetLimbAffStrength("infectedwound", Limb);

                double BandageDirtifySpeed = 0.1
                    + Math.Clamp(WoundDamage / 100, 0, 0.4)
                    + C.GetLimbAffStrength("bleeding", Limb) / 20;

                // Dirtify bandage over time
                AffData.Strength[Limb] -= BandageDirtifySpeed * NT.DeltaTime;

                float DirtyBandageStrength = (float)C.GetLimbAffStrength("bandageddirty", Limb);

                // Transition to dirty bandage
                if (AffData.Strength[Limb] <= 0.5f)
                {
                    HF.SetAfflictionLimb(C.Human, "bandageddirty", Limb, (float)Math.Max(DirtyBandageStrength, 1), null);
                    AffData.Strength[Limb] = 0f;
                }

                if (DirtyBandageStrength > 0)
                {
                    HF.AddAfflictionLimb(C.Human, "bandageddirty", Limb, (float)(BandageDirtifySpeed * NT.DeltaTime), null);
                }

                // Effects:
                // Slowdown
                C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.9);

                // Wound Healing
                HF.AddAfflictionLimb(C.Human, "lacerations", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.1 * NT.DeltaTime), null);
                HF.AddAfflictionLimb(C.Human, "firstdegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.1 * NT.DeltaTime), null);
                HF.AddAfflictionLimb(C.Human, "seconddegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.1 * NT.DeltaTime), null);
                HF.AddAfflictionLimb(C.Human, "thirddegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.1 * NT.DeltaTime), null);

                // Infection Healing
                if (C.GetLimbAffStrength("infectedwound", Limb) > 0)
                {
                    HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 1.5 * NT.DeltaTime), null);
                }
            };

        // Dirty Bandage around Limb
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Dirtyfication.
        // Effects: Reduces bleeding, heals wounds, causes infection.
        LimbAfflictionsToAdd["bandageddirty"] = new("bandageddirty", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["bandageddirty"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                float BandagedStrength = (float)C.GetLimbAffStrength("bandaged", Limb);
                if (BandagedStrength > 0)
                {
                    HF.SetAfflictionLimb(C.Human, "bandaged", Limb, 0);
                }

                double WoundDamage = C.GetLimbAffStrength("firstdegreeburn", Limb)
                    + C.GetLimbAffStrength("seconddegreeburn", Limb)
                    + C.GetLimbAffStrength("thirddegreeburn", Limb)
                    + C.GetLimbAffStrength("lacerations", Limb)
                    + C.GetLimbAffStrength("foreignbody", Limb)
                    + C.GetLimbAffStrength("arterialcut", Limb)
                    + C.GetLimbAffStrength("infectedwound", Limb);

                double BandageDirtifySpeed = 0.1
                    + Math.Clamp(WoundDamage / 100, 0, 0.4)
                    + C.GetLimbAffStrength("bleeding", Limb) / 20;

                AffData.Strength[Limb] += BandageDirtifySpeed * NT.DeltaTime;

                // Effects:
                // Slowdown
                C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.9);

                // Wound Healing
                HF.AddAfflictionLimb(C.Human, "lacerations", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.05 * NT.DeltaTime), null);
                HF.AddAfflictionLimb(C.Human, "firstdegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.05 * NT.DeltaTime), null);
                HF.AddAfflictionLimb(C.Human, "seconddegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.05 * NT.DeltaTime), null);
                HF.AddAfflictionLimb(C.Human, "thirddegreeburn", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.05 * NT.DeltaTime), null);
            };

        // Gel Coolant Pack applied to Limb
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Gel Coolant Pack.
        // Effects: Amplifies healing, slows character.
        LimbAfflictionsToAdd["iced"] = new("iced", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["iced"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Passive Decrease
                AffData.Strength[Limb] -= 1.7 * NT.DeltaTime;

                // Effects:
                // Slowdown (5% per limb)
                C.SetDoubleStatStrength("speedmultiplier", C.GetDoubleStatStrength("speedmultiplier") * 0.95);

                // Effects:
                // Reduce Internal Bleeding if on Torso
                if (Limb == LimbType.Torso)
                {
                    C.GetAffData("internalbleeding").Strength -= (0.2 * NT.DeltaTime);
                }

                // Heal Blunt Force Trauma
                HF.AddAfflictionLimb(C.Human, "blunttrauma", Limb, (float)(-Math.Clamp(AffData.Strength[Limb], 0, 1) * 0.3 * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime), null);
            };

        // Antibiotic Ointment applied to Limb
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Antibiotic Ointment.
        // Effects: Amplifies healing.
        LimbAfflictionsToAdd["ointmented"] = new("ointmented", 0, 100, 0, AfflictionPriority.MEDIUM);
        LimbAfflictionsToAdd["ointmented"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Passive Decrease
                AffData.Strength[Limb] -= 1.2 * NT.DeltaTime;

                // Effects:
                // Reduce Infected Wounds
                if (C.GetLimbAffStrength("infectedwound", Limb) <= 60)
                {
                    HF.AddAfflictionLimb(C.Human, "infectedwound", Limb, (float)(-3 * NT.DeltaTime), null);
                }
            };

        // Infected Wound
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Burns, Foreign Bodies, Lacerations, Explosive Damage, Gunshot Wounds.
        // Effects: Inflammation.
        LimbAfflictionsToAdd["infectedwound"] = new("infectedwound", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["infectedwound"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Does not progress in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                // Passive decrease from immunity, bandaged, ointmented
                double InfectIndex = (-C.GetBloodAffData("immunity").PrevStrength / 200
                    - Math.Clamp(C.GetLimbAffStrength("bandaged", Limb), 0, 1) * 1.5
                    - C.GetLimbAffStrength("ointmented", Limb) * 3
                ) * NT.DeltaTime;

                // Dirty bandage :skull:
                if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                {
                    InfectIndex += (C.GetLimbAffStrength("bandageddirty", Limb) / 20) * NT.DeltaTime;
                }

                if (InfectIndex > 0)
                {
                    InfectIndex *= NTConfig.Get("NT_infectionRate", 1) * Math.Clamp(C.GetLimbAffStrength("iced", Limb), 1, 10);
                }

                AffData.Strength[Limb] += InfectIndex;

                // Effects:
                // Inflammation
                if (AffData.Strength[Limb] > 10)
                {
                    C.GetLimbSymptomData("inflammation").Strength[Limb] += .5 * NT.DeltaTime;
                }
            };

        // Foreign Body
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, fractures.
        // Effects: Inflammation, Sepsis.
        LimbAfflictionsToAdd["foreignbody"] = new("foreignbody", 0, 100, 0, AfflictionPriority.HIGH);
        LimbAfflictionsToAdd["foreignbody"].UpdateAction =
            (NTHuman C, string ID, LimbType Limb, NTHumanLimbAffData AffData) =>
            {
                if (!(AffData.Strength[Limb] > 0)) return;

                // Passive Decrease
                if (AffData.Strength[Limb] < 15)
                {
                    AffData.Strength[Limb] -= 0.05 * C.GetDoubleStatStrength("healingrate") * NT.DeltaTime;
                }

                // Arterial Cut chance
                double ForeignBodyAbove20 = AffData.Strength[Limb] >= 20 ? AffData.Strength[Limb] : 0;
                double ForeignBodyCutChance = Math.Pow(ForeignBodyAbove20 / 100, 6) * 0.5;

                if (C.GetLimbAffStrength("bleeding", Limb) > 80 || HF.Chance((float)ForeignBodyCutChance))
                {
                    HF.ArteryCutLimb(C.Human, Limb);
                }

                // Effects:
                // Sepsis
                double GangreneAbove15 = C.GetLimbAffStrength("gangrene", Limb) >= 15 ? C.GetLimbAffStrength("gangrene", Limb) : 0;
                double InfectedAbove50 = C.GetLimbAffStrength("infectedwound", Limb) >= 50 ? C.GetLimbAffStrength("infectedwound", Limb) : 0;

                double SepsisChance = GangreneAbove15 / 400
                    + InfectedAbove50 / 1000
                    + ForeignBodyCutChance;

                if (HF.Chance((float)SepsisChance))
                {
                    C.GetAffData("sepsis").Strength += (NT.DeltaTime * NTConfig.Get("NT_SepsisRate", 1));
                }

                // Inflammation
                if (AffData.Strength[Limb] > 15)
                {
                    C.GetLimbSymptomData("inflammation").Strength[Limb] += .5 * NT.DeltaTime;
                }

                // Infected Wounds
                // Does not progress in Stasis
                if (C.GetBoolStatStrength("stasis")) return;

                double ForeignBodyInfectIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                C.GetLimbAffData("infectedwound").Strength[Limb] += ForeignBodyInfectIndex / 5;

                // Decrease Immunity
                C.GetAffData("immunity").Strength -= Math.Clamp(ForeignBodyInfectIndex / 3, 0, 10);

                if (C.GetLimbAffStrength("bandageddirty", Limb) > 10)
                {
                    double ForeignBodyDirtyIndex = AffData.Strength[Limb] / 40 * NT.DeltaTime;
                    C.GetLimbAffData("infectedwound").Strength[Limb] += ForeignBodyInfectIndex / 5;

                    // Decrease Immunity
                    C.GetAffData("immunity").Strength -= Math.Clamp(ForeignBodyDirtyIndex / 3, 0, 10);
                }
            };

        // Burn
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Fire, items, damage.
        // Effects: Specific Burns, infection.
        AfflictionsToAdd.Add(
            builder.New("burn")
            .SetStrengths(0, 200, 0)
            .IsLimbSpecific(true)
            .SetUpdateAction((C,ID,Limb,dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Decrease
                if (str < 50)
                {
                    C.AddAfflictionLimb(ID, Limb, -((C.GetAfflictionStrength("immunity") / 3000
                        + Math.Clamp(C.GetAfflictionStrengthLimb("bandaged", Limb), 0, 1) * 0.1f
                        + Math.Clamp(C.GetAfflictionStrengthLimb("ointmented", Limb), 0, 1) * 0.12f)
                    * C.GetFloatStat("healingrate") * dT));
                }

                float burn = str;

                // Conversion:
                // First-degree Burns
                C.SetAfflictionLimb("firstdegreeburn", Limb, (float)((burn < 1 || burn > 20) ? 0 : burn * 5));

                // Second-degree Burns
                C.SetAfflictionLimb("seconddegreeburn", Limb, (float)((burn <= 20 || burn > 50) ? 0 : Math.Max(5, (burn - 20) / 30 * 100)));

                // Third-degree Burns
                C.SetAfflictionLimb("thirddegreeburn", Limb, (float)(burn <= 50 ? 0 : Math.Clamp((burn - 50) / 50 * 100, 5, 100)));

                // Effects:
                // Infected Wounds
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float BurnInfectIndex = str / 20 * dT;
                C.AddAfflictionLimb("infectedwound", Limb, BurnInfectIndex / 5);

                // Decrease Immunity
                C.AddAffliction("immunity", -(Math.Clamp(BurnInfectIndex / 3, 0, 10)));

                // Dirty Bandage results
                if (C.GetAfflictionStrengthLimb("bandageddirty", Limb) > 10)
                {
                    float BurnDirtyIndex = str / 40 * dT;
                    C.AddAfflictionLimb("infectedwound", Limb, BurnInfectIndex / 5);

                    // Decrease Immunity
                    C.AddAffliction("immunity", -(Math.Clamp(BurnDirtyIndex / 3, 0, 10)));
                }
            })
            .Build()
            );

        // First-degree Burns
        AfflictionsToAdd.Add(
            builder.New("firstdegreeburn")
            .IsLimbSpecific(true)
            .Build()
            );

        // Second-degree Burns
        AfflictionsToAdd.Add(
            builder.New("seconddegreeburn")
            .IsLimbSpecific(true)
            .Build()
            );

        // Third-degree Burns
        AfflictionsToAdd.Add(
            builder.New("thirddegreeburn")
            .IsLimbSpecific(true)
            .Build()
            );

        // Lacerations
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, failed skill checks.
        // Effects: Damage, infection.
        AfflictionsToAdd.Add(
            builder.New("lacerations")
            .SetStrengths(0, 200, 0)
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Regeneration
                if (str < 50)
                {
                    C.AddAfflictionLimb(ID, Limb, -((
                            C.GetAfflictionStrength("immunity") / 3000
                            + Math.Clamp(C.GetAfflictionStrengthLimb("bandaged", Limb), 0, 1) * .1f
                            + Math.Clamp(C.GetAfflictionStrengthLimb("ointmented", Limb), 0, 1) * .12f
                            )
                            * C.GetFloatStat("healingrate")
                            * dT));
                }

                // Effects:
                // Infected Wounds
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float LacerationInfectIndex = str / 40 * dT;

                C.AddAfflictionLimb("infectedwound", Limb, LacerationInfectIndex / 5);

                // Decrease Immunity
                C.AddAffliction("immunity", -(Math.Clamp(LacerationInfectIndex / 3, 0, 10)));

                if (C.GetAfflictionStrengthLimb("bandageddirty", Limb) > 10)
                {
                    float LacerationDirtyIndex = str / 40 * dT;
                    C.AddAfflictionLimb("infectedwound", Limb, LacerationInfectIndex / 5);

                    // Decrease Immunity
                    C.AddAffliction("immunity", -(Math.Clamp(LacerationDirtyIndex / 3, 0, 10)));
                }
            })
            .Build());

        // Gunshot Wound
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Getting shot.
        // Effects: Damage, infection.
        AfflictionsToAdd.Add(
            builder.New("gunshotwound")
            .SetStrengths(0, 200, 0)
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Regeneration
                if (str < 50)
                {
                    C.AddAfflictionLimb(ID, Limb, -((
                        C.GetAfflictionStrength("immunity") / 3000
                        + Math.Clamp(C.GetAfflictionStrengthLimb("bandaged", Limb), 0, 1) * 0.1f
                        + Math.Clamp(C.GetAfflictionStrengthLimb("ointmented", Limb), 0, 1) * 0.12f
                    ) * C.GetFloatStat("healingrate") * dT));
                }

                // Effects:
                // Infected Wounds
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float GSWInfectIndex = str / 40 * dT;
                C.AddAfflictionLimb("infectedwound", Limb, GSWInfectIndex / 5);

                // Decrease Immunity
                C.AddAffliction("immunity", -(Math.Clamp(GSWInfectIndex / 3, 0, 10)));

                if (C.GetAfflictionStrengthLimb("bandageddirty", Limb) > 10)
                {
                    float GSWDirtyIndex = str / 40 * dT;
                    C.AddAfflictionLimb("infectedwound", Limb, GSWInfectIndex / 5);

                    // Decrease Immunity
                    C.AddAffliction("immunity", -(Math.Clamp(GSWDirtyIndex / 3, 0, 10)));
                }
            })
            .Build());

        // Explosion Damage
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Being near explosions.
        // Effects: Damage, infection.
        AfflictionsToAdd.Add(
            builder.New("explosiondamage")
            .SetStrengths(0, 200, 0)
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {

                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Regeneration
                if (str < 50)
                {
                    C.AddAfflictionLimb(ID, Limb, -((
                        C.GetAfflictionStrength("immunity") / 3000
                        + Math.Clamp(C.GetAfflictionStrengthLimb("bandaged", Limb), 0, 1) * 0.1f
                        + Math.Clamp(C.GetAfflictionStrengthLimb("ointmented", Limb), 0, 1) * 0.12f
                    ) * C.GetAfflictionStrength("healingrate") * dT));
                    
                }

                // Effects:
                // Infected Wounds
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float ExplosionDamageInfectIndex = str / 40 * dT;
                C.AddAfflictionLimb("infectedwound", Limb, ExplosionDamageInfectIndex / 5);

                // Decrease Immunity
                C.AddAffliction("immunity", -(Math.Clamp(ExplosionDamageInfectIndex / 3, 0, 10)));

                if (C.GetAfflictionStrengthLimb("bandageddirty", Limb) > 10)
                {
                    float ExplosionDamageDirtyIndex = str / 40 * dT;
                    C.AddAfflictionLimb("infectedwound", Limb, ExplosionDamageInfectIndex / 5);

                    // Decrease Immunity
                    C.AddAffliction("immunity", -(Math.Clamp(ExplosionDamageDirtyIndex / 3f, 0f, 10f)));

                }
            })
            .Build());

        // Bite Wounds
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Being chomped.
        // Effects: Damage, infection.
        AfflictionsToAdd.Add(
            builder.New("bitewounds")
            .SetStrengths(0, 200, 0)
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Regenerations
                if (str < 100)
                {
                    C.AddAfflictionLimb(ID, Limb, -((C.GetAfflictionStrength("immunity") / 3000
                        + Math.Clamp(C.GetAfflictionStrengthLimb("bandaged", Limb), 0, 1) * 0.1f
                        + Math.Clamp(C.GetAfflictionStrengthLimb("ointmented", Limb), 0, 1) * 0.12f
                    ) * C.GetFloatStat("healingrate") * dT));
                        
                }

                // Effects:
                // Infected Wounds
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float BitesInfectIndex = str / 30 * dT;

                C.AddAfflictionLimb("infectedwound", Limb, BitesInfectIndex / 5);

                // Decrease Immunity
                C.AddAffliction("immunity", -(Math.Clamp(BitesInfectIndex / 3, 0, 10)));

                if (C.GetAfflictionStrengthLimb("bandageddirty", Limb) > 10)
                {
                    float BitesDirtyIndex = str / 40 * dT;
                    C.AddAfflictionLimb("infectedwounds", Limb, BitesInfectIndex / 5);

                    // Decrease Immunity
                    C.AddAffliction("immunity", -(Math.Clamp(BitesDirtyIndex / 3, 0, 10)));
                }
            })
            .Build()
            );

        // Blunt Force Trauma
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Fall damage or weapons.
        // Effects: Damage.
        AfflictionsToAdd.Add(
            builder.New("blunttrauma")
            .SetStrengths(0, 200, 0)
            .IsLimbSpecific(true)
            .SetUpdateAction((C,ID,Limb,dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (str == 0) return;

                // Passive Regeneration
                if (str < 100)
                {


                    float IsIced = Math.Clamp(C.GetAfflictionStrengthLimb("iced", Limb), 0, 1);
                    float IsBandaged = Math.Clamp(C.GetAfflictionStrengthLimb("bandaged", Limb), 0, 1);
                    float IsOintmented = Math.Clamp(C.GetAfflictionStrengthLimb("ointmented", Limb), 0, 1);

                    float BFTHealRate = (
                        C.GetAfflictionStrength("immunity") / 8000
                        + IsIced * 0.3f
                        + IsBandaged * 0.1f
                        + IsOintmented * 0.12f
                    ) * C.GetFloatStat("healingrate") * dT;

                    C.AddAfflictionLimb(ID, Limb, -BFTHealRate);


                }
            })
            .Build()
            );

        // Internal Damage
        // Not Constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Dislocations, Fractures, Neck Fractures, sustaining damage.
        // Effects: Damage.
        AfflictionsToAdd.Add(
            builder.New("internaldamage")
            .SetStrengths(0, 200, 0)
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (str == 0) return;

                if (str < 50)
                {
                    C.AddAfflictionLimb(ID, Limb, -0.05f * C.GetFloatStat("healingrate") * dT);
                }
            })
            .Build()
            );


        // Blood afflictions are literally the same to write as NonLimbAfflictions, they're just here for organization purposes.

        // Blood Loss
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Bleeding, Damage.
        // Effects: Changes Blood Pressure.
        AfflictionsToAdd.Add(
            builder.New("bloodloss")
            .SetStrengths(0, 200, 0)
            .Build());

        // Blood Pressure
        // Constant; too complicated otherwise.
        // Type: Vital Mechanic
        // Handles the entire blood pressure system and application of effects.
        AfflictionsToAdd.Add(
            builder.New("bloodpressure")
            .IsConst(true)
            .SetStrengths(0, 200, 100)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float str = C.GetAfflictionStrength(ID);

                double desiredBloodPressure = (
                    C.GetFloatStat("bloodamount")
                    - C.GetAfflictionStrength("tamponade") / 2
                    - Math.Clamp(C.GetAfflictionStrength("afpressuredrug") * 5, 0, 45)
                    - Math.Clamp(C.GetAfflictionStrength("anesthesia"), 0, 15)
                    + Math.Clamp(C.GetAfflictionStrength("afadrenaline") * 10, 0, 30)
                    + Math.Clamp(C.GetAfflictionStrength("afsaline") * 5, 0, 30)
                    + Math.Clamp(C.GetAfflictionStrength("afringerssolution") * 5, 0, 30)
                )
                    * (1 + 0.5 * Math.Pow(C.GetAfflictionStrength("liverdamage") / 100, 2))
                    * (1 + 0.5 * Math.Pow(C.GetAfflictionStrength("kidneydamage") / 100, 2))
                    * (1 + C.GetAfflictionStrength("alcoholwithdrawal") / 200)
                    * Math.Clamp((100 - C.GetAfflictionStrength("traumaticshock") * 2) / 100, 0, 1)
                    * ((100 - C.GetAfflictionStrength("fibrillation")) / 100)
                    * (1 - Math.Min(1, C.GetAfflictionStrength("cardiacarrest")))
                    * NTC.GetMultiplier(C, "bloodpressure");

                float bloodPressureLerp = 0.2f * NTC.GetMultiplier(C, "bloodpressurerate");

                if (desiredBloodPressure > str)
                {
                    bloodPressureLerp /= 3;
                }

                // Move to desired amount
                C.SetAffliction(ID, (float)Math.Clamp(Double.Lerp(str, desiredBloodPressure, bloodPressureLerp), 5, 200));

                // Effects:
                // Confusion
                if (str < 60)
                {
                    if (C.GetAfflictionStrength("unconsciousness") <= 0)
                    {
                        C.SetSymptomTrue("lightheadedness", 2);
                        C.SetSymptomTrue("headache", 2);
                    }

                    // Blurred Vision
                    if (str < 55)
                    {
                        if (C.GetAfflictionStrength("unconsciousness") <= 0)
                        {
                            C.SetSymptomTrue("blurredvision", 2);
                        }

                        // Pale Skin
                        if (str < 50)
                        {
                            C.SetSymptomTrue("paleskin", 2);

                            // Confusion
                            if (str < 30)
                            {
                                if (C.GetAfflictionStrength("unconsciousness") <= 0)
                                {
                                    C.SetSymptomTrue("confusion", 2);
                                }
                            }
                        }
                    }
                }

                // Heart Attack + Stroke
                if (str > 150)
                {
                    if (C.GetAfflictionStrength("afstreptokinase") <= 0 && C.GetAfflictionStrength("heartremoved") <= 0 && HF.Chance((float)(NTConfig.Get("NT_heartattackChance", 1f) * ((str - 150) / 50 * 0.02f))))
                    {
                        C.AddAffliction("heartattack", 50);
                    }

                    if (HF.Chance((float)(NTConfig.Get("NT_strokeChance", 1) * ((str - 150) / 50 * 0.02f + Math.Clamp(C.GetAfflictionStrength("afstreptokinase"), 0, 1) * 0.05f))))
                    {
                        C.AddAffliction("stroke", 5);
                    }
                }
            })
            .Build());

        // Hypoxemia
        // Constant, too complicated otherwise
        // Type: Blood Affliction
        // Caused By: Oxygen Low, Blood Loss.
        // Effects: Changes Blood Pressure, Specific Organ Damage (requires them to be Constant).
        AfflictionsToAdd.Add(
            builder.New("hypoxemia")
            .IsConst(true)
            .SetUpdateAction((C, ID, Limb, dT) => 
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                C.SetFloatStat("availableoxygen", Math.Min(C.GetFloatStat("availableoxygen"), 100f - C.GetAfflictionStrength("pneumothorax") / 2f));

                float HypoxemiaGain = NTC.GetMultiplier(C, "hypoxemiagain");
                float RegularHypoxemiaChange = (-C.GetFloatStat("availableoxygen") + 50f) / 8f;

                if (RegularHypoxemiaChange > 0)
                {
                    RegularHypoxemiaChange *= HypoxemiaGain;
                }
                else
                {
                    RegularHypoxemiaChange = (float) Double.Lerp(RegularHypoxemiaChange * 2, 0, Math.Clamp((50 - C.GetFloatStat("bloodamount")) / 50, 0, 1));
                }

                float str = C.GetAfflictionStrength(ID);

                // Passively Increase / Decrease
                C.SetAffliction(ID, Math.Clamp(
                    str + (
                        -Math.Min(0, (C.GetAfflictionStrength("bloodpressure") - 70) / 7) * HypoxemiaGain
                        - Math.Min(0, (C.GetFloatStat("bloodamount") - 60) / 4) * HypoxemiaGain
                        + RegularHypoxemiaChange
                    ) * dT,
                    0, 100
                ));

                // Effects:
                // Neurotrauma
                float NeurotraumaGain = str / 100 * dT
                    * NTC.GetMultiplier(C, "neurotraumagain")
                    * NTConfig.Get("NT_neurotraumaGain", 1)
                    * (1 - Math.Clamp(C.GetAfflictionStrength("afmannitol"), 0, 0.5f));

                C.AddAffliction("neurotrauma", NeurotraumaGain);

                // Bone Damage
                C.AddAffliction("bonedamage", str / 1000 * NTC.GetMultiplier(C, "bonedamagegain") * dT);

                // Shortness of Breath
                if (str > 20)
                {
                    if (C.GetAfflictionStrength("respiratoryarrest") <= 0)
                    {
                        C.SetSymptomTrue("shortnessofbreath", 2);
                    }

                    // Headache
                    if (str > 40)
                    {
                        if (C.GetAfflictionStrength("unconsciousness") <= 0)
                        {
                            C.SetSymptomTrue("headache", 2);
                        }

                        // Confusion
                        if (str > 50)
                        {
                            if (C.GetAfflictionStrength("unconsciousness") <= 0)
                            {
                                C.SetSymptomTrue("confusion", 2);
                            }

                            // Respiratory Arrest
                            if (str > 70 && HF.Chance(0.05f))
                            {
                                C.AddAffliction("respiratoryarrest", 200);
                            }

                            // Unconsciousness & Cardiac Arrest
                            if (str > 80)
                            {
                                C.SetSymptomTrue("unconsciousness", 2);

                                if (HF.Chance(0.01f))
                                {
                                    C.AddAffliction("cardiacarrest", 200);
                                }
                            }
                        }
                    }
                }
            })
            .Build());

        // Alkalosis
        // Not constant; gets applied by other sources.
        // Type: Blood
        // Caused By: Hyperventilation, Vomiting, Blood Pack items.
        // Effects: Seizures, Palpitations.
        AfflictionsToAdd.Add(
            builder.New("alkalosis")
            .SetUpdateAction((C,ID,Limb,dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float str = C.GetAfflictionStrength(ID);

                // Passive Increase / Decrease
                C.AddAffliction(ID, -dT * 0.03f);

                // Alkalosis Interaction
                if (C.GetAfflictionStrength("acidosis") > 1f && str > 1f)
                {
                    float min = Math.Min(C.GetAfflictionStrength("acidosis"), str);
                    C.AddAffliction("acidosis", -min);
                    C.AddAffliction(ID, -min);
                }

                // Effects:
                // Palpitations
                if (str > 20f)
                {
                    if (C.GetAfflictionStrength("cardiacarrest") <= 0f)
                    {
                        C.SetSymptomTrue("palpitations", 2);
                    }

                    // Seizures
                    if (str > 60f && HF.Chance(0.05f))
                    {
                        C.AddAffliction("seizure", 10f);
                    }
                }
            })
            .Build()
            );


        // Acidosis
        // Not constant; gets applied by other sources.
        // Type: Blood
        // Caused By: Hypoventilation, Respiratory Arrest, Cardiac Arrest, Kidney Damage, Saline, Blood Pack items.
        // Effects: Seizures, Coma, Increased Heartrate, Fibrillation, Headache, Confusion, Weakness.
        AfflictionsToAdd.Add(
            builder.New("acidosis")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                C.AddAffliction(ID, HF.BoolToNum(
                        C.GetAfflictionStrength("hypoventilation") > 0f
                        && C.GetAfflictionStrength("artificialventilation") <= 0.1f
                    ) * 0.09f * dT
                    + Math.Max(0f, C.GetAfflictionStrength("kidneydamage") - 80f) / 2f * 0.1f * dT
                    - dT * 0.03f);

                // Effects:

                // Fibrillation (in IncreasedHeartrate constant)
                // Increased Heartrate (in IncreasedHeartrate constant)

                float str = C.GetAfflictionStrength(ID);

                // Confusion
                if (str > 15)
                {
                    bool IsConscious = C.GetAfflictionStrength("unconsciousness") <= 0f;

                    if (IsConscious)
                    {
                        C.SetSymptomTrue("confusion", 2);

                        // Headache
                        if (str > 20)
                        {
                            C.SetSymptomTrue("headache", 2);
                        }
                    }

                    // Weakness
                    if (str > 35)
                    {
                        C.SetSymptomTrue("weakness", 2);

                        if (str > 60)
                        {
                            // Coma
                            if (HF.Chance(0.05f + (float)(str - 60f) / 100f))
                            {
                                C.AddAffliction("coma", 14);
                            }

                            // Seizures
                            if (HF.Chance(0.05f))
                            {
                                C.AddAffliction("seizure", 10);
                            }
                        }
                    }
                }
            })
            .Build()
            );

        // Hemotransfusion Shock
        // Not constant; gets applied by other sources.
        // Type: Blood
        // Caused By: Wrong Blood Type, Bozo.
        // Effects: Vomiting, Chest Pain, Blood Loss (XML), Vanilla Organ Damage (XML), Liver Damage (XML), Heart Damage (XML), Kidney Damage (XML), Lung Damage (XML), Shortness of Breath, Abdominal Pain, Wheezing.
        AfflictionsToAdd.Add(
            builder.New("hemotransfusionshock")
            .SetUpdateAction((C,ID,Limb,dT) =>
            {
                float str = C.GetAfflictionStrength(ID);

                // Effects:
                // Wheezing
                if (str < 90)
                {
                    bool IsConscious = C.GetAfflictionStrength("unconsciousness") <= 0;
                    bool IsSedated = C.GetBoolStat("sedated");

                    if (C.GetAfflictionStrength("respiratoryarrest") <= 0)
                    {
                        C.SetSymptomTrue("wheezing", 2);
                    }

                    // Abdominal Pain
                    if (str < 80)
                    {
                        if (IsConscious && !IsSedated)
                        {
                            C.SetSymptomTrue("abdominalpain", 2);
                        }

                        // Shortness of Breath
                        if (str < 70)
                        {
                            if (C.GetAfflictionStrength("respiratoryarrest") <= 0)
                            {
                                C.SetSymptomTrue("shortnessofbreath", 2);
                            }

                            // Chest Pain
                            if (str < 60)
                            {
                                if (IsConscious && !IsSedated)
                                {
                                    C.SetSymptomTrue("chestpain", 2);
                                }

                                // Vomiting
                                if (str < 40)
                                {
                                    C.SetSymptomTrue("vomiting", 2);
                                }
                            }
                        }
                    }
                }
            })
            .Build()
            );

        // Sepsis
        // Not constant; gets applied by other sources.
        // Type: Blood
        // Caused By: Blood Pack Items, Gangrene, Foreign Bodies, Infected Wounds, Azathioprine failed skillcheck.
        // Effects: Organ Damage (via Stats), Gangrene, Fever, Hyperventilation, Increased Heartrate (Constant), Neurotrauma, Confusion, Bone Damage.
        AfflictionsToAdd.Add(
            builder.New("sepsis")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float str = C.GetAfflictionStrength(ID);

                // Passive Increase
                if (str > 0.1f)
                {
                    C.AddAffliction(ID, 0.05f * dT);
                }

                // Effects:
                // Neurotrauma
                float NeurotraumaGain = str / 100f * 0.4f * dT
                    * (float) NTC.GetMultiplier(C, "neurotraumagain")
                    * NTConfig.Get("NT_neurotraumaGain", 1)
                    * (1f - Math.Clamp(C.GetAfflictionStrength("afmannitol"), 0f, 0.5f));

                C.AddAffliction("neurotrauma", NeurotraumaGain);

                // Bone Damage
                C.AddAffliction("bonedamage", str / 500f * (float) NTC.GetMultiplier(C, "bonedamagegain") * dT);

                // Fever
                if (str > 5)
                {
                    C.SetSymptomTrue("fever", 2);

                    // Gangrene
                    if (HF.Chance(0.04f))
                    {
                        foreach (LimbType AllLimbs in HF.LimbsToCheck)
                        {
                            if (HF.LimbIsExtremity(Limb))
                            {
                                C.AddAfflictionLimb("gangrene", Limb, (0.5f + str / 150f) * NTConfig.Get("NT_gangrenespeed", 1f) * dT);
                            }
                        }
                    }

                    // Confusion
                    if (str > 40 && C.GetAfflictionStrength("unconsciousness") <= 0)
                    {
                        C.SetSymptomTrue("confusion", 2);
                    }
                }
            })
            .Build()
            );

        // Immunity
        // Constant; else too complicated.
        // Type: Blood, Mechanic
        // Caused By: Existing.
        // Effects: Increased regeneration for Burns and Wounds.
        AfflictionsToAdd.Add(
            builder.New("immunity")
            .SetStrengths(0,100,100)
            .IsConst(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                if (C.GetAfflictionStrength(ID, -1) == -1)
                {
                    if (NTBloodTypes.HasBloodType(C.Human))
                    {
                        C.SetAffliction(ID, 100f);
                    }
                    else
                    {
                        C.SetAffliction(ID, 100f);
                        NTBloodTypes.TryRandomizeBlood(C.Human);
                    }
                }

                if (C.GetBoolStat("stasis")) return;

                C.SetAffliction(ID, (float) Math.Clamp(C.GetAfflictionStrength(ID) + (0.5 + C.GetAfflictionStrength(ID, -1) / 100) * dT, 5f, 100f));

                
            })
            .Build()
            );

        // Cough
        // Type: Symptom, Mental
        // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
        AfflictionsToAdd.Add(
            builder.New("cough")
            .IsSymptom(true)
            .Build()
            );

        // Pale Skin
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("paleskin")
            .IsSymptom(true)
            .Build()
            );

        // Lightheadedness
        // Type: Symptom, Mental
        // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
        AfflictionsToAdd.Add(
            builder.New("lightheadedness")
            .IsSymptom(true)
            .Build()
            );

        // Blurred Vision
        // Type: Symptom, Mental
        // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
        AfflictionsToAdd.Add(
            builder.New("blurredvision")
            .IsSymptom(true)
            .Build()
            );

        // Confusion
        // Type: Symptom, Mental
        // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
        AfflictionsToAdd.Add(
            builder.New("confusion")
            .IsSymptom(true)
            .Build()
            );

        // Headache
        // Type: Symptom, Mental, Pain
        // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious. Removed if under Painkillers.
        AfflictionsToAdd.Add(
            builder.New("headache")
            .IsSymptom(true)
            .Build()
            );

        // Leg Swelling
        // Type: Symptom, Organic
        // Removes itself when conditions are NOT met. Applied by other afflictions. Not present on Cybernetics.
        AfflictionsToAdd.Add(
            builder.New("legswelling")
            .IsSymptom(true)
            .Build()
            );

        // Weakness
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("weakness")
            .IsSymptom(true)
            .Build()
            );

        // Wheezing
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("wheezing")
            .IsSymptom(true)
            .Build()
            );

        // Vomiting
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        // Effects: Alkalosis
        AfflictionsToAdd.Add(
            builder.New("vomiting")
            .IsSymptom(true)
            .Build()
            );

        // Vomiting Blood
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("vomitingblood")
            .IsSymptom(true)
            .Build()
            );

        // Fever
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("fever")
            .IsSymptom(true)
            .Build()
            );

        // Abdominal Discomfort
        // Type: Symptom, Mental
        // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
        AfflictionsToAdd.Add(
            builder.New("abdominaldiscomfort")
            .IsSymptom(true)
            .Build()
            );

        // Bloating
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("bloating")
            .IsSymptom(true)
            .Build()
            );

        // Jaundice
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("jaundice")
            .IsSymptom(true)
            .Build()
            );

        // Sweating
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("sweating")
            .IsSymptom(true)
            .Build()
            );

        // Palpitations
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("palpitations")
            .IsSymptom(true)
            .Build()
            );

        // Unconsciousness
        // Type: Symptom, Talent Interaction
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("unconsciousness")
            .IsSymptom(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                if (C.GetAfflictionStrength(ID) > 0) C.AddAffliction("givein", 2);
            })
            .Build()
            );


        // Craving
        // Type: Symptom, Mental
        // Removes itself when conditions are NOT met. Applied by other afflictions. Removed when Unconscious.
        AfflictionsToAdd.Add(
            builder.New("craving")
            .IsSymptom(true)
            .Build()
            );

        // Nausea
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        // Effects: Alkalosis
        AfflictionsToAdd.Add(
            builder.New("nausea")
            .IsSymptom(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                var str = C.GetAfflictionStrength(ID);

                if (str > 0)
                {
                    C.SetAffliction("alkalosis", Math.Clamp(str, 0, 1) * 0.1f * dT);
                }
            })
            .Build()
            );

        // Chest Pain
        // Type: Symptom, Mental
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("chestpain")
            .IsSymptom(true)
            .Build()
            );

        // Abdominal Pain
        // Type: Symptom, Mental, Pain
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("abdominalpain")
            .IsSymptom(true)
            .Build()
            );

        // Intense Pain
        // Type: Symptom, Mental, Pain
        AfflictionsToAdd.Add(
            builder.New("intensepain")
            .IsSymptom(true)
            .Build()
            );

        // Shortness of Breath
        // Type: Symptom
        // Removes itself when conditions are NOT met. Applied by other afflictions.
        AfflictionsToAdd.Add(
            builder.New("shortnessofbreath")
            .IsSymptom(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                if (C.GetAfflictionStrength("respiratoryarrest") > 0)
                {
                    C.SetAffliction(ID, 0f);
                    C.SetSymptomFalse(ID);
                    return;
                }
            })
            .Build()
            );

        // On Wheelchair
        // Constant.
        // Type: Functionality
        // Effects: Changes the animations of a character to one in a wheelchair.
        // Applied via Stats.
        AfflictionsToAdd.Add(
            builder.New("onwheelchair")
            .IsSymptom(true)
            .IsConst(true)
            .SetStrengths(0,2,0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Removal Conditions
                bool keep = C.GetAfflictionStrength("unconsciousness") <= 0
                        && (C.HasSymptom("onwheelchair") || HF.GetOuterWearIdentifier(C.Human) == "nt_wheelchair");

                C.SetAffliction(ID, HF.BoolToNum(keep, 2f));
                if (!keep) C.SetSymptomFalse(ID);

                if (keep)
                {
                    float SpeedMult = .8f;
                    List<LimbType> Arms = [LimbType.LeftArm, LimbType.RightArm];

                    foreach (LimbType Arm in Arms)
                    {
                        if (HF.LimbIsBroken(C.Human, Arm, true) || HF.LimbIsDislocated(C.Human, Limb, true) || HF.LimbIsAmputated(C.Human, Limb)) SpeedMult -= .2f;
                    }

                    C.SetFloatStat("speedmultiplier", C.GetFloatStat("speedmultiplier") * SpeedMult); // slow the character down.

                }
            })
            .Build()
            );

        // Force Prone
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Changes the animations of a character to be unable to walk.
        AfflictionsToAdd.Add(
            builder.New("forceprone")
            .IsConst(true)
            .IsSymptom(true)
            .SetStrengths(0,2,0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {

                // Readability / 10 ?
                bool keep = C.GetAfflictionStrength("unconsciousness") <= 0
                        && (!C.Human.IsClimbing)
                        && (C.HasSymptom("forceprone")
                            || (C.GetBoolStat("lockleftleg")
                            && C.GetBoolStat("lockrightleg")
                            && (!C.GetBoolStat("wheelchaired")))
                        );

                C.SetAffliction(ID,HF.BoolToNum(keep, 2f));

                if (!keep) C.SetSymptomFalse(ID);

                

            })
            .Build()
            );

        // Hyperventilation
        // Not constant; gets applied by other sources, removes itself however.
        // Type: Non-Limb Specific
        // Caused By: Hypotension, Hypoxemia, Pneumothorax, Sepsis, Adrenaline
        // Effects: Alkalosis.
        AfflictionsToAdd.Add(
            builder.New("hyperventilation")
            .IsSymptom(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {

                if(C.GetAfflictionStrength("respiratoryarrest") > 0)
                {
                    C.SetAffliction(ID, 0f);
                    C.SetSymptomFalse(ID);
                    return;
                }

                // Effects:
                // Alkalosis
                C.AddAffliction("alkalosis", Math.Clamp(C.GetAfflictionStrength(ID), 0f, 1f) * 0.09f * dT);
            })
            .Build()
            );

        // Hypoventilation
        // Not constant; gets applied by other sources, removes itself however.
        // Type: Non-Limb Specific
        // Caused By: Opiate Overdose, Opiods, Anesthesia
        // Effects: Acidosis.
        AfflictionsToAdd.Add(
            builder.New("hypoventilation")
            .IsSymptom(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                if (C.GetAfflictionStrength("respiratoryarrest") > 0)
                {
                    C.SetAffliction(ID, 0f);
                    C.SetSymptomFalse(ID);
                    return;
                }
                // Counteracting with Hyperventilation
                if (C.GetAfflictionStrength("hyperventilation") > 0 && C.GetAfflictionStrength(ID) > 0)
                {
                    C.SetAffliction(ID, 0f);
                    C.SetSymptomFalse(ID);
                    C.SetAffliction("hyperventilation", 0f);
                    C.SetSymptomFalse("hyperventilation");
                }

                // Effects:
                // Acidosis
                if (C.GetAfflictionStrength(ID) > 0 && C.GetAfflictionStrength("artificialventilation") <= 0.1)
                {
                    C.AddAffliction("acidosis", 0.09f * dT);
                }
            })
            .Build());

        //TODO: make non real afflictions a thing

        //SymptomsToAdd["lockleftarm"] = new("lockleftarm", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["lockleftarm"].Real = false;
        //SymptomsToAdd["lockrightarm"] = new("lockrightarm", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["lockrightarm"].Real = false;
        //SymptomsToAdd["lockleftleg"] = new("lockleftleg", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["lockleftleg"].Real = false;
        //SymptomsToAdd["lockrightleg"] = new("lockrightleg", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["lockrightleg"].Real = false;

        //SymptomsToAdd["triggersym_respiratoryarrest"] = new("triggersym_respiratoryarrest", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_respiratoryarrest"].Real = false;
        //SymptomsToAdd["triggersym_respiratoryarrest"].Const = true;
        //SymptomsToAdd["triggersym_respiratoryarrest"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAffData("respiratoryarrest").Strength = 100;
        //    };
        //SymptomsToAdd["triggersym_seizure"] = new("triggersym_seizure", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_seizure"].Real = false;
        //SymptomsToAdd["triggersym_seizure"].Const = true;
        //SymptomsToAdd["triggersym_seizure"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAffData("seizure").Strength = 100;
        //    };
        //SymptomsToAdd["triggersym_stroke"] = new("triggersym_stroke", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_stroke"].Real = false;
        //SymptomsToAdd["triggersym_stroke"].Const = true;
        //SymptomsToAdd["triggersym_stroke"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAffData("stroke").Strength = 100;
        //    };
        //SymptomsToAdd["triggersym_coma"] = new("triggersym_coma", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_coma"].Real = false;
        //SymptomsToAdd["triggersym_coma"].Const = true;
        //SymptomsToAdd["triggersym_coma"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAffData("seizure").Strength = 100;
        //    };
        //SymptomsToAdd["triggersym_cardiacarrest"] = new("triggersym_cardiacarrest", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_cardiacarrest"].Real = false;
        //SymptomsToAdd["triggersym_cardiacarrest"].Const = true;
        //SymptomsToAdd["triggersym_cardiacarrest"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAffData("cardiacarrest").Strength = 100;
        //    };



        // Inflammation
        // Type: Limb-specific
        // Caused by: Foreign Bodies, Infected Wounds
        // Effects: Fever
        AfflictionsToAdd.Add(
            builder.New("inflammation")
            .IsLimbSpecific(true)
            .IsSymptom(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (str > 0)
                {
                    C.SetAfflictionLimb(ID, Limb, str - (0.1f * dT));
                }
            })
            .Build()
            );

        // Spasms
        // Type: Symptom
        // Caused By: Seizure
        // Effects: Makes character twitch on the ground via XML.
        AfflictionsToAdd.Add(
            builder.New("spasm")
            .IsSymptom(true)
            .Build());

        NeurotraumaInit.NTAfflLoader.Registers(AfflictionsToAdd);
    }
}