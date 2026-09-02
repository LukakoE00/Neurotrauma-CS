using Barotrauma.Networking;

namespace Neurotrauma.ClientSource
{
    internal class ButtonsHUI
    {
        private static Harmony? Harmony;

        // Main UI
        private static GUIFrame? Frame;
        private static GUILayoutGroup? ButtonContainer;
        private static GUIDragHandle? DragHandle;
        private static readonly List<ButtonData> RegisteredButtons = new List<ButtonData>();
        private static readonly Dictionary<string, GUIButton> Buttons = new Dictionary<string, GUIButton>();

        // Context Menu (SubUI)
        private static GUIFrame? ContextMenu;
        private static GUILayoutGroup? ContextMenuContainer;
        private static ButtonData? ContextMenuOwner;
        private static GUIButton? ContextMenuAnchor;
        private static readonly Dictionary<string, GUIButton> ContextMenuButtons = new Dictionary<string, GUIButton>();

        private static readonly FieldInfo SelectedLimbIndexField = typeof(CharacterHealth).GetField("selectedLimbIndex", BindingFlags.Instance | BindingFlags.NonPublic);

        // GO MY PATCHES!!!!!!!!!!!!!!
        public static void InitClient()
        {
            Harmony = new Harmony("NTButtonsHUI");

            var InitMethod = AccessTools.Method(typeof(CharacterHealth), "InitProjSpecific", new[] { typeof(ContentXElement), typeof(Character) });
            Harmony.Patch(InitMethod, postfix: new HarmonyMethod(typeof(ButtonsHUI), nameof(OnInitProjSpecific)));

            var Setter = AccessTools.PropertySetter(typeof(CharacterHealth), nameof(CharacterHealth.OpenHealthWindow));
            Harmony.Patch(Setter, postfix: new HarmonyMethod(typeof(ButtonsHUI), nameof(OnOpenHealthWindowChanged)));

            var AddToUpdateList = AccessTools.Method(typeof(CharacterHealth), nameof(CharacterHealth.AddToGUIUpdateList));
            Harmony.Patch(AddToUpdateList, postfix: new HarmonyMethod(typeof(ButtonsHUI), nameof(OnAddToGUIUpdateList)));

            foreach (ButtonData Entry in ButtonDefinitions.Entries)
            {
                AddButton(Entry);
            }
        }

        // Create the basic UI element that holds our buttons
        static void OnInitProjSpecific(CharacterHealth __instance)
        {
            Frame = new GUIFrame(new RectTransform(new Vector2(0.35f, 0.2f), GUI.Canvas, Anchor.CenterLeft), style: "GUIFrameListBox");

            DragHandle = new GUIDragHandle(new RectTransform(Vector2.One, Frame.RectTransform, Anchor.Center), Frame.RectTransform, null)
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

            BuildMainButtonList();
        }

        // When we open the Health Menu, do the following
        static void OnOpenHealthWindowChanged(CharacterHealth __0)
        {
            if (Frame == null)
            {
                return;
            }

            Frame.Visible = __0 != null;

            if (!Frame.Visible)
            {
                CloseContextMenu();
            }
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

            if (ContextMenu != null)
            {
                ToggleContextMenuButtonUsage();

                // Keep the sub-UI attached to its anchor button every frame.. gotta keep it modular
                if (ContextMenuAnchor != null)
                {
                    Point CurrentPos = new Point(ContextMenuAnchor.Rect.X, ContextMenuAnchor.Rect.Bottom + 4);
                    ContextMenu.RectTransform.AbsoluteOffset = CurrentPos;
                }

                ContextMenu.AddToGUIUpdateList();
            }
        }

        // Make the UI element that holds the tool buttons and automatically populate it with info from ButtonData
        static void BuildMainButtonList()
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

            int ButtonCount = RegisteredButtons.Count;

            Point FrameSize = CalculateGridSize(ButtonCount, MaxPerRow, OutlineSize, Padding, RowSpacing, HeaderHeight + HeaderRowGap);
            Frame.RectTransform.Resize(FrameSize);

            int ContentWidth = Math.Min(ButtonCount, MaxPerRow) * OutlineSize;

            // Add some text on top so it doesnt look naked
            var Header = new GUITextBlock(new RectTransform(new Point(ContentWidth, HeaderHeight), ButtonContainer!.RectTransform), "Surgery Tools", font: GUIStyle.LargeFont)
            {
                TextAlignment = Alignment.TopCenter,
                CanBeFocused = false
            };

            PopulateButtonGrid(ButtonContainer, RegisteredButtons, Buttons, MaxPerRow, ButtonSize, OutlineSize, (Entry, Button) =>
            {
                if (Entry.SubButtons != null && Entry.SubButtons.Count > 0)
                {
                    if (ContextMenuOwner == Entry)
                    {
                        CloseContextMenu();
                    }
                    else
                    {
                        BuildContextButtonList(Entry, Button);
                    }
                }
                else
                {
                    OnButtonPressed(Entry);
                }
            });
        }

        // This can DEFINITELY be done better. Too bad!
        static void BuildContextButtonList(ButtonData ParentEntry, GUIButton AnchorButton)
        {
            CloseContextMenu();

            if (ParentEntry.SubButtons == null || ParentEntry.SubButtons.Count == 0)
            {
                return;
            }

            ContextMenuOwner = ParentEntry;
            ContextMenuAnchor = AnchorButton;

            // Can still go to hell
            const int MaxPerRow = 5;
            const int ButtonSize = 40;
            const int OutlineSize = 48;
            const int Padding = 10;
            const int RowSpacing = 4;

            Point MenuSize = CalculateGridSize(ParentEntry.SubButtons.Count, MaxPerRow, OutlineSize, Padding, RowSpacing);
            Point MenuPos = new Point(AnchorButton.Rect.X, AnchorButton.Rect.Bottom + 4);

            var MenuRT = new RectTransform(MenuSize, GUI.Canvas, anchor: Anchor.TopLeft)
            {
                AbsoluteOffset = MenuPos
            };

            ContextMenu = new GUIFrame(MenuRT, style: "GUIFrameListBox")
            {
                CanBeFocused = true
            };

            ContextMenuContainer = new GUILayoutGroup(new RectTransform(new Point(MenuSize.X - Padding * 2, MenuSize.Y - Padding * 2), ContextMenu.RectTransform, Anchor.Center), childAnchor: Anchor.TopCenter)
            {
                CanBeFocused = false,
                Stretch = false,
                AbsoluteSpacing = RowSpacing
            };

            PopulateButtonGrid(ContextMenuContainer, ParentEntry.SubButtons, ContextMenuButtons, MaxPerRow, ButtonSize, OutlineSize, (SubEntry, Button) =>
            {
                OnButtonPressed(SubEntry);
            });
        }

        static void OnButtonPressed(ButtonData Entry)
        {
            // Get the data we need to do treatment!
            CharacterHealth HealthData = CharacterHealth.OpenHealthWindow ?? Character.Controlled?.CharacterHealth;
            Character User = Character.Controlled;
            Character Target = HealthData.Character;

            int SelectedLimbIndex = (int)SelectedLimbIndexField.GetValue(HealthData);

            if (SelectedLimbIndex < 0)
            {
                return;
            }

            Identifier ItemIdentifier = Entry.ItemIdentifier;

            if (GameMain.NetworkMember == null)
            {
                ApplyTreatmentLocally(User, Target, SelectedLimbIndex, ItemIdentifier);
                return;
            }

            if (!GameMain.NetworkMember.IsClient)
            {
                return;
            }

            // SEND THAT SHIT TO BIKINI BOTTOM NOW!
            IWriteMessage Msg = LuaCsSetup.Instance.Networking.Start("NT.HUIButtonTreatment");

            Msg.WriteUInt16(User.ID);
            Msg.WriteUInt16(Target.ID);
            Msg.WriteInt32(SelectedLimbIndex);
            Msg.WriteString(ItemIdentifier.Value);

            LuaCsSetup.Instance.Networking.Send(Msg);
        }

        // If we press a button in MP, we send the information to the server. In SP, we can just run that shit
        private static void ApplyTreatmentLocally(Character User, Character Target, int SelectedLimbIndex, Identifier ItemIdentifier)
        {
            Limb? TargetLimb = Target.AnimController.Limbs.FirstOrDefault(Limb => Limb.HealthIndex == SelectedLimbIndex);
            if (TargetLimb == null)
            {
                return;
            }

            if (!NTItemMethods.NTItemsRegistry.TryGetValue(ItemIdentifier.Value, out var ItemFunction))
            {
                return;
            }

            ItemPrefab Prefab = ItemPrefab.FindByIdentifier(ItemIdentifier) as ItemPrefab;

            var Infos = new NTItemMethods.ItemUpdateFunctionInfos(Prefab, User, Target, TargetLimb);
            ItemFunction(Infos);
        }

        private static void PopulateButtonGrid(GUIComponent Container, IReadOnlyList<ButtonData> Entries, Dictionary<string, GUIButton> TargetDict, int MaxPerRow, int ButtonSize, int OutlineSize, Action<ButtonData, GUIButton> OnClicked)
        {
            int ContentWidth = Math.Min(Entries.Count, MaxPerRow) * OutlineSize;
            GUILayoutGroup Row = null!;

            for (int i = 0; i < Entries.Count; i++)
            {
                if (i % MaxPerRow == 0)
                {
                    Row = new GUILayoutGroup(new RectTransform(new Point(ContentWidth, OutlineSize), Container.RectTransform), isHorizontal: true, childAnchor: Anchor.CenterLeft)
                    {
                        CanBeFocused = false
                    };
                }

                ButtonData Entry = Entries[i];

                var Outline = new GUIFrame(new RectTransform(new Point(OutlineSize, OutlineSize), Row.RectTransform), style: "GUIFrameListBox")
                {
                    CanBeFocused = false
                };

                var Button = new GUIButton(new RectTransform(new Point(ButtonSize, ButtonSize), Outline.RectTransform, Anchor.Center), text: "", style: Entry.StyleName)
                {
                    UserData = Entry.Identifier,
                    ToolTip = Entry.Tooltip,
                    CanBeFocused = true
                };

                TargetDict[Entry.Identifier] = Button;

                Button.OnClicked = (Button, Userdata) =>
                {
                    OnClicked(Entry, Button);
                    return true;
                };
            }
        }

        // USEFUL FUNCTIONS!!!!!!

        // Add / Override a button!
        static void AddButton(ButtonData Entry)
        {
            RegisteredButtons.RemoveAll(Button => Button.Identifier == Entry.Identifier);
            RegisteredButtons.Add(Entry);

            BuildMainButtonList();
        }

        // Sub-buttons also require the correct limb to be selected, in addition to the item check!
        static void ToggleContextMenuButtonUsage()
        {
            if (ContextMenuOwner?.SubButtons == null)
            {
                return;
            }

            LimbType? SelectedLimb = GetSelectedLimbType();

            foreach (ButtonData SubEntry in ContextMenuOwner.SubButtons)
            {
                if (!ContextMenuButtons.TryGetValue(SubEntry.Identifier, out GUIButton? Button))
                {
                    continue;
                }

                bool HasItem = HasRequiredItem(SubEntry.RequiredItemIdentifier);
                bool LimbMatches = SubEntry.RequiredLimb == null || SelectedLimb == SubEntry.RequiredLimb;

                Button.Enabled = HasItem && LimbMatches;
                Button.ToolTip = Button.Enabled ? RichString.Rich(SubEntry.Tooltip.Value) : RichString.Rich(SubEntry.DisabledTooltip.Value);
            }
        }

        // HF to see if the item we're looking for is present in a player inventory
        private static bool HasRequiredItem(Identifier? RequiredItemIdentifier)
        {
            return RequiredItemIdentifier == null || Character.Controlled.Inventory.AllItems.Any(Item => Item.Prefab.Identifier == RequiredItemIdentifier);
        }

        // HF to check if our buttons should be usable; for instance, you should only be able to use Surgery Tools if you have a Surgery Kit on you.
        static void ToggleButtonUsage()
        {
            foreach (ButtonData Entry in RegisteredButtons)
            {
                if (!Buttons.TryGetValue(Entry.Identifier, out GUIButton? Button))
                {
                    continue;
                }

                Button.Enabled = HasRequiredItem(Entry.RequiredItemIdentifier);
                Button.ToolTip = Button.Enabled ? RichString.Rich(Entry.Tooltip.Value) : RichString.Rich(Entry.DisabledTooltip.Value);
            }
        }

        // HF to determine which limb is currently selected in the HealthUI
        static LimbType? GetSelectedLimbType()
        {
            CharacterHealth? HealthData = CharacterHealth.OpenHealthWindow;
            if (HealthData?.Character == null)
            {
                return null;
            }

            int SelectedLimbIndex = (int)SelectedLimbIndexField.GetValue(HealthData);

            if (SelectedLimbIndex < 0)
            {
                return null;
            }

            Limb? MatchingLimb = HealthData.Character.AnimController.Limbs.FirstOrDefault(Limb => Limb.HealthIndex == SelectedLimbIndex);

            return MatchingLimb?.type;
        }

        // Because I have no idea if auto resizing is a thing, calculate it based on the amount of buttons (so that if addons add their own its not a problem) and force resize the button holder UI
        private static Point CalculateGridSize(int ItemCount, int MaxPerRow, int OutlineSize, int Padding, int RowSpacing, int ExtraHeight = 0)
        {
            int RowCount = (ItemCount + MaxPerRow - 1) / MaxPerRow;
            int MaxButtonsInRow = Math.Min(ItemCount, MaxPerRow);

            int Width = MaxButtonsInRow * OutlineSize + Padding * 2;
            int Height = ExtraHeight + (RowCount * OutlineSize) + ((RowCount - 1) * RowSpacing) + Padding * 2;

            return new Point(Width, Height);
        }

        // HF to obliterate an expanded 'context' menu
        static void CloseContextMenu()
        {
            ContextMenu = null;
            ContextMenuContainer = null;
            ContextMenuOwner = null;
            ContextMenuAnchor = null;
            ContextMenuButtons.Clear();
        }

        // Remove the UI elements on exiting so it doesn't infinitely duplicate
        public static void RemoveNTButtons()
        {
            if (Frame != null)
            {
                Frame.RectTransform.Parent = null;
                Frame = null;
            }

            Harmony?.UnpatchSelf();
            Harmony = null;
        }
    }
}