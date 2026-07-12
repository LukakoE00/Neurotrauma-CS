print("[NT2] This is the SharedExample init print!")

NT = {}
NT.Name = "Neurotrauma"
NT.Version = "1.0.0h0"
NT.VersionNum = 000000001
NT.Path = table.pack(...)[1]
NT.SymsForNPC = {}
NT.BLOODTYPE = {}
NT.ContainerFills = {}

LuaUserData.RegisterType("Neurotrauma.NTConfig")
LuaUserData.RegisterType("Neurotrauma.ConfigExpansion")
LuaUserData.RegisterType("Neurotrauma.ConfigEntry")
LuaUserData.RegisterType("Neurotrauma.ConfigEntryType")
LuaUserData.RegisterType("Neurotrauma.NTConfigData")
LuaUserData.RegisterType("Neurotrauma.NeurotraumaInit")
LuaUserData.RegisterType("Neurotrauma.NT")
LuaUserData.RegisterType("Neurotrauma.NTAfflictions")
LuaUserData.RegisterType("Neurotrauma.NTC")
LuaUserData.RegisterType("Neurotrauma.NTInfo")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+NTHuman")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+CharacterAfflictions")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+CharacterStats")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+CharacterTags")

LuaUserData.RegisterType("Neurotrauma.HumanUpdate+NTHumanAffData")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+NTHumanNonLimbAffData")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+NTHumanLimbAffData")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+NTHumanBloodAffData")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+NTHumanSymptomData")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+NTHumanLimbSymptomData")

LuaUserData.RegisterType("Neurotrauma.HumanUpdate+CharacterStats+NTHumanStatDoubleData")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+CharacterStats+NTHumanStatBoolData")


dofile(NT.Path .. "/Lua/Scripts/Shared/ConfigData.lua") 		
dofile(NT.Path .. "/Lua/Scripts/Shared/HelperFunctions.lua") 		
dofile(NTServer.Path .. "/Lua/Scripts/Shared/DummyHumanUpdate.lua") 	
dofile(NTServer.Path .. "/Lua/Scripts/Shared/LegacyAfflictions.lua") 	
dofile(NTServer.Path .. "/Lua/Scripts/Shared/NTCompat.lua") 	
dofile(NTServer.Path .. "/Lua/Scripts/Shared/Items.lua") 		