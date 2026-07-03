print("[NT2] This is the ServerExample init print!")

NT = {}
NT.Name = "Neurotrauma"
NT.Version = "1.0.0h0"
NT.VersionNum = 000000001
NT.Path = table.pack(...)[1]

dofile(NT.Path .. "/Lua/Scripts/Server/DummyHumanUpdate.lua") 	
dofile(NT.Path .. "/Lua/Scripts/Server/NTCompat.lua") 		

-- Register our needed classes

LuaUserData.RegisterType("Neurotrauma.NeurotraumaInit")
LuaUserData.RegisterType("Neurotrauma.NT")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate")
LuaUserData.RegisterType("Neurotrauma.HumanUpdate+NTHuman")