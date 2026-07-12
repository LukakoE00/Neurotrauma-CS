using Barotrauma.LuaCs.Events;
using MonoGame.Utilities;
using MoonSharp.Interpreter;
using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma
{
    public partial class NeurotraumaInit : IAssemblyPlugin, IEventChangeFallDamage, IEventUpdate, IEventCharacterCreated, IEventCharacterDeath
    {
        // ---------------------------         Ydrec Shit         --------------------------- \\

        // These are automatically assigned by the plugin service after the Constructor is called
        public IConfigService ConfigService { get; set; }
        public IEventService EventService { get; set; }
        public IPluginManagementService PluginService { get; set; }
        public ILoggerService LoggerService { get; set; }
        public ILuaScriptManagementService luaScriptManagementService = LuaCsSetup.Instance.LuaScriptManagementService;
        private Harmony harmony;

        // ---------------------------        Functions        --------------------------- \\
        // Called right after the constructor
        public void PreInitPatching()
        {
        }

        // When your plugin is loading, use this instead of the constructor for code relying on
        // the services above.
        // Put any code here that does not rely on other plugins.

        // No fucking clue what should go here for now tbh. - Lukako
        public void Initialize()
        {
            UserData.RegisterType(typeof(HF));
            UserData.RegisterType(typeof(NTConfig));
            UserData.RegisterType(typeof(NTConfigData));

            if (HF.GameIsMultiplayer())
            {
                #if SERVER
                    HF.Print("Initializing for Multiplayer.");
                    InitializeServer();
                #endif
            }

            if (HF.GameIsSingleplayer())
            {
                // ServersideInit.cs
                HF.Print("Initializing for Singleplayer.");
                InitializeServer();
            }
        }

        public void AddPatches()
        {
            //EventService.Subscribe<IEventChangeFallDamage>(this);  //subscribe your plugin
            //EventService.Subscribe<IEventUpdate>(this);  //subscribe your plugin
            EventService.Subscribe<IEventCharacterCreated>(this);  //subscribe your plugin
            EventService.Subscribe<IEventCharacterDeath>(this);  //subscribe your plugin
        }

        public void RemovePatches()
        {
            //EventService.Unsubscribe<IEventChangeFallDamage>(this); //remove your plugin
            //EventService.Unsubscribe<IEventUpdate>(this);  //remove your plugin
            EventService.Unsubscribe<IEventCharacterCreated>(this);  //remove your plugin
            EventService.Unsubscribe<IEventCharacterDeath>(this);  //remove your plugin
        }

        // After all plugins have loaded
        // Put code that interacts with other plugins here.
        public void OnLoadCompleted()
        {
            // Shared Scripts
            NTConfigData.Register();
            NTConfig.LoadConfig();

            // Serverside code that ALSO runs in Singleplayer
            // Add functions in SharedSource/SharedInit.cs
            if (HF.GameIsMultiplayer())
            {
                #if SERVER
                    HF.Print("OnLoadCompleted for Multiplayer.");
                    OnLoadCompletedServerside();
                    AddPatches();
                #endif
            }

            if (HF.GameIsSingleplayer())
            {
                // ServersideInit.cs
                HF.Print("OnLoadCompleted for Singleplayer.");
                OnLoadCompletedServerside();
                AddPatches();
            }

            // Clientside code
            // Add functions in ClientSource/ClientInit.cs
            #if CLIENT
                InitClientOnly();
            #endif

            // Serverside code that ONLY runs in Multiplayer
            // Add functions in ServerSource/ServerInit.cs
            #if SERVER
                InitServerOnly();
#endif
        }

        public void Dispose()
        {
            RemovePatches();

            #if SERVER
                DisposeServer();
#endif
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }


            this.LoggerService = null;
            this.ConfigService = null;
            this.EventService = null;
            this.PluginService = null;
            this.luaScriptManagementService = null;
        }

        // -------------------------------------- Our IEvent Plugins -------------------------------------- \\

        public float? OnChangeFallDamage(float impactDamage, Character character, Vector2 impactPos, Vector2 velocity)
        {
            NTFallDamage.OnChangeFallDamage(impactDamage, character, impactPos, velocity);
            return 0;
        }

        public void OnUpdate(double deltaTime) // Unused
        {
            HumanUpdate.ThinkUpdate();
        }

        public void OnCharacterCreated(Character character)
        {
            HumanUpdate.AddCharacterToUpdate(character);
        }

        public void OnCharacterDeath(Character character, Affliction causeOfDeathAffliction, CauseOfDeathType causeOfDeathType)
        {
            HumanUpdate.RemoveCharacterFromUpdate(character);
        }
    }

    // Stores our random shit.
    public static class NT
    {
        public static double DeltaTime = 2;
    }
}
