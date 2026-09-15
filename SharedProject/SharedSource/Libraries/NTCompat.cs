using System;

namespace Neurotrauma
{
    /// <summary>
    /// A library used by NT and C# Addons.
    /// Provides methods to interact with NT features such as Symptoms, Tags, Debugging, Human Update systems and more.
    /// </summary>
    public static class NTC
    {

        public static void DebugPrintAllData() // UNFINISHED
        {
            string Res = "Neurotrauma Compatibility Data:\n";
            foreach (KeyValuePair<Character, NTHuman> Pair in NTHuman.NTHumans)
            {
                Character Char = Pair.Key;
                NTHuman NTHum = Pair.Value;
                Res += "\n" + Char.Name;

            }

            HF.Print(Res); // Not 1:1 with OG NT
        }

        
        public static void DebugPrintAllAffStrengths()
        {
            string Res = "Neurotrauma Affliction Strength Data:\n";
            foreach (KeyValuePair<Character, NTHuman> Pair in NTHuman.NTHumans)
            {
                Character Char = Pair.Key;
                NTHuman NTHum = Pair.Value;
                Res += "-------------------------------------";
                Res += "\n\n" + Char.Name;
                Res += "\n" + "Afflictions";
                /*foreach (KeyValuePair<string,NTHumanNonLimbAffData> Pair2 in NTHum.LocalAfflictions.UpdatingNonLimbAfflictions)
                {
                    if (Pair2.Value.Strength > 0)
                    {
                        Res += "\n- " + Pair2.Key + ": " + Pair2.Value.Strength.ToString() + "%";
                    }
                }
                foreach (KeyValuePair<string, NTHumanLimbAffData> Pair2 in NTHum.LocalAfflictions.UpdatingLimbAfflictions)
                {
                    foreach (KeyValuePair<LimbType, double> Pair3 in Pair2.Value.Strength)
                    {
                        if (Pair3.Value > 0)
                        {
                            Res += "\n- " + Pair2.Key + ": " + Pair3.Value.ToString() + "%";
                        }
                    }
                }
                foreach (KeyValuePair<string, NTHumanBloodAffData> Pair2 in NTHum.LocalAfflictions.UpdatingBloodAfflictions)
                {
                    if (Pair2.Value.Strength > 0)
                    {
                        Res += "\n- " + Pair2.Key + ": " + Pair2.Value.Strength.ToString() + "%";
                    }
                }
                foreach (KeyValuePair<string, NTHumanSymptomData> Pair2 in NTHum.LocalAfflictions.UpdatingSymptoms)
                {
                    if (Pair2.Value.Strength > 0)
                    {
                        Res += "\n- " + Pair2.Key + ": " + Pair2.Value.Strength.ToString() + "%";
                    }
                }*/
            }

            HF.PrintUtility(Res); // Not 1:1 with OG NT
        }

        public static List<Action<CharacterHealth, AttackResult, Limb>> OnDamagedHooks = new();

        public static void AddOnDamagedHook(Action<CharacterHealth, AttackResult, Limb> Hook)
        {
            OnDamagedHooks.Add(Hook);
        }

        public static void AddOnDamagedHook(LuaCsAction Hook)
        {
            Action<CharacterHealth, AttackResult, Limb> TranslatedHook = (H,Res,L) => 
            { 
                Hook.Invoke(H,Res,L);
            };
            OnDamagedHooks.Add(TranslatedHook);
        }

        // These might work?
        public static readonly List<Action<CharacterHealth, List<Affliction>, Limb>> ModifyingOnDamagedHooks = new();

        public static void AddModifyingOnDamagedHook(Action<CharacterHealth, List<Affliction>, Limb> Hook)
        {
            ModifyingOnDamagedHooks.Add(Hook);
        }

        public static void AddModifyingOnDamagedHook(LuaCsAction Hook)
        {
            Action<CharacterHealth, List<Affliction>, Limb> TranslatedHook = (H, Affs, L) =>
            {
                Hook.Invoke(H, Affs, L);
            };
            ModifyingOnDamagedHooks.Add(TranslatedHook);
        }

        public static Dictionary<NTHuman, double> CharacterSpeedMultipliers = new();

