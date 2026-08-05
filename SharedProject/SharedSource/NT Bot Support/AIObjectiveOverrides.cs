// Considerations for Bot Support:
// 1. Treatment method in the basegame is via XML recommended treatments. This can work, but breaks on multi-step processes.
// 2. Multi-stage treatments are currently impossible.
// 3. CPR / Oxygen Tank usage are hardcoded.
// 4. Changes made to AI medicine must not break non-NT afflictions
// 5. Heal order checks all characters on the map, even non-humans / hostiles

// Original source code for these features:
// https://github.com/FakeFishGames/Barotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/AI/Objectives/AIObjectiveRescue.cs
// https://github.com/FakeFishGames/Barotrauma/blob/master/Barotrauma/BarotraumaShared/SharedSource/Characters/AI/Objectives/AIObjectiveRescueAll.cs

// Todo:
// 1. Override basegame treatment loop only for listed Neurotrauma afflictions; since the AI already 'know' all afflictions on a character anyway they dont need diagnostics
// 2. Change the way CPR / O2 is handled in line with NT functionality; random ass bots should not use CPR since it most likely wont do anything. Only enable that above a certain skill level, else look for AED?
// 3. Make all this a toggle in the config options
// 4. Figure out a way to decouple all this from vitality; since Vitality is hardly impacted by most NT afflictions bots will ignore them.
// 5. Add Triage somehow, so they wont be popping Thiamine into someone practically fine while someone else is missing an arm. Priority affliction healing, perhaps?
// 6. Skip non-humans / hostiles for healing