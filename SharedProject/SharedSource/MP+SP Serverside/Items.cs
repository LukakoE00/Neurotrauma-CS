using static Barotrauma.Items.Components.ItemComponent;
using static Neurotrauma.HumanUpdate;

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

        ///<summary>This function will be run to know if the affliction can be cured by the drainage.</summary>
        /// <example>
        /// <code>
        /// bool conditionFunction(ItemUpdateFunctionInfos infos)
        /// {
        ///     return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
        /// }
        /// </code>
        /// </example>
        public string Case { get; } = "";
        public Func<ItemUpdateFunctionInfos, bool> Conditions { get; }

        public ItemsAfflictionInfos(string affID, int xpGain, Func<ItemUpdateFunctionInfos, bool> conditions, string newCase = "") 
        { 
            AfflictionID = affID;
            XPGain = xpGain;
            Conditions = conditions;
            Case = newCase;
        }
    }

    /// <summary>
    /// A List containing Identifiers for all afflictions curable by using the Drainage item.
    /// </summary>
    public static Dictionary<string,ItemsAfflictionInfos> DrainageAfflictions { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all afflictions removable by either Trauma Shears or Diving Knives.
    /// </summary>
    public static List<string> CuttableAfflictions { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all afflictions removable by Trauma Shears.
    /// </summary>
    public static List<string> TraumaShearsAfflictions { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all afflictions healable by Sutures.
    /// </summary>
    public static Dictionary<string, ItemsAfflictionInfos> SutureAfflictions { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all afflictions detectable by the Blood Analyzer.
    /// </summary>
    public static List<string> HematologyDetectable { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all items with Wrench functionality.
    /// </summary>
    public static List<string> WrenchItems { get; } = [];

    /// <summary>
    /// A List containing Identifiers for all Blood Pack items.
    /// </summary>
    public static List<string> BloodPacks { get; } = [];

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

    /// <summary>
    /// Used by Diagnostic Tools to format the output to be printed in the chatbox.
    /// </summary>
    /// <param name="content">A chunk of text to add to the final print.</param>
    /// <param name="color">The color of this chunk of text.</param>
    /// <returns></returns>
    public static string FormatLine(string content, Color color)
    {
        if (string.IsNullOrEmpty(content)) { return ""; }
        return $"‖color:{color.R},{color.G},{color.B}‖{content}‖color:end‖";
    }

    /// <summary>
    /// Used by WrenchItems to cure Dislocations or cause Fractures.
    /// </summary>
    /// <param name="infos">Contextual data used in item functionality.</param>
    public static void WrenchFunctionality(NTItemMethods.ItemUpdateFunctionInfos infos)
    {
        if (HF.LimbIsDislocated(infos.target, infos.targetLimb.type, false))
        {
            float skillrequired = 60;

            if (HF.HasAffliction(infos.target, "analgesia", 0.5f) ||
                HF.HasAffliction(infos.target, "afadrenaline", 0.5f))
            {
                skillrequired -= 30;
            }

            if (HF.GetSkillRequirementMet(infos.user, "medical", skillrequired))
            {
                HF.DislocateLimb(infos.target, infos.targetLimb.type, -1000);
                HF.GiveSkillScaled(infos.user, "medical", 4000);
            }
            else
            {
                HF.BreakLimb(infos.target, infos.targetLimb.type, 1000);
            }

            if (!HF.HasAffliction(infos.target, "analgesia", 0.5f))
            {
                HF.AddAffliction(infos.target, "severepain", 5, infos.user);
            }

            HF.GiveItem(infos.target, "ntsfx_smack");
        }
        else if (!HF.HasAffliction(infos.target, "sym_unconsciousness", 0.1f))
        {
            var outerWearId = HF.GetOuterWearIdentifier(infos.target);

            if (outerWearId == "stasisbag" || outerWearId == "bodybag" || outerWearId == "autocpr")
            {
                var equippedOuterItem = HF.GetItemInOuterWear(infos.target);

                if (infos.user.Inventory.TryPutItem(equippedOuterItem, null, new List<InvSlotType> { InvSlotType.Any }))
                {
                    HF.GiveItem(infos.target, "ntsfx_velcro");
                }
            }
        }
    }

    /// <summary>
    /// Used by BloodPacks to decrease Blood Loss, store Alkalosis/Acidosis/Sepsis and handle Hemotransfusion Shock.
    /// </summary>
    /// <param name="infos">Contextual data used in item functionality.</param>
    public static void InfuseBloodpack(ItemUpdateFunctionInfos infos)
    {
        string id = infos.item.Prefab.Identifier.Value;

        bool packhasantibodyA = id is "bloodpacka_positive" or "bloodpacka_negative" or "bloodpackab_positive" or "bloodpackab_negative";
        bool packhasantibodyB = id is "bloodpackb_positive" or "bloodpackb_negative" or "bloodpackab_positive" or "bloodpackab_negative";
        bool packhasantibodyC = false; 
        bool packhasantibodyRh = id is "bloodpacko_positive" or "bloodpacka_positive" or "bloodpackb_positive" or "bloodpackab_positive";

        string targettype = NTBloodTypes.GetBloodType(infos.target);
        bool targethasantibodyA = targettype.Contains("a");
        bool targethasantibodyB = targettype.Contains("b");
        bool targethasantibodyC = targettype.Contains("c");
        bool targethasantibodyRh = targettype.Contains("positive");

        bool compatible = (targethasantibodyRh || !packhasantibodyRh)
                       && (targethasantibodyA || !packhasantibodyA)
                       && (targethasantibodyB || !packhasantibodyB)
                       && (targethasantibodyC || !packhasantibodyC);

        // TODO: give always true to team of bots on enemy submarines for future medic AI logic

        float bloodloss = HF.GetAfflictionStrength(infos.target, "bloodloss", 0);
        float usefulFraction = Math.Clamp(bloodloss / 30f, 0f, 1f);

        if (compatible)
        {
            HF.AddAffliction(infos.target, "bloodloss", -30, infos.user);
            HF.AddAffliction(infos.target, "bloodpressure", 30, infos.user);
            HF.GiveSkillScaled(infos.user, "medical", 4000 * HF.BoolToNum(bloodloss > 100));
        }
        else
        {
            HF.AddAffliction(infos.target, "bloodloss", -20, infos.user);
            HF.AddAffliction(infos.target, "bloodpressure", 30, infos.user);
            HF.GiveSkillScaled(infos.user, "medical", 4000 * HF.BoolToNum(bloodloss > 100));

            float immunity = HF.GetAfflictionStrength(infos.target, "immunity", 100);
            HF.AddAffliction(infos.target, "hemotransfusionshock", Math.Max(immunity - 6f, 0f), infos.user);
        }

        // Move towards isotonic
        HF.SetAffliction(infos.target, "acidosis", HF.GetAfflictionStrength(infos.target, "acidosis", 0) * Single.Lerp(1f, 0.9f, usefulFraction), null, 0);
        HF.SetAffliction(infos.target, "alkalosis", HF.GetAfflictionStrength(infos.target, "alkalosis", 0) * Single.Lerp(1f, 0.9f, usefulFraction), null, 0);

        // Check item tags for acidosis, alkalosis, sepsis
        string[] tags = infos.item.Tags.Split(',');

        foreach (string tag in tags)
        {
            string t = tag.Trim();

            if (t == "sepsis")
            {
                HF.AddAffliction(infos.target, "sepsis", 1f, infos.user);
            }
            else if (t.StartsWith("acid"))
            {
                string[] split = t.Split(':');
                if (split.Length > 1 && float.TryParse(split[1], out float acidVal)) HF.AddAffliction(infos.target, "acidosis", acidVal / 10f * usefulFraction, infos.user);
            }
            else if (t.StartsWith("alkal"))
            {
                string[] split = t.Split(':');
                if (split.Length > 1 && float.TryParse(split[1], out float alkalVal)) HF.AddAffliction(infos.target, "alkalosis", alkalVal / 10f * usefulFraction, infos.user);
            }
        }

        HF.RemoveItem(infos.item);
        HF.GiveItem(infos.user, "emptybloodpack");
        HF.GiveItem(infos.target, "ntsfx_syringe");
    }

    /// <summary>
    /// Used by Organ Scalpels to preferably deposit removed Organs in Refrigerated Containers, if present.
    /// </summary>
    /// <param name="transplantID">The identifier of the Organ to spawn.</param>
    /// <param name="usingCharacter">The character removing the organs.</param>
    /// <param name="condition">The spawn condition of the Organ.</param>
    private static void SpawnOrganTransplantInContainer(string transplantID, Character usingCharacter, float condition)
    {
        var container = usingCharacter.Inventory.GetItemInLimbSlot(InvSlotType.RightHand);
        if (container == null || container.OwnInventory == null || container.OwnInventory.IsFull())
            container = usingCharacter.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand);

        if (container != null && container.OwnInventory != null && !container.OwnInventory.IsFull())
        {
            HF.SpawnItemPlusFunction(transplantID, container.OwnInventory, InvSlotType.Any, usingCharacter.WorldPosition, (args) =>
            {
                Item item = (Item)args[0];
                item.Condition = condition;
            });
        }
        else
        {
            HF.GiveItemPlusFunction(transplantID, usingCharacter, (args) =>
            {
                Item item = (Item)args[0];
                item.Condition = condition;
            });
        }
    }


    /// <summary>
    /// Used by Extremity Transplants to remove Surgical Amputation and regenerate the limb.
    /// </summary>
    /// <param name="infos"></param>
    static void ReattachLimb(NTItemMethods.ItemUpdateFunctionInfos infos)
    {
        LimbType itemLimbType;

        switch (infos.item.Prefab.Identifier.Value)
        {
            case "rarm":
            case "rarmp":
                itemLimbType = LimbType.RightArm;
                break;

            case "larm":
            case "larmp":
                itemLimbType = LimbType.LeftArm;
                break;

            case "rleg":
            case "rlegp":
                itemLimbType = LimbType.RightLeg;
                break;

            case "lleg":
            case "llegp":
                itemLimbType = LimbType.LeftLeg;
                break;

            default:
                return;
        }

        LimbType limbType = HF.NormalizeLimbType(infos.targetLimb.type);

        if (limbType != itemLimbType) return;

        if (HF.HasAfflictionLimb(infos.target, "sawedbones", limbType, 99))
        {
            if (!HF.LimbIsAmputated(infos.target, limbType))
            {
                HF.SurgicallyAmputateLimbAndGenerateItem(infos.user, infos.target, limbType);
            }

            HF.SetAfflictionLimb(infos.target, "sawedbones", limbType, 0f, infos.user, 99);
            HF.SurgicallyAmputateLimb(infos.target, limbType, 0, 0);
            HF.RemoveItem(infos.item);
        }
    }

    /// <summary>
    /// Used to define functionality for given Items based on their ID. Replaces Lua ItemMethods.
    /// </summary>
    public static void DefineAllItems()
    {
        // ============== Blood ==============
        // Expands the list of HematologyDetectable afflictions.
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

            Color baseColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_basecolor") : new Color(127, 255, 255);
            Color nameColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_namecolor") : new Color(127, 255, 255);
            Color lowColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_lowcolor") : new Color(127, 255, 255);
            Color medColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_medcolor") : new Color(127, 255, 255);
            Color highColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_highcolor") : new Color(127, 255, 255);
            Color vitalColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_vitalcolor") : new Color(127, 255, 255);
            Color removalColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_removalcolor") : new Color(127, 255, 255);
            Color customColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_customcolor") : new Color(127, 255, 255);

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
        BloodPacks.AddRange(
        [
            "antibloodloss2",
            "bloodpacko_positive",
            "bloodpacka_positive",
            "bloodpacka_negative",
            "bloodpackb_positive",
            "bloodpackb_negative",
            "bloodpackab_positive",
            "bloodpackab_negative"
        ]);

        foreach (string id in BloodPacks)
        {
            RegisterItemUseFunction(id, InfuseBloodpack);
        }

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

            HF.GiveItemPlusFunction(btID, infos.user, (args) => {

                List<string> tags = [];

                double acid = (double)args[0];
                double alkal = (double)args[1];
                double seps = (double)args[2];

                if (acid > 0) tags.Add($"acid:{Math.Round(acid)}");
                if (alkal > 0) tags.Add($"alkal:{Math.Round(alkal)}");
                if (seps > 0) tags.Add("sepsis");

                Item item = (Item)args[3];

                foreach (var tag in tags) item.AddTag(tag);

            }, acidosis, alkalosis, sepsis);

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // ============== BodyParts ==============

        // ForEach are used to act like the 'startswith' method, to prevent us having to do this twice for each organ. - Lukako
        // Liver Transplant
        foreach (string id in new[] { "livertransplant", "livertransplant_q1" })
        {
            RegisterItemUseFunction(id, infos =>
            {
                if (infos.targetLimb.type != LimbType.Torso) return;

                if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99)) return;
                if (!HF.HasAffliction(infos.target, "liverremoved", 1) && !HF.HasAffliction(infos.target, "liverswap", 1)) return;

                float modifier = HF.GetSurgerySkillRequirementMet(infos.user, 40) ? 0f : -40f;
                float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);
                float damage = HF.GetAfflictionStrength(infos.target, "liverdamage", 0);

                HF.AddAffliction(infos.target, "liverdamage", -workcondition, infos.user);

                if (damage == 100f)
                {
                    HF.AddAffliction(infos.target, "liverdamage", -workcondition, infos.user);
                    HF.AddAffliction(infos.target, "organdamage", -workcondition / 5f, infos.user);
                    HF.SetAffliction(infos.target, "liverremoved", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "liverswap", 0f, infos.user, 0);
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    float newdamage = Math.Clamp((100f - damage) - workcondition, -100f, 100f);
                    HF.SetAffliction(infos.target, "liverdamage", 100f - workcondition, infos.user, 0);
                    HF.SetAffliction(infos.target, "liverremoved", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "liverswap", 0f, infos.user, 0);
                    HF.AddAffliction(infos.target, "organdamage", newdamage / 5f, infos.user);

                    string transplantID = NTC.HasTag(new NTHuman(infos.user), "organssellforfull") ? "livertransplant" : "livertransplant_q1";

                    if (damage < 90f)
                    {
                        HF.SpawnItemPlusFunction(transplantID, infos.item.ParentInventory, InvSlotType.Any, infos.user.WorldPosition, (args) => { ((Item)args[0]).Condition = 100f - damage; });
                        HF.RemoveItem(infos.item);
                    }
                }

                float rejectionchance = (float)Math.Clamp((HF.GetAfflictionStrength(infos.target, "immunity", 0) - 10f) / 150f * NTC.GetMultiplier(new NTHuman(infos.user), "organrejectionchance"), 0f, 1f);

                if (HF.Chance(rejectionchance) && NTConfig.Get("NT_organRejection", false)) HF.SetAffliction(infos.target, "liverdamage", 100f, infos.user, 0);
            });
        }

        // Heart Transplant
        foreach (string id in new[] { "hearttransplant", "hearttransplant_q1" })
        {
            RegisterItemUseFunction(id, infos =>
            {
                if (infos.targetLimb.type != LimbType.Torso) return;

                if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99)) return;
                if (!HF.HasAffliction(infos.target, "heartremoved", 1) && !HF.HasAffliction(infos.target, "heartswap", 1)) return;

                float modifier = HF.GetSurgerySkillRequirementMet(infos.user, 40) ? 0f : -40f;
                float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);
                float damage = HF.GetAfflictionStrength(infos.target, "heartdamage", 0);

                if (damage == 100f)
                {
                    HF.AddAffliction(infos.target, "heartdamage", -workcondition, infos.user);
                    HF.AddAffliction(infos.target, "organdamage", -workcondition / 5f, infos.user);
                    HF.SetAffliction(infos.target, "heartremoved", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "heartswap", 0f, infos.user, 0);
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    float newdamage = Math.Clamp((100f - damage) - workcondition, -100f, 100f);
                    HF.SetAffliction(infos.target, "heartdamage", 100f - workcondition, infos.target, 0);
                    HF.SetAffliction(infos.target, "heartremoved", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "heartswap", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "cardiacarrest", 100f, infos.target, 0);
                    HF.SetAffliction(infos.target, "tamponade", 0f, infos.target, 0);
                    HF.SetAffliction(infos.target, "heartattack", 0f, infos.target, 0);
                    HF.AddAffliction(infos.target, "organdamage", newdamage / 5f, infos.target);

                    string transplantID = NTC.HasTag(new NTHuman(infos.user), "organssellforfull") ? "hearttransplant" : "hearttransplant_q1";

                    if (damage < 90f)
                    {
                        HF.SpawnItemPlusFunction(transplantID, infos.item.ParentInventory, InvSlotType.Any, infos.user.WorldPosition, (args) => { ((Item)args[0]).Condition = 100f - damage; });
                        HF.RemoveItem(infos.item);
                    }
                }

                float rejectionchance = (float)Math.Clamp((HF.GetAfflictionStrength(infos.target, "immunity", 0) - 10f) / 150f * NTC.GetMultiplier(new NTHuman(infos.user), "organrejectionchance"), 0f, 1f);
                
                if (HF.Chance(rejectionchance) && NTConfig.Get("NT_organRejection", false)) HF.SetAffliction(infos.target, "heartdamage", 100f, infos.user, 0);
            });
        }

        // Lung Transplant
        foreach (string id in new[] { "lungtransplant", "lungtransplant_q1" })
        {
            RegisterItemUseFunction(id, infos =>
            {
                if (infos.targetLimb.type != LimbType.Torso) return;

                if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99)) return;
                if (!HF.HasAffliction(infos.target, "lungremoved", 1) && !HF.HasAffliction(infos.target, "lungswap", 1)) return;

                float modifier = HF.GetSurgerySkillRequirementMet(infos.user, 40) ? 0f : -40f;
                float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);
                float damage = HF.GetAfflictionStrength(infos.target, "lungdamage", 0);

                if (damage == 100f)
                {
                    HF.AddAffliction(infos.target, "lungdamage", -workcondition, infos.user);
                    HF.AddAffliction(infos.target, "organdamage", -workcondition / 5f, infos.user);
                    HF.SetAffliction(infos.target, "lungremoved", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "lungswap", 0f, infos.user, 0);
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    float newdamage = Math.Clamp((100f - damage) - workcondition, -100f, 100f);
                    HF.SetAffliction(infos.target, "lungdamage", 100f - workcondition, infos.target, 0);
                    HF.SetAffliction(infos.target, "lungremoved", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "lungswap", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "respiratoryarrest", 100f, infos.target, 0);
                    HF.SetAffliction(infos.target, "pneumothorax", 0f, infos.target, 0);
                    HF.SetAffliction(infos.target, "needlec", 0f, infos.target, 0);
                    HF.AddAffliction(infos.target, "organdamage", newdamage / 5f, infos.target);

                    string transplantID = NTC.HasTag(new NTHuman(infos.user), "organssellforfull")
                        ? "lungtransplant" : "lungtransplant_q1";

                    if (damage < 90f)
                    {
                        HF.SpawnItemPlusFunction(transplantID, infos.item.ParentInventory, InvSlotType.Any, infos.user.WorldPosition, (args) => { ((Item)args[0]).Condition = 100f - damage; });
                        HF.RemoveItem(infos.item);
                    }
                }

                float rejectionchance = (float)Math.Clamp((HF.GetAfflictionStrength(infos.target, "immunity", 0) - 10f) / 150f * NTC.GetMultiplier(new NTHuman(infos.user), "organrejectionchance"), 0f, 1f);
                
                if (HF.Chance(rejectionchance) && NTConfig.Get("NT_organRejection", false)) HF.SetAffliction(infos.target, "lungdamage", 100f, infos.user, 0);
            });
        }

        // Kidney Transplant
        foreach (string id in new[] { "kidneytransplant", "kidneytransplant_q1" })
        {
            RegisterItemUseFunction(id, infos =>
            {
                if (infos.targetLimb.type != LimbType.Torso) return;
                
                if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99)) return;
                if (!HF.HasAffliction(infos.target, "kidneyremoved", 1) && !HF.HasAffliction(infos.target, "kidneyswap", 1)) return;

                float modifier = HF.GetSurgerySkillRequirementMet(infos.user, 40) ? 0f : -40f;
                float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);
                float damage = HF.GetAfflictionStrength(infos.target, "kidneydamage", 0);

                float rejectionchance = (float)Math.Clamp((HF.GetAfflictionStrength(infos.target, "immunity", 0) - 10f) / 150f * NTC.GetMultiplier(new NTHuman(infos.user), "organrejectionchance"), 0f, 1f);
                
                if (HF.Chance(rejectionchance) && NTConfig.Get("NT_organRejection", false))
                {
                    HF.RemoveItem(infos.item);
                    return;
                }

                if (damage > 50f)
                {
                    LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                    {
                        HF.SetAffliction(infos.target, "kidneyremoved", 0f, infos.user, 0);
                        HF.SetAffliction(infos.target, "kidneyswap", 0f, infos.user, 0);
                    }, 3000);

                    HF.AddAffliction(infos.target, "kidneydamage", -workcondition / 2f, infos.user);
                    HF.AddAffliction(infos.target, "organdamage", -workcondition / 5f, infos.user);
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    float newdamage = Math.Clamp(((100f - damage) - workcondition) / 2f, -100f, 100f);
                    HF.SetAffliction(infos.target, "kidneyremoved", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "kidneyswap", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "kidneydamage", 50f - workcondition / 2f, infos.user, 0);
                    HF.AddAffliction(infos.target, "organdamage", newdamage / 5f, infos.user);

                    string transplantID = NTC.HasTag(new NTHuman(infos.user), "organssellforfull") ? "kidneytransplant" : "kidneytransplant_q1";

                    HF.RemoveItem(infos.item);

                    if (damage < 45f)
                    {
                        HF.SpawnItemPlusFunction(transplantID, infos.item.ParentInventory, InvSlotType.Any, infos.user.WorldPosition, (args) => { ((Item)args[0]).Condition = 100f - damage * 2f; });
                    }
                }
            });
        }

        // Brain Transplant
        RegisterItemUseFunction("braintransplant", infos =>
        {
            if (infos.targetLimb.type != LimbType.Head) return;
            if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type)) return;
            if (!HF.HasAffliction(infos.target, "brainremoved", 1)) return;

            float modifier = HF.GetSurgerySkillRequirementMet(infos.user, 100) ? 0f : -40f;
            float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);

            HF.AddAffliction(infos.target, "cerebralhypoxia", -workcondition, infos.user);
            HF.SetAffliction(infos.target, "brainremoved", 0f, infos.user, 0);

            #if SERVER
                string donorName = infos.item.Description;
                var client = HF.ClientFromName(donorName);
                if (client != null) client.SetClientCharacter(infos.target);
            #endif

            HF.RemoveItem(infos.item);
        });

        // Extremity Transplants
        RegisterItemUseFunction("rarm", ReattachLimb);
        RegisterItemUseFunction("larm", ReattachLimb);
        RegisterItemUseFunction("rleg", ReattachLimb);
        RegisterItemUseFunction("lleg", ReattachLimb);

        // Bionic Transplants
        RegisterItemUseFunction("rarmp", ReattachLimb);
        RegisterItemUseFunction("larmp", ReattachLimb);
        RegisterItemUseFunction("rlegp", ReattachLimb);
        RegisterItemUseFunction("llegp", ReattachLimb);

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
            HF.GiveItem(infos.target, "ntsfx_bandage");
        });

        // Ringer's Solution
        RegisterItemUseFunction("ringerssolution", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user, "Medical", 20);

            int totalAmount = success ? 50 : 30;
            int duration = 10;

            HF.ApplyAfflictionOverTime(infos.target, "afringerssolution", totalAmount, duration, infos.user);

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // Mannitol
        RegisterItemUseFunction("mannitol", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user, "Medical", 60);

            int duration = 10;

            if (success)
            {
                HF.ApplyAfflictionOverTime(infos.target, "afmannitol", 50, duration, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "organdamage", 5, duration, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "heartdamage", 10, duration, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "kidneydamage", 10, duration, infos.user);
            }
            else
            {
                HF.ApplyAfflictionOverTime(infos.target, "afmannitol", 30, duration, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "organdamage", 10, duration, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "heartdamage", 20, duration, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "kidneydamage", 20, duration, infos.user);
            }

            HF.GiveItem(infos.target, "ntsfx_syringe");
            HF.RemoveItem(infos.item);
        });

        // Thiamine
        RegisterItemUseFunction("thiamine", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user, "Medical", 10);

            int duration = 10;
            int totalAmount = success ? 50 : 30;

            HF.ApplyAfflictionOverTime(infos.target, "afthiamine", totalAmount, duration, infos.user);

            HF.GiveItem(infos.target, "ntsfx_pills");
            HF.RemoveItem(infos.item);
        });

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
        RegisterItemUseFunction("defibrillator", infos =>
        {
            if (infos.item.Condition <= 0) return;

            infos.item.Condition = 0; // Start Cooldown

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                infos.item.Condition = 100; // Finish Cooldown
            }, 5000);

            var battery = infos.item.OwnInventory.GetItemAt(0);
            if (battery == null) return;

            bool hasVoltage = battery.Condition > 0;
            if (!hasVoltage) return;

            HF.GiveItem(infos.target, "ntsfx_manualdefib");

            if (battery.Prefab.Identifier.Value != "fulguriumbatterycell")
            {
                battery.Condition -= 20;
            }
            else
            {
                battery.Condition -= 10;
            }

            float medicalSkill = HF.GetSkillLevel(infos.user, "medical");

            float successChance = MathF.Pow(medicalSkill / 100f, 2);
            float arrestSuccessChance = MathF.Pow(medicalSkill / 100f, 4);
            float arrestFailChance = MathF.Pow(1f - (medicalSkill / 100f), 2) * 0.3f;

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                HF.AddAffliction(infos.target, "stun", 2f, infos.user);

                if (HF.Chance(successChance))
                {
                    HF.SetAffliction(infos.target, "tachycardia", 0f, infos.user, 0);
                    HF.SetAffliction(infos.target, "fibrillation", 0f, infos.user, 0);
                }

                if (HF.Chance(arrestSuccessChance))
                {
                    HF.SetAffliction(infos.target, "cardiacarrest", 0f, infos.user, 0);
                }
            }, 2000);
        });

        // AED
        RegisterItemUseFunction("aed", infos =>
        {
            if (infos.item.Condition <= 0) return;

            infos.item.Condition = 0;

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                infos.item.Condition = 100;
            }, 5000);

            var battery = infos.item.OwnInventory.GetItemAt(0);
            if (battery == null) return;

            bool hasVoltage = battery.Condition > 0;
            if (!hasVoltage) return;

            bool actionRequired =
                HF.HasAffliction(infos.target, "tachycardia", 5) ||
                HF.HasAffliction(infos.target, "fibrillation", 1) ||
                HF.HasAffliction(infos.target, "cardiacarrest");

            if (!actionRequired)
            {
                HF.GiveItem(infos.target, "ntsfx_defib2");
                return;
            }

            HF.GiveItem(infos.target, "ntsfx_defib1");

            if (battery.Prefab.Identifier.Value != "fulguriumbatterycell")
            {
                battery.Condition -= 20;
            }
            else
            {
                battery.Condition -= 10;
            }

            float medicalSkill = HF.GetSkillLevel(infos.user, "medical");

            float arrestSuccessChance = Math.Clamp(medicalSkill / 200f, 0.2f, 0.4f);

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                HF.AddAffliction(infos.target, "stun", 2f, infos.user);
                HF.SetAffliction(infos.target, "tachycardia", 0f, infos.user, 0);
                HF.SetAffliction(infos.target, "fibrillation", 0f, infos.user, 0);

                if (HF.Chance(arrestSuccessChance))
                {
                    HF.SetAffliction(infos.target, "cardiacarrest", 0f, infos.user, 0);
                }

            }, 3200);
        });

        // Blue Shark
        RegisterItemUseFunction("blahaj", infos =>
        {
            HF.AddAffliction(infos.target, "psychosis", -2f, infos.user);
            HF.GiveItem(infos.target, "ntsfx_squeak");
        });

        // ============== Overrides ==============
        // Wrenches
        WrenchItems.AddRange(
        [
            "wrench",
            "heavywrench",
            "wrenchhardened",
            "repairpack",
            "wrench_murdermystery"
        ]);

        foreach (string id in WrenchItems)
        {
            RegisterItemUseFunction(id, WrenchFunctionality);
        }

        // Health Scanner
        RegisterItemUseFunction("healthscanner", infos =>
        {
            LimbType limbType = HF.NormalizeLimbType(infos.targetLimb.type);

            var Battery = infos.item.OwnInventory.GetItemAt(0);
            if (Battery == null) return;

            bool HasVoltage = Battery.Condition > 0;
            if (!HasVoltage) return;

            bool useColoredScanner = NTConfig.Get("NTSCAN_enablecoloredscanner", true);

            Color baseColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_basecolor") : new Color(127, 255, 255);
            Color nameColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_namecolor") : new Color(127, 255, 255);
            Color lowColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_lowcolor") : new Color(127, 255, 255);
            Color medColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_medcolor") : new Color(127, 255, 255);
            Color highColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_highcolor") : new Color(127, 255, 255);
            Color vitalColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_vitalcolor") : new Color(127, 255, 255);
            Color removalColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_removalcolor") : new Color(127, 255, 255);
            Color customColor = useColoredScanner ? HF.GetColorFromConfigEntry("NTSCAN_customcolor") : new Color(127, 255, 255);

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
        RegisterItemUseFunction("antibloodloss1", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user, "Medical", 10);

            int totalAmount = success ? 50 : 30;
            int duration = 10;

            HF.ApplyAfflictionOverTime(infos.target, "afsaline", totalAmount, duration, infos.user);

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // Bandage
        RegisterItemUseFunction("antibleeding1", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user, "medical", 10);
            bool hasMedExpertise = HF.HasTalent(infos.user, "medicalexpertise");

            // Cookie mentioned this would work
            int successNum = success ? 1 : 0;
            int talentNum = hasMedExpertise ? 1 : 0;

            HF.AddAfflictionLimb(infos.target, "bandageddirty", infos.targetLimb.type, -100, infos.user);
            HF.AddAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, 36 + successNum * 12 + talentNum * 12, infos.user);
            HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, -18 - successNum * 6 - talentNum * 6, infos.user);

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_bandage");
        });

        // Plastiseal
        RegisterItemUseFunction("antibleeding2", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user, "medical", 22);
            int successNum = success ? 1 : 0;

            HF.AddAfflictionLimb(infos.target, "bandageddirty", infos.targetLimb.type, -100, infos.user);
            HF.AddAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, 50 + successNum * 50, infos.user);
            HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, -24 - successNum * 24, infos.user);

            if (HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type))
            {
                float affAmount = HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "burn");
                float healedAmount = Math.Min(affAmount, 200f);

                HF.AddAfflictionLimb(infos.target, "burn", infos.targetLimb.type, -healedAmount, infos.user);
                HF.GiveSkillScaled(infos.user, "medical", (float)healedAmount * 150);
            }
            else if (HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "burn", 0) > 50f)
            {
                HF.AddAfflictionLimb(infos.target, "burn", infos.targetLimb.type, -12 - successNum * 12, infos.user);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_bandage");
        });

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
                HF.ApplyAfflictionOverTime(infos.target, "paralysisresistance", 390f, 60, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "psychosis", 45f, 60, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "anesthesia", -180f, 60, infos.user);
                HF.ApplyAfflictionOverTime(infos.target, "afanaesthetic", -180f, 60, infos.user);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // Nitroglycerin
        RegisterItemUseFunction("nitroglycerin", infos =>
        {
            if (HF.GetSkillRequirementMet(infos.user, "medical", 35f))
            {
                HF.AddAffliction(infos.target, "afpressuredrug", 100f, infos.user);
            }
            else
            {
                HF.AddAffliction(infos.target, "afpressuredrug", 50f, infos.user);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target, "ntsfx_syringe");
        });

        // ============== SurgicalEquipment ==============
        // Sutures
        // Bad to the bones 💀
        SutureAfflictions["sawedbones"] = new ItemsAfflictionInfos("sawedbones", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["drilledbones"] = new ItemsAfflictionInfos("drilledbones", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        });

        // Organs
        SutureAfflictions["liverswap"] = new ItemsAfflictionInfos("liverswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["heartswap"] = new ItemsAfflictionInfos("heartswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["lungswap"] = new ItemsAfflictionInfos("lungswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["kidneyswap"] = new ItemsAfflictionInfos("kidneyswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["brainswap"] = new ItemsAfflictionInfos("brainswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 95);
        });

        // Arterialcuts

        SutureAfflictions["arterialcut"] = new ItemsAfflictionInfos("arterialcut", 3, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        });

        SutureAfflictions["carotidarterialcut"] = new ItemsAfflictionInfos("carotidarterialcut", 3, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        });

        SutureAfflictions["aorticrupture"] = new ItemsAfflictionInfos("aorticrupture", 3, infos => {

            if (!NTConfig.Get<bool>("NT_HardmodeAorticRupture", false)) return false;

            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        });

        // Tamponade

        SutureAfflictions["tamponade"] = new ItemsAfflictionInfos("tamponade", 3, infos => {

            if (NTConfig.Get<bool>("NT_OpenCloseTamponade", false)) return false;

            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        });

        // Misc

        SutureAfflictions["arteriesclamp"] = new ItemsAfflictionInfos("arteriesclamp", 0, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        });

        SutureAfflictions["internalbleeding"] = new ItemsAfflictionInfos("internalbleeding", 3, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        });

        SutureAfflictions["stroke"] = new ItemsAfflictionInfos("stroke", 6, infos => {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 95);
        });

        // Surgery Related

        SutureAfflictions["clampedbleeding"] = new ItemsAfflictionInfos("clampedbleeding", 0, infos => {
            return true;
        });

        SutureAfflictions["surgeryincision"] = new ItemsAfflictionInfos("surgeryincision", 0, infos => {
            return true;
        });

        SutureAfflictions["retractedskin"] = new ItemsAfflictionInfos("retractedskin", 0, infos => {
            return true;
        });

        SutureAfflictions["caviclean"] = new ItemsAfflictionInfos("caviclean", 0, infos => {
            return true;
        });

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

            // A slight delay is needed for the Surgery afflictions to clear themselves.
            if (HF.HasAfflictionLimb(infos.target, "sawedbones", infos.targetLimb.type, 1))
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    HF.SurgicallyAmputateLimbAndGenerateItem(infos.user, infos.target, infos.targetLimb.type);
                }, 1);
            }

            HF.AddAffliction(infos.target, "tshocktimeout", -100, infos.user);

            // rewritten
            foreach (KeyValuePair<string, ItemsAfflictionInfos> Pair in SutureAfflictions)
            {
                ItemsAfflictionInfos affInfos = Pair.Value;

                // If the target doesn't have the affliction, we skip it
                if (!AfflictionPrefab.Prefabs.ContainsKey(affInfos.AfflictionID)) continue;
                AfflictionPrefab prefab = AfflictionPrefab.Prefabs[affInfos.AfflictionID];
                if (prefab == null)
                {
                    LuaCsLogger.LogError($"Error trying to heal {affInfos.AfflictionID} with sutures. The provided ID is probably incorrect.");
                    continue;
                }

                bool hasAffliction = prefab.LimbSpecific ? HF.HasAfflictionLimb(infos.target, affInfos.AfflictionID, infos.targetLimb.type, 1) : HF.HasAffliction(infos.target, affInfos.AfflictionID, 1);
                if (!hasAffliction) continue;

                // If the affliction's conditions are not met, we skip it
                if (!affInfos.Conditions(infos)) continue;

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
        DrainageAfflictions["pneumothorax"] = new ItemsAfflictionInfos("pneumothorax", 3, infos =>
        {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
        });

        DrainageAfflictions["tamponade"] = new ItemsAfflictionInfos("tamponade", 3, infos =>
        {
            bool retractedSkin = HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);

            if (NTConfig.Get<bool>("NT_OpenCloseTamponade", false)) return false;

            return retractedSkin;
        });

        // From 48 lines to 12 my point stands, why tf was the lua function so girthy?
        RegisterItemUseFunction("drainage", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            foreach (KeyValuePair<string, ItemsAfflictionInfos> Pair in DrainageAfflictions)
            {
                ItemsAfflictionInfos affInfos = Pair.Value;
                if (!affInfos.Conditions(infos)) continue;

                HF.SetAffliction(infos.target, affInfos.AfflictionID, 0, infos.user, 0);
                HF.GiveSurgerySkill(infos.user, affInfos.XPGain);
            }
        });

        // Needle
        RegisterItemUseFunction("needle", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.targetLimb.type == LimbType.Torso &&
                !HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type))
            {
                if (HF.GetSkillRequirementMet(infos.user, "medical", 20f))
                {
                    // If Pneumothorax OR Cardiac Tamponade is present, give skill.
                    if ((HF.HasAffliction(infos.target, "pneumothorax") ||
                         HF.HasAffliction(infos.target, "tamponade")) &&
                        !HF.HasAffliction(infos.target, "needlec", 0.1f))
                    {
                        HF.GiveSkillScaled(infos.user, "medical", 4000f);
                    }

                    HF.SetAffliction(infos.target, "needlec", 100f, infos.user, 0);

                    // If neither condition is present, cause a pneumothorax.
                    if (!HF.HasAffliction(infos.target, "pneumothorax") &&
                        !HF.HasAffliction(infos.target, "tamponade"))
                    {
                        HF.AddAffliction(infos.target, "pneumothorax", 1f, infos.user);
                    }
                    
                    // Originally, this had a check for NTSP NTCompat code; I'll do that later. - Lukako
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    HF.AddAffliction(infos.target, "organdamage", 10f, infos.user);
                    HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 10f,infos.user);
                }
            }
        });

        // Osteosynthesis Implants
        RegisterItemUseFunction("osteosynthesisimplants", infos =>
        {
            if (!HF.CanPerformSurgeryOn(infos.target) ||
                !HF.HasAfflictionLimb(infos.target, "drilledbones", infos.targetLimb.type, 99f))
            {
                return;
            }

            // Originally NTSP integrated for Surgery Skill, TODO.
            if (HF.GetSkillRequirementMet(infos.user, "Medical", 45f))
            {
                void removeAfflictionPlusGainSkill(string afflictionId, float skillGain)
                {
                    if (HF.HasAfflictionLimb(infos.target, afflictionId, infos.targetLimb.type))
                    {
                        HF.SetAfflictionLimb(infos.target, afflictionId, infos.targetLimb.type, 0f, infos.user, 0);
                        HF.GiveSkillScaled(infos.user, "medical", skillGain / 4f);
                    }
                }

                void removeAfflictionNonLimbSpecificPlusGainSkill(string afflictionId, float skillGain)
                {
                    if (HF.HasAffliction(infos.target, afflictionId))
                    {
                        HF.SetAffliction(infos.target, afflictionId, 0f, infos.user, 0);
                        HF.GiveSkillScaled(infos.user, "medical", skillGain / 4f);
                    }
                }

                var implantAfflictions = new Dictionary<string, float>
                {
                    ["ll_fracture"] = 10000f,
                    ["rl_fracture"] = 10000f,
                    ["la_fracture"] = 10000f,
                    ["ra_fracture"] = 10000f,
                    ["h_fracture"] = 10000f,
                    ["n_fracture"] = 10000f,
                    ["t_fracture"] = 10000f,
                    ["boneclamp"] = 0f,
                    ["drilledbones"] = 0f
                };

                foreach (var kvp in implantAfflictions)
                {
                    string identifier = kvp.Key;
                    float skillGain = kvp.Value;

                    var prefab = AfflictionPrefab.Prefabs[identifier];

                    if (prefab == null) continue;

                    if (prefab.LimbSpecific)
                    {
                        removeAfflictionPlusGainSkill(identifier, skillGain);
                    }
                    else if (prefab.IndicatorLimb == infos.targetLimb.type)
                    {
                        removeAfflictionNonLimbSpecificPlusGainSkill(identifier, skillGain);
                    }
                }

                HF.SetAfflictionLimb(infos.target, "bonegrowth", infos.targetLimb.type, 100f, infos.user, 0);

                float itemUses = (1f / NTConfig.Get("NT_OsteoImplants_uses", 4)) * 100f;

                infos.item.Condition -= itemUses;
                if (infos.item.Condition <= 1f)
                {
                    HF.RemoveItem(infos.item);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 5f, infos.user);
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 5f, infos.user);
            }

            // XML-derived sound
            if (HF.HasAfflictionLimb(infos.target, "drilledbones", infos.targetLimb.type, 99f) &&
                HF.HasAffliction(infos.target, "analgesia", 1f))
            {
                HF.GiveItem(infos.target, "ntsfx_drill");
            }
        });

        // Spinal Cord Implants
        RegisterItemUseFunction("spinalimplant", infos =>
        {
            if (!HF.CanPerformSurgeryOn(infos.target) ||
                !HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99f) ||
                !HF.HasAffliction(infos.target, "spinalcordinjury", 0.1f))
            {
                return;
            }

            // Originally NTSP integrated for Surgery Skill, TODO.
            if (HF.GetSkillRequirementMet(infos.user, "Medical", 45f))
            {
                HF.SetAffliction(infos.target, "spinalcordinjury", 0f, infos.user, 0);

                float itemUses = (1f / NTConfig.Get("NT_SpinalImplants_uses", 1)) * 100f;

                infos.item.Condition -= itemUses;

                if (infos.item.Condition <= 1f)
                {
                    HF.RemoveItem(infos.item);
                }

                HF.GiveSkillScaled(infos.user, "medical", 6000f);
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 5f, infos.user);
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 5f, infos.user);
            }

            // XML-derived sound
            if (HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99f))
            {
                HF.GiveItem(infos.target, "ntsfx_drill");
            }
        });

        // Scalpel
        RegisterItemUseFunction("advscalpel", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

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
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target)) return;

            if (!HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 99) || HF.HasAfflictionLimb(infos.target, "clampedbleeding", infos.targetLimb.type, 1)) return;

            HF.AddAfflictionLimb(infos.target, "clampedbleeding", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user) / 2, infos.user);
        });

        // Skin Retractors
        RegisterItemUseFunction("advretractors", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target)) return;

            if (!HF.HasAfflictionLimb(infos.target, "clampedbleeding", infos.targetLimb.type, 99) || HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1)) return;

            if (HF.GetSurgerySkillRequirementMet(infos.user, 30))
            {
                HF.AddAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user) / 2, infos.user);
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 10, infos.user);
            }
        });

        // Surgical Drill
        RegisterItemUseFunction("surgicaldrill", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target) ||
                !HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99f) ||
                HF.HasAfflictionLimb(infos.target, "drilledbones", infos.targetLimb.type, 1f))
            {
                return;
            }

            if (HF.GetSurgerySkillRequirementMet(infos.user, 45f))
            {
                HF.AddAfflictionLimb(infos.target, "drilledbones", infos.targetLimb.type, 1f + HF.GetSurgerySkill(infos.user) / 2f, infos.user);
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "burn", infos.targetLimb.type, 12f, infos.user);
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 10f, infos.user);
            }

            // XML-derived sound
            if (HF.HasAffliction(infos.target, "analgesia", 1f))
            {
                HF.GiveItem(infos.target, "ntsfx_drill");
            }
        });

        // Surgical Saw
        RegisterItemUseFunction("surgerysaw", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target) ||
                !HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99f) ||
                HF.HasAfflictionLimb(infos.target, "sawedbones", infos.targetLimb.type, 1f))
            {
                return;
            }

            if (HF.GetSurgerySkillRequirementMet(infos.user, 50f))
            {
                if (infos.targetLimb.type != LimbType.Torso)
                {
                    HF.AddAfflictionLimb(infos.target, "sawedbones", infos.targetLimb.type, 1f + HF.GetSurgerySkill(infos.user) / 2f, infos.user);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15f, infos.user);
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 6f, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 4f, infos.user);
            }

            // XML-derived sound
            if (HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99f) &&
                HF.HasAffliction(infos.target, "analgesia", 1f))
            {
                HF.GiveItem(infos.target, "ntsfx_breakbone");
            }
        });

        // Tweezers
        RegisterItemUseFunction("tweezers", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            string usecase = "";

            // Through surgical wound
            if (HF.CanPerformSurgeryOn(infos.target) &&
                HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 99f))
            {
                usecase = "surgery";
            }
            // Through normal wound
            else if (HF.HasAfflictionLimb(infos.target, "gunshotwound", infos.targetLimb.type, 1f) ||
                     HF.HasAfflictionLimb(infos.target, "explosiondamage", infos.targetLimb.type, 1f))
            {
                usecase = "ghetto";
            }

            if (usecase != "")
            {
                if (HF.GetSkillRequirementMet(infos.user, "medical", 30f))
                {
                    HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 5f, infos.user);

                    if (usecase == "ghetto")
                    {
                        HF.AddAffliction(infos.target, "traumaticshock", 5f, infos.user);
                    }

                    void HealAfflictionGiveSkill(string identifier, float healAmount, float skillGain)
                    {
                        float affAmount = HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, identifier);
                        float healedAmount = Math.Min(affAmount, healAmount);

                        HF.AddAfflictionLimb(infos.target, identifier, infos.targetLimb.type, -healAmount, infos.user);
                        HF.GiveSkillScaled(infos.user, "medical", healedAmount * skillGain / 2f);
                    }

                    float foreignBody = HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "foreignbody", 0f);
                    float scrapDropChance = Math.Min(foreignBody, 5f) / 5f * 0.05f;

                    if (HF.Chance(scrapDropChance))
                    {
                        HF.GiveItem(infos.user, "scrap");
                    }

                    float toHealAmount = Rand.Range(3f, 10f);

                    HealAfflictionGiveSkill("foreignbody", toHealAmount, 600f);

                    if (usecase == "surgery")
                    {
                        HealAfflictionGiveSkill("internaldamage", toHealAmount, 3f);
                        HealAfflictionGiveSkill("blunttrauma", toHealAmount, 3f);
                    }
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 6f, infos.user);
                }
            }
            else
            {
                bool sedated = HF.CanPerformSurgeryOn(infos.target);

                // pinchy pinchy!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 1f, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 0.5f, infos.user);

                if (!sedated)
                {
                    HF.AddAfflictionLimb(infos.target, "intensepain", infos.targetLimb.type, 5f, infos.user);
                    HF.AddAffliction(infos.target, "stun", 0.1f, infos.user);
                }

                // special head handling
                if (infos.targetLimb.type == LimbType.Head)
                {
                    HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 3f, infos.user);
                    HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 2f, infos.user);

                    if (!sedated)
                    {
                        HF.AddAfflictionLimb(infos.target, "intensepain", infos.targetLimb.type, 5f, infos.user);
                    }
                }
            }
        });

        // Liver Transplant Scalpel
        RegisterItemUseFunction("organscalpel_liver", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Torso) return;
            if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target, "liverremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target, "liverswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target, "liverdamage", 0) >= 100)
                        HF.SetAffliction(infos.target, "liverremoved", 100, infos.user, 0);
                    else
                        HF.SetAffliction(infos.target, "liverswap", 100, infos.user, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                    HF.AddAfflictionLimb(infos.target, "organdamage", infos.targetLimb.type, 5, infos.user);
                    HF.AddAffliction(infos.target, "liverdamage", 20, infos.user);
                }
                HF.GiveItem(infos.target, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target, "liverdamage", 0);
                if (damage >= 100) return;
                if (!HF.GetSurgerySkillRequirementMet(infos.user, 50)) return;

                HF.SetAffliction(infos.target, "liverremoved", 100, infos.user, 0);
                HF.SetAffliction(infos.target, "liverswap", 0, infos.user, 0);
                HF.SetAffliction(infos.target, "liverdamage", 100, infos.user, 0);
                HF.AddAffliction(infos.target, "organdamage", (100 - damage) / 5, infos.user);

                if (damage < 90)
                {
                    string transplantID = "livertransplant_q1";
                    // TODO: if (NTC.HasTag(infos.user, "organssellforfull")) transplantID = "livertransplant";
                    SpawnOrganTransplantInContainer(transplantID, infos.user, 100 - damage);
                }
            }
        });

        // Lung Transplant Scalpel
        RegisterItemUseFunction("organscalpel_lungs", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Torso) return;
            if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target, "lungremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target, "lungswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target, "lungdamage", 0) >= 100)
                        HF.SetAffliction(infos.target, "lungremoved", 100, infos.user, 0);
                    else
                        HF.SetAffliction(infos.target, "lungswap", 100, infos.user, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                    HF.AddAfflictionLimb(infos.target, "organdamage", infos.targetLimb.type, 5, infos.user);
                    HF.AddAffliction(infos.target, "lungdamage", 20, infos.user);
                }
                HF.GiveItem(infos.target, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target, "lungdamage", 0);
                if (damage >= 100) return;

                HF.SetAffliction(infos.target, "lungremoved", 100, infos.user, 0);
                HF.SetAffliction(infos.target, "lungswap", 0, infos.user, 0);
                HF.SetAffliction(infos.target, "lungdamage", 100, infos.target, 0);
                HF.SetAffliction(infos.target, "respiratoryarrest", 100, infos.target, 0);
                HF.SetAffliction(infos.target, "pneumothorax", 0, infos.target, 0);
                HF.SetAffliction(infos.target, "needlec", 0, infos.target, 0);
                HF.AddAffliction(infos.target, "organdamage", (100 - damage) / 5, infos.target);

                if (damage < 90)
                {
                    string transplantID = "lungtransplant_q1";
                    // TODO: if (NTC.HasTag(infos.user, "organssellforfull")) transplantID = "lungtransplant";
                    SpawnOrganTransplantInContainer(transplantID, infos.user, 100 - damage);
                }
            }
        });

        // Heart Transplant Scalpel
        RegisterItemUseFunction("organscalpel_heart", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Torso) return;
            if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target, "heartremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target, "heartswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target, "heartdamage", 0) >= 100)
                        HF.SetAffliction(infos.target, "heartremoved", 100, infos.user, 0);
                    else
                        HF.SetAffliction(infos.target, "heartswap", 100, infos.user, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                    HF.AddAfflictionLimb(infos.target, "organdamage", infos.targetLimb.type, 5, infos.user);
                    HF.AddAffliction(infos.target, "heartdamage", 20, infos.user);
                }
                HF.GiveItem(infos.target, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target, "heartdamage", 0);
                if (damage >= 100) return;

                HF.SetAffliction(infos.target, "heartremoved", 100, infos.user, 0);
                HF.SetAffliction(infos.target, "heartswap", 0, infos.user, 0);
                HF.SetAffliction(infos.target, "heartdamage", 100, infos.target, 0);
                HF.SetAffliction(infos.target, "cardiacarrest", 100, infos.target, 0);
                HF.SetAffliction(infos.target, "tamponade", 0, infos.target, 0);
                HF.SetAffliction(infos.target, "heartattack", 0, infos.target, 0);
                HF.AddAffliction(infos.target, "organdamage", (100 - damage) / 5, infos.target);

                if (damage < 90)
                {
                    string transplantID = "hearttransplant_q1";
                    // TODO: if (NTC.HasTag(infos.user, "organssellforfull")) transplantID = "hearttransplant";
                    SpawnOrganTransplantInContainer(transplantID, infos.user, 100 - damage);
                }
            }
        });

        // Kidney Transplant Scalpel
        RegisterItemUseFunction("organscalpel_kidneys", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Torso) return;
            if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target, "kidneyremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target, "kidneyswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target, "kidneydamage", 0) >= 100)
                        HF.SetAffliction(infos.target, "kidneyremoved", 100, infos.user, 0);
                    else
                        HF.SetAffliction(infos.target, "kidneyswap", 100, infos.user, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                    HF.AddAfflictionLimb(infos.target, "organdamage", infos.targetLimb.type, 5, infos.user);
                    HF.AddAffliction(infos.target, "kidneydamage", 10, infos.user);
                }
                HF.GiveItem(infos.target, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target, "kidneydamage", 0);
                if (damage >= 100) return;

                string transplantID = "kidneytransplant_q1";
                // TODO: if (NTC.HasTag(infos.user, "organssellforfull")) transplantID = "kidneytransplant";

                if (damage < 50)
                {
                    // First kidney
                    HF.SetAffliction(infos.target, "kidneydamage", 50, infos.user, 0);
                    HF.AddAffliction(infos.target, "organdamage", (100 - damage) / 5, infos.user);
                    SpawnOrganTransplantInContainer(transplantID, infos.user, 100);
                }
                else if (damage < 95)
                {
                    // Second kidney
                    HF.SetAffliction(infos.target, "kidneyremoved", 100, infos.user, 0);
                    HF.SetAffliction(infos.target, "kidneyswap", 0, infos.user, 0);
                    HF.SetAffliction(infos.target, "kidneydamage", 100, infos.user, 0);
                    HF.AddAffliction(infos.target, "organdamage", (100 - damage) / 5, infos.user);
                    SpawnOrganTransplantInContainer(transplantID, infos.user, 100 - (damage - 50) * 2);
                }
            }
        });

        // Brain Transplant Scalpel
        RegisterItemUseFunction("organscalpel_brain", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Head) return;
            if (!HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target, "brainremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target, "brainswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target, "cerebralhypoxia", 0) >= 100)
                        HF.SetAffliction(infos.target, "brainremoved", 100, infos.user, 0);
                    else
                        HF.SetAffliction(infos.target, "brainswap", 100, infos.user, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                    HF.AddAffliction(infos.target, "cerebralhypoxia", 50, infos.user);
                }
                HF.GiveItem(infos.target, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target, "cerebralhypoxia", 0);
                if (damage >= 100) return;

                HF.AddAffliction(infos.target, "cerebralhypoxia", 100, infos.user);
                HF.SetAffliction(infos.target, "brainremoved", 100, infos.user, 0);
                HF.SetAffliction(infos.target, "brainswap", 0, infos.user, 0);

                // TODO: NTSP artificialbrain check
                // if (HF.HasAffliction(infos.target, "artificialbrain"))
                // {
                //     HF.SetAffliction(infos.target, "artificialbrain", 0, infos.user, 0);
                //     damage = 100;
                // }

                if (damage < 90)
                {
                    float finalCondition = 100 - damage;
                    var client = HF.CharacterToClient(infos.target);
                    var capturedTarget = infos.target;
                    var capturedUser = infos.user;

                    var container = infos.user.Inventory.GetItemInLimbSlot(InvSlotType.RightHand);
                    if (container == null || container.OwnInventory == null || container.OwnInventory.IsFull())
                        container = infos.user.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand);
                    var capturedContainer = container;

                    if (capturedContainer != null && capturedContainer.OwnInventory != null && !capturedContainer.OwnInventory.IsFull())
                    {
                        HF.SpawnItemPlusFunction("braintransplant", capturedContainer.OwnInventory, InvSlotType.Any, capturedUser.WorldPosition, (args) =>
                        {
                            Item item = (Item)args[0];
                            item.Condition = finalCondition;

#if SERVER
                            if (client != null) item.Description = client.Name;
                            if (client != null) client.SetClientCharacter(null);
#endif
                        });
                    }
                    else
                    {
                        HF.GiveItemPlusFunction("braintransplant", capturedUser, (args) =>
                        {
                            Item item = (Item)args[0];
                            item.Condition = finalCondition;

#if SERVER
                            if (client != null) item.Description = client.Name;
                            if (client != null) client.SetClientCharacter(null);
#endif
                        });
                    }
                }
            }
        });

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
        RegisterItemUseFunction("antisepticspray", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.item.Condition <= 0) return;

            infos.item.Condition = 0; // Start Cooldown

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                infos.item.Condition = 100; // Finish Cooldown
            }, 2000);

            var containedItem = infos.item.OwnInventory?.GetItemAt(0);
            bool hasSaline = containedItem != null && containedItem.Prefab.Identifier == "antibloodloss1";
            bool hasAntiseptic = containedItem != null && containedItem.Prefab.Identifier == "antiseptic";

            // Surgery use
            if (hasSaline && infos.targetLimb.type == LimbType.Torso && HF.HasAffliction(infos.target, "infectedcavity", 1f) && HF.HasAffliction(infos.target, "retractedskin", 1f))
            {
                float skill = HF.GetSurgerySkill(infos.user);
                float delay = 11000f - skill * 10f;

                HF.AddAfflictionLimb(infos.target, "caviclean", infos.targetLimb.type, Math.Max(100f - skill / 2f, 10f), infos.user);

                LuaCsSetup.Instance.Timer.Wait((object[] _) =>
                {
                    if (!HF.HasAffliction(infos.target, "infectedcavity", 1f))
                    {
                        HF.GiveSkillScaled(infos.user, "medical", 10000f);
                    }
                }, (int)delay);

                return;
            }
            
            // Antiseptic use
            if (hasAntiseptic)
            {
                HF.AddAffliction(infos.target, "infectedwound", -100f, infos.user);
                HF.AddAffliction(infos.target, "ointmented", 20f, infos.user);
            }
            
            HF.GiveItem(infos.target, "ntsfx_spray");
        });

        // ============== Toggleable ==============
        // Endovascular Balloon
        RegisterItemUseFunction("endovascballoon", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.targetLimb.type == LimbType.Torso && HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 1f) && HF.HasAffliction(infos.target, "aorticrupture", 1f))
            {
                // Main effect
                HF.AddAffliction(infos.target, "balloonedaorta", 100f, infos.user);
                HF.SetAffliction(infos.target, "internalbleeding", 0f, infos.user, 0);

                HF.GiveSkillScaled(infos.user, "medical", 5000f);

                HF.GiveItem(infos.target, "ntsfx_syringe");
                HF.RemoveItem(infos.item);
            }
        });

        // Medical Stent
        RegisterItemUseFunction("medstent", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target, "stasis", 0.1f)) return;

            if (infos.targetLimb.type == LimbType.Torso &&
                HF.HasAffliction(infos.target, "balloonedaorta", 1f))
            {
                // Remove vascular condition
                HF.SetAffliction(infos.target, "balloonedaorta", 0f, infos.user, 0);
                HF.SetAffliction(infos.target, "aorticrupture", 0f, infos.user, 0);

                HF.GiveSkillScaled(infos.user, "medical", 10000f);
            }

            HF.GiveItem(infos.target, "ntsfx_syringe");
            HF.RemoveItem(infos.item);
        });

        // Sodium Nitroprusside
        RegisterItemUseFunction("pressuremeds", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user, "medical", 10f);

            int totalAmount = success ? 50 : 30;
            int duration = 10;

            HF.ApplyAfflictionOverTime(infos.target, "afpressuredrug", totalAmount, duration, infos.user);

            HF.GiveItem(infos.target, "ntsfx_pills");
            HF.RemoveItem(infos.item);
        });
    }

    /// <summary>
    /// Register a new identifier => function to the NTItemRegistry; this function will then trigger when the item matching the identifier is used.
    /// </summary>
    /// <param name="itemID">The Identifier for the item, defined within XML.</param>
    /// <param name="function">The function that should run after item usage.</param>
    /// <returns>'True' if the item was defined correctly ánd not already defined; otherwise 'False'.</returns>
    public static bool RegisterItemUseFunction(string itemID, Action<ItemUpdateFunctionInfos> function)
    {
        if (!NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry.Add(itemID, function);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Overrides an already defined function matching the itemID. Does nothing if the itemID isn't defined in the registry.
    /// </summary>
    /// <param name="itemID">The Identifier for the item, defined within XML.</param>
    /// <param name="function">The function that should run after item usage.</param>
    /// <returns></returns>
    public static bool UpdateItemUseFunction(string itemID, Action<ItemUpdateFunctionInfos> function)
    {
        if (NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry.Add(itemID, function);
            return true;
        }

        return false;
    }

    /// <summary>
    /// The function patching the base game Item.ApplyTreatment
    /// </summary>
    public static void Override_ApplyTreatment(Barotrauma.Item __instance, Character user, Character character, Limb targetLimb)
    {

        string itemID = __instance.Prefab.Identifier.ToString();
        if (NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry[itemID].Invoke(new ItemUpdateFunctionInfos(__instance, user, character, targetLimb));
        }
    }

    /// <summary>
    /// The function patching the base game Item.Use
    /// </summary>
    public static void Override_Use(Barotrauma.Item __instance, float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
    {
       // LuaCsLogger.Log("use");
    }
}