        public static void MultiplySpeed(NTHuman Character, double Multiplier) // Im not gonna lie, I have no clue where this is used at.
        {
            if (Character == null) return;
            if (CharacterSpeedMultipliers.ContainsKey(Character))
            {
                CharacterSpeedMultipliers[Character] *= Multiplier;
                return;
            }
            CharacterSpeedMultipliers[Character] = Multiplier;
        }

        public static void MultiplySpeed(Character Char, double Multiplier) // Im not gonna lie, I have no clue where this is used at.
        {
            if (Char == null) return;
            NTHuman? Human = NTHuman.getNTHumanFromCharacter(Char);

            if (Human == null)
            {
                HF.PrintError($"Trying to multiply speed from an unknown NTHuman : {Char.Name}");
                return;
            }

            if (CharacterSpeedMultipliers.ContainsKey(Human))
            {
                CharacterSpeedMultipliers[Human] *= Multiplier;
                return;
            }
            CharacterSpeedMultipliers[Human] = Multiplier;
        }

        public static void DivideSpeed(NTHuman Character, double Multiplier)
        {
            if (Character == null) return;
            if (CharacterSpeedMultipliers.ContainsKey(Character))
            {
                CharacterSpeedMultipliers[Character] /= Multiplier;
                return;
            }
            CharacterSpeedMultipliers[Character] = Multiplier;
        }

        public static void DivideSpeed(Character Char, double Multiplier)
        {
            if (Char == null) return;
            NTHuman ?Human = NTHuman.getNTHumanFromCharacter(Char);

            if (Human == null)
            {
                HF.PrintError($"Trying to divide speed from an unknown NTHuman : {Char.Name}");
                return;
            }

            if (CharacterSpeedMultipliers.ContainsKey(Human))
            {
                CharacterSpeedMultipliers[Human] /= Multiplier;
                return;
            }
            CharacterSpeedMultipliers[Human] = Multiplier;
        }

        public static double GetSpeed(NTHuman Character)
        {
            if (Character == null) return 1;
            return (CharacterSpeedMultipliers.ContainsKey(Character)) ? CharacterSpeedMultipliers[Character]: 1 ; // W C# moment
        }

        public static double GetSpeed(Character Char)
        {
            if (Char == null) return 1;
            NTHuman ?Human = NTHuman.getNTHumanFromCharacter(Char);

            if (Human == null)
            {
                HF.PrintError($"Trying to get speed from an unknwon NTHuman : {Char.Name}");
                return 1;
            }
            

            return (CharacterSpeedMultipliers.ContainsKey(Human)) ? CharacterSpeedMultipliers[Human] : 1; // W C# moment
        }

        public static void SetSpeed(NTHuman Character, double Amount)
        {
            if (Character == null) return;
            CharacterSpeedMultipliers[Character] = Amount;
        }

        public static void SetSpeed(Character Char, double Amount)
        {
            if (Char == null) return;
            NTHuman ?Human = NTHuman.getNTHumanFromCharacter(Char);

            if (Human == null)
            {
                HF.PrintError($"Trying to set speed from an unknown NTHuman : {Char.Name}");
                return;
            }

            CharacterSpeedMultipliers[Human] = Amount;
        }

        public static void AddHematologyAffliction(string Identifier)
        {
            NTItems.HematologyDetectable.Add(Identifier);
        }

        public static void AddSuturableAffliction(string Identifier, int SurgerySkillGain, string RequiredAfflictionID, Func<NTItems.ItemUpdateFunctionInfos, bool> Func)
        {
            if (!NTItems.SutureAfflictions.ContainsKey(Identifier))
            {
                NTItems.SutureAfflictions[Identifier] = new(Identifier, SurgerySkillGain, Func, RequiredAfflictionID);
            }
        }

        public static void AddSuturableAffliction(string Identifier, int SurgerySkillGain, string RequiredAfflictionID)
        {
            if (!NTItems.SutureAfflictions.ContainsKey(Identifier))
            {
                Func<NTItems.ItemUpdateFunctionInfos, bool> TranslatedFunc = (info) =>
                {
                    return info.target.HasAfflictionLimb(RequiredAfflictionID, LimbType.Torso, 1);
                };
                NTItems.SutureAfflictions[Identifier] = new(Identifier, SurgerySkillGain, TranslatedFunc, RequiredAfflictionID);
            }
        }

