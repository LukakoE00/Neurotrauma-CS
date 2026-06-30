using System;

namespace Neurotrauma
{
    public static class NTC
    {
        // Symptom magic
        /// <summary>
        /// Sets the symptom to true for a certain amount of human updates.
        /// </summary>
        /// <param name="Human"> The human to have the symptom.</param>
        /// <param name="SymptomIdentifier"> The ID of the symptom.</param>
        /// <param name="Duration"> The duration of human updates the symptom should be true for.</param>
        public static void SetSymptomTrue(HumanUpdate.NTHuman Human, string SymptomIdentifier, int Duration = 2)
        {
            Dictionary<string, HumanUpdate.NTHumanSymptomData> Afflictions = Human.LocalAfflictions.UpdatingSymptoms;
            HumanUpdate.NTHumanSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateTime = Duration;
            Sym.Strength = 100;
        }

        public static void SetSymptomTrue(Character Char, string SymptomIdentifier, int Duration = 2)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            Dictionary<string, HumanUpdate.NTHumanSymptomData> Afflictions = Human.LocalAfflictions.UpdatingSymptoms;
            HumanUpdate.NTHumanSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateTime = Duration;
            Sym.Strength = 100;
        }

        /// <summary>
        /// Sets the symptom to true for a certain amount of human updates.
        /// </summary>
        /// <param name="Human"> The human to have the symptom.</param>
        /// <param name="SymptomIdentifier"> The ID of the symptom.</param>
        /// <param name="Limb"> The limb to set the symptom on.</param>
        /// <param name="Duration"> The duration of human updates the symptom should be true for.</param>
        public static void SetLimbSymptomTrue(HumanUpdate.NTHuman Human, string SymptomIdentifier, LimbType Limb, int Duration = 2)
        {
            Dictionary<string, HumanUpdate.NTHumanLimbSymptomData> Afflictions = Human.LocalAfflictions.UpdatingLimbSymptoms;
            HumanUpdate.NTHumanLimbSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateTime[Limb] = Duration;
            Sym.Strength[Limb] = 100;
        }

        public static void SetLimbSymptomTrue(Character Char, string SymptomIdentifier, LimbType Limb, int Duration = 2)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            Dictionary<string, HumanUpdate.NTHumanLimbSymptomData> Afflictions = Human.LocalAfflictions.UpdatingLimbSymptoms;
            HumanUpdate.NTHumanLimbSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateTime[Limb] = Duration;
            Sym.Strength[Limb] = 100;
        }

        /// <summary>
        /// Sets the symptom to false for a certain amount of human updates.
        /// </summary>
        /// <param name="Human"> The human to have the symptom.</param>
        /// <param name="SymptomIdentifier"> The ID of the symptom.</param>
        /// <param name="Duration"> The duration of human updates the symptom should be false for.</param>
        public static void SetSymptomFalse(HumanUpdate.NTHuman Human, string SymptomIdentifier, int Duration = 2)
        {
            Dictionary<string, HumanUpdate.NTHumanSymptomData> Afflictions = Human.LocalAfflictions.UpdatingSymptoms;
            HumanUpdate.NTHumanSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateStoptime = Duration;
            Sym.Strength = 0;
        }

        public static void SetSymptomFalse(Character Char, string SymptomIdentifier, int Duration = 2)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            Dictionary<string, HumanUpdate.NTHumanSymptomData> Afflictions = Human.LocalAfflictions.UpdatingSymptoms;
            HumanUpdate.NTHumanSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateStoptime = Duration;
            Sym.Strength = 0;
        }

        /// <summary>
        /// Sets the symptom to false for a certain amount of human updates.
        /// </summary>
        /// <param name="Human"> The human to have the symptom.</param>
        /// <param name="SymptomIdentifier"> The ID of the symptom.</param>
        /// <param name="Limb"> The limb to set the symptom on.</param>
        /// <param name="Duration"> The duration of human updates the symptom should be false for.</param>
        public static void SetLimbSymptomFalse(HumanUpdate.NTHuman Human, string SymptomIdentifier, LimbType Limb, int Duration = 2)
        {
            Dictionary<string, HumanUpdate.NTHumanLimbSymptomData> Afflictions = Human.LocalAfflictions.UpdatingLimbSymptoms;
            HumanUpdate.NTHumanLimbSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateStoptime[Limb] = Duration;
            Sym.Strength[Limb] = 0;
        }

        public static void SetLimbSymptomFalse(Character Char, string SymptomIdentifier, LimbType Limb, int Duration = 2)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            Dictionary<string, HumanUpdate.NTHumanLimbSymptomData> Afflictions = Human.LocalAfflictions.UpdatingLimbSymptoms;
            HumanUpdate.NTHumanLimbSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateStoptime[Limb] = Duration;
            Sym.Strength[Limb] = 0;
        }

        public static void DebugPrintAllData() // UNFINISHED
        {
            string Res = "Neurotrauma Compatibility Data:\n";
            foreach (KeyValuePair<Character, HumanUpdate.NTHuman> Pair in NeurotraumaInit.HU.GetUpdatingCharacters())
            {
                Character Char = Pair.Key;
                HumanUpdate.NTHuman NTHum = Pair.Value;
                Res += "\n" + Char.Name;

            }

            HF.Print(Res); // Not 1:1 with OG NT
        }

        public static void DebugPrintAllAffStrengths()
        {
            string Res = "Neurotrauma Affliction Strength Data:\n";
            foreach (KeyValuePair<Character, HumanUpdate.NTHuman> Pair in NeurotraumaInit.HU.GetUpdatingCharacters())
            {
                Character Char = Pair.Key;
                HumanUpdate.NTHuman NTHum = Pair.Value;
                Res += "-------------------------------------";
                Res += "\n\n" + Char.Name;
                Res += "\n" + "Afflictions";
                foreach (KeyValuePair<string,HumanUpdate.NTHumanNonLimbAffData> Pair2 in NTHum.LocalAfflictions.UpdatingNonLimbAfflictions)
                {
                    if (Pair2.Value.Strength > 0)
                    {
                        Res += "\n- " + Pair2.Key + ": " + Pair2.Value.Strength.ToString() + "%";
                    }
                }
                foreach (KeyValuePair<string, HumanUpdate.NTHumanLimbAffData> Pair2 in NTHum.LocalAfflictions.UpdatingLimbAfflictions)
                {
                    foreach (KeyValuePair<LimbType, double> Pair3 in Pair2.Value.Strength)
                    {
                        if (Pair3.Value > 0)
                        {
                            Res += "\n- " + Pair2.Key + ": " + Pair3.Value.ToString() + "%";
                        }
                    }
                }
                foreach (KeyValuePair<string, HumanUpdate.NTHumanBloodAffData> Pair2 in NTHum.LocalAfflictions.UpdatingBloodAfflictions)
                {
                    if (Pair2.Value.Strength > 0)
                    {
                        Res += "\n- " + Pair2.Key + ": " + Pair2.Value.Strength.ToString() + "%";
                    }
                }
                foreach (KeyValuePair<string, HumanUpdate.NTHumanSymptomData> Pair2 in NTHum.LocalAfflictions.UpdatingSymptoms)
                {
                    if (Pair2.Value.Strength > 0)
                    {
                        Res += "\n- " + Pair2.Key + ": " + Pair2.Value.Strength.ToString() + "%";
                    }
                }
            }

            HF.PrintUtility(Res); // Not 1:1 with OG NT
        }

        public static List<Action<HumanUpdate.NTHuman>> PreHumanUpdateHooks = new(); // Store our functions to call in here.

        /// <summary>
        /// Adds a mew Action<HumanUpdate.NTHuman> to the PreHumanUpdate List. Gets called for each human.
        /// </summary>
        /// <param name="Hook"></param>
        public static void AddPreHumanUpdateHook(Action<HumanUpdate.NTHuman> Hook)
        {
            PreHumanUpdateHooks.Add(Hook);
        }

        public static List<Action<HumanUpdate.NTHuman>> PostHumanUpdateHooks = new(); // Store our functions to call in here.

        /// <summary>
        /// Adds a mew Action<HumanUpdate.NTHuman> to the PostHumanUpdate List. Gets called for each human.
        /// </summary>
        /// <param name="Hook"></param>
        public static void AddPostHumanUpdateHook(Action<HumanUpdate.NTHuman> Hook)
        {
            PostHumanUpdateHooks.Add(Hook);
        }

        public static List<Action<CharacterHealth, AttackResult, Limb>> OnDamagedHooks = new();

        public static void AddOnDamagedHook(Action<CharacterHealth, AttackResult, Limb> Hook)
        {
            OnDamagedHooks.Add(Hook);
        }

        // These might work?
        public static readonly List<Action<CharacterHealth, List<Affliction>, Limb>> ModifyingOnDamagedHooks = new();

        public static void AddModifyingOnDamagedHook(Action<CharacterHealth, List<Affliction>, Limb> Hook)
        {
            ModifyingOnDamagedHooks.Add(Hook);
        }

        public static Dictionary<HumanUpdate.NTHuman, double> CharacterSpeedMultipliers = new();

        public static void MultiplySpeed(HumanUpdate.NTHuman Character, double Multiplier) // Im not gonna lie, I have no clue where this is used at.
        {
            if (CharacterSpeedMultipliers.ContainsKey(Character))
            {
                CharacterSpeedMultipliers[Character] *= Multiplier;
                return;
            }
            CharacterSpeedMultipliers[Character] = Multiplier;
        }

        public static void MultiplySpeed(Character Char, double Multiplier) // Im not gonna lie, I have no clue where this is used at.
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            if (CharacterSpeedMultipliers.ContainsKey(Human))
            {
                CharacterSpeedMultipliers[Human] *= Multiplier;
                return;
            }
            CharacterSpeedMultipliers[Human] = Multiplier;
        }

        public static void DivideSpeed(HumanUpdate.NTHuman Character, double Multiplier)
        {
            if (CharacterSpeedMultipliers.ContainsKey(Character))
            {
                CharacterSpeedMultipliers[Character] /= Multiplier;
                return;
            }
            CharacterSpeedMultipliers[Character] = Multiplier;
        }

        public static void DivideSpeed(Character Char, double Multiplier)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            if (CharacterSpeedMultipliers.ContainsKey(Human))
            {
                CharacterSpeedMultipliers[Human] /= Multiplier;
                return;
            }
            CharacterSpeedMultipliers[Human] = Multiplier;
        }

        public static double GetSpeed(HumanUpdate.NTHuman Character)
        {
            return (CharacterSpeedMultipliers.ContainsKey(Character)) ? CharacterSpeedMultipliers[Character]: 1 ; // W C# moment
        }

        public static double GetSpeed(Character Char)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            return (CharacterSpeedMultipliers.ContainsKey(Human)) ? CharacterSpeedMultipliers[Human] : 1; // W C# moment
        }

        public static void SetSpeed(HumanUpdate.NTHuman Character, double Amount)
        {
            CharacterSpeedMultipliers[Character] = Amount;
        }

        public static void SetSpeed(Character Char, double Amount)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            CharacterSpeedMultipliers[Human] = Amount;
        }

        public static void AddHematologyAffliction(string Identifier)
        {
            NTItemMethods.HematologyDetectable.Add(Identifier);
        }

        public static void AddSuturableAffliction(string Identifier, int SurgerySkillGain, string RequiredAfflictionID, Func<NTItemMethods.ItemUpdateFunctionInfos, bool> Func)
        {
            if (!NTItemMethods.SutureAfflictions.ContainsKey(Identifier))
            {
                NTItemMethods.SutureAfflictions[Identifier] = new(Identifier, SurgerySkillGain, Func, RequiredAfflictionID);
            }
        }

        public static void AddDrainageAffliction(string Identifier, int SurgerySkillGain, string RequiredAfflictionID, Func<NTItemMethods.ItemUpdateFunctionInfos, bool> Func)
        {
            if (!NTItemMethods.DrainageAfflictions.ContainsKey(Identifier))
            {
                NTItemMethods.DrainageAfflictions[Identifier] = new(Identifier, SurgerySkillGain, Func, RequiredAfflictionID);
            }
        }

        public static List<Identifier> AfflictionsAffectingVitality = ["bleeding","bleedingnonstop","burn","acidburn","opiateaddiction",
                                                                "lacerations","gunshotwound","bitewounds","explosiondamage",
                                                                "blunttrauma","internaldamage","organdamage","neurotrauma",
                                                                "gangrene","th_amputation","sh_amputation","alcoholaddiction"];

        public static void AddAfflictionAffectingVitality(string Identifier)
        {
            if (!AfflictionsAffectingVitality.Contains(Identifier))
            {
                AfflictionsAffectingVitality.Add(Identifier);
            }
        }

        public static bool HasSymptom(HumanUpdate.NTHuman Character, string SymIdentifier)
        {
            if (Character == null) return false;
            HumanUpdate.NTHumanSymptomData Symptom = Character.GetSymptomAffData(SymIdentifier);
            if (Symptom == null) return false;
            if (Symptom.Strength > 0) return true;
            return false;
        }

        public static bool HasSymptom(Character Char, string SymIdentifier)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            if (Human == null) return false;
            HumanUpdate.NTHumanSymptomData Symptom = Human.GetSymptomAffData(SymIdentifier);
            if (Symptom == null) return false;
            if (Symptom.Strength > 0) return true;
            return false;
        }

        public static bool HasLimbSymptom(HumanUpdate.NTHuman Character, string SymIdentifier, LimbType Limb)
        {
            if (Character == null) return false;
            HumanUpdate.NTHumanLimbSymptomData Symptom = Character.GetLimbSymptomData(SymIdentifier);
            if (Symptom == null) return false;
            if (Symptom.Strength[Limb] > 0) return true;
            return false;
        }

        public static bool HasLimbSymptom(Character Char, string SymIdentifier, LimbType Limb)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            if (Human == null) return false;
            HumanUpdate.NTHumanLimbSymptomData Symptom = Human.GetLimbSymptomData(SymIdentifier);
            if (Symptom == null) return false;
            if (Symptom.Strength[Limb] > 0) return true;
            return false;
        }

        public static bool HasSymptomFalse(HumanUpdate.NTHuman Character, string SymIdentifier)
        {
            if (Character == null) return false;
            HumanUpdate.NTHumanSymptomData Symptom = Character.GetSymptomAffData(SymIdentifier);
            if (Symptom == null) return false;

            if (Symptom.HumanUpdateStoptime <= 0) return true;
            return false;
        }

        public static bool HasSymptomFalse(Character Char, string SymIdentifier)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            if (Human == null) return false;
            HumanUpdate.NTHumanSymptomData Symptom = Human.GetSymptomAffData(SymIdentifier);
            if (Symptom == null) return false;

            if (Symptom.HumanUpdateStoptime <= 0) return true;
            return false;
        }

        public static bool HasLimbSymptomFalse(HumanUpdate.NTHuman Character, string SymIdentifier, LimbType Limb)
        {
            if (Character == null) return false;
            HumanUpdate.NTHumanLimbSymptomData Symptom = Character.GetLimbSymptomData(SymIdentifier);
            if (Symptom == null) return false;

            if (Symptom.HumanUpdateStoptime[Limb] <= 0) return true;
            return false;
        }

        public static bool HasLimbSymptomFalse(Character Char, string SymIdentifier, LimbType Limb)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            if (Human == null) return false;
            HumanUpdate.NTHumanLimbSymptomData Symptom = Human.GetLimbSymptomData(SymIdentifier);
            if (Symptom == null) return false;

            if (Symptom.HumanUpdateStoptime[Limb] <= 0) return true;
            return false;
        }

        public static void SetMultiplier(HumanUpdate.NTHuman Character, string MultiplierIdentifier, double Multiplier)
        {
            HumanUpdate.CharacterTags Tags = Character.GetTags();
            double CurrentMultiplier = GetMultiplier(Character, MultiplierIdentifier);
            Tags.SetTag("mult", MultiplierIdentifier, CurrentMultiplier * Multiplier);
        }

        public static void SetMultiplier(Character Char, string MultiplierIdentifier, double Multiplier)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            HumanUpdate.CharacterTags Tags = Human.GetTags();
            double CurrentMultiplier = GetMultiplier(Human, MultiplierIdentifier);
            Tags.SetTag("mult", MultiplierIdentifier, CurrentMultiplier * Multiplier);
        }

        public static double GetMultiplier(HumanUpdate.NTHuman Character, string MultiplierIdentifier)
        {
            HumanUpdate.CharacterTags Tags = Character.GetTags();
            if (!Tags.HasTag("mult", MultiplierIdentifier)) return 1;
            return Tags.GetTag("mult", MultiplierIdentifier);
        }

        public static double GetMultiplier(Character Char, string MultiplierIdentifier)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            HumanUpdate.CharacterTags Tags = Human.GetTags();
            if (!Tags.HasTag("mult", MultiplierIdentifier)) return 1;
            return Tags.GetTag("mult", MultiplierIdentifier);
        }

        public static void SetTag(HumanUpdate.NTHuman Character, string TagIdentifier)
        {
            Character.GetTags().SetTag("tag", TagIdentifier);
        }

        public static void SetTag(Character Char, string TagIdentifier)
        {
            HumanUpdate.NTHuman Human = HumanUpdate.CharacterToNTHuman(Char);
            Human.GetTags().SetTag("tag", TagIdentifier);
        }

        public static bool HasTag(HumanUpdate.NTHuman Character, string TagIdentifier)
        {
            return Character.GetTags().HasTag("tag",TagIdentifier);
        }

        public static void TickCharacterTags(HumanUpdate.NTHuman Character) // Previously "TickCharacter", however due to changes with code this is a different function now.
        {
            List<string> TagsToRemove = new();
            foreach (KeyValuePair<string,double> Pair in Character.GetTags().Tags)
            {
                string Tag = Pair.Key;
                if (Tag.StartsWith("mult"))
                {
                    TagsToRemove.Add(Tag);
                }
            }
            foreach (string Tag in TagsToRemove)
            {
                Character.GetTags().Tags.Remove(Tag);
            }
        }
    }

}