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
                            Name = TextManager.Get("ntconfigname_header1"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_dislocationChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_dislocationchance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_fractureChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_fracturechance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_pneumothoraxChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_pneumothoraxchance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_tamponadeChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_tamponadechance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_heartattackChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_heartattackchance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_strokeChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_strokechance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_infectionRate"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_infectionrate"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_SepsisRate"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_sepsisrate"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_CPRFractureChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_cprfracturechance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_traumaticAmputationChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_traumaticamputationchance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_neurotraumaGain"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_neurotraumagain"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_organDamageGain"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_organdamagegain"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_fibrillationSpeed"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_fibrillationspeed"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_gangrenespeed"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_gangrenespeed"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamageCeiling"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_falldamageceiling"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamage"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_falldamage"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamageSeriousInjuryChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_falldamageseriousinjurychance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_Calculations"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_calculations"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_calculations"),
                        },

                        ["NT_vanillaSkillCheck"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_vanillaskillcheck"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_vanillaskillcheck"),
                        },

                        ["NT_disableBotAlgorithms"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_disablebotalgorithms"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_disablebotalgorithms"),
                        },

                        ["NT_screams"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_screams"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_screams"),
                        },

                        ["NT_organRejection"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_organrejection"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_organrejection"),
                        },

                        ["NT_fracturesRemoveCasts"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_fracturesremovecasts"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_fracturesremovecasts"),
                        },

                        ["NTCRE_ConsentRequiredExtra"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_consentrequiredextra"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_consentrequiredextra"),
                        },

                        ["NT_creatureNoFallDamage"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_creaturenofalldamage"),
                            Default = new List<string>
                            {
                                "Mudraptor",
                                "Mudraptor_unarmored",
                                "Mudraptor_veteran",
                                "Spineling_giant",
                            },
                            Style = TextManager.Get("ntconfigstyle_creaturenofalldamage"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfigdescription_creaturenofalldamage"),
                        },

                        ["NTSCAN_header1"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_header2"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NTSCAN_enablecoloredscanner"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_enablecoloredscanner"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_enablecoloredscanner"),
                        },

                        ["NTSCAN_lowmedThreshold"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_lowmedthreshold"),
                            Default = 25f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Description = TextManager.Get("ntconfigdescription_lowmedthreshold"),
                            Group = true,
                        },

                        ["NT_medhighThreshold"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_medhighthreshold"),
                            Default = 65f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Description = TextManager.Get("ntconfigdescription_medhighthreshold"),
                            Group = true,
                        },

                        ["NTSCAN_basecolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_basecolor"),
                            Default = new List<string> { "100,100,200" },
                            Style = TextManager.Get("ntconfigstyle_basecolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfigdescription_basecolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_namecolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_namecolor"),
                            Default = new List<string> { "125,125,225" },
                            Style = TextManager.Get("ntconfigstyle_namecolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfigdescription_namecolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_lowcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_lowcolor"),
                            Default = new List<string> { "100,200,100" },
                            Style = TextManager.Get("ntconfigstyle_lowcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfigdescription_lowcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_medcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_medcolor"),
                            Default = new List<string> { "200,200,100" },
                            Style = TextManager.Get("ntconfigstyle_medcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfigdescription_medcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_highcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_highcolor"),
                            Default = new List<string> { "250,100,100" },
                            Style = TextManager.Get("ntconfigstyle_highcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfigdescription_highcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_vitalcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_vitalcolor"),
                            Default = new List<string> { "255,0,0" },
                            Style = TextManager.Get("ntconfigstyle_vitalcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfigdescription_vitalcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_removalcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_removalcolor"),
                            Default = new List<string> { "0,255,255" },
                            Style = TextManager.Get("ntconfigstyle_removalcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfigdescription_removalcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_customcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_customcolor"),
                            Default = new List<string> { "180,50,200" },
                            Style = TextManager.Get("ntconfigstyle_customcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfigdescription_customcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_VitalCategory"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_vitalcategory"),
                            Default = new List<string>
                            {
                                "cardiacarrest",
                                "arterialcut",
                                "carotidarterialcut",
                                "aorticrupture",
                                "tra_amputation",
                                "tla_amputation",
                                "trl_amputation",
                                "tll_amputation",
                                "th_amputation",
                            },
                            Style = TextManager.Get("ntconfigstyle_vitalcategory"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfigdescription_vitalcategory"),
                        },

                        ["NTSCAN_RemovalCategory"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_removalcategory"),
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
                            Style = TextManager.Get("ntconfigstyle_removalcategory"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfigdescription_removalcategory"),
                        },

                        ["NTSCAN_CustomCategory"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_customcategory"),
                            Default = new List<string> { "" },
                            Style = TextManager.Get("ntconfigstyle_customcategory"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfigdescription_customcategory"),
                        },

                        ["NTSCAN_IgnoredCategory"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_ignoredcategory"),
                            Default = new List<string> { "" },
                            Style = TextManager.Get("ntconfigstyle_ignoredcategory"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfigdescription_ignoredcategory"),
                        },

                        ["NT_DEBUG_MODE"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_debugmode"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_debugmode"),
                        },

                        // ================================= COMMON ITEMS ========================================
                        ["NT_ItemPriceHeaderFirstAid"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_header3"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_antidama1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antidama1"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_gypsum"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_gypsum"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_suture"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_suture"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_tourniquet"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_tourniquet"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_needle"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_needle"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_drainage"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_drainage"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_gelipack"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_gelipack"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_ointment"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_ointment"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antibleeding1"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding2"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antibleeding2"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bloodpacks"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_bloodpacks"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_emptybloodpack"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_emptybloodpack"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_osteosynthesisimplants"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_osteosynthesisimplants"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_spinalimplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_spinalimplant"),
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
                            Name = TextManager.Get("ntconfigname_header4"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_arms"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_arms"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_legs"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_legs"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bionicarms"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_bionicarms"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bioniclegs"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_bioniclegs"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_livertransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_livertransplant"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_lungtransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_lungtransplant"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_kidneytransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_kidneytransplant"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_hearttransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_hearttransplant"),
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
                            Name = TextManager.Get("ntconfigname_header5"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_healthscanner"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_healthscanner"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bloodanalyzer"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_bloodanalyzer"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_defibrillator"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_defibrillator"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_aed"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_aed"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bvm"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_bvm"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_autocpr"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_autocpr"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_organcrate"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_organcrate"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_organtoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_organtoolbox"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_medtoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_medtoolbox"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerytoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_surgerytoolbox"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerytoolboxset"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_surgerytoolboxset"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_medstartercrate"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_medstartercrate"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bodybag"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_bodybag"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_stasisbag"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_stasisbag"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_wheelchair"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_wheelchair"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_analgesictank"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_analgesictank"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_toxfilter"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_toxfilter"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_dialyzer"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_dialyzer"),
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
                            Name = TextManager.Get("ntconfigname_header6"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_antibloodloss1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antibloodloss1"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_opium"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_opium"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antidama2"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antidama2"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_ringerssolution"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_ringerssolution"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_mannitol"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_mannitol"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_immunosuppressant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_immunosuppressant"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_thiamine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_thiamine"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_streptokinase"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_streptokinase"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antinarc"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antinarc"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibiotics"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antibiotics"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_adrenaline"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_adrenaline"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_liquidoxygenite"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_liquidoxygenite"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_deusizine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_deusizine"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding3"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antibleeding3"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_meth"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_meth"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_hyperzine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_hyperzine"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antipsychosis"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antipsychosis"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antiparalysis"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_antiparalysis"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_nitroglycerin"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_nitroglycerin"),
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
                            Name = TextManager.Get("ntconfigname_header7"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_advhemostat"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_advhemostat"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_advretractors"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_advretractors"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgicaldrill"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_surgicaldrill"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerysaw"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_surgerysaw"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_tweezers"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_tweezers"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_traumashears"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_traumashears"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_advscalpel"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfigname_itemprice_advscalpel"),
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
                            Name = TextManager.Get("ntconfigname_header8"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_OsteoImplants_uses"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfigname_osteoimplants_uses"),
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
                            Name = TextManager.Get("ntconfigname_spinalimplants_uses"),
                            Default = 1f,
                            Range = new float[] { 0.99f, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_HardmodeAorticRupture"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = TextManager.Get("ntconfigname_hardmodeaorticrupture"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_hardmodeaorticrupture"),
                        },

                        ["NT_OpenCloseTamponade"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = TextManager.Get("ntconfigname_openclosetamponade"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_openclosetamponade"),
                        },

                        ["NT_DoNitroprusside"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = TextManager.Get("ntconfigname_donitroprusside"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_donitroprusside"),
                        },

                        ["NT_DoOrganScalpels"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = TextManager.Get("ntconfigname_doorganscalpels"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_doorganscalpels"),
                        },
                    }
                }
            );
        }
    }
}