namespace Neurotrauma
{
    public static class NTStats
    {
        public static Dictionary<string, NTStat> Stats = new Dictionary<string, NTStat>();

        public static void RegisterStat(string id, NTStat NewStat) // Register a new stat to the NTStat Dictionary.
        {
            if (!Stats.ContainsKey(id))
            {
                Stats.Add(id, NewStat);
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} already exists! Multiple addons might be trying to register the same stat.\n" +
                    $"If you want to recalculate a stat, use CharacterStats.RecalculateSingle instead of registering it again.");
            }
        }

        public static void OverrideStat(string id, NTStat NewStat) // Override a stat in NTStat Dictionary.
        {
            if (Stats.ContainsKey(id))
            {
                Stats[id] = NewStat;
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} does not exist! You can't override a stat that doesn't exist.\n" +
                    $"If you want to register a new stat, use RegisterStat instead of trying to override it.");
            }
        }

        public static void RemoveStat(string id) // Remove a stat to the NTStat Dictionary.
        {
            if (!Stats.ContainsKey(id))
            {
                Stats.Remove(id);
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} does not exist! You can't remove a stat that doesn't exist.");
            }
        }

    }

    public class NTStat(double MinStrength = 0, double MaxStrength = 1, double DefaultStrength = 1, Action<HumanUpdate.NTHuman> Update = null)
    {
        private double Amount = Math.Clamp(DefaultStrength, MinStrength, MaxStrength);
        private Action<HumanUpdate.NTHuman> StatUpdate = Update;

        public void AddStrength(double AddingAmount)
        {
            Amount += AddingAmount;
        }

        public void RemoveStrength(double RemovingAmount) // This function is kinda stupid.
        {
            AddStrength(-RemovingAmount);
        }

        public void SetStrength(double SetingAmount)
        {
            Amount = SetingAmount;
        }

        public void Recalculate(HumanUpdate.NTHuman Character)
        {
            if (StatUpdate != null)
            {
                StatUpdate(Character);
            }
        }
    }
}