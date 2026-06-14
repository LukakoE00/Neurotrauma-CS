
namespace Neurotrauma;



public class NTItemMethods
{

    /**
     * <summary>
     * Contains all the data necessary to add an Affliction to DrainageAfflictions.
     * 
     * </summary>
     * 
     */
    public class ItemsAfflictionInfos { 
    
        ///<summary>The ID defined in the XML. The affliction MUST be non limb-specific.</summary>
        public string AfflictionID { get; }

        ///<summary>The amount of XP given to the surgery or medical skill when the item is applied successfully.</summary>
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
            //TODO check if affID is limb specific and throw error if so

            this.AfflictionID = affID;
            this.XPGain = xpGain;
            this.Conditions = conditions;
        }
    
    }

    /**
     * <summary>
     * Contains the list of every afflictions cured by drainage.
     * 
     * </summary>
     */
    public static List<ItemsAfflictionInfos> DrainageAfflictions { get; } = [];


    /**
     * <summary>
     * Contains the list of every afflictions removable by traumashears and knives.
     * </summary>
     */
    public static List<string> CuttableAfflictions { get; } = [];

    /**
     * <summary>
     * Contains the list of every afflictions removable by traumashears only.
     * </summary>
     */
    public static List<string> TraumaShearsAfflictions { get; } = [];

    /**
     * <summary>
     * Contains the list of every afflictions healable by sutures.
     * </summary>
     */
    public static List<ItemsAfflictionInfos> SutureAfflictions { get; } = [];

    /**
     * <summary>
     * Contains all the data necessary for an item use function.
     * </summary>
     */
    public class ItemUpdateFunctionInfos
    {
        public Item item { get; }
        public Character user {  get; }
        public Character target { get; }
        public Limb targetLimb { get; }

        public ItemUpdateFunctionInfos(Item item, Character user, Character target, Limb targetLimb)
        {
            this.item = item;
            this.user = user;
            this.target = target;
            this.targetLimb = targetLimb;
        }
    }

    public static Dictionary<string, Action<ItemUpdateFunctionInfos>> NTItemsRegistry { get; } = new Dictionary<string, Action<ItemUpdateFunctionInfos>> { };

    public static void DefineAllItems()
    {
        // Azathioprine
        RegisterItemUseFunction("immunosuppressant", infos =>
        {
            bool success = HF.GetSkillRequirmentMet(infos.user, "Medical", 10);
            HF.AddAffliction(infos.target, "afimmunosuppressant", success ? 5 : 3, infos.user);
        });

        // Antibiotic Ointment
        RegisterItemUseFunction("ointment", (infos) =>
        {

            bool success = HF.GetSkillRequirmentMet(infos.user, "medical", 10);

            HF.AddAfflictionLimb(infos.target, "ointmented", infos.targetLimb.type, success ? 120 : 60, infos.user);
            HF.AddAfflictionLimb(infos.target, "infectedwound", infos.targetLimb.type, success ? -72 : -24, infos.user);

            // Check for third degree burn might not be working correctly
            if (HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "burn", 0) < 50)
            {
                HF.AddAfflictionLimb(infos.target, "burn", infos.targetLimb.type, success ? -12 : (float)-7.2, infos.user);
            }

            HF.GiveItem(infos.target, "ntsfx_ointment");

        });

        // Scalpel
        RegisterItemUseFunction("advscalpel", infos =>
        {
            // Not in stasis
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            if (!HF.CanPerformSurgeryOn(infos.target) || HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 1)) { return; }

            bool success = HF.GetSurgerySkillRequirmentMet(infos.user, 30);

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

        DrainageAfflictions.Add(new ItemsAfflictionInfos("pneumothorax", 3, infos =>
        {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
        }));

        DrainageAfflictions.Add(new ItemsAfflictionInfos("tamponade", 3, infos =>
        {
            bool retractedSkin = HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);

            // TODO check for Config thingy

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

        // Hemostat
        RegisterItemUseFunction("advhemostat", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            if (!HF.CanPerformSurgeryOn(infos.target)) { return; }

            if (!HF.HasAffliction(infos.target, "surgeryincision", 99) || HF.HasAffliction(infos.target, "clampedbleeders", 1)) {  return; }

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

        // TODO may need to change the ids to match the new ones
        CuttableAfflictions.Add("bandaged");
        CuttableAfflictions.Add("dirtybandage");
        CuttableAfflictions.Add("arteriesclamp");

        TraumaShearsAfflictions.Add("gypsumcast");

        // Trauma Shears
        RegisterItemUseFunction("traumashears", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            List<string> cuttables = CuttableAfflictions;
            cuttables = [.. cuttables, .. TraumaShearsAfflictions];

            if (HF.GetSkillRequirmentMet(infos.user, "medical", 10))
            {
                foreach (var affID in cuttables)
                {
                    HF.SetAfflictionLimb(infos.target, affID, infos.targetLimb.type, 0, infos.user, 0);
                }
            } else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 10, infos.user);
            }

        });

        // Diving Knife 
        RegisterItemUseFunction("divingknife", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            List<string> cuttables = CuttableAfflictions;

            if (HF.GetSkillRequirmentMet(infos.user, "medical", 30))
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

        // Gypsum
        RegisterItemUseFunction("gypsum", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            // Needs to be bandaged, not already in a cast, not during a surgery, and the limb needs to be extremity.
            if (!HF.HasAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, (float) 0.1) || 
            HF.HasAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, (float) 0.1) ||
            HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, (float) 0.1) ||
            !HF.LimbIsExtremity(infos.targetLimb.type)) { return; }

            if (HF.GetSkillRequirmentMet(infos.user, "medical", (float) 40))
            {
                HF.SetAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, 0, infos.user, 0);
                HF.SetAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, 100, infos.user, 0);
                HF.BreakLimb(infos.target, infos.targetLimb.type, -20);
                HF.GiveSkillScaled(infos.user, "medical", 6000);

            } 

            HF.RemoveItem(infos.item);


        });
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
    public static void Override_ApplyTreatment(Item __instance, Character user, Character character, Limb targetLimb)
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
    public static void Override_Use(Item __instance, float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
    {
       // LuaCsLogger.Log("use");
    }
}


