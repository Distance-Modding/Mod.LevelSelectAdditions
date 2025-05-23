using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Distance.LevelSelectAdditions.Events;
using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Helpers;
using Distance.LevelSelectAdditions.Scripts;
using Distance.LevelSelectAdditions.Scripts.Menus;
using Distance.LevelSelectAdditions.Sorting;
using JsonFx.Json;
using JsonFx.Serialization;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.LevelSelectAdditions
{
	/// <summary>
	/// The mod's main class containing its entry point
	/// </summary>
	[BepInPlugin(modGUID, modName, modVersion)]
	public sealed class Mod : BaseUnityPlugin
	{
		//Mod Details
		private const string modGUID = "Distance.LevelSelectAdditions";
		public const string modName = "Level Select Additions";
		private const string modVersion = "1.1.5";

		//Config Entry Strings
		public static string HideWorkshopPlaylistsKey = "Hide Workshop Levels in Playlist";
		public static string WorkshopLimitKey = "Workshop Level Limit";
		public static string WorkshopSortingKey = "Workshop Sorting (1st)";
		public static string WorkshopSortingKey2 = "Workshop Sorting (2nd)";
		public static string WorkshopSortingKey3 = "Workshop Sorting (3rd)";
		public static string WorkshopReverseSortingKey = "Reverse Workshop Sorting (1st)";
		public static string WorkshopReverseSortingKey2 = "Reverse Workshop Sorting (2nd)";
		public static string WorkshopReverseSortingKey3 = "Reverse Workshop Sorting (3rd)";
		public static string EnableOtherCampaignKey = "Enable Extra Sprint Campaigns";
		public static string EnableLevelSetOptionsKey = "Enable Playlist Options Menu";
		public static string EnableQuickPlaylistKey = "Enable Playlist Mode for Main Menus";
		public static string EnableVisitWorkshopKey = "Enable Workshop Button for Main Menus";
		public static string EnableRateWorkshopKey = "Enable Rate Workshop Level Button";
		public static string HideUnusedButtonsKey = "Hide Unused Buttons for Main Menus";
		public static string RandomStartupKey = "Decide Menu on Startup";

		//Config Entries
		public static ConfigEntry<bool> HideWorkshopLevelsInPlaylists { get; set; }
		public static ConfigEntry<int> WorkshopLevelLimit { get; set; }
		public static ConfigEntry<SortingMethod> WorkshopSortingMethod { get; set; }
		public static ConfigEntry<SortingMethod> WorkshopSortingMethod2 { get; set; }
		public static ConfigEntry<SortingMethod> WorkshopSortingMethod3 { get; set; }
		public static ConfigEntry<bool> WorkshopReverseSortingMethod { get; set; }
		public static ConfigEntry<bool> WorkshopReverseSortingMethod2 { get; set; }
		public static ConfigEntry<bool> WorkshopReverseSortingMethod3 { get; set; }
		public static ConfigEntry<bool> EnableTheOtherSideSprintCampaign { get; set; }
		public static ConfigEntry<bool> EnableLevelSetOptionsMenu { get; set; }
		public static ConfigEntry<bool> EnableChooseMainMenuQuickPlaylist { get; set; }
		public static ConfigEntry<bool> EnableChooseMainMenuVisitWorkshopButton { get; set; }
		public static ConfigEntry<bool> EnableRateWorkshopLevelButton { get; set; }
		public static ConfigEntry<bool> HideChooseMainMenuUnusedButtons { get; set; }
		public static ConfigEntry<bool> RandomStartupMainMenu { get; set; }

		//Public Variables
		public const bool BasicLevelSetOptionsSupported = false;

		public Dictionary<LevelSetMenuType, Dictionary<bool, LevelSetOptionsMenu>> LevelSetOptionsMenus { get; } = new Dictionary<LevelSetMenuType, Dictionary<bool, LevelSetOptionsMenu>>();
		public Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>> State_LastLevelSetIDs { get; set; } = new Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>>();
		public Dictionary<string, string> ProfileMainMenuLevelSets { get; set; } = new Dictionary<string, string>();
		public Dictionary<string, string> State_LastProfileMainMenuLevels { get; set; } = new Dictionary<string, string>();

		public OptionsMenuLogic OptionsMenu { get; internal set; }

		//Other
		private static readonly Harmony harmony = new Harmony(modGUID);
		public static ManualLogSource Log = new ManualLogSource(modName);
		public static Mod Instance;

		/// <summary>
		/// Method called as soon as the mod is loaded.
		/// </summary>
		public void Awake()
		{
			// Do not destroy the current game object when loading a new scene
			DontDestroyOnLoad(this);

			if (Instance == null)
			{
				Instance = this;
			}

			Log = BepInEx.Logging.Logger.CreateLogSource(modGUID);

			// Arbitrary values for limits, since the Input Prompt is kind of a pain.
			// You can always change them for more precise values in the config file anyways.
			// Maybe the Input Prompt can *also* be added, for more precise changes...
			//  hopefully that would update the other setting in the menu.
			Dictionary<string, int> limitDictionary = LevelFilter.GetArbitraryLimitsList()
																 .ToDictionary(l => LevelFilter.GetLimitName(l));

			Dictionary<string, SortingMethod> sortDictionary = LevelSort.GetSupportedMethodsList()
																		.ToDictionary(s => LevelSort.GetMethodName(s));

			//Config Setup
			HideWorkshopLevelsInPlaylists = Config.Bind("General",
				HideWorkshopPlaylistsKey,
				false,
				new ConfigDescription("Exclude levels from the Workshop Level Set that appear in personal playlists."));

			WorkshopLevelLimit = Config.Bind("General",
				WorkshopLimitKey,
				1000,
				new ConfigDescription("Set maximum number of levels shown in Workshop Level Set.",
					new AcceptableValueRange<int>(-1, 2000)));

			WorkshopSortingMethod = Config.Bind("General",
				WorkshopSortingKey,
				SortingMethod.Recently_Downloaded,
				new ConfigDescription("Choose how Workshop Level Set levels are sorted."));

			WorkshopReverseSortingMethod = Config.Bind("General",
				WorkshopReverseSortingKey,
				false,
				new ConfigDescription("Reverse the order of the first Workshop sorting method."));

			WorkshopSortingMethod2 = Config.Bind("General",
				WorkshopSortingKey2,
				SortingMethod.Recently_Downloaded,
				new ConfigDescription("Second fallback method for how Workshop Level Set levels are sorted."));

			WorkshopReverseSortingMethod2 = Config.Bind("General",
				WorkshopReverseSortingKey2,
				false,
				new ConfigDescription("Reverse the order of the second Workshop sorting method."));

			WorkshopSortingMethod3 = Config.Bind("General",
				WorkshopSortingKey3,
				SortingMethod.Recently_Downloaded,
				new ConfigDescription("Third fallback method for how Workshop Level Set levels are sorted."));

			WorkshopReverseSortingMethod3 = Config.Bind("General",
				WorkshopReverseSortingKey3,
				false,
				new ConfigDescription("Reverse the order of the third Workshop sorting method."));

			EnableTheOtherSideSprintCampaign = Config.Bind("General",
				EnableOtherCampaignKey,
				false,
				new ConfigDescription("Shows extra sprint campaign level sets that aren't normally available (requires unlock)."));

			//Page 2
			EnableLevelSetOptionsMenu = Config.Bind("General",
				EnableLevelSetOptionsKey,
				true,
				new ConfigDescription("Enables the Options menu in the Level Set grid view for customizing personal playlists and choosing main menu collections."));

			EnableChooseMainMenuQuickPlaylist = Config.Bind("General",
				EnableQuickPlaylistKey,
				true,
				new ConfigDescription("Allows creating playlists when choosing a Main Menu level (does not allow selecting multiple levels)."));

			EnableChooseMainMenuVisitWorkshopButton = Config.Bind("General",
				EnableVisitWorkshopKey,
				true,
				new ConfigDescription("Enables the 'Visit Workshop page' button in the Advanced level select menu when choosing a Main Menu level."));

			EnableRateWorkshopLevelButton = Config.Bind("General",
				EnableRateWorkshopKey,
				true,
				new ConfigDescription("Re-introduces the 'Rate this level' button in the Advanced level select menu."));

			HideChooseMainMenuUnusedButtons = Config.Bind("General",
				HideUnusedButtonsKey,
				true,
				new ConfigDescription("Hides unused buttons in the Advanced level select menu when choosing a Main Menu level."));

			RandomStartupMainMenu = Config.Bind("General",
				RandomStartupKey,
				false,
				new ConfigDescription("When using a playlist for the main menu, a random level will only be chosen when starting up the game. Otherwise a level will be chosen every time the main menu is loaded."));

			//Steamworks Setup
			try
			{
				SteamworksHelper.Init(); // Handle this here for early error reporting.
			}
			catch (Exception ex)
			{
				Log.LogError(modName + ": Error during SteamworksHelper.Init()");
				Log.LogError(ex);
				throw;
			}

			//Loading Dictionaries
			State_LastLevelSetIDs = LoadStateLastLevelSetIDs();
			ProfileMainMenuLevelSets = LoadProfileMainMenus("ProfileMainMenuLevelSets.json");
			State_LastProfileMainMenuLevels = LoadProfileMainMenus("State_LastProfileMainMenuLevels.json");

			//Subscribe to events
			PlaylistFileRenamed.Subscribe(OnPlaylistFileRenamed);
			PlaylistFileDeleted.Subscribe(OnPlaylistFileDeleted);

			Log.LogInfo(modName + ": Initializing...");
			harmony.PatchAll();
			Log.LogInfo(modName + ": Initialized!");
		}

        #region Save/Load

        public void SaveDictionary(Dictionary<string, string> dic, string fileName)
        {
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
			DataWriterSettings st = new DataWriterSettings { PrettyPrint = true };
			JsonWriter writer = new JsonWriter(st);
			try
			{
				using (var sw = new StreamWriter(Path.Combine(rootDirectory, fileName), false))
				{
					sw.WriteLine(writer.Write(dic));
				}
			}
			catch (Exception e)
			{
				Log.LogWarning(e);
			}
		}

		public void SaveDictionary(Dictionary<LevelSetMenuType, Dictionary<bool, LevelSetOptionsMenu>> dic)
        {
			string fileName = "LevelSetOptionsMenus.json";
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
			DataWriterSettings st = new DataWriterSettings { PrettyPrint = true };
			JsonWriter writer = new JsonWriter(st);
			try
			{
				using (var sw = new StreamWriter(Path.Combine(rootDirectory, fileName), false))
				{
					sw.WriteLine(writer.Write(dic));
				}
			}
			catch (Exception e)
			{
				Log.LogWarning(e);
			}
		}

		public void SaveDictionary(Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>> dic)
        {
			string fileName = "State_LastLevelSetIDs.json";
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
			DataWriterSettings st = new DataWriterSettings { PrettyPrint = true };
			JsonWriter writer = new JsonWriter(st);
			try
			{
				using (var sw = new StreamWriter(Path.Combine(rootDirectory, fileName), false))
				{
					sw.WriteLine(writer.Write(dic));
				}
			}
			catch (Exception e)
			{
				Log.LogWarning(e);
			}
		}

		public Dictionary<string, string> LoadProfileMainMenus(string fileName)
        {
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

			try
			{
				using (var sr = new StreamReader(Path.Combine(rootDirectory, fileName)))
				{
					string json = sr.ReadToEnd();
					JsonReader reader = new JsonReader();
					Dictionary<string, string> pMainMenuDictionary = reader.Read<Dictionary<string, string>>(json);

					return pMainMenuDictionary;
				}
			}
			catch (DirectoryNotFoundException ex)
			{
				Log.LogWarning("Failed to load car randomization weights due to the directory not existing. \nNew weights will be saved when necessary.");
				return new Dictionary<string, string>();
			}
			catch (Exception ex)
			{
				Log.LogWarning("Failed to load car randomization weights");
				Log.LogWarning(ex);
				return new Dictionary<string, string>();
			}
		}

		public Dictionary<LevelSetMenuType, Dictionary<bool, LevelSetOptionsMenu>> LoadLevelSetOptions()
        {
			string fileName = "LevelSetOptionsMenus.json";
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

			try
			{
				using (var sr = new StreamReader(Path.Combine(rootDirectory, fileName)))
				{
					string json = sr.ReadToEnd();
					JsonReader reader = new JsonReader();
					Dictionary<LevelSetMenuType, Dictionary<bool, LevelSetOptionsMenu>> levelSetOptionsDictionary = reader.Read<Dictionary<LevelSetMenuType, Dictionary<bool, LevelSetOptionsMenu>>>(json);

					return levelSetOptionsDictionary;
				}
			}
			catch (DirectoryNotFoundException ex)
			{
				Log.LogWarning("Failed to load car randomization weights due to the directory not existing. \nNew weights will be saved when necessary.");
				return new Dictionary<LevelSetMenuType, Dictionary<bool, LevelSetOptionsMenu>>();
			}
			catch (Exception ex)
			{
				Log.LogWarning("Failed to load car randomization weights");
				Log.LogWarning(ex);
				return new Dictionary<LevelSetMenuType, Dictionary<bool, LevelSetOptionsMenu>>();
			}
		}

		public Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>> LoadStateLastLevelSetIDs()
        {
			string fileName = "State_LastLevelSetIDs.json";
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

			try
			{
				using (var sr = new StreamReader(Path.Combine(rootDirectory, fileName)))
				{
					string json = sr.ReadToEnd();
					JsonReader reader = new JsonReader();
					Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>> sLastLevelSetDictionary = reader.Read<Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>>>(json);

					return sLastLevelSetDictionary;
				}
			}
			catch (DirectoryNotFoundException ex)
			{
				Log.LogWarning("Failed to load car randomization weights due to the directory not existing. \nNew weights will be saved when necessary.");
				return new Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>>();
			}
			catch (Exception ex)
			{
				Log.LogWarning("Failed to load car randomization weights");
				Log.LogWarning(ex);
				return new Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>>();
			}
		}

		#endregion

		#region Helpers

		public SortingMethod[] GetWorkshopSortingMethods()
		{
			return new SortingMethod[]
			{
				WorkshopSortingMethod.Value,
				WorkshopSortingMethod2.Value,
				WorkshopSortingMethod3.Value,
			};
		}

		public bool[] GetWorkshopReverseSortingMethods()
		{
			return new bool[]
			{
				WorkshopReverseSortingMethod.Value,
				WorkshopReverseSortingMethod2.Value,
				WorkshopReverseSortingMethod3.Value,
			};
		}

		public string GetStateLastLevelSetID(LevelSelectMenuAbstract.DisplayType displayType, GameModeID modeID)
		{
			if (State_LastLevelSetIDs.TryGetValue(displayType, out var lastPlaylists_mode))
			{
				if (lastPlaylists_mode.TryGetValue(modeID, out string levelSetID))
				{
					return levelSetID;
				}
			}
			return null;
		}

		public void SetStateLastLevelSetID(LevelSelectMenuAbstract.DisplayType displayType, GameModeID modeID, string levelSetID)
		{
			var lastPlaylists_display = State_LastLevelSetIDs;
			if (!lastPlaylists_display.TryGetValue(displayType, out Dictionary<GameModeID, string> lastPlaylists_mode))
			{
				lastPlaylists_mode = new Dictionary<GameModeID, string>();
				lastPlaylists_mode[modeID] = levelSetID;
				lastPlaylists_display[displayType] = lastPlaylists_mode;
			}
			//Line was previous here but that didn't make sense to me, moved it, will test for errors
			//lastPlaylists_mode[modeID] = levelSetID;
			SaveDictionary(lastPlaylists_display); // auto save
		}

		public string GetProfileMainMenuRelativePathID(string profileName)
		{
			if (ProfileMainMenuLevelSets.TryGetValue(profileName, out string relativePathID))
			{
				return relativePathID;
			}
			return null;
		}

		public bool SetProfileMainMenuRelativePathID(string profileName, string relativePathID)
		{
			var profileMainMenuLevelSets = ProfileMainMenuLevelSets;
			if (profileMainMenuLevelSets.TryGetValue(profileName, out string oldRelativePathID))
			{
				if (relativePathID == oldRelativePathID)
				{
					return false;
				}
			}
			// Clear state for last level used.
			SetStateLastMainMenuLevelRelativePath(profileName, null);
			profileMainMenuLevelSets[profileName] = relativePathID;
			SaveDictionary(profileMainMenuLevelSets, "ProfileMainMenuLevelSets.json"); // auto save
			return true;
		}

		public string GetStateLastMainMenuLevelRelativePath(string profileName)
		{
			if (State_LastProfileMainMenuLevels.TryGetValue(profileName, out string relativeLevelPath))
			{
				return relativeLevelPath;
			}
			return null;
		}

		public void SetStateLastMainMenuLevelRelativePath(string profileName, string relativeLevelPath)
		{
			State_LastProfileMainMenuLevels[profileName] = relativeLevelPath;
			SaveDictionary(State_LastProfileMainMenuLevels, "State_LastProfileMainMenuLevels.json"); // auto save
		}

		private void OnPlaylistFileRenamed(PlaylistFileRenamed.Data data)
		{
			var lastPlaylists_display = State_LastLevelSetIDs;
			// Use ToArray to enumerate with foreach and allow updating values.
			foreach (var displayPair in lastPlaylists_display.ToArray())
			{
				foreach (var modePair in displayPair.Value.ToArray())
				{
					if (modePair.Value == data.oldLevelSetID)
					{
						lastPlaylists_display[displayPair.Key][modePair.Key] = data.newLevelSetID;
					}
				}
			}

			var profileMainMenuLevelSets = ProfileMainMenuLevelSets;
			// Use ToArray to enumerate with foreach and allow updating values.
			foreach (var profilePair in profileMainMenuLevelSets.ToArray())
			{
				if (string.Equals(profilePair.Value, data.oldLevelSetID, StringComparison.InvariantCultureIgnoreCase))
				{
					profileMainMenuLevelSets[profilePair.Key] = data.playlist.GetRelativePathID();
				}
			}

			//TODO: When LevelSetOptions dictionary gets added, enumerate over and rename here too.

			SaveDictionary(lastPlaylists_display);
			SaveDictionary(profileMainMenuLevelSets, "ProfileMainMenuLevelSets.json"); // auto save
		}

		private void OnPlaylistFileDeleted(PlaylistFileDeleted.Data data)
		{
			var lastPlaylists_display = State_LastLevelSetIDs;
			// Use ToArray to enumerate with foreach and allow updating values.
			foreach (var displayPair in lastPlaylists_display.ToArray())
			{
				foreach (var modePair in displayPair.Value.ToArray())
				{
					if (modePair.Value == data.levelSetID)
					{
						lastPlaylists_display[displayPair.Key][modePair.Key] = null;
					}
				}
			}

			var profileMainMenuLevelSets = ProfileMainMenuLevelSets;
			// Use ToArray to enumerate with foreach and allow updating values.
			foreach (var profilePair in profileMainMenuLevelSets.ToArray())
			{
				if (string.Equals(profilePair.Value, data.levelSetID, StringComparison.InvariantCultureIgnoreCase))
				{
					profileMainMenuLevelSets[profilePair.Key] = null;
				}
			}

			//TODO: When LevelSetOptions dictionary gets added, enumerate over and delete here too.

			SaveDictionary(profileMainMenuLevelSets, "ProfileMainMenuLevelSets.json"); // auto save
		}

		public void ShowLevelSetOptionsMenu(LevelSelectMenuAbstract.DisplayType displayType, GameModeID modeID, LevelPlaylist playlist, Action onDeletePlaylist)
		{
			var menuType = playlist.GetLevelSetMenuType();
			bool isMainMenu = displayType == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
			LevelSetOptionsMenus[menuType][isMainMenu].Show(displayType, modeID, playlist, onDeletePlaylist);
		}

		#endregion
	}
}



