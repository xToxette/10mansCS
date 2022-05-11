using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Victoria;
using Newtonsoft.Json;
using System;
using System.IO;

namespace TiimmmeeyyyCS.Services
{
    public class TeamBuildingMessage
    {
        private IUserMessage? _TeamBuildingMessage;
	
        public List<Tuple<IGuildUser, bool>> PlayersCopy = new List<Tuple<IGuildUser, bool>>();

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

        public ulong Id
        {
            get
            {
                return _TeamBuildingMessage.Id;
            }
        }
	
        private DiscordSocketClient _client;

        public TeamBuildingMessage(DiscordSocketClient client)
        {
            _client = client;
            foreach (var player in Program.BotConfig.Players)
            {
                PlayersCopy.Add(Tuple.Create(player, false));
            }
        }

        public async Task InitializeMessage()
        {
            _TeamBuildingMessage = await _client.GetGuild(Program.BotConfig.GuildID).GetTextChannel(Program.BotConfig.SetupChannelID).SendMessageAsync(embed: BuildEmbed(), components: BuildComponents()) as IUserMessage;
        }

        public async Task Delete()
        {
            await _TeamBuildingMessage!.DeleteAsync();
            var config = Program.BotConfig;
            config.teambuildingmessage = null;
            Program.BotConfig = config;
        }

        public async Task Update()
        {
            await _TeamBuildingMessage.ModifyAsync(x => { x.Embed = BuildEmbed(); x.Components = BuildComponents(); });
        }


        private Embed BuildEmbed()
        {
            string TeamOneValue = $"⠀⠀⠀⠀⠀⠀⠀**Team 1**```{StringifyUsernames.TeamOneLeader(Program.BotConfig.TeamOne[0].DisplayName)}```";
            string TeamTwoValue = $"⠀⠀⠀⠀⠀⠀⠀**Team 2**```{StringifyUsernames.TeamTwoLeader(Program.BotConfig.TeamTwo[0].DisplayName)}```";
            for (int p = 1; p < 5; p++)
	    {
                if (p < Program.BotConfig.TeamOne.Count)
		{
		    TeamOneValue += $"```{StringifyUsernames.TeamOneMember(Program.BotConfig.TeamOne[p].DisplayName)}```";
                }
		else
		{
                    TeamOneValue += $"```{StringifyUsernames.TeamOneMember("")}```";
                }

		if (p < Program.BotConfig.TeamTwo.Count)
		{
                    TeamTwoValue += $"```{StringifyUsernames.TeamTwoMember(Program.BotConfig.TeamTwo[p].DisplayName)}```";
                }
		else
		{
                    TeamTwoValue += $"```{StringifyUsernames.TeamTwoMember("")}```";
                }
	    }

                Embed embed = new EmbedBuilder()
		    .WithTitle("⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀Choosing the teams⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀")
		    .AddField("⠀", TeamOneValue, true)           // TeamOne
		    .AddField("⠀", "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀", true) // This field is just for formatting
		    .AddField("⠀", TeamTwoValue, true)
		    .AddField("⠀", $"**{CurrentLeader.DisplayName}'s** Turn!").Build();

            return embed;
        }


        private MessageComponent BuildComponents()
        {
            var builder = new ComponentBuilder()
		.WithButton($"{PlayersCopy[0].Item1.DisplayName}", "player:0", ButtonStyle.Primary, disabled: PlayersCopy[0].Item2)
		.WithButton($"{PlayersCopy[1].Item1.DisplayName}", "player:1", ButtonStyle.Primary, disabled: PlayersCopy[1].Item2)
		.WithButton($"{PlayersCopy[2].Item1.DisplayName}", "player:2", ButtonStyle.Primary, disabled: PlayersCopy[2].Item2)
		.WithButton($"{PlayersCopy[3].Item1.DisplayName}", "player:3", ButtonStyle.Primary, disabled: PlayersCopy[3].Item2)
		.WithButton($"{PlayersCopy[4].Item1.DisplayName}", "player:4", ButtonStyle.Primary, disabled: PlayersCopy[4].Item2)
		.WithButton($"{PlayersCopy[5].Item1.DisplayName}", "player:5", ButtonStyle.Primary, disabled: PlayersCopy[5].Item2)
		.WithButton($"{PlayersCopy[6].Item1.DisplayName}", "player:6", ButtonStyle.Primary, disabled: PlayersCopy[6].Item2)
		.WithButton($"{PlayersCopy[7].Item1.DisplayName}", "player:7", ButtonStyle.Primary, disabled: PlayersCopy[7].Item2);

            return builder.Build();
        }

        // Just a simple function that is called when the current leader
        // has to be changed
        private async Task ChangeLeader()
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
        public class TeamBuildingMessageInteraction : InteractionModuleBase<SocketInteractionContext>
        {
            [ComponentInteraction("player:*", true)]
            public async Task PlayerButtonHandlder(string index)
            {
                int Index = Convert.ToUInt16(index);
		
                if (Context.Interaction.User.Id == Program.BotConfig.teambuildingmessage.CurrentLeader.Id)
                {
                    Console.WriteLine("User that pressed was the current leader");
                    var config = Program.BotConfig;
                    if (Program.BotConfig.teambuildingmessage._currentleader == 0)
                    {
                        Console.WriteLine("Leader is of team one so adding to team one");
                        config.TeamOne.Add(config.teambuildingmessage.PlayersCopy[Index].Item1);
                        config.Players.Remove(config.teambuildingmessage.PlayersCopy[Index].Item1);
                        config.teambuildingmessage.PlayersCopy[Index] = Tuple.Create(config.teambuildingmessage.PlayersCopy[Index].Item1, true);
                        Console.WriteLine("Done adding person to team one");
                    }
		    else
		    {
                        config.TeamTwo.Add(config.teambuildingmessage.PlayersCopy[Index].Item1);
                        config.Players.Remove(config.teambuildingmessage.PlayersCopy[Index].Item1);
                        config.teambuildingmessage.PlayersCopy[Index] = Tuple.Create(config.teambuildingmessage.PlayersCopy[Index].Item1, true);
                    }
                    Program.BotConfig = config;
                    await Program.BotConfig.teambuildingmessage.ChangeLeader();
                    await Program.BotConfig.teambuildingmessage.Update();
                }
		
                await RespondNothing(Context.Interaction);

		if (Program.BotConfig.TeamOne.Count + Program.BotConfig.TeamTwo.Count >= 10)
		{
		    
		}
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

    static class StringifyUsernames
    {
	private static string Whitespace(int n)
	{
            string result = "";
            for (int i = 0; i < n; i++)
	    {
                result += "⠀";
            }
            return result;
        }

	
        public static string TeamOneLeader(string name)
	{
	    switch (name.Length)
            {
		case <= 6:
                    return "👑⠀" + name + Whitespace(15 - name.Length);
                case 7:
		    return "👑⠀" + name + Whitespace(9);
		case <= 13:
		    return "👑⠀" + name + Whitespace(16 - name.Length);
		case 14:
		    return "👑⠀" + name + Whitespace(4);
		case <= 18:
		    return "👑⠀" + name + Whitespace(17 - name.Length);
		default:
		    return "👑⠀" + name.Substring(0, 16) + "..";
            }
        }

	public static string TeamOneMember(string name)
	{
            if (name == "")
	    {
                return "- ";
            }
	    
	    switch (name.Length)
	    {
		case < 20:
		    return "- " + name;
		default:
                    return "- " + name.Substring(0, 17) + "..";
            }
	}

	public static string TeamTwoLeader(string name)
	{
	    switch (name.Length)
	    {
		case <= 2:
		    return Whitespace(15 - name.Length) + name + "⠀👑";
		case 3:
		    return Whitespace(13) + name + "⠀👑";
		case <= 8:
		    return Whitespace(16 - name.Length) + name + "⠀👑";
		case 9:
		    return Whitespace(8) + name + "⠀👑";
		case <= 15:
		    return Whitespace(17 - name.Length) + name + "⠀👑";
		case 16:
		    return Whitespace(2) + name + "⠀👑";
		case <= 19:
		    return Whitespace(18 - name.Length) + name + "⠀👑";
		default:
		    return name.Substring(0, 17) + ".." + "⠀👑";
	    }
	}

	public static string TeamTwoMember(string name)
	{
	    if (name == "")
	    {
                return "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀-";
            }
	    switch (name.Length)
	    {
		case 1:
		    return Whitespace(16) + name + "⠀-";
		case <= 7:
		    return Whitespace(17 - name.Length) + name + "⠀-";
		case 8:
		    return Whitespace(11) + name + "⠀-";
		case <= 13:
		    return Whitespace(18 - name.Length) + name + "⠀-";
		case 14:
		    return Whitespace(14) + name + "⠀-";
		case <= 20:
		    return Whitespace(19 - name.Length) + name + "⠀-";
		default:
		    return name.Substring(0, 18) + ".." + "⠀-";
	    }
        }
    }
    
}
