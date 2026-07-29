// Disable the Hook warning
#pragma warning disable CS0618
namespace Neurotrauma
{
    public static class NTMultiscalpel
    {
        private static readonly Random _rng = new Random();

        private static string GetMultiscalpelMode(Item item)
        {
            if (string.IsNullOrEmpty(item.Tags)) return "";

            foreach (var tag in item.Tags.Split(','))
            {
                var t = tag.Trim();
                if (t.StartsWith("multiscalpel_"))
                    return t.Substring("multiscalpel_".Length);
            }
            return "";
        }

        public static void SetMultiscalpelFunction(Item item, string func)
        {
            item.Tags = string.IsNullOrEmpty(func) ? "" : "multiscalpel_" + func;
            RefreshScalpelDescription(item);
        }

        private static void RefreshScalpelDescription(Item item)
        {
            // Host-side only in multiplayer
            #if CLIENT
                if (GameMain.IsMultiplayer) return;
            #endif

            if (Entity.Spawner == null)
            {
                LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                {
                    RefreshScalpelDescription(item);
                }, 35);
                return;
            }

            string functiontag = GetMultiscalpelMode(item);
            if (string.IsNullOrEmpty(functiontag)) return;

            var targetInventory = item.ParentInventory;
            int targetSlot = targetInventory?.FindIndex(item) ?? 0;
            var prefab = item.Prefab;
            var worldPos = item.WorldPosition;

            HF.RemoveItem(item);

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                Entity.Spawner.AddItemToSpawnQueue(prefab, worldPos, null, null,
                    (Item newItem) =>
                    {
                        if (targetInventory != null)
                            targetInventory.TryPutItem(newItem, targetSlot, true, true, null);

                        newItem.DescriptionTag = "multiscalpel." + functiontag;
                        newItem.Tags = "multiscalpel_" + functiontag;
                    });
            }, 35);
        }

        public static void RegisterMultiscalpel()
        {
            LuaCsSetup.Instance.Hook.Add("NT.multiscalpel.incision", "NTCS.multiscalpel.incision",
                (params object[] args) =>
                {
                    var item = args[2] as Item;
                    if (item != null) SetMultiscalpelFunction(item, "incision");
                    return null;
                });

            LuaCsSetup.Instance.Hook.Add("NT.multiscalpel.bandage", "NTCS.multiscalpel.bandage",
                (params object[] args) =>
                {
                    var item = args[2] as Item;
                    if (item != null) SetMultiscalpelFunction(item, "bandage");
                    return null;
                });

            LuaCsSetup.Instance.Hook.Add("NT.multiscalpel.speedflex", "NTCS.multiscalpel.speedflex",
                (params object[] args) =>
                {
                    var item = args[2] as Item;
                    if (item != null) SetMultiscalpelFunction(item, "speedflex");
                    return null;
                });

            NTItemMethods.RegisterItemUseFunction("multiscalpel", infos =>
            {
                string mode = GetMultiscalpelMode(infos.item);
                if (string.IsNullOrEmpty(mode)) mode = "none";

                switch (mode)
                {
                    case "none":
                        break;

                    case "incision":
                        NTItemMethods.NTItemsRegistry["advscalpel"]?.Invoke(infos);
                        break;

                    case "bandage":
                        UseBandageMode(infos);
                        break;

                    case "speedflex":
                        UseSpeedflexMode(infos);
                        break;
                }

                if (mode != "none")
                {
                    string captured = mode;
                    LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
                    {
                        infos.item.Tags = "multiscalpel_" + captured;
                    }, 50);
                }
            });
        }

        private static void UseBandageMode(NTItemMethods.ItemUpdateFunctionInfos infos)
        {
            // Check if there's anything cuttable on this limb.
            bool canCut = false;
            var cuttables = new List<string>(NTItemMethods.CuttableAfflictions);
            cuttables.AddRange(NTItemMethods.TraumaShearsAfflictions);

            foreach (var val in cuttables)
            {
                var prefab = AfflictionPrefab.Prefabs[val];
                if (prefab == null) continue;

                if (prefab.LimbSpecific)
                {
                    if (HF.HasAfflictionLimb(infos.target, val, infos.targetLimb.type, 0.1f)) { canCut = true; break; }
                }
                else if (infos.targetLimb.type == prefab.IndicatorLimb)
                {
                    if (HF.HasAffliction(infos.target, val, 0.1f)) { canCut = true; break; }
                }
            }

            if (canCut)
            {
                NTItemMethods.NTItemsRegistry["traumashears"]?.Invoke(infos);
                return;
            }

            // Malpractice time!!!!!!!!!!!!!!
            bool open = HF.HasAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1f);
            bool isTorso = infos.targetLimb.type == LimbType.Torso;
            bool isHead = infos.targetLimb.type == LimbType.Head;

            // I'm fairly certain this is how random chance works now?
            if (!open)
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 6f + (float)_rng.NextDouble() * 4f, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 2.5f + (float)_rng.NextDouble() * 5f, infos.user);
                HF.GiveItem(infos.target, "ntsfx_slash");
            }
            else if (isTorso)
            {
                HF.AddAffliction(infos.target, "internalbleeding", 6f + (float)_rng.NextDouble() * 12f, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 4f + (float)_rng.NextDouble() * 6f, infos.user);
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 4f + (float)_rng.NextDouble() * 6f, infos.user);

                float roll = (float)_rng.NextDouble();
                if (roll < 0.25f) HF.AddAffliction(infos.target, "kidneydamage", 10f + (float)_rng.NextDouble() * 10f, infos.user);
                else if (roll < 0.50f) HF.AddAffliction(infos.target, "liverdamage", 10f + (float)_rng.NextDouble() * 10f, infos.user);
                else if (roll < 0.75f) HF.AddAffliction(infos.target, "lungdamage", 10f + (float)_rng.NextDouble() * 10f, infos.user);
                else HF.AddAffliction(infos.target, "heartdamage", 10f + (float)_rng.NextDouble() * 10f, infos.user);

                HF.GiveItem(infos.target, "ntsfx_slash");
            }
            else if (isHead)
            {
                HF.AddAffliction(infos.target, "cerebralhypoxia", 15f + (float)_rng.NextDouble() * 15f, infos.user);
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 10f + (float)_rng.NextDouble() * 10f, infos.user);
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 6f + (float)_rng.NextDouble() * 12f, infos.user);
                HF.GiveItem(infos.target, "ntsfx_slash");
            }
            else
            {
                // Open arm/leg
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 6f + (float)_rng.NextDouble() * 6f, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 4f + (float)_rng.NextDouble() * 6f, infos.user);
                HF.AddAfflictionLimb(infos.target, "internaldamage", infos.targetLimb.type, 4f + (float)_rng.NextDouble() * 6f, infos.user);

                if (HF.Chance(0.1f)) HF.BreakLimb(infos.target, infos.targetLimb.type);

                HF.GiveItem(infos.target, "ntsfx_slash");
            }
        }

        private static void UseSpeedflexMode(NTItemMethods.ItemUpdateFunctionInfos infos)
        {
            var torsoLimb = infos.target.AnimController?.MainLimb ?? infos.targetLimb;
            // Me when I LIE!!!!
            // Act like its the torso (even when we apply to a limb)
            var torsoInfos = new NTItemMethods.ItemUpdateFunctionInfos(infos.item, infos.user, infos.target, torsoLimb);
            var UsedOnLimb = HF.NormalizeLimbType(infos.targetLimb.type);

            switch (UsedOnLimb)
            {
                case LimbType.Head:
                    NTItemMethods.NTItemsRegistry["organscalpel_brain"]?.Invoke(infos);
                    break;

                case LimbType.LeftArm:
                    NTItemMethods.NTItemsRegistry["organscalpel_kidneys"]?.Invoke(torsoInfos);
                    break;

                case LimbType.Torso:
                    NTItemMethods.NTItemsRegistry["organscalpel_liver"]?.Invoke(torsoInfos);
                    break;

                case LimbType.RightArm:
                    NTItemMethods.NTItemsRegistry["organscalpel_heart"]?.Invoke(torsoInfos);
                    break;

                case LimbType.LeftLeg:
                    NTItemMethods.NTItemsRegistry["organscalpel_lungs"]?.Invoke(torsoInfos);
                    break;
            }
        }
    }
}