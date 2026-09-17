using static Barotrauma.Networking.MessageFragment;
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
        AfflictionsToAdd.Add(
            builder.New("oxygenlow")
            .SetStrengths(0, 200, 0)
            .SetUpdateAction((NTHuman C, string ID, LimbType Limb, float DeltaTime) =>
            {
                if (C.GetAfflictionStrength("respiratoryarrest") > 0)
                {
                    C.AddAffliction(ID, 15f * DeltaTime);
                }
            })
            .Build()
            );

        // Drunk
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Vanilla Override
        // Caused By: ROOOTT BEEERRRRRR.
        // Effects: idk.

        AfflictionsToAdd.Add(
            builder.New("drunk")
            .SetStrengths(0, 200, 0)
            .Build()
            );

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
            .SetUpdateAction((NTHuman C, string ID, LimbType Limb, float dT) =>
            {
                // Removal Conditions
                if ((!C.GetBoolStat("stasis")) // Not in Stasis
                        && C.GetAfflictionStrength("lungremoved")<= 0 // No Lungs Removed
                        && C.GetAfflictionStrength("brainremoved")<= 0 // No Brain Removed
                        && C.GetAfflictionStrength("opiateoverdose")<= 60 // Below Opiate Overdose Threshold
                        && C.GetAfflictionStrength("lungdamage")<= 99 // Below Lung Damage Threshold
                        && C.GetAfflictionStrength("traumaticshock")<= 30 // Below Traumatic Shock Threshold
                        && C.GetAfflictionStrength("neurotrauma")<= 100 // Below Neurotrauma Threshold
                        && C.GetAfflictionStrength("hypoxemia")<= 70 // Below Hypoxemia Threshold
                        )
                {
                    // Passive Regeneration
                    C.AddAffliction(ID, (5f + HF.BoolToNum(C.GetAfflictionStrength("unconsciousness") < 0.1f, 45f)) * -dT);
                }

                // Effects:
                // Acidosis
                // Shares increase with Cardiac Arrest
                float AcidosisIncrease = HF.BoolToNum(C.GetAfflictionStrength("cardiacarrest")<= 0
                        && C.GetAfflictionStrength("respiratoryarrest")> 0
                        && C.GetAfflictionStrength("artificialventilation")<= 0.1f)
                    * 0.18f * dT;

                C.AddAffliction("acidosis", AcidosisIncrease);

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
        AfflictionsToAdd.Add(
            builder.New("fracturedribs")
            .SetPriority(AfflictionPriority.MEDIUM)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                if (C.GetAfflictionStrength(ID) > 0)
                {
                    C.AddAffliction(ID, 4 * dT);
                }

                // Effects:
                // Chest Pain
                if (C.GetAfflictionStrength(ID) > 0 && C.GetAfflictionStrength("unconsciousness") <= 0 && (!C.GetBoolStat("sedated")))
                {
                    C.SetSymptomTrue("chestpain", 3);
                }
            })
            .Build()
            );

        // Neck Fracture
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Internal Wounds (DMG), Open Wounds (DMG), Bone Death.
        // Effects: Spinal Cord Injury if not Bandaged (XML), Internal Damage if not Bandaged (XML).
        AfflictionsToAdd.Add(
            builder.New("fracturedneck")
            .SetPriority(AfflictionPriority.MEDIUM)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                if (C.GetAfflictionStrength(ID) > 0)
                {
                    C.AddAffliction(ID, 4 * dT);
                }
            })
            .Build()
            );

        // Skull Fracture
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Internal Wounds (DMG), Open Wounds (DMG), Bone Death.
        // Effects: Headache, prevents Neurotrauma Regeneration.
        AfflictionsToAdd.Add(
            builder.New("fracturedskull")
            .SetPriority(AfflictionPriority.MEDIUM)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                if (C.GetAfflictionStrength(ID) > 0)
                {
                    // Passive Increase
                    C.AddAffliction(ID, 4f * dT);
                }

                // Effects:
                // Headache
                if (C.GetAfflictionStrength(ID) > 0 && C.GetAfflictionStrength("unconsciousness") <= 0)
                {
                    C.SetSymptomTrue("headache", 3);
                }

                // Neurotrauma Regeneration (in Neurotrauma itself)
            })
            .Build()
            );

        // =============== Drugs =============== //

        // Opiate Overdose
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Drug, Vanilla Override
        // Caused By: Application of Opiates.
        // Effects: Respiratory Arrest, Unconsciousness, Seizures, Death.
        AfflictionsToAdd.Add(
            builder.New("opiateoverdose")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrength(ID);

                // Effects:
                if (str > 60)
                {
                    // Respiratory Arrest
                    C.AddAffliction("respiratoryarrest", 200);

                    // Unconsciousness
                    C.SetSymptomTrue("unconsciousness", 2);

                    // Seizures
                    if (HF.Chance(str / 500f))
                    {
                        C.AddAffliction("seizure", 10);
                    }
                }
            })
            .Build()
            );

        // =============== Organs =============== //

        // Lung Damage
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Organ Damage
        // Caused By: ABX, LiOxy, Ambubag, HemoTransShock, RadSickness, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG).
        // Effects: Cough, Shortness of Breath, Respiratory Arrest
        AfflictionsToAdd.Add(
            builder.New("lungdamage")
            .IsConst(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress while in Stasis
                if (C.GetBoolStat("stasis")) return;

                float LungDamage = (float) HF.OrganDamageCalc(C, C.GetAfflictionStrength(ID) + NTC.GetMultiplier(C, "lungdamagegain") * C.GetFloatStat("neworgandamage"));

                // Passive Regeneration / Increase
                C.SetAffliction(ID, LungDamage);

                float str = C.GetAfflictionStrength(ID);

                // Effects:
                // Shortness of Breath
                if (str > 45)
                {
                    if (C.GetAfflictionStrength("respiratoryarrest") <= 0)
                    {
                        C.SetSymptomTrue("shortnessofbreath", 2);
                    }

                    // Cough
                    if (str > 50 && C.GetAfflictionStrength("unconsciousness") <= 0 && C.GetAfflictionStrength("lungremoved") <= 0)
                    {
                        C.SetSymptomTrue("cough", 2);
                    }

                    // Respiratory Arrest
                    if (str > 99 && HF.Chance(0.8f))
                    {
                        C.AddAffliction("respiratoryarrest", 200);
                    }
                }
            })
            .Build()
            );

        // Lung Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: Respiratory Arrest, Unconsciousness, eventual Death.
        AfflictionsToAdd.Add(
            builder.New("lungremoved")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrength(ID);

                if (str <= 0) return;

                // State check; strength is 1 if Retracted Skin is present, else 100.
                C.SetAffliction(ID, 1 + HF.BoolToNum(C.HasAfflictionLimb("retractedskin", LimbType.Head, 99), 99));

                // Effects:
                // Respiratory Arrest
                C.AddAffliction("respiratoryarrest", 200);

                // Unconsciousness
                C.SetSymptomTrue("unconsciousness", 2);
            })
            .Build()
            );

        // Lung Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd.Add(
            builder.New("lungswap").Build());

        // Brain Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: Cardiac Arrest, Respiratory Arrest, Unconsciousness, eventual Death.
        AfflictionsToAdd.Add(
            builder.New("brainremoved")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrength(ID);

                if (str <= 0) return;
                // State check; strength is 1 if Retracted Skin is present, else 100.
                C.SetAffliction(ID, 1 + HF.BoolToNum(C.HasAfflictionLimb("retractedskin", LimbType.Head, 99), 99));

                // Effects:
                // Cardiac Arrest
                C.AddAffliction("cardiacarrest", 200);

                // Respiratory Arrest
                C.AddAffliction("respiratoryarrest", 200);

                // Unconsciousness
                C.SetSymptomTrue("unconsciousness", 2);

                // Neurotrauma
                float NeurotraumaGain = 2.4f;
                if (C.GetAfflictionStrength("afmannitol") <= 0.5)
                {
                    NeurotraumaGain += 1.6f;
                }

                C.AddAffliction("neurotrauma", NeurotraumaGain);
            })
            .Build()
            );

        // Brain Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd.Add(
            builder.New("brainswap").Build());

        // Cardiac Tamponade
        // Type: Non-Limb Specific
        // Not constant; gets applied by other sources.
        // Caused By: Open Wounds (DMG) to Torso.
        // Effects: Decreases Blood Pressure, Weakness, Cough, Shortness of Breath.
        // Additional interaction with: Needle.
        AfflictionsToAdd.Add(
            builder.New("tamponade")
            .SetPriority(AfflictionPriority.MEDIUM)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Cannot have Cardiac Tamponade without a heart.
                if (C.GetAfflictionStrength("heartremoved") > 0)
                {
                    C.SetAffliction(ID, 0);
                }

                float str = C.GetAfflictionStrength(ID);

                // Passive Regeneration / Increase
                // Increases if there is no needle until 100%; else decreases until 5%.
                if (str > 0)
                {
                    C.SetAffliction(ID, Math.Clamp(str + dT * (0.5f - HF.BoolToNum(str > 5) * Math.Clamp(C.GetAfflictionStrength("needlec"), 0, 1)),
                        0,
                        100
                    ));
                }

                // Effects:
                // Shortness of Breath
                if (str > 10)
                {
                    if (C.GetAfflictionStrength("respiratoryarrest") <= 0)
                    {
                        C.SetSymptomTrue("shortnessofbreath", 3);
                    }

                    // Cough
                    if (str > 20 && C.GetAfflictionStrength("unconsciousness") <= 0 && C.GetAfflictionStrength("lungremoved") <= 0)
                    {
                        C.SetSymptomTrue("cough", 3);
                    }

                    // Weakness
                    if (str > 30)
                    {
                        C.SetSymptomTrue("weakness", 3);
                    }
                }
            })
            .Build()
            );

        // Increased Heartrate (previously Tachycardia)
        // Type: Non-Limb Specific
        // Constant; too complicated otherwise.
        // Harmless Causes: Sepsis, Blood Loss, Acidosis, Pneumothorax, Adrenaline, Alcohol Withdrawal.
        // Harmful Causes: Aortic Rupture, Acidosis, Hypotension, Hypoxemia, Traumatic Shock.
        AfflictionsToAdd.Add(
            builder.New("increasedheartrate")
            .IsConst(true)
            .SetDelay(2)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Fibrillation cannot occur without a (beating) heart
                if (C.GetAfflictionStrength("cardiacarrest") > 0 || C.GetAfflictionStrength("heartremoved") > 0)
                {
                    C.SetAffliction("fibrillation", 0);
                    C.SetAffliction(ID, 0);
                    return;
                }


                // Harmless symptom (does not lead to Fibrillation)
                bool hasSymHarmless =
                    C.GetAfflictionStrength("sepsis") > 20
                    || C.GetFloatStat("bloodamount") < 60
                    || C.GetAfflictionStrength("acidosis") > 20
                    || C.GetAfflictionStrength("pneumothorax") > 30
                    || C.GetAfflictionStrength("afadrenaline") > 1
                    || C.GetAfflictionStrength("alcoholwithdrawal") > 75;

                C.SetAffliction(ID, Math.Max(C.GetAfflictionStrength(ID), HF.BoolToNum(hasSymHarmless, 2)));

                // Fibrillation speed calculation
                float fibrillationSpeed = -0.1f
                    + Math.Clamp(C.GetAfflictionStrength("aorticrupture"), 0f, 2f)
                    + Math.Clamp(C.GetAfflictionStrength("acidosis") / 200f, 0f, 0.5f)
                    + Math.Clamp(
                        0.9f - ((C.GetAfflictionStrength("bloodpressure") + Math.Clamp(C.GetAfflictionStrength("afpressuredrug") * 5f, 0f, 20f)) / 90f),
                        0f, 1f
                    ) * 2f
                    + Math.Clamp(C.GetAfflictionStrength("hypoxemia") / 100f, 0f, 1f) * 1.5f
                    + Math.Clamp((C.GetAfflictionStrength("traumaticshock") - 5f) / 40f, 0f, 3f)
                    - Math.Clamp(C.GetAfflictionStrength("afadrenaline"), 0f, 0.9f);

                // Adrenaline halves Fibrillation speed
                if (fibrillationSpeed > 0 && C.GetAfflictionStrength("afadrenaline") > 0f)
                {
                    fibrillationSpeed /= 2f;
                }


                // Apply Fibrillation multipliers only when progressing
                if (fibrillationSpeed > 0)
                {
                    fibrillationSpeed *= NTC.GetMultiplier(C, "fibrillation") * NTConfig.Get("NT_fibrillationSpeed", 1);
                }


                // Progress IncreasedHeartrate or Fibrillation
                if (C.GetAfflictionStrength("fibrillation") <= 0)
                {
                    C.AddAffliction(ID, fibrillationSpeed * 5 * dT);

                    if (C.GetAfflictionStrength(ID) >= 100)
                    {
                        C.SetAffliction("fibrillation", 5);
                        C.SetAffliction(ID, 0);
                    }
                }
                else
                {
                    C.AddAffliction("fibrillation", fibrillationSpeed * dT);
                    C.SetAffliction(ID, 0);
                }
            })
            .Build()
            );

        // Fibrillation
        // Type: Non-Limb Specific, Mechanic
        // Not constant; gets applied by other sources.
        // Effects: Cardiac Arrest.
        AfflictionsToAdd.Add(
            builder.New("fibrillation")
            .SetPriority(AfflictionPriority.MEDIUM)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Fibrillation cannot occur without a (beating) heart
                if (C.GetAfflictionStrength("cardiacarrest") >= 1 || C.GetAfflictionStrength("heartremoved") >= 1)
                {
                    C.SetAffliction(ID, 0);
                    return;
                }

                float str = C.GetAfflictionStrength(ID);

                // Cardiac Arrest
                if (str > 20 && HF.Chance((float)Math.Pow(str / 100f, 4f)))
                {
                    C.AddAffliction("cardiacarrest", 200);
                }

            })
            .Build()
            );

        // Cardiac Arrest
        // Type: Non-Limb Specific, Lethal
        // Not constant; gets applied by other sources.
        // Caused By: Heart Removed, Brain Removed, Heart Damage, Traumatic Shock, Coma, Hypoxemia, Fibrillation, Stasis.
        // Effects: Coma, Acidosis, Hypotension, Hypoxemia.
        AfflictionsToAdd.Add(
            builder.New("cardiacarrest")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Removal Conditions
                if ((!C.GetBoolStat("stasis")) // Not in Stasis
                    && C.GetAfflictionStrength("heartremoved") <= 0 // Heart not removed
                    && C.GetAfflictionStrength("brainremoved") <= 0 // Brain not removed
                    && C.GetAfflictionStrength("heartdamage") <= 99 // Below Heart Damage threshold
                    && C.GetAfflictionStrength("traumaticshock") <= 40 // Below Traumatic Shock threshold
                    && C.GetAfflictionStrength("coma") <= 40 // Below Coma threshold
                    && C.GetAfflictionStrength("hypoxemia") <= 80 // Below Hypoxemia threshold
                    && C.GetAfflictionStrength("fibrillation") <= 20) // Below Fibrillation threshold
                {
                    C.SetAffliction(ID, 0);
                    // AffData.Strength -= 50 * dT;
                }

                // Effects:
                // Acidosis
                // Shares increase with Respiratory Arrest
                float AcidosisIncrease = 0.18f * dT;

                C.AddAffliction("acidosis", AcidosisIncrease);

                // Coma
                if (C.GetAfflictionStrength(ID) > 1 && HF.Chance(0.05f))
                {
                    C.AddAffliction("coma", 14);
                }

                // Hypotension (in BloodPressure constant itself)
                // Hypoxemia (in Hypoxemia constant itself)
            })
            .Build()
            );

        // Infected Cavity
        // Type: Non-Limb Specific, Lethal
        // Not constant; gets applied by other sources.
        // Caused By: Damage.
        // Effects: Sepsis.
        AfflictionsToAdd.Add(
            builder.New("infectedcavity")
            .SetPriority(AfflictionPriority.MEDIUM)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if ((C.GetBoolStat("stasis"))) return;

                float str = C.GetAfflictionStrength(ID);

                if (str > 0)
                {
                    C.AddAffliction("immunity", dT * (Math.Min(1.4f, Math.Max(1, .8f + str / 100)))); // Lose Immunity

                    if (C.GetAfflictionStrength("afantibiotics") < 0.1f || str > 20)
                    {
                        if (C.GetAfflictionStrength("combatstimulant") > 0) return;

                        C.AddAffliction(ID, dT * (.65f - .0125f * Math.Max(.44f * C.GetAfflictionStrength("immunity"), 20))); // Gain infections
                    }
                    else
                    {
                        C.AddAffliction(ID, dT * -0.8f); // Lose infection
                    }
                }
            })
            .Build()
            );

        // Heart Attack
        // Type: Non-Limb Specific, Lethal
        // Not constant; gets applied by other sources.
        // Caused By: Hypertension, Antibiotic Glue (XML).
        // Effects: Sweating, Shortness of Breath, Heart Damage.
        AfflictionsToAdd.Add(
            builder.New("heartattack")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Cannot have a heart attack without a heart.
                if (C.GetAfflictionStrength("heartremoved") > 0)
                {
                    C.SetAffliction(ID, 0);
                    return;
                }

                // Passive Regeneration
                C.AddAffliction(ID, 1f * dT);

                // Effects:
                // Sweating
                C.SetSymptomTrue("sweating", 2);

                // Shortness of Breath
                if (C.GetAfflictionStrength("respiratoryarrest") <= 0)
                {
                    C.SetSymptomTrue("shortnessofbreath", 2);
                }

                // Heart Damage
                C.AddAffliction("heartdamage", (Math.Clamp(C.GetAfflictionStrength("heartattack"), 0, 0.5f) * dT));
            })
            .Build()
            );

        // Heart Damage
        // Type: Non-Limb Specific, Organ Damage
        // Constant for Regeneration
        // Caused By: Heart Attack, ABX, LiOxy, HemoTransShock, Mannitol, RadSickness, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
        // Effects: Cough, Leg Swelling, Shortness of Breath, Cardiac Arrest.
        AfflictionsToAdd.Add(
            builder.New("heartdamage")
            .IsConst(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress while in Stasis
                if (C.GetBoolStat("stasis")) return;

                float str = C.GetAfflictionStrength(ID);

                float HeartDamage = (float) HF.OrganDamageCalc(C, str + NTC.GetMultiplier(C, "heartdamagegain") * C.GetFloatStat("neworgandamage"));

                // Passive Regeneration / Increase
                C.SetAffliction(ID, HeartDamage);

                // Effects:
                // Cough
                if (str > 50)
                {
                    if (C.GetAfflictionStrength("unconsciousness") <= 0 && C.GetAfflictionStrength("lungremoved") <= 0)
                    {
                        C.SetSymptomTrue("cough", 2);
                    }

                    // Leg Swelling & Shortness of Breath
                    if (str > 80)
                    {
                        if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                        {
                            C.SetSymptomTrue("legswelling", 2);
                        }

                        if (C.GetAfflictionStrength("respiratoryarrest") <= 0)
                        {
                            C.SetSymptomTrue("shortnessofbreath", 2);
                        }

                        // Cardiac Arrest
                        if (str > 99 && HF.Chance(0.3f))
                        {
                            C.AddAffliction("cardiacarrest", 200);
                        }
                    }
                }
            })
            .Build()
            );

        // Heart Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: Cardiac Arrest.
        AfflictionsToAdd.Add(
            builder.New("heartremoved")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                if (C.GetAfflictionStrength(ID) <= 0) return;
                // State check; strength is 1 if Retracted Skin is present, else 100.
                C.SetAffliction(ID, 1 + HF.BoolToNum(HF.HasAfflictionLimb(C.Human, "retractedskin", LimbType.Torso, 99), 99));

                // Effects:
                // Cardiac Arrest
                C.AddAffliction("cardiacarrest", 200);
            })
            .Build()
            );

        // Heart Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd.Add(
            builder.New("heartswap").Build());


        // Kidney Damage
        // Type: Non-Limb Specific, Organ Damage
        // Constant for Regeneration
        // Caused By: ABX, LiOxy, HemoTransShock, Mannitol, RadSickness, Hypertension, Sepsis, Hypoxemia, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
        // Effects: Acidosis, Leg Swelling, Hypertension, Bone Damage, Vomiting, Neurotrauma, Nausea.
        AfflictionsToAdd.Add(
            builder.New("kidneydamage")
            .IsConst(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress while in Stasis
                if (C.GetBoolStat("stasis")) return;

                float str = C.GetAfflictionStrength(ID);

                float KidneyDamage = (float) HF.KidneyDamageCalc(C, str
                    + NTC.GetMultiplier(C, "kidneydamagegain") * (C.GetFloatStat("neworgandamage")
                    + Math.Clamp((C.GetAfflictionStrength("bloodpressure") - 120) / 160, 0, 0.5f) * dT * 0.5f));

                // Passive Regeneration / Increase
                C.SetAffliction(ID, KidneyDamage);

                // Effects:
                // Acidosis
                float AcidosisIncrease = Math.Max(0, str - 80) / 20.0f * 0.1f * dT;

                C.AddAffliction("acidosis", AcidosisIncrease);

                // Neurotrauma
                float NeurotraumaIncrease = str / 1000.0f * dT
                    * NTC.GetMultiplier(C, "neurotraumagain")
                    * NTConfig.Get("NT_neurotraumaGain", 1)
                    * (1 - Math.Clamp(C.GetAfflictionStrength("afmannitol"), 0, 0.5f));

                C.AddAffliction("neurotrauma", NeurotraumaIncrease);

                // Hypertension (in BloodPressure constant)

                // Nausea & Leg Swelling
                if (str > 60)
                {
                    C.SetSymptomTrue("nausea", 2);

                    if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                    {
                        C.SetSymptomTrue("legswelling", 2);
                    }

                    // Vomiting
                    if (!C.HasSymptom("vomiting") && HF.Chance((float)(str - 60) / 40f * 0.07f))
                    {
                        C.SetSymptomTrue("vomiting", Rand.Range(3, 11));
                    }

                    // Bone Damage
                    if (str > 70)
                    {
                        C.AddAffliction("bonedamage", ((str - 70) / 30 * 0.15f * dT));
                    }
                }
            })
            .Build()
            );

        // Kidney Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: None; Kidney Damage 100% via Removal Surgery causes effects.
        AfflictionsToAdd.Add(
            builder.New("kidneyremoved").Build());

        // Kidney Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd.Add(
            builder.New("kidneyswap").Build());

        // Liver Damage
        // Type: Non-Limb Specific, Organ Damage
        // Constant for Regeneration
        // Caused By: ABX, LiOxy, HemoTransShock, RadSickness, Sepsis, Hypoxemia, Drunk, BFT (DMG), GSW (DMG), Sufforin Poisoning (XML).
        // Effects: Leg Swelling, Internal Bleeding, Vomiting Blood, Hypertension, Neurotrauma, AbdomDiscomfort, Jaundice, Bloating.
        AfflictionsToAdd.Add(
            builder.New("liverdamage")
            .IsConst(true)
            .SetUpdateAction((C,ID,Limb,dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float str = C.GetAfflictionStrength(ID);

                float LiverDamage = (float) HF.OrganDamageCalc(C, str + NTC.GetMultiplier(C, "liverdamagegain") * C.GetFloatStat("neworgandamage"));

                // Passive Regeneration / Increase
                C.SetAffliction(ID, LiverDamage);

                // Effects:
                // Neurotrauma
                float  NeurotraumaIncrease = (str / 800.0f * dT)
                    * NTC.GetMultiplier(C, "neurotraumagain")
                    * NTConfig.Get("NT_neurotraumaGain", 1)
                    * (1 - Math.Clamp(C.GetAfflictionStrength("afmannitol"), 0, 0.5f));

                C.AddAffliction("neurotrauma", NeurotraumaIncrease);

                // Hypertension (in BloodPressure constant itself)

                // Leg Swelling
                if (str > 40)
                {
                    if (HF.GetAfflictionStrength(C.Human, "rl_cyber", 0) < 0.1)
                    {
                        C.SetSymptomTrue("legswelling", 2);
                    }

                    if (str > 50)
                    {
                        // Bloating
                        C.SetSymptomTrue("bloating", 2);

                        if (str > 65)
                        {
                            // Abdominal Discomfort
                            if (C.GetAfflictionStrength("unconsciousness") <= 0)
                            {
                                C.SetSymptomTrue("abdominaldiscomfort", 2);
                            }

                            if (str > 80)
                            {
                                // Jaundice
                                C.SetSymptomTrue("jaundice", 2);

                                if (str >= 99 && HF.Chance(0.05f))
                                {
                                    // Internal Bleeding & Vomiting Blood
                                    C.SetSymptomTrue("vomitingblood", Random.Shared.Next(3, 10));
                                    C.AddAffliction("internalbleeding", 2);
                                }
                            }
                        }
                    }
                }
            })
            .Build()
            );

        // Organ Damage
        // Type: Non-Limb Specific, Organ Damage
        // Not constant; gets applied by other sources.
        // Caused By: Many things.
        // Effects: Many things.
        AfflictionsToAdd.Add(
            builder.New("organdamage").Build());

        // Liver Removed
        // Not constant; gets applied by other sources.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 2x.
        // Effects: None; Liver Damage 100% via Removal Surgery causes effects.
        AfflictionsToAdd.Add(
            builder.New("liverremoved").Build());

        // Liver Swap
        // Not constant; gets applied by other sources, removed on surgery end.
        // Type: Surgical Action
        // Caused By: Organ Removal Scalpel action 1x.
        // Effects: None.
        AfflictionsToAdd.Add(
            builder.New("liverswap").Build());

        // Pneumothorax
        // Type: Non-Limb Specific
        // Not constant; gets applied by other sources.
        // Caused By: Rib Fracture, Trauma to the Torso, Needle application.
        // Effects: Shortness of Breath, Hyperventilation, Increased Heartrate
        // Additional interaction with: Needle.
        AfflictionsToAdd.Add(
            builder.New("pneumothorax")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrength(ID);

                // Passive Regeneration / Increase
                // Increases if there is no needle until 100%; else decreases until 5%.
                if (str > 0)
                {
                    C.SetAffliction(ID, Math.Clamp(str + dT * (0.5f - HF.BoolToNum(str > 15) * Math.Clamp(C.GetAfflictionStrength("needlec"), 0, 1)),
                        0,
                        100
                    ));
                }

                // Effects:
                // Increased Heartrate (in IncreasedHeartrate constant)

                // Hyperventilation
                if (str > 15)
                {
                    C.SetSymptomTrue("hyperventilation", 2);

                    // Shortness of Breath
                    if (str > 40 && C.GetAfflictionStrength("respiratoryarrest") <= 0)
                    {
                        C.SetSymptomTrue("shortnessofbreath", 2);
                    }
                }
            })
            .Build()
            );

        // =============== Limbs =============== //
        // Traumatic Right Arm Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing.
        AfflictionsToAdd.Add(
            builder.New("tra_amputation").Build());

        // Traumatic Left Arm Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing.
        AfflictionsToAdd.Add(
            builder.New("tla_amputation").Build());

        // Traumatic Right Leg Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing.
        AfflictionsToAdd.Add(
            builder.New("trl_amputation").Build());

        // Traumatic Left Leg Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing.
        AfflictionsToAdd.Add(
            builder.New("tll_amputation").Build());

        // Traumatic Head Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator
        // Effects: None.
        // Applied via Damage Sustained. Does nothing. Act of removing the head kills instantly.
        AfflictionsToAdd.Add(
            builder.New("th_amputation").Build());

        // Surgical Right Arm Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing.
        AfflictionsToAdd.Add(
            builder.New("sra_amputation").Build());

        // Surgical Left Arm Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing.
        AfflictionsToAdd.Add(
            builder.New("sla_amputation").Build());

        // Surgical Right Leg Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing.
        AfflictionsToAdd.Add(
            builder.New("srl_amputation").Build());

        // Surgical Left Leg Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing.
        AfflictionsToAdd.Add(
            builder.New("sll_amputation").Build());

        // Surgical Head Amputation
        // Not constant; gets applied by other sources.
        // Type: Indicator, Surgery
        // Effects: None.
        // Result of Surgical Amputation. Does nothing. Act of removing the head kills instantly.
        AfflictionsToAdd.Add(
            builder.New("sh_amputation").Build());

        // =============== Utility =============== //
        // Luabotomy
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: None.
        // Used to determine whether or not someone should be updated.
        AfflictionsToAdd.Add(
            builder.New("luabotomy")
            .SetStrengths(0, 15, 0)
            .SetPriority(AfflictionPriority.LOW)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.SetAffliction(ID, 0.1f);
            })
            .Build()
            );

        // Luabotomy Purger
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Removes Luabotomy, then itself.
        AfflictionsToAdd.Add(
            builder.New("luabotomypurger")
            .SetStrengths(0, 2, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.SetAffliction(ID, 0);
                C.SetAffliction("luabotomy", 0);
            })
            .Build()
            );
        // StopCreatureAbuse
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: None.
        // Used to decrease additional fall damage for certain creatures.
        AfflictionsToAdd.Add(
            builder.New("stopcreatureabuse")
            .SetStrengths(0, 2, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.AddAffliction(ID, 0.5f * dT);
            })
            .Build()
            );

        // TShockTimeout
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Removes Traumatic Shock.
        // Applied during level change to prevent Traumatic Shock from taking place.
        AfflictionsToAdd.Add(
            builder.New("tshocktimeout")
            .SetStrengths(0, 300, 0)
            .SetPriority(AfflictionPriority.LOW)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.AddAffliction(ID, 3f * dT);
            })
            .Build()
            );

        // GiveIn
        // Type: Functionality
        // Effects: Enables give-in button.
        // Allows you to die while stuck in certain afflictions like Spinal Cord Injury, which are not lethal yet prevent character use.
        AfflictionsToAdd.Add(
            builder.New("givein")
            .SetStrengths(0, 2, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.AddAffliction(ID, 0.5f * dT);
            })
            .Build()
            );

        // CPR Buff
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Decreases Cardiac Arrest and Fibrillation while increasing Blood Pressure.
        // Originally done in XML.
        AfflictionsToAdd.Add(
            builder.New("cpr_buff")
            .SetStrengths(0, 2, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                C.AddAffliction(ID, -0.5f * dT);

                // Effects:
                // Reduce Cardiac Arrest
                C.SetAffliction("cardiacarrest", Math.Max(0, C.GetAfflictionStrength("cardiacarrest") - 2 * dT));

                // Reduce Fibrillation
                C.SetAffliction("fibrillation", Math.Max(0, C.GetAfflictionStrength("fibrillation") - 1 * dT));

                // Increase Blood Pressure
                C.AddAffliction("bloodpressure", 5f * dT);

                // Reduce Oxygen Low
                C.AddAffliction("oxygenlow", Math.Max(0, C.GetAfflictionStrength("oxygenlow") - 3 * dT)); // ?

                // If Cardiac Arrest is above 0 and below or equal to 0.5, clear it and apply Fibrillation
                float CardiacArrest = C.GetAfflictionStrength("cardiacarrest");
                if (CardiacArrest > 0 && CardiacArrest <= 0.5)
                {
                    C.SetAffliction("cardiacarrest", 0);
                    C.AddAffliction("fibrillation", 20);
                }
            })
            .Build()
            );

        // CPR Buff AutoPulse
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Decreases Cardiac Arrest and Fibrillation while increasing Blood Pressure.
        // Originally done in XML.
        AfflictionsToAdd.Add(
            builder.New("cpr_buff_auto")
            .SetStrengths(0, 2, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                C.AddAffliction(ID, -0.5f * dT);

                // Effects:
                // Reduce Cardiac Arrest
                C.SetAffliction("cardiacarrest", Math.Max(0, C.GetAfflictionStrength("cardiacarrest") - 1.5f * dT));

                // Reduce Fibrillation
                C.SetAffliction("fibrillation", Math.Max(0, C.GetAfflictionStrength("fibrillation") - 1 * dT));

                // Increase Blood Pressure
                C.AddAffliction("bloodpressure", 5f * dT);

                // Reduce Oxygen Low
                C.AddAffliction("oxygenlow", Math.Max(0, C.GetAfflictionStrength("oxygenlow") - 3 * dT)); // ?

                // If Cardiac Arrest is above 0 and below or equal to 0.5, clear it and apply Fibrillation
                float CardiacArrest = C.GetAfflictionStrength("cardiacarrest");
                if (CardiacArrest > 0 && CardiacArrest <= 0.5)
                {
                    C.SetAffliction("cardiacarrest", 0);
                    C.AddAffliction("fibrillation", 20);
                }
            })
            .Build()
            );

        // CPR Fracture Buff
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Prevents Fractures from fall damage during CPR.
        // Originally done in XML.
        AfflictionsToAdd.Add(
            builder.New("cpr_fracturebuff")
            .SetStrengths(0, 2, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.AddAffliction(ID, -0.5f * dT);
            })
            .Build()
            );

        // Stasis Bag Overlay
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Overlays the stasis bag sprite.
        // Applied via Stasis Bag item.
        AfflictionsToAdd.Add(
            builder.New("stasisbagoverlay")
            .SetStrengths(0, 2, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.SetAffliction(ID, HF.BoolToNum(HF.GetOuterWearIdentifier(C.Human) == "stasisbag", 2));
            })
            .Build()
            );

        // BodyBag Overlay
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Overlays the body bag sprite.
        // Applied via Body Bag item.
        AfflictionsToAdd.Add(
            builder.New("bodybagoverlay")
            .SetStrengths(0, 2, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.SetAffliction(ID, HF.BoolToNum(HF.GetOuterWearIdentifier(C.Human) == "bodybag", 2));
            })
            .Build()
            );

        // Stasis
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Enables Stasis stattype.
        // Applied via Stasis Bag item.
        AfflictionsToAdd.Add(
            builder.New("stasis")
            .SetStrengths(0,3,0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                C.AddAffliction(ID, -1f * dT);

                // Reduce Husk Infection if below 100
                if (HF.HasAffliction(C.Human, "huskinfection") && C.GetAfflictionStrength("huskinfection") < 100)
                {
                    C.AddAffliction("huskinfection", -0.075f * dT);

                    // Additional reduction if no Husk Infection Resistance
                    if (!HF.HasAffliction(C.Human, "huskinfectionresistance") || C.GetAfflictionStrength("huskinfectionresistance") <= 0)
                    {
                        C.AddAffliction("huskinfection", -0.075f * dT);
                    }
                }
            })
            .Build()
            );

        // Locked Hands
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Prevent usage of the left/right arms when triggered.
        AfflictionsToAdd.Add(
            builder.New("lockedhands")
            .IsConst(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
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

                if (LeftArmLocked && !C.GetBoolStat("lockleftarm"))
                {
                    HF.RemoveItem(LeftLockItem);
                }

                if (RightArmLocked && !C.GetBoolStat("lockrightarm"))
                {
                    HF.RemoveItem(RightLockItem);
                }

                if (!LeftArmLocked && C.GetBoolStat("lockleftarm"))
                {
                    HF.ForceArmLock(C.Human, "LeftArm");
                }

                if (!RightArmLocked && C.GetBoolStat("lockrightarm"))
                {
                    HF.ForceArmLock(C.Human, "RightArm");
                }

                C.SetAffliction(ID, HF.BoolToNum((C.GetBoolStat("lockleftarm") && C.GetBoolStat("lockrightarm")) || Handcuffed, 100));
            })
            .Build()
            );

        // TraumaticAmputating Left Leg + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_ll").Build());

        // TraumaticAmputating Right Leg + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_rl").Build());

        // TraumaticAmputating Left Arm + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_la").Build());

        // TraumaticAmputating Right Arm + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Spawns the respective limb while applying the Traumatic Amputation affliction for that limb; also applies arterial bleeding, pain and a fracture.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_ra").Build());

        // TraumaticAmputating Head + Item
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Kills you ontop of spawning a severed head.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_h").Build());

        // TraumaticAmputating Left Leg
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_ll_2").Build());

        // TraumaticAmputating Right Leg
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_rl_2").Build());

        // TraumaticAmputating Left Arm
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_la_2").Build());

        // TraumaticAmputating Right Arm
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Traumatically amputates a limb, causes a fracture, pain and an arterial bleed without spawning an item.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_ra_2").Build());

        // TraumaticAmputating Head
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Effects: Kills you.
        // Uses XML to cause TraumaAmputations.
        AfflictionsToAdd.Add(
            builder.New("gate_ta_h_2").Build());

        // Opioids in Blood
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Opioids
        // Effects: Hypoventilation.
        AfflictionsToAdd.Add(
            builder.New("afopioid")
            .SetStrengths(0,200,0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Decreases itself by 0.3/s or 0.6/2s; originally done in XML
                C.AddAffliction(ID, -0.3f * dT);

                // Effects:
                // Hypoventilation
                if (C.GetAfflictionStrength(ID) > 1)
                {
                    C.SetSymptomTrue("hypoventilation", 2);
                }
            })
            .Build()
            );

        // Anaesthetic in Blood
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Propofol
        // Effects: Hypoventilation.
        AfflictionsToAdd.Add(
            builder.New("afanaesthetic")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Decreases itself by 0.3/s or 0.6/2s; originally done in XML
                C.AddAffliction(ID, -0.3f * dT);

                // Effects:
                // Hypoventilation
                if (C.GetAfflictionStrength(ID) > 1)
                {
                    C.SetSymptomTrue("hypoventilation", 2);
                }
            })
            .Build()
            );

        // Safe Surgery (via Surgery Table)
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Surgery Table / Hospital Bed
        // Effects: Reduces / Prevents Traumatic Shock.
        AfflictionsToAdd.Add(
            builder.New("safesurgery")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                C.AddAffliction(ID, -30 * dT);
            })
            .Build()
            );

        // Artificial Ventilation (via Surgery Table)
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Surgery Table / Hospital Bed
        // Effects: Reduces Oxygen Low
        AfflictionsToAdd.Add(
            builder.New("artificialventilation")
            .SetUpdateAction((C,ID,Limb, dT) =>
            {
                // Passive Decrease
                C.AddAffliction(ID, -20 * dT);

                // Reduce Oxygen Low if lungs are present
                if (C.GetAfflictionStrength("lungremoved") <= 0)
                {
                    C.AddAffliction("oxygenlow", -100 * dT);
                }
            })
            .Build()
            );

        AfflictionsToAdd.Add(
            builder.New("chemwithdrawal")
            .Build()
            );

        AfflictionsToAdd.Add(
            builder.New("opiatewithdrawal")
            .Build()
            );

        // Alcohol Addiction
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Consuming alcohol.
        // Effects: Alcohol Withdrawal if not eternally drinking (XML).
        AfflictionsToAdd.Add(
            builder.New("alcoholaddiction")
            .Build()
            );

        // Alcohol Withdrawal
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Not consuming Alcohol with an addiction (applies via XML).
        // Effects: Craving, Sweating, Nausea, Fever, Vomiting, Headache, Confusion, Increased Heartrate, Seizure, Hypertension. 
        AfflictionsToAdd.Add(
            builder.New("alcoholwithdrawal")
            .SetUpdateAction((C,ID,Limb,dT) =>
            {
                // Effects:
                // Hypertension (in BloodPressure constant)
                // Increased Heartrate (in IncreasedHeartrate constant)

                float str = C.GetAfflictionStrength(ID);

                // Craving
                if (str > 20)
                {
                    if (C.GetAfflictionStrength("unconsciousness") <= 0)
                    {
                        C.SetSymptomTrue("craving", 2);
                    }

                    // Sweating
                    if (str > 30)
                    {
                        C.SetSymptomTrue("sweating", 2);

                        // Nausea
                        if (str > 40)
                        {
                            C.SetSymptomTrue("nausea", 2);

                            if (str > 50)
                            {
                                // Headache
                                if (C.GetAfflictionStrength("unconsciousness") <= 0)
                                {
                                    C.SetSymptomTrue("headache", 2);
                                }

                                // Seizure
                                if (HF.Chance((float)str / 1000f))
                                {
                                    C.AddAffliction("seizure", 10);
                                }

                                // Vomiting
                                if (str > 60)
                                {
                                    C.SetSymptomTrue("vomiting", 2);

                                    // Confusion
                                    if (str > 80 && C.GetAfflictionStrength("unconsciousness") <= 0)
                                    {
                                        C.SetSymptomTrue("confusion", 2);

                                        // Fever
                                        if (str > 90)
                                        {
                                            C.SetSymptomTrue("fever", 2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            })
            .Build()
            );

        // On Fire!
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Being on fire too long.
        // Effects: Visibly on fire (XML), burns (XML).
        AfflictionsToAdd.Add(builder.New("onfire").SetStrengths(0, 1, 0).Build());

        // Screaming
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Fractures, Amputations, Dislocations.
        // Effects: Character screams (XML).
        AfflictionsToAdd.Add(builder.New("screaming").SetStrengths(0, 1, 0).Build());

        // Severe Pain
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Fractures, Amputations, Dislocations.
        // Effects: Character screams (XML), gets momentarily stunned (XML).
        AfflictionsToAdd.Add(builder.New("severepain").SetStrengths(0, 2, 0).Build());

        // Pain
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Damage.
        // Effects: Damage Sounds (XML), Slowdown (XML). Removes self via XML.
        AfflictionsToAdd.Add(builder.New("pain").SetStrengths(0, 2, 0).Build());

        // Shock Pain
        // Not constant; gets applied by other sources.
        // Type: Functionality
        // Caused By: Traumatic Shock.
        // Effects: Damage Sounds (XML), Slowdown (XML). Removes self via XML.
        AfflictionsToAdd.Add(builder.New("shockpain").SetStrengths(0,2,0).Build());

        // Analgesia
        // Not constant; gets applied by other sources.
        // Type: Functionality, Surgery, Buff
        // Caused By: Painkillers.
        // Effects: Damage resistance (XML), allows surgery, reduces Pain (XML), applies screen changes (XML). Removes self via XML.
        AfflictionsToAdd.Add(builder.New("analgesia").Build());

        // Anesthesia
        // Not constant; gets applied by other sources.
        // Type: Functionality, Surgery
        // Caused By: Propofol.
        // Effects: Applies Analgesia (XML) and has side effects. Increases and removes self via XML.
        AfflictionsToAdd.Add(
           builder.New("anesthesia")
           .SetUpdateAction((C, ID, Limb, dT) =>
           {
               // Technically speaking, the lower the update interval the mroe frequent side-effects are. side effects chance should scale with delta time -Cookie 

               // Apply random side-effects.
               if (!HF.Chance(0.06f)) return;

               double casecount = 7;
               double case_ = Random.Shared.NextDouble();

               if (case_ < 1 / casecount)
               {
                   C.SetSymptomTrue("vomitingblood", (int)(5 + Random.Shared.NextDouble() * 10));
               }
               else if (case_ < 2 / casecount)
               {
                   if (C.GetAfflictionStrength("unconsciousness") <= 0)
                   {
                       C.SetSymptomTrue("blurredvision", (int)(5 + Random.Shared.NextDouble() * 10));
                   }
               }
               else if (case_ < 3 / casecount)
               {
                   if (C.GetAfflictionStrength("unconsciousness") <= 0)
                   {
                       C.SetSymptomTrue("confusion", (int)(5 + Random.Shared.NextDouble() * 10));
                   }
               }
               else if (case_ < 4 / casecount)
               {
                   C.SetSymptomTrue("fever", (int)(5 + Random.Shared.NextDouble() * 10));
               }
               else if (case_ < 5 / casecount)
               {
                   C.SetSymptomTrue("triggersym_seizure", (int)(1 + Random.Shared.NextDouble() * 2));
               }
               else if (case_ < 6 / casecount)
               {
                   HF.Fibrillate(C.Human, (float)(5 + Random.Shared.NextDouble() * 30));
               }
               else
               {
                   C.AddAffliction("psychosis", 10);
               }
           })
           .Build()
           );

       // =============== Head =============== //

       // Stroke
       // Not constant; gets applied by other sources.
       // Type: Non-Limb Specific
       // Caused By: Hypertension.
       // Effects: Headache, Coma, Seizure, Neurotrauma.
       AfflictionsToAdd.Add(
            builder.New("stroke")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                // Passive Regeneration
                C.AddAffliction(ID, (1.0f / 20f) * C.GetFloatStat("clottingrate") * -dT);

                float str = C.GetAfflictionStrength(ID);

                // Effects:
                // Neurotrauma
                float NeurotraumaGain = Math.Clamp(str, 0, 20) * 0.1f * dT
                    * NTC.GetMultiplier(C, "neurotraumagain")
                    * NTConfig.Get("NT_neurotraumaGain", 1)
                    * (1 - Math.Clamp(C.GetAfflictionStrength("afmannitol"), 0f, 0.5f));

                C.AddAffliction("neurotrauma", NeurotraumaGain);

                // Headache
                if (str > 1 && C.GetAfflictionStrength("unconsciousness") <= 0)
                {
                    C.SetSymptomTrue("headache", 2);
                }

                // Coma & Seizure
                if (str > 1 && HF.Chance(0.05f))
                {
                    C.AddAffliction("coma", 14);
                    C.AddAffliction("seizure", 10);
                }
            })
            .Build()
            );

        // Neurotrauma
        // Constant for Regeneration
        // Type: Non-Limb Specific, Organ Damage, Lethal
        // Caused By: Stroke, Liver Damage, Kidney Damage, Sepsis, Hypoxemia, Items, Traumatic Shock, Cyanide Poisoning, GSW (DMG).
        // Effects: Unconsciousness, Respiratory Arrest
         AfflictionsToAdd.Add(
            builder.New("neurotrauma")
            .IsConst(true)
            .SetStrengths(0, 200, 0)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                // Does not regenerate with a Skull Fracture
                bool HasFracture = C.HasAfflictionLimb("fracturedskull", LimbType.Head, 1);
                float FractureModifier = HasFracture ? 0 : 1;

                float PassiveRegeneration = -0.1f * C.GetFloatStat("healingrate") * FractureModifier * dT;

                if (PassiveRegeneration < -0.08f * dT)
                {
                    PassiveRegeneration *= 2.5f;
                }

                float str = C.GetAfflictionStrength(ID);

                C.SetAffliction(ID, Math.Clamp(str + PassiveRegeneration, 0, 200));


                // Effects:
                // Unconsciousness & Respiratory Arrest
                if (str > 100)
                {
                    C.SetSymptomTrue("unconsciousness", 2);
                    if (HF.Chance(0.05f))
                    {
                        C.AddAffliction("respiratoryarrest", 200);
                    }
                }
            })
            .Build()
            );

        // Seizure
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Stroke, Acidosis, Alkalosis, Withdrawal, Opiate Overdose, Anesthesia, Radiation Sickness
        // Effects: Unconsciousness, Spasms
        AfflictionsToAdd.Add(
            builder.New("seizure")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Regeneration:
                C.AddAffliction(ID, -1f * dT);

                // Effects:
                // Spasms
                if (C.GetAfflictionStrength(ID) > 0.1f)
                {
                    C.SetSymptomTrue("unconsciousness", 2);

                    foreach (LimbType l in HF.LimbsToCheck)
                    {
                        C.AddAfflictionLimb("spasm", l, 10);
                    }
                }
            })
            .Build()
            );

        // Coma
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Stroke, Cardiac Arrest, High Acidosis, Morbusine Poisoning, Naloxone fail.
        // Effects: Cardiac Arrest, Unconsciousness.
        AfflictionsToAdd.Add(
            builder.New("coma")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                // Passive Regeneration
                if (C.GetAfflictionStrength("acidosis") < 20
                    && C.GetAfflictionStrength("alkalosis") < 20
                    && C.GetAfflictionStrength("heartdamage") < 30
                    && C.GetAfflictionStrength("lungdamage") < 40
                    && C.GetFloatStat("availableoxygen") > 60)
                {
                    C.AddAffliction(ID, -1f * dT);
                }
                else
                {
                    C.AddAffliction(ID, -0.4f * dT);
                }

                // Effects:

                float str = C.GetAfflictionStrength(ID);

                // Unconsciousness
                if (str > 15)
                {
                    C.SetSymptomTrue("unconsciousness", 2);

                    // Cardiac Arrest
                    if (str > 40 && HF.Chance(0.03f))
                    {
                        C.AddAffliction("cardiacarrest", 200);
                    }
                }
            })
            .Build()
            );

        // Spinal Cord Injury
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Unstable Neck Fractures.
        // Effects: Paralysis (XML), Analgesia (XML).
        AfflictionsToAdd.Add(
            builder.New("spinalcordinjury")
            .Build()
            );

        // Carotid Arterial Cut
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Damage.
        // Effects: Blood Loss (XML), Internal Bleeding (XML). Increases self via XML.
        AfflictionsToAdd.Add(
            builder.New("carotidarterialcut")
            .Build()
            );

        // =============== Item Derived =============== //

        // Adrenaline in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Adrenaline item.
        // Effects: Melee Damage increased (XML), Analgesia (XML).
        AfflictionsToAdd.Add(
            builder.New("afadrenaline")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.AddAffliction(ID, -1f * dT);
            })
            .Build()
            );

        // Needle in Chest
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Needle item.
        // Effects: Reduced Pneumothorax / Cardiac Tamponade.
        AfflictionsToAdd.Add(
            builder.New("needlec")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.AddAffliction(ID, -0.15f * dT);
            })
            .Build()
            );

        // Saline in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Saline item.
        // Effects: Increased Acidosis, Blood Pressure.
        AfflictionsToAdd.Add(
            builder.New("afsaline")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.AddAffliction(ID, -0.25f * dT);
                C.AddAffliction("acidosis", 0.1f * dT);
            })
            .Build()
            );

        // Ringers Solution in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Ringer's Solution item.
        // Effects: Increased Alkalosis, Blood Pressure.
        AfflictionsToAdd.Add(
            builder.New("afringerssolution")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                C.AddAffliction(ID, -0.25f * dT);

                // Effects:
                // Alkalosis
                C.AddAffliction("acidosis", 0.2f);

                // Blood Pressure (in BloodPressure constant)
            })
            .Build()
            );


        // Mannitol in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Mannitol Item.
        // Effects: Reduce Neurotrauma.
        AfflictionsToAdd.Add(
            builder.New("afmannitol")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.5 per second in XML.
                // Adjusted, that became 1 per 2 seconds.
                C.AddAffliction(ID, -0.5f * dT);

                // Effects:
                // Reduce Neurotrauma if Blood Pressure and Hypoxemia conditions are met.
                if (C.GetAfflictionStrength("bloodpressure") >= 70 && C.GetAfflictionStrength("hypoxemia") <= 30)
                {
                    C.AddAffliction("neurotrauma", -2 * dT);
                }
            })
            .Build()
            );


        // Immunosuppressants in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Azathioprine Item.
        // Effects: Reduce Immunity.
        AfflictionsToAdd.Add(
            builder.New("afimmunosuppressant")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                C.AddAffliction(ID, -0.25f * dT);

                // Effects:
                // Reduce Immunity
                if (C.GetAfflictionStrength("immunity") >= 2.5)
                {
                    C.AddAffliction("immunity", -4 * dT);
                }
            })
            .Build()
            );

        // Pressure-increasing drugs in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Nitroglycerin, Sodium Nitroprusside Items.
        // Effects: Increase target Blood Pressure.
        AfflictionsToAdd.Add(
            builder.New("afpressuredrug")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                C.AddAffliction(ID, -0.25f * dT);

                // Effects:
                // Blood Pressure (in BloodPressure constant)
            })
            .Build()
            );

        // Thiamine in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Thiamine Item.
        // Effects: Increase specific organ damage healing.
        AfflictionsToAdd.Add(
            builder.New("afthiamine")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                C.AddAffliction(ID, -0.25f * dT);

                // Effects:
                // Additional Healing (in HF.NewOrganDamage)
            })
            .Build()
            );

        // Streptokinase in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Streptokinase Item.
        // Effects: Increase stroke chance, cure Heart Attack / Hemotransfusion shock.
        AfflictionsToAdd.Add(
            builder.New("afstreptokinase")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                C.AddAffliction(ID, 0.25f * dT);

                // Effects:
                // Cures Heart Attack / HemoTransShock in ItemFunctions
                // Hypertension Stroke (in BloodPressure constant)
            })
            .Build()
            );

        // Antibiotics in Blood
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Item Derived
        // Caused By: Broad-Spectrum Antibiotics Item.
        // Effects: Decreases Sepsis, extra Organ Damage, decreased Husk Infection.
        AfflictionsToAdd.Add(
            builder.New("afantibiotics")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 0.25 per second in XML.
                // Adjusted, that became 0.5 per 2 seconds.
                C.AddAffliction(ID, -0.5f * dT);

                // Effects:
                // Specific Organ Damage
                C.AddAffliction("organdamage", 0.2f * dT);
                C.AddAffliction("kidneydamage", 0.175f * dT);
                C.AddAffliction("liverdamage", 0.175f * dT);
                C.AddAffliction("heartdamage", 0.1f * dT);
                C.AddAffliction("lungdamage", 0.1f * dT);

                // Reduce Husk Infection
                if (HF.HasAffliction(C.Human, "huskinfection") && C.GetAfflictionStrength("huskinfection") < 75)
                {
                    C.AddAffliction("huskinfection", 0.5f * dT);
                }

                // Sepsis
                if (C.GetAfflictionStrength("sepsis") > 0)
                {
                    C.AddAffliction("sepsis", 1 * dT);
                }
            })
            .Build()
            );

        // =============== Surgical =============== //
        // Cavity Cleaning
        // Not constant; gets applied by other sources
        // Type: Surgery, Non-Limb Specific
        // Caused By: Antiseptic Sprayer + Saline
        // Effects: Cures Infected Cavity
        AfflictionsToAdd.Add(
            builder.New("caviclean")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Once it hits 100, remove itself and infected cavity.
                if (C.GetAfflictionStrength(ID) == 100)
                {
                    C.SetAffliction(ID, 0);
                    C.SetAffliction("infectedcavity", 0);
                }
            })
            .Build()
            );


        // Traumatic Shock
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific, Lethal
        // Caused By: Unsafe Surgery
        // Effects: Hypotension, Cardiac Arrest, Respiratory Arrest, Neurotrauma, Psychosis, Pain.
        AfflictionsToAdd.Add(
            builder.New("traumaticshock")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Removes on TShockTimeout
                if (C.GetAfflictionStrength("tshocktimeout") > 0)
                {
                    C.SetAffliction(ID, 0);
                    return;
                }

                // Passive Decrease
                bool IsSedated = C.GetBoolStat("sedated");
                bool IsSafeSurgery = C.GetAfflictionStrength("safesurgery") > 0;
                bool IsAnesthesized = C.GetAfflictionStrength("anesthesia") > 15;

                bool ShouldReduce = (IsSedated && IsSafeSurgery || IsAnesthesized);

                C.AddAffliction(ID, (0.5f + HF.BoolToNum(ShouldReduce, 1.5f)) * -dT);

                float str = C.GetAfflictionStrength(ID);

                // Effects:
                // Pain & Psychosis
                if (str > 5)
                {
                    if (C.GetAfflictionStrength("unconsciousness") < 0.1)
                    {
                        C.AddAffliction("shockpain", (10 * dT));
                        C.AddAffliction("psychosis", (str / 100 * dT));
                    }

                    // Respiratory Arrest
                    if (str > 30 && HF.Chance(0.2f))
                    {
                        C.AddAffliction("respiratoryarrest", 200);
                    }

                    // Cardiac Arrest
                    if (str > 40 && HF.Chance(0.1f))
                    {
                        C.AddAffliction("cardiacarrest", 200);
                    }
                }
            })
            .Build()
            );

        // Ballooned Aorta
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Endovascular Balloon item.
        // Effects: Gangrene in extremities (XML), Organ Damage, Specific Organ Damage, Reduced Bleeding in extremities (XML).
        AfflictionsToAdd.Add(
            builder.New("balloonedaorta")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Effects:
                // Vanilla Organ Damage
                C.AddAffliction("organdamage", 0.5f * dT);

                // Specific Organ Damage
                C.AddAffliction("liverdamage", 0.5f * dT);
                C.AddAffliction("kidneydamage", 0.5f * dT);
            })
            .Build()
            );

        // =============== Torso =============== //
        // Aortic Rupture
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Damage.
        // Effects: Blood Loss (XML), Internal Bleeding (XML), Chest Pain, Abdominal Pain, Unconsciousness.
        AfflictionsToAdd.Add(
            builder.New("aorticrupture")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrength(ID);

                // Chest Pain & Abdominal Pain & Unconsciousness
                if (str > 0)
                {
                    if (C.GetAfflictionStrength("unconsciousness") <= 0 && (!C.GetBoolStat("sedated")))
                    {
                        C.SetSymptomTrue("chestpain", 2);
                        C.SetSymptomTrue("abdominalpain", 2);
                    }
                }
            })
            .Build()
            );

        // Internal Bleeding
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Damage.
        // Effects: Blood Loss (XML), Internal Bleeding (XML), Chest Pain, Abdominal Pain, Unconsciousness.
        AfflictionsToAdd.Add(
            builder.New("internalbleeding")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                // Passive Regeneration
                C.AddAffliction(ID, -dT * 0.02f * C.GetFloatStat("clottingrate"));

                float str = C.GetAfflictionStrength(ID);

                // Effects:
                // Blood Loss
                if (str > 0)
                {
                    C.AddAffliction("bloodloss", (str * (1f / 40f) * dT));

                    // Vomiting Blood
                    if (str > 50)
                    {
                        C.SetSymptomTrue("vomitingblood", 2);
                    }
                }
            })
            .Build()
            );

        // =============== Bones =============== //

        // Bone Damage
        // Not constant; gets applied by other sources.
        // Type: Non-Limb Specific
        // Caused By: Kidney Damage, Radiation Sickness, Sepsis, Hypoxemia.
        // Effects: Bone Death, Fractures.
        // Fractures
        AfflictionsToAdd.Add(
            builder.New("bonedamage")
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float str = C.GetAfflictionStrength(ID);

                if (!(str > 0)) return;

                // Passive Regeneration
                C.SetAffliction(ID, (float) HF.OrganDamageCalc(C, str));

                // Bone Regeneration
                if (str < 90)
                {
                    C.AddAffliction(ID, -C.GetFloatStat("bonegrowthCount") * 0.3f * dT);
                }
                else if (C.GetFloatStat("bonegrowthCount") >= 6)
                {
                    C.AddAffliction(ID, -2 * dT);
                }

                if (str <= 90) return;

                // Fractures
                foreach (var limb in HF.LimbsToCheck)
                {
                    if (HF.Chance(0.01f))
                    {
                        HF.BreakLimb(C.Human, Limb);
                    }
                }
            })
            .Build()
            );

        // =============== MUST RUN AFTER EVERYTHING ELSE =============== //
        // Probably needs even special treatment than this.

        // Slowdown 
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Decreases character by a percentage proportional to the affliction strength.

        // TODO: move it to post update

        AfflictionsToAdd.Add(
            builder.New("slowdown")
            .IsConst(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                C.SetAffliction(ID, C.GetFloatStat("slowdown"));
            })
            .Build()
            );

        // Stun 
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Used to stun the character.
        AfflictionsToAdd.Add(
            builder.New("stun")
            .SetStrengths(0, 30, 0)
            .IsConst(true)
            .SetUpdateAction((C,ID,Limb,dT) =>
            {
                if (C.GetAfflictionStrength("spinalcordinjury") > 0
                   || C.GetAfflictionStrength("anesthesia") > 15
                   || C.HasSymptom("unconsciousness"))
                {
                    C.SetAffliction(ID, Math.Max(5, C.GetAfflictionStrength(ID)));
                }
                else
                {
                    C.SetAffliction(ID, 0);
                }
            })
            .Build()
            );

        // Stun 
        // Constant; too complicated otherwise.
        // Type: Functionality
        // Effects: Used to stun the character.
        AfflictionsToAdd.Add(
            builder.New("combatstimulant")
            .Build());

        // Surgical Incision
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Scalpel.
        // Effects: Blood Loss, Traumatic Shock, increases self in XML.
        AfflictionsToAdd.Add(
            builder.New("surgeryincision")
            .IsLimbSpecific(true)
            .Build());

        // Clamped Bleeding
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Hemostat.
        // Effects: Prevents Surgery Incision Blood Loss (Scalpel XML).
        AfflictionsToAdd.Add(
            builder.New("clampedbleeding")
            .IsLimbSpecific(true)
            .Build());

        // Drilled Bones
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Surgical Drill.
        // Effects: Applies Traumatic Shock (XML).
        AfflictionsToAdd.Add(
            builder.New("drilledbones")
            .IsLimbSpecific(true)
            .Build());

        // Retracted Skin
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Skin Retractors.
        // Effects: Applies Traumatic Shock (XML).
        AfflictionsToAdd.Add(
            builder.New("retractedskin")
            .IsLimbSpecific(true)
            .Build());

        // Sutured Incision
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Stitching a Surgical Incision.
        // Effects: None.
        AfflictionsToAdd.Add(
            builder.New("suturedi")
            .IsLimbSpecific(true)
            .SetPriority(AfflictionPriority.MEDIUM)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 1 per second in XML.
                // Adjusted, that became 0.44 per 2 seconds.
                // math ain't mathing -cookie
                C.AddAfflictionLimb(ID, Limb, -1f * dT);
            })
            .Build()
            );

        // Sutured Wound
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Stitching an Open Wound.
        // Effects: Vitality damage proportional to affliction strength.
        AfflictionsToAdd.Add(
            builder.New("suturedw")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                // Passive Decrease
                // Originally had a maxstrength of 100, and reduced by 1 per second in XML.
                // Adjusted, that became 0.44 per 2 seconds.
                // math ain't mathing -cookie
                C.AddAfflictionLimb(ID, Limb, -0.3f * dT);
            })
            .Build()
            );

        // Sawed Bones
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Surgical
        // Caused By: Surgical Saw.
        // Effects: Applies Traumatic Shock (XML).
        AfflictionsToAdd.Add(
            builder.New("sawedbones")
            .IsLimbSpecific(true)
            .Build());

        // Bleeding
        // Not constant; gets applied by other sources.
        // Type: Limb Specific, Basegame Override
        // Caused By: Damage, failed skill checks.
        // Effects: Blood Loss (Hardcoded?)
        AfflictionsToAdd.Add(
            builder.New("bleeding")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Regeneration
                C.AddAfflictionLimb(ID, Limb, -(C.GetFloatStat("clottingrate") * 0.1f
                    + Math.Clamp(C.GetAfflictionStrengthLimb("bandaged", Limb), 0, 1) * 0.5f
                    + Math.Clamp(C.GetAfflictionStrengthLimb("bandageddirty", Limb), 0, 1) * 0.25f
                ) * dT);

            })
            .Build()
            );

        // Stimulated Bone Growth
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, failed skill checks.
        // Effects: Decreases bone damage.
        AfflictionsToAdd.Add(
            builder.New("stimulatedbonegrowth")
            .SetPriority(AfflictionPriority.MEDIUM)
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                C.AddAfflictionLimb(ID, Limb, -0.5f * dT);
            })
            .Build()
            );

        // Arm + Leg Fractures
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, failed skill checks.
        // Effects: Pain, lost ability of limb, Internal Damage.
        AfflictionsToAdd.Add(
           builder.New("fracturedextremity")
           .IsLimbSpecific(true)
           .SetUpdateAction((C, ID, Limb, dT) =>
           {
               float str = C.GetAfflictionStrengthLimb(ID, Limb);

               if (!(str > 0)) return;

               bool HasCast = HF.HasAfflictionLimb(C.Human, "plastercast", Limb);
               bool HasBandage = HF.HasAfflictionLimb(C.Human, "bandaged", Limb) || HF.HasAfflictionLimb(C.Human, "bandageddirty", Limb);

               // Arms: halt progression between 90-100 if bandaged
               if (Limb == LimbType.LeftArm || Limb == LimbType.RightArm)
               {
                   if (str > 90 && str < 100 && HasBandage)
                   {
                       return;
                   }
               }

               // Passive Increase if no cast
               C.AddAfflictionLimb(ID, Limb, 2 * HF.BoolToNum(!HasCast) * dT);

               // Legs: adrenaline causes Bleeding if no cast and not ragdolled
               if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
               {
                   if (!HasCast && HF.HasAffliction(C.Human, "afadrenaline", 1) && !C.Human.IsRagdolled)
                   {
                       C.AddAfflictionLimb("bleeding", Limb, 15);
                   }
               }

               // Internal Damage if no cast
               if (!HasCast && !C.GetBoolStat("sedated") && (HF.LimbIsExtremity(Limb) || !HasBandage))
               {
                   C.AddAfflictionLimb("internaldamage", Limb, 0.1f * dT);
               }
           })
           .Build()
           );

        // Arm + Leg Dislocation
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, failed skill checks.
        // Effects: Pain (XML), lost ability of limb, Internal Damage.
        AfflictionsToAdd.Add(
           builder.New("dislocation")
           .IsLimbSpecific(true)
           .SetUpdateAction((C, ID, Limb, dT) =>
           {
               float str = C.GetAfflictionStrengthLimb(ID, Limb);

               if (!(str > 0)) return;

               // If painlessness is present, don't cause problems
               if (C.GetBoolStat("sedated")) return;

               if (C.GetAfflictionStrengthLimb("plastercast", Limb) <= 0 && C.GetAfflictionStrengthLimb("bandaged", Limb) <= 0 && C.GetAfflictionStrengthLimb("bandageddirty", Limb) <= 0)
               {
                   C.AddAfflictionLimb("internaldamage", Limb, 0.1f * dT);
                   if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
                   {
                       C.SetFloatStat("speedmultiplier", C.GetFloatStat("speedmultiplier") * 0.8f); // slow the character down.
                   }
               }
           })
           .Build()
           );

       // Tourniquet around Extremity
       // Not constant; gets applied by other sources.
       // Type: Limb Specific
       // Caused By: Tourniquet item.
       // Effects: Reduces Bleeding (XML), Gangrene.
       AfflictionsToAdd.Add(
            builder.New("tourniqueted")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                C.AddAfflictionLimb("gangrene", Limb, HF.BoolToNum(HF.Chance(0.1f)) * 0.5f * NTConfig.Get("NT_gangrenespeed", 1) * dT);
            })
            .Build()
            );

        // Plaster Cast
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Gypsum item.
        // Effects: Heals fractures, slows character.
        AfflictionsToAdd.Add(
            builder.New("plastercast")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Effects:
                // Leg slowdown
                if (Limb == LimbType.LeftLeg || Limb == LimbType.RightLeg)
                {
                    C.SetFloatStat("speedmultiplier", C.GetFloatStat("speedmultiplier") * 0.8f);
                }

                // Heal Fracture
                HF.BreakLimb(C.Human, Limb, -(100.0f / 300.0f) * dT);
            })
            .Build()
            );

        // Arterial Cut on Extremity
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage.
        // Effects: Blood Loss (XML).
        AfflictionsToAdd.Add(
            builder.New("arterialcut")
            .Build()
            );

        // Gangrene
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Tourniquets, Sepsis, Aortic Balloon (XML).
        // Effects: Blood Loss (XML).
        AfflictionsToAdd.Add(
            builder.New("gangrene")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Limb must be an extremity
                if (!HF.LimbIsExtremity(Limb)) return;

                // Surgical amputation prevents Gangrene on that stump
                if (HF.LimbIsSurgicallyAmputated(C.Human, Limb))
                {
                    C.SetAfflictionLimb(ID, Limb, 0);
                    return;
                }

                // Passive Regeneration below 15
                if (str < 15)
                {
                    C.AddAfflictionLimb(ID, Limb, -0.01f * C.GetFloatStat("healingrate") * dT);
                }
            })
            .Build()
            );

        // Bandage applied to Limb
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Bandage items.
        // Effects: Reduces bleeding, heals wounds, reduces infection.
        AfflictionsToAdd.Add(
            builder.New("bandaged")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {

                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                float WoundDamage = C.GetAfflictionStrengthLimb("firstdegreeburn", Limb)
                    + C.GetAfflictionStrengthLimb("seconddegreeburn", Limb)
                    + C.GetAfflictionStrengthLimb("thirddegreeburn", Limb)
                    + C.GetAfflictionStrengthLimb("lacerations", Limb)
                    + C.GetAfflictionStrengthLimb("foreignbody", Limb)
                    + C.GetAfflictionStrengthLimb("arterialcut", Limb)
                    + C.GetAfflictionStrengthLimb("infectedwound", Limb);
                
                float BandageDirtifySpeed = 0.1f
                    + Math.Clamp(WoundDamage / 100, 0, 0.4f)
                    + C.GetAfflictionStrengthLimb("bleeding", Limb) / 20;

                // Dirtify bandage over time
                C.AddAfflictionLimb(ID, Limb, -BandageDirtifySpeed * dT);

                float DirtyBandageStrength = C.GetAfflictionStrengthLimb("bandageddirty", Limb);

                // Transition to dirty bandage
                if (str <= 0.5f)
                {
                    
                    C.SetAfflictionLimb("bandageddirty", Limb, Math.Max(DirtyBandageStrength, 1));
                    C.SetAfflictionLimb(ID, Limb, 0);
                }

                if (DirtyBandageStrength > 0)
                {
                    C.AddAfflictionLimb("bandageddirty", Limb, BandageDirtifySpeed * dT);
                }

                // Effects:
                // Slowdown
                C.SetFloatStat("speedmultiplier", C.GetFloatStat("speedmultiplier") * 0.9f);

                // Wound Healing
                C.AddAfflictionLimb("lacerations", Limb, -Math.Clamp(str, 0f, 1f) * 0.1f * dT);
                C.AddAfflictionLimb("firstdegreeburn", Limb, -Math.Clamp(str, 0f, 1f) * 0.1f * dT);
                C.AddAfflictionLimb("seconddegreeburn", Limb, -Math.Clamp(str, 0f, 1f) * 0.1f * dT);
                C.AddAfflictionLimb("thirddegreeburn", Limb, -Math.Clamp(str, 0f, 1f) * 0.1f * dT);


                // Infection Healing
                if (C.GetAfflictionStrengthLimb("infectedwound", Limb) > 0)
                {
                    C.AddAfflictionLimb("infectedwound", Limb, -Math.Clamp(str, 0f, 1f) * 1.5f * dT);
                }
            })
            .Build()
            );

        // Dirty Bandage around Limb
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Dirtyfication.
        // Effects: Reduces bleeding, heals wounds, causes infection.
        AfflictionsToAdd.Add(
            builder.New("bandageddirty")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                float BandagedStrength = C.GetAfflictionStrengthLimb("bandaged", Limb);
                if (BandagedStrength > 0)
                {
                    C.SetAfflictionLimb("bandaged", Limb, 0);
                }

                float WoundDamage = C.GetAfflictionStrengthLimb("firstdegreeburn", Limb)
                    + C.GetAfflictionStrengthLimb("seconddegreeburn", Limb)
                    + C.GetAfflictionStrengthLimb("thirddegreeburn", Limb)
                    + C.GetAfflictionStrengthLimb("lacerations", Limb)
                    + C.GetAfflictionStrengthLimb("foreignbody", Limb)
                    + C.GetAfflictionStrengthLimb("arterialcut", Limb)
                    + C.GetAfflictionStrengthLimb("infectedwound", Limb);

                float BandageDirtifySpeed = 0.1f
                    + Math.Clamp(WoundDamage / 100, 0, 0.4f)
                    + C.GetAfflictionStrengthLimb("bleeding", Limb) / 20;

                C.AddAfflictionLimb(ID, Limb, BandageDirtifySpeed * dT);

                // Effects:
                // Slowdown
                C.SetFloatStat("speedmultiplier", C.GetFloatStat("speedmultiplier") * 0.9f);

                // Wound Healing

                C.AddAfflictionLimb("lacerations", Limb, -Math.Clamp(str, 0f, 1f) * 0.05f * dT);
                C.AddAfflictionLimb("firstdegreeburn", Limb, -Math.Clamp(str, 0f, 1f) * 0.05f * dT);
                C.AddAfflictionLimb("seconddegreeburn", Limb, -Math.Clamp(str, 0f, 1f) * 0.05f * dT);
                C.AddAfflictionLimb("thirddegreeburn", Limb, -Math.Clamp(str, 0f, 1f) * 0.05f * dT);
            })
            .Build()
            );

        // Gel Coolant Pack applied to Limb
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Gel Coolant Pack.
        // Effects: Amplifies healing, slows character.
        AfflictionsToAdd.Add(
            builder.New("iced")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Decrease
                C.AddAfflictionLimb(ID, Limb, -1.7f * dT);

                // Effects:
                // Slowdown (5% per limb)
                C.SetFloatStat("speedmultiplier", C.GetFloatStat("speedmultiplier") * 0.95f);
                
                // Effects:
                // Reduce Internal Bleeding if on Torso
                if (Limb == LimbType.Torso)
                {
                    C.AddAffliction("internalbleeding", -0.2f * dT);
                }

                // Heal Blunt Force Trauma
                C.AddAfflictionLimb("blunttrauma", Limb, -Math.Clamp(str, 0f, 1f) * 0.3f * C.GetFloatStat("healingrate") * dT);
            })
            .Build()
            );

        // Antibiotic Ointment applied to Limb
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Antibiotic Ointment.
        // Effects: Amplifies healing.
        AfflictionsToAdd.Add(
            builder.New("ointmented")
            .IsLimbSpecific(true)
            .SetPriority(AfflictionPriority.MEDIUM)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Decrease
                C.AddAfflictionLimb(ID, Limb, -1.2f * dT);

                // Effects:
                // Reduce Infected Wounds
                if (C.GetAfflictionStrengthLimb("infectedwound", Limb) <= 60)
                {
                    C.AddAfflictionLimb("infectedwound", Limb, -3f * dT);
                }
            })
            .Build()
            );

        // Infected Wound
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Burns, Foreign Bodies, Lacerations, Explosive Damage, Gunshot Wounds.
        // Effects: Inflammation.
        AfflictionsToAdd.Add(
            builder.New("infectedwound")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                // Passive decrease from immunity, bandaged, ointmented
                float InfectIndex = (-C.GetAfflictionStrength("immunity") / 200
                    - Math.Clamp(C.GetAfflictionStrengthLimb("bandaged", Limb), 0, 1) * 1.5f
                    - C.GetAfflictionStrengthLimb("ointmented", Limb) * 3
                ) * dT;

                // Dirty bandage :skull:
                if (C.GetAfflictionStrengthLimb("bandageddirty", Limb) > 10)
                {
                    InfectIndex += (C.GetAfflictionStrengthLimb("bandageddirty", Limb) / 20) * dT;
                }

                if (InfectIndex > 0)
                {
                    InfectIndex *= NTConfig.Get("NT_infectionRate", 1) * Math.Clamp(C.GetAfflictionStrengthLimb("iced", Limb), 1, 10);
                }

                C.AddAfflictionLimb(ID, Limb, InfectIndex);

                // Effects:
                // Inflammation
                if (str > 10)
                {
                    C.AddAfflictionLimb("inflammation", Limb, 0.5f * dT);
                }
            })
            .Build()
            );

        // Foreign Body
        // Not constant; gets applied by other sources.
        // Type: Limb Specific
        // Caused By: Damage, fractures.
        // Effects: Inflammation, Sepsis.
        AfflictionsToAdd.Add(
            builder.New("foreignbody")
            .IsLimbSpecific(true)
            .SetUpdateAction((C, ID, Limb, dT) =>
            {
                float str = C.GetAfflictionStrengthLimb(ID, Limb);

                if (!(str > 0)) return;

                // Passive Decrease
                if (str < 15)
                {
                    C.AddAfflictionLimb(ID, Limb, 0.05f * C.GetFloatStat("healingrate") * dT);
                }

                // Arterial Cut chance
                float ForeignBodyAbove20 = str >= 20 ? str : 0;
                float ForeignBodyCutChance = (float) Math.Pow(ForeignBodyAbove20 / 100, 6) * 0.5f;

                if (C.GetAfflictionStrengthLimb("bleeding", Limb) > 80 || HF.Chance(ForeignBodyCutChance))
                {
                    HF.ArteryCutLimb(C.Human, Limb);
                }

                // Effects:
                // Sepsis
                float GangreneAbove15 = C.GetAfflictionStrengthLimb("gangrene", Limb) >= 15 ? C.GetAfflictionStrengthLimb("gangrene", Limb) : 0;
                float InfectedAbove50 = C.GetAfflictionStrengthLimb("infectedwound", Limb) >= 50 ? C.GetAfflictionStrengthLimb("infectedwound", Limb) : 0;

                float SepsisChance = GangreneAbove15 / 400
                    + InfectedAbove50 / 1000
                    + ForeignBodyCutChance;

                if (HF.Chance((float)SepsisChance))
                {
                    C.AddAffliction("sepsis", dT * NTConfig.Get("NT_SepsisRate", 1));
                }

                // Inflammation
                if (str > 15)
                {

                    C.AddAfflictionLimb("inflammation", Limb, 0.5f * dT);
                }

                // Infected Wounds
                // Does not progress in Stasis
                if (C.GetBoolStat("stasis")) return;

                float ForeignBodyInfectIndex = str / 40 * dT;
                C.AddAfflictionLimb("infectedwound", Limb, ForeignBodyInfectIndex / 5);

                // Decrease Immunity
                C.AddAffliction("immunity", -(Math.Clamp(ForeignBodyInfectIndex / 3, 0, 10)));

                if (C.GetAfflictionStrengthLimb("bandageddirty", Limb) > 10)
                {
                    float ForeignBodyDirtyIndex = str / 40 * dT;
                    C.AddAffliction("infectedwound", ForeignBodyInfectIndex / 5);

                    // Decrease Immunity
                    C.AddAffliction("immunity", -(Math.Clamp(ForeignBodyDirtyIndex / 3, 0, 10)));
                }
            })
            .Build()
            );

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
        //        C.GetAfflictionStrength("respiratoryarrest")= 100;
        //    };
        //SymptomsToAdd["triggersym_seizure"] = new("triggersym_seizure", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_seizure"].Real = false;
        //SymptomsToAdd["triggersym_seizure"].Const = true;
        //SymptomsToAdd["triggersym_seizure"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAfflictionStrength("seizure")= 100;
        //    };
        //SymptomsToAdd["triggersym_stroke"] = new("triggersym_stroke", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_stroke"].Real = false;
        //SymptomsToAdd["triggersym_stroke"].Const = true;
        //SymptomsToAdd["triggersym_stroke"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAfflictionStrength("stroke")= 100;
        //    };
        //SymptomsToAdd["triggersym_coma"] = new("triggersym_coma", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_coma"].Real = false;
        //SymptomsToAdd["triggersym_coma"].Const = true;
        //SymptomsToAdd["triggersym_coma"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAfflictionStrength("seizure")= 100;
        //    };
        //SymptomsToAdd["triggersym_cardiacarrest"] = new("triggersym_cardiacarrest", 0, 100, 0, AfflictionPriority.HIGH);
        //SymptomsToAdd["triggersym_cardiacarrest"].Real = false;
        //SymptomsToAdd["triggersym_cardiacarrest"].Const = true;
        //SymptomsToAdd["triggersym_cardiacarrest"].UpdateAction =
        //    (NTHuman C, string ID, LimbType Limb, NTHumanSymptomData AffData) =>
        //    {
        //        if (AffData.Strength <= 0) return;
        //        C.GetAfflictionStrength("cardiacarrest")= 100;
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