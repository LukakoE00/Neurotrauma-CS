using Barotrauma.LuaCs.Compatibility;
using Barotrauma.Networking;
using Neurotrauma.ClientSource;

namespace Neurotrauma
{
    internal class ConfigurationMenu
    {
        private static Harmony? Harmony;
        public static GUIButton? NTPauseMenuButton;
        public static GUIListBox? PageListBox;
        public static GUIFrame? BaseFrame;
        public static GUITextBox? ConfigFileNameTextBox;

        public static String? SelectedExpansion;
        public static string? SelectedType = null;

        public static readonly List<(string UIName, string Identifier, string Type)> Pages = new();
        public static readonly List<(LocalizedString Name, string Identifier)> BaseConfigPages = new()
        {
            (TextManager.Get("ntconfig_pagename_prices"), "prices"),
            (TextManager.Get("ntconfig_pagename_availability"), "availability")
        };

        public class LayoutChunk
        {
            public string? Type;
            public string? Key;
            public ConfigEntry? Entry;
            public List<(string key, ConfigEntry entry)>? Items;
        }

        // Initialize the Harmony Patches so the Config UI can be used.
        public static void InitNTConfig()
        {
            Harmony = new Harmony("NTConfigButton");

            var InitMethod = AccessTools.Method(typeof(Barotrauma.GUI), "TogglePauseMenu");
            Harmony.Patch(InitMethod, postfix: new HarmonyMethod(typeof(ConfigurationMenu), nameof(AddConfigButtonToPauseMenu)));
        }

        // Add a button to the Pause Menu to open the Config UI.
        public static void AddConfigButtonToPauseMenu()
        {
            if (!GUI.PauseMenuOpen)
            {
                return;
            }

            GUIComponent PauseMenu = GUI.PauseMenu; // The actual menu screen
            GUIComponent PauseMenuPanel = PauseMenu.Children.Skip(1).First(); // The panel containing the elements
            GUIComponent PauseMenuButtons = PauseMenuPanel.Children.First(); // The buttons!

            NTPauseMenuButton = new GUIButton(new RectTransform(new Vector2(1f, 0.1f), PauseMenuButtons.RectTransform), TextManager.Get("ntgui_pausemenubutton_name"), textAlignment: Alignment.Center, style: "GUIButtonSmall");

            NTPauseMenuButton.OnClicked = (_, _) =>
            {
                CreateConfigGUI(PauseMenu);
                return true;
            };
        }

        // Remove the button during Disposal so it doesn't duplicate + unpatch this harmony instance.
        public static void RemoveConfigButtonFromPauseMenu()
        {
            if (NTPauseMenuButton != null)
            {
                NTPauseMenuButton.RectTransform.Parent = null;
                NTPauseMenuButton = null;
            }

            Harmony?.UnpatchSelf();
            Harmony = null;
        }

        // Create the main Config UI.
        public static GUIListBox CreateConfigGUI(GUIComponent Parent)
        {
            // Sync the config settings on opening.
            if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
            {
                IWriteMessage ConfigRequest = LuaCsSetup.Instance.Networking.Start("NT.ConfigRequest");
                LuaCsSetup.Instance.Networking.Send(ConfigRequest);
            }

            // Frame 75% / 80% the size of the screen that will hold the UI.
            // This is the green, see-through background used in many UI elements.
            BaseFrame = new GUIFrame(new RectTransform(new Vector2(0.75f, 0.8f), parent: Parent.RectTransform, anchor: Anchor.Center));

            // LayoutGroup 95% the size of the BaseFrame
            // This holds all the content via the InnerFrame + the 3 buttons at the bottom of the page.
            var BaseLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), parent: BaseFrame.RectTransform, anchor: Anchor.Center));

            // Frame 95% the size of the BaseLayoutGroup; this gives us a small border.
            // This is a black, see-through background - the config isn't perfectly opaque but this looks better in my opinion.
            var InnerFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.95f), parent: BaseLayoutGroup.RectTransform), style: "InnerFrame");

            // LayoutGroup 95% the size of the InnerFrame.
            // Used to determine the layout of the other elements below it so they don't overlap.
            var InnerLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), parent: InnerFrame.RectTransform, anchor: Anchor.TopCenter));

            // LayoutGroup 100% / 9% the size of the InnerLayoutGroup.
            // This block at the top of the UI holds the Title + the Config Preset UI elements.
            var TopsideLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.09f), parent: InnerLayoutGroup.RectTransform), isHorizontal: true);

            // Textblock 70% / 100% the size of the TopsideLayoutGroup.
            // Holds the Neurotrauma title text centered at the top left-ish.
            var TitleTextBlock = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1f), parent: TopsideLayoutGroup.RectTransform), text: TextManager.Get("ntgui_config_title"), font: GUIStyle.LargeFont, textAlignment: Alignment.TopCenter);

            // LayoutGroup 30% / 100% the size of the TopsideLayoutGroup.
            // This holds the following three elements needed to get the Config Presets to work.
            var ConfigPresetLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 1f), parent: TopsideLayoutGroup.RectTransform), isHorizontal: true)
            {
                RelativeSpacing = 0.01f
            };

            // Button 10% / 45% the size of the ConfigPresetLayoutGroup.
            // Opens a popup that can create new Config Presets.
            var SavePresetButton = new GUIButton(new RectTransform(new Vector2(0.1f, 0.45f), parent: ConfigPresetLayoutGroup.RectTransform, anchor: Anchor.CenterLeft), style: "SaveButton")
            {
                OnClicked = (_, _) =>
                {
                    ConfigPresets.OpenSaveConfigPreset();
                    return false;
                },
                ToolTip = TextManager.Get("ntgui_config_tooltip_savepresetbutton")
            };

            // Button 10% / 45% the size of the ConfigPresetLayoutGroup.
            // Opens a popup that can load existing Config Presets.
            var LoadPresetButton = new GUIButton(new RectTransform(new Vector2(0.1f, 0.45f), parent: ConfigPresetLayoutGroup.RectTransform, anchor: Anchor.CenterLeft), style: "OpenButton")
            {
                OnClicked = (_, _) =>
                {
                    ConfigPresets.OpenLoadConfigPreset();
                    return false;
                },
                ToolTip = TextManager.Get("ntgui_config_tooltip_loadpresetbutton")
            };

            // TextBox 80% / 50% the size of the ConfigPresetLayoutGroup.
            // This holds the name of the currently selected Config Preset and is used to determine to which Preset settings should be saved.
            ConfigFileNameTextBox = new GUITextBox(new RectTransform(new Vector2(0.8f, 0.5f), parent: ConfigPresetLayoutGroup.RectTransform, anchor: Anchor.CenterLeft), createPenIcon: false)
            {
                Text = Path.GetFileNameWithoutExtension(NTConfig.CurrentConfigPath),
                CanBeFocused = false
            };

            // LayoutGroup 95% / 95% the size of the InnerFrame. Invisible.
            // This group holds all the elements needed to display or alter visible content.
            var ContentLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.95f), parent: InnerLayoutGroup.RectTransform), isHorizontal: true)
            {
                RelativeSpacing = 0.01f
            };

            // ListBox 15% / 100% the size of the ContentLayoutGroup.
            // Holds the sidebar buttons to navigate Config Pages.
            var SidebarListBox = new GUIListBox(new RectTransform(new Vector2(0.15f, 1f), parent: ContentLayoutGroup.RectTransform))
            {
                Padding = new Vector4(5, 5, 5, 5),
                Spacing = 10
            };

            // ListBox 85% / 100% the size of the ContentLayoutGroup.
            // Holds the content relevant to the selected Config Page.
            PageListBox = new GUIListBox(new RectTransform(new Vector2(0.85f, 1.0f), parent: ContentLayoutGroup.RectTransform))
            {
                Padding = new Vector4(10, 15, 10, 10)
            };

            // LayoutGroup 100% of the size of the BaseLayoutGroup.
            // Holds the 3 buttons to Save / Reset / Exit the config.
            var ButtonLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), BaseLayoutGroup.RectTransform), isHorizontal: true)
            {
                RelativeSpacing = 0.02f
            };

            PopulateSidebar(SidebarListBox, PageListBox);
            PopulateSettings(PageListBox, SelectedExpansion);
            GUIComponents.CreateButtonRow(ButtonLayoutGroup, BaseFrame, ConfigFileNameTextBox);

            return null;
        }

        // Go over the Expansions set in the Config and sort them.
        private static void BuildExpansionList()
        {
            // Pull Expansions and populate ExpansionNameForUI, putting NT + base NT pages on top.
            Pages.Clear();

            // Neurotrauma, when adding its own config options, shouldn't be treated like an actual expansion.
            // TODO: Entirely sidestep this.
            foreach (var Expansion in NTConfig.Expansions)
            {
                if (Expansion.Name == "Neurotrauma")
                {
                    Pages.Add((Expansion.Name, Expansion.Name, "expansion"));
                    break;
                }
            }

            // Iterate the base config pages and add them.
            foreach (var Page in BaseConfigPages)
            {
                Pages.Add((Page.Name.Value, Page.Identifier, "page"));
            }

            // Go over the expansions that added config settings and add them.
            foreach (var Expansion in NTConfig.Expansions)
            {
                if (Expansion.Name == "Neurotrauma")
                {
                    continue;
                }

                // Some expansions have the NT prefix; some don't. This pisses me off, so if they don't have the prefix we force-add it.
                var UIName = Expansion.Name;

                if (!UIName.StartsWith("NT "))
                {
                    UIName = "NT " + UIName;
                }

                Pages.Add((UIName, Expansion.Name, "expansion"));
            }
        }

        // Fill the Sidebar with all possible Config Pages.
        private static void PopulateSidebar(GUIListBox Sidebar, GUIListBox PageListBox)
        {
            Sidebar.ClearChildren();

            BuildExpansionList();

            List<GUIButton> SidebarButtons = new List<GUIButton>();

            foreach (var Page in Pages)
            {
                var SidebarButton = new GUIButton(new RectTransform(new Vector2(1.0f, 0.05f), parent: Sidebar.Content.RectTransform), text: Page.UIName, textAlignment: Alignment.Center, style: "GUIButtonSmallFreeScale")
                {
                    UserData = Page,
                    Color = Color.Transparent
                };

                SidebarButton.TextBlock.Wrap = true;

                SidebarButton.OnClicked = (Button, _) =>
                {
                    var Selection = ((string Name, string Identifier, string Type))Button.UserData;

                    SelectedExpansion = Selection.Identifier;
                    SelectedType = Selection.Type;

                    PopulateSettings(PageListBox, SelectedExpansion);

                    foreach (var SidebarButton in SidebarButtons)
                    {
                        SidebarButton.Selected = SidebarButton == Button;
                    }

                    return true;
                };

                SidebarButtons.Add(SidebarButton);
            }

            if (SidebarButtons.Count > 0)
            {
                var FirstPage = Pages[0];
                SelectedExpansion = FirstPage.Identifier;
                SelectedType = FirstPage.Type;

                SidebarButtons[0].Selected = true;
            }
        }

        // Determine how the Config Layout should look with all entries folded together.
        private static List<LayoutChunk> PrebuildConfigLayout(Dictionary<string, ConfigEntry> Entries, string SelectedIdentifier, string SelectedType)
        {
            var PrebuiltConfig = new List<LayoutChunk>();

            LayoutChunk? CurrentGroup = null;
            ConfigEntry? LastEntry = null;

            foreach (var kvp in Entries)
            {
                var key = kvp.Key;
                var entry = kvp.Value;

                if (SelectedType == "page")
                {
                    if (entry.Page != SelectedIdentifier) continue;
                }
                else if (SelectedType == "expansion")
                {
                    if (entry.Expansion != SelectedIdentifier || entry.Page != null) continue;
                }

                if (LastEntry != null && entry.Type != LastEntry.Type)
                {
                    PrebuiltConfig.Add(new LayoutChunk { Type = "spacer" });
                }

                LastEntry = entry;

                bool IsGrouped = entry.Group && (entry.Type == ConfigEntryType.Float || entry.Type == ConfigEntryType.Integer || entry.Type == ConfigEntryType.String);

                if (entry.Type == ConfigEntryType.Category)
                {
                    CurrentGroup = null;
                    PrebuiltConfig.Add(new LayoutChunk
                    {
                        Type = "category",
                        Key = key,
                        Entry = entry
                    });

                    continue;
                }

                if (IsGrouped)
                {
                    string groupType = entry.Type switch
                    {
                        ConfigEntryType.Float => "float_group",
                        ConfigEntryType.Integer => "integer_group",
                        ConfigEntryType.String => "string_group",
                        _ => "standalone"
                    };

                    if (CurrentGroup == null || CurrentGroup.Type != groupType)
                    {
                        CurrentGroup = new LayoutChunk
                        {
                            Type = groupType,
                            Items = new List<(string, ConfigEntry)>()
                        };

                        PrebuiltConfig.Add(CurrentGroup);
                    }

                    CurrentGroup.Items.Add((key, entry));
                }
                else
                {
                    CurrentGroup = null;

                    PrebuiltConfig.Add(new LayoutChunk
                    {
                        Type = "standalone",
                        Key = key,
                        Entry = entry
                    });
                }
            }

            return PrebuiltConfig;
        }

        // Take the settings + their groups and add them to the UI dynamically.
        public static void PopulateSettings(GUIListBox PageListBox, string SelectedPage)
        {
            PageListBox.Content.ClearChildren();

            new GUITextBlock(new RectTransform(new Vector2(1, 0.05f), PageListBox.Content.RectTransform), TextManager.Get("ntgui_defaultmessage_config"), font: GUIStyle.SmallFont)
            {
                CanBeFocused = false,
                TextAlignment = Alignment.Center
            };

            var layout = PrebuildConfigLayout(NTConfig.Entries, SelectedExpansion, SelectedType);

            foreach (var chunk in layout)
            {
                switch (chunk.Type)
                {
                    case "category":
                        new GUITextBlock(new RectTransform(new Vector2(1, 0.1f), PageListBox.Content.RectTransform), chunk.Entry.Name, font: GUIStyle.LargeFont)
                        {
                            CanBeFocused = false,
                            TextAlignment = Alignment.BottomCenter
                        };
                        break;

                    case "spacer":
                        new GUILayoutGroup(new RectTransform(new Vector2(1, 0.02f), PageListBox.Content.RectTransform));
                        break;

                    case "float_group":
                        CreateFloatGroup(PageListBox, chunk.Items);
                        break;

                    case "integer_group":
                        CreateIntegerGroup(PageListBox, chunk.Items);
                        break;

                    case "string_group":
                        CreateStringGroup(PageListBox, chunk.Items);
                        break;

                    case "standalone":
                        CreateEntry(PageListBox, chunk.Key, chunk.Entry);
                        break;
                }
            }

            // Lock the page for multiplayer clients who aren't allowed to change settings.
            if (HF.GameIsMultiplayer() && !GUIComponents.CanCurrentClientEditSettings())
            {
                foreach (GUIComponent c in PageListBox.GetAllChildren())
                {
                    c.Enabled = false;
                }
            }
        }

        // Create grouped Floats.
        private static void CreateFloatGroup(GUIListBox PageListBox, List<(string key, ConfigEntry entry)> Entries)
        {
            const int MaxPerRow = 2;
            GUILayoutGroup? Row = null;
            int EntriesInRow = 0;

            foreach (var (key, entry) in Entries)
            {
                if (Row == null || EntriesInRow % MaxPerRow == 0)
                {
                    Row = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), PageListBox.Content.RectTransform), isHorizontal: true)
                    {
                        RelativeSpacing = 0.01f
                    };
                }

                float BaseWidth = 1f / MaxPerRow;
                float TextWidth = BaseWidth * 0.53f;
                float ScalarWidth = BaseWidth * 0.30f;
                float ResetWidth = BaseWidth * 0.07f;

                var TextCell = new GUILayoutGroup(new RectTransform(new Vector2(TextWidth, 1f), Row.RectTransform), isHorizontal: true);
                var ScalarCell = new GUILayoutGroup(new RectTransform(new Vector2(ScalarWidth, 1f), Row.RectTransform), isHorizontal: true);
                var ResetCell = new GUILayoutGroup(new RectTransform(new Vector2(ResetWidth, 0.6f), Row.RectTransform), isHorizontal: true);
                var SpacerCell = new GUILayoutGroup(new RectTransform(new Vector2(ResetWidth, 0.6f), Row.RectTransform), isHorizontal: true);

                var Label = entry.Name;

                if (entry.Type == ConfigEntryType.Float && entry.Range != null && entry.Range.Length >= 2)
                {
                    Label += $" ({entry.Range[0]} - {entry.Range[1]})";
                }

                float DefaultValue = (float)entry.Default;

                var LabelBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.6f), TextCell.RectTransform), Label)
                {
                    CanBeFocused = false,
                    TextAlignment = Alignment.Center,
                    Wrap = true,
                    AutoScaleHorizontal = true,
                    TextColor = (float)entry.Value == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange
                };

                GUIComponents.ApplyDescriptionTooltip(LabelBlock, entry, SetHoverColour: true);

                var Scalar = new GUINumberInput(new RectTransform(new Vector2(1f, 0.6f), ScalarCell.RectTransform), NumberType.Float)
                {
                    MinValueFloat = entry.Range[0],
                    MaxValueFloat = entry.Range[1],
                    FloatValue = (float)entry.Value,
                    ValueStep = 0.1f
                };

                Scalar.OnValueChanged += input =>
                {
                    NTConfig.Set(key, input.FloatValue);
                    LabelBlock.TextColor = input.FloatValue == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange;
                };

                EntriesInRow++;

                if (entry.Resettable)
                {
                    GUIComponents.CreateResetButton(ResetCell.RectTransform, () =>
                    {
                        Scalar.FloatValue = DefaultValue;
                        NTConfig.Set(key, DefaultValue);

                        LabelBlock.TextColor = GUIStyle.TextColorNormal;
                    });
                }
            }
        }

        // Create grouped Integers
        private static void CreateIntegerGroup(GUIListBox PageListBox, List<(string key, ConfigEntry entry)> Entries)
        {
            const int MaxPerRow = 2;
            GUILayoutGroup? Row = null;
            int EntriesInRow = 0;

            foreach (var (key, entry) in Entries)
            {
                if (Row == null || EntriesInRow % MaxPerRow == 0)
                {
                    Row = new GUILayoutGroup(
                        new RectTransform(
                            new Vector2(1f, 0.1f),
                            PageListBox.Content.RectTransform),
                        isHorizontal: true)
                    {
                        RelativeSpacing = 0.01f
                    };
                }

                float BaseWidth = 1f / MaxPerRow;
                float TextWidth = BaseWidth * 0.53f;
                float ScalarWidth = BaseWidth * 0.30f;
                float ResetWidth = BaseWidth * 0.07f;

                var TextCell = new GUILayoutGroup(new RectTransform(new Vector2(TextWidth, 1f), Row.RectTransform), isHorizontal: true);
                var ScalarCell = new GUILayoutGroup(new RectTransform(new Vector2(ScalarWidth, 1f), Row.RectTransform), isHorizontal: true);
                var ResetCell = new GUILayoutGroup(new RectTransform(new Vector2(ResetWidth, 0.6f), Row.RectTransform), isHorizontal: true);
                var SpacerCell = new GUILayoutGroup(new RectTransform(new Vector2(ResetWidth, 0.6f), Row.RectTransform), isHorizontal: true);

                var Label = entry.Name;

                if (entry.Type == ConfigEntryType.Integer && entry.Range != null && entry.Range.Length >= 2)
                {
                    Label += $" ({entry.Range[0]} - {entry.Range[1]})";
                }

                int DefaultValue = (int)(entry.Default);

                var LabelBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.6f), TextCell.RectTransform), Label)
                {
                    CanBeFocused = false,
                    TextAlignment = Alignment.Center,
                    Wrap = true,
                    AutoScaleHorizontal = true,
                    TextColor = ((int)(entry.Value) == DefaultValue) ? GUIStyle.TextColorNormal : GUIStyle.Orange
                };

                GUIComponents.ApplyDescriptionTooltip(LabelBlock, entry, SetHoverColour: true);

                var Scalar = new GUINumberInput(new RectTransform(new Vector2(1f, 0.6f), ScalarCell.RectTransform), NumberType.Int)
                {
                    MinValueInt = (int)entry.Range[0],
                    MaxValueInt = (int)entry.Range[1],
                    IntValue = (int)entry.Value,
                    ValueStep = 1f
                };

                Scalar.OnValueChanged += input =>
                {
                    NTConfig.Set(key, input.IntValue);

                    LabelBlock.TextColor = input.IntValue == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange;
                };

                EntriesInRow++;

                if (entry.Resettable)
                {
                    GUIComponents.CreateResetButton(ResetCell.RectTransform, () =>
                    {
                        Scalar.IntValue = DefaultValue;
                        NTConfig.Set(key, DefaultValue);

                        LabelBlock.TextColor = GUIStyle.TextColorNormal;
                    });
                }
            }
        }

        // Create grouped strings.
        private static void CreateStringGroup(GUIListBox PageListBox, List<(string key, ConfigEntry entry)> Entries)
        {
            const int MaxPerRow = 2;
            GUILayoutGroup? Row = null;

            for (int i = 0; i < Entries.Count; i++)
            {
                var (key, entry) = Entries[i];

                if (Row == null || i % MaxPerRow == 0)
                {
                    Row = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.09f), PageListBox.Content.RectTransform), isHorizontal: true)
                    {
                        RelativeSpacing = 0.01f
                    };
                }

                const float BaseWidth = 1f / MaxPerRow;
                const float TextWidth = BaseWidth * 0.50f;
                const float InputWidth = BaseWidth * 0.33f;
                const float ResetWidth = BaseWidth * 0.07f;

                var textCell = new GUILayoutGroup(new RectTransform(new Vector2(TextWidth, 1f), Row.RectTransform), isHorizontal: true);
                var inputCell = new GUILayoutGroup(new RectTransform(new Vector2(InputWidth, 1f), Row.RectTransform), isHorizontal: true);
                var resetCell = new GUILayoutGroup(new RectTransform(new Vector2(ResetWidth, 0.6f), Row.RectTransform), isHorizontal: true);

                string Label = entry.Name.Value + (entry.Style.IsNullOrWhiteSpace() ? "" : $" ({entry.Style})");
                string DefaultValue = Convert.ToString(entry.Default) ?? string.Empty;
                string Value = GUIComponents.GetStringValue(key, entry);

                var LabelBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.4f), textCell.RectTransform), Label)
                {
                    CanBeFocused = false,
                    TextAlignment = Alignment.Center,
                    Wrap = true,
                };

                var Input = GUIComponents.CreateStringInput(inputCell.RectTransform, entry, Value);

                Input.OnTextChanged += (_, text) =>
                {
                    GUIComponents.SetStringValue(key, entry, text);
                    return true;
                };

                if (entry.Resettable)
                {
                    GUIComponents.CreateResetButton(resetCell.RectTransform, () =>
                    {
                        GUIComponents.ResetStringValue(key, entry, Input);
                    });
                }
            }
        }

        // Create Standalone settings.
        private static void CreateEntry(GUIListBox PageListBox, string Identifier, ConfigEntry entry)
        {
            if (entry.Type == ConfigEntryType.Category)
            {
                var header = new GUITextBlock(new RectTransform(new Vector2(1f, 0.09f), PageListBox.Content.RectTransform), entry.Name)
                {
                    CanBeFocused = false,
                    TextAlignment = Alignment.BottomCenter,
                    Wrap = true
                };

                return;
            }

            switch (entry.Type)
            {
                case ConfigEntryType.Float:
                {
                    float min = entry.Range?.Length > 0 ? entry.Range[0] : 0f;
                    float max = entry.Range?.Length > 1 ? entry.Range[1] : 100f;
                    float DisplayMin = min == 0.99f ? 1f : min;
                    float DefaultValue = (float)(entry.Default);

                    var Label = new GUITextBlock(new RectTransform(new Vector2(1f, 0.04f), PageListBox.Content.RectTransform), $"{entry.Name} ({DisplayMin}-{max})")
                    {
                        CanBeFocused = false,
                        TextAlignment = Alignment.Center,
                        Wrap = true,
                        AutoScaleHorizontal = true,
                        TextColor = NTConfig.Get(Identifier, DefaultValue) == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange
                    };

                    GUIComponents.ApplyDescriptionTooltip(Label, entry);

                    var Scalar = new GUINumberInput(new RectTransform(new Vector2(1f, 0.08f), PageListBox.Content.RectTransform), NumberType.Float)
                    {
                        ValueStep = 0.1f,
                        MinValueFloat = min,
                        MaxValueFloat = max,
                        FloatValue = NTConfig.Get(Identifier, DefaultValue)
                    };

                    Scalar.OnValueChanged += input =>
                    {
                        NTConfig.Set(Identifier, input.FloatValue);
                        Label.TextColor = NTConfig.Get(Identifier, DefaultValue) == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange;
                    };

                    if (entry.Resettable)
                    {
                        GUIComponents.CreateResetButton(Scalar.RectTransform, () =>
                        {
                            Scalar.FloatValue = DefaultValue;
                            NTConfig.Set(Identifier, DefaultValue);

                            Label.TextColor = GUIStyle.TextColorNormal;
                        });
                    }

                    break;
                }

                case ConfigEntryType.Integer:
                {
                    int min = entry.Range?.Length > 0 ? (int)entry.Range[0] : 0;
                    int max = entry.Range?.Length > 1 ? (int)entry.Range[1] : 100;
                    int DefaultValue = (int)(entry.Default);

                    var Label = new GUITextBlock(new RectTransform(new Vector2(1f, 0.04f), PageListBox.Content.RectTransform), $"{entry.Name} ({min}-{max})")
                    {
                        CanBeFocused = false,
                        TextAlignment = Alignment.Center,
                        Wrap = true,
                        AutoScaleHorizontal = true,
                        TextColor = NTConfig.Get(Identifier, DefaultValue) == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange
                    };

                    GUIComponents.ApplyDescriptionTooltip(Label, entry);

                    var Scalar = new GUINumberInput(new RectTransform(new Vector2(1f, 0.08f), PageListBox.Content.RectTransform), NumberType.Int)
                    {
                        ValueStep = 1f,
                        MinValueInt = min,
                        MaxValueInt = max,
                        IntValue = (int)NTConfig.Get(Identifier, DefaultValue)
                    };

                    Scalar.OnValueChanged += input =>
                    {
                        NTConfig.Set(Identifier, input.IntValue);
                        Label.TextColor = input.IntValue == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange;
                    };

                    if (entry.Resettable)
                    {
                        GUIComponents.CreateResetButton(Scalar.RectTransform, () =>
                        {
                            Scalar.IntValue = DefaultValue;
                            NTConfig.Set(Identifier, DefaultValue);

                            Label.TextColor = GUIStyle.TextColorNormal;
                        });
                    }

                    break;
                }

                case ConfigEntryType.Bool:
                {
                    bool DefaultValue = Convert.ToBoolean(entry.Default);
                    bool CurrentValue = NTConfig.Get(Identifier, DefaultValue);

                    var TickBox = new GUITickBox(new RectTransform(new Vector2(0.5f, 0.05f), PageListBox.Content.RectTransform), entry.Name)
                    {
                        TextColor = CurrentValue == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange
                    };

                    if (!entry.Description.IsNullOrWhiteSpace())
                    {
                        TickBox.ToolTip = RichString.Rich(entry.Description);
                    }

                    TickBox.Selected = CurrentValue;

                    TickBox.OnSelected += tb =>
                    {
                        NTConfig.Set(Identifier, tb.Selected);
                        TickBox.TextBlock.OverrideTextColor(tb.Selected == DefaultValue ? GUIStyle.TextColorNormal : GUIStyle.Orange);
                        return true;
                    };

                    break;
                }

                case ConfigEntryType.String:
                {
                    string StyleSuffix = entry.Style.IsNullOrWhiteSpace() ? string.Empty : $" ({entry.Style})";

                    var Label = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), PageListBox.Content.RectTransform), $"{entry.Name}{StyleSuffix}")
                    {
                        CanBeFocused = false,
                        TextAlignment = Alignment.Center,
                        Wrap = true,
                        AutoScaleHorizontal = true
                    };

                    GUIComponents.ApplyDescriptionTooltip(Label, entry);

                    float Boxsize = entry.Boxsize > 0f ? entry.Boxsize : 0.08f;
                    string Value = GUIComponents.GetStringValue(Identifier, entry);

                    GUITextBox Input;

                    if (entry.NoMLTB)
                    {
                        Input = new GUITextBox(new RectTransform(new Vector2(1f, Boxsize), PageListBox.Content.RectTransform));
                        Input.Text = Value;
                    }
                    else
                    {
                        Input = GUIComponents.CreateMultiLineTextBox(PageListBox.Content.RectTransform, Value, Boxsize);
                    }

                    Input.OnTextChanged += (_, text) =>
                    {
                        GUIComponents.SetStringValue(Identifier, entry, text);
                        return true;
                    };

                    if (entry.Resettable)
                    {
                        GUIComponents.CreateResetButton(Input.RectTransform, () => GUIComponents.ResetStringValue(Identifier, entry, Input));
                    }

                    break;
                }
            }
        }
    }

    public static class GUIComponents
    {

        // Is this client the server host?
        public static bool IsServerHost() => GameMain.NetworkMember?.IsServer == true;

        // Is the client allowed to change the settings?
        public static bool CanCurrentClientEditSettings()
        {
            if (IsServerHost())
            {
                return true;
            }

            Client? client = GameMain.Client?.MyClient;

            return client != null && (client.IsOwner || client.HasPermission(ClientPermissions.ManageSettings));
        }


        // Save & Exit Button.
        public static GUIButton CreateSettingsSaveExitButton(GUILayoutGroup Parent, GUIFrame Container, GUITextBox? ConfigFileNameBox)
        {
            var Button = new GUIButton(new RectTransform(new Vector2(0.32f, 1f), parent: Parent.RectTransform), text: TextManager.Get("ntgui_configmenubutton_saveexit"));

            Button.OnClicked = (_, _) =>
            {
                string targetPath = (ConfigFileNameBox != null && !ConfigFileNameBox.Text.IsNullOrWhiteSpace()) ? NTConfig.ResolvePathFromName(ConfigFileNameBox.Text) : NTConfig.CurrentConfigPath;

                NTConfig.SetCurrentConfigPath(targetPath);
                NTConfig.SaveConfig(targetPath);

                if (CanCurrentClientEditSettings())
                {
                    NTConfig.SendConfig();
                }

                Container.Parent.RemoveChild(Container);
                return true;
            };

            return Button;
        }

        // Discard & Exit Button.
        public static GUIButton CreateSettingsDiscardExitButton(GUILayoutGroup Parent, GUIFrame Container)
        {
            var Button = new GUIButton(new RectTransform(new Vector2(0.32f, 1f), parent: Parent.RectTransform), text: TextManager.Get("ntgui_configmenubutton_discardexit"));

            Button.OnClicked = (_, _) =>
            {
                Container.Parent.RemoveChild(Container);
                return true;
            };

            return Button;
        }

        // Reset Button.
        public static GUIButton CreateSettingsResetButton(GUILayoutGroup Parent, GUIFrame Container)
        {
            var Button = new GUIButton(new RectTransform(new Vector2(0.32f, 1f), parent: Parent.RectTransform), text: TextManager.Get("ntgui_configmenubutton_resetvalues"));

            Button.OnClicked = (_, _) =>
            {
                bool AllowedToReset = CanCurrentClientEditSettings();

                if (!AllowedToReset)
                {
                    return true;
                }

                ShowResetMessage(Container);

                return true;
            };

            return Button;
        }

        // Show a warning message after clicking the Reset Button.
        public static void ShowResetMessage(GUIFrame Container)
        {
            var resetMessage = new GUIMessageBox(TextManager.Get("ntgui_resetconfirm_title"), TextManager.Get("ntgui_resetconfirm_body"), new LocalizedString[]
            {
                TextManager.Get("ntgui_resetconfirm_yes"),
                TextManager.Get("ntgui_resetconfirm_no")
            })
            {
                DrawOnTop = true
            };

            resetMessage.Text.TextAlignment = Alignment.Center;

            resetMessage.Buttons[0].OnClicked = (_, _) =>
            {
                NTConfig.ResetConfig();

                if (IsServerHost())
                {
                    NTConfig.SaveConfig();
                }
                else if (CanCurrentClientEditSettings())
                {
                    NTConfig.SendConfig();
                }

                Container.Parent.RemoveChild(Container);
                resetMessage.Close();
                return true;
            };

            resetMessage.Buttons[1].OnClicked = (_, _) =>
            {
                resetMessage.Close();
                return true;
            };
        }

        // Add the 3 buttons to the UI.
        public static void CreateButtonRow(GUILayoutGroup Parent, GUIFrame Container, GUITextBox? ConfigFileNameBox = null)
        {
            CreateSettingsSaveExitButton(Parent, Container, ConfigFileNameBox);
            CreateSettingsDiscardExitButton(Parent, Container);
            CreateSettingsResetButton(Parent, Container);
        }

        // Attach a tooltip to a label if the entry has a description + prevent the bad-looking hover effect on unfocused textboxes.
        public static void ApplyDescriptionTooltip(GUITextBlock Label, ConfigEntry entry, bool SetHoverColour = false)
        {
            if (!entry.Description.IsNullOrWhiteSpace())
            {
                Label.ToolTip = RichString.Rich(entry.Description);
                Label.CanBeFocused = true;

                if (SetHoverColour)
                {
                    Label.HoverColor = Color.Gray;
                }
            }
        }

        // Create a TextBox that can hold multiple lines + automatically resize.
        public static GUITextBox CreateMultiLineTextBox(RectTransform parent, string text = "", float size = 0.2f)
        {
            var listBox = new GUIListBox(new RectTransform(new Vector2(1f, size), parent));
            var textBox = new GUITextBox(new RectTransform(new Vector2(1f, 1f), listBox.Content.RectTransform), FormatList(text), textColor: null, font: null, textAlignment: Alignment.Left, wrap: true, style: "GUITextBoxNoBorder");

            textBox.OnSelected += (_, _) =>
            {
                float caretY = textBox.CaretScreenPos.Y;
                float bottomCaretExtent = textBox.Font.LineHeight * 1.5f;
                float topCaretExtent = -textBox.Font.LineHeight * 0.5f;

                if (caretY + bottomCaretExtent > listBox.Rect.Bottom)
                {
                    listBox.ScrollBar.BarScroll = (caretY - textBox.Rect.Top - listBox.Rect.Height + bottomCaretExtent) / (textBox.Rect.Height - listBox.Rect.Height);
                }
                else if (caretY + topCaretExtent < listBox.Rect.Top)
                {
                    listBox.ScrollBar.BarScroll = (caretY - textBox.Rect.Top + topCaretExtent) / (textBox.Rect.Height - listBox.Rect.Height);
                }
            };

            textBox.OnTextChanged += (_, __) =>
            {
                Vector2 textSize = textBox.Font.MeasureString(textBox.WrappedText);

                textBox.RectTransform.NonScaledSize = new Point(textBox.RectTransform.NonScaledSize.X, (int)Math.Max(listBox.Content.Rect.Height, textSize.Y + 10));

                listBox.UpdateScrollBarSize();

                return true;
            };

            textBox.OnEnterPressed += (tb, _) =>
            {
                string str = tb.Text;
                int caret = tb.CaretIndex;

                tb.Text = str.Substring(0, caret) + "\n" + str.Substring(caret);
                tb.CaretIndex = caret + 1;

                return true;
            };

            return textBox;
        }

        // Create a button to reset a config option to its default.
        public static GUIButton CreateResetButton(RectTransform Parent, Action Reset)
        {
            var ResetButton = new GUIButton(new RectTransform(new Vector2(1f, 1f), parent: Parent), style: "GUIButtonRefresh")
            {
                ToolTip = TextManager.Get("ntgui_resetbutton_tooltip")
            };

            ResetButton.OnClicked += (_, _) =>
            {
                Reset();
                return true;
            };

            return ResetButton;
        }

        // Create a string input (Multi-line or not).
        public static GUITextBox CreateStringInput(RectTransform Parent, ConfigEntry entry, string value)
        {
            float boxSize = entry.Boxsize > 0f ? entry.Boxsize : 0.08f;
            GUITextBox input = entry.NoMLTB ? new GUITextBox(new RectTransform(new Vector2(1f, boxSize), Parent)) : GUIComponents.CreateMultiLineTextBox(Parent, value, boxSize);

            input.Text = value;
            return input;
        }

        // Reset a string config option to its default value.
        public static void ResetStringValue(string key, ConfigEntry entry, GUITextBox input)
        {
            string defaultText;

            if (entry.Default is List<string> defaultList)
            {
                defaultText = string.Join(", ", defaultList);
            }
            else if (entry.Default is string defaultString)
            {
                defaultText = FormatList(defaultString);
            }
            else
            {
                defaultText = entry.Default?.ToString() ?? "";
            }

            input.Text = defaultText;

            SetStringValue(key, entry, defaultText);
        }


        // Set a string's config value
        public static void SetStringValue(string key, ConfigEntry entry, object? value)
        {
            if (value is List<string> list)
            {
                NTConfig.Set(key, list);
                return;
            }

            string text = value?.ToString() ?? "";

            if (entry.Value is List<string> || entry.Default is List<string>)
            {
                var values = text.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

                NTConfig.Set(key, values);
            }
            else
            {
                NTConfig.Set(key, text);
            }
        }


        // Format a string of text.
        public static string FormatList(string TextToFormat)
        {
            if (string.IsNullOrWhiteSpace(TextToFormat))
            {
                return "";
            }

            return string.Join(", ", TextToFormat.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        public static string GetStringValue(string key, ConfigEntry entry)
        {
            var storedObj = NTConfig.Get(key);

            if (storedObj is List<string> list)
            {
                return string.Join(", ", list);
            }

            if (storedObj is string s)
            {
                return FormatList(s);
            }

            if (entry.Value is List<string> v)
            {
                return string.Join(", ", v);
            }

            if (entry.Default is List<string> d)
            {
                return string.Join(", ", d);
            }

            if (entry.Default is string ds)
            {
                return FormatList(ds);
            }

            return "";
        }
    }

    public static class ConfigPresets
    {
        // Hide the config UI + pause menu while a preset popup is open.
        private static void HideMenus()
        {
            if (ConfigurationMenu.BaseFrame != null) 
            { 
                ConfigurationMenu.BaseFrame.Visible = false; 
            }

            if (GUI.PauseMenu != null) 
            { 
                GUI.PauseMenu.Visible = false; 
            }
        }

        // Close the given popup and restore visibility of the config UI + pause menu.
        private static void CloseAndRestore(GUIMessageBox MessageBox)
        {
            MessageBox.Close();

            if (ConfigurationMenu.BaseFrame != null) 
            { 
                ConfigurationMenu.BaseFrame.Visible = true; 
            }

            if (GUI.PauseMenu != null) 
            { 
                GUI.PauseMenu.Visible = true;
            }
        }

        public static void OpenSaveConfigPreset()
        {
            HideMenus();

            var MessageBox = new GUIMessageBox(TextManager.Get("ntgui_save_preset_header"), "", buttons: new[] 
            { 
                TextManager.Get("Save"), 
                TextManager.Get("Cancel") 
            }, 
            relativeSize: (0.4f, 0.2f));

            var NameBox = new GUITextBox(new RectTransform((1.0f, 0.3f), parent: MessageBox.Content.RectTransform), text: "");

            MessageBox.Buttons[0].OnClicked = (_, _) =>
            {
                if (NameBox.Text.IsNullOrEmpty())
                {
                    NameBox.Flash(GUIStyle.Red);
                    return false;
                }

                NTConfig.SavePreset(NameBox.Text);

                if (ConfigurationMenu.ConfigFileNameTextBox != null)
                {
                    ConfigurationMenu.ConfigFileNameTextBox.Text = NameBox.Text;
                }

                CloseAndRestore(MessageBox);
                return false;
            };

            MessageBox.Buttons[1].OnClicked = (button, o) =>
            {
                CloseAndRestore(MessageBox);
                return false;
            };
        }

        public static void OpenLoadConfigPreset()
        {
            HideMenus();

            var MessageBox = new GUIMessageBox(TextManager.Get("ntgui_load_preset_header"), "", buttons: new[]
            {
                TextManager.Get("Load"),
                TextManager.Get("Cancel")
            }, relativeSize: (0.4f, 0.6f));

            var PresetListBox = new GUIListBox(new RectTransform((1.0f, 0.7f), parent: MessageBox.Content.RectTransform));

            void AddPresetEntry(string PresetPath, bool IsDefault)
            {
                string PresetName = Path.GetFileNameWithoutExtension(PresetPath);

                var PresetFrame = new GUIFrame(new RectTransform((1.0f, 0.09f), parent: PresetListBox.Content.RectTransform), style: "ListBoxElement")
                {
                    UserData = PresetPath
                };

                new GUITextBlock(new RectTransform(Vector2.One, parent: PresetFrame.RectTransform), PresetName)
                {
                    CanBeFocused = false
                };

                if (!IsDefault)
                {
                    new GUIButton(new RectTransform((0.2f, 1.0f), parent: PresetFrame.RectTransform, Anchor.CenterRight), text: TextManager.Get("Delete"), style: "GUIButtonSmall")
                    {
                        OnClicked = (_, _) =>
                        {
                            File.Delete(PresetPath);
                            PresetListBox.Content.RemoveChild(PresetFrame);
                            return false;
                        }
                    };
                }
            }

            foreach (string path in NTConfig.GetDefaultPresetFiles())
            {
                AddPresetEntry(path, IsDefault: true);
            }

            foreach (string path in NTConfig.GetPresetFiles())
            {
                AddPresetEntry(path, IsDefault: false);
            }

            MessageBox.Buttons[0].OnClicked = (button, o) =>
            {
                if (PresetListBox.SelectedData is string path)
                {
                    NTConfig.LoadPreset(path);

                    if (ConfigurationMenu.ConfigFileNameTextBox != null)
                    {
                        ConfigurationMenu.ConfigFileNameTextBox.Text = Path.GetFileNameWithoutExtension(path);
                    }

                    ConfigurationMenu.PopulateSettings(ConfigurationMenu.PageListBox, ConfigurationMenu.SelectedExpansion);
                }

                CloseAndRestore(MessageBox);
                return false;
            };

            MessageBox.Buttons[1].OnClicked = (_, _) =>
            {
                CloseAndRestore(MessageBox);
                return false;
            };
        }
    }
}