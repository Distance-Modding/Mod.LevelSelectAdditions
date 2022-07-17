using Distance.LevelSelectAdditions.Events;
using Distance.LevelSelectAdditions.Sorting;
using Reactor.API.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.LevelSelectAdditions
{
	public class ConfigurationLogic : MonoBehaviour
	{
		#region Properties

		private const string HideWorkshopLevelsInPlaylists_ID = "workshop.hide_levels_in_playlists";
		public bool HideWorkshopLevelsInPlaylists
		{
			get => Get<bool>(HideWorkshopLevelsInPlaylists_ID);
			set => Set(HideWorkshopLevelsInPlaylists_ID, value);
		}

		private const string WorkshopLevelLimit_ID = "workshop.level_limit";
		public int WorkshopLevelLimit
		{
			get => Get<int>(WorkshopLevelLimit_ID);
			set => Set(WorkshopLevelLimit_ID, value);
		}

		private const string WorkshopSortingMethod_ID = "workshop.sorting_method";
		public SortingMethod WorkshopSortingMethod
		{
			get => Get<SortingMethod>(WorkshopSortingMethod_ID);
			set => Set(WorkshopSortingMethod_ID, value);
		}

		private const string WorkshopSortingMethod2_ID = "workshop.sorting_method2";
		public SortingMethod WorkshopSortingMethod2
		{
			get => Get<SortingMethod>(WorkshopSortingMethod2_ID);
			set => Set(WorkshopSortingMethod2_ID, value);
		}

		private const string WorkshopSortingMethod3_ID = "workshop.sorting_method3";
		public SortingMethod WorkshopSortingMethod3
		{
			get => Get<SortingMethod>(WorkshopSortingMethod3_ID);
			set => Set(WorkshopSortingMethod3_ID, value);
		}

		private const string WorkshopReverseSortingMethod_ID = "workshop.reverse_sorting_method";
		public bool WorkshopReverseSortingMethod
		{
			get => Get<bool>(WorkshopReverseSortingMethod_ID);
			set => Set(WorkshopReverseSortingMethod_ID, value);
		}

		private const string WorkshopReverseSortingMethod2_ID = "workshop.reverse_sorting_method2";
		public bool WorkshopReverseSortingMethod2
		{
			get => Get<bool>(WorkshopReverseSortingMethod2_ID);
			set => Set(WorkshopReverseSortingMethod2_ID, value);
		}

		private const string WorkshopReverseSortingMethod3_ID = "workshop.reverse_sorting_method3";
		public bool WorkshopReverseSortingMethod3
		{
			get => Get<bool>(WorkshopReverseSortingMethod3_ID);
			set => Set(WorkshopReverseSortingMethod3_ID, value);
		}

		// Future plans:
		/*private const string EnableRecentlDownloadsLevelSet_ID = "recentdownloads.enable";
		public bool EnableRecentlDownloadsLevelSet
		{
			get => Get<bool>(EnableRecentlDownloadsLevelSet_ID);
			set => Set(EnableRecentlDownloadsLevelSet_ID, value);
		}

		private const string RecentDownloadedsTimeInDays_ID = "recentdownloads.time_in_days";
		public double RecentDownloadedsTimeInDays
		{
			get => Get<double>(RecentDownloadedsTimeInDays_ID);
			set => Set(RecentDownloadedsTimeInDays_ID, value);
		}
		
		private const string HideRecentDownloadsLevelsInPlaylists_ID = "recentdownloads.hide_levels_in_playlists";
		public bool HideRecentDownloadsLevelsInPlaylists
		{
			get => Get<bool>(HideRecentDownloadsLevelsInPlaylists_ID);
			set => Set(HideRecentDownloadsLevelsInPlaylists_ID, value);
		}

		private const string RecentDownloadsLevelLimit_ID = "recentdownloads.level_limit";
		public int RecentDownloadsLevelLimit
		{
			get => Get<int>(RecentDownloadsLevelLimit_ID);
			set => Set(RecentDownloadsLevelLimit_ID, value);
		}*/



		private const string EnableTheOtherSideSprintCampaign_ID = "levelsets.enable_extra_sprint_campaigns";
		public bool EnableTheOtherSideSprintCampaign
		{
			get => Get<bool>(EnableTheOtherSideSprintCampaign_ID);
			set => Set(EnableTheOtherSideSprintCampaign_ID, value);
		}

		/*private const string LevelSetSettingsTable_ID = "levelsets.options";
		public Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>> LevelSetSettingsTable
		{
			get => Convert(LevelSetSettingsTable_ID, new Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>>(), overwriteNull: true);
			set => Set(LevelSetSettingsTable_ID, value);
		}*/

		private const string State_LastLevelSets_ID = "state.last_levelsets";
		public Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>> State_LastLevelSetIDs
		{
			get => Convert(State_LastLevelSets_ID, new Dictionary<LevelSelectMenuAbstract.DisplayType, Dictionary<GameModeID, string>>(), overwriteNull: true);
			set => Set(State_LastLevelSets_ID, value);
		}

		// No config option for this yet
		public bool EnableLevelSetOptionsMenu => true;

		#endregion

		#region Helpers

		public SortingMethod[] GetWorkshopSortingMethods()
		{
			return new SortingMethod[]
			{
				WorkshopSortingMethod,
				WorkshopSortingMethod2,
				WorkshopSortingMethod3,
			};
		}

		public bool[] GetWorkshopReverseSortingMethods()
		{
			return new bool[]
			{
				WorkshopReverseSortingMethod,
				WorkshopReverseSortingMethod2,
				WorkshopReverseSortingMethod3,
			};
		}

		public string GetStateLastLevelSetID(LevelSelectMenuAbstract.DisplayType displayType, GameModeID modeID)
		{
			if (State_LastLevelSetIDs.TryGetValue(displayType, out var lastPlaylists_mode))
			{
				if (lastPlaylists_mode.TryGetValue(modeID, out string pathName))
				{
					return pathName;
				}
			}
			return null;
		}

		public void SetStateLastLevelSetID(LevelSelectMenuAbstract.DisplayType displayType, GameModeID modeID, string pathName)
		{
			var lastPlaylists_display = State_LastLevelSetIDs;
			if (!lastPlaylists_display.TryGetValue(displayType, out Dictionary<GameModeID, string> lastPlaylists_mode))
			{
				lastPlaylists_mode = new Dictionary<GameModeID, string>();
				lastPlaylists_display[displayType] = lastPlaylists_mode;
			}
			lastPlaylists_mode[modeID] = pathName;
			this.Save(); // auto save
		}

		#endregion

		internal Settings Config;

		public event Action<ConfigurationLogic> OnChanged;

		private void Load()
		{
			Config = new Settings("Config");// Mod.FullName);
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

			//TODO: When LevelSetOptions dictionary gets added, enumerate over and rename here too.
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

			//TODO: When LevelSetOptions dictionary gets added, enumerate over and delete here too.
		}

		public void Awake()
		{
			PlaylistFileRenamed.Subscribe(OnPlaylistFileRenamed);
			PlaylistFileDeleted.Subscribe(OnPlaylistFileDeleted);

			Load();

			// Assign default settings (if not already assigned).
			Get(HideWorkshopLevelsInPlaylists_ID, false);
			Get(WorkshopLevelLimit_ID, 1000);
			Get(WorkshopSortingMethod_ID, LevelSort.Recently_Downloaded);
			Get(WorkshopSortingMethod2_ID, LevelSort.None);
			Get(WorkshopSortingMethod3_ID, LevelSort.None);
			Get(WorkshopReverseSortingMethod_ID, false);
			Get(WorkshopReverseSortingMethod2_ID, false);
			Get(WorkshopReverseSortingMethod3_ID, false);
			Get(EnableTheOtherSideSprintCampaign_ID, false);

			// Save settings, and any defaults that may have been added.
			Save();
		}

		public T Get<T>(string key, T @default = default)
		{
			return Config.GetOrCreate(key, @default);
		}

		public T Convert<T>(string key, T @default = default, bool overwriteNull = false)
		{
			// Assign the object back after conversion, this allows for deep nested settings
			//  that can be preserved and updated without reassigning to the root property.
			var value = Config.GetOrCreate(key, @default);
			if (overwriteNull && value == null)
			{
				value = @default;
			}
			Config[key] = value;
			return value;
		}

		public void Set<T>(string key, T value)
		{
			Config[key] = value;
			Save();
		}

		public void Save()
		{
			Config?.Save();
			OnChanged?.Invoke(this);
		}
	}
}
