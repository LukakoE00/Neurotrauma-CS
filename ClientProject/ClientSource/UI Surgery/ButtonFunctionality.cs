using Barotrauma;
using Barotrauma.Networking;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;

namespace Neurotrauma.ClientSource
{
    internal class ButtonsHUI
    {
        private static Harmony? Harmony;
        private static bool IsInitialized;

        private static GUIFrame? Frame;
        private static GUILayoutGroup? ButtonContainer;

        private static readonly List<ButtonData> RegisteredButtons = new List<ButtonData>();
        private static readonly Dictionary<string, GUIButton> Buttons = new Dictionary<string, GUIButton>();

        public static void InitClient()
        {
            Harmony = new Harmony("NTButtonsHUI");

            var initMethod = AccessTools.Method(typeof(CharacterHealth), "InitProjSpecific", new[] { typeof(ContentXElement), typeof(Character) });
            Harmony.Patch(initMethod, postfix: new HarmonyMethod(typeof(ButtonsHUI), nameof(OnInitProjSpecific)));

            var setter = AccessTools.PropertySetter(typeof(CharacterHealth), nameof(CharacterHealth.OpenHealthWindow));
            Harmony.Patch(setter, postfix: new HarmonyMethod(typeof(ButtonsHUI), nameof(OnOpenHealthWindowChanged)));

            var addToUpdateList = AccessTools.Method(typeof(CharacterHealth), nameof(CharacterHealth.AddToGUIUpdateList));
            Harmony.Patch(addToUpdateList, postfix: new HarmonyMethod(typeof(ButtonsHUI), nameof(OnAddToGUIUpdateList)));

            foreach (ButtonData entry in ButtonDefinitions.Entries)
            {
                AddButton(entry);
            }
        }

        static void OnInitProjSpecific(CharacterHealth __instance)
        {
            // Create a new UI element looking like the HealthUI
            Frame = new GUIFrame(new RectTransform(new Vector2(0.35f, 0.2f), GUI.Canvas, anchor: Anchor.CenterLeft), style: "GUIFrameListBox");

            var DragHandle = new GUIDragHandle(new RectTransform(Vector2.One, Frame.RectTransform, Anchor.Center), Frame.RectTransform, null)
            {
                CanBeFocused = true
            };

            var MainLayout = new GUILayoutGroup(new RectTransform(Vector2.One, Frame.RectTransform, Anchor.Center), childAnchor: Anchor.TopCenter)
            {
                CanBeFocused = false
            };

            ButtonContainer = new GUILayoutGroup(new RectTransform(Vector2.One, MainLayout.RectTransform, Anchor.TopCenter), childAnchor: Anchor.TopCenter)
            {
                CanBeFocused = false,
                Stretch = false,
                AbsoluteSpacing = 8
            };

            // Automatically format Buttons into the UI based on a list!
            RebuildButtonList();
        }

        private static void RebuildButtonList()
        {
            if (ButtonContainer == null || Frame == null) 
            {
                return; 
            }

            ButtonContainer.ClearChildren();
            Buttons.Clear();

            // Auto resizing can go straight to fucking hell
            const int MaxPerRow = 5;
            const int ButtonSize = 55;
            const int OutlineSize = 65;
            const int Padding = 16;
            const int HeaderHeight = 30;
            const int HeaderRowGap = 8;
            const int RowSpacing = 4;

            // Check how many buttons we have in total!
            int ButtonCount = RegisteredButtons.Count;

            int RowCount = (ButtonCount + MaxPerRow - 1) / MaxPerRow;
            int MaxButtonsInRow = Math.Min(ButtonCount, MaxPerRow);

            // Determine how tall / wide the UI element ought to be!
            int FrameWidth = MaxButtonsInRow * OutlineSize + Padding * 2;
            int FrameHeight = HeaderHeight + HeaderRowGap + (RowCount * OutlineSize) + ((RowCount - 1) * RowSpacing) + Padding * 2;

            Frame.RectTransform.Resize(new Point(FrameWidth, FrameHeight));

            int ContentWidth = MaxButtonsInRow * OutlineSize;

            // Add some text on top so it doesnt look naked
            var Header = new GUITextBlock(new RectTransform(new Point(ContentWidth, HeaderHeight), ButtonContainer!.RectTransform), "Surgery Tools", font: GUIStyle.LargeFont)
            {
                TextAlignment = Alignment.TopCenter,
                CanBeFocused = false
            };

            GUILayoutGroup ButtonRow = null!;

            // This should look familiar cause its literally just the same idea as the config
            for (int i = 0; i < ButtonCount; i++)
            {
                if (i % MaxPerRow == 0)
                {
                    ButtonRow = new GUILayoutGroup(new RectTransform(new Point(ContentWidth, OutlineSize), ButtonContainer.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft)
                    {
                        CanBeFocused = false
                    };
                }

                ButtonData entry = RegisteredButtons[i];

                var ButtonOutline = new GUIFrame(new RectTransform(new Point(OutlineSize, OutlineSize), ButtonRow.RectTransform), style: "GUIFrameListBox")
                {
                    CanBeFocused = false
                };

                var ToolButton = new GUIButton(new RectTransform(new Point(ButtonSize, ButtonSize), ButtonOutline.RectTransform, Anchor.Center), text: "", style: entry.StyleName)
                {
                    UserData = entry.Identifier,
                    ToolTip = entry.Tooltip,
                    CanBeFocused = true
                };

                Buttons[entry.Identifier] = ToolButton;

                ToolButton.OnClicked = (button, userdata) =>
                {
                    OnButtonPressed(entry);
                    return true;
                };
            }
        }

        // When we open the Health Menu, do the following
        static void OnOpenHealthWindowChanged(CharacterHealth __0)
        {
            if (Frame == null) { return; }

            Frame.Visible = __0 != null;
        }

        // Ensure the new UI actually exists by tying it to the HealthUI
        static void OnAddToGUIUpdateList(CharacterHealth __instance)
        {
            if (Frame == null || !Frame.Visible)
            {
                return;
            }

            if (CharacterHealth.OpenHealthWindow != __instance)
            {
                return;
            }

            ToggleButtonUsage();
            Frame.AddToGUIUpdateList();
        }

        // Check if our buttons should be usable; for instance, you should only be able to use Surgery Tools if you have a Surgery Kit on you.
        private static void ToggleButtonUsage()
        {
            foreach (ButtonData entry in RegisteredButtons)
            {
                if (!Buttons.TryGetValue(entry.Identifier, out GUIButton? button)) 
                { 
                    continue; 
                }

                // The buttons get enabled if the RequiredItemIdentifier (defined in ButtonData) is present in the character's inventory
                button.Enabled = entry.RequiredItemIdentifier == null || Character.Controlled.Inventory.AllItems.Any(item => item.Prefab.Identifier == entry.RequiredItemIdentifier);

                // Likewise, we change the Tooltip to tell the gamer which item they actually need to have on them to enable the buttons.
                button.ToolTip = button.Enabled ? RichString.Rich(entry.Tooltip.Value) : RichString.Rich(entry.DisabledTooltip.Value);
            }
        }

        // Add / Override a button!
        public static void AddButton(ButtonData entry)
        {
            RegisteredButtons.RemoveAll(button => button.Identifier == entry.Identifier);
            RegisteredButtons.Add(entry);

            RebuildButtonList();
        }

        private static void OnButtonPressed(ButtonData entry)
        {
            // Get the data we need to do treatment!
            CharacterHealth HealthData = CharacterHealth.OpenHealthWindow ?? Character.Controlled?.CharacterHealth;
            Character User = Character.Controlled;
            Character Target = HealthData.Character;
            FieldInfo TargetLimb = typeof(CharacterHealth).GetField("selectedLimbIndex", BindingFlags.Instance | BindingFlags.NonPublic);

            int selectedLimbIndex = (int)TargetLimb.GetValue(HealthData);

            if (selectedLimbIndex < 0)
            {
                return;
            }

            Identifier itemIdentifier = entry.ItemIdentifier;

            if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
            {
                return;
            }

            // SEND THAT SHIT TO BIKINI BOTTOM NOW!
            IWriteMessage msg = LuaCsSetup.Instance.Networking.Start("NT.HUIButtonTreatment");

            msg.WriteUInt16(User.ID);
            msg.WriteUInt16(Target.ID);
            msg.WriteInt32(selectedLimbIndex);
            msg.WriteString(itemIdentifier.Value);

            LuaCsSetup.Instance.Networking.Send(msg);
        }
    }
}