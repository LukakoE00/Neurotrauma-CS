
using Barotrauma;
using Barotrauma.LuaCs.Events;
using FarseerPhysics.Common;
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
        static public void  Update(IEnumerable<HumanUpdate.NTHuman> CharacterList, List<AfflictionPriority> Priorities)
        {
            SyncCharacters(CharacterList);
            SyncAfflictions(CharacterList,Priorities);
        }

        // Empty Methods, exist purely so we can hook Lua functions.

        static public void SyncAfflictions(IEnumerable<HumanUpdate.NTHuman> CharacterList, List<AfflictionPriority> Priorities)
        {
            NTLua.Call("SyncAfflictions", NT.DeltaTime, CharacterList, Priorities);
        }

        static public void SyncCharacters(IEnumerable<HumanUpdate.NTHuman> CharacterList)
        {
            NTLua.Call("SyncCharacters", CharacterList);
        }

        static public void SyncCharacterSpeed(Character Human, double Speed)
        {
            NTLua.Call("SyncCharacterSpeed", Human, Speed);
        }

        static public void SyncPreHumanUpdateHooks(Character Character)
        {
            NTLua.Call("SyncPreHumanUpdateHooks", Character);
        }
    }
}