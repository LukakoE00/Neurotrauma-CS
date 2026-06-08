namespace Neurotrauma
{


    // NTStat is just a way to store data on a character.
    public static class NTStat
    {
        public static Dictionary<string, Func<NTUpdateFunctionInfos, int>> Stats = new Dictionary<string, Func<NTUpdateFunctionInfos, int>>();

        private static Dictionary<Character, CharacterStats> CharacterStats = new Dictionary<Character, CharacterStats>();

        // Call this to get the stats of a character
        public static CharacterStats? GetCharacterStats(Character character)
        {
            if (CharacterStats.ContainsKey(character))
            {
                return CharacterStats[character];
            }
            return null;
        }

        public static void RegisterStat(string id, Func<NTUpdateFunctionInfos, int> updateFunction)
        {
            if (!Stats.ContainsKey(id))
            {
                Stats.Add(id, updateFunction);
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} already exists! Multiple addons might be trying to register the same stat.\n" +
                    $"If you want to recalculate a stat, use CharacterStats.RecalculateSingle instead of registering it again.");
            }
        }

        public static void OverrideStat(string id, Func<NTUpdateFunctionInfos, int> updateFunction)
        {
            if (Stats.ContainsKey(id))
            {
                Stats[id] = updateFunction;
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} does not exist! You can't override a stat that doesn't exist.\n" +
                    $"If you want to register a new stat, use RegisterStat instead of trying to override it.");
            }
        }
    }

    public class CharacterStats
    {
        private Dictionary<string, int> Stats { get; }

        public CharacterStats()
        {
            Stats = new Dictionary<string, int>();
        }

        // If you want to recalculate a single stat
        public void RecalculateSingle(string id, NTUpdateFunctionInfos character)
        {

            if (NTStat.Stats[id] != null)
            {
                Stats[id] = NTStat.Stats[id].Invoke(character);
            }
        }

        // If we need to recalculate every stats for a character we can call this
        public void RecalculateAll(NTUpdateFunctionInfos character)
        {
            foreach (var stat in NTStat.Stats)
            {
                Stats[stat.Key] = stat.Value.Invoke(character);
            }
        }
    }

    // I have no idea wtf am doing
}