using Discord;
using Newtonsoft.Json;
using System;
using System.IO;

namespace TiimmmeeyyyCS.Services
{
    public class SteamIDs
    {
        private Dictionary<ulong, string> _idslist;

        public Dictionary<ulong, string> IDsList
        {
            get
            {
                return _idslist;
            }
        }

	/// <summary>
	/// Name of the config file
	/// </summary>
	public const string FileName = "SteamIDs.json";
	
	public SteamIDs()
	{
            if (File.Exists(FileName))
            {
		string json = File.ReadAllText(FileName);
                _idslist = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(json)!;
            }
	    else
	    {
                File.Create(FileName);
                _idslist = new Dictionary<ulong, string>();
            }
        }

	/// <summary>
	/// Saves a SteamID linked to a DiscordID
	/// </summary>
	public bool Add(ulong DiscordID, string SteamID)
	{
	    if (_idslist.ContainsKey(DiscordID))
	    {
                _idslist[DiscordID] = SteamID;
            }
            else
            {
                try
                {
                    _idslist.Add(DiscordID, SteamID);
                }
                catch (Exception err)
                {
                    Console.WriteLine($"ERROR:\tWhile trying to add SteamID to dictionary: {err.Message}");
                    return false;
                }
            }
	    
            Save();
            return true;
        }

	/// <summary>
	/// Saves
	public void Save()
	{
            File.WriteAllText(FileName, JsonConvert.SerializeObject(_idslist, Formatting.Indented));
        }
    }
}
