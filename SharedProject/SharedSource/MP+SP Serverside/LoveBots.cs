
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
        public static bool Prefix_IsValidTarget(Character target, Character healer, ref bool isValid)
        {
            if (!NTConfig.Get("NT_disableBotAlgorithms", true))
            {
                return true; // Prevent original method from executing
            }

            isValid = false;
            return false; // Don't prevent original method from executing.. very important to know!
        }
    }

    public static class SpeakAboutIssuesPatch
    {
        private static readonly string[] Afflictions =
        {
            "fracturedneck",
            "carotidarterialcut",
            "aorticrupture",
            "arterialcut",
            "vomitingblood",
            "paleskin",
            "confusion",
            "lightheadedness",
            "abdominalpain",
            "inflammation",
            "gangrene",
            "fever",
            "headache",
            "blurredvision",
            "fracturedribs",
            "fracturedskull",
            "fracturedextremity",
            "dislocation",
            "chestpain",
            "weakness",
            "sweating",
            "shortnessofbreath",
            "bloating",
            "legswelling",
            "craving",
            "palpitations"
        };

        public static void Postfix_SpeakAboutIssues(HumanAIController __instance)
        {
            Character character = __instance.Character;

            if (!HF.HasAffliction(character, "luabotomy", 1f)) return;
            ChatMessageType chatType = ChatMessage.CanUseRadio(character) ? ChatMessageType.Radio : ChatMessageType.Default;

            foreach (string identifier in Afflictions)
            {
                if (!HF.HasAffliction(character, identifier, 1f)) continue;

                string message = TextManager.Get($"npcdialogsym.{identifier}").Value;
                character.Speak(message, chatType, Rand.Range(0, 5), new Identifier($"{identifier}DialogSym"), 600f);

                break;
            }
        }
    }
}