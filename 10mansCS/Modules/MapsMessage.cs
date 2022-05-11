using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Victoria;
using Newtonsoft.Json;
using System;
using System.IO;

namespace TiimmmeeyyyCS.Services
{
    public class MapsMessage
    {
        private IUserMessage? _message;
	
        public ulong Id
        {
            get
            {
                return _message!.Id;
            }
        }

        public int _currentleader = 0;

	public IGuildUser CurrentLeader
        {
            get
            {
		if (_currentleader == 0)
                {
                    return Program.BotConfig.TeamOne[0];
                }
                else
                {
                    return Program.BotConfig.TeamTwo[0];
                }
	   }
        }

        private DiscordSocketClient _client;

        public MapsMessage(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task InitializeMessage()
        {
            _message = await _client.GetGuild(Program.BotConfig.GuildID).GetTextChannel(Program.BotConfig.SetupChannelID).SendMessageAsync(embed: BuildEmbed(), components: BuildComponents()) as IUserMessage;
        }

        public async Task Delete()
        {
            await _message!.DeleteAsync();
            var config = Program.BotConfig;
            config.readymessage = null;
            Program.BotConfig = config;
        }

        public async Task Update()
        {
            await _message!.ModifyAsync(x => { x.Embed = BuildEmbed(); x.Components = BuildComponents(); });
        }

        public Embed BuildEmbed()
        {
            Embed embed = new EmbedBuilder()
		.WithTitle("Choose the maps")
		.WithDescription($"it's voting time. The leaders will ban the maps one by one until one map is left.  \n\n**{CurrentLeader.DisplayName}'s** Turn!").Build();
	    
            return embed;
        }

        public MessageComponent BuildComponents()
        {
            var builder = new ComponentBuilder()
		.WithButton("Office", "map:cs_office", ButtonStyle.Primary)
		.WithButton("Vertigo", "map:de_vertigo", ButtonStyle.Primary)
        .WithButton("Train", "map:de_train", ButtonStyle.Primary)
        .WithButton("Overpass", "map:de_overpass", ButtonStyle.Primary)
        .WithButton("Nuke", "map:de_nuke", ButtonStyle.Primary)
        .WithButton("Mirage", "map:de_mirage", ButtonStyle.Primary)
        .WithButton("Inferno", "map:de_inferno", ButtonStyle.Primary)
        .WithButton("Cache", "map:de_cache", ButtonStyle.Primary)
        .WithButton("Dust 2", "map:de_dust2", ButtonStyle.Danger);

            return builder.Build();
        }

        public async Task ChangeLeader()
        {
            if (_currentleader == 0)
            {
                _currentleader = 1;
            }
            else
            {
                _currentleader = 0;
            }
        }


        // This class handles the what happens when the buttons are pressed
        public class MapsInteractionModule : InteractionModuleBase<SocketInteractionContext>
        {
            [ComponentInteraction("map:*", true)]
            public async Task Maps(string map_name)
            {
		if (Context.Interaction.User.Id == Program.)
	    }
        }
    }
}
