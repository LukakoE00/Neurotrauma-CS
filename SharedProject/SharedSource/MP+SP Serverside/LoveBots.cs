
using Barotrauma.Networking;

namespace Neurotrauma
{
    public class LoveBots
    {
        private static Harmony harmony;

        public static void InitBotPatches()
        {
            harmony = new Harmony("NPCPatches");

            DisableBotRescueLogic();
            SpeakAboutNeurotraumaIssues();
        }

        public static void Dispose()
        {
            if ( harmony != null ) harmony.UnpatchSelf();
        }

        public static void DisableBotRescueLogic()
        {
            var method_IsValidTarget = AccessTools.Method(typeof(AIObjectiveRescueAll), "IsValidTarget", new[] { typeof(Character), typeof(Character), typeof(bool).MakeByRefType() });

            harmony.Patch(method_IsValidTarget, prefix: new HarmonyMethod(typeof(AIObjectiveRescueAllPatch), nameof(AIObjectiveRescueAllPatch.Prefix_IsValidTarget)));
        }

        public static void SpeakAboutNeurotraumaIssues()
        {
            var method_SpeakAboutIssues = AccessTools.Method(typeof(HumanAIController), "SpeakAboutIssues");

            harmony.Patch(method_SpeakAboutIssues, postfix: new HarmonyMethod(typeof(SpeakAboutIssuesPatch), nameof(SpeakAboutIssuesPatch.Postfix_SpeakAboutIssues)));
        }
    }

    public static class AIObjectiveRescueAllPatch
    {
        public static bool Prefix_IsValidTarget(Character target, Character character, ref bool ignoredAsMinorWounds, ref bool __result)
        {
            if (!NTConfig.Get("NT_disableBotAlgorithms", true))
            {
                return true; // Prevent original method from executing
            }

            __result = false;
            return false;        // Don't prevent original method from executing.. very important to know!
        }
    }

    public static class SpeakAboutIssuesPatch
    {
        // A list that contains the Identifier of an affliction, the strength at which a character should report them, and the amount of unique voicelines.
        private static readonly List<(string Identifier, float MinStrength, int VoiceLineCount)> Afflictions = new()
        {
            // AffIdentifier    MinStrength  #OfLines
            ("fracturedneck",        1f,        2),
            ("carotidarterialcut",   1f,        2),
            ("arterialcut",          1f,        2),
            ("vomitingblood",        1f,        3),
            ("paleskin",             1f,        2),
            ("confusion",            1f,        3),
            ("lightheadedness",      1f,        3),
            ("abdominalpain",        1f,        3),
            ("inflammation",         1f,        3),
            ("gangrene",             1f,        3),
            ("fever",                1f,        3),
            ("headache",             1f,        4),
            ("blurredvision",        1f,        3),
            ("fracturedribs",        10f,       3),
            ("fracturedskull",       10f,       3),
            ("fracturedextremity",   10f,       3),
            ("dislocation",          10f,       3),
            ("chestpain",            1f,        2),
            ("weakness",             1f,        2),
            ("sweating",             1f,        3),
            ("shortnessofbreath",    1f,        3),
            ("bloating",             1f,        2),
            ("legswelling",          1f,        2),
            ("craving",              1f,        2),
            ("palpitations",         1f,        3)
        };

        /// <summary>
        /// Registers an affliction (or updates its threshold/voiceline count if already present) for the NPC 'SpeakAboutIssues' Patch.
        /// </summary>
        /// <param name="Identifier"></param>
        /// <param name="MinStrength"></param>
        /// <param name="VoiceLineCount">How many numbered voicelines exist in the Localization File. Defaults to 1.</param>
        public static void AddAfflictionToSpeakAbout(string Identifier, float MinStrength, int VoiceLineCount = 1)
        {
            int existingIndex = Afflictions.FindIndex(a => a.Identifier == Identifier);

            // If it already exists, update the entry; else we add a new entry to the list.
            if (existingIndex >= 0)
            {
                Afflictions[existingIndex] = (Identifier, MinStrength, VoiceLineCount);
            }
            else
            {
                Afflictions.Add((Identifier, MinStrength, VoiceLineCount));
            }
        }

        public static void Postfix_SpeakAboutIssues(HumanAIController __instance)
        {
            Character character = __instance.Character;
            ChatMessageType chatType = ChatMessage.CanUseRadio(character) ? ChatMessageType.Radio : ChatMessageType.Default;

            // Go over the List above and if a character has them, force them to play a voice line on the Radio.
            foreach (var (Identifier, MinStrength, VoiceLineCount) in Afflictions)
            {
                if (!HF.HasAffliction(character, Identifier, MinStrength)) continue;

                // Randomly pick a VoiceLine from Localization, based on the 'known' amount of them.
                int VoiceLineToSay = Rand.Range(1, VoiceLineCount + 1);
                string LocalizationKey = $"npcdialogsym.{Identifier}_{VoiceLineToSay}";
                string Message = TextManager.Get(LocalizationKey).Value;

                // If the rolled VoiceLine doesnt exist in this language, default to the first one (which SHOULD!! exist).
                if (string.IsNullOrEmpty(Message) && VoiceLineToSay != 1)
                {
                    LocalizationKey = $"npcdialogsym.{Identifier}_1";
                    Message = TextManager.Get(LocalizationKey).Value;
                }

                // If the message is STILL empty then something went wrong!! Throw an error for that affliction.
                if (string.IsNullOrEmpty(Message))
                {
                    LuaCsLogger.LogError($"[NT] NPC Voiceline for Affliction {Identifier} was not found! Possibly empty localization?");
                    continue;
                }

                // Play the selected message on the Radio.
                float IntervalBetweenSameMessage = 600f;
                int DelayBeforeSendingMessage = Rand.Range(0, 5);

                character.Speak(Message, chatType, DelayBeforeSendingMessage, new Identifier($"{Identifier}DialogSym"), IntervalBetweenSameMessage);

                break;
            }
        }
    }
}