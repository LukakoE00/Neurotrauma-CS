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
                        ["NT_dislocationChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.dislocationchance"),
                            Description = TextManager.Get("ntconfig.entrydescription.dislocationchance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_fractureChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.fracturechance"),
                            Description = TextManager.Get("ntconfig.entrydescription.fracturechance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_pneumothoraxChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.pneumothoraxchance"),
                            Description = TextManager.Get("ntconfig.entrydescription.pneumothoraxchance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_tamponadeChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.tamponadechance"),
                            Description = TextManager.Get("ntconfig.entrydescription.tamponadechance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_heartattackChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.heartattackchance"),
                            Description = TextManager.Get("ntconfig.entrydescription.heartattackchance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_strokeChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.strokechance"),
                            Description = TextManager.Get("ntconfig.entrydescription.strokechance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_infectionRate"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.infectionrate"),
                            Description = TextManager.Get("ntconfig.entrydescription.infectionrate"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_SepsisRate"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.sepsisrate"),
                            Description = TextManager.Get("ntconfig.entrydescription.sepsisrate"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_CPRFractureChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.cprfracturechance"),
                            Description = TextManager.Get("ntconfig.entrydescription.cprfracturechance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_traumaticAmputationChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.traumaticamputationchance"),
                            Description = TextManager.Get("ntconfig.entrydescription.traumaticamputationchance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_neurotraumaGain"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.neurotraumagain"),
                            Description = TextManager.Get("ntconfig.entrydescription.neurotraumagain"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_organDamageGain"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.organdamagegain"),
                            Description = TextManager.Get("ntconfig.entrydescription.organdamagegain"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_fibrillationSpeed"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.fibrillationspeed"),
                            Description = TextManager.Get("ntconfig.entrydescription.fibrillationspeed"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_gangrenespeed"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.gangrenespeed"),
                            Description = TextManager.Get("ntconfig.entrydescription.gangrenespeed"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamageCeiling"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.falldamageceiling"),
                            Description = TextManager.Get("ntconfig.entrydescription.falldamageceiling"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamage"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.falldamage"),
                            Description = TextManager.Get("ntconfig.entrydescription.falldamage"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_falldamageSeriousInjuryChance"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.falldamageseriousinjurychance"),
                            Description = TextManager.Get("ntconfig.entrydescription.falldamageseriousinjurychance"),
                            Default = 1f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_cl_DoHUIButtons"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.DoHUIButtons"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.DoHUIButtons"),
                            IsClientside = true,
                        },

                        ["NT_DoDeterministicBloodTypes"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.DoDeterministicBloodTypes"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.DoDeterministicBloodTypes"),
                        },

                        ["NT_Calculations"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.calculations"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.calculations"),
                        },

                        ["NT_vanillaSkillCheck"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.vanillaskillcheck"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.vanillaskillcheck"),
                        },

                        ["NT_disableBotAlgorithms"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.disablebotalgorithms"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.disablebotalgorithms"),
                        },

                        ["NT_screams"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.screams"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.screams"),
                        },

                        ["NT_organRejection"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.organrejection"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.organrejection"),
                        },

                        ["NT_fracturesRemoveCasts"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.fracturesremovecasts"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.fracturesremovecasts"),
                        },

                        ["NTCRE_ConsentRequiredExtra"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.consentrequiredextra"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.consentrequiredextra"),
                        },

                        ["NT_creatureNoFallDamage"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.creaturenofalldamage"),
                            Default = new List<string>
                            {
                                "Mudraptor",
                                "Mudraptor_unarmored",
                                "Mudraptor_veteran",
                                "Spineling_giant",
                            },
                            Style = TextManager.Get("ntconfig.style.creaturenofalldamage"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfig.entrydescription.creaturenofalldamage"),
                        },

                        ["NTSCAN_header1"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.header2"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NTSCAN_enablecoloredscanner"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.enablecoloredscanner"),
                            Default = true,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.enablecoloredscanner"),
                        },

                        ["NTSCAN_lowmedThreshold"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.lowmedthreshold"),
                            Default = 25f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Description = TextManager.Get("ntconfig.entrydescription.lowmedthreshold"),
                            Group = true,
                        },

                        ["NT_medhighThreshold"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.medhighthreshold"),
                            Default = 65f,
                            Range = new float[] { 0, 100 },
                            Type = ConfigEntryType.Float,
                            Description = TextManager.Get("ntconfig.entrydescription.medhighthreshold"),
                            Group = true,
                        },

                        ["NTSCAN_basecolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.basecolor"),
                            Default = new List<string> { "100,100,200" },
                            Style = TextManager.Get("ntconfig.style.basecolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfig.entrydescription.basecolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_namecolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.namecolor"),
                            Default = new List<string> { "125,125,225" },
                            Style = TextManager.Get("ntconfig.style.namecolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfig.entrydescription.namecolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_lowcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.lowcolor"),
                            Default = new List<string> { "100,200,100" },
                            Style = TextManager.Get("ntconfig.style.lowcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfig.entrydescription.lowcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_medcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.medcolor"),
                            Default = new List<string> { "200,200,100" },
                            Style = TextManager.Get("ntconfig.style.medcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfig.entrydescription.medcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_highcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.highcolor"),
                            Default = new List<string> { "250,100,100" },
                            Style = TextManager.Get("ntconfig.style.highcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfig.entrydescription.highcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_vitalcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.vitalcolor"),
                            Default = new List<string> { "255,0,0" },
                            Style = TextManager.Get("ntconfig.style.vitalcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfig.entrydescription.vitalcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_removalcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.removalcolor"),
                            Default = new List<string> { "0,255,255" },
                            Style = TextManager.Get("ntconfig.style.removalcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfig.entrydescription.removalcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_customcolor"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.customcolor"),
                            Default = new List<string> { "180,50,200" },
                            Style = TextManager.Get("ntconfig.style.customcolor"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.05f,
                            Description = TextManager.Get("ntconfig.entrydescription.customcolor"),
                            NoMLTB = true,
                            Group = true,
                            Resettable = true,
                        },

                        ["NTSCAN_VitalCategory"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.vitalcategory"),
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
                            Style = TextManager.Get("ntconfig.style.vitalcategory"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfig.entrydescription.vitalcategory"),
                        },

                        ["NTSCAN_RemovalCategory"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.removalcategory"),
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
                            Style = TextManager.Get("ntconfig.style.removalcategory"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfig.entrydescription.removalcategory"),
                        },

                        ["NTSCAN_CustomCategory"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.customcategory"),
                            Default = new List<string> { "" },
                            Style = TextManager.Get("ntconfig.style.customcategory"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfig.entrydescription.customcategory"),
                        },

                        ["NTSCAN_IgnoredCategory"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.ignoredcategory"),
                            Default = new List<string> { "" },
                            Style = TextManager.Get("ntconfig.style.ignoredcategory"),
                            Type = ConfigEntryType.String,
                            Boxsize = 0.1f,
                            Description = TextManager.Get("ntconfig.entrydescription.ignoredcategory"),
                        },

                        // ================================= COMMON ITEMS ========================================
                        ["NT_ItemPriceHeaderFirstAid"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.header3"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_antidama1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antidama1"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_gypsum"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_gypsum"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_suture"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_suture"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_tourniquet"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_tourniquet"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_needle"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_needle"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_drainage"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_drainage"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_gelipack"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_gelipack"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_ointment"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_ointment"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antibleeding1"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding2"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antibleeding2"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bloodpacks"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_bloodpacks"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_emptybloodpack"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_emptybloodpack"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_osteosynthesisimplants"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_osteosynthesisimplants"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_spinalimplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_spinalimplant"),
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
                            Name = TextManager.Get("ntconfig.entryname.header4"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_arms"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_arms"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_legs"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_legs"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bionicarms"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_bionicarms"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bioniclegs"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_bioniclegs"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_livertransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_livertransplant"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_lungtransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_lungtransplant"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_kidneytransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_kidneytransplant"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_hearttransplant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_hearttransplant"),
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
                            Name = TextManager.Get("ntconfig.entryname.header5"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_healthscanner"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_healthscanner"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bloodanalyzer"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_bloodanalyzer"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_defibrillator"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_defibrillator"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_aed"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_aed"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bvm"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_bvm"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_autocpr"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_autocpr"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_organcrate"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_organcrate"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_organtoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_organtoolbox"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_medtoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_medtoolbox"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerytoolbox"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_surgerytoolbox"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerytoolboxset"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_surgerytoolboxset"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_medstartercrate"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_medstartercrate"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_bodybag"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_bodybag"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_stasisbag"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_stasisbag"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_wheelchair"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_wheelchair"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_analgesictank"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_analgesictank"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_toxfilter"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_toxfilter"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_dialyzer"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_dialyzer"),
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
                            Name = TextManager.Get("ntconfig.entryname.header6"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_antibloodloss1"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antibloodloss1"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_opium"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_opium"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antidama2"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antidama2"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_ringerssolution"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_ringerssolution"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_mannitol"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_mannitol"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_immunosuppressant"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_immunosuppressant"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_thiamine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_thiamine"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_streptokinase"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_streptokinase"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antinarc"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antinarc"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibiotics"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antibiotics"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_adrenaline"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_adrenaline"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_liquidoxygenite"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_liquidoxygenite"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_deusizine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_deusizine"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antibleeding3"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antibleeding3"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_meth"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_meth"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_hyperzine"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_hyperzine"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antipsychosis"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antipsychosis"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_antiparalysis"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_antiparalysis"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_nitroglycerin"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_nitroglycerin"),
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
                            Name = TextManager.Get("ntconfig.entryname.header7"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_ItemPrice_advhemostat"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_advhemostat"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_advretractors"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_advretractors"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgicaldrill"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_surgicaldrill"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_surgerysaw"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_surgerysaw"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_tweezers"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_tweezers"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_traumashears"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_traumashears"),
                            Default = 1f,
                            Range = new float[] { 0, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_ItemPrice_advscalpel"] = new ConfigEntry
                        {
                            Page = "prices",
                            Name = TextManager.Get("ntconfig.entryname.itemprice_advscalpel"),
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
                            Name = TextManager.Get("ntconfig.entryname.header8"),
                            Type = ConfigEntryType.Category,
                        },

                        ["NT_OsteoImplants_uses"] = new ConfigEntry
                        {
                            Name = TextManager.Get("ntconfig.entryname.osteoimplants_uses"),
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
                            Name = TextManager.Get("ntconfig.entryname.spinalimplants_uses"),
                            Default = 1f,
                            Range = new float[] { 0.99f, 10 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        ["NT_HardmodeAorticRupture"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = TextManager.Get("ntconfig.entryname.hardmodeaorticrupture"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.hardmodeaorticrupture"),
                        },

                        ["NT_OpenCloseTamponade"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = TextManager.Get("ntconfig.entryname.openclosetamponade"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.openclosetamponade"),
                        },

                        ["NT_DoNitroprusside"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = TextManager.Get("ntconfig.entryname.donitroprusside"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.donitroprusside"),
                        },

                        ["NT_DoOrganScalpels"] = new ConfigEntry
                        {
                            Page = "availability",
                            Name = TextManager.Get("ntconfig.entryname.doorganscalpels"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfig.entrydescription.doorganscalpels"),
                        },


                        // ================================= EXPERIMENTAL =================================

                        ["NT_ExperimentalHeader"] = new ConfigEntry
                        {
                            Page = "experimental",
                            Name = TextManager.Get("ntconfigname_header9"),
                            Type = ConfigEntryType.Category,
                        },

                        //["NT_ExperimentalDescription1"] = new ConfigEntry
                        //{
                        //    Page = "experimental",
                        //    Name = TextManager.Get("ntconfigname_description1"),
                        //    Type = ConfigEntryType.Category,
                        //},

                        ["NT_UpdateInterval_High"] = new ConfigEntry
                        {
                            Page = "experimental",
                            Name = TextManager.Get("ntconfigname_updateinterval_high"),
                            Description = TextManager.Get("ntconfigdescription_updateinterval_high"),
                            Default = 120f,
                            Range = new float[] { 30, 480 },
                            Type = ConfigEntryType.Float,
                            Group = true,
                            Resettable = true,
                        },

                        //["NT_UpdateInterval_Monster"] = new ConfigEntry
                        //{
                        //    Page = "experimental",
                        //    Name = TextManager.Get("ntconfigname_updateinterval_monster"),
                        //    Description = TextManager.Get("ntconfigdescription_updateinterval_monster"),
                        //    Default = 120,
                        //    Range = new float[] { 30, 480 },
                        //    Type = ConfigEntryType.Float,
                        //    Group = true,
                        //    Resettable = true,
                        //},

                        ["NT_DEBUG_MODE"] = new ConfigEntry
                        {
                            Page = "experimental",
                            Name = TextManager.Get("ntconfigname_debugmode"),
                            Default = false,
                            Type = ConfigEntryType.Bool,
                            Description = TextManager.Get("ntconfigdescription_debugmode"),
                        },


                    }
                }
            );
        }
    }
}