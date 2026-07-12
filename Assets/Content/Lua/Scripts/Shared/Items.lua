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

	if methodtorun ~= nil then
		if manuallyCalledItems[identifier] and not manualCall then return end
		-- Run said function
		methodtorun(item, usingCharacter, targetCharacter, limb)
		return
	end

	-- StartsWith functions
	for key, value in pairs(NT.ItemStartsWithMethods) do
		if HF.StartsWith(identifier, key) then
			value(item, usingCharacter, targetCharacter, limb)
			return
		end
	end
end

-- TODO: some items trigger afflictions after a single human update, to fix, trigger them immediately for consistency
-- Store all item-specific functions in a table;
NT.ItemMethods = {} -- with the identifier as the key
NT.ItemStartsWithMethods = {} -- with the start of the identifier as the key

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

NT.DrainageAfflictions = {}


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

Hook.Add("meleeWeapon.handleImpact", "NT.fracturedOnMelee", function(meleeWeapon, target)
	if meleeWeapon == nil or target == nil then return end

	local itemUser = meleeWeapon.picker
	if itemUser == nil then return end

	local item = meleeWeapon.Item
	if item == nil then return end

	Timer.Wait(function()
		local adrenaline = HF.HasAffliction(itemUser, "afadrenaline", 1)
		-- Right Arm Fracture
		if itemUser.Inventory.IsInLimbSlot(item, 2) then
			if
				HF.HasAffliction(itemUser, "ra_fracture", 1)
				and not HF.HasAfflictionLimb(itemUser, "gypsumcast", LimbType.RightArm, 0.1)
			then
				if adrenaline then
					HF.AddAfflictionLimb(itemUser, "bleeding", LimbType.RightArm, 15)
				else
					itemUser.Inventory.ForceRemoveFromSlot(item, 0)
					item.Drop(itemUser, true)
					HF.SetAffliction(itemUser, "ra_fracture", 100)
					HF.AddAfflictionLimb(itemUser, "bleeding", LimbType.RightArm, 40)
				end
			-- Dislocation
			elseif HF.HasAffliction(itemUser, "dislocation3", 1) and not adrenaline then
				itemUser.Inventory.ForceRemoveFromSlot(item, 0)
				item.Drop(itemUser, true)
				HF.SetAffliction(itemUser, "dislocation3", 100)
			end
		end
		-- Left Arm Fracture
		if itemUser.Inventory.IsInLimbSlot(item, 4) then
			if
				HF.HasAffliction(itemUser, "la_fracture", 1)
				and not HF.HasAfflictionLimb(itemUser, "gypsumcast", LimbType.LeftArm, 0.1)
			then
				if adrenaline then
					HF.AddAfflictionLimb(itemUser, "bleeding", LimbType.LeftArm, 15)
				else
					itemUser.Inventory.ForceRemoveFromSlot(item, 0)
					item.Drop(itemUser, true)
					HF.SetAffliction(itemUser, "la_fracture", 100)
					HF.AddAfflictionLimb(itemUser, "bleeding", LimbType.LeftArm, 40)
				end
			-- Dislocation
			elseif HF.HasAffliction(itemUser, "dislocation4", 1) and not adrenaline then
				itemUser.Inventory.ForceRemoveFromSlot(item, 0)
				item.Drop(itemUser, true)
				HF.SetAffliction(itemUser, "dislocation4", 100)
			end
		end
	end, 1)
end)

Hook.Add("item.use", "NT.fracturedOnShoot", function(item, itemUser, targetLimb)
	Timer.Wait(function()
		if item == nil or item.GetComponentString("RangedWeapon") == nil or itemUser == nil then return end
		local adrenaline = HF.HasAffliction(itemUser, "afadrenaline", 1)

		-- Right Arm Fracture
		if itemUser.Inventory.IsInLimbSlot(item, 2) then
			if
				HF.HasAffliction(itemUser, "ra_fracture", 1)
				and not HF.HasAfflictionLimb(itemUser, "gypsumcast", LimbType.RightArm, 0.1)
			then
				if adrenaline then
					HF.AddAfflictionLimb(itemUser, "bleeding", LimbType.RightArm, 15)
				else
					itemUser.Inventory.ForceRemoveFromSlot(item, 0)
					item.Drop(itemUser, true)
					HF.SetAffliction(itemUser, "ra_fracture", 100)
					HF.AddAfflictionLimb(itemUser, "bleeding", LimbType.RightArm, 40)
				end
			-- Dislocation
			elseif HF.HasAffliction(itemUser, "dislocation3", 1) and not adrenaline then
				itemUser.Inventory.ForceRemoveFromSlot(item, 0)
				item.Drop(itemUser, true)
				HF.SetAffliction(itemUser, "dislocation3", 100)
			end
		end
		-- Left Arm Fracture
		if itemUser.Inventory.IsInLimbSlot(item, 4) then
			if
				HF.HasAffliction(itemUser, "la_fracture", 1)
				and not HF.HasAfflictionLimb(itemUser, "gypsumcast", LimbType.LeftArm, 0.1)
			then
				if adrenaline then
					HF.AddAfflictionLimb(itemUser, "bleeding", LimbType.LeftArm, 15)
				else
					itemUser.Inventory.ForceRemoveFromSlot(item, 0)
					item.Drop(itemUser, true)
					HF.SetAffliction(itemUser, "la_fracture", 100)
					HF.AddAfflictionLimb(itemUser, "bleeding", LimbType.LeftArm, 40)
				end
			-- Dislocation
			elseif HF.HasAffliction(itemUser, "dislocation4", 1) and not adrenaline then
				itemUser.Inventory.ForceRemoveFromSlot(item, 0)
				item.Drop(itemUser, true)
				HF.SetAffliction(itemUser, "dislocation4", 100)
			end
		end
	end, 1)
end)