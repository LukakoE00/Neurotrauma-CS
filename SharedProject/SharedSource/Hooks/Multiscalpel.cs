// Disable the Hook warning
#pragma warning disable CS0618
using System.ComponentModel;

namespace Neurotrauma
{
    public static class NTMultiscalpel
    {
        private static NTItems.NTItemFunctionLoader loader = Initialization.NTItemsLoader;

        private static readonly Random _rng = new Random();

        public static string GetMultiscalpelMode(Item item)
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

        public static void RefreshScalpelDescription(Item item)
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

        public static void UseBandageMode(NTItems.ItemUpdateFunctionInfos infos)
        {
            // Check if there's anything cuttable on this limb.
            bool canCut = false;
            var cuttables = new List<string>(NTItems.CuttableAfflictions);
            cuttables.AddRange(NTItems.TraumaShearsAfflictions);

            foreach (var val in cuttables)
            {
                var prefab = AfflictionPrefab.Prefabs[val];
                if (prefab == null) continue;

                if (prefab.LimbSpecific)
                {
                    if (infos.target.HasAfflictionLimb(val, infos.targetLimb.type,0.1f)) { canCut = true; break; }
                }
                else if (infos.targetLimb.type == prefab.IndicatorLimb)
                {
                    if (infos.target.HasAffliction(val, 0.1f)) { canCut = true; break; }
                }
            }

            if (canCut)
            {
                loader.Get("traumashears")?.Invoke(infos);
                return;
            }

            // Malpractice time!!!!!!!!!!!!!!
            bool open = infos.target.HasAfflictionLimb("retractedskin", infos.targetLimb.type, 1f);
            bool isTorso = infos.targetLimb.type == LimbType.Torso;
            bool isHead = infos.targetLimb.type == LimbType.Head;

            // I'm fairly certain this is how random chance works now?
            if (!open)
            {

                infos.target.AddAfflictionLimb("bleeding", infos.targetLimb.type, 6f + (float)_rng.NextDouble() * 4f, infos.user);
                infos.target.AddAfflictionLimb("lacerations", infos.targetLimb.type, 2.5f + (float)_rng.NextDouble() * 5f, infos.user);
                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
            else if (isTorso)
            {

                infos.target.AddAffliction("internalbleeding", 6f + (float)_rng.NextDouble() * 12f, infos.user);
                infos.target.AddAfflictionLimb("lacerations", infos.targetLimb.type, 4f + (float)_rng.NextDouble() * 6f, infos.user);
                infos.target.AddAfflictionLimb("internaldamage", infos.targetLimb.type, 4f + (float)_rng.NextDouble() * 6f, infos.user);

                float roll = (float)_rng.NextDouble();
                if (roll < 0.25f) infos.target.AddAffliction("kidneydamage", 10f + (float)_rng.NextDouble() * 10f, infos.user);
                else if (roll < 0.50f) infos.target.AddAffliction("liverdamage", 10f + (float)_rng.NextDouble() * 10f, infos.user);
                else if (roll < 0.75f) infos.target.AddAffliction("lungdamage", 10f + (float)_rng.NextDouble() * 10f, infos.user);
                else infos.target.AddAffliction("heartdamage", 10f + (float)_rng.NextDouble() * 10f, infos.user);

                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
            else if (isHead)
            {
                infos.target.AddAffliction("neurotrauma", 15f + (float)_rng.NextDouble() * 15f, infos.user);
                infos.target.AddAfflictionLimb("internaldamage", infos.targetLimb.type, 10f + (float)_rng.NextDouble() * 10f, infos.user);
                infos.target.AddAfflictionLimb("bleeding", infos.targetLimb.type, 6f + (float)_rng.NextDouble() * 12f, infos.user);
                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
            else
            {
                // Open arm/leg
                infos.target.AddAfflictionLimb("bleeding", infos.targetLimb.type, 6f + (float)_rng.NextDouble() * 6f, infos.user);
                infos.target.AddAfflictionLimb("lacerations", infos.targetLimb.type, 4f + (float)_rng.NextDouble() * 6f, infos.user);
                infos.target.AddAfflictionLimb("internaldamage", infos.targetLimb.type, 4f + (float)_rng.NextDouble() * 6f, infos.user);

                if (HF.Chance(0.1f)) HF.BreakLimb(infos.target.Human, infos.targetLimb.type);

                HF.GiveItem(infos.target.Human, "ntsfx_slash");
            }
        }

        public static void UseSpeedflexMode(NTItems.ItemUpdateFunctionInfos infos)
        {

            var torsoLimb = infos.target.Human.AnimController?.MainLimb ?? infos.targetLimb;
            // Me when I LIE!!!!
            // Act like its the torso (even when we apply to a limb)
            var torsoInfos = new NTItems.ItemUpdateFunctionInfos(infos.item, infos.user, infos.target, torsoLimb);
            var UsedOnLimb = HF.NormalizeLimbType(infos.targetLimb.type);

            switch (UsedOnLimb)
            {
                case LimbType.Head:
                    loader.Get("organscalpel_brain")?.Invoke(new NTItems.ItemUpdateFunctionInfos(infos.item, infos.user, infos.target, torsoLimb));
                    break;

                case LimbType.LeftArm:
                    loader.Get("organscalpel_kidneys")?.Invoke(new NTItems.ItemUpdateFunctionInfos(infos.item, infos.user, infos.target, torsoLimb));
                    break;

                case LimbType.Torso:
                    loader.Get("organscalpel_liver")?.Invoke(new NTItems.ItemUpdateFunctionInfos(infos.item, infos.user, infos.target, torsoLimb));
                    break;

                case LimbType.RightArm:
                    loader.Get("organscalpel_heart")?.Invoke(new NTItems.ItemUpdateFunctionInfos(infos.item, infos.user, infos.target, torsoLimb));
                    break;

                case LimbType.LeftLeg:
                    loader.Get("organscalpel_lungs")?.Invoke(new NTItems.ItemUpdateFunctionInfos(infos.item, infos.user, infos.target, torsoLimb));
                    break;
            }
        }
    }
}