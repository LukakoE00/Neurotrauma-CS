-- TODO: fix line wrapping
-- DONE: All hooks moved together to the bottom

-- Items for which we call ItemMethod via LuaHook in its xml so they shouldnt be called again by applytreatment
local manuallyCalledItems = {
	needle = true,
	streptokinase = true,
	propofol = true,
	adrenaline = true,
}

local function UseItemMethod(item, usingCharacter, targetCharacter, limb, manualCall)
	-- Invalid use; don't do anything
	if item == nil or usingCharacter == nil or targetCharacter == nil or limb == nil then return end

	if not HF.HasAffliction(targetCharacter, "luabotomy") then HF.SetAffliction(targetCharacter, "luabotomy", 1) end

	-- Get the function associated with the identifier
	local identifier = item.Prefab.Identifier.Value
	local methodtorun = NT.ItemMethods[identifier]
	local isoverriden = NT.ItemsToIgnore[identifier]

	if methodtorun ~= nil and isoverriden == nil then
		if manuallyCalledItems[identifier] and not manualCall then return end
		-- Run said function
		methodtorun(item, usingCharacter, targetCharacter, limb)
		return
	end

	-- StartsWith functions
	for key, value in pairs(NT.ItemStartsWithMethods) do
		if HF.StartsWith(identifier, key) and isoverriden == nil then
			value(item, usingCharacter, targetCharacter, limb)
			return
		end
	end
end

-- TODO: some items trigger afflictions after a single human update, to fix, trigger them immediately for consistency
-- Store all item-specific functions in a table;
NT.ItemMethods = {} -- with the identifier as the key
NT.ItemStartsWithMethods = {} -- with the start of the identifier as the key
NT.ItemsToIgnore = {["healthscanner"] = true,["bloodanalyzer"] = true,["traumashears"] = true,["divingknife"] = true,["gypsum"] = true,["suture"] = true,["tourniquet"] = true,["emptybloodpack"] = true,
					["propofol"] = true,["streptokinase"] = true,["adrenaline"] = true,["ointment"] = true,["antibleeding1"] = true,["antibleeding2"] = true,["defibrillator"] = true,["aed"] = true,
					["blahaj"] = true,["advscalpel"] = true,["advhemostat"] = true,["advretractors"] = true,["surgicaldrill"] = true,["surgerysaw"] = true,["tweezers"] = true,["organscalpel_liver"] = true,
					["organscalpel_lungs"] = true,["organscalpel_heart"] = true,["organscalpel_kidneys"] = true,["organscalpel_brain"] = true,["osteosynthesisimplants"] = true,["spinalimplant"] = true,
					["drainage"] = true,["needle"] = true,["braintransplant"] = true,["rarm"] = true,["larm"] = true,["rleg"] = true,["lleg"] = true,["rarmp"] = true,["larmp"] = true,["rlegp"] = true,["llegp"] = true,
					["antibloodloss2"] = true,["autocpr"] = true,["gelipack"] = true,["livertransplant"] = true,["hearttransplant"] = true,["lungtransplant"] = true,["kidneytransplant"] = true,["wrench"] = true,
					["heavywrench"] = true,["repairpack"] = true,["bloodpack"] = true,["endovascballoon"] = true,["medstent"] = true,["antisepticspray"] = true}
NT.LegacyItemMethods = {}


-- Make formatting lines easier on the eyes in-code for the Health Scanner / Hematology Analyzer
local function formatLine(readoutString, readoutColor)
	if readoutString ~= "" then
		return "‖color:" .. readoutColor .. "‖" .. readoutString .. "‖color:end‖"
	else
		return readoutString
	end
end

NT.HematologyDetectable = {
}

NT.CuttableAfflictions = {
}

NT.TraumashearsAfflictions = {
}

-- Treatment Items
NT.SutureAfflictions = {
}

NT.DrainageAfflictions = {
}


-- ============================ HOOKS ===========================

NT.FixCondition = {
	"healthscanner",
	"bloodanalyzer",
	"defibrillator",
	"bvm",
	"autocpr",
	"aed",
}

function NT.RefreshCondition()
	for item in Item.ItemList do
		if HF.TableContains(NT.FixCondition, item.Prefab.Identifier.Value) then item.Condition = 100 end
	end
end

Timer.Wait(function()
	NT.RefreshCondition()
end, 1000)

Hook.Add("roundStart", "NT.RoundStart.ConditionItems", function()
	Timer.Wait(function()
		NT.RefreshCondition()
	end, 10000)
end)

Hook.Add("item.applyTreatment", "NT.itemused", function(item, usingCharacter, targetCharacter, limb)
	UseItemMethod(item, usingCharacter, targetCharacter, limb)
end)

Hook.Add("NT.runItemMethod", "NT.itemused_manual", function(effect, deltaTime, item, targets, worldPosition, element)
	local target = targets[1]

	if not target then return end

	if LuaUserData.IsTargetType(target, "Barotrauma.Limb") then
		UseItemMethod(item, effect.user, target.character, target, true)
	elseif LuaUserData.IsTargetType(target, "Barotrauma.Character") then
		UseItemMethod(item, effect.user, target, target.AnimController.MainLimb, true)
	end
end)

-- ToDo: Make this a for loop and be smart lmao

NT.LegacyItemMethods.healthscanner = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"healthscanner")
end

-- Updated likewise to the Health Scanner
NT.LegacyItemMethods.bloodanalyzer = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"bloodanalyzer")
end

-- Trauma Shears
NT.LegacyItemMethods.traumashears = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"traumashears")
end

-- Diving Knife
NT.LegacyItemMethods.divingknife = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"divingknife")
end

-- Gypsum
NT.LegacyItemMethods.gypsum = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"gypsum")
end

-- Sutures
NT.LegacyItemMethods.suture = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"suture")
end

-- Tourniquet
NT.LegacyItemMethods.tourniquet = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"tourniquet")
end

-- Empty Blood Packs
NT.LegacyItemMethods.emptybloodpack = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"emptybloodpack")
end

-- Propofol :skull:
NT.LegacyItemMethods.propofol = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"propofol")
end

-- Streptokinase
NT.LegacyItemMethods.streptokinase = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"streptokinase")
end

-- Antibiotic Ointment
NT.LegacyItemMethods.ointment = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"ointment")
end

-- Bandages
NT.LegacyItemMethods.antibleeding1 = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"antibleeding1")
end

-- Plastiseal
NT.LegacyItemMethods.antibleeding2 = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"antibleeding2")
end

-- Manual Defibrillator
NT.LegacyItemMethods.defibrillator = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"defibrillator")
end

-- Automated External Defibrillator (AED)
NT.LegacyItemMethods.aed = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"aed")
end

-- Blahaj / Blue Shark Plushie
NT.LegacyItemMethods.blahaj = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"blahaj")
end

-- Surgery
-- Scalpel
NT.LegacyItemMethods.advscalpel = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"advscalpel")
end

-- Hemostat
NT.LegacyItemMethods.advhemostat = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"advhemostat")
end

-- Skin Retractors
NT.LegacyItemMethods.advretractors = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"advretractors")
end

-- Surgical Drill
NT.LegacyItemMethods.surgicaldrill = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"surgicaldrill")
end

-- Surgical Saw
NT.LegacyItemMethods.surgerysaw = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"surgerysaw")
end

-- Tweezers
NT.LegacyItemMethods.tweezers = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"tweezers")
end

-- Liver scalpel (used by Multipurpose Scalpel)
NT.LegacyItemMethods.organscalpel_liver = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"organscalpel_liver")
end

-- Lung scalpel (used by Multipurpose Scalpel)
NT.LegacyItemMethods.organscalpel_lungs = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"organscalpel_lungs")
end

-- Heart scalpel (used by Multipurpose Scalpel)
NT.LegacyItemMethods.organscalpel_heart = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"organscalpel_heart")
end

-- Kidney scalpel (used by Multipurpose Scalpel)
NT.LegacyItemMethods.organscalpel_kidneys = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"organscalpel_kidneys")
end

-- Brain scalpel (used by Multipurpose Scalpel)
NT.LegacyItemMethods.organscalpel_brain = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"organscalpel_brain")
end

-- Osteosynthesis Implants
NT.LegacyItemMethods.osteosynthesisimplants = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"osteosynthesisimplants")
end

-- Spinal Cord Implants
NT.LegacyItemMethods.spinalimplant = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"spinalimplant")
end

-- Drainage
NT.LegacyItemMethods.drainage = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"drainage")
end

-- Needle
NT.LegacyItemMethods.needle = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"needle")
end

-- Brain Transplant (Item)
NT.LegacyItemMethods.braintransplant = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"braintransplant")
end


NT.LegacyItemMethods.rarm = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"rarm")
end

NT.LegacyItemMethods.larm = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"larm")
end

NT.LegacyItemMethods.rleg = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"rleg")
end

NT.LegacyItemMethods.lleg = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"lleg")
end

-- Bionic Prosthetics
NT.LegacyItemMethods.rarmp = NT.LegacyItemMethods.rarm
NT.LegacyItemMethods.larmp = NT.LegacyItemMethods.larm
NT.LegacyItemMethods.rlegp = NT.LegacyItemMethods.rleg
NT.LegacyItemMethods.llegp = NT.LegacyItemMethods.lleg

NT.LegacyItemMethods.antibloodloss2 = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"antibloodloss2")
end

-- AutoPulse
NT.LegacyItemMethods.autocpr = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"autocpr")
end

-- Gel Coolant Pack
NT.LegacyItemMethods.gelipack = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"gelipack")
end

--=================== StartsWith region begins ========================
-- Transplants
-- Liver Transplant
NT.LegacyItemMethods.livertransplant = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"livertransplant")
end

-- Heart Transplant
NT.LegacyItemMethods.hearttransplant = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"hearttransplant")
end

-- Lung Transplant
NT.LegacyItemMethods.lungtransplant = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"lungtransplant")
end

-- Kidney Transplant
NT.LegacyItemMethods.kidneytransplant = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"kidneytransplant")
end

-- Miscellaneous
-- Wrench
NT.LegacyItemMethods.wrench = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"wrench")
end

-- Variants
NT.LegacyItemMethods.heavywrench = NT.LegacyItemMethods.wrench
NT.LegacyItemMethods.repairpack = NT.LegacyItemMethods.wrench

-- Blood Packs
NT.LegacyItemMethods.bloodpack = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"bloodpack")
end

-- Dynamic Items
-- Endovascular Balloon
NT.LegacyItemMethods.endovascballoon = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"endovascballoon")
end

-- Medical Stent
NT.LegacyItemMethods.medstent = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"medstent")
end

-- Antiseptic Spray
NT.LegacyItemMethods.antisepticspray = function(item, usingCharacter, targetCharacter, limb)
	CSItems.CallItemUseMethod(item, usingCharacter, targetCharacter, limb,"antisepticspray")
end

NT.ItemMethods.healthscanner = NT.LegacyItemMethods["healthscanner"]

-- Updated likewise to the Health Scanner
NT.ItemMethods.bloodanalyzer = NT.LegacyItemMethods["bloodanalyzer"]

-- Trauma Shears
NT.ItemMethods.traumashears = NT.LegacyItemMethods["traumashears"]

-- Diving Knife
NT.ItemStartsWithMethods.divingknife = NT.LegacyItemMethods["divingknife"]

-- Gypsum
NT.ItemMethods.gypsum = NT.LegacyItemMethods["gypsum"]

-- Sutures
NT.ItemMethods.suture = NT.LegacyItemMethods["suture"]

-- Tourniquet
NT.ItemMethods.tourniquet = NT.LegacyItemMethods["tourniquet"]

-- Empty Blood Packs
NT.ItemMethods.emptybloodpack = NT.LegacyItemMethods["emptybloodpack"]

-- Propofol :skull:
NT.ItemMethods.propofol = NT.LegacyItemMethods["propofol"]

-- Streptokinase
NT.ItemMethods.streptokinase = NT.LegacyItemMethods["streptokinase"]

-- Antibiotic Ointment
NT.ItemMethods.ointment = NT.LegacyItemMethods["ointment"]

-- Bandages
NT.ItemMethods.antibleeding1 = NT.LegacyItemMethods["antibleeding1"]

-- Plastiseal
NT.ItemMethods.antibleeding2 = NT.LegacyItemMethods["antibleeding2"]

-- Manual Defibrillator
NT.ItemMethods.defibrillator = NT.LegacyItemMethods["defibrillator"]

-- Automated External Defibrillator (AED)
NT.ItemStartsWithMethods.aed = NT.LegacyItemMethods["aed"]

-- Blahaj / Blue Shark Plushie
NT.ItemMethods.blahaj = NT.LegacyItemMethods["blahaj"]

-- Surgery
-- Scalpel
NT.ItemMethods.advscalpel = NT.LegacyItemMethods["advscalpel"]

-- Hemostat
NT.ItemMethods.advhemostat = NT.LegacyItemMethods["advhemostat"]

-- Skin Retractors
NT.ItemMethods.advretractors = NT.LegacyItemMethods["advretractors"]

-- Surgical Drill
NT.ItemMethods.surgicaldrill = NT.LegacyItemMethods["surgicaldrill"]

-- Surgical Saw
NT.ItemMethods.surgerysaw = NT.LegacyItemMethods["surgerysaw"]

-- Tweezers
NT.ItemMethods.tweezers = NT.LegacyItemMethods["tweezers"]

-- Liver scalpel (used by Multipurpose Scalpel)
NT.ItemMethods.organscalpel_liver = NT.LegacyItemMethods["organscalpel_liver"]

-- Lung scalpel (used by Multipurpose Scalpel)
NT.ItemMethods.organscalpel_lungs = NT.LegacyItemMethods["organscalpel_lungs"]

-- Heart scalpel (used by Multipurpose Scalpel)
NT.ItemMethods.organscalpel_heart = NT.LegacyItemMethods["organscalpel_heart"]

-- Kidney scalpel (used by Multipurpose Scalpel)
NT.ItemMethods.organscalpel_kidneys = NT.LegacyItemMethods["organscalpel_kidneys"]

-- Brain scalpel (used by Multipurpose Scalpel)
NT.ItemMethods.organscalpel_brain = NT.LegacyItemMethods["organscalpel_brain"]

-- Osteosynthesis Implants
NT.ItemMethods.osteosynthesisimplants = NT.LegacyItemMethods["osteosynthesisimplants"]

-- Spinal Cord Implants
NT.ItemMethods.spinalimplant = NT.LegacyItemMethods["spinalimplant"]

-- Drainage
NT.ItemMethods.drainage = NT.LegacyItemMethods["drainage"]

-- Needle
NT.ItemMethods.needle = NT.LegacyItemMethods["needle"]

-- Brain Transplant (Item)
NT.ItemMethods.braintransplant = NT.LegacyItemMethods["braintransplant"]


NT.ItemMethods.rarm = NT.LegacyItemMethods["rarm"]

NT.ItemMethods.larm = NT.LegacyItemMethods["larm"]

NT.ItemMethods.rleg = NT.LegacyItemMethods["rleg"]

NT.ItemMethods.lleg = NT.LegacyItemMethods["lleg"]

-- Bionic Prosthetics
NT.ItemMethods.rarmp = NT.ItemMethods.rarm
NT.ItemMethods.larmp = NT.ItemMethods.larm
NT.ItemMethods.rlegp = NT.ItemMethods.rleg
NT.ItemMethods.llegp = NT.ItemMethods.lleg

NT.ItemMethods.antibloodloss2 = NT.LegacyItemMethods["antibloodloss2"]

-- AutoPulse
NT.ItemMethods.autocpr = NT.LegacyItemMethods["autocpr"]

-- Gel Coolant Pack
NT.ItemMethods.gelipack = NT.LegacyItemMethods["gelipack"]

--=================== StartsWith region begins ========================
-- Transplants
-- Liver Transplant
NT.ItemStartsWithMethods.livertransplant = NT.LegacyItemMethods["livertransplant"]

-- Heart Transplant
NT.ItemStartsWithMethods.hearttransplant = NT.LegacyItemMethods["hearttransplant"]

-- Lung Transplant
NT.ItemStartsWithMethods.lungtransplant = NT.LegacyItemMethods["lungtransplant"]

-- Kidney Transplant
NT.ItemStartsWithMethods.kidneytransplant = NT.LegacyItemMethods["kidneytransplant"]

-- Miscellaneous
-- Wrench
NT.ItemStartsWithMethods.wrench = NT.LegacyItemMethods["wrench"]

-- Variants
NT.ItemMethods.heavywrench = NT.ItemStartsWithMethods.wrench
NT.ItemMethods.repairpack = NT.ItemStartsWithMethods.wrench

-- Blood Packs
NT.ItemStartsWithMethods.bloodpack = NT.LegacyItemMethods["bloodpack"]

-- Dynamic Items
-- Endovascular Balloon
NT.ItemMethods.endovascballoon = NT.LegacyItemMethods["endovascballoon"]

-- Medical Stent
NT.ItemMethods.medstent = NT.LegacyItemMethods["medstent"]

-- Antiseptic Spray
NT.ItemStartsWithMethods.antisepticspray = NT.LegacyItemMethods["antisepticspray"]
