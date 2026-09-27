using Neurotrauma.ClientSource;

namespace Neurotrauma.ClientSource
{
    internal class ButtonData
    {
        public string Identifier = "";
        public LocalizedString Tooltip = "";
        public LocalizedString DisabledTooltip = "";
        public string StyleName = "";
        public string? ItemIdentifier;
        public string? RequiredItemIdentifier;
        public LimbType? RequiredLimb;
        public List<ButtonData>? SubButtons;
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