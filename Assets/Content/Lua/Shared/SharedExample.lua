print("[NT2] This is the SharedExample init print!")

NT = {}
NT.Name = "Neurotrauma"
NT.Version = "1.0.0h0"
NT.VersionNum = 000000001
NT.Path = table.pack(...)[1]
NT.SymsForNPC = {}
NT.BLOODTYPE = {}
NT.ContainerFills = {}

Init = LuaUserData.CreateStatic("Neurotrauma.NT",false)-- stores our class ref
NTConfig = LuaUserData.CreateStatic("Neurotrauma.NTConfig",false)
NTInfo = LuaUserData.CreateStatic("Neurotrauma.NTInfo",false)
CSNTCompat = LuaUserData.CreateStatic("Neurotrauma.NTC",false)
CSNTAfflictions = LuaUserData.CreateStatic("Neurotrauma.NTAfflictions",false)
CSHumanUpdate = LuaUserData.CreateStatic("Neurotrauma.HumanUpdate",false)-- stores our class ref
NTLua = LuaUserData.CreateStatic("Neurotrauma.NTLua",false)
AfflictionPriority = LuaUserData.CreateEnumTable("Neurotrauma.AfflictionPriority",false)

dofile(NT.Path .. "/Lua/Scripts/Shared/ConfigData.lua") 		
dofile(NT.Path .. "/Lua/Scripts/Shared/HelperFunctions.lua") 		
dofile(NT.Path .. "/Lua/Scripts/Shared/DummyHumanUpdate.lua") 	
dofile(NT.Path .. "/Lua/Scripts/Shared/LegacyAfflictions.lua") 	
dofile(NT.Path .. "/Lua/Scripts/Shared/NTCompat.lua") 	
dofile(NT.Path .. "/Lua/Scripts/Shared/Items.lua") 		