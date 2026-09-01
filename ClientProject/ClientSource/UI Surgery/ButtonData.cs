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
            RequiredItemIdentifier = "antidama1" // TODO: Change to Surgery Kit item! Same for all below here
        },

        // Hemostat
        new ButtonData
        {
            Identifier = "hemostat",
            Tooltip = TextManager.Get("nt_huibutton_enabled_hemostat"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonHemostat",
            ItemIdentifier = "advhemostat",
            RequiredItemIdentifier = "antidama1"
        },

        // Skin Retractors
        new ButtonData
        {
            Identifier = "skinretractors",
            Tooltip = TextManager.Get("nt_huibutton_enabled_skinretractors"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonSkinRetractors",
            ItemIdentifier = "advretractors",
            RequiredItemIdentifier = "antidama1"
        },

        // Surgical Drill
        new ButtonData
        {
            Identifier = "surgicaldrill",
            Tooltip = TextManager.Get("nt_huibutton_enabled_surgicaldrill"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonSurgicalDrill",
            ItemIdentifier = "surgicaldrill",
            RequiredItemIdentifier = "antidama1"
        },

        // Surgery Saw
        new ButtonData
        {
            Identifier = "surgerysaw",
            Tooltip = TextManager.Get("nt_huibutton_enabled_surgerysaw"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonSurgicalSaw",
            ItemIdentifier = "surgerysaw",
            RequiredItemIdentifier = "antidama1"
        },

        // Tweezers
        new ButtonData
        {
            Identifier = "tweezers",
            Tooltip = TextManager.Get("nt_huibutton_enabled_tweezers"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonTweezers",
            ItemIdentifier = "tweezers",
            RequiredItemIdentifier = "antidama1"
        },

        // Trauma Shears
        new ButtonData
        {
            Identifier = "traumashears",
            Tooltip = TextManager.Get("nt_huibutton_enabled_traumashears"),
            DisabledTooltip = TextManager.Get("nt_huibutton_disabled_surgerykit"),
            StyleName = "GUIButtonTraumaShears",
            ItemIdentifier = "traumashears",
            RequiredItemIdentifier = "antidama1"
        }
    };
}