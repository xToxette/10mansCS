using Discord;
using Discord.Net;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TiimmmeeyyyCS.Services
{
    public class DiscordEventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public DiscordEventHandler(DiscordSocketClient client, InteractionService interaction, IServiceProvider services)
        {
            _client = client;
            _commands = interaction;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // process the InteractionCreated payloads to execute Interactions commands
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _client.MessageDeleted += MessageDeleted;
        }
	

	private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
	{
            // Adding user to QueuedUsers or SpectatingUsers if they joined the queuing channel
            if (after.VoiceChannel != null && (after.VoiceChannel.Id == Program.BotConfig.QueuingChannelID))
	    {
		var config = Program.BotConfig;
                var guilduser = user as IGuildUser;
		
                if (Program.BotConfig.QueuedUsers.Count < 10)
		{
		    Console.WriteLine("Adding user to queued users");
                    config.QueuedUsers.Add(Tuple.Create(guilduser!, false));
		    Console.WriteLine($"Amount of queuing users in now: {config.QueuedUsers.Count}");
                }
		else
		{
                    config.SpectatingUsers.Add(guilduser!);
                }
		
                Program.BotConfig = config;
            }

            // Remove the user from QueuedUsers/SpectatingUsers list, and if as a
            // result a spot is free in QueuedUsers, fill it up with a spectator if
            // possible
            if (before.VoiceChannel != null && (before.VoiceChannel.Id == Program.BotConfig.QueuingChannelID))
	    {
                var config = Program.BotConfig;
                var guilduser = user as IGuildUser;

                Console.WriteLine("User left queuing channel");
                for (int n = 0; n < config.QueuedUsers.Count; n++)
		{
		    if (config.QueuedUsers[n].Item1.Id == guilduser.Id)
		    {
                        Console.WriteLine("Removing user from QueuedUsers");
                        config.QueuedUsers.RemoveAt(n);
			if (config.SpectatingUsers.Count >0)
			{
                            config.QueuedUsers.Add(Tuple.Create(config.SpectatingUsers[0], false));
                            config.SpectatingUsers.RemoveAt(0);
                        }
                    }
                }
                if (config.SpectatingUsers.Contains(guilduser!))
		{
		    config.SpectatingUsers.Remove(guilduser!);
		}

                Program.BotConfig = config;
            }


            // Now the starting message will be sent that requires everyone to ready up
            if (Program.BotConfig.QueuedUsers.Count == 10)
            {
		
                if (Program.BotConfig.readymessage == null)
                {
                    var config = Program.BotConfig;
                    config.readymessage = new ReadyMessage(_client);
                    await config.readymessage.InitializeMessage();
                    Program.BotConfig = config;

                }
                else
                {
                    await Program.BotConfig.readymessage!.Update();
                }
            }
            else
            {
                if (Program.BotConfig.readymessage != null)
                {
                    await Program.BotConfig.readymessage.Delete();
                }
            }
        }

	private async Task MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
	{
	    if (Program.BotConfig.readymessage != null)
	    {
		if (Program.BotConfig.readymessage.Id == message.Id)
		{
                    var config = Program.BotConfig;
		    config.readymessage = null;
                    Program.BotConfig = config;
                }
	    }
	}
    }
}