        public static void AddDrainageAffliction(string Identifier, int SurgerySkillGain, string RequiredAfflictionID, Func<NTItems.ItemUpdateFunctionInfos, bool> Func)
        {
            if (!NTItems.DrainageAfflictions.ContainsKey(Identifier))
            {
                NTItems.DrainageAfflictions[Identifier] = new(Identifier, SurgerySkillGain, Func, RequiredAfflictionID);
            }
        }

        public static void AddDrainageAffliction(string Identifier, int SurgerySkillGain, string RequiredAfflictionID)
        {
            if (!NTItems.DrainageAfflictions.ContainsKey(Identifier))
            {
                Func<NTItems.ItemUpdateFunctionInfos, bool> TranslatedFunc = (info) =>
                {
                    return info.target.HasAfflictionLimb("retractedskin", LimbType.Torso, 95);
                };
                NTItems.DrainageAfflictions[Identifier] = new(Identifier, SurgerySkillGain, TranslatedFunc, RequiredAfflictionID);
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

        public static void SetMultiplier(NTHuman ?Character, string MultiplierIdentifier, double Multiplier)
        {
            if (Character == null) return;
            NTHuman.CharacterTags Tags = Character.Tags;
            double CurrentMultiplier = GetMultiplier(Character, MultiplierIdentifier);
            Tags.SetTag("mult", MultiplierIdentifier, CurrentMultiplier * Multiplier);
        }

        public static void SetMultiplier(Character ?Char, string MultiplierIdentifier, double Multiplier)
        {
            if (Char == null) return;
            NTHuman ?Human = NTHuman.getNTHumanFromCharacter(Char);

            if (Human == null)
            {
                HF.PrintError($"Trying to set the multiplier {MultiplierIdentifier} from an unknwon NTHuman : {Char.Name}");
                return;
            }
            

            NTHuman.CharacterTags Tags = Human.Tags;
            double CurrentMultiplier = GetMultiplier(Human, MultiplierIdentifier);
            Tags.SetTag("mult", MultiplierIdentifier, CurrentMultiplier * Multiplier);
        }

        public static double GetMultiplier(NTHuman ?Character, string MultiplierIdentifier)
        {
            if (Character == null) return 1;
            NTHuman.CharacterTags Tags = Character.Tags;
            if (!Tags.HasTag("mult", MultiplierIdentifier)) return 1;
            return Tags.GetTag("mult", MultiplierIdentifier);
        }

        public static double GetMultiplier(Character ?Char, string MultiplierIdentifier)
        {
            if (Char == null) return 1;
            NTHuman ?Human = NTHuman.getNTHumanFromCharacter(Char);

            if (Human == null) {
                HF.PrintError($"Trying to get the multiplier {MultiplierIdentifier} from an unknwon NTHuman : {Char.Name}");
                return 1;
            }
            

            NTHuman.CharacterTags Tags = Human.Tags;
            if (!Tags.HasTag("mult", MultiplierIdentifier)) return 1;
            return Tags.GetTag("mult", MultiplierIdentifier);
        }

        public static void SetTag(NTHuman ?Character, string TagIdentifier)
        {
            if (Character == null) return;
            Character.Tags.SetTag("tag", TagIdentifier);
        }

        public static void SetTag(Character ?Char, string TagIdentifier)
        {
            if (Char == null) return;
            NTHuman ?Human = NTHuman.getNTHumanFromCharacter(Char);

            if (Human == null)
            {
                HF.PrintError($"Trying to set tag {TagIdentifier} from an unknwon NTHuman : {Char.Name}");
                return;
            }

            Human.Tags.SetTag("tag", TagIdentifier);
        }

        public static bool HasTag(NTHuman ?Character, string TagIdentifier)
        {
            if (Character == null) return false;
            return Character.Tags.HasTag("tag",TagIdentifier);
        }

        public static void TickCharacterTags(NTHuman ?Character) // Previously "TickCharacter", however due to changes with code this is a different function now.
        {
            if (Character == null) return;
            List<string> TagsToRemove = new();
            foreach (KeyValuePair<string,double> Pair in Character.Tags.Tags)
            {
                string Tag = Pair.Key;
                if (Tag.StartsWith("mult"))
                {
                    TagsToRemove.Add(Tag);
                }
            }
            foreach (string Tag in TagsToRemove)
            {
                Character.Tags.Tags.Remove(Tag);
            }
        }
    }

}