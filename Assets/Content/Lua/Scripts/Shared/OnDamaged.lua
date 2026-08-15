-- Has no functionality, just here for other addons to ref


NT.OnDamagedMethods = {}

-- cause foreign bodies, rib fractures, pneumothorax, tamponade, internal bleeding, fractures, neurotrauma
NT.OnDamagedMethods.gunshotwound = function(character, strength, limbtype)
	CSOnDamaged.GunshotWound(character,strength,limbtype)
end

-- cause foreign bodies, rib fractures, pneumothorax, internal bleeding, concussion, fractures
NT.OnDamagedMethods.explosiondamage = function(character, strength, limbtype)
	CSOnDamaged.ExplosionDamage(character,strength,limbtype)
end

-- cause rib fractures, pneumothorax, internal bleeding, concussion, fractures
NT.OnDamagedMethods.bitewounds = function(character, strength, limbtype)
	CSOnDamaged.BiteWounds(character,strength,limbtype)
end

-- cause rib fractures, pneumothorax, tamponade, internal bleeding, fractures
NT.OnDamagedMethods.lacerations = function(character, strength, limbtype)
	CSOnDamaged.Lacerations(character,strength,limbtype)
end

-- cause rib fractures, organ damage, pneumothorax, concussion, fractures, neurotrauma
NT.OnDamagedMethods.blunttrauma = function(character, strength, limbtype)
	CSOnDamaged.BluntTrauma(character,strength,limbtype)
end

-- cause rib fractures, organ damage, pneumothorax, concussion, fractures
NT.OnDamagedMethods.internaldamage = function(character, strength, limbtype)
	CSOnDamaged.InternalDamage(character,strength,limbtype)
end