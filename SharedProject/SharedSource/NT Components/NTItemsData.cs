using static Neurotrauma.NTItems;

namespace Neurotrauma;

public class NTItemsData
{




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
    public static void WrenchFunctionality(NTItems.ItemUpdateFunctionInfos infos)
    {
        if (HF.LimbIsDislocated(infos.target.Human, infos.targetLimb.type, false))
        {
            float skillrequired = 60;

            if (HF.HasAffliction(infos.target.Human, "analgesia", 0.5f) ||
                HF.HasAffliction(infos.target.Human, "afadrenaline", 0.5f))
            {
                skillrequired -= 30;
            }

            if (HF.GetSkillRequirementMet(infos.user.Human, "medical", skillrequired))
            {
                HF.DislocateLimb(infos.target.Human, infos.targetLimb.type, -1000);
                HF.GiveSkillScaled(infos.user.Human, "medical", 4000);
            }
            else
            {
                HF.BreakLimb(infos.target.Human, infos.targetLimb.type, 1);
            }

            if (!HF.HasAffliction(infos.target.Human, "analgesia", 0.5f))
            {
                HF.AddAffliction(infos.target.Human, "severepain", 5, infos.user.Human);
            }

            HF.GiveItem(infos.target.Human, "ntsfx_smack");
        }
        else if (!HF.HasAffliction(infos.target.Human, "unconsciousness", 0.1f))
        {
            var outerWearId = HF.GetOuterWearIdentifier(infos.target.Human);

            if (outerWearId == "stasisbag" || outerWearId == "bodybag" || outerWearId == "autocpr")
            {
                var equippedOuterItem = HF.GetItemInOuterWear(infos.target.Human);

                if (infos.user.Human.Inventory.TryPutItem(equippedOuterItem, null, new List<InvSlotType> { InvSlotType.Any }))
                {
                    HF.GiveItem(infos.target.Human, "ntsfx_velcro");
                }
            }
        }
    }

    /// <summary>
    /// Used by BloodPacks to decrease Blood Loss, store Alkalosis/Acidosis/Sepsis and handle Hemotransfusion Shock.
    /// </summary>
    /// <param name="infos">Contextual data used in item functionality.</param>
    public static void InfuseBloodpack(NTItems.ItemUpdateFunctionInfos infos)
    {
        string id = infos.item.Prefab.Identifier.Value;

        bool packhasantibodyA = id is "bloodpacka_positive" or "bloodpacka_negative" or "bloodpackab_positive" or "bloodpackab_negative";
        bool packhasantibodyB = id is "bloodpackb_positive" or "bloodpackb_negative" or "bloodpackab_positive" or "bloodpackab_negative";
        bool packhasantibodyC = false;
        bool packhasantibodyRh = id is "bloodpacko_positive" or "bloodpacka_positive" or "bloodpackb_positive" or "bloodpackab_positive";

        string targettype = NTBloodTypes.GetBloodType(infos.target.Human);
        bool targethasantibodyA = targettype.Contains("a");
        bool targethasantibodyB = targettype.Contains("b");
        bool targethasantibodyC = targettype.Contains("c");
        bool targethasantibodyRh = targettype.Contains("positive");

        bool compatible = (targethasantibodyRh || !packhasantibodyRh)
                       && (targethasantibodyA || !packhasantibodyA)
                       && (targethasantibodyB || !packhasantibodyB)
                       && (targethasantibodyC || !packhasantibodyC);

        // TODO: give always true to team of bots on enemy submarines for future medic AI logic

        float bloodloss = HF.GetAfflictionStrength(infos.target.Human, "bloodloss", 0);
        float usefulFraction = Math.Clamp(bloodloss / 30f, 0f, 1f);

        if (compatible)
        {
            HF.AddAffliction(infos.target.Human, "bloodloss", -30, infos.user.Human);
            HF.AddAffliction(infos.target.Human, "bloodpressure", 30, infos.user.Human);
            HF.GiveSkillScaled(infos.user.Human, "medical", 4000 * HF.BoolToNum(bloodloss > 100));
        }
        else
        {
            HF.AddAffliction(infos.target.Human, "bloodloss", -20, infos.user.Human);
            HF.AddAffliction(infos.target.Human, "bloodpressure", 30, infos.user.Human);
            HF.GiveSkillScaled(infos.user.Human, "medical", 4000 * HF.BoolToNum(bloodloss > 100));

            float immunity = HF.GetAfflictionStrength(infos.target.Human, "immunity", 100);
            HF.AddAffliction(infos.target.Human, "hemotransfusionshock", Math.Max(immunity - 6f, 0f), infos.user.Human);
        }

        // Move towards isotonic
        HF.SetAffliction(infos.target.Human, "acidosis", HF.GetAfflictionStrength(infos.target.Human, "acidosis", 0) * Single.Lerp(1f, 0.9f, usefulFraction));
        HF.SetAffliction(infos.target.Human, "alkalosis", HF.GetAfflictionStrength(infos.target.Human, "alkalosis", 0) * Single.Lerp(1f, 0.9f, usefulFraction));

        // Check item tags for acidosis, alkalosis, sepsis
        string[] tags = infos.item.Tags.Split(',');

        foreach (string tag in tags)
        {
            string t = tag.Trim();

            if (t == "sepsis")
            {
                HF.AddAffliction(infos.target.Human, "sepsis", 1f, infos.user.Human);
            }
            else if (t.StartsWith("acid"))
            {
                string[] split = t.Split(':');
                if (split.Length > 1 && float.TryParse(split[1], out float acidVal)) HF.AddAffliction(infos.target.Human, "acidosis", acidVal / 10f * usefulFraction, infos.user.Human);
            }
            else if (t.StartsWith("alkal"))
            {
                string[] split = t.Split(':');
                if (split.Length > 1 && float.TryParse(split[1], out float alkalVal)) HF.AddAffliction(infos.target.Human, "alkalosis", alkalVal / 10f * usefulFraction, infos.user.Human);
            }
        }

        HF.RemoveItem(infos.item);
        HF.GiveItem(infos.user.Human, "emptybloodpack");
        HF.GiveItem(infos.target.Human, "ntsfx_syringe");
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
    static void ReattachLimb(NTItems.ItemUpdateFunctionInfos infos)
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

        if (HF.HasAfflictionLimb(infos.target.Human, "sawedbones", limbType, 99))
        {
            HF.SetAfflictionLimb(infos.target.Human, "sawedbones", limbType, 0f, infos.user.Human, 99);
            HF.SurgicallyAmputateLimb(infos.target.Human, limbType, 0, 0);
            HF.RemoveItem(infos.item);
        }
    }

    /// <summary>
    /// Used to define functionality for given Items based on their ID. Replaces Lua ItemMethods.
    /// </summary>
    public static void DefineAllItems()
    {
        NTItemFunctionLoader loader = NeurotraumaInit.NTItemsLoader;


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
        loader.Register("bloodanalyzer", infos =>
        {
            // Only work if not on cooldown
            if (infos.item.Condition < 50) return;
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "medical", 30);
            float BloodLossInduced = 3f;
            if (success) BloodLossInduced = 1f;

            HF.AddAffliction(infos.target.Human, "bloodloss", BloodLossInduced, infos.user.Human);

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

                string BloodType = NTBloodTypes.GetBloodType(infos.target.Human);

                var TargetIDCard = infos.target.Human.Inventory.GetItemAt(0);

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
            float lowMedThreshold = NTConfig.Get("NTSCAN_lowmedThreshold", 1);
            float medHighThreshold = NTConfig.Get("NT_medhighThreshold", 1);

            // Strings
            List<string> vitalCategory = NTConfig.Get("NTSCAN_VitalCategory", []);
            List<string> removalCategory = NTConfig.Get("NTSCAN_RemovalCategory", []);
            List<string> customCategory = NTConfig.Get("NTSCAN_CustomCategory", []);
            List<string> ignoredCategory = NTConfig.Get("NTSCAN_IgnoredCategory", []);

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

            string BloodTypeName = AfflictionPrefab.Prefabs[NTBloodTypes.GetBloodType(infos.target.Human)].Name.Value;

            string startReadout =
                $"‖color:{nameColor.R},{nameColor.G},{nameColor.B}‖" +
                $"Bloodtype: {BloodTypeName}" +
                "‖color:end‖\n" +
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                $"Affliction readout for {infos.target.Human.Name}:" +
                "‖color:end‖\n";

            int afflictionsDisplayed = 0;

            HashSet<string> checkedAfflictions = [];

            // I personally like using brackets in single-line if statements but this is way more compact - Lukako
            foreach (var value in infos.target.Human.CharacterHealth.GetAllAfflictions())
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
                HF.CharacterToClient(infos.user.Human),
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
            loader.Register(id, InfuseBloodpack);
        }

        // Empty Blood Pack
        loader.Register("emptybloodpack", infos =>
        {
            if (!infos.target.Human.IsHuman) return;
            if (infos.item.condition <= 0) return;

            // changing from 31 to somthing like 15 can stop easy station kill by using two blood pack in a row
            // Minor Spelling Mistake :skull: - Lukako
            float BloodLossStrength = HF.GetAfflictionStrength(infos.target.Human, "bloodloss", 0);
            if (BloodLossStrength >= 31f) return;

            bool success = HF.GetSkillRequirementMet(infos.user.Human, "medical", 30);
            int bloodlossinduced = success ? 30 : 40;

            string bloodtype = NTBloodTypes.GetBloodType(infos.target.Human);

            // We store the data we need for the item tags
            double acidosis = HF.GetAfflictionStrength(infos.target.Human, "acidosis", 0);
            double alkalosis = HF.GetAfflictionStrength(infos.target.Human, "alkalosis", 0);
            double sepsis = HF.GetAfflictionStrength(infos.target.Human, "sepsis", 0);

            HF.SetAffliction(infos.target.Human, "acidosis", (float)HF.GetAfflictionStrength(infos.target.Human, "acidosis", 0) * (float)0.9, infos.user.Human, 0);
            HF.SetAffliction(infos.target.Human, "alkalosis", (float)HF.GetAfflictionStrength(infos.target.Human, "alkalosis", 0) * (float)0.9, infos.user.Human, 0);
            HF.AddAffliction(infos.target.Human, "bloodloss", bloodlossinduced, infos.user.Human);

            string btID = bloodtype == "o_negative" ? "antibloodloss2" : "bloodpack" + bloodtype;

            HF.GiveItemPlusFunction(btID, infos.user.Human, (args) => {

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
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
        });

        // ============== BodyParts ==============

        // ForEach are used to act like the 'startswith' method, to prevent us having to do this twice for each organ. - Lukako
        // Liver Transplant
        foreach (string id in new[] { "livertransplant", "livertransplant_q1" })
        {
            loader.Register(id, infos =>
            {
                if (infos.targetLimb.type != LimbType.Torso) return;

                if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99)) return;
                if (!HF.HasAffliction(infos.target.Human, "liverremoved", 1) && !HF.HasAffliction(infos.target.Human, "liverswap", 1)) return;

                float modifier = HF.GetSurgerySkillRequirementMet(infos.user.Human, 40) ? 0f : -40f;
                float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);
                float damage = HF.GetAfflictionStrength(infos.target.Human, "liverdamage", 0);

                HF.AddAffliction(infos.target.Human, "liverdamage", -workcondition, infos.user.Human);

                if (damage == 100f)
                {
                    HF.AddAffliction(infos.target.Human, "liverdamage", -workcondition, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "organdamage", -workcondition / 5f, infos.user.Human);
                    HF.SetAffliction(infos.target.Human, "liverremoved", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "liverswap", 0f, infos.user.Human, 0);
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    float newdamage = Math.Clamp((100f - damage) - workcondition, -100f, 100f);
                    HF.SetAffliction(infos.target.Human, "liverdamage", 100f - workcondition, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "liverremoved", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "liverswap", 0f, infos.user.Human, 0);
                    HF.AddAffliction(infos.target.Human, "organdamage", newdamage / 5f, infos.user.Human);


                    string transplantID = infos.user.Human.HasTalent("ntsp_unspecialisedspecialist") ? "livertransplant" : "livertransplant_q1";

                    if (damage < 90f)
                    {
                        HF.SpawnItemPlusFunction(transplantID, infos.item.ParentInventory, InvSlotType.Any, infos.user.Human.WorldPosition, (args) => { ((Item)args[0]).Condition = 100f - damage; });
                        HF.RemoveItem(infos.item);
                    }
                }

                float rejectionchance = (float)Math.Clamp((HF.GetAfflictionStrength(infos.target.Human, "immunity", 0) - 10f) / 150f * NTC.GetMultiplier(infos.user.Human, "organrejectionchance"), 0f, 1f);

                if (HF.Chance(rejectionchance) && NTConfig.Get("NT_organRejection", false)) HF.SetAffliction(infos.target.Human, "liverdamage", 100f, infos.user.Human, 0);
            });
        }

        // Heart Transplant
        foreach (string id in new[] { "hearttransplant", "hearttransplant_q1" })
        {
            loader.Register(id, infos =>
            {
                if (infos.targetLimb.type != LimbType.Torso) return;

                if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99)) return;
                if (!HF.HasAffliction(infos.target.Human, "heartremoved", 1) && !HF.HasAffliction(infos.target.Human, "heartswap", 1)) return;

                float modifier = HF.GetSurgerySkillRequirementMet(infos.user.Human, 40) ? 0f : -40f;
                float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);
                float damage = HF.GetAfflictionStrength(infos.target.Human, "heartdamage", 0);

                if (damage == 100f)
                {
                    HF.AddAffliction(infos.target.Human, "heartdamage", -workcondition, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "organdamage", -workcondition / 5f, infos.user.Human);
                    HF.SetAffliction(infos.target.Human, "heartremoved", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "heartswap", 0f, infos.user.Human, 0);
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    float newdamage = Math.Clamp((100f - damage) - workcondition, -100f, 100f);
                    HF.SetAffliction(infos.target.Human, "heartdamage", 100f - workcondition, infos.target.Human, 0);
                    HF.SetAffliction(infos.target.Human, "heartremoved", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "heartswap", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "cardiacarrest", 100f, infos.target.Human, 0);
                    HF.SetAffliction(infos.target.Human, "tamponade", 0f, infos.target.Human, 0);
                    HF.SetAffliction(infos.target.Human, "heartattack", 0f, infos.target.Human, 0);
                    HF.AddAffliction(infos.target.Human, "organdamage", newdamage / 5f, infos.target.Human);

                    string transplantID = infos.user.Human.HasTalent("ntsp_unspecialisedspecialist") ? "hearttransplant" : "hearttransplant_q1";

                    if (damage < 90f)
                    {
                        HF.SpawnItemPlusFunction(transplantID, infos.item.ParentInventory, InvSlotType.Any, infos.user.Human.WorldPosition, (args) => { ((Item)args[0]).Condition = 100f - damage; });
                        HF.RemoveItem(infos.item);
                    }
                }

                float rejectionchance = (float)Math.Clamp((HF.GetAfflictionStrength(infos.target.Human, "immunity", 0) - 10f) / 150f * NTC.GetMultiplier(infos.user.Human, "organrejectionchance"), 0f, 1f);

                if (HF.Chance(rejectionchance) && NTConfig.Get("NT_organRejection", false)) HF.SetAffliction(infos.target.Human, "heartdamage", 100f, infos.user.Human, 0);
            });
        }

        // Lung Transplant
        foreach (string id in new[] { "lungtransplant", "lungtransplant_q1" })
        {
            loader.Register(id, infos =>
            {
                if (infos.targetLimb.type != LimbType.Torso) return;

                if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99)) return;
                if (!HF.HasAffliction(infos.target.Human, "lungremoved", 1) && !HF.HasAffliction(infos.target.Human, "lungswap", 1)) return;

                float modifier = HF.GetSurgerySkillRequirementMet(infos.user.Human, 40) ? 0f : -40f;
                float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);
                float damage = HF.GetAfflictionStrength(infos.target.Human, "lungdamage", 0);

                if (damage == 100f)
                {
                    HF.AddAffliction(infos.target.Human, "lungdamage", -workcondition, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "organdamage", -workcondition / 5f, infos.user.Human);
                    HF.SetAffliction(infos.target.Human, "lungremoved", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "lungswap", 0f, infos.user.Human, 0);
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    float newdamage = Math.Clamp((100f - damage) - workcondition, -100f, 100f);
                    HF.SetAffliction(infos.target.Human, "lungdamage", 100f - workcondition, infos.target.Human, 0);
                    HF.SetAffliction(infos.target.Human, "lungremoved", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "lungswap", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "respiratoryarrest", 100f, infos.target.Human, 0);
                    HF.SetAffliction(infos.target.Human, "pneumothorax", 0f, infos.target.Human, 0);
                    HF.SetAffliction(infos.target.Human, "needlec", 0f, infos.target.Human, 0);
                    HF.AddAffliction(infos.target.Human, "organdamage", newdamage / 5f, infos.target.Human);

                    //TODO: check if it works
                    

                    string transplantID = infos.user.Human.HasTalent("ntsp_unspecialisedspecialist")
                        ? "lungtransplant" : "lungtransplant_q1";

                    if (damage < 90f)
                    {
                        HF.SpawnItemPlusFunction(transplantID, infos.item.ParentInventory, InvSlotType.Any, infos.user.Human.WorldPosition, (args) => { ((Item)args[0]).Condition = 100f - damage; });
                        HF.RemoveItem(infos.item);
                    }
                }

                float rejectionchance = (float)Math.Clamp((HF.GetAfflictionStrength(infos.target.Human, "immunity", 0) - 10f) / 150f * NTC.GetMultiplier(infos.user.Human, "organrejectionchance"), 0f, 1f);

                if (HF.Chance(rejectionchance) && NTConfig.Get("NT_organRejection", false)) HF.SetAffliction(infos.target.Human, "lungdamage", 100f, infos.user.Human, 0);
            });
        }

        // Kidney Transplant
        foreach (string id in new[] { "kidneytransplant", "kidneytransplant_q1" })
        {
            loader.Register(id, infos =>
            {
                if (infos.targetLimb.type != LimbType.Torso) return;

                if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99)) return;
                if (!HF.HasAffliction(infos.target.Human, "kidneyremoved", 1) && !HF.HasAffliction(infos.target.Human, "kidneyswap", 1)) return;

                float modifier = HF.GetSurgerySkillRequirementMet(infos.user.Human, 40) ? 0f : -40f;
                float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);
                float damage = HF.GetAfflictionStrength(infos.target.Human, "kidneydamage", 0);

                float rejectionchance = (float)Math.Clamp((HF.GetAfflictionStrength(infos.target.Human, "immunity", 0) - 10f) / 150f * NTC.GetMultiplier(infos.user.Human, "organrejectionchance"), 0f, 1f);

                if (HF.Chance(rejectionchance) && NTConfig.Get("NT_organRejection", false))
                {
                    HF.RemoveItem(infos.item);
                    return;
                }

                if (damage > 50f)
                {
                    LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                    {
                        HF.SetAffliction(infos.target.Human, "kidneyremoved", 0f, infos.user.Human, 0);
                        HF.SetAffliction(infos.target.Human, "kidneyswap", 0f, infos.user.Human, 0);
                    }, 3000);

                    HF.AddAffliction(infos.target.Human, "kidneydamage", -workcondition / 2f, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "organdamage", -workcondition / 5f, infos.user.Human);
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    float newdamage = Math.Clamp(((100f - damage) - workcondition) / 2f, -100f, 100f);
                    HF.SetAffliction(infos.target.Human, "kidneyremoved", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "kidneyswap", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "kidneydamage", 50f - workcondition / 2f, infos.user.Human, 0);
                    HF.AddAffliction(infos.target.Human, "organdamage", newdamage / 5f, infos.user.Human);

                    string transplantID = infos.user.Human.HasTalent("ntsp_unspecialisedspecialist") ? "kidneytransplant" : "kidneytransplant_q1";

                    HF.RemoveItem(infos.item);

                    if (damage < 45f)
                    {
                        HF.SpawnItemPlusFunction(transplantID, infos.item.ParentInventory, InvSlotType.Any, infos.user.Human.WorldPosition, (args) => { ((Item)args[0]).Condition = 100f - damage * 2f; });
                    }
                }
            });
        }
        
        // Brain Transplant
        loader.Register("braintransplant", infos =>
        {
            if (infos.targetLimb.type != LimbType.Head) return;
            if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type)) return;
            if (!HF.HasAffliction(infos.target.Human, "brainremoved", 1)) return;

            float modifier = HF.GetSurgerySkillRequirementMet(infos.user.Human, 100) ? 0f : -40f;
            float workcondition = Math.Clamp(infos.item.Condition + modifier, 0f, 100f);

            HF.AddAffliction(infos.target.Human, "neurotrauma", -workcondition, infos.user.Human);
            HF.SetAffliction(infos.target.Human, "brainremoved", 0f, infos.user.Human, 0);

#if SERVER
            string donorName = infos.item.Description;
            var client = HF.ClientFromName(donorName);
            if (client != null) client.SetClientCharacter(infos.target.Human);
#endif

            HF.RemoveItem(infos.item);
        });

        // Extremity Transplants
        loader.Register("rarm", ReattachLimb);
        loader.Register("larm", ReattachLimb);
        loader.Register("rleg", ReattachLimb);
        loader.Register("lleg", ReattachLimb);

        // Bionic Transplants
        loader.Register("rarmp", ReattachLimb);
        loader.Register("larmp", ReattachLimb);
        loader.Register("rlegp", ReattachLimb);
        loader.Register("llegp", ReattachLimb);

        // ============== Consumables ==============
        // Antibiotic Ointment
        loader.Register("ointment", (infos) =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "medical", 10);

            HF.AddAfflictionLimb(infos.target.Human, "ointmented", infos.targetLimb.type, success ? 120 : 60, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "infectedwound", infos.targetLimb.type, success ? -72 : -24, infos.user.Human);

            // Check for third degree burn might not be working correctly
            if (HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "burn", 0) < 50)
            {
                HF.AddAfflictionLimb(infos.target.Human, "burn", infos.targetLimb.type, success ? -12 : (float)-7.2, infos.user.Human);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_ointment");
        });

        // Azathioprine
        loader.Register("immunosuppressant", infos =>
        {
            // XML applied 5 or 3 strength for 10 seconds; use a new HF to do the same. - Lukako
            // TODO: Emulate MultiplyByMaxVitality
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "Medical", 10);

            int totalAmount = success ? 50 : 30;
            int duration = 10;

            HF.ApplyAfflictionOverTime(infos.target.Human, "afimmunosuppressant", totalAmount, duration, infos.user.Human);

            // Technically, this is different from the XML original; this applies 1 Sepsis instantly, the other had a 50% chance to apply per tick.
            if (!success && HF.Chance(0.5f))
            {
                HF.AddAffliction(infos.target.Human, "sepsis", 1f);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_pills");
        });

        // Tourniquet
        loader.Register("tourniquet", infos =>
        {
            if (HF.HasAfflictionLimb(infos.target.Human, "tourniqueted", infos.targetLimb.type, 1)) return;

            // Failure
            if (!HF.GetSkillRequirementMet(infos.user.Human, "medical", 30))
            {
                HF.AddAfflictionLimb(infos.target.Human, "blunttrauma", infos.targetLimb.type, 6, infos.user.Human);
                return;
            }

            if (HF.LimbIsExtremity(infos.targetLimb.type))
            {
                HF.SetAfflictionLimb(infos.target.Human, "tourniqueted", infos.targetLimb.type, 100, infos.user.Human, 0);
            }
            else if (infos.targetLimb.type == LimbType.Head)
            {
                HF.SetAffliction(infos.target.Human, "oxygenlow", 200, infos.user.Human, 0);
                HF.AddAffliction(infos.target.Human, "neurotrauma", 15, infos.user.Human);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_bandage");
        });

        // Gel Ice Pack
        loader.Register("gelipack", infos =>
        {
            if (infos.item.Condition < 25) return;

            float success = HF.BoolToNum(HF.GetSkillRequirementMet(infos.user.Human, "medical", 40));
            HF.AddAfflictionLimb(infos.target.Human, "iced", infos.targetLimb.type, 75 + success * 25, infos.user.Human);

            infos.item.Condition = infos.item.Condition - 35;
            HF.GiveItem(infos.target.Human, "ntsfx_bandage");
        });

        // Gypsum
        loader.Register("gypsum", infos =>
        {
            if (HF.HasAffliction(infos.target.Human, "stasis", (float)0.1))
            {
                return;
            }

            // Needs to be bandaged, not already in a cast, not during a surgery, and the limb needs to be extremity.
            if (!HF.HasAfflictionLimb(infos.target.Human, "bandaged", infos.targetLimb.type, (float)0.1)
            || HF.HasAfflictionLimb(infos.target.Human, "plastercast", infos.targetLimb.type, (float)0.1)
            || HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, (float)0.1)
            || !HF.LimbIsExtremity(infos.targetLimb.type))
            {
                return;
            }

            if (HF.GetSkillRequirementMet(infos.user.Human, "medical", (float)40))
            {
                HF.SetAfflictionLimb(infos.target.Human, "bandaged", infos.targetLimb.type, 0, infos.user.Human, 0);
                HF.SetAfflictionLimb(infos.target.Human, "plastercast", infos.targetLimb.type, 100, infos.user.Human, 0);
                HF.BreakLimb(infos.target.Human, infos.targetLimb.type, -20);
                HF.GiveSkillScaled(infos.user.Human, "medical", 6000);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_bandage");
        });

        // Ringer's Solution
        loader.Register("ringerssolution", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "Medical", 20);

            int totalAmount = success ? 50 : 30;
            int duration = 10;

            HF.ApplyAfflictionOverTime(infos.target.Human, "afringerssolution", totalAmount, duration, infos.user.Human);

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
        });

        // Mannitol
        loader.Register("mannitol", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "Medical", 60);

            int duration = 10;

            if (success)
            {
                HF.ApplyAfflictionOverTime(infos.target.Human, "afmannitol", 50, duration, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "organdamage", 5, duration, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "heartdamage", 10, duration, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "kidneydamage", 10, duration, infos.user.Human);
            }
            else
            {
                HF.ApplyAfflictionOverTime(infos.target.Human, "afmannitol", 30, duration, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "organdamage", 10, duration, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "heartdamage", 20, duration, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "kidneydamage", 20, duration, infos.user.Human);
            }

            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
            HF.RemoveItem(infos.item);
        });

        // Thiamine
        loader.Register("thiamine", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "Medical", 10);

            int duration = 10;
            int totalAmount = success ? 50 : 30;

            HF.ApplyAfflictionOverTime(infos.target.Human, "afthiamine", totalAmount, duration, infos.user.Human);

            HF.GiveItem(infos.target.Human, "ntsfx_pills");
            HF.RemoveItem(infos.item);
        });

        // Streptokinase
        loader.Register("streptokinase", infos =>
        {
            HF.AddAffliction(infos.target.Human, "heartattack", -100, infos.user.Human);
            HF.AddAffliction(infos.target.Human, "hemotransfusionshock", -100, infos.user.Human);
            HF.AddAffliction(infos.target.Human, "afstreptokinase", 50, infos.user.Human);

            if (HF.HasAffliction(infos.target.Human, "stroke"))
            {
                HF.AddAffliction(infos.target.Human, "stroke", 5, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "neurotrauma", 10, infos.user.Human);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
        });

        // Propofol
        loader.Register("propofol", infos =>
        {
            float anesthesiaStrength = HF.GetAfflictionStrength(infos.target.Human, "anesthesia", 0);
            float anesthesiaGained = 1;

            if (HF.HasTalent(infos.user.Human, "ntsp_propofol")) anesthesiaGained = 15;

            if (anesthesiaStrength < 15)
            {
                HF.AddAffliction(infos.target.Human, "anesthesia", anesthesiaGained, infos.user.Human);
            }
            else
            {
                anesthesiaGained = 15 - anesthesiaStrength;
                HF.AddAffliction(infos.target.Human, "anesthesia", anesthesiaGained, infos.user.Human);
            }

            HF.AddAffliction(infos.target.Human, "afanaesthetic", 100, infos.user.Human);
            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");

        });

        // ============== OtherEquipment ==============
        // Manual Defibrillator
        loader.Register("defibrillator", infos =>
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

            HF.GiveItem(infos.target.Human, "ntsfx_manualdefib");

            if (battery.Prefab.Identifier.Value != "fulguriumbatterycell")
            {
                battery.Condition -= 20;
            }
            else
            {
                battery.Condition -= 10;
            }

            float medicalSkill = HF.GetSkillLevel(infos.user.Human, "medical");

            float successChance = MathF.Pow(medicalSkill / 100f, 2);
            float arrestSuccessChance = MathF.Pow(medicalSkill / 100f, 4);
            float arrestFailChance = MathF.Pow(1f - (medicalSkill / 100f), 2) * 0.3f;

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                HF.AddAffliction(infos.target.Human, "stun", 2f, infos.user.Human);

                if (HF.Chance(successChance))
                {
                    HF.SetAffliction(infos.target.Human, "increasedheartrate", 0f, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "fibrillation", 0f, infos.user.Human, 0);
                }

                if (HF.Chance(arrestSuccessChance))
                {
                    HF.SetAffliction(infos.target.Human, "cardiacarrest", 0f, infos.user.Human, 0);
                }
            }, 2000);
        });

        // AED
        loader.Register("aed", infos =>
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
                HF.HasAffliction(infos.target.Human, "increasedheartrate", 5) ||
                HF.HasAffliction(infos.target.Human, "fibrillation", 1) ||
                HF.HasAffliction(infos.target.Human, "cardiacarrest");

            if (!actionRequired)
            {
                HF.GiveItem(infos.target.Human, "ntsfx_defib2");
                return;
            }

            HF.GiveItem(infos.target.Human, "ntsfx_defib1");

            if (battery.Prefab.Identifier.Value != "fulguriumbatterycell")
            {
                battery.Condition -= 20;
            }
            else
            {
                battery.Condition -= 10;
            }

            float medicalSkill = HF.GetSkillLevel(infos.user.Human, "medical");

            float arrestSuccessChance = Math.Clamp(medicalSkill / 200f, 0.2f, 0.4f);

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                HF.AddAffliction(infos.target.Human, "stun", 2f, infos.user.Human);
                HF.SetAffliction(infos.target.Human, "increasedheartrate", 0f, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "fibrillation", 0f, infos.user.Human, 0);

                if (HF.Chance(arrestSuccessChance))
                {
                    HF.SetAffliction(infos.target.Human, "cardiacarrest", 0f, infos.user.Human, 0);
                }

            }, 3200);
        });

        // AutoPulse
        loader.Register("autocpr", infos =>
        {
            if (infos.target.Human.InWater) return;

            var targetInventory = infos.target.Human.Inventory;
            if (targetInventory == null) return;

            // Try to put the autopulse directly into the target's outerwear slot
            if (targetInventory.TryPutItem(infos.item, 4, true, true, infos.user.Human, true, true))
            {
                HF.GiveItem(infos.target.Human, "ntsfx_zipper");
            }
            else
            {
                var UserInventory = infos.user.Human.Inventory;
                var TargetOuterWear = HF.GetItemInOuterWear(infos.target.Human);
                var LeftHand = HF.GetItemInLeftHand(infos.user.Human);
                var RightHand = HF.GetItemInRightHand(infos.user.Human);

                if (RightHand != null)
                {
                    if (!UserInventory.TryPutItem(RightHand, null, new List<InvSlotType> { InvSlotType.Any }))
                    {
                        RightHand.Drop(infos.user.Human, true);
                    }
                }

                if (LeftHand != null)
                {
                    if (!UserInventory.TryPutItem(LeftHand, null, new List<InvSlotType> { InvSlotType.Any }))
                    {
                        LeftHand.Drop(infos.user.Human, true);
                    }
                }

                // Move the target's current outerwear to the user's inventory (yoink)
                UserInventory.TryPutItem(TargetOuterWear, 5, true, true, infos.user.Human, true, true);

                // Try again to put the autopulse on the target
                if (targetInventory.TryPutItem(infos.item, 4, true, true, infos.user.Human, true, true))
                {
                    HF.GiveItem(infos.target.Human, "ntsfx_zipper");
                }
            }
        });

        // Blue Shark
        loader.Register("blahaj", infos =>
        {
            HF.AddAffliction(infos.target.Human, "psychosis", -2f, infos.user.Human);
            HF.GiveItem(infos.target.Human, "ntsfx_squeak");
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
            loader.Register(id, WrenchFunctionality);
        }

        // Health Scanner
        loader.Register("healthscanner", infos =>
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
            float lowMedThreshold = NTConfig.Get("NTSCAN_lowmedThreshold", 1);
            float medHighThreshold = NTConfig.Get("NT_medhighThreshold", 1);

            // Strings
            List<string> vitalCategory = NTConfig.Get("NTSCAN_VitalCategory", new List<string>());
            List<string> removalCategory = NTConfig.Get("NTSCAN_RemovalCategory", new List<string>());
            List<string> customCategory = NTConfig.Get("NTSCAN_CustomCategory", new List<string>());
            List<string> ignoredCategory = NTConfig.Get("NTSCAN_IgnoredCategory", new List<string>());

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
            HF.GiveItem(infos.target.Human, "ntsfx_selfscan");
            Battery.Condition -= 5;

            HF.AddAffliction(infos.target.Human, "radiationsickness", 1, infos.user.Human);
            HF.AddAffliction(infos.user.Human, "radiationsickness", (float)0.6, infos.user.Human);

            // Print readout of afflictions
            string startReadout =
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                "Affliction readout for " +
                "‖color:end‖" +
                $"‖color:{nameColor.R},{nameColor.G},{nameColor.B}‖" +
                infos.target.Human.Name +
                "‖color:end‖" +
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                " on limb " +
                HF.LimbToString(limbType) +
                ":\n" +
                "‖color:end‖";

            var afflictionList = infos.target.Human.CharacterHealth.GetAllAfflictions();
            int afflictionsDisplayed = 0;

            foreach (var value in afflictionList)
            {
                float strength = MathF.Round(value.Strength);
                var prefab = value.Prefab;
                var afflictionLimb = infos.target.Human.CharacterHealth.GetAfflictionLimb(value);

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
                    HF.CharacterToClient(infos.user.Human),
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
        loader.Register("alienblood", infos =>
        {
            if (HF.GetSkillRequirementMet(infos.user.Human, "medical", 55f))
            {
                HF.AddAffliction(infos.target.Human, "bloodloss", 20f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "hemotransfusionshock", 100f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "psychosis", 30f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "bloodpressure", 20f, infos.user.Human);

            }
            else
            {
                HF.AddAffliction(infos.target.Human, "bloodloss", 15f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "hemotransfusionshock", 100f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "psychosis", 30f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "bloodpressure", 15f, infos.user.Human);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
        });

        // Saline
        loader.Register("antibloodloss1", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "Medical", 10);

            int totalAmount = success ? 50 : 30;
            int duration = 10;

            HF.ApplyAfflictionOverTime(infos.target.Human, "afsaline", totalAmount, duration, infos.user.Human);

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
        });

        // Bandage
        loader.Register("antibleeding1", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "medical", 10);
            bool hasMedExpertise = HF.HasTalent(infos.user.Human, "medicalexpertise");

            // Cookie mentioned this would work
            int successNum = success ? 1 : 0;
            int talentNum = hasMedExpertise ? 1 : 0;

            HF.AddAfflictionLimb(infos.target.Human, "bandageddirty", infos.targetLimb.type, -100, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "bandaged", infos.targetLimb.type, 36 + successNum * 12 + talentNum * 12, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, -18 - successNum * 6 - talentNum * 6, infos.user.Human);

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_bandage");
        });

        // Plastiseal
        loader.Register("antibleeding2", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "medical", 22);
            int successNum = success ? 1 : 0;

            HF.AddAfflictionLimb(infos.target.Human, "bandageddirty", infos.targetLimb.type, -100, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "bandaged", infos.targetLimb.type, 50 + successNum * 50, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, -24 - successNum * 24, infos.user.Human);

            if (HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type))
            {
                float affAmount = HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "burn");
                float healedAmount = Math.Min(affAmount, 200f);

                HF.AddAfflictionLimb(infos.target.Human, "burn", infos.targetLimb.type, -healedAmount, infos.user.Human);

                if (HF.IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", true))
                {
                    HF.GiveSkillScaled(infos.user.Human, "surgery", (float)healedAmount * 300);
                }
                else
                {
                    HF.GiveSkillScaled(infos.user.Human, "medical", (float)healedAmount * 150);
                }

            }
            else if (HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "burn", 0) > 50f)
            {
                HF.AddAfflictionLimb(infos.target.Human, "burn", infos.targetLimb.type, -12 - successNum * 12, infos.user.Human);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_bandage");
        });

        // Adrenaline
        loader.Register("adrenaline", infos =>
        {
            HF.AddAffliction(infos.target.Human, "afadrenaline", 55, infos.user.Human);
            HF.AddAffliction(infos.target.Human, "adrenalinerush", 8, infos.user.Human);

            if (HF.HasAffliction(infos.target.Human, "cardiacarrest", 0.1f))
            {
                HF.AddAffliction(infos.target.Human, "cardiacarrest", -100, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "fibrillation", 20, infos.user.Human);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
        });

        // Anaparalyzant
        // REWRITTEN FROM XML
        loader.Register("antiparalysis", infos =>
        {
            if (HF.GetSkillRequirementMet(infos.user.Human, "medical", 64f))
            {
                HF.AddAffliction(infos.target.Human, "paralysisresistance", 800f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "psychosis", 5f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "anesthesia", -200f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "afanaesthetic", -200f, infos.user.Human);
            }
            else
            {
                HF.ApplyAfflictionOverTime(infos.target.Human, "paralysisresistance", 390f, 60, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "psychosis", 45f, 60, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "anesthesia", -180f, 60, infos.user.Human);
                HF.ApplyAfflictionOverTime(infos.target.Human, "afanaesthetic", -180f, 60, infos.user.Human);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
        });

        // Nitroglycerin
        loader.Register("nitroglycerin", infos =>
        {
            if (HF.GetSkillRequirementMet(infos.user.Human, "medical", 35f))
            {
                HF.AddAffliction(infos.target.Human, "afpressuredrug", 100f, infos.user.Human);
            }
            else
            {
                HF.AddAffliction(infos.target.Human, "afpressuredrug", 50f, infos.user.Human);
            }

            HF.RemoveItem(infos.item);
            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
        });

        // ============== SurgicalEquipment ==============
        // Sutures
        // Bad to the bones 💀
        SutureAfflictions["sawedbones"] = new ItemsAfflictionInfos("sawedbones", 0, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["drilledbones"] = new ItemsAfflictionInfos("drilledbones", 0, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 95);
        });

        // Organs
        SutureAfflictions["liverswap"] = new ItemsAfflictionInfos("liverswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["heartswap"] = new ItemsAfflictionInfos("heartswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["lungswap"] = new ItemsAfflictionInfos("lungswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["kidneyswap"] = new ItemsAfflictionInfos("kidneyswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 95);
        });

        SutureAfflictions["brainswap"] = new ItemsAfflictionInfos("brainswap", 0, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 95);
        });

        // Arterialcuts

        SutureAfflictions["arterialcut"] = new ItemsAfflictionInfos("arterialcut", 3, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 95);
        });

        SutureAfflictions["carotidarterialcut"] = new ItemsAfflictionInfos("carotidarterialcut", 3, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 95);
        });

        SutureAfflictions["aorticrupture"] = new ItemsAfflictionInfos("aorticrupture", 3, infos => {

            if (!NTConfig.Get("NT_HardmodeAorticRupture", false)) return false;

            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 95);
        });

        // Tamponade

        SutureAfflictions["tamponade"] = new ItemsAfflictionInfos("tamponade", 3, infos => {

            if (NTConfig.Get("NT_OpenCloseTamponade", false)) return false;

            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 95);
        });

        // Misc

        SutureAfflictions["arteriesclamp"] = new ItemsAfflictionInfos("arteriesclamp", 0, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 95);
        });

        SutureAfflictions["internalbleeding"] = new ItemsAfflictionInfos("internalbleeding", 3, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 95);
        });

        SutureAfflictions["stroke"] = new ItemsAfflictionInfos("stroke", 6, infos => {
            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 95);
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

        // Sutures
        loader.Register("suture", infos =>
        {

            // Base NT has no stasis check ?
            if (!HF.GetSurgerySkillRequirementMet(infos.user.Human, 30))
            {
                HF.AddAfflictionLimb(infos.target.Human, "internaldamage", infos.targetLimb.type, 6, infos.user.Human);
                return;
            }

            // Common afflictions part
            double healeddamage = 0;

            // Could be better if HF.AddAfflictionLimb returned the amount healed
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "lacerations", 0), 0, 20);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "bitewounds", 0), 0, 20);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "explosiondamage", 0), 0, 20);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "gunshotwound", 0), 0, 20);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "bleeding", 0) / 10, 0, 40);
            healeddamage += Math.Clamp(HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "bleedingnonstop", 0) / 10, 0, 40);

            HF.AddAfflictionLimb(infos.target.Human, "lacerations", infos.targetLimb.type, -20, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "bitewounds", infos.targetLimb.type, -20, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "explosiondamage", infos.targetLimb.type, -20, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "gunshotwound", infos.targetLimb.type, -20, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, -40, infos.user.Human);
            HF.AddAfflictionLimb(infos.target.Human, "bleedingnonstop", infos.targetLimb.type, -40, infos.user.Human);

            HF.AddAfflictionLimb(infos.target.Human, "suturedw", infos.targetLimb.type, (float)healeddamage, infos.user.Human);

            HF.GiveSkillScaled(infos.user.Human, "medical", (float)healeddamage * 100);

            // A slight delay is needed for the Surgery afflictions to clear themselves.
            if (HF.HasAfflictionLimb(infos.target.Human, "sawedbones", infos.targetLimb.type, 1))
            {
                if (!infos.target.Human.IsHuman)
                {
                    HF.AddAffliction(infos.target.Human, "sawedbones", -200, infos.user.Human);
                    return;
                }

                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    HF.SurgicallyAmputateLimbAndGenerateItem(infos.user.Human, infos.target.Human, infos.targetLimb.type);
                }, 1);
            }

            HF.AddAffliction(infos.target.Human, "tshocktimeout", -100, infos.user.Human);

            // rewritten
            foreach (KeyValuePair<string, NTItems.ItemsAfflictionInfos> Pair in SutureAfflictions)
            {
                NTItems.ItemsAfflictionInfos affInfos = Pair.Value;
                // If the target doesn't have the affliction, we skip it
                if (!AfflictionPrefab.Prefabs.ContainsKey(affInfos.AfflictionID)) continue;
                AfflictionPrefab prefab = AfflictionPrefab.Prefabs[affInfos.AfflictionID];
                if (prefab == null)
                {
                    LuaCsLogger.LogError($"Error trying to heal {affInfos.AfflictionID} with sutures. The provided ID is probably incorrect.");
                    continue;
                }

                bool hasAffliction = prefab.LimbSpecific ? HF.HasAfflictionLimb(infos.target.Human, affInfos.AfflictionID, infos.targetLimb.type, 1) : HF.HasAffliction(infos.target.Human, affInfos.AfflictionID, 1);
                if (!hasAffliction) continue;

                // If the affliction's conditions are not met, we skip it
                if (!affInfos.Conditions.Invoke(infos)) continue;

                if (prefab.LimbSpecific)
                {
                    HF.SetAfflictionLimb(infos.target.Human, affInfos.AfflictionID, infos.targetLimb.type, 0, infos.user.Human, 0);
                }
                else
                {
                    HF.SetAffliction(infos.target.Human, affInfos.AfflictionID, 0, infos.user.Human, 0);
                }

                HF.GiveSurgerySkill(infos.user.Human, affInfos.XPGain);
            }
        });

        // Drainage
        DrainageAfflictions["pneumothorax"] = new ItemsAfflictionInfos("pneumothorax", 3, infos =>
        {
            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", LimbType.Torso, 95);
        }, "pneumothorax");

        DrainageAfflictions["tamponade"] = new ItemsAfflictionInfos("tamponade", 3, infos =>
        {
            if (NTConfig.Get("NT_OpenCloseTamponade", false)) return false;

            return HF.HasAfflictionLimb(infos.target.Human, "retractedskin", LimbType.Torso, 95); ;
        }, "tamponade");

        // From 48 lines to 12 my point stands, why tf was the lua function so girthy?
        loader.Register("drainage", infos =>
        {
            if (HF.HasAffliction(infos.target.Human, "stasis", (float)0.1)) { return; }

            bool consumeItem = false;

            foreach (KeyValuePair<string, ItemsAfflictionInfos> Pair in DrainageAfflictions)
            {
                ItemsAfflictionInfos affInfos = Pair.Value;
                if (!affInfos.Conditions.Invoke(infos)) continue;
                if (!HF.HasAffliction(infos.target.Human, affInfos.Case, 1)) continue;

                if (affInfos.Used != null)
                {
                    affInfos.Used.Invoke(infos);
                }

                HF.SetAffliction(infos.target.Human, affInfos.AfflictionID, 0, infos.user.Human, 0);
                HF.GiveSurgerySkill(infos.user.Human, affInfos.XPGain);
                consumeItem = true;
            }

            if (consumeItem) HF.RemoveItem(infos.item); // Actually use the item.
        });

        // Needle
        loader.Register("needle", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.targetLimb.type == LimbType.Torso &&
                !HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type))
            {
                if (HF.GetSkillRequirementMet(infos.user.Human, "medical", 20f))
                {
                    // If Pneumothorax OR Cardiac Tamponade is present, give skill.
                    if ((HF.HasAffliction(infos.target.Human, "pneumothorax") ||
                         HF.HasAffliction(infos.target.Human, "tamponade")) &&
                        !HF.HasAffliction(infos.target.Human, "needlec", 0.1f))
                    {
                        HF.GiveSkillScaled(infos.user.Human, "medical", 4000f);
                    }

                    HF.SetAffliction(infos.target.Human, "needlec", 100f, infos.user.Human, 0);

                    // If neither condition is present, cause a pneumothorax.
                    if (!HF.HasAffliction(infos.target.Human, "pneumothorax") &&
                        !HF.HasAffliction(infos.target.Human, "tamponade"))
                    {
                        HF.AddAffliction(infos.target.Human, "pneumothorax", 1f, infos.user.Human);
                    }

                    // Originally, this had a check for NTSP NTCompat code; I'll do that later. - Lukako
                    HF.RemoveItem(infos.item);
                }
                else
                {
                    HF.AddAffliction(infos.target.Human, "organdamage", 10f, infos.user.Human);
                    HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 10f, infos.user.Human);
                }
            }
        });

        // Osteosynthesis Implants
        loader.Register("osteosynthesisimplants", infos =>
        {
            if (!HF.CanPerformSurgeryOn(infos.target.Human) ||
                !HF.HasAfflictionLimb(infos.target.Human, "drilledbones", infos.targetLimb.type, 99f))
            {
                return;
            }

            // Originally NTSP integrated for Surgery Skill, TODO.
            if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 45f))
            {
                void removeAfflictionPlusGainSkill(string afflictionId, float skillGain)
                {
                    if (HF.HasAfflictionLimb(infos.target.Human, afflictionId, infos.targetLimb.type))
                    {
                        HF.SetAfflictionLimb(infos.target.Human, afflictionId, infos.targetLimb.type, 0f, infos.user.Human, 0);

                        if (HF.IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", true))
                        {
                            HF.GiveSkillScaled(infos.user.Human, "surgery", skillGain);
                        }
                        else
                        {
                            HF.GiveSkillScaled(infos.user.Human, "medical", skillGain / 4f);
                        }

                    }
                }

                void removeAfflictionNonLimbSpecificPlusGainSkill(string afflictionId, float skillGain)
                {
                    if (HF.HasAffliction(infos.target.Human, afflictionId))
                    {
                        HF.SetAffliction(infos.target.Human, afflictionId, 0f, infos.user.Human, 0);

                        if (HF.IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", true))
                        {
                            HF.GiveSkillScaled(infos.user.Human, "surgery", skillGain);
                        }
                        else
                        {
                            HF.GiveSkillScaled(infos.user.Human, "medical", skillGain / 4f);
                        }
                    }
                }

                var implantAfflictions = new Dictionary<string, float>
                {
                    //["ll_fracture"] = 10000f,
                    //["rl_fracture"] = 10000f,
                    //["la_fracture"] = 10000f,
                    //["ra_fracture"] = 10000f,
                    ["fracturedextremity"] = 10000f,
                    //["h_fracture"] = 10000f,
                    ["fracturedskull"] = 10000f,
                    //["n_fracture"] = 10000f,
                    ["fracturedneck"] = 10000f,
                    //["t_fracture"] = 10000f,
                    ["fracturedribs"] = 10000f,
                    // No idea if we still use boneclamp
                    //["boneclamp"] = 0f,
                    ["drilledbones"] = 0f
                };

                foreach (var kvp in implantAfflictions)
                {
                    string identifier = kvp.Key;
                    float skillGain = kvp.Value;

                    // This will crash the game if the identifier does not exist -Cookie
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

                HF.SetAfflictionLimb(infos.target.Human, "stimulatedbonegrowth", infos.targetLimb.type, 100f, infos.user.Human, 0);

                float itemUses = (1f / NTConfig.Get("NT_OsteoImplants_uses", 4)) * 100f;

                infos.item.Condition -= itemUses;
                if (infos.item.Condition <= 1f)
                {
                    HF.RemoveItem(infos.item);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 5f, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "internaldamage", infos.targetLimb.type, 5f, infos.user.Human);
            }

            // XML-derived sound
            if (HF.HasAfflictionLimb(infos.target.Human, "drilledbones", infos.targetLimb.type, 99f) &&
                HF.HasAffliction(infos.target.Human, "analgesia", 1f))
            {
                HF.GiveItem(infos.target.Human, "ntsfx_drill");
            }
        });

        // Spinal Cord Implants
        loader.Register("spinalimplant", infos =>
        {
            if (!HF.CanPerformSurgeryOn(infos.target.Human) ||
                !HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99f) ||
                !HF.HasAffliction(infos.target.Human, "spinalcordinjury", 0.1f))
            {
                return;
            }

            // Originally NTSP integrated for Surgery Skill, TODO.
            if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 45f))
            {
                HF.SetAffliction(infos.target.Human, "spinalcordinjury", 0f, infos.user.Human, 0);

                float itemUses = (1f / NTConfig.Get("NT_SpinalImplants_uses", 1)) * 100f;

                infos.item.Condition -= itemUses;

                if (infos.item.Condition <= 1f)
                {
                    HF.RemoveItem(infos.item);
                }

                if (HF.IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", true))
                {
                    HF.GiveSkillScaled(infos.user.Human, "surgery", 12000f);
                }
                else
                {
                    HF.GiveSkillScaled(infos.user.Human, "medical", 6000f);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 5f, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "internaldamage", infos.targetLimb.type, 5f, infos.user.Human);
            }

            // XML-derived sound
            if (HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99f))
            {
                HF.GiveItem(infos.target.Human, "ntsfx_drill");
            }
        });

        // Scalpel
        loader.Register("advscalpel", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target.Human) || HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 1)) return;

            bool success = HF.GetSurgerySkillRequirementMet(infos.user.Human, 30);

            if (success)
            {
                HF.AddAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user.Human) / 2, infos.user.Human);
                HF.SetAfflictionLimb(infos.target.Human, "suturedi", infos.targetLimb.type, 0, infos.user.Human, 0);
                HF.SetAfflictionLimb(infos.target.Human, "plastercast", infos.targetLimb.type, 0, infos.user.Human, 0);
                HF.SetAfflictionLimb(infos.target.Human, "bandaged", infos.targetLimb.type, 0, infos.user.Human, 0);

            }
            else
            {
                HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "lacerations", infos.targetLimb.type, 10, infos.user.Human);
            }

            HF.GiveItem(infos.target.Human, "ntsfx_slash");
        });

        // Hemostat
        loader.Register("advhemostat", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target.Human)) return;

            if (!HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 99) || HF.HasAfflictionLimb(infos.target.Human, "clampedbleeding", infos.targetLimb.type, 1)) return;

            HF.AddAfflictionLimb(infos.target.Human, "clampedbleeding", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user.Human) / 2, infos.user.Human);
        });

        // Skin Retractors
        loader.Register("advretractors", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target.Human)) return;

            if (!HF.HasAfflictionLimb(infos.target.Human, "clampedbleeding", infos.targetLimb.type, 99) || HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 1)) return;

            if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 30))
            {
                HF.AddAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user.Human) / 2, infos.user.Human);
            }
            else
            {
                HF.AddAfflictionLimb(infos.target.Human, "internaldamage", infos.targetLimb.type, 10, infos.user.Human);
            }
        });

        // Surgical Drill
        loader.Register("surgicaldrill", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target.Human) ||
                !HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99f) ||
                HF.HasAfflictionLimb(infos.target.Human, "drilledbones", infos.targetLimb.type, 1f))
            {
                return;
            }

            if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 45f))
            {
                HF.AddAfflictionLimb(infos.target.Human, "drilledbones", infos.targetLimb.type, 1f + HF.GetSurgerySkill(infos.user.Human) / 2f, infos.user.Human);
            }
            else
            {
                HF.AddAfflictionLimb(infos.target.Human, "burn", infos.targetLimb.type, 12f, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "internaldamage", infos.targetLimb.type, 10f, infos.user.Human);
            }

            // XML-derived sound
            if (HF.HasAffliction(infos.target.Human, "analgesia", 1f))
            {
                HF.GiveItem(infos.target.Human, "ntsfx_drill");
            }
        });

        // Surgical Saw
        loader.Register("surgerysaw", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (!HF.CanPerformSurgeryOn(infos.target.Human) ||
                !HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99f) ||
                HF.HasAfflictionLimb(infos.target.Human, "sawedbones", infos.targetLimb.type, 1f))
            {
                return;
            }

            if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 50f))
            {
                if (infos.targetLimb.type != LimbType.Torso)
                {
                    HF.AddAfflictionLimb(infos.target.Human, "sawedbones", infos.targetLimb.type, 1f + HF.GetSurgerySkill(infos.user.Human) / 2f, infos.user.Human);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15f, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "internaldamage", infos.targetLimb.type, 6f, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "lacerations", infos.targetLimb.type, 4f, infos.user.Human);
            }

            // XML-derived sound
            if (HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99f) &&
                HF.HasAffliction(infos.target.Human, "analgesia", 1f))
            {
                HF.GiveItem(infos.target.Human, "ntsfx_breakbone");
            }
        });

        // Tweezers
        loader.Register("tweezers", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            string usecase = "";

            // Through surgical wound
            if (HF.CanPerformSurgeryOn(infos.target.Human) &&
                HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 99f))
            {
                usecase = "surgery";
            }
            // Through normal wound
            else if (HF.HasAfflictionLimb(infos.target.Human, "gunshotwound", infos.targetLimb.type, 1f) ||
                     HF.HasAfflictionLimb(infos.target.Human, "explosiondamage", infos.targetLimb.type, 1f))
            {
                usecase = "ghetto";
            }

            if (usecase != "")
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 30f))
                {
                    HF.AddAfflictionLimb(infos.target.Human, "lacerations", infos.targetLimb.type, 5f, infos.user.Human);

                    if (usecase == "ghetto")
                    {
                        HF.AddAffliction(infos.target.Human, "traumaticshock", 5f, infos.user.Human);
                    }

                    void HealAfflictionGiveSkill(string identifier, float healAmount, float skillGain)
                    {
                        float affAmount = HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, identifier);
                        float healedAmount = Math.Min(affAmount, healAmount);

                        HF.AddAfflictionLimb(infos.target.Human, identifier, infos.targetLimb.type, -healAmount, infos.user.Human);

                        if (HF.IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", true))
                        {
                            HF.GiveSkillScaled(infos.user.Human, "surgery", healedAmount * skillGain);
                        }
                        else
                        {
                            HF.GiveSkillScaled(infos.user.Human, "medical", healedAmount * skillGain / 2f);
                        }

                    }

                    float foreignBody = HF.GetAfflictionStrengthLimb(infos.target.Human, infos.targetLimb.type, "foreignbody", 0f);
                    float scrapDropChance = Math.Min(foreignBody, 5f) / 5f * 0.05f;

                    if (HF.Chance(scrapDropChance))
                    {
                        HF.GiveItem(infos.user.Human, "scrap");
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
                    HF.AddAfflictionLimb(infos.target.Human, "internaldamage", infos.targetLimb.type, 6f, infos.user.Human);
                }
            }
            else
            {
                bool sedated = HF.CanPerformSurgeryOn(infos.target.Human);

                // pinchy pinchy!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 1f, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "lacerations", infos.targetLimb.type, 0.5f, infos.user.Human);

                if (!sedated)
                {
                    HF.AddAfflictionLimb(infos.target.Human, "intensepain", infos.targetLimb.type, 5f, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "stun", 0.1f, infos.user.Human);
                }

                // special head handling
                if (infos.targetLimb.type == LimbType.Head)
                {
                    HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 3f, infos.user.Human);
                    HF.AddAfflictionLimb(infos.target.Human, "lacerations", infos.targetLimb.type, 2f, infos.user.Human);

                    if (!sedated)
                    {
                        HF.AddAfflictionLimb(infos.target.Human, "intensepain", infos.targetLimb.type, 5f, infos.user.Human);
                    }
                }
            }
        });

        // Liver Transplant Scalpel
        loader.Register("organscalpel_liver", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Torso) return;
            if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target.Human, "liverremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target.Human, "liverswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target.Human, "liverdamage", 0) >= 100)
                        HF.SetAffliction(infos.target.Human, "liverremoved", 100, infos.user.Human, 0);
                    else
                        HF.SetAffliction(infos.target.Human, "liverswap", 100, infos.user.Human, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15, infos.user.Human);
                    HF.AddAfflictionLimb(infos.target.Human, "organdamage", infos.targetLimb.type, 5, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "liverdamage", 20, infos.user.Human);
                }
                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target.Human, "liverdamage", 0);
                if (damage >= 100) return;
                if (!HF.GetSurgerySkillRequirementMet(infos.user.Human, 50)) return;

                HF.SetAffliction(infos.target.Human, "liverremoved", 100, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "liverswap", 0, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "liverdamage", 100, infos.user.Human, 0);
                HF.AddAffliction(infos.target.Human, "organdamage", (100 - damage) / 5, infos.user.Human);

                if (damage < 90)
                {
                    string transplantID = "livertransplant_q1";
                    if (infos.user.Tags.HasTag("tag", "organssellforfull")) transplantID = "livertransplant";
                    SpawnOrganTransplantInContainer(transplantID, infos.user.Human, 100 - damage);
                }
            }
        });

        // Lung Transplant Scalpel
        loader.Register("organscalpel_lungs", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Torso) return;
            if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target.Human, "lungremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target.Human, "lungswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target.Human, "lungdamage", 0) >= 100)
                        HF.SetAffliction(infos.target.Human, "lungremoved", 100, infos.user.Human, 0);
                    else
                        HF.SetAffliction(infos.target.Human, "lungswap", 100, infos.user.Human, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15, infos.user.Human);
                    HF.AddAfflictionLimb(infos.target.Human, "organdamage", infos.targetLimb.type, 5, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "lungdamage", 20, infos.user.Human);
                }

                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target.Human, "lungdamage", 0);
                if (damage >= 100) return;

                HF.SetAffliction(infos.target.Human, "lungremoved", 100, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "lungswap", 0, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "lungdamage", 100, infos.target.Human, 0);
                HF.SetAffliction(infos.target.Human, "respiratoryarrest", 100, infos.target.Human, 0);
                HF.SetAffliction(infos.target.Human, "pneumothorax", 0, infos.target.Human, 0);
                HF.SetAffliction(infos.target.Human, "needlec", 0, infos.target.Human, 0);
                HF.AddAffliction(infos.target.Human, "organdamage", (100 - damage) / 5, infos.target.Human);

                if (damage < 90)
                {
                    string transplantID = "lungtransplant_q1";
                    if (infos.user.Tags.HasTag("tag", "organssellforfull")) transplantID = "lungtransplant";
                    
                    SpawnOrganTransplantInContainer(transplantID, infos.user.Human, 100 - damage);
                    
                }
            }
        });

        // Heart Transplant Scalpel
        loader.Register("organscalpel_heart", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Torso) return;
            if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target.Human, "heartremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target.Human, "heartswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target.Human, "heartdamage", 0) >= 100)
                        HF.SetAffliction(infos.target.Human, "heartremoved", 100, infos.user.Human, 0);
                    else
                        HF.SetAffliction(infos.target.Human, "heartswap", 100, infos.user.Human, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15, infos.user.Human);
                    HF.AddAfflictionLimb(infos.target.Human, "organdamage", infos.targetLimb.type, 5, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "heartdamage", 20, infos.user.Human);
                }

                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target.Human, "heartdamage", 0);
                if (damage >= 100) return;

                HF.SetAffliction(infos.target.Human, "heartremoved", 100, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "heartswap", 0, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "heartdamage", 100, infos.target.Human, 0);
                HF.SetAffliction(infos.target.Human, "cardiacarrest", 100, infos.target.Human, 0);
                HF.SetAffliction(infos.target.Human, "tamponade", 0, infos.target.Human, 0);
                HF.SetAffliction(infos.target.Human, "heartattack", 0, infos.target.Human, 0);
                HF.AddAffliction(infos.target.Human, "organdamage", (100 - damage) / 5, infos.target.Human);

                if (damage < 90)
                {
                    string transplantID = "hearttransplant_q1";
                    if (infos.user.Tags.HasTag("tag", "organssellforfull")) transplantID = "hearttransplant";

                    SpawnOrganTransplantInContainer(transplantID, infos.user.Human, 100 - damage);
                }
            }
        });

        // Kidney Transplant Scalpel
        loader.Register("organscalpel_kidneys", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Torso) return;
            if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target.Human, "kidneyremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target.Human, "kidneyswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target.Human, "kidneydamage", 0) >= 100)
                        HF.SetAffliction(infos.target.Human, "kidneyremoved", 100, infos.user.Human, 0);
                    else
                        HF.SetAffliction(infos.target.Human, "kidneyswap", 100, infos.user.Human, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15, infos.user.Human);
                    HF.AddAfflictionLimb(infos.target.Human, "organdamage", infos.targetLimb.type, 5, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "kidneydamage", 10, infos.user.Human);
                }
                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target.Human, "kidneydamage", 0);
                if (damage >= 100) return;

                string transplantID = "kidneytransplant_q1";
                if (infos.user.Tags.HasTag("tag", "organssellforfull")) transplantID = "kidneytransplant";

                if (damage < 50)
                {
                    // First kidney
                    HF.SetAffliction(infos.target.Human, "kidneydamage", 50, infos.user.Human, 0);
                    HF.AddAffliction(infos.target.Human, "organdamage", (100 - damage) / 5, infos.user.Human);
                    SpawnOrganTransplantInContainer(transplantID, infos.user.Human, 100);
                }
                else if (damage < 95)
                {
                    // Second kidney
                    HF.SetAffliction(infos.target.Human, "kidneyremoved", 100, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "kidneyswap", 0, infos.user.Human, 0);
                    HF.SetAffliction(infos.target.Human, "kidneydamage", 100, infos.user.Human, 0);
                    HF.AddAffliction(infos.target.Human, "organdamage", (100 - damage) / 5, infos.user.Human);
                    SpawnOrganTransplantInContainer(transplantID, infos.user.Human, 100 - (damage - 50) * 2);
                }
            }
        });

        // Brain Transplant Scalpel
        loader.Register("organscalpel_brain", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.targetLimb.type != LimbType.Head) return;
            if (!HF.HasAfflictionLimb(infos.target.Human, "retractedskin", infos.targetLimb.type, 1)) return;

            bool procureready = HF.GetAfflictionStrength(infos.target.Human, "brainremoved", 0) <= 0
                             && HF.GetAfflictionStrength(infos.target.Human, "brainswap", 0) >= 0.1f;

            if (!procureready)
            {
                if (HF.GetSurgerySkillRequirementMet(infos.user.Human, 40))
                {
                    if (HF.GetAfflictionStrength(infos.target.Human, "neurotrauma", 0) >= 100)
                        HF.SetAffliction(infos.target.Human, "brainremoved", 100, infos.user.Human, 0);
                    else
                        HF.SetAffliction(infos.target.Human, "brainswap", 100, infos.user.Human, 0);
                }
                else
                {
                    HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15, infos.user.Human);
                    HF.AddAffliction(infos.target.Human, "neurotrauma", 50, infos.user.Human);
                }
                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
            else
            {
                float damage = HF.GetAfflictionStrength(infos.target.Human, "neurotrauma", 0);
                if (damage >= 100) return;

                HF.AddAffliction(infos.target.Human, "neurotrauma", 100, infos.user.Human);
                HF.SetAffliction(infos.target.Human, "brainremoved", 100, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "brainswap", 0, infos.user.Human, 0);

                if (HF.IsNTSPEnabled())
                {
                    if (HF.HasAffliction(infos.target.Human, "artificialbrain"))
                    {
                        HF.SetAffliction(infos.target.Human, "artificialbrain", 0, infos.user.Human, 0);
                        damage = 100;
                    }
                }

                if (damage < 90)
                {
                    float finalCondition = 100 - damage;
                    var client = HF.CharacterToClient(infos.target.Human);
                    var capturedTarget = infos.target.Human;
                    var capturedUser = infos.user.Human;

                    var container = infos.user.Human.Inventory.GetItemInLimbSlot(InvSlotType.RightHand);
                    if (container == null || container.OwnInventory == null || container.OwnInventory.IsFull())
                        container = infos.user.Human.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand);
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

        TraumaShearsAfflictions.Add("plastercast");

        loader.Register("traumashears", infos =>
        {
            if (HF.HasAffliction(infos.target.Human, "stasis", (float)0.1)) { return; }

            List<string> cuttables = CuttableAfflictions;
            cuttables = [.. cuttables, .. TraumaShearsAfflictions];

            if (HF.GetSkillRequirementMet(infos.user.Human, "medical", 10))
            {
                foreach (var affID in cuttables)
                {
                    HF.SetAfflictionLimb(infos.target.Human, affID, infos.targetLimb.type, 0, infos.user.Human, 0);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "lacerations", infos.targetLimb.type, 10, infos.user.Human);
            }

        });

        // Diving Knife (Technically Override but similar to shears)
        loader.Register("divingknife", infos =>
        {
            if (HF.HasAffliction(infos.target.Human, "stasis", (float)0.1)) { return; }

            List<string> cuttables = CuttableAfflictions;

            if (HF.GetSkillRequirementMet(infos.user.Human, "medical", 30))
            {
                foreach (var affID in cuttables)
                {
                    HF.SetAfflictionLimb(infos.target.Human, affID, infos.targetLimb.type, 0, infos.user.Human, 0);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target.Human, "bleeding", infos.targetLimb.type, 15, infos.user.Human);
                HF.AddAfflictionLimb(infos.target.Human, "lacerations", infos.targetLimb.type, 10, infos.user.Human);
            }

        });

        // Antiseptic Sprayer
        loader.Register("antisepticspray", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.item.Condition <= 0)
            {
                return;
            }

            infos.item.Condition = 0; // Start Cooldown

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                infos.item.Condition = 100; // Finish Cooldown
            }, 2000);

            var containedItem = infos.item.OwnInventory?.GetItemAt(0);
            bool hasSaline = containedItem != null && containedItem.Prefab.Identifier == "antibloodloss1";
            bool hasAntiseptic = containedItem != null && containedItem.Prefab.Identifier == "antiseptic";

            // Surgery use
            if (hasSaline && infos.targetLimb.type == LimbType.Torso && HF.HasAffliction(infos.target.Human, "infectedcavity", 1f) && HF.HasAffliction(infos.target.Human, "retractedskin", 1f))
            {
                HF.RemoveItem(containedItem);
                HF.GiveItem(infos.target.Human, "ntsfx_spray");

                float skill = HF.GetSurgerySkill(infos.user.Human);
                float delay = 11000f - skill * 10f;

                HF.AddAfflictionLimb(infos.target.Human, "caviclean", infos.targetLimb.type, Math.Max(0f + skill / 2f, 10f), infos.user.Human);

                LuaCsSetup.Instance.Timer.Wait((object[] _) =>
                {
                    if (!HF.HasAffliction(infos.target.Human, "infectedcavity", 1f))
                    {
                        if (HF.IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", true))
                        {
                            HF.GiveSkillScaled(infos.user.Human, "surgery", 20000f);
                        }
                        else
                        {
                            HF.GiveSkillScaled(infos.user.Human, "medical", 10000f);
                        }
                    }
                }, (int)10000);

                return;
            }

            // Antiseptic use
            if (hasAntiseptic)
            {
                containedItem.Condition -= 10f;

                HF.AddAffliction(infos.target.Human, "infectedwound", -100f, infos.user.Human);
                HF.AddAffliction(infos.target.Human, "ointmented", 20f, infos.user.Human);
                HF.GiveItem(infos.target.Human, "ntsfx_spray");
            }
        });

        // ============== Toggleable ==============
        // Endovascular Balloon
        loader.Register("endovascballoon", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.targetLimb.type == LimbType.Torso && HF.HasAfflictionLimb(infos.target.Human, "surgeryincision", infos.targetLimb.type, 1f) && HF.HasAffliction(infos.target.Human, "aorticrupture", 1f))
            {
                // Main effect
                HF.AddAffliction(infos.target.Human, "balloonedaorta", 100f, infos.user.Human);
                HF.SetAffliction(infos.target.Human, "internalbleeding", 0f, infos.user.Human, 0);

                if (HF.IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", true))
                {
                    HF.GiveSkillScaled(infos.user.Human, "surgery", 10000f);
                }
                else
                {
                    HF.GiveSkillScaled(infos.user.Human, "medical", 5000f);
                }

                HF.GiveItem(infos.target.Human, "ntsfx_syringe");
                HF.RemoveItem(infos.item);
            }
        });

        // Medical Stent
        loader.Register("medstent", infos =>
        {
            // Stasis check
            if (HF.HasAffliction(infos.target.Human, "stasis", 0.1f)) return;

            if (infos.targetLimb.type == LimbType.Torso &&
                HF.HasAffliction(infos.target.Human, "balloonedaorta", 1f))
            {
                // Remove vascular condition
                HF.SetAffliction(infos.target.Human, "balloonedaorta", 0f, infos.user.Human, 0);
                HF.SetAffliction(infos.target.Human, "aorticrupture", 0f, infos.user.Human, 0);

                if (HF.IsNTSPEnabled() && NTConfig.Get("NTSP_enableSurgerySkill", true))
                {
                    HF.GiveSkillScaled(infos.user.Human, "surgery", 20000f);
                }
                else
                {
                    HF.GiveSkillScaled(infos.user.Human, "medical", 10000f);
                }
            }

            HF.GiveItem(infos.target.Human, "ntsfx_syringe");
            HF.RemoveItem(infos.item);
        });

        // Sodium Nitroprusside
        loader.Register("pressuremeds", infos =>
        {
            bool success = HF.GetSkillRequirementMet(infos.user.Human, "medical", 10f);

            int totalAmount = success ? 50 : 30;
            int duration = 10;

            HF.ApplyAfflictionOverTime(infos.target.Human, "afpressuredrug", totalAmount, duration, infos.user.Human);

            HF.GiveItem(infos.target.Human, "ntsfx_pills");
            HF.RemoveItem(infos.item);
        });
    }

    public static readonly HashSet<string> FixCondition = new()
    {
        "healthscanner",
        "bloodanalyzer",
        "defibrillator",
        "bvm",
        "autocpr",
        "aed",
        "antisepticspray"
    };

    private static void TryFixCondition(Item item)
    {
        if (item != null && FixCondition.Contains(item.Prefab.Identifier.Value))
        {
            item.Condition = 100f;
        }
    }

    public static void RefreshCondition()
    {
        foreach (Item item in Item.ItemList)
        {
            TryFixCondition(item);
        }
    }

    public static void EnsureWorkingItems()
    {
#pragma warning disable CS0618

        LuaCsSetup.Instance.Hook.Add("roundStart", "NT.RoundStartFixItems", (params object[] args) =>
        {
            RefreshCondition(); // catches items already present when the round starts
            return null;
        });


        LuaCsSetup.Instance.Hook.Add("item.created", "NT.ItemCreatedFixItems", (params object[] args) =>
        {
            TryFixCondition(args[0] as Item);
            return null;
        });

#pragma warning restore CS0618
    }
}