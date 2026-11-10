using Neurotrauma.ClientSource;

namespace Neurotrauma.ClientSource
{
    public class ButtonData
    {
        public string Identifier = "";
        public LocalizedString Tooltip = "";
        public LocalizedString DisabledTooltip = "";
        public string StyleName = "";
        public string? ItemIdentifier;
        public string? RequiredItemIdentifier;
        public LimbType? RequiredLimb;
        public List<ButtonData>? SubButtons;

        /// <summary>
        /// Find a button via their Identifier
        /// </summary>
        public static ButtonData? Find(string Identifier)
        {
            return FindIn(ButtonDefinitions.Entries, Identifier);
        }

        // SubButtons need a bit more looking
        private static ButtonData? FindIn(List<ButtonData>? List, string Identifier)
        {
            if (List == null) return null;

            foreach (var button in List)
            {
                if (button.Identifier == Identifier) return button;

                var nested = FindIn(button.SubButtons, Identifier);
                if (nested != null) return nested;
            }
            return null;
        }

        /// <summary>
        /// Adds a button. With no parent it becomes a new entry; otherwise it is added to the SubButtons of the button with that identifier.
        /// </summary>
        /// <param name="Button">The button to add</param>
        /// <param name="ParentIdentifier">Identifier of an existing button to add this under, or null for a new top-level entry</param>
        /// <param name="Position">Position of the button in the list; if empty just add it to the end</param>
        public static bool AddButton(ButtonData Button, string? ParentIdentifier = null, int? Position = null)
        {
            if (string.IsNullOrEmpty(Button.Identifier)) return false;

            // Identifiers must be unique
            if (Find(Button.Identifier) != null) return false;

            List<ButtonData> Target;

            if (ParentIdentifier == null)
            {
                Target = ButtonDefinitions.Entries;
            }
            else
            {
                var parent = Find(ParentIdentifier);
                if (parent == null) return false;

                parent.SubButtons ??= new List<ButtonData>();
                Target = parent.SubButtons;
            }

            if (Position.HasValue && Position.Value >= 0 && Position.Value <= Target.Count)
            {
                Target.Insert(Position.Value, Button);
            }
            else
            {
                Target.Add(Button);
            }

            return true;
        }

        /// <summary>
        /// Function for Lua addons to create new buttons.
        /// </summary>
        /// <param name="Identifier">Identifier of this button</param>
        /// <param name="StyleName">StyleName used for button icon</param>
        /// <param name="ItemIdentifier">ItemIdentifier of the item that should be used when pressed</param>
        /// <param name="RequiredItemIdentifier">ItemIdentifier of the required item before this button unlocks</param>
        /// <param name="ToolTipKey">Localization key for the ToolTip when enabled</param>
        /// <param name="DisabledToolTipKey">Localization key for the ToolTip when disabled</param>
        /// <param name="RequiredLimb">Limb that needs to be selected before button enbables</param>
        /// <returns></returns>
        public static ButtonData? Create(string Identifier, string StyleName, string? ItemIdentifier, string? RequiredItemIdentifier, string? ToolTipKey, string? DisabledToolTipKey, string? RequiredLimb)
        {
            LimbType? limb = null;
            if (!string.IsNullOrEmpty(RequiredLimb))
            {
                if (!Enum.TryParse<LimbType>(RequiredLimb, true, out var parsed)) return null;
                limb = parsed;
            }

            return new ButtonData
            {
                Identifier = Identifier,
                StyleName = StyleName,
                ItemIdentifier = ItemIdentifier,
                RequiredItemIdentifier = RequiredItemIdentifier,
                Tooltip = string.IsNullOrEmpty(ToolTipKey) ? "" : TextManager.Get(ToolTipKey),
                DisabledTooltip = string.IsNullOrEmpty(DisabledToolTipKey) ? "" : TextManager.Get(DisabledToolTipKey),
                RequiredLimb = limb
            };
        }

        /// <summary>
        /// Create and instantly add a new button to the list via Lua.
        /// </summary>
        /// <param name="ParentIdentifier">Identifier of the button we're adding SubButtons to. Leave empty for a truly new one.</param>
        public static bool Add(string Identifier, string StyleName, string? ItemIdentifier, string? RequiredItemIdentifier, string? ToolTipKey, string? DisabledToolTipKey, string? RequiredLimb, string? ParentIdentifier)
        {
            var button = Create(Identifier, StyleName, ItemIdentifier, RequiredItemIdentifier, ToolTipKey, DisabledToolTipKey, RequiredLimb);
            if (button == null) return false;

            return AddButton(button, string.IsNullOrEmpty(ParentIdentifier) ? null : ParentIdentifier);
        }
    }
}

internal static class ButtonDefinitions
{
    public static readonly List<ButtonData> Entries = new()
    {
        // Scalpel
        new ButtonData
        {
            Identifier = "scalpel",
            Tooltip = TextManager.Get("nt_huibutton_enabled_scalpel"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonScalpel",
            ItemIdentifier = "advscalpel",
            RequiredItemIdentifier = "surgerykit"
        },

        // Hemostat
        new ButtonData
        {
            Identifier = "hemostat",
            Tooltip = TextManager.Get("nt_huibutton_enabled_hemostat"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonHemostat",
            ItemIdentifier = "advhemostat",
            RequiredItemIdentifier = "surgerykit"
        },

        // Skin Retractors
        new ButtonData
        {
            Identifier = "skinretractors",
            Tooltip = TextManager.Get("nt_huibutton_enabled_skinretractors"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonSkinRetractors",
            ItemIdentifier = "advretractors",
            RequiredItemIdentifier = "surgerykit"
        },

        // Surgical Drill
        new ButtonData
        {
            Identifier = "surgicaldrill",
            Tooltip = TextManager.Get("nt_huibutton_enabled_surgicaldrill"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonSurgicalDrill",
            ItemIdentifier = "surgicaldrill",
            RequiredItemIdentifier = "surgerykit"
        },

        // Surgery Saw
        new ButtonData
        {
            Identifier = "surgerysaw",
            Tooltip = TextManager.Get("nt_huibutton_enabled_surgerysaw"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonSurgicalSaw",
            ItemIdentifier = "surgerysaw",
            RequiredItemIdentifier = "surgerykit"
        },

        // Tweezers
        new ButtonData
        {
            Identifier = "tweezers",
            Tooltip = TextManager.Get("nt_huibutton_enabled_tweezers"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonTweezers",
            ItemIdentifier = "tweezers",
            RequiredItemIdentifier = "surgerykit"
        },

        // Trauma Shears
        new ButtonData
        {
            Identifier = "traumashears",
            Tooltip = TextManager.Get("nt_huibutton_enabled_traumashears"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonTraumaShears",
            ItemIdentifier = "traumashears",
            RequiredItemIdentifier = "surgerykit"
        },

        // Surgery on Organs
        new ButtonData
        {
            Identifier = "organscalpel",
            Tooltip = TextManager.Get("nt_huibutton_enabled_organscalpel"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonOrganScalpel",
            RequiredItemIdentifier = "surgerykit",
            SubButtons = new List<ButtonData>
            {
                new ButtonData { Identifier = "surgery_kidney", StyleName = "GUIButtonKidney", ItemIdentifier = "organscalpel_kidneys", RequiredItemIdentifier = "surgerykit", RequiredLimb = LimbType.Torso },
                new ButtonData { Identifier = "surgery_liver",  StyleName = "GUIButtonLiver",  ItemIdentifier = "organscalpel_liver",   RequiredItemIdentifier = "surgerykit", RequiredLimb = LimbType.Torso },
                new ButtonData { Identifier = "surgery_lungs",  StyleName = "GUIButtonLungs",  ItemIdentifier = "organscalpel_lungs",   RequiredItemIdentifier = "surgerykit", RequiredLimb = LimbType.Torso },
                new ButtonData { Identifier = "surgery_heart",  StyleName = "GUIButtonHeart",  ItemIdentifier = "organscalpel_heart",   RequiredItemIdentifier = "surgerykit", RequiredLimb = LimbType.Torso },
                new ButtonData { Identifier = "surgery_brain",  StyleName = "GUIButtonBrain",  ItemIdentifier = "organscalpel_brain",   RequiredItemIdentifier = "surgerykit", RequiredLimb = LimbType.Head },
            }
        }
    };
}