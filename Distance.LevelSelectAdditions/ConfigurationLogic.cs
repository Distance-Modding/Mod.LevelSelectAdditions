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

		#endregion

		internal Settings Config;

		public event Action<ConfigurationLogic> OnChanged;

		private void Load()
		{
			Config = new Settings("Config");// Mod.FullName);
		}

		public void Awake()
		{
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

			// Save settings, and any defaults that may have been added.
			Save();
		}

		public T Get<T>(string key, T @default = default)
		{
			return Config.GetOrCreate(key, @default);
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
