using Discord;
using Newtonsoft.Json;
using System;
using System.IO;

namespace TiimmmeeyyyCS.Services
{
    public class ConfigHelper
    {
        public struct Config
        {
            public ulong GuildID { get; set; }
            public ulong LinkingChannelID { get; set; }
            public ulong SetupChannelID { get; set; }
            public ulong QueuingChannelID { get; set; }
            public string Token { get; set; }
            public string Prefix { get; set; }

            [JsonIgnore]
            public ReadyMessage? readymessage { get; set; }

	    [JsonIgnore]
	    public TeamBuildingMessage? teambuildingmessage { get; set; }

	    [JsonIgnore]
	    public MapsMessage? mapsmessage { get; set; }

            [JsonIgnore]
            public List<Tuple<IGuildUser, bool>> QueuedUsers { get; set; }

            [JsonIgnore]
            public List<IGuildUser> SpectatingUsers { get; set; }

	    [JsonIgnore]
	    public List<IGuildUser> Players { get; set; }

            [JsonIgnore]
            public List<IGuildUser> TeamOne { get; set; }

	    [JsonIgnore]
	    public List<IGuildUser> TeamTwo { get; set; }

	    public void Clear()
	    {
                Players.Clear();
                TeamOne.Clear();
                TeamTwo.Clear();
                readymessage = null;
                teambuildingmessage = null;
                mapsmessage = null;
            }
        }

        /// <summary>
        /// Name of the config file
        /// </summary>
        public const string ConfigName = "appConfig.json";


        /// <summary> 
        /// Loads the config file
        /// </summary>
        /// <returns>Config struct with the values of the config file</returns>
        public static Config LoadConfigFile()
        { 
            string json = File.ReadAllText(ConfigName);
            var config = JsonConvert.DeserializeObject<Config>(json);
            config.SpectatingUsers = new List<IGuildUser>();
            config.QueuedUsers = new List<Tuple<IGuildUser, bool>>();
            config.Players = new List<IGuildUser>();
            config.TeamOne = new List<IGuildUser>();
	    config.TeamTwo = new List<IGuildUser>();
            return config;
        }

        /// <summary>
        /// Creates config File
        /// </summary>
        public static void CreateConfigFile()
        {
            string token;
            do
            {
                Console.Write("Enter bot token: ");
                token = Console.ReadLine().Trim();
                Console.WriteLine();
            } 
            while (string.IsNullOrWhiteSpace(token));

            Console.Write("Enter a prefix: ");
            string prefix = Console.ReadLine().Trim();
            Console.Write("Enter the guild id: ");
            string guildid = Console.ReadLine().Trim();
            Console.Write("Enter the linking channel id: ");
            string linkingchannelid = Console.ReadLine().Trim();
            Console.Write("Enter the setup channel id: ");
            string setupchannelid = Console.ReadLine().Trim();
            Console.Write("Enter the queuing channel id: ");
            string queuingchannelid = Console.ReadLine().Trim();



            Config config = new Config
            {
                Prefix = string.IsNullOrWhiteSpace(prefix) ? "#" : prefix,
                Token = token,
                GuildID = Convert.ToUInt64(guildid),
                LinkingChannelID = Convert.ToUInt64(linkingchannelid),
                SetupChannelID = Convert.ToUInt64(setupchannelid),
		QueuingChannelID = Convert.ToUInt64(queuingchannelid),
	    };

            File.WriteAllText(ConfigName, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        /// <summary>
        /// Updates the config file
        /// </summary>
        /// <param name="config">Config struct to be used to update</param>
        public static void UpdateConfigFile(Config config){
            File.WriteAllText(ConfigName, JsonConvert.SerializeObject(config, Formatting.Indented));
        }
    }
}
