// Disable the Hook warning
#pragma warning disable CS0618
namespace Neurotrauma
{
    /// <summary>
    /// Our custom way to talk with our Lua scripts.
    /// </summary>
    public static class NTLua
    {
        private static Dictionary<string,LuaCsAction> LuaMethods = new Dictionary<string,LuaCsAction>();

        public static void Add(string Name, LuaCsAction Method)
        {
            if (!LuaMethods.ContainsKey(Name))
            {
                LuaMethods[Name] = Method;
            }
            else
            {
                HF.PrintError("Attempted to add a new NTLua method named '" +Name+ "' but it already has been added!");
            }
        }

        public static void Remove(string Name)
        {
            if (LuaMethods.ContainsKey(Name))
            {
                LuaMethods.Remove(Name);
            }
            else
            {
                HF.PrintError("Attempted to remove a NTLua method named '" + Name + "' but it doesn't exist!");
            }
        }

        public static void Call(string Name, params object[] _)
        {
            if (LuaMethods.ContainsKey(Name))
            {
                LuaMethods[Name].Invoke(_);
            }
            else
            {
                HF.PrintError("Attempted to call a NTLua method named '" + Name + "' but it doesn't exist!");
            }
        }

        public static void Override(string Name, LuaCsAction NewMethod)
        {
            if (LuaMethods.ContainsKey(Name))
            {
                Remove(Name);
                Add(Name, NewMethod);
            }
            else
            {
                HF.PrintError("Attempted to override a NTLua method named '" + Name + "' but it doesn't exist!");
            }
        }

    }
}