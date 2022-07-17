using Centrifuge.Distance.Game;
using Centrifuge.Distance.GUI.Controls;
using Centrifuge.Distance.GUI.Data;
using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Scripts;
using Distance.LevelSelectAdditions.Scripts.Menus;
using Distance.LevelSelectAdditions.Sorting;
using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using Reactor.API.Logging;
using Reactor.API.Runtime.Patching;
using UnityEngine;
using System.Reflection;

using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.LevelSelectAdditions
{
	/// <summary>
	/// The mod's main class containing its entry point
	/// </summary>
	[ModEntryPoint("com.github.trigger-segfault/Distance.LevelSelectAdditions")]
	public sealed class Mod : MonoBehaviour
	{
		public const string Name = "LevelSelectAdditions";
		public const string FullName = "Distance." + Name;
		public const string FriendlyName = "Level Select Additions";

		public const bool BasicLevelSetOptionsSupported = false;


		public static Mod Instance { get; private set; }

		public IManager Manager { get; private set; }

		public Log Logger { get; private set; }

		public ConfigurationLogic Config { get; private set; }


		public LevelSetOptionsMenu LevelSetOptionsMenu { get; internal set; }

		public OptionsMenuLogic OptionsMenu { get; internal set; }

		/// <summary>
		/// Method called as soon as the mod is loaded.
		/// WARNING:	Do not load asset bundles/textures in this function
		///				The unity assets systems are not yet loaded when this
		///				function is called. Loading assets here can lead to
		///				unpredictable behaviour and crashes!
		/// </summary>
		public void Initialize(IManager manager)
		{
			// Do not destroy the current game object when loading a new scene
			DontDestroyOnLoad(this);

			Instance = this;
			Manager = manager;

			Logger = LogManager.GetForCurrentAssembly();
			Logger.Info(Mod.Name + ": Initializing...");

			Config = gameObject.AddComponent<ConfigurationLogic>();

			try
			{
				// Never ever EVER use this!!!
				// It's the same as below (with `GetCallingAssembly`) wrapped around a silent catch-all.
				//RuntimePatcher.AutoPatch();

				RuntimePatcher.HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
			}
			catch (Exception ex)
			{
				Logger.Error(Mod.Name + ": Error during Harmony.PatchAll()");
				Logger.Exception(ex);
				throw;
			}

			try
			{
				CreateSettingsMenu();
			}
			catch (Exception ex)
			{
				Logger.Error(Mod.Name + ": Error during CreateSettingsMenu()");
				Logger.Exception(ex);
				throw;
			}

			Logger.Info(Mod.Name + ": Initialized!");
		}

		private void CreateSettingsMenu()
		{
			// Arbitrary values for limits, since the Input Prompt is kind of a pain.
			// You can always change them for more precise values in the config file anyways.
			// Maybe the Input Prompt can *also* be added, for more precise changes...
			//  hopefully that would update the other setting in the menu.
			Dictionary<string, int> limitDictionary = LevelFilter.GetArbitraryLimitsList()
																 .ToDictionary(l => LevelFilter.GetLimitName(l));

			Dictionary<string, SortingMethod> sortDictionary = LevelSort.GetSupportedMethodsList()
																		.ToDictionary(s => LevelSort.GetMethodName(s));


			MenuTree settingsMenu = new MenuTree("menu.mod." + Mod.Name.ToLower(), Mod.FriendlyName);

			settingsMenu.CheckBox(MenuDisplayMode.MainMenu,
				"setting:workshop_hide_levels_in_playlists",
				"HIDE WORKSHOP LEVELS IN PLAYLISTS",
				() => Config.HideWorkshopLevelsInPlaylists,
				(value) => Config.HideWorkshopLevelsInPlaylists = value,
				description: "Exclude levels from the Workshop Level Set that appear in personal playlists.");

			settingsMenu.ListBox<int>(MenuDisplayMode.MainMenu,
				"setting:workshop_level_limit",
				"WORKSHOP LEVEL LIMIT",
				() => Config.WorkshopLevelLimit,
				(value) => Config.WorkshopLevelLimit = value,
				limitDictionary,
				description: "Set maximum number of levels shown in Workshop Level Set.");

			// Unfortunately, ListBoxes don't support displaying unknown entries, so using this would be ugly :(
			/*settingsMenu.InputPrompt(MenuDisplayMode.MainMenu,
				"setting:workshop_level_limit",
				"CHANGE WORKSHOP LEVEL LIMIT...",
				(string x) => {
					if (int.TryParse(x, out int result))
					{
						Config.WorkshopLevelLimit = Math.Max(//-1
															 0,
															 result);
					}
					else
					{
						Logger.Warning("Failed to parse user input.");
					}
				},
				title: "NEW LEVEL LIMIT",
				defaultValue: null, //"1000",
				description: "Manually set the maximum number of levels shown in Workshop Level Set.");*/

			settingsMenu.ListBox<LevelSelectMenuLogic.SortingMethod>(MenuDisplayMode.MainMenu,
				"setting:workshop_sorting_method",
				"WORKSHOP SORTING (1ST)",
				() => Config.WorkshopSortingMethod,
				(value) => Config.WorkshopSortingMethod = value,
				sortDictionary,
				description: "Choose how Workshop Level Set levels are sorted.");

			settingsMenu.CheckBox(MenuDisplayMode.MainMenu,
				"setting:workshop_reverse_sorting_method",
				"REVERSE WORKSHOP SORTING (1ST)",
				() => Config.WorkshopReverseSortingMethod,
				(value) => Config.WorkshopReverseSortingMethod = value,
				description: "Reverse the order of the first Workshop sorting method.");

			settingsMenu.ListBox<LevelSelectMenuLogic.SortingMethod>(MenuDisplayMode.MainMenu,
				"setting:workshop_sorting_method2",
				"WORKSHOP SORTING (2ND)",
				() => Config.WorkshopSortingMethod2,
				(value) => Config.WorkshopSortingMethod2 = value,
				sortDictionary,
				description: "Second fallback method for how Workshop Level Set levels are sorted.");

			settingsMenu.CheckBox(MenuDisplayMode.MainMenu,
				"setting:workshop_reverse_sorting_method2",
				"REVERSE WORKSHOP SORTING (2ND)",
				() => Config.WorkshopReverseSortingMethod2,
				(value) => Config.WorkshopReverseSortingMethod2 = value,
				description: "Reverse the order of the second Workshop sorting method.");

			settingsMenu.ListBox<LevelSelectMenuLogic.SortingMethod>(MenuDisplayMode.MainMenu,
				"setting:workshop_sorting_method3",
				"WORKSHOP SORTING (3RD)",
				() => Config.WorkshopSortingMethod3,
				(value) => Config.WorkshopSortingMethod3 = value,
				sortDictionary,
				description: "Third fallback method for how Workshop Level Set levels are sorted.");

			settingsMenu.CheckBox(MenuDisplayMode.MainMenu,
				"setting:workshop_reverse_sorting_method3",
				"REVERSE WORKSHOP SORTING (3RD)",
				() => Config.WorkshopReverseSortingMethod3,
				(value) => Config.WorkshopReverseSortingMethod3 = value,
				description: "Reverse the order of the third Workshop sorting method.");


			settingsMenu.CheckBox(MenuDisplayMode.MainMenu,
				"setting:levelsets_enable_extra_sprint_campaigns",
				"ENABLE EXTRA SPRINT CAMPAIGNS",
				() => Config.EnableTheOtherSideSprintCampaign,
				(value) => Config.EnableTheOtherSideSprintCampaign = value,
				description: "Shows extra sprint campaign level sets that aren't normally available (requires unlock).");


			Menus.AddNew(MenuDisplayMode.MainMenu, settingsMenu,
				Mod.FriendlyName.ToUpper(),
				"Settings for level selection limits, filtering, sorting, and organization.");
		}
	}
}



