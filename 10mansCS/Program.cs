using System;
using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using TiimmmeeyyyCS.Services;
using System.Threading;

namespace TiimmmeeyyyCS
{
    class Program
    {
        // setup our fields we assign later
        public static ConfigHelper.Config BotConfig { get; set; }
	public static SteamIDs steamIds { get; set; }
        private DiscordSocketClient client;
        private InteractionService interaction;

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            if (!File.Exists(ConfigHelper.ConfigName))
            {
                ConfigHelper.CreateConfigFile();
            }

            BotConfig = ConfigHelper.LoadConfigFile();
            steamIds = new SteamIDs();

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildVoiceStates
            });

            var services = ConfigureServices();
            interaction = services.GetRequiredService<InteractionService>();
            await client.LoginAsync(TokenType.Bot, BotConfig.Token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandler>().InitializeAsync();
	    await services.GetRequiredService<DiscordEventHandler>().InitializeAsync();
            client.Ready += ReadyAsync;
            client.GuildUnavailable += OnGuildAvailable;

            Console.CancelKeyPress += (s, e) =>
            {
                ConfigHelper.UpdateConfigFile(BotConfig);
            };

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                ConfigHelper.UpdateConfigFile(BotConfig);
            };

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            if (IsDebug())
            {
                // this is where you put the id of the test discord guild
                System.Console.WriteLine($"In debug mode, adding commands to {BotConfig.GuildID}...");
                await interaction.RegisterCommandsToGuildAsync(BotConfig.GuildID);
            }
            else
            {
                // this method will add commands globally, but can take around an hour
                await interaction.RegisterCommandsGloballyAsync(true);
            }
            Console.WriteLine($"Connected as -> [{client.CurrentUser}] :)");
	    await client.SetGameAsync("music", type: ActivityType.Listening);
        }

	
	// Is executed when the bot had logged in and is fully ready to go.
        private async Task OnGuildAvailable(SocketGuild guild)
        {
	    // Probably want to put all the members that are in the currently connected in the matchmaking
	    // voicechat 
        }


        // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
        public IServiceProvider ConfigureServices()
        {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using csharpi.Services;
            return new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
		.AddSingleton<DiscordEventHandler>()
                //.AddSingleton<EmbedHelper>()
                .BuildServiceProvider();
        } 
            
        static bool IsDebug ( )
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }
    }
}
