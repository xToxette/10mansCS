using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using SpotifyAPI.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace TiimmmeeyyyCS.Services
{
    [Group("10mans", "The 10mans command group")]
    public class TENmans : InteractionModuleBase<SocketInteractionContext>
    {
	private InteractionService Commands { get; set; }

	private async Task RespondNothing(SocketInteraction interaction){
            try
	    {
                await interaction.RespondAsync("");
            }
            catch {}
        }

	[RequireOwner()]
	[SlashCommand("debug", "Starts a test run for the bot with fake players")]
	public async Task Debug()
	{
            Console.WriteLine("Starting a test run for the bot");

	    // First clearing all the lists and messages
            var config = Program.BotConfig;

            config.Clear();

            config.TeamOne.Add(config.QueuedUsers[0].Item1);
            config.TeamTwo.Add(config.QueuedUsers[1].Item1);

            for (int i = 0; i < 4; i++)
	    {
                config.Players.Add(config.QueuedUsers[0].Item1);
                config.Players.Add(config.QueuedUsers[1].Item1);
            }

            config.teambuildingmessage = new TeamBuildingMessage(Context.Client);
            await config.teambuildingmessage.InitializeMessage();

            Program.BotConfig = config;
        }

	[SlashCommand("link", "Links SteamID to discord account")]
	public async Task Linking([Summary(description: "Enter your Steam ID, an example: STEAM_1:1:18592381")] string SteamID)
	{
	    if (SteamID.Substring(0, 7) != "STEAM_1")
	    {
                await Context.Interaction.RespondAsync("Make sure the Steam ID starts with 'STEAM_1'. If it says 'STEAM_0' just change the 0 to a 1.", ephemeral: true);
                return;
            }

            Program.steamIds.Add(Context.Interaction.User.Id, SteamID);
            await RespondNothing(Context.Interaction);
        }

	[SlashCommand("account", "Shows the Steam ID currently linked to you account")]
	public async Task Account()
	{
	    if (Program.steamIds.IDsList.ContainsKey(Context.Interaction.User.Id))
	    {
                await Context.Interaction.RespondAsync($"Your account is currently linked to Steam ID: {Program.steamIds.IDsList[Context.Interaction.User.Id]}", ephemeral: true);
            }
	    else
	    {
                await Context.Interaction.RespondAsync($"You haven't linked your account to any Steam ID yet..", ephemeral: true);
            }
        }
    }
}
