using Barotrauma.LuaCs.Compatibility;
using Barotrauma.Networking;
using Neurotrauma.ClientSource;

namespace Neurotrauma
{
    internal class ConfigurationMenu
    {
        private static Harmony? Harmony;
        private static GUIButton NTPauseMenuButton;

        private static readonly List<(string UIName, string Identifier, string Type)> Pages = new();
        private static readonly List<(LocalizedString Name, string Identifier)> BaseConfigPages = new()
        {
            (TextManager.Get("ntconfig_pagename_prices"), "prices"),
            (TextManager.Get("ntconfig_pagename_availability"), "availability"),
            (TextManager.Get("ntconfig_pagename_experimental"), "experimental")
        };

        private static string? SelectedExpansion = null;
        private static string? SelectedType = null;

        private class LayoutChunk
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

            return;
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
            // Sync the config settings on opening
            if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
            {
                IWriteMessage ConfigRequest = LuaCsSetup.Instance.Networking.Start("NT.ConfigRequest");
                LuaCsSetup.Instance.Networking.Send(ConfigRequest);
            }

            // Frame 75% / 80% the size of the screen that will hold the UI.
            // This is the green, see-through background used in many UI elements.
            var BaseFrame = new GUIFrame(new RectTransform(new Vector2(0.75f, 0.8f), parent: Parent.RectTransform, anchor: Anchor.Center));

