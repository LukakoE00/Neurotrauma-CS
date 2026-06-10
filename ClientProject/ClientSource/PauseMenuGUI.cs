using Barotrauma;
using Barotrauma.LuaCs.Compatibility;

namespace Neurotrauma
{
    internal class ConfigurationMenu
    {
        // This is just what I did in Lua, but now via C#. - Lukako
        // We hook opening the Pause Menu, then force it to also render our Button to open the settings.
        public static void AddConfigToPauseMenu()
        {
            LuaCsSetup.Instance.Hook.Patch(
                "AddButtonToPauseMenu", // identifier for our Patch
                "Barotrauma.GUI",       // className
                "TogglePauseMenu",      // methodName
                Array.Empty<string>(),  // parameterTypes (this one has none)

                new LuaCsPatchFunc((object instance, LuaPatcherService.ParameterTable ptable) =>
                {
                    if (!GUI.PauseMenuOpen) return null;
                    
                    GUIComponent frame = GUI.PauseMenu; // The actual menu screen
                    GUIComponent secondChild = frame.Children.Skip(1).First(); // The panel containing the elements
                    GUIComponent list = secondChild.Children.First(); // The buttons!

                    var btn = new GUIButton(
                        new RectTransform(new Vector2(1f, 0.1f), list.RectTransform),
                        TextManager.Get("ntgui_pausemenubutton_name"),
                        textAlignment: Alignment.Center,
                        style: "GUIButtonSmall");

                    btn.OnClicked = (_, _) =>
                    {
                        HF.Print("Button clicked!");
                        return true;
                    };

                    return null;
                }),
                ILuaCsHook.HookMethodType.After
            );
        }
    }
}