print("[NT2] This is the ServerExample init print!")

NTServer = {}
NTServer.Path = table.pack(...)[1]

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

dofile(NTServer.Path .. "/Lua/Scripts/Server/DummyHumanUpdate.lua") 	
dofile(NTServer.Path .. "/Lua/Scripts/Server/LegacyAfflictions.lua") 	
dofile(NTServer.Path .. "/Lua/Scripts/Server/NTCompat.lua") 	
dofile(NTServer.Path .. "/Lua/Scripts/Server/Items.lua") 		
