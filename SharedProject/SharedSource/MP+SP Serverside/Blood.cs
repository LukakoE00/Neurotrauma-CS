// Disable the Hook warning
#pragma warning disable CS0618
namespace Neurotrauma
{
    public static class NTBloodTypes
    {
        public static readonly (string Identifier, int Chance)[] BloodTypes =
        {
            // Affliction ID (they got changed!), Application Weight
            ("o_negative",  7), ("o_positive",  37),
            ("a_negative",  6), ("a_positive",  36),
            ("b_negative",  2), ("b_positive",   8),
            ("ab_negative", 1), ("ab_positive",  3),
        };

        // Randomize BloodType and set it
        private static readonly Random random = new Random();
        public static string RandomizeBloodType(Character character)
        {
            int roll = random.Next(0, 100); // 0–99 (dont forgor)
            int cumulative = 0;

            foreach (var (BloodType, Chance) in BloodTypes)
            {
                cumulative += Chance;

                if (cumulative > roll)
                {
                    HF.SetAffliction(character, BloodType, 100f, character, 0f);
                    // DEBUG HF.Print($"Applied BloodType: {BloodType} to {character.Name}");
                    return BloodType;
                }
            }
            return null;
        }

        // This is here so that the Immunity update script can force bloodtype application if it ever got removed
        // Remnant of HumanUpdate
        public static void TryRandomizeBlood(Character character)
        {
            GetBloodType(character);
        }

        // Check if character has a BloodType. If not, then apply one; else return the already existing one.
        public static string GetBloodType(Character character)
        {
            foreach (var (identifier, _) in BloodTypes)
            {
                if (HF.HasAffliction(character, identifier))
                {
                    return identifier;
                }
            }

            return RandomizeBloodType(character);
        }

        // Checks if a character has a BloodType. If not, return false; else return true.
        public static bool HasBloodType(Character character)
        {
            foreach (var (identifier, _) in BloodTypes)
            {
                if (HF.HasAffliction(character, identifier)) return true;
            }

            return false;
        }

        // Initialize the Lua Hooks.
        public static void InitializeBloodHooks()
        {
            // Character.Created
            LuaCsSetup.Instance.Hook.Add("character.created", "NTCS.BloodAndImmunity", (params object[] args) =>
            {
                var createdCharacter = args[0] as Barotrauma.Character;
                if (createdCharacter == null) return null;

                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    if (createdCharacter.IsHuman && !createdCharacter.IsDead)
                    {
                        GetBloodType(createdCharacter);

                        if (createdCharacter.CharacterHealth?.GetAffliction("immunity") == null)
                        {
                            HF.SetAffliction(createdCharacter, "immunity", 100f, null, 0f);
                        }
                    }
                }, 1000);

                return null;
            });

            // BloodAnalyzer
            LuaCsSetup.Instance.Hook.Add("OnInsertedIntoBloodAnalyzer", "NTCS.BloodAnalyzer",
            (params object[] args) =>
            {
                // Couldn't find a better way for this.
                var effect = args[0];

                var deltaTime = args[1];

                var item = args[2] as Barotrauma.Item;
                    if (item == null) return null;

                var targets = args[3];

                var position = args[4];

                var owner = item.GetRootInventoryOwner();
                    if (owner == null) return null;

                var character = owner as Barotrauma.Character;

                var contained = item.OwnInventory.GetItemAt(0);
                    if (contained == null) return null;
                    if (!(contained.HasTag("bloodbag") || contained.HasTag("allblood"))) return null;

                // Doesn't yet work in SP?
                HF.GiveItem(character, "ntsfx_syringe");

                // See if Colored Scanner is enabled; then adjust colors accordingly.
                bool UseColor = NTConfig.Get("NTSCAN_enablecoloredscanner", true);

                Color BaseColor = UseColor ? HF.GetColor("NTSCAN_basecolor") : new Color(127, 255, 255);
                Color NameColor = UseColor ? HF.GetColor("NTSCAN_namecolor") : new Color(127, 255, 255);
                Color LowColor = UseColor ? HF.GetColor("NTSCAN_lowcolor") : new Color(127, 255, 255);
                Color HighColor = UseColor ? HF.GetColor("NTSCAN_highcolor") : new Color(127, 255, 255);
                Color VitalColor = UseColor ? HF.GetColor("NTSCAN_vitalcolor") : new Color(127, 255, 255);
                
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    if (item == null) return;
                    if (character == null) return;
                    // Goofy but eh
                    var current = item.OwnInventory?.GetItemAt(0);
                    if (current != contained) return;

                    string id = contained.Prefab?.Identifier.Value ?? "";

                    string packtype = "o-";
                    if (id != "antibloodloss2" && id.Length > "bloodpack".Length)
                        packtype = id.Substring("bloodpack".Length);

                    string bloodTypeDisplay = packtype
                        .Replace("abc", "c")
                        .Replace("_positive", "+")
                        .Replace("_negative", "-")
                        .ToUpperInvariant();

                    string readout =
                        $"‖color:{BaseColor.R},{BaseColor.G},{BaseColor.B}‖Bloodpack: ‖color:end‖" +
                        $"‖color:{NameColor.R},{NameColor.G},{NameColor.B}‖{bloodTypeDisplay}‖color:end‖";

                    string defects = "";
                    var tags = (contained.Tags ?? "").Split(',');

                    // Check for defects and their strengths.
                    // Probably works. Cannot test easily until ItemMethods works! - Lukako
                    foreach (var raw in tags)
                    {
                        var t = raw.Trim();

                        if (t == "sepsis")
                        {
                            defects += $"\n‖color:{VitalColor.R},{VitalColor.G},{VitalColor.B}‖Sepsis detected!‖color:end‖";
                        }
                        else if (t.StartsWith("acid"))
                        {
                            var split = t.Split(':');
                            if (split.Length > 1)
                            {
                                defects += $"\n‖color:{HighColor.R},{HighColor.G},{HighColor.B}‖Acidosis: {split[1]}%‖color:end‖";
                            }
                        }
                        else if (t.StartsWith("alkal"))
                        {
                            var split = t.Split(':');
                            if (split.Length > 1)
                            {
                                defects += $"\n‖color:{HighColor.R},{HighColor.G},{HighColor.B}‖Alkalosis: {split[1]}%‖color:end‖";
                            }
                        }
                    }

                    // If defects present, print them.
                    if (string.IsNullOrEmpty(defects))
                    {
                        readout += $"\n‖color:{LowColor.R},{LowColor.G},{LowColor.B}‖No blood defects found.‖color:end‖";
                    }
                    else
                    {
                        readout += defects;
                    }

                    HF.DMClient(HF.CharacterToClient(character), readout, new Color(127, 255, 255, 255));

                }, 1500);

                return null;
            });
        }
    }
}