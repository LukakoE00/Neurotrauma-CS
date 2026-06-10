--if Game.IsMultiplayer and CLIENT then return end

-- server-side code (also run in singleplayer)
print("Init Check")
if (Game.IsMultiplayer and SERVER) or not Game.IsMultiplayer then
	print("Init")
end