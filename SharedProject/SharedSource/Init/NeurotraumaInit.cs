using Barotrauma.LuaCs.Events;
using MoonSharp.Interpreter;


namespace Neurotrauma
{
    public partial class NeurotraumaInit : IAssemblyPlugin, IEventCharacterCreated, IEventCharacterDeath
    {
        // ---------------------------         Ydrec Shit         --------------------------- \\

        // These are automatically assigned by the plugin service after the Constructor is called
        public IConfigService ConfigService { get; set; }
        public IEventService EventService { get; set; }
        public IPluginManagementService PluginService { get; set; }
        public ILoggerService LoggerService { get; set; }
        public ILuaScriptManagementService luaScriptManagementService = LuaCsSetup.Instance.LuaScriptManagementService;

        public static NTAfflictions.NTAfflictionsLoader NTAfflLoader = new NTAfflictions.NTAfflictionsLoader(NTInfo.Name);

        public static NTStats.NTStatLoader NTStatsLoader = new NTStats.NTStatLoader(NTInfo.Name);

        public static NTItems.NTItemFunctionLoader NTItemsLoader = new NTItems.NTItemFunctionLoader(NTInfo.Name);

        private Harmony ?harmony;

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

            //TODO: update that idk what it does but it seems to be important for lua scripts to work properly so ill let BEAN (may God strikes him down)s -Cookie

            UserData.RegisterType(typeof(HF));
            UserData.RegisterType(typeof(NTInfo));
            UserData.RegisterType(typeof(NTC));

            UserData.RegisterType(typeof(NTConfig));
            UserData.RegisterType(typeof(NTConfigData));
            UserData.RegisterType(typeof(ConfigExpansion));
            UserData.RegisterType(typeof(ConfigEntry));
            UserData.RegisterType(typeof(ConfigEntryType));

            UserData.RegisterType(typeof(NeurotraumaInit));

            UserData.RegisterType(typeof(NTAfflictions));
            UserData.RegisterType(typeof(NTHuman));
            UserData.RegisterType(typeof(NTStats));
            UserData.RegisterType(typeof(NTItems));

            UserData.RegisterType(typeof(SpeakAboutIssuesPatch));


            UserData.RegisterType(typeof(NTAfflictions.AfflictionPriority));
            UserData.RegisterType(typeof(List<NTAfflictions.AfflictionPriority>));

            UserData.RegisterType(typeof(OnDamaged));

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
            EventService.Subscribe<IEventCharacterCreated>(this);  //subscribe your plugin
            EventService.Subscribe<IEventCharacterDeath>(this);  //subscribe your plugin
        }

        public void RemovePatches()
        {
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

            if (HF.IsMain())
            {
                harmony?.UnpatchSelf();
                LoveBots.Dispose();
                CharacterPatches.Dispose();
            }

            DisposeClient();
        }

        partial void DisposeClient();

        // -------------------------------------- Our IEvent Plugins -------------------------------------- \\

        public void OnCharacterCreated(Character character)
        {
            if (character.IsHuman)
            {
                

                LuaCsSetup.Instance.Timer.Wait((params object[] _) => {
                    var h = new NTHuman(character);

                    if (h.Human.teamID == CharacterTeamType.Team1)
                    {
                        h.AddAffliction("luabotomy", 100f);
                    }

                }, 1000);
            }
        }

        public void OnCharacterDeath(Character character, Affliction causeOfDeathAffliction, CauseOfDeathType causeOfDeathType)
        {
            if (character.IsHuman)
            {
                NTHuman? human = NTHuman.getNTHumanFromCharacter(character);

                if (human == null) return;

                NTHuman.RemoveNTHuman(human);
            }
        }
    }
}