            // LayoutGroup 95% the size of the BaseFrame
            // This holds all the content via the InnerFrame + the 3 buttons at the bottom of the page.
            var BaseLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), parent: BaseFrame.RectTransform, anchor: Anchor.Center));

            // Frame 95% the size of the BaseLayoutGroup; this gives us a small border.
            // This is a black, see-through background - the config isn't perfectly opaque but this looks better in my opinion.
            var InnerFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.95f), parent: BaseLayoutGroup.RectTransform), style: "InnerFrame");

            // LayoutGroup 95% the size of the InnerFrame.
            // Used to determine the layout of the other elements below it so they don't overlap.
            var InnerLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), InnerFrame.RectTransform, anchor: Anchor.TopCenter));

            // Textblock 100% / 5% the size of the LayoutGroup.
            // Holds the Neurotrauma title text centered at the top.
            var TitleTextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), parent: InnerLayoutGroup.RectTransform), text: TextManager.Get("ntgui_config_title"), font: GUIStyle.LargeFont, textAlignment: Alignment.TopCenter);

            // LayoutGroup 95% / 95% the size of the InnerFrame. Invisible.
            // This group holds all the elements needed to display or alter visible content.
            var ContentLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.95f), parent: InnerLayoutGroup.RectTransform), isHorizontal: true)
            {
                RelativeSpacing = 0.01f
            };

            // ListBox 10% / 100% the size of the ContentLayoutGroup.
            // Holds the sidebar buttons to navigate Config Pages.
            var SidebarListBox = new GUIListBox(new RectTransform(new Vector2(0.15f, 1f), parent: ContentLayoutGroup.RectTransform))
            {
                Padding = new Vector4(5, 5, 5, 5),
                Spacing = 10
            };

            // ListBox 90% / 100% the size of the ContentLayoutGroup.
            // Holds the content relevant to the selected Config Page.
            var PageListBox = new GUIListBox(new RectTransform(new Vector2(0.85f, 1.0f), parent: ContentLayoutGroup.RectTransform))
            {
                Padding = new Vector4(10, 15, 10, 10)
            };

            // LayoutGroup 100% of the size of the BaseLayoutGroup.
            // Holds the 3 buttons to Save / Reset / Exit the config.
            var ButtonLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), BaseLayoutGroup.RectTransform), isHorizontal: true)
            {
                RelativeSpacing = 0.02f
            };

            // Populate the Sidebar.
            PopulateSidebar(SidebarListBox, PageListBox);

            // Populate the active Page.
            PopulateSettings(PageListBox, SelectedExpansion);

            // Initialize the buttons.
            GUIComponents.CreateButtonRow(ButtonLayoutGroup, BaseFrame);

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

                // PopulateSettings(menuList, selectedExpansion);
                SidebarButtons[0].Selected = true;
            }
        }

        private static List<LayoutChunk> PrebuildConfigLayout(Dictionary<string, ConfigEntry> entries, string selectedId, string selectedType)
        {
            var result = new List<LayoutChunk>();

            LayoutChunk? currentGroup = null;
            ConfigEntry? lastEntry = null;

            foreach (var kvp in entries)
            {
                var key = kvp.Key;
                var entry = kvp.Value;

                if (selectedType == "page")
                {
                    if (entry.Page != selectedId) continue;
                }

                else if (selectedType == "expansion")
                {
                    if (entry.Expansion != selectedId || entry.Page != null) continue;
                }

                if (lastEntry != null && entry.Type != lastEntry.Type)
                {
                    result.Add(new LayoutChunk { Type = "spacer" });
                }

                lastEntry = entry;

                bool isGrouped = entry.Group && (entry.Type == ConfigEntryType.Float || entry.Type == ConfigEntryType.String);

                if (entry.Type == ConfigEntryType.Category)
                {
                    currentGroup = null;
                    result.Add(new LayoutChunk { Type = "category", Key = key, Entry = entry });
                    continue;
                }

                if (isGrouped)
                {
                    string groupType = entry.Type == ConfigEntryType.Float ? "float_group" : "string_group";

                    if (currentGroup == null || currentGroup.Type != groupType)
                    {
                        currentGroup = new LayoutChunk
                        {
                            Type = groupType,
                            Items = new List<(string, ConfigEntry)>()
                        };
                        result.Add(currentGroup);
                    }

                    currentGroup.Items.Add((key, entry));
                }
                else
                {
                    currentGroup = null;
                    result.Add(new LayoutChunk
                    {
                        Type = "standalone",
                        Key = key,
                        Entry = entry
                    });
                }
            }

            return result;
        }

        // Take the settings + their groups and add them to the UI dynamically.
        private static void PopulateSettings(GUIListBox list, string selected)
        {
            list.Content.ClearChildren();

            new GUITextBlock(new RectTransform(new Vector2(1, 0.05f), list.Content.RectTransform), TextManager.Get("ntgui_defaultmessage_config"), font: GUIStyle.SmallFont)
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
                        new GUITextBlock(new RectTransform(new Vector2(1, 0.1f), list.Content.RectTransform), chunk.Entry.Name, font: GUIStyle.LargeFont)
                        {
                            CanBeFocused = false,
                            TextAlignment = Alignment.BottomCenter
                        };
                        break;

                    case "spacer":
                        new GUILayoutGroup(new RectTransform(new Vector2(1, 0.02f), list.Content.RectTransform));
                        break;

                    case "float_group":
                        CreateFloatGroup(list, chunk.Items);
                        break;

                    case "string_group":
                        CreateStringGroup(list, chunk.Items);
                        break;

                    case "standalone":
                        CreateEntry(list, chunk.Key, chunk.Entry);
                        break;
                }
            }

            Client? client = GameMain.Client?.MyClient;

            if (client == null || !(client.IsOwner || client.HasPermission(ClientPermissions.ManageSettings))) // Need to add a (!IsMultiplayer) check
            {
                if (HF.GameIsMultiplayer())
                {
                    foreach (GUIComponent c in list.GetAllChildren())
                        c.Enabled = false;
                }
            }
        }

        // Create grouped Floats
        private static void CreateFloatGroup(GUIListBox list, List<(string key, ConfigEntry entry)> items)
        {
            const int MaxPerRow = 2;
            GUILayoutGroup? row = null;
            int count = 0;

            foreach (var (key, entry) in items)
            {
                if (row == null || count % MaxPerRow == 0)
                {
                    row = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), list.Content.RectTransform), isHorizontal: true)
                    {
                        RelativeSpacing = 0.01f
                    };
                }

                float BaseWidth = 1f / MaxPerRow;
                float TextWidth = BaseWidth * 0.53f;
                float ScalarWidth = BaseWidth * 0.30f;
                float ResetWidth = BaseWidth * 0.07f;

                var TextCell = new GUILayoutGroup(new RectTransform(new Vector2(TextWidth, 1f), row.RectTransform), isHorizontal: true);
                var ScalarCell = new GUILayoutGroup(new RectTransform(new Vector2(ScalarWidth, 1f), row.RectTransform), isHorizontal: true);
                var ResetCell = new GUILayoutGroup(new RectTransform(new Vector2(ResetWidth, 0.6f), row.RectTransform), isHorizontal: true);
                var SpacerCell = new GUILayoutGroup(new RectTransform(new Vector2(ResetWidth, 0.6f), row.RectTransform), isHorizontal: true);

                var label = entry.Name;

                if (entry.Type == ConfigEntryType.Float && entry.Range != null && entry.Range.Length >= 2)
                {
                    label += $" ({entry.Range[0]} - {entry.Range[1]})";
                }

                new GUITextBlock(new RectTransform(new Vector2(1f, 0.6f), TextCell.RectTransform), label)
                {
                    CanBeFocused = false,
                    TextAlignment = Alignment.Center,
                    Wrap = true,
                    AutoScaleHorizontal = true,
                };

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
                };

                count++;

                if (entry.Resettable)
                {
                    var ResetButton = new GUIButton(new RectTransform(new Vector2(1f, 1f), ResetCell.RectTransform), style: "GUIButtonRefresh")
                    {
                        ToolTip = TextManager.Get("ntgui_resetbutton_tooltip")
                    };

                    ResetButton.OnClicked += (btn, obj) =>
                    {
                        float defaultValue = Convert.ToSingle(entry.Default);
                        Scalar.FloatValue = defaultValue;
                        NTConfig.Set(key, defaultValue);
                        return true;
                    };
                }
            }
        }

        // Create grouped strings
        private static void CreateStringGroup(GUIListBox list, List<(string key, ConfigEntry entry)> items)
        {
            const int MaxPerRow = 2;
            GUILayoutGroup? row = null;
            int count = 0;

            foreach (var (key, entry) in items)
            {
                if (row == null || count % MaxPerRow == 0)
                {
                    row = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.09f), list.Content.RectTransform), isHorizontal: true)
                    {
                        RelativeSpacing = 0.01f
                    };
                }

                float BaseWidth = 1f / MaxPerRow;
                float TextWidth = BaseWidth * 0.50f;
                float InputWidth = BaseWidth * 0.33f;
                float ResetWidth = BaseWidth * 0.07f;

                var TextCell = new GUILayoutGroup(new RectTransform(new Vector2(TextWidth, 1f), row.RectTransform), isHorizontal: true);
                var InputCell = new GUILayoutGroup(new RectTransform(new Vector2(InputWidth, 1f), row.RectTransform), isHorizontal: true);
                var ResetCell = new GUILayoutGroup(new RectTransform(new Vector2(ResetWidth, 0.6f), row.RectTransform), isHorizontal: true);
                var Label = entry.Name + (entry.Style.IsNullOrWhiteSpace() ? "" : $" ({entry.Style})");
                new GUITextBlock(new RectTransform(new Vector2(1f, 0.4f), TextCell.RectTransform), Label)
                {
                    CanBeFocused = false,
                    TextAlignment = Alignment.Center,
                    Wrap = true
                };

                string value = GUIComponents.GetStringValue(key, entry);

                GUITextBox input;

                if (entry.NoMLTB)
                {
                    input = new GUITextBox(new RectTransform(new Vector2(1f, entry.Boxsize), InputCell.RectTransform));
                }
                else
                {
                    input = GUIComponents.CreateMultiLineTextBox(InputCell.RectTransform, value, entry.Boxsize);
                }

                input.Text = value;

                input.OnTextChanged += (_, text) =>
                {
                    if (entry.Value is List<string>)
                    {
                        NTConfig.Set(key, text.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                    }
                    else
                    {
                        NTConfig.Set(key, text);
                    }

                    return true;
                };

                if (entry.Resettable)
                {
                    var ResetButton = new GUIButton(new RectTransform(new Vector2(1f, 1f), ResetCell.RectTransform), style: "GUIButtonRefresh")
                    {
                        ToolTip = TextManager.Get("ntgui_resetbutton_tooltip")
                    };

                    ResetButton.OnClicked += (_, _) =>
                    {
                        var defObj = entry.Default;

                        if (defObj is List<string> dl)
                        {
                            input.Text = string.Join(",", dl);
                            NTConfig.Set(key, dl);
                        }
                        else if (defObj is string ds)
                        {
                            input.Text = ds;
                            NTConfig.Set(key, ds);
                        }
                        else
                        {
                            string def = defObj?.ToString() ?? "";
                            input.Text = def;
                            NTConfig.Set(key, def);
                        }

                        return true;
                    };
                }

                count++;
            }
        }

        // Create Standalone settings
        private static void CreateEntry(GUIListBox list, string id, ConfigEntry entry)
        {
            if (entry.Type == ConfigEntryType.Category)
            {
                var header = new GUITextBlock(new RectTransform(new Vector2(1f, 0.09f), list.Content.RectTransform), entry.Name)
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

                        var Label = new GUITextBlock(new RectTransform(new Vector2(1f, 0.04f), list.Content.RectTransform), $"{entry.Name} ({DisplayMin}-{max})")
                        {
                            CanBeFocused = false,
                            TextAlignment = Alignment.Center,
                            Wrap = true,
                            AutoScaleHorizontal = true
                        };

                        if (!entry.Description.IsNullOrEmpty())
                        {
                            Label.ToolTip = entry.Description;
                            Label.CanBeFocused = true;
                        }

                        var Scalar = new GUINumberInput(new RectTransform(new Vector2(1f, 0.08f), list.Content.RectTransform), NumberType.Float)
                        {
                            ValueStep = 0.1f,
                            MinValueFloat = min,
                            MaxValueFloat = max,
                            FloatValue = NTConfig.Get(id, Convert.ToSingle(entry.Default))
                        };

                        Scalar.OnValueChanged += input =>
                        {
                            NTConfig.Set(id, input.FloatValue);
                        };

                        if (entry.Resettable)
                        {
                            var ResetButton = new GUIButton(new RectTransform(new Vector2(0.1f, 1f), Scalar.RectTransform), style: "GUIButtonRefresh")
                            {
                                ToolTip = TextManager.Get("ntgui_resetbutton_tooltip")
                            };

                            ResetButton.OnClicked += (_, _) =>
                            {
                                float def = Convert.ToSingle(entry.Default);
                                Scalar.FloatValue = def;
                                NTConfig.Set(id, def);
                                return true;
                            };
                        }

                        break;
                    }

                case ConfigEntryType.Bool:
                    {
                        var TickBox = new GUITickBox(new RectTransform(new Vector2(0.5f, 0.05f), list.Content.RectTransform), entry.Name);

                        if (!entry.Description.IsNullOrWhiteSpace())
                        {
                            TickBox.ToolTip = entry.Description;
                        }

                        TickBox.Selected = NTConfig.Get(id, false);

                        TickBox.OnSelected += tb =>
                        {
                            NTConfig.Set(id, tb.Selected);
                            return true;
                        };

                        break;
                    }

                case ConfigEntryType.String:
                    {
                        string styleSuffix = entry.Style.IsNullOrWhiteSpace() ? string.Empty : $" ({entry.Style})";

                        var Label = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), list.Content.RectTransform), $"{entry.Name}{styleSuffix}")
                        {
                            CanBeFocused = false,
                            TextAlignment = Alignment.Center,
                            Wrap = true,
                            AutoScaleHorizontal = true
                        };

                        if (!entry.Description.IsNullOrWhiteSpace())
                        {
                            Label.ToolTip = entry.Description;
                            Label.CanBeFocused = true;
                        }

                        float Boxsize = entry.Boxsize > 0f ? entry.Boxsize : 0.08f;
                        string value = GUIComponents.GetStringValue(id, entry);

                        GUITextBox input;

                        if (entry.NoMLTB)
                        {
                            input = new GUITextBox(new RectTransform(new Vector2(1f, Boxsize), list.Content.RectTransform));
                            input.Text = value;
                        }
                        else
                        {
                            input = GUIComponents.CreateMultiLineTextBox(list.Content.RectTransform, value, Boxsize);
                        }

                        input.OnTextChanged += (textBox, text) =>
                        {
                            if (entry.Value is List<string> || entry.Default is List<string>)
                            {
                                NTConfig.Set(id, text.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                            }
                            else
                            {
                                NTConfig.Set(id, text);
                            }
                            return true;
                        };

                        if (entry.Resettable)
                        {
                            var ResetButton = new GUIButton(new RectTransform(new Vector2(0.1f, 1f), input.RectTransform), style: "GUIButtonRefresh")
                            {
                                ToolTip = TextManager.Get("ntgui_resetbutton_tooltip")
                            };

                            ResetButton.OnClicked += (_, _) =>
                            {
                                var defObj = entry.Default;

                                if (defObj is List<string> dl)
                                {
                                    input.Text = string.Join(",", dl);
                                    NTConfig.Set(id, dl);
                                }
                                else if (defObj is string ds)
                                {
                                    input.Text = ds;
                                    NTConfig.Set(id, ds);
                                }
                                else
                                {
                                    string def = defObj?.ToString() ?? "";
                                    input.Text = def;
                                    NTConfig.Set(id, def);
                                }

                                return true;
                            };
                        }
                        break;
                    }
            }
        }
    }



    public static class GUIComponents
    {
        // Save & Exit Button.
        public static GUIButton CreateSaveExitButton(GUILayoutGroup Parent, GUIFrame Container)
        {
            var Button = new GUIButton(new RectTransform(new Vector2(0.32f, 1f), parent: Parent.RectTransform), text: TextManager.Get("ntgui_configmenubutton_saveexit"));

            Button.OnClicked = (_, _) =>
            {
                if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
                {
                    Client? Client = GameMain.Client?.MyClient;

                    if (Client != null && Client.HasPermission(ClientPermissions.ManageSettings))
                    {
                        NTConfig.SendConfig();
                    }
                }
                else
                {
                    NTConfig.SaveConfig();
                }

                Container.Parent.RemoveChild(Container);
                return true;
            };

            return Button;
        }

        // Discard & Exit Button.
        public static GUIButton CreateDiscardExitButton(GUILayoutGroup Parent, GUIFrame Container)
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
        public static GUIButton CreateResetButton(GUILayoutGroup Parent, GUIFrame Container)
        {
            var Button = new GUIButton(new RectTransform(new Vector2(0.32f, 1f), parent: Parent.RectTransform), text: TextManager.Get("ntgui_configmenubutton_resetvalues"));

            Button.OnClicked = (_, _) =>
            {
                bool AllowedToReset = !HF.GameIsMultiplayer() || (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && GameMain.Client?.MyClient != null && GameMain.Client.MyClient.HasPermission(ClientPermissions.ManageSettings));

                if (!AllowedToReset)
                {
                    return true;
                }

                ResetMessage(Container);

                return true;
            };

            return Button;
        }

        // Show a warning message after clicking the Reset Button.
        private static void ResetMessage(GUIFrame Container)
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

                if (HF.GameIsMultiplayer() && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && GameMain.Client?.MyClient != null && GameMain.Client.MyClient.HasPermission(ClientPermissions.ManageSettings))
                {
                    NTConfig.SendConfig();
                }
                else if (!HF.GameIsMultiplayer())
                {
                    NTConfig.SaveConfig();
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
        public static void CreateButtonRow(GUILayoutGroup Parent, GUIFrame Container)
        {
            CreateSaveExitButton(Parent, Container);
            CreateDiscardExitButton(Parent, Container);
            CreateResetButton(Parent, Container);
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

        // Format a string of text
        private static string FormatList(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            return string.Join(", ", text.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)));
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
}