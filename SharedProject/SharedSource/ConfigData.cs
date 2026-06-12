namespace Neurotrauma
{
    public static class NTConfigData
    {
        public static void Register()
        {
            NTConfig.AddConfigOptions(
                new ConfigExpansion
                {
                    Name = "Neurotrauma",
                    ConfigData = new Dictionary<string, ConfigEntry>
                    {
                        ["NT_header1"] = new ConfigEntry
                        {
                            Name = "Neurotrauma",
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_dislocationChance"] = new ConfigEntry
                        {
                            Name = "Dislocation chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_fractureChance"] = new ConfigEntry
                        {
                            Name = "Fracture chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_pneumothoraxChance"] = new ConfigEntry
                        {
                            Name = "Pneumothorax chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_tamponadeChance"] = new ConfigEntry
                        {
                            Name = "Tamponade chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_heartattackChance"] = new ConfigEntry
                        {
                            Name = "Heart attack chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_strokeChance"] = new ConfigEntry
                        {
                            Name = "Stroke chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_infectionRate"] = new ConfigEntry
                        {
                            Name = "Infection rate multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_SepsisRate"] = new ConfigEntry
                        {
                            Name = "Sepsis rate multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_CPRFractureChance"] = new ConfigEntry
                        {
                            Name = "CPR fracture chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_traumaticAmputationChance"] = new ConfigEntry
                        {
                            Name = "Traumatic amputation chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_neurotraumaGain"] = new ConfigEntry
                        {
                            Name = "Neurotrauma gain rate",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_organDamageGain"] = new ConfigEntry
                        {
                            Name = "Organ damage gain rate",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_fibrillationSpeed"] = new ConfigEntry
                        {
                            Name = "Fibrillation gain rate",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_gangrenespeed"] = new ConfigEntry
                        {
                            Name = "Gangrene gain rate",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamageCeiling"] = new ConfigEntry
                        {
                            Name = "Maximum fall damage multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamage"] = new ConfigEntry
                        {
                            Name = "Falldamage multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamageSeriousInjuryChance"] = new ConfigEntry
                        {
                            Name = "Falldamage serious injury chance multiplier",
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_Calculations"] = new ConfigEntry
                        {
                            Name = "Character calculations",
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = "Runs calculations that are necessary for the functionality of the mod. Shouldn't be disabled unless there is borderline unplayable desynchronisation and lag, in which case it might help with a bit.",
                        },

                        ["NT_vanillaSkillCheck"] = new ConfigEntry
                        {
                            Name = "Vanilla skill check formula",
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = "Changes the chance to succeed a lua skillcheck from skill/requiredskill to 100-(requiredskill-skill))/100 .",
                        },

                        ["NT_disableBotAlgorithms"] = new ConfigEntry
                        {
                            Name = "Disable bot treatment algorithms",
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = "Prevents bots from attempting to treat afflictions.\nThis is desireable, because bots suck at treating things for the current moment.",
                        },

                        ["NT_screams"] = new ConfigEntry
                        {
                            Name = "Screams",
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = "Characters scream when in pain.",
                        },

                        ["NT_ignoreModConflicts"] = new ConfigEntry
                        {
                            Name = "Ignore mod conflicts",
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = "Prevent the mod conflict affliction from showing up.",
                        },

                        ["NT_organRejection"] = new ConfigEntry
                        {
                            Name = "Organ rejection",
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = "When transplanting an organ, there is a chance that the organ gets rejected.\nThe higher the patients immunity at the time of the transplant, the higher the chance.",
                        },

                        ["NT_fracturesRemoveCasts"] = new ConfigEntry
                        {
                            Name = "Fractures remove casts",
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = "When receiving damage that would cause a fracture, remove plaster casts on the limb",
                        },

                        ["NTCRE_ConsentRequiredExtra"] = new ConfigEntry
                        {
                            Name = "NPCs consent requirement to medical interactions",
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = "Integrated consent required mod.\nIf enabled, NPCs outside of your team or submarine mission will get aggravated by medical interactions.",
                        },

                        ["NT_creatureNoFallDamage"] = new ConfigEntry
                        {
                            Name = "Excluded creatures that abuse the fall damage mechanic",
                            Default = new List<string>
                            {
                                "Mudraptor",
                                "Mudraptor_unarmored",
                                "Mudraptor_veteran",
                                "Spineling_giant",
                            },
                            Style = "SpeciesName,SpeciesName",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = "An abuse of fall damage is commonly shown by creatures with heavy or ridicilous knockback, that at worst will instakill or stunlock you.\nYou can add or remove creatures to customize this list to your liking. Use debug command `nt_listcreatures` to list the SpeciesName of the creature you are patching in your game.\nReport other creatures that abuse fall damage to the discord server to improve this default list.",
                        },

                        ["NTSCAN_header1"] = new ConfigEntry
                        {
                            Name = "Scanner Settings",
                            Type = ConfigEntryType.Category,
                        },

                        ["NTSCAN_enablecoloredscanner"] = new ConfigEntry
                        {
                            Name = "Colored Scanner",
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = "Enable colored health scanner text messages.",
                        },

                        ["NTSCAN_lowmedThreshold"] = new ConfigEntry
                        {
                            Name = "Low-Medium Text Threshold",
                            Default = 25f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Description = "Where the Low progress color ends and Medium progress color begins.",
                            Group = true,
                        },

                        ["NT_medhighThreshold"] = new ConfigEntry
                        {
                            Name = "Medium-High Text Threshold",
                            Default = 65f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Description = "Where the Medium progress color ends and High progress color begins.",
                            Group = true,
                        },

                        ["NTSCAN_basecolor"] = new ConfigEntry
                        {
                            Name = "Base Text Color",
                            Default = new List<string> { "100,100,200" },
                            Style = "R,G,B",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = "Scanner text color.",
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_namecolor"] = new ConfigEntry
                        {
                            Name = "Name Text Color",
                            Default = new List<string> { "125,125,225" },
                            Style = "R,G,B",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = "Scanner text color for player names.",
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_lowcolor"] = new ConfigEntry
                        {
                            Name = "Low Priority Color",
                            Default = new List<string> { "100,200,100" },
                            Style = "R,G,B",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = "Scanner text color for afflictions that have low progress.",
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_medcolor"] = new ConfigEntry
                        {
                            Name = "Medium Priority Color",
                            Default = new List<string> { "200,200,100" },
                            Style = "R,G,B",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = "Scanner text color for afflictions that have medium progress.",
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_highcolor"] = new ConfigEntry
                        {
                            Name = "High Priority Color",
                            Default = new List<string> { "250,100,100" },
                            Style = "R,G,B",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = "Scanner text color for afflictions that have high progress.",
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_vitalcolor"] = new ConfigEntry
                        {
                            Name = "Vital Priority Color",
                            Default = new List<string> { "255,0,0" },
                            Style = "R,G,B",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = "Scanner text color for vital afflictions (Arterial bleed, Traumatic amputation).",
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_removalcolor"] = new ConfigEntry
                        {
                            Name = "Removed Organ Color",
                            Default = new List<string> { "0,255,255" },
                            Style = "R,G,B",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = "Scanner text color for removed organs (Heart removed, leg amputation).",
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_customcolor"] = new ConfigEntry
                        {
                            Name = "Custom Category Color",
                            Default = new List<string> { "180,50,200" },
                            Style = "R,G,B",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = "Scanner text color for the custom category.",
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_VitalCategory"] = new ConfigEntry
                        {
                            Name = "Included Vital Afflictions",
                            Default = new List<string>
                            {
                                "cardiacarrest",
                                "ll_arterialcut",
                                "rl_arterialcut",
                                "la_arterialcut",
                                "ra_arterialcut",
                                "t_arterialcut",
                                "h_arterialcut",
                                "tra_amputation",
                                "tla_amputation",
                                "trl_amputation",
                                "tll_amputation",
                                "th_amputation",
                            },
                            Style = "identifier,identifier",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = "You can add or remove afflictions to customize this list to your liking.",
                        },

                        ["NTSCAN_RemovalCategory"] = new ConfigEntry
                        {
                            Name = "Included Removal Affictions",
                            Default = new List<string>
                            {
                                "heartremoved",
                                "brainremoved",
                                "lungremoved",
                                "kidneyremoved",
                                "liverremoved",
                                "sra_amputation",
                                "sla_amputation",
                                "srl_amputation",
                                "sll_amputation",
                                "sh_amputation",
                            },
                            Style = "identifier, identifier",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = "You can add or remove afflictions to customize this list to your liking.",
                        },

                        ["NTSCAN_CustomCategory"] = new ConfigEntry
                        {
                            Name = "Custom Affliction Category",
                            Default = new List<string> { "" },
                            Style = "identifier,identifier",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = "You can add or remove afflictions to customize this list to your liking.",
                        },

                        ["NTSCAN_IgnoredCategory"] = new ConfigEntry
                        {
                            Name = "Ignored Afflictions",
                            Default = new List<string> { "" },
                            Style = "identifier,identifier",
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = "Afflictions added to this category will be ignored by the health scanner.",
                        },

                        // ================================= COMMON ITEMS ========================================
                        ["NT_ItemPriceHeaderFirstAid"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Common Item Price Multipliers",
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_antidama1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Morphine",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_gypsum"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Gypsum",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_suture"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Suture",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_tourniquet"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Tourniquet",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_needle"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Needle",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_drainage"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Drainage",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_gelipack"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Gel Coolant Pack",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_ointment"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Antibiotic Ointment",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Bandage",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding2"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Plastiseal",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bloodpacks"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Blood Packs",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_emptybloodpack"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Empty Blood Pack",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_osteosynthesisimplants"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Osteosynthesis Implants",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_spinalimplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Spinal Cord Implants",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        // ================================= BODY PARTS ========================================
                        ["NT_ItemPriceHeaderBodyParts"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Bodypart Price Multipliers",
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_arms"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Arms",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_legs"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Legs",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bionicarms"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Bionic Arms",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bioniclegs"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Bionic Legs",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_livertransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Liver Transplant",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_lungtransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Lung Transplant",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_kidneytransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Kidney Transplant",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_hearttransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Heart Transplant",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        // ================================= GEAR ========================================
                        ["NT_ItemPriceHeaderGear"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Gear Price Multipliers",
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_healthscanner"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Health Scanner",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bloodanalyzer"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Hematology Analyzer",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_defibrillator"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Manual Defibrillator",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_aed"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Automated External Defibrillator",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bvm"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "BVM",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_autocpr"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "AutoPulse",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_organcrate"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Refrigerated Crate",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_organtoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Refrigerated Container",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_medtoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Medical Container",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerytoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Surgery Toolbox",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerytoolboxset"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Surgery Toolbox (Kit)",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_medstartercrate"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Medical Starter Crate",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bodybag"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Bodybag",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_stasisbag"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Stasis Bag",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_wheelchair"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Wheelchair",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_analgesictank"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Analgesic Tank",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_toxfilter"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Toxin Filter",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_dialyzer"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Dialyzer",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        // ================================= OTHER MEDICINES ========================================
                        ["NT_ItemPriceHeaderMedicines"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Other Medicine Items Price Multipliers",
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_antibloodloss1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Saline",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_opium"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Opium",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antidama2"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Fentanyl",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_ringerssolution"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Ringer's Solution",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_mannitol"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Mannitol",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_immunosuppressant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Azathioprine",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_thiamine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Thiamine",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_streptokinase"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Streptokinase",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antinarc"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Naloxone",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibiotics"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Broad-Spectrum Antibiotics",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_adrenaline"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Adrenaline",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_liquidoxygenite"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Liquid Oxygenite",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_deusizine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Deusizine",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding3"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Antibiotic Glue",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_meth"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Methamphetamine",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_hyperzine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Hyperzine",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antipsychosis"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Haloperidol",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antiparalysis"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Anaparalyzant",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_nitroglycerin"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Nitroglycerin",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        // ================================= SURGERY TOOLS ========================================
                        ["NT_ItemPriceHeaderSurgeryTools"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Surgical Tools Price Multipliers",
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_advhemostat"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Hemostat",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_advretractors"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Retractors",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgicaldrill"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Surgical Drill",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerysaw"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Surgical Saw",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_tweezers"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Tweezers",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_traumashears"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Trauma Shears",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_advscalpel"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = "Multipurpose Scalpel ",
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        // ================================= DYNAMIC ITEM AVAILABILITY =================================
                        ["NT_ItemDurabilityHeader"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = "Change Item Use Amounts",
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_OsteoImplants_uses"] = new ConfigEntry
                        {
                            Name = "Osteosynthesis Implant Uses",
                            Page = "availability",
                            Default = 4f,
                            Range = new float[] { 1, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_SpinalImplants_uses"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = "Spinal Cord Implant Uses",
                            Default = 1f,
                            Range = new float[] { 0.99f, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_HardmodeAorticRupture"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = "Hardmode Aortic Rupture",
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = "Enable the Medical Stent and Aortic Balloon as the only method to fix Aortic Rupture.",
                        },

                        ["NT_OpenCloseTamponade"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = "Open Close Tamponade",
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = "Enable the closing with sutures as the only method to fix Cardiac Tamponade.",
                        },

                        ["NT_DoNitroprusside"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = "Enable Sodium Nitroprusside",
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = "Allow Sodium Nitroprusside to be fabricated, bought and found.",
                        },

                        ["NT_DoOrganScalpels"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = "Enable Organ Scalpels",
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = "Allow Organ scalpels and Surgery Box Scalpel set to be fabricated, bought and found.",
                        },
                    }
                }
            );
        }
    }
}