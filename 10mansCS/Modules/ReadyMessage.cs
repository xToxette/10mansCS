using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Victoria;
using Newtonsoft.Json;
using System;
using System.IO;

namespace TiimmmeeyyyCS.Services
{
    public class ReadyMessage
    {
        private IUserMessage? _ReadyMessage;
        public ulong Id
        {
            get
            {
                return _ReadyMessage.Id;
            }
        }
        private DiscordSocketClient _client;

        public ReadyMessage(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task InitializeMessage()
        {
            _ReadyMessage = await _client.GetGuild(Program.BotConfig.GuildID).GetTextChannel(Program.BotConfig.SetupChannelID).SendMessageAsync(embed: BuildReadyEmbed(), components: BuildReadyComponent()) as IUserMessage;
        }

        public async Task Delete()
        {
            await _ReadyMessage!.DeleteAsync();
            var config = Program.BotConfig;
            config.readymessage = null;
            Program.BotConfig = config;

        }

        public async Task Update()
        {
            await _ReadyMessage.ModifyAsync(x => x.Embed = BuildReadyEmbed());
        }

        public Embed BuildReadyEmbed()
        {
            string Ready = "";
            string Unready = "";
            foreach (var player in Program.BotConfig.QueuedUsers)
            {
                if (player.Item2 == true)
                {
                    Ready += $"{player.Item1.Username}\n";
                }
                else
                {
                    Unready += $"{player.Item1.Username}\n";
                }
            }

            Embed embed = new EmbedBuilder()
		.WithTitle("Ready to start")
		.WithDescription("There are 10 people. It's time to ready up!")
		.AddField("**Unready**", Unready == "" ? "_Everyone is ready_" : Unready, true)
		.AddField("**Ready**", Ready == "" ? "_No players are ready_" : Ready, true).Build();

            return embed;
        }

        public MessageComponent BuildReadyComponent()
        {
            var builder = new ComponentBuilder()
		.WithButton("Ready", "ready", ButtonStyle.Success)
		.WithButton("Unready", "unready", ButtonStyle.Danger);
	    
            return builder.Build();
        }


        // This class handles the what happens when the buttons are pressed
        public class ReadyMessageInteraction : InteractionModuleBase<SocketInteractionContext>
        {
            [ComponentInteraction("ready", true)]
            public async Task ReadyHandler()
            {
		
                for (int n = 0; n < Program.BotConfig.QueuedUsers.Count; n++)
                {
                    if (Program.BotConfig.QueuedUsers[n].Item1.Id == Context.Interaction.User.Id)
                    {
                        if (Program.BotConfig.QueuedUsers[n].Item2 == false)
                        {
                            var config1 = Program.BotConfig;
                            config1.QueuedUsers[n] = Tuple.Create(config1.QueuedUsers[n].Item1, true);
                            Program.BotConfig = config1;
                            await Program.BotConfig.readymessage.Update();
                        }
                    }
                }
                await RespondNothing(Context.Interaction);

                // Returns if any player is not ready yet
                foreach (var queuedplayer in Program.BotConfig.QueuedUsers)
                {
                    if (queuedplayer.Item2 == false)
                    {
                        return;
                    }
                }
		
                // Select two leaders and place them in TeamOne and TeamTwo lists
                var config2 = Program.BotConfig;
                foreach (var queuedplayer in config2.QueuedUsers)
                {
                    config2.Players.Add(queuedplayer.Item1);
                }

                config2.TeamOne.Add(config2.Players[0]);
                config2.Players.RemoveAt(0);

                config2.TeamTwo.Add(config2.Players[0]);
                config2.Players.RemoveAt(0);

                config2.teambuildingmessage = new TeamBuildingMessage(Context.Client);
                Program.BotConfig = config2;
                await Program.BotConfig.readymessage!.Delete();
            }

            [ComponentInteraction("unready", true)]
            public async Task UnreadyHandler()
            {
                for (int n = 0; n < Program.BotConfig.QueuedUsers.Count; n++)
                {
                    if (Program.BotConfig.QueuedUsers[n].Item1.Id == Context.Interaction.User.Id)
                    {
                        if (Program.BotConfig.QueuedUsers[n].Item2 == true)
                        {
                            var config = Program.BotConfig;
                            config.QueuedUsers[n] = Tuple.Create(config.QueuedUsers[n].Item1, false);
                            Program.BotConfig = config;
                            await Program.BotConfig.readymessage.Update();
                        }
                    }
                }
                await RespondNothing(Context.Interaction);
            }

            private async Task RespondNothing(SocketInteraction interaction)
            {
                try
                {
                    await interaction.RespondAsync("");
                }
                catch { }
            }
        }
    }
}
