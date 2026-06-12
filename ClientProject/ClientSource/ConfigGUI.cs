using Barotrauma.LuaCs.Compatibility;
using Barotrauma.Networking;
using Neurotrauma;
using System.Drawing;
using static Barotrauma.Items.Components.Fabricator;
using static Barotrauma.Networking.MessageFragment;
using static Barotrauma.PetBehavior.ItemProduction;
using Color = Microsoft.Xna.Framework.Color;

namespace Neurotrauma
{
    internal class ConfigurationMenu
    {
        private static readonly List<(string uiName, string id, string type)> ExpansionNameForUI = new();
        private static readonly List<(string name, string id)> BaseConfigPages = new()
        {
            ("Item Prices", "prices"),
            ("Item Availability", "availability")
        };

        // This is just what I did in Lua, but now via C#. - Lukako
        // We hook opening the Pause Menu, then force it to also render our Button to open the settings.
        public static void AddConfigToPauseMenu()
        {
            LuaCsSetup.Instance.Hook.Patch(
                "AddButtonToPauseMenu", // identifier for our Patch
                "Barotrauma.GUI",       // className
                "TogglePauseMenu",      // methodName
                Array.Empty<string>(),  // parameterTypes (this one has none)

                new LuaCsPatchFunc((object instance, LuaPatcherService.ParameterTable ptable) =>
                {
                    if (!GUI.PauseMenuOpen) return null;

                    GUIComponent frame = GUI.PauseMenu; // The actual menu screen
                    GUIComponent secondChild = frame.Children.Skip(1).First(); // The panel containing the elements
                    GUIComponent list = secondChild.Children.First(); // The buttons!

                    var btn = new GUIButton(
                        new RectTransform(new Vector2(1f, 0.1f), list.RectTransform),
                        TextManager.Get("ntgui_pausemenubutton_name"),
                        textAlignment: Alignment.Center,
                        style: "GUIButtonSmall");

                    btn.OnClicked = (_, _) =>
                    {
                        CreateConfigGUI(frame);
                        return true;
                    };

                    return null;
                }),
                ILuaCsHook.HookMethodType.After
            );
        }

        public static GUIListBox CreateConfigGUI(GUIComponent parent)
        {
            // GUI shape identical to Lua
            var menuContent = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.8f), parent.RectTransform, Anchor.Center));
            var mainLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), menuContent.RectTransform, Anchor.Center));
            var innerFrame = new GUIFrame(new RectTransform(new Vector2(1.0f, 0.95f), mainLayout.RectTransform), style: "InnerFrame");
            var innerLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), innerFrame.RectTransform, Anchor.TopCenter));
            var title = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.07f), innerLayout.RectTransform), TextManager.Get("ntgui_config_title"), font: GUIStyle.LargeFont)
            {
                TextAlignment = Alignment.TopCenter
            };

            var menuList = new GUIListBox(new RectTransform(new Vector2(1.0f, 0.97f), innerLayout.RectTransform))
            {
                Padding = new Vector4(10, 15, 10, 10)
            };

            var buttonRow = new GUILayoutGroup(new RectTransform(new Vector2(1.0f, 0.1f), mainLayout.RectTransform), isHorizontal: true)
            {
                RelativeSpacing = 0.02f
            };

            var dropdown = new GUIDropDown(new RectTransform(new Vector2(0.3f, 0.1f), title.RectTransform), "");

            PopulateDropdown(dropdown);
            dropdown.Select(0);

            var first = ExpansionNameForUI[dropdown.SelectedIndex];
            selectedExpansion = first.id;
            selectedType = first.type;

            PopulateSettings(menuList, selectedExpansion);

            dropdown.OnSelected += (_, __) =>
            {
                var sel = ExpansionNameForUI[dropdown.SelectedIndex];
                selectedExpansion = sel.id;
                selectedType = sel.type;

                PopulateSettings(menuList, selectedExpansion);
                return true;
            };

            GUIComponents.CreateButtonRow(buttonRow, menuContent);

            return menuList;
        }

        private static void PopulateDropdown(GUIDropDown dropdown)
        {
            // Pull Expansions and add them to the dropdown, putting NT + base NT pages on top.
            ExpansionNameForUI.Clear();

            foreach (var expansion in NTConfig.Expansions)
            {
                if (expansion.Name == "Neurotrauma")
                {
                    ExpansionNameForUI.Add((expansion.Name, expansion.Name, "expansion"));
                    dropdown.AddItem(expansion.Name);
                    break;
                }
            }

            foreach (var page in BaseConfigPages)
            {
                ExpansionNameForUI.Add((page.name, page.id, "page"));
                dropdown.AddItem(page.name);
            }

            foreach (var expansion in NTConfig.Expansions)
            {
                if (expansion.Name == "Neurotrauma") continue;

                var uiName = expansion.Name;
                if (!uiName.StartsWith("NT "))
                    uiName = "NT " + uiName;

                ExpansionNameForUI.Add((uiName, expansion.Name, "expansion"));
                dropdown.AddItem(uiName);
            }
        }

        private static string selectedExpansion = null;
        private static string selectedType = null;
        private class LayoutChunk
        {
            public string Type;
            public string Key;
            public ConfigEntry Entry;
            public List<(string key, ConfigEntry entry)> Items;
        }

        // Automatically combined the Grouped settings
        private static List<LayoutChunk> PrebuildConfigLayout(Dictionary<string, ConfigEntry> entries, string selectedId, string selectedType)
        {
            var result = new List<LayoutChunk>();

            LayoutChunk currentGroup = null;
            ConfigEntry lastEntry = null;

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

            new GUITextBlock(new RectTransform(new Vector2(1, 0.1f), list.Content.RectTransform), TextManager.Get("ntgui_defaultmessage_config"), font: GUIStyle.SubHeadingFont)
            {
                CanBeFocused = false
            };

            var layout = PrebuildConfigLayout(NTConfig.Entries, selectedExpansion, selectedType);

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

            Client client = GameMain.Client?.MyClient;

            if (client == null || !(client.IsOwner || client.HasPermission(ClientPermissions.ManageSettings)))
            {
                foreach (GUIComponent c in list.GetAllChildren())
                    c.Enabled = false;
            }
        }

        // Create grouped Floats
        private static void CreateFloatGroup(GUIListBox list, List<(string key, ConfigEntry entry)> items)
        {
            const int MaxPerRow = 2;
            GUILayoutGroup row = null;
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
            GUILayoutGroup row = null;
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
                var Label = entry.Name + (string.IsNullOrEmpty(entry.Style) ? "" : $" ({entry.Style})");
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

                        if (!string.IsNullOrWhiteSpace(entry.Description))
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

                        if (!string.IsNullOrWhiteSpace(entry.Description))
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
                        string styleSuffix = string.IsNullOrWhiteSpace(entry.Style) ? string.Empty : $" ({entry.Style})";

                        var Label = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), list.Content.RectTransform), $"{entry.Name}{styleSuffix}")
                        {
                            CanBeFocused = false,
                            TextAlignment = Alignment.Center,
                            Wrap = true,
                            AutoScaleHorizontal = true
                        };

                        if (!string.IsNullOrWhiteSpace(entry.Description))
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
        // SaveExit + CloseExit + Reset Buttons
        public static GUIButton CreateButtonRow(GUILayoutGroup parent, GUIFrame container)
        {
            var save = new GUIButton(new RectTransform(new Vector2(0.32f, 1f), parent.RectTransform), TextManager.Get("ntgui_configmenubutton_saveexit"));
            save.OnClicked = (_, _) =>
            {
                if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
                {
                    Client client = GameMain.Client?.MyClient;
                    if (client != null && client.HasPermission(ClientPermissions.ManageSettings))
                    {
                        NTConfig.SendConfig();
                    }
                }
                else
                {
                    NTConfig.SaveConfig();
                }

                container.Parent.RemoveChild(container);
                return true;
            };

            var discard = new GUIButton(new RectTransform(new Vector2(0.32f, 1f), parent.RectTransform), TextManager.Get("ntgui_configmenubutton_discardexit"));
            discard.OnClicked = (_, _) =>
            {
                container.Parent.RemoveChild(container);
                return true;
            };

            var reset = new GUIButton(new RectTransform(new Vector2(0.32f, 1f), parent.RectTransform), TextManager.Get("ntgui_configmenubutton_resetvalues"));
            reset.OnClicked = (_, _) =>
            {
                NTConfig.ResetConfig();
                container.Parent.RemoveChild(container);
                return true;
            };

            return save;
        }

        // This was fucking miserable
        public static GUITextBox CreateMultiLineTextBox(RectTransform parent, string text = "", float size = 0.2f)
        {
            var listBox = new GUIListBox(
                new RectTransform(new Vector2(1f, size), parent));

            var textBox = new GUITextBox(
                new RectTransform(new Vector2(1f, 1f), listBox.Content.RectTransform),
                FormatList(text),
                textColor: null,
                font: null,
                textAlignment: Alignment.Left,
                wrap: true,
                style: "GUITextBoxNoBorder");

            textBox.OnSelected += (_, _) =>
            {
                float caretY = textBox.CaretScreenPos.Y;
                float bottomCaretExtent = textBox.Font.LineHeight * 1.5f;
                float topCaretExtent = -textBox.Font.LineHeight * 0.5f;

                if (caretY + bottomCaretExtent > listBox.Rect.Bottom)
                {
                    listBox.ScrollBar.BarScroll =
                        (caretY - textBox.Rect.Top - listBox.Rect.Height + bottomCaretExtent)
                        / (textBox.Rect.Height - listBox.Rect.Height);
                }
                else if (caretY + topCaretExtent < listBox.Rect.Top)
                {
                    listBox.ScrollBar.BarScroll =
                        (caretY - textBox.Rect.Top + topCaretExtent)
                        / (textBox.Rect.Height - listBox.Rect.Height);
                }
            };

            textBox.OnTextChanged += (_, __) =>
            {
                Vector2 textSize = textBox.Font.MeasureString(textBox.WrappedText);

                textBox.RectTransform.NonScaledSize =
                    new Microsoft.Xna.Framework.Point(
                        textBox.RectTransform.NonScaledSize.X,
                        (int)Math.Max(listBox.Content.Rect.Height, textSize.Y + 10));

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

        private static string FormatList(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            return string.Join(", ",
                text.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        public static string GetStringValue(string key, ConfigEntry entry)
        {
            var storedObj = NTConfig.Get<object>(key, null);

            if (storedObj is List<string> list)
                return string.Join(", ", list);

            if (storedObj is string s)
                return FormatList(s);

            if (entry.Value is List<string> v)
                return string.Join(", ", v);

            if (entry.Default is List<string> d)
                return string.Join(", ", d);

            if (entry.Default is string ds)
                return FormatList(ds);

            return "";
        }
    }
}