-- Basically we have to trick our Lua NT Addons into using this human update.

local limbtypes = {
	LimbType.Torso,
	LimbType.Head,
	LimbType.LeftArm,
	LimbType.RightArm,
	LimbType.LeftLeg,
	LimbType.RightLeg,
}

local CSHumanUpdate = LuaUserData.CreateStatic("Neurotrauma.HumanUpdate",false)-- stores our class ref

NT.UsingAddons = function ()
	return #NTC.RegisteredExpansions == 0
end

NT.NonLimbAfflictionTranslations = 
{
	-- Bones
	["fracturedribs"] = "t_fracture",
	["fracturedskull"] = "h_fracture",
	["fracturedneck"] = "n_fracture",

	-- Head
	["spinalcordinjury"] = "t_paralysis",
	["neurotrauma"] = "cerebralhypoxia",

	-- Limbs
	["tourniqueted"] = "clampedarteries",

	-- Heart
	["increasedheartrate"] = "tachycardia",

	-- Torso
	["aorticrupture"] = "t_arterialcut",
	
	-- Symptoms
	["cough"] = "sym_cough",
	["paleskin"] = "sym_paleskin",
	["lightheadedness"] = "sym_lightheadedness",
	["blurredvision"] = "sym_blurredvision",
	["confusion"] = "sym_confusion",
	["headache"] = "sym_headache",
	["legswelling"] = "sym_legswelling",
	["weakness"] = "sym_weakness",
	["wheezing"] = "sym_wheezing",
	["vomiting"] = "sym_vomiting",
	["vomitingblood"] = "sym_hematemesis",
	["abdominaldiscomfort"] = "sym_abdomdiscomfort ",
	["bloating"] = "sym_bloating",
	["sweating"] = "sym_sweating",
	["palpitations"] = "sym_palpitations",
	["unconsciousness"] = "sym_unconsciousness ",
	["craving"] = "sym_craving",
	["nausea"] = "sym_nausea",
	["shortnessofbreath"] = "dyspnea",
	["jaundice"] = "sym_jaundice",

	-- Pain
	["chestpain"] = "pain_chest",

	-- Mechanics
	["artificialventilation"] = "alv",
}

NT.LimbAfflictionTranslations = 
{
	["bandageddirty"] = "dirtybandage",
	["firstdegreeburn"] = "burn_deg1",
	["seconddegreeburn"] = "burn_deg2",
	["thirddegreeburn"] = "burn_deg3",

	["fracturedextremity"] = {[LimbType.LeftArm] = "la_fracture", [LimbType.RightArm] = "ra_fracture", [LimbType.LeftLeg] = "ll_fracture",[LimbType.RightLeg] = "rl_fracture"},
	["arterialbleeding"] = {[LimbType.LeftLeg] = "ll_arterialcut",[LimbType.RightLeg] = "rl_arterialcut",[LimbType.LeftArm] = "la_arterialcut",[LimbType.RightArm] = "ra_arterialcut"},
}

NT.ConvertToLimbLegacy = function (Identifier, Limb)
	if NT.LimbAfflictionTranslations[Identifier] ~= nil then
		
		if NT.LimbAfflictionTranslations[Identifier][Limb] ~= nil then
			return NT.LimbAfflictionTranslations[Identifier][Limb]
		end

		return NT.LimbAfflictionTranslations[Identifier]
	end

	return Identifier
end

NT.ConvertToLegacy = function (Identifier)

	if NT.NonLimbAfflictionTranslations[Identifier] ~= nil then
		return NT.NonLimbAfflictionTranslations[Identifier]
	end

	return Identifier
end

NT.CreateLimbTables = function (CharData)
	for limb in limbtypes do
				local keystring = tostring(limb) .. "afflictions"
				CharData[keystring] = {}
		end
end

Hook.Patch("Neurotrauma.HumanUpdateLuaSync","SyncLuaAfflictions", function(GameSession, ptable)
	if not NT.UsingAddons()  then return end
	for NTHuman in ptable["CharacterList"] do
		local CharData = { character = NTHuman.Human, afflictions = {}, stats = {} }

		for AffData in NTHuman.LocalAfflictions.UpdatingNonLimbAfflictions do
			CharData.afflictions[NT.ConvertToLegacy(AffData.ID)] = { prev = AffData.PrevStrength, strength = AffData.Strength }
		end

		for AffData in NTHuman.LocalAfflictions.UpdatingBloodAfflictions do
			CharData.afflictions[NT.ConvertToLegacy(AffData.ID)] = { prev = AffData.PrevStrength, strength = AffData.Strength }
		end

		for AffData in NTHuman.LocalAfflictions.UpdatingSymptoms do
			CharData.afflictions[NT.ConvertToLegacy(AffData.ID)] = { prev = AffData.PrevStrength, strength = AffData.Strength }
		end

		for AffData in NTHuman.LocalAfflictions.UpdatingLimbSymptoms do
			CharData.afflictions[NT.ConvertToLegacy(AffData.ID)] = { prev = AffData.PrevStrength, strength = AffData.Strength }
		end

		NT.CreateLimbTables(CharData)
		for AffData in NTHuman.LocalAfflictions.UpdatingLimbAfflictions do
			for limb in limbtypes do
				local keystring = tostring(limb) .. "afflictions"
				CharData[keystring][NT.ConvertToLegacy(AffData.ID)] = { prev = AffData.GetLimbPrevStrength(limb), strength = AffData.GetLimbStrength(limb) }
			end
		end

		for StatData in NTHuman.LocalStats.DoubleStats do
			CharData.stats[StatData.ID] = StatData.Strength
		end

		for StatData in NTHuman.LocalStats.BoolStats do
			CharData.stats[StatData.ID] = StatData.Strength
		end

		NT.UpdateHuman(NTHuman.Human,CharData)
	end
end,  Hook.HookMethodType.After)

Hook.Patch("Neurotrauma.HumanUpdateLuaSync","SyncLuaCharacters", function(GameSession, ptable)
	if not NT.UsingAddons() then return end
	for NTHuman in ptable["CharacterList"] do
		NTC.AddEmptyCharacterData(NTHuman.Human)
	end
end,  Hook.HookMethodType.After)

-- Neurotrauma human update functions
-- Hooks Lua event "think" to update and use for applying NT specific character data (its called 'c') with
-- values/functions defined here in NT.UpdateHuman, NT.LimbAfflictions and NT.Afflictions
NT.UpdateCooldown = 0
NT.UpdateInterval = 120
NT.Deltatime = NT.UpdateInterval / 60 -- Time in seconds that transpires between updates

NT.organDamageCalc = function(c, damagevalue, nomaxstrength)
	if damagevalue >= 99 and (nomaxstrength == nil or nomaxstrength == false) then return 100 end
	return damagevalue - 0.01 * c.stats.healingrate * c.stats.specificOrganDamageHealMultiplier * NT.Deltatime
end

function NT.UpdateHuman(character, currentCharData)
	-- Doing this additional check here enables NT updates for 'important' people like players, crew and AI opponents from the get-go
	-- instead of waiting on other interactions before updates start. - Lukako
	if character.IsHuman and character.TeamID == 1 or character.TeamID == 2 and not character.IsDead then
	else
		-- Original check
		if not HF.HasAffliction(character, "luabotomy") then return end
	end

	NTC.CharacterSpeedMultipliers[character] = 1

	-- pre humanupdate hooks
	for key, val in pairs(NTC.PreHumanUpdateHooks) do
		val(character)
	end

	local charData = currentCharData

	-- fetch all the current affliction data
	for identifier, data in pairs(NT.Afflictions) do
		if NT.LegacyAfflictions[identifier] == nil and not NT.Afflictions[identifier].legacy then
			local strength = HF.GetAfflictionStrength(character, identifier, data.default or 0)
			charData.afflictions[identifier] = { prev = strength, strength = strength }
		end
	end
	-- fetch and calculate all the current stats
	for identifier, data in pairs(NT.CharStats) do
		if NT.LegacyCharStats[identifier] == nil and not NT.CharStats[identifier].legacy then
			if data.getter ~= nil then
				charData.stats[identifier] = data.getter(charData)
			else
				charData.stats[identifier] = data.default or 1
			end
		end
	end
	-- update non-limb-specific afflictions
	for identifier, data in pairs(NT.Afflictions) do
		if NT.LegacyAfflictions[identifier] == nil and not NT.Afflictions[identifier].legacy  then
			if data.update ~= nil then data.update(charData, identifier) end
		end
	end

	-- update and apply limb specific stuff
	local function FetchLimbData(type)
		local keystring = tostring(type) .. "afflictions"
		charData[keystring] = {}
		for identifier, data in pairs(NT.LimbAfflictions) do
			if NT.LegacyLimbAfflictions[identifier] == nil and not NT.LimbAfflictions[identifier].legacy then
				local strength = HF.GetAfflictionStrengthLimb(character, type, identifier, data.default or 0)
				charData[keystring][identifier] = { prev = strength, strength = strength }
			end
		end
	end
	local function UpdateLimb(type, stasisflag)
		local keystring = tostring(type) .. "afflictions"
		for identifier, data in pairs(NT.LimbAfflictions) do
			if NT.LegacyLimbAfflictions[identifier] == nil and not NT.LimbAfflictions[identifier].legacy then
				if
					data.update ~= nil
					and (
						not stasisflag
						or (
							NT.LimbAfflictions[identifier].ignorestasis ~= nil
							and NT.LimbAfflictions[identifier].ignorestasis == true
						)
					)
				then
					data.update(charData, charData[keystring], identifier, type)
				end
			end
		end
	end
	local function ApplyLimb(type, stasisflag)
		local keystring = tostring(type) .. "afflictions"
		for identifier, data in pairs(charData[keystring]) do
			if NT.LegacyLimbAfflictions[identifier] == nil and not NT.LimbAfflictions[identifier].legacy then
				local newval = HF.Clamp(
					data.strength,
					NT.LimbAfflictions[identifier].min or 0,
					NT.LimbAfflictions[identifier].max or 100
				)
				if
					newval ~= data.prev
					and (
						not stasisflag
						or (
							NT.LimbAfflictions[identifier].ignorestasis ~= nil
							and NT.LimbAfflictions[identifier].ignorestasis == true
						)
					)
				then
					if NT.LimbAfflictions[identifier].apply == nil then
						HF.SetAfflictionLimb(character, identifier, type, newval)
					else
						NT.LimbAfflictions[identifier].apply(charData, identifier, type, newval)
					end
				end
			end
		end
	end

	-- stasis completely halts activity in limbs
	for type in limbtypes do
		FetchLimbData(type)
	end
	for type in limbtypes do
		UpdateLimb(type, charData.stats.stasis)
	end
	for type in limbtypes do
		ApplyLimb(type, charData.stats.stasis)
	end

	-- non-limb-specific late update (useful for things that use stats that are altered by limb specifics)
	for identifier, data in pairs(NT.Afflictions) do
		if NT.LegacyAfflictions[identifier] == nil and not NT.Afflictions[identifier].legacy then
			if data.lateupdate ~= nil then data.lateupdate(charData, identifier) end
		end
	end

	-- apply non-limb-specific changes
	for identifier, data in pairs(charData.afflictions) do
		if NT.LegacyAfflictions[identifier] == nil and NT.Afflictions[identifier] ~= nil then
			local newval =
				HF.Clamp(data.strength, NT.Afflictions[identifier].min or 0, NT.Afflictions[identifier].max or 100)
			if newval ~= data.prev then
				if NT.Afflictions[identifier].apply == nil then
					HF.SetAffliction(character, identifier, newval)
				else
					NT.Afflictions[identifier].apply(charData, identifier, newval)
				end
			end
		end
	end

	-- compatibility
	NTC.TickCharacter(character)
	-- humanupdate hooks
	for key, val in pairs(NTC.HumanUpdateHooks) do
		val(character)
	end

	HF.SetAffliction(character,"slowdown", HF.Clamp(100 * (1 - NTC.CharacterSpeedMultipliers[character]), 0, 100));
	NTC.CharacterSpeedMultipliers[character] = nil
end