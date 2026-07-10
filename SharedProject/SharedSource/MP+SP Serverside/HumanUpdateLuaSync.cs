
using Barotrauma;
using Barotrauma.LuaCs.Events;
using FarseerPhysics.Dynamics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using static Barotrauma.DebugConsole;
using static Neurotrauma.HF;
using static Neurotrauma.NTC;

namespace Neurotrauma
{
    /// <summary>
    /// Our post Human Update Lua syncing system.
    /// Needed since most addons are written in Lua and require the old Lua Human Update.
    /// </summary>
    public static class HumanUpdateLuaSync
    {

        /// <summary>
        /// Syncs our C# human update with our Lua human update, syncs our C# characters with our lua characters
        /// </summary>
        static public void PreSync(IEnumerable<HumanUpdate.NTHuman> CharacterList)
        {
            SyncLuaCharacters(CharacterList);
            SyncLuaAfflictions(CharacterList);
        }

        static public void SyncLuaAfflictions(IEnumerable<HumanUpdate.NTHuman> CharacterList)
        {
        }

        static public void SyncLuaCharacters(IEnumerable<HumanUpdate.NTHuman> CharacterList)
        {
        }

    }
}