-- Basically we have to trick our Lua NT Addons into using this human update.

local limbtypes = {
	LimbType.Torso,
	LimbType.Head,
	LimbType.LeftArm,
	LimbType.RightArm,
	LimbType.LeftLeg,
	LimbType.RightLeg,
}

NT.UsingAddons = function ()
	return #NTC.RegisteredExpansions > 0
end

NT.NonLimbAfflictionTranslations = 
{
	-- Bones
	["fracturedribs"] = "t_fracture",
	["fracturedskull"] = "h_fracture",
	["fracturedneck"] = "n_fracture",
	["sawedbones"] = "bonecut",
	["stimulatedbonegrowth"] = "bonegrowth",

	-- Head
	["spinalcordinjury"] = "t_paralysis",
	["neurotrauma"] = "cerebralhypoxia",

	-- Limbs
	["tourniqueted"] = "clampedarteries",
	["clampedbleeding"] = "clampedbleeders",
	["plastercast"] = "gypsumcast",

	-- Heart
	["increasedheartrate"] = "tachycardia",

	-- Torso
	["aorticrupture"] = "t_arterialcut",
	["carotidarterialcut"] = "h_arterialcut",
	
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
	["abdominaldiscomfort"] = "sym_abdomdiscomfort",
	["bloating"] = "sym_bloating",
	["sweating"] = "sym_sweating",
	["palpitations"] = "sym_palpitations",
	["unconsciousness"] = "sym_unconsciousness",
	["craving"] = "sym_craving",
	["nausea"] = "sym_nausea",
	["shortnessofbreath"] = "dyspnea",
	["jaundice"] = "sym_jaundice",

	-- Pain
	["chestpain"] = "pain_chest",

	-- Mechanics
	["artificialventilation"] = "alv",
	["safesurgery"] = "table",
}

NT.NonLimbAfflictionTranslationsModern = 
{
--     [OLD ID]  <---->  [NEW ID]
	-- Bones
	["t_fracture"] = "fracturedribs",
	["h_fracture"] = "fracturedskull",
	["n_fracture"] = "fracturedneck",
	["bonecut"] = "sawedbones",
	["bonegrowth"] = "stimulatedbonegrowth",

	-- Head
	["t_paralysis"] = "spinalcordinjury",
	["cerebralhypoxia"] = "neurotrauma",

	-- Limbs
	["clampedarteries"] = "tourniqueted",
	["clampedbleeders"] = "clampedbleeding",
	["gypsumcast"] = "plastercast",

	-- Heart
	["tachycardia"] = "increasedheartrate",

	-- Torso
	["t_arterialcut"] = "aorticrupture",
	["h_arterialcut"] = "carotidarterialcut",
	
	-- Symptoms
	["sym_cough"] = "cough",
	["sym_paleskin"] = "paleskin",
	["sym_lightheadedness"] = "lightheadedness",
	["sym_blurredvision"] = "blurredvision",
	["sym_confusion"] = "confusion",
	["sym_headache"] = "headache",
	["sym_legswelling"] = "legswelling",
	["sym_weakness"] = "weakness",
	["sym_wheezing"] = "wheezing",
	["sym_vomiting"] = "vomiting",
	["sym_hematemesis"] = "vomitingblood",
	["sym_abdomdiscomfort"] = "abdominaldiscomfort",
	["sym_bloating"] = "bloating",
	["sym_sweating"] = "sweating",
	["sym_palpitations"] = "palpitations",
	["sym_unconsciousness"] = "unconsciousness",
	["sym_craving"] = "craving",
	["sym_nausea"] = "nausea",
	["dyspnea"] = "shortnessofbreath",
	["sym_jaundice"] = "jaundice",

	-- Pain
	["chestpain"] = "pain_chest",

	-- Mechanics
	["table"] = "safesurgery",
	["alv"] = "artificialventilation",

	-- Limb afflictions
	["dirtybandage"] = "bandageddirty",
	["burn_deg1"] = "firstdegreeburn",
	["burn_deg2"] = "seconddegreeburn",
	["burn_deg3"] = "thirddegreeburn",
	["la_fracture"] = "fracturedextremity",
	["ra_fracture"] = "fracturedextremity",
	["ll_fracture"] = "fracturedextremity",
	["rl_fracture"] = "fracturedextremity",
	["ll_arterialcut"] = "arterialbleeding",
	["rl_arterialcut"] = "arterialbleeding",
	["la_arterialcut"] = "arterialbleeding",
	["ra_arterialcut"] = "arterialbleeding",
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

NT.LimbSymptoms = 
{
	["inflammation"] = true,
	["spasm"] = true
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

NT.ConvertToLimbModern = function (Identifier, Limb)
	if NT.LimbAfflictionTranslationsModern[Identifier] ~= nil then
		
		if NT.LimbAfflictionTranslationsModern[Identifier][Limb] ~= nil then
			return NT.LimbAfflictionTranslationsModern[Identifier][Limb]
		end

		return NT.LimbAfflictionTranslationsModern[Identifier]
	end

	return Identifier
end

NT.ConvertToLegacy = function (Identifier)

	if NT.NonLimbAfflictionTranslations[Identifier] ~= nil then
		return NT.NonLimbAfflictionTranslations[Identifier]
	end

	return Identifier
end

NT.ConvertToModern = function (Identifier)
	if NT.NonLimbAfflictionTranslationsModern[Identifier] ~= nil then
		return NT.NonLimbAfflictionTranslationsModern[Identifier]
	end

	return Identifier
end

NT.CreateLimbTables = function (CharData)
	for limb in limbtypes do
				local keystring = tostring(limb) .. "afflictions"
				CharData[keystring] = {}
		end
end

NTLua.Add("SyncAfflictions", function(deltatime, characterlist, priorities)
	NT.Deltatime = deltatime
	for NTHuman in characterlist do
		local CharData = { character = NTHuman.Human, afflictions = {}, stats = {} }

		for StatData in NTHuman.LocalStats.DoubleStats do
			CharData.stats[StatData.ID] = StatData.Strength
		end

		for StatData in NTHuman.LocalStats.BoolStats do
			CharData.stats[StatData.ID] = StatData.Strength
		end

		NT.CreateLimbTables(CharData)
		NT.UpdateHuman(NTHuman.Human,CharData,NTHuman,priorities)

	end
end)

NTLua.Add("SyncCharacters", function(characterlist)
	for NTHuman in characterlist do
		NTC.AddEmptyCharacterData(NTHuman.Human)
	end
end)

NTLua.Add("SyncCharacterSpeed", function(character,speed)
	NTC.CharacterSpeedMultipliers[character] = speed
end)

NTLua.Add("SyncPreHumanUpdateHooks", function(character)
	-- pre humanupdate hooks
	for key, val in pairs(NTC.PreHumanUpdateHooks) do
		val(character)
	end
end)

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

function NT.UpdateHuman(character, currentCharData, NTHuman, Priorities)
	-- Doing this additional check here enables NT updates for 'important' people like players, crew and AI opponents from the get-go
	-- instead of waiting on other interactions before updates start. - Lukako
	if character.IsHuman and character.TeamID == 1 or character.TeamID == 2 and not character.IsDead then
	else
		-- Original check
		if not HF.HasAffliction(character, "luabotomy") then return end
	end

	NTC.CharacterSpeedMultipliers[character] = 1

	local charData = currentCharData

	-- fetch all the current affliction data
	for identifier, data in pairs(NT.Afflictions) do
			local strength = HF.GetAfflictionStrength(character,  NT.ConvertToModern(identifier), data.default or 0)
			charData.afflictions[identifier] = { prev = strength, strength = strength }
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
			if data.const ~= false or (data.const == false and HF.HasAffliction(charData,identifier,data.default or 0)) then -- Const check
				if data.update ~= nil and (Priorities.Contains(data.priority or AfflictionPriority.HIGH)) then data.update(charData, identifier) end
			end
		end
	end

	-- update and apply limb specific stuff
	local function FetchLimbData(type)
		local keystring = tostring(type) .. "afflictions"
		for identifier, data in pairs(NT.LimbAfflictions) do
				local strength = HF.GetAfflictionStrengthLimb(character, type, NT.ConvertToModern(identifier), data.default or 0)
				charData[keystring][identifier] = { prev = strength, strength = strength }
		end
	end
	local function UpdateLimb(type, stasisflag)
		local keystring = tostring(type) .. "afflictions"
		for identifier, data in pairs(NT.LimbAfflictions) do
			if data.const ~= false or (data.const == false and HF.HasAfflictionLimb(charData,identifier,type,data.default or 0)) then -- Const check
				if NT.LegacyLimbAfflictions[identifier] == nil and not NT.LimbAfflictions[identifier].legacy then
					if
						data.update ~= nil and (not data.priority or Priorities.Contains(AfflictionPriority.HIGH)) -- Priority check
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
	end
	local function ApplyLimb(type, stasisflag)
		local keystring = tostring(type) .. "afflictions"
		for identifier, data in pairs(charData[keystring]) do
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
						HF.SetAfflictionLimb(character, NT.ConvertToModern(identifier), type, newval)
					else
						NT.LimbAfflictions[identifier].apply(charData, identifier, type, newval)
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
			local newval =
				HF.Clamp(data.strength, NT.Afflictions[identifier].min or 0, NT.Afflictions[identifier].max or 100)
			if newval ~= data.prev then
				if NT.Afflictions[identifier].apply == nil then
					HF.SetAffliction(character, NT.ConvertToModern(identifier), newval)
				else
					NT.Afflictions[identifier].apply(charData, identifier, newval)
				end
		end
	end

	CSNTCompat.TickCharacterTags(NTHuman)

	-- humanupdate hooks
	NTHuman.UpdatePostHumanHooks()
	for key, val in pairs(NTC.HumanUpdateHooks) do
		val(character)
	end
	
	HF.SetAffliction(character,"slowdown", HF.Clamp(100 * (1 - NTHuman.GetDoubleStatStrength("speedmultiplier")), 0, 100));
	NTHuman.ClearSpeedMultiplier();
	NTC.CharacterSpeedMultipliers[character] = nil
end