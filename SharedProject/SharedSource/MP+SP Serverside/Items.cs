using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma;

public class NTItemMethods
{

    /// <summary>
    /// Contains all the data necessary to add an Affliction to DrainageAfflictions.
    /// </summary>
    public class ItemsAfflictionInfos { 
    
        /// <summary>
        /// The ID defined in the XML. The affliction CANNOT BE Limb-Specific.
        /// </summary>
        public string AfflictionID { get; }

        /// <summary>
        /// The amount of XP given to the surgery or medical skill when the item is applied successfully.
        /// </summary>
        public int XPGain { get; }

        /**<summary>This function will be run to know if the affliction can be cured by the drainage.</summary>
           <example>
           <code>
           bool conditionFunction(ItemUpdateFunctionInfos infos)
           {
               return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
           }
           </code>
           </example>
        */
        public Func<ItemUpdateFunctionInfos, bool> Conditions { get; }

        public ItemsAfflictionInfos(string affID, int xpGain, Func<ItemUpdateFunctionInfos, bool> conditions) 
        { 
            this.AfflictionID = affID;
            this.XPGain = xpGain;
            this.Conditions = conditions;
        }
    }

    /// <summary>
    /// Contains the list of every afflictions cured by drainage.
    /// </summary>
    public static List<ItemsAfflictionInfos> DrainageAfflictions { get; } = [];

    /// <summary>
    /// Contains the list of every afflictions removable by traumashears and knives.
    /// </summary>
    public static List<string> CuttableAfflictions { get; } = [];

    /// <summary>
    /// Contains the list of every afflictions removable by traumashears only.
    /// </summary>
    public static List<string> TraumaShearsAfflictions { get; } = [];

    /// <summary>
    /// Contains the list of every afflictions healable by sutures.
    /// </summary>
    public static List<ItemsAfflictionInfos> SutureAfflictions { get; } = [];

    /// <summary>
    /// Contains the list of detectable afflictions when using the Hematology Analyzer
    /// </summary>
    public static List<string> HematologyDetectable { get; } = [];

    /// <summary>
    /// Contains all the data necessary for an item use function.
    /// </summary>
    public class ItemUpdateFunctionInfos
    {
        public Barotrauma.Item item { get; }
        public Character user {  get; }
        public Character target { get; }
        public Limb targetLimb { get; }

        public ItemUpdateFunctionInfos(Barotrauma.Item item, Character user, Character target, Limb targetLimb)
        {
            this.item = item;
            this.user = user;
            this.target = target;
            this.targetLimb = targetLimb;
        }
    }

    public static Dictionary<string, Action<ItemUpdateFunctionInfos>> NTItemsRegistry { get; } = new Dictionary<string, Action<ItemUpdateFunctionInfos>> { };

    // Used by Diagnostic Tools to format chat output
    public static string FormatLine(string content, Color color)
    {
        if (string.IsNullOrEmpty(content)) { return ""; }
        return $"‖color:{color.R},{color.G},{color.B}‖{content}‖color:end‖";
    }

    // REFER TO README.MD WITHIN THE ITEMS FOLDER IN ASSETS!!!!!!!!
    // REFER TO README.MD WITHIN THE ITEMS FOLDER IN ASSETS!!!!!!!!
    // REFER TO README.MD WITHIN THE ITEMS FOLDER IN ASSETS!!!!!!!!

    public static void DefineAllItems()
    {
        // ============== Blood ==============
        // Add the Hematology Detectable afflictions in here
        HematologyDetectable.AddRange(
        [
            "sepsis",
            "immunity",
            "acidosis",
            "alkalosis",
            "bloodloss",
            "bloodpressure",
            "afimmunosuppressant",
            "afthiamine",
            "afadrenaline",
            "afstreptokinase",
            "afantibiotics",
            "afsaline",
            "afringerssolution",
            "afpressuredrug",
            "afopioid",
            "afanaesthetic"
        ]);

        // Hematology Analyzer
        RegisterItemUseFunction("bloodanalyzer", infos =>
        {
            // Only work if not on cooldown
            if (infos.item.Condition < 50) return;
            bool success = HF.GetSkillRequirementMet(infos.user, "medical", 30);
            float BloodLossInduced = 3f;
            if (success) BloodLossInduced = 1f;

            HF.AddAffliction(infos.target, "bloodloss", BloodLossInduced, infos.user);

            // Spawn donor card
            var ContainedItem = infos.item.OwnInventory.GetItemAt(0);

            bool HasCartridge = ContainedItem != null &&
                (
                    ContainedItem.Prefab.Identifier.Value == "bloodcollector" ||
                    ContainedItem.HasTag("donorCard")
                );

            if (HasCartridge)
            {
                HF.RemoveItem(ContainedItem);

                string BloodType = NTBloodTypes.GetBloodType(infos.target);

                var TargetIDCard = infos.target.Inventory.GetItemAt(0);

                if (TargetIDCard != null && TargetIDCard.OwnInventory != null && TargetIDCard.OwnInventory.GetItemAt(0) == null)
                {
                    HF.PutItemInContainer(TargetIDCard, BloodType + "card");
                }
                else
                {
                    HF.PutItemInContainer(infos.item, BloodType + "card");
                }
            }

            bool useColoredScanner = NTConfig.Get("NTSCAN_enablecoloredscanner", true);

            Color baseColor = useColoredScanner ? HF.GetColor("NTSCAN_basecolor") : new Color(127, 255, 255);
            Color nameColor = useColoredScanner ? HF.GetColor("NTSCAN_namecolor") : new Color(127, 255, 255);
            Color lowColor = useColoredScanner ? HF.GetColor("NTSCAN_lowcolor") : new Color(127, 255, 255);
            Color medColor = useColoredScanner ? HF.GetColor("NTSCAN_medcolor") : new Color(127, 255, 255);
            Color highColor = useColoredScanner ? HF.GetColor("NTSCAN_highcolor") : new Color(127, 255, 255);
            Color vitalColor = useColoredScanner ? HF.GetColor("NTSCAN_vitalcolor") : new Color(127, 255, 255);
            Color removalColor = useColoredScanner ? HF.GetColor("NTSCAN_removalcolor") : new Color(127, 255, 255);
            Color customColor = useColoredScanner ? HF.GetColor("NTSCAN_customcolor") : new Color(127, 255, 255);

            // Floats
            float lowMedThreshold = NTConfig.Get<float>("NTSCAN_lowmedThreshold", 1);
            float medHighThreshold = NTConfig.Get<float>("NT_medhighThreshold", 1);

            // Strings
            List<string> vitalCategory = NTConfig.Get<List<string>>("NTSCAN_VitalCategory", []);
            List<string> removalCategory = NTConfig.Get<List<string>>("NTSCAN_RemovalCategory", []);
            List<string> customCategory = NTConfig.Get<List<string>>("NTSCAN_CustomCategory", []);
            List<string> ignoredCategory = NTConfig.Get<List<string>>("NTSCAN_IgnoredCategory", []);

            // Not changeable
            List<string> pressureCategory = ["bloodpressure"];

            // Readout Strings
            string lowPressureReadout = "";
            string highPressureReadout = "";
            string lowStrengthReadout = "";
            string mediumStrengthReadout = "";
            string highStrengthReadout = "";
            string vitalReadout = "";
            string removalReadout = "";
            string customReadout = "";

            string BloodTypeName = AfflictionPrefab.Prefabs[NTBloodTypes.GetBloodType(infos.target)].Name.Value;

            string startReadout =
                $"‖color:{nameColor.R},{nameColor.G},{nameColor.B}‖" +
                $"Bloodtype: {BloodTypeName}" +
                "‖color:end‖\n" +
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                $"Affliction readout for {infos.target.Name}:" +
                "‖color:end‖\n";

            int afflictionsDisplayed = 0;

            HashSet<string> checkedAfflictions = [];

            // I personally like using brackets in single-line if statements but this is way more compact - Lukako
            foreach (var value in infos.target.CharacterHealth.GetAllAfflictions())
            {
                float strength = MathF.Round(value.Strength);
                var prefab = value.Prefab;
                string id = value.Identifier.Value;

                if (strength <= 2) continue;
                if (!HematologyDetectable.Contains(prefab.Identifier.Value)) continue;
                if (!checkedAfflictions.Add(id)) continue;
                if (ignoredCategory.Contains(id)) continue;

                string entry = $"\n{prefab.Name.Value}: {strength}%";

                bool isVital = vitalCategory.Contains(id);
                bool isRemoval = removalCategory.Contains(id);
                bool isCustom = customCategory.Contains(id);
                bool isPressure = pressureCategory.Contains(id);

                if (isVital) vitalReadout += entry;
                else if (isRemoval) removalReadout += entry;
                else if (isCustom) customReadout += entry;
                else if (isPressure)
                {
                    if (strength > 130 || strength < 70) highPressureReadout += entry;
                    else lowPressureReadout += entry;
                }
                else
                {
                    if (strength < lowMedThreshold) lowStrengthReadout += entry;
                    else if (strength < medHighThreshold) mediumStrengthReadout += entry;
                    else highStrengthReadout += entry;
                }

                afflictionsDisplayed++;
            }

            if (afflictionsDisplayed <= 0) lowStrengthReadout += "\nNo blood pressure detected...";

            HF.DMClient(
                HF.CharacterToClient(infos.user),
                startReadout
                    + FormatLine(lowPressureReadout, lowColor)
                    + FormatLine(highPressureReadout, highColor)
                    + FormatLine(lowStrengthReadout, lowColor)
                    + FormatLine(mediumStrengthReadout, medColor)
                    + FormatLine(highStrengthReadout, highColor)
                    + FormatLine(vitalReadout, vitalColor)
                    + FormatLine(removalReadout, removalColor)
                    + FormatLine(customReadout, customColor),
                null
            );
        });

        // Blood Packs (A, B, AB, 0)
        // TODO

        // Empty Blood Pack
        RegisterItemUseFunction("emptybloodpack", infos =>
        {
            if (infos.item.condition <= 0) return;

            // changing from 31 to somthing like 15 can stop easy station kill by using two blood pack in a row
            // Minor Spelling Mistake :skull: - Lukako
            float BloodLossStrength = HF.GetAfflictionStrength(infos.target, "bloodloss", 0);
            if (BloodLossStrength >= 31f) return;

            bool success = HF.GetSkillRequirementMet(infos.user, "medical", 30);
            int bloodlossinduced = success ? 40 : 30;

            string bloodtype = NTBloodTypes.GetBloodType(infos.target);

            // We store the data we need for the item tags
            double acidosis = HF.GetAfflictionStrength(infos.target, "acidosis", 0);
            double alkalosis = HF.GetAfflictionStrength(infos.target, "alkalosis", 0);
            double sepsis = HF.GetAfflictionStrength(infos.target, "sepsis", 0);

            HF.SetAffliction(infos.target, "acidosis", (float)HF.GetAfflictionStrength(infos.target, "acidosis", 0) * (float)0.9, infos.user, 0);
            HF.SetAffliction(infos.target, "alkalosis", (float)HF.GetAfflictionStrength(infos.target, "alkalosis", 0) * (float)0.9, infos.user, 0);
            HF.AddAffliction(infos.target, "bloodloss", bloodlossinduced, infos.user);

            string btID = bloodtype == "o_negative" ? "antibloodloss2" : "bloodpack" + bloodtype;

            // Inshallah ca marche -Cookie
            HF.GiveItemPlusFunction(btID, infos.user, (args) => {

                string[] tags = [];

                double acid = (double)args[0];
                double alkal = (double)args[1];
                double seps = (double)args[2];

                if (acid > 0) tags.Append($"acid:{Math.Round(acid)}");
                if (alkal > 0) tags.Append($"alkal:{Math.Round(alkal)}");
                if (seps > 0) tags.Append($"sepsis");

                Item item = (Item)args[3];

                foreach (var tag in tags) item.AddTag(tag);

            }, acidosis, alkalosis, sepsis);

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // ============== BodyParts ==============
        // Liver Transplant
        // TODO

        // Lung Transplant
        // TODO

        // Kidney Transplant
        // TODO

        // Heart Transplant
        // TODO

        // Brain Transplant
        // TODO

        // Extremity Transplants
        // TODO
        // In Lua, these use the reattachLimb HF for each specific limbtype; RightArm, LeftArm, RightLeg, LeftLeg.

        // Bionic Prosthetics
        // TODO
        // In Lua, these just re-use the Arm/Leg transplants above, respective to the prosthetic.

        // ============== Consumables ==============
        // Antibiotic Ointment
        RegisterItemUseFunction("ointment", (infos) =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user, "medical", 10);

            HF.AddAfflictionLimb(infos.target, "ointmented", infos.targetLimb.type, success ? 120 : 60, infos.user);
            HF.AddAfflictionLimb(infos.target, "infectedwound", infos.targetLimb.type, success ? -72 : -24, infos.user);

            // Check for third degree burn might not be working correctly
            if (HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "burn", 0) < 50)
            {
                HF.AddAfflictionLimb(infos.target, "burn", infos.targetLimb.type, success ? -12 : (float)-7.2, infos.user);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_ointment");
        });

        // Azathioprine
        RegisterItemUseFunction("immunosuppressant", infos =>
        {
            // XML applied 5 or 3 strength for 10 seconds; use a new HF to do the same. - Lukako
            // TODO: Emulate MultiplyByMaxVitality
            bool success = HF.GetSkillRequirementMet(infos.user, "Medical", 10);

            int totalAmount = success ? 50 : 30;
            int duration = 10;

            HF.ApplyAfflictionOverTime(infos.target, "afimmunosuppressant", totalAmount, duration, infos.user);

            // Technically, this is different from the XML original; this applies 1 Sepsis instantly, the other had a 50% chance to apply per tick.
            if (!success && HF.Chance(0.5f))
            {
                HF.AddAffliction(infos.target, "sepsis", 1f, null);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_pills");
        });

        // Tourniquet
        RegisterItemUseFunction("tourniquet", infos =>
        {
            if (HF.HasAfflictionLimb(infos.target, "tourniqueted", infos.targetLimb.type, 1)) return;

            // Failure
            if (!HF.GetSkillRequirementMet(infos.user, "medical", 30))
            {
                HF.AddAfflictionLimb(infos.target, "blunttrauma", infos.targetLimb.type, 6, infos.user);
                return;
            }

            if (HF.LimbIsExtremity(infos.targetLimb.type))
            {
                HF.SetAfflictionLimb(infos.target, "tourniqueted", infos.targetLimb.type, 100, infos.user, 0);
            }
            else if (infos.targetLimb.type == LimbType.Head)
            {
                HF.SetAffliction(infos.target, "oxygenlow", 200, infos.user, 0);
                HF.AddAffliction(infos.target, "neurotrauma", 15, infos.user);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_bandage");
        });

        // Gel Ice Pack
        RegisterItemUseFunction("gelipack", infos =>
        {
            if (infos.item.Condition < 25) return;

            float success = HF.BoolToNum(HF.GetSkillRequirementMet(infos.user, "medical", 40));
            HF.AddAfflictionLimb(infos.target, "iced", infos.targetLimb.type, 75 + success * 25, infos.user);
            
            infos.item.Condition = infos.item.Condition - 35;
            HF.GiveItem(infos.target, "ntsfx_bandage");
        });

        // Gypsum
        RegisterItemUseFunction("gypsum", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            // Needs to be bandaged, not already in a cast, not during a surgery, and the limb needs to be extremity.
            if (!HF.HasAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, (float)0.1) ||
            HF.HasAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, (float)0.1) ||
            HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, (float)0.1) ||
            !HF.LimbIsExtremity(infos.targetLimb.type)) { return; }

            if (HF.GetSkillRequirementMet(infos.user, "medical", (float)40))
            {
                HF.SetAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, 0, infos.user, 0);
                HF.SetAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, 100, infos.user, 0);
                HF.BreakLimb(infos.target, infos.targetLimb.type, -20);
                HF.GiveSkillScaled(infos.user, "medical", 6000);

            }

            HF.RemoveItem(infos.item);
        });

        // Ringer's Solution
        // TODO

        // Mannitol
        // TODO

        // Thiamine
        // TODO

        // Streptokinase
        RegisterItemUseFunction("streptokinase", infos =>
        {
            HF.AddAffliction(infos.target, "heartattack", -100, infos.user);
            HF.AddAffliction(infos.target, "hemotransfusionshock", -100, infos.user);
            HF.AddAffliction(infos.target, "afstreptokinase", 50, infos.user);

            if (HF.HasAffliction(infos.target, "stroke"))
            {
                HF.AddAffliction(infos.target, "stroke", 5, infos.user);
                HF.AddAffliction(infos.target, "cerebralhypoxia", 10, infos.user);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // Propofol
        RegisterItemUseFunction("propofol", infos =>
        {
            float anesthesiaStrength = HF.GetAfflictionStrength(infos.target, "anesthesia", 0);
            float anesthesiaGained = 1;

            if (HF.HasTalent(infos.user, "ntsp_propofol")) anesthesiaGained = 15;

            if (anesthesiaStrength < 15)
            {
                HF.AddAffliction(infos.target, "anesthesia", anesthesiaGained, infos.user);
            }
            else
            {
                anesthesiaGained = 15 - anesthesiaStrength;
                HF.AddAffliction(infos.target, "anesthesia", anesthesiaGained, infos.user);
            }

            HF.AddAffliction(infos.target, "afanaesthetic", 100, infos.user);
            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");

        });

        // ============== OtherEquipment ==============
        // Manual Defibrillator
        // TODO

        // AED
        // TODO

        // Blue Shark
        // TODO

        // ============== Overrides ==============
        // Wrench + Variants
        // TODO

        // Health Scanner
        RegisterItemUseFunction("healthscanner", infos =>
        {
            LimbType limbType = HF.NormalizeLimbType(infos.targetLimb.type);

            var Battery = infos.item.OwnInventory.GetItemAt(0);
            if (Battery == null) return;

            bool HasVoltage = Battery.Condition > 0;
            if (!HasVoltage) return;

            bool useColoredScanner = NTConfig.Get("NTSCAN_enablecoloredscanner", true);

            Color baseColor = useColoredScanner ? HF.GetColor("NTSCAN_basecolor") : new Color(127, 255, 255);
            Color nameColor = useColoredScanner ? HF.GetColor("NTSCAN_namecolor") : new Color(127, 255, 255);
            Color lowColor = useColoredScanner ? HF.GetColor("NTSCAN_lowcolor") : new Color(127, 255, 255);
            Color medColor = useColoredScanner ? HF.GetColor("NTSCAN_medcolor") : new Color(127, 255, 255);
            Color highColor = useColoredScanner ? HF.GetColor("NTSCAN_highcolor") : new Color(127, 255, 255);
            Color vitalColor = useColoredScanner ? HF.GetColor("NTSCAN_vitalcolor") : new Color(127, 255, 255);
            Color removalColor = useColoredScanner ? HF.GetColor("NTSCAN_removalcolor") : new Color(127, 255, 255);
            Color customColor = useColoredScanner ? HF.GetColor("NTSCAN_customcolor") : new Color(127, 255, 255);

            // Floats
            float lowMedThreshold = NTConfig.Get<float>("NTSCAN_lowmedThreshold", 1);
            float medHighThreshold = NTConfig.Get<float>("NT_medhighThreshold", 1);

            // Strings
            List<string> vitalCategory = NTConfig.Get<List<string>>("NTSCAN_VitalCategory", new List<string>());
            List<string> removalCategory = NTConfig.Get<List<string>>("NTSCAN_RemovalCategory", new List<string>());
            List<string> customCategory = NTConfig.Get<List<string>>("NTSCAN_CustomCategory", new List<string>());
            List<string> ignoredCategory = NTConfig.Get<List<string>>("NTSCAN_IgnoredCategory", new List<string>());

            // Not changeable
            List<string> pressureCategory = new() { "bloodpressure" };

            // Readout strings
            string lowPressureReadout = "";
            string highPressureReadout = "";
            string lowStrengthReadout = "";
            string mediumStrengthReadout = "";
            string highStrengthReadout = "";
            string vitalReadout = "";
            string removalReadout = "";
            string customReadout = "";

            // Character effects
            HF.GiveItem(infos.target, "ntsfx_selfscan");
            Battery.Condition -= 5;

            HF.AddAffliction(infos.target, "radiationsickness", 1, infos.user);
            HF.AddAffliction(infos.user, "radiationsickness", (float)0.6, infos.user);

            // Print readout of afflictions
            string startReadout =
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                "Affliction readout for " +
                "‖color:end‖" +
                $"‖color:{nameColor.R},{nameColor.G},{nameColor.B}‖" +
                infos.target.Name +
                "‖color:end‖" +
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                " on limb " +
                HF.LimbToString(limbType) +
                ":\n" +
                "‖color:end‖";

            var afflictionList = infos.target.CharacterHealth.GetAllAfflictions();
            int afflictionsDisplayed = 0;

            foreach (var value in afflictionList)
            {
                float strength = MathF.Round(value.Strength);
                var prefab = value.Prefab;
                var afflictionLimb = infos.target.CharacterHealth.GetAfflictionLimb(value);

                LimbType afflimbtype = LimbType.Torso;
                if (!prefab.LimbSpecific)
                {
                    afflimbtype = prefab.IndicatorLimb;
                }
                else if (afflictionLimb != null)
                {
                    afflimbtype = afflictionLimb.type;
                }

                afflimbtype = HF.NormalizeLimbType(afflimbtype);

                if (strength >= prefab.ShowInHealthScannerThreshold && afflimbtype == limbType)
                {
                    string id = value.Identifier.Value;
                    bool isIgnored = ignoredCategory.Contains(id);

                    if (!isIgnored)
                    {
                        string name = prefab.Name.Value;
                        string entry = $"\n{name}: {strength}%";

                        // Check which category the affliction should be in
                        bool isVital = vitalCategory.Contains(id);
                        bool isRemoval = removalCategory.Contains(id);
                        bool isCustom = customCategory.Contains(id);
                        bool isPressure = pressureCategory.Contains(id);

                        // Add it to the respective readout
                        if (isVital)
                        {
                            vitalReadout += entry;
                        }
                        else if (isRemoval)
                        {
                            removalReadout += entry;
                        }
                        else if (isCustom)
                        {
                            customReadout += entry;
                        }
                        else if (isPressure)
                        {
                            if (strength > 130 || strength < 70)
                            {
                                highPressureReadout += entry;
                            }
                            else
                            {
                                lowPressureReadout += entry;
                            }
                        }

                        // If not in an already mentioned category, just apply normal colour logic
                        else
                        {
                            if (strength < lowMedThreshold)
                            {
                                lowStrengthReadout += entry;
                            }
                            else if (strength < medHighThreshold)
                            {
                                mediumStrengthReadout += entry;
                            }
                            else
                            {
                                highStrengthReadout += entry;
                            }
                        }

                        afflictionsDisplayed++;
                    }
                }
            }
            // Add a message in case there is nothing to display
            if (afflictionsDisplayed <= 0)
            {
                lowStrengthReadout += "\nNo afflictions! Good work!";
            }

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                HF.DMClient(
                    HF.CharacterToClient(infos.user),
                    startReadout
                        + FormatLine(lowPressureReadout, lowColor)
                        + FormatLine(highPressureReadout, highColor)
                        + FormatLine(lowStrengthReadout, lowColor)
                        + FormatLine(mediumStrengthReadout, medColor)
                        + FormatLine(highStrengthReadout, highColor)
                        + FormatLine(vitalReadout, vitalColor)
                        + FormatLine(removalReadout, removalColor)
                        + FormatLine(customReadout, customColor),
                    null
                );
            }, 2000);
        });

        // Alien Blood
        // REWRITTEN FROM XML
        RegisterItemUseFunction("alienblood", infos =>
        {
            if (HF.GetSkillRequirementMet(infos.user, "medical", 55f))
            {
                HF.AddAffliction(infos.target, "bloodloss", 20f, infos.user);
                HF.AddAffliction(infos.target, "hemotransfusionshock", 100f, infos.user);
                HF.AddAffliction(infos.target, "psychosis", 30f, infos.user);
                HF.AddAffliction(infos.target, "bloodpressure", 20f, infos.user);

            }
            else
            {
                HF.AddAffliction(infos.target, "bloodloss", 15f, infos.user);
                HF.AddAffliction(infos.target, "hemotransfusionshock", 100f, infos.user);
                HF.AddAffliction(infos.target, "psychosis", 30f, infos.user);
                HF.AddAffliction(infos.target, "bloodpressure", 15f, infos.user);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // Saline
        // TODO

        // Bandage
        // TODO

        // Plastiseal
        // TODO

        // Opium
        // TODO

        // Morphine
        // TODO

        // Fentanyl
        // TODO

        // Naloxone
        // TODO

        // Broad-Spectrum Antibiotics
        // TODO

        // Adrenaline
        RegisterItemUseFunction("adrenaline", infos =>
        {
            HF.AddAffliction(infos.target, "afadrenaline", 55, infos.user);
            HF.AddAffliction(infos.target, "adrenalinerush", 8, infos.user);

            if (HF.HasAffliction(infos.target, "cardiacarrest", 0.1f))
            {
                HF.AddAffliction(infos.target, "cardiacarrest", -100, infos.user);
                HF.AddAffliction(infos.target, "fibrillation", 20, infos.user);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // Liquid Oxygenite
        // TODO

        // Deusizine
        // TODO

        // Antibiotic Glue
        // TODO

        // Methamphetamine
        // TODO

        // Hyperzine
        // TODO

        // Haloperidol
        // TODO

        // Anaparalyzant
        // REWRITTEN FROM XML
        RegisterItemUseFunction("antiparalysis", infos =>
        {
            if (HF.GetSkillRequirementMet(infos.user, "medical", 64f))
            {
                HF.AddAffliction(infos.target, "paralysisresistance", 800f, infos.user);
                HF.AddAffliction(infos.target, "psychosis", 5f, infos.user);
                HF.AddAffliction(infos.target, "anesthesia", -200f, infos.user);
                HF.AddAffliction(infos.target, "afanaesthetic", -200f, infos.user);
            }
            else
            {

                // TODO Apply it over time

            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // Tonic Liquid
        // TODO

        // Nitroglycerin
        // TODO

        // ============== SurgicalEquipment ==============
        // Sutures
        // Bad to the bones 💀
        SutureAfflictions.Add(new ItemsAfflictionInfos("bonecut", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("drilledbones", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        }));

        // Organs
        SutureAfflictions.Add(new ItemsAfflictionInfos("liverswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("heartswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("lungswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("kidneyswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("brainswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        }));

        // Arterialcuts

        SutureAfflictions.Add(new ItemsAfflictionInfos("arterialcut", 3, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("carotidarterialcut", 3, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("aorticrupture", 3, infos => {

            if (!NTConfig.Get<bool>("NT_HardmodeAorticRupture", false)) return false;

            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        }));

        // Tamponade

        SutureAfflictions.Add(new ItemsAfflictionInfos("tamponade", 3, infos => {

            if (NTConfig.Get<bool>("NT_OpenCloseTamponade", false)) return false;

            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        }));

        // Misc

        SutureAfflictions.Add(new ItemsAfflictionInfos("arteriesclamp", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("internalbleeding", 3, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("stroke", 6, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        }));

        // Surgery Related

        SutureAfflictions.Add(new ItemsAfflictionInfos("clampedbleeders", 0, infos => {
            return true;
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("surgeryincision", 0, infos => {
            return true;
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("retractedskin", 0, infos => {
            return true;
        }));

        SutureAfflictions.Add(new ItemsAfflictionInfos("caviclean", 0, infos => {
            return true;
        }));

        RegisterItemUseFunction("suture", infos =>
        {

            // Base NT has no stasis check ?

            if (!HF.GetSkillRequirementMet(infos.user, "medical", 30))
            {
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 6, infos.user);
                return;
            }

            // Common afflictions part
            double healeddamage = 0;

            // Could be better if HF.AddAfflictionLimb returned the amount healed
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "lacerations", 0), 0, 20);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "bitewounds", 0), 0, 20);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "explosiondamage", 0), 0, 20);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "gunshotwound", 0), 0, 20);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "bleeding", 0) / 10, 0, 40);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "bleedingnonstop", 0) / 10, 0, 40);

            HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, -20, infos.user);
            HF.AddAfflictionLimb(infos.target, "bitewounds", infos.targetLimb.type, -20, infos.user);
            HF.AddAfflictionLimb(infos.target, "explosiondamage", infos.targetLimb.type, -20, infos.user);
            HF.AddAfflictionLimb(infos.target, "gunshotwound", infos.targetLimb.type, -20, infos.user);
            HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, -40, infos.user);
            HF.AddAfflictionLimb(infos.target, "bleedingnonstop", infos.targetLimb.type, -40, infos.user);

            HF.AddAfflictionLimb(infos.target, "suturedw", infos.targetLimb.type, (float)healeddamage, infos.user);

            HF.GiveSkillScaled(infos.user, "medical", (float)healeddamage * 100);

            if (HF.HasAfflictionLimb(infos.target, "bonecut", infos.targetLimb.type, 1))
            {
                HF.SurgicallyAmputateLimbAndGenerateItem(infos.user, infos.target, infos.targetLimb.type);
            }

            HF.AddAffliction(infos.target, "tshocktimeout", -100, infos.user);

            // rewritten
            foreach (var affInfos in SutureAfflictions)
            {
                // If the target doesn't has the affliction, we skip it
                if (!HF.HasAfflictionLimb(infos.target, affInfos.AfflictionID, infos.targetLimb.type, 1)) continue;

                // If the affliction's conditions are not met, we skip it
                if (!affInfos.Conditions(infos)) continue;

                AfflictionPrefab prefab = AfflictionPrefab.Prefabs[affInfos.AfflictionID];

                if (prefab == null)
                {
                    LuaCsLogger.LogError($"Error trying to heal {affInfos.AfflictionID} with sutures. The providded ID is probably incorrect.");
                    continue;
                }

                if (prefab.LimbSpecific)
                {
                    HF.SetAfflictionLimb(infos.target, affInfos.AfflictionID, infos.targetLimb.type, 0, infos.user, 0);

                }
                else
                {
                    HF.SetAffliction(infos.target, affInfos.AfflictionID, 0, infos.user, 0);
                }

                HF.GiveSurgerySkill(infos.user, affInfos.XPGain);
            }
        });

        // Drainage
        DrainageAfflictions.Add(new ItemsAfflictionInfos("pneumothorax", 3, infos =>
        {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
        }));

        DrainageAfflictions.Add(new ItemsAfflictionInfos("tamponade", 3, infos =>
        {
            bool retractedSkin = HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);

            if (NTConfig.Get<bool>("NT_OpenCloseTamponade", false)) return false;

            return retractedSkin;
        }));

        // From 48 lines to 12 my point stands, why tf was the lua function so girthy?
        RegisterItemUseFunction("drainage", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            foreach (var affInfos in DrainageAfflictions)
            {
                if (!affInfos.Conditions(infos)) continue;

                HF.SetAffliction(infos.target, affInfos.AfflictionID, 0, infos.user, 0);
                HF.GiveSurgerySkill(infos.user, affInfos.XPGain);
            }
        });

        // Needle
        // TODO

        // Osteosynthesis Implants
        // TODO

        // Spinal Cord Implants
        // TODO

        // Scalpel
        RegisterItemUseFunction("advscalpel", infos =>
        {
            // Not in stasis
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            if (!HF.CanPerformSurgeryOn(infos.target) || HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 1)) { return; }

            bool success = HF.GetSurgerySkillRequirementMet(infos.user, 30);

            if (success)
            {
                HF.AddAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user) / 2, infos.user); // TODO change this to using surgery instead of medical

                HF.SetAfflictionLimb(infos.target, "suturedi", infos.targetLimb.type, 0, infos.user, 0);
                HF.SetAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, 0, infos.user, 0);
                HF.SetAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, 0, infos.user, 0);

            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 10, infos.user);
            }

            HF.GiveItem(infos.target, "ntsfx_slash");
        });

        // Hemostat
        RegisterItemUseFunction("advhemostat", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            if (!HF.CanPerformSurgeryOn(infos.target)) { return; }

            if (!HF.HasAffliction(infos.target, "surgeryincision", 99) || HF.HasAffliction(infos.target, "clampedbleeders", 1)) { return; }

            HF.AddAfflictionLimb(infos.target, "clampedbleeders", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user) / 2, infos.user);

        });

        // Skin Retractors
        RegisterItemUseFunction("advretractors", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            if (!HF.CanPerformSurgeryOn(infos.target)) { return; }

            if (!HF.HasAffliction(infos.target, "clampedbleeders", 99) || HF.HasAffliction(infos.target, "retractedskin", 1)) { return; }

            HF.AddAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user) / 2, infos.user);
        });

        // Surgical Drill
        // TODO

        // Surgical Saw
        // TODO

        // Tweezers
        // TODO

        // Organ Scalpels [Lungs, Heart, Kidneys, Liver, Brain]; used by the MultiScalpel + Toggleable StandAlone Scalpels
        // TODO

        // Trauma Shears
        CuttableAfflictions.Add("bandaged");
        CuttableAfflictions.Add("bandageddirty");
        CuttableAfflictions.Add("tourniqueted");

        TraumaShearsAfflictions.Add("gypsumcast");

        RegisterItemUseFunction("traumashears", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            List<string> cuttables = CuttableAfflictions;
            cuttables = [.. cuttables, .. TraumaShearsAfflictions];

            if (HF.GetSkillRequirementMet(infos.user, "medical", 10))
            {
                foreach (var affID in cuttables)
                {
                    HF.SetAfflictionLimb(infos.target, affID, infos.targetLimb.type, 0, infos.user, 0);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 10, infos.user);
            }

        });

        // Diving Knife (Technically Override but similar to shears)
        RegisterItemUseFunction("divingknife", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            List<string> cuttables = CuttableAfflictions;

            if (HF.GetSkillRequirementMet(infos.user, "medical", 30))
            {
                foreach (var affID in cuttables)
                {
                    HF.SetAfflictionLimb(infos.target, affID, infos.targetLimb.type, 0, infos.user, 0);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 10, infos.user);
            }

        });

        // Antiseptic Sprayer
        // TODO

        // ============== Toggleable ==============
        // Endovascular Balloon
        // TODO

        // Medical Stent
        // TODO

        // Antiseptic
        // TODO

        // Sodium Nitroprusside
        // TODO
    }

    /**<summary>
     * Register a new identifier => function to the NTItemRegistry.
     * The function will trigger when the item matching the identifier is used.
     * </summary>
     * <param name="itemID">The identifier described in the XML.</param>
     * <param name="function">The item update function.</param>
     * <returns>Returns true if the item was defined correctly (if it was not already defined). Returns false otherwise.</returns>
     */
    public static bool RegisterItemUseFunction(string itemID, Action<ItemUpdateFunctionInfos> function)
    {
        if (!NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry.Add(itemID, function);
            return true;
        }

        return false;
    }

    /**<summary>
     * Overrides an already defined function matching the itemID.
     * Does nothing if the itemID isn't defined in the registry.
     * </summary>
     * <param name="itemID">The identifier described in the XML.</param>
     * <param name="function">The item update function.</param>
     * <returns>Returns true if the override was succesful, false otherwise.</returns>
     */
    public static bool UpdateItemUseFunction(string itemID, Action<ItemUpdateFunctionInfos> function)
    {
        if (NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry.Add(itemID, function);
            return true;
        }

        return false;
    }


    /**
     * <summary>
     * The function patching the base game Item.ApplyTreatment
     * </summary>
     */
    public static void Override_ApplyTreatment(Barotrauma.Item __instance, Character user, Character character, Limb targetLimb)
    {

        string itemID = __instance.Prefab.Identifier.ToString();
        if (NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry[itemID].Invoke(new ItemUpdateFunctionInfos(__instance, user, character, targetLimb));
        }
    }

    /**
     * <summary>
     * The function patching the base game Item.Use
     * </summary>
     */
    public static void Override_Use(Barotrauma.Item __instance, float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
    {
       // LuaCsLogger.Log("use");
    }
}


