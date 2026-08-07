print("[NT2] Loaded Shared Side!")

NT = {}
NT.Name = "Neurotrauma"
NT.Version = "1.0.0h0"
NT.VersionNum = 01090000
NT.Path = table.pack(...)[1]
NT.SymsForNPC = {}
NT.BLOODTYPE = {}
NT.ContainerFills = {}

NTConfig = LuaUserData.CreateStatic("Neurotrauma.NTConfig",false)
NTLua = LuaUserData.CreateStatic("Neurotrauma.NTLua",false)
NTInfo = LuaUserData.CreateStatic("Neurotrauma.NTInfo",false)
CSNTCompat = LuaUserData.CreateStatic("Neurotrauma.NTC",false)
CSNTAfflictions = LuaUserData.CreateStatic("Neurotrauma.NTAfflictions",false)
CSHumanUpdate = LuaUserData.CreateStatic("Neurotrauma.HumanUpdate",false)-- stores our class ref
CSItems = LuaUserData.CreateStatic("Neurotrauma.NTItemMethodsLuaCompat",false)
CSInit = LuaUserData.CreateStatic("Neurotrauma.NT",false)-- stores our class ref
CSSpeakIssues = LuaUserData.CreateStatic("Neurotrauma.SpeakAboutIssuesPatch",false)
AfflictionPriority = LuaUserData.CreateEnumTable("Neurotrauma.AfflictionPriority",false)

dofile(NT.Path .. "/Lua/Scripts/Shared/ConfigData.lua") 		
dofile(NT.Path .. "/Lua/Scripts/Shared/HelperFunctions.lua") 		
dofile(NT.Path .. "/Lua/Scripts/Shared/DummyHumanUpdate.lua") 	
dofile(NT.Path .. "/Lua/Scripts/Shared/LegacyAfflictions.lua") 	
dofile(NT.Path .. "/Lua/Scripts/Shared/NTCompat.lua") 	
dofile(NT.Path .. "/Lua/Scripts/Shared/Items.lua") 		