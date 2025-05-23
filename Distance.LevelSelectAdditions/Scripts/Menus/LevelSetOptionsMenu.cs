using DistanceModConfigurationManager.DistanceGUI.Menu;
using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Helpers;
using System;
using UnityEngine;

using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.LevelSelectAdditions.Scripts.Menus
{
	// Use `ModdingMenuAbstract` because it already handles assinging the `menuBlueprint_` field.
	public class LevelSetOptionsMenu : ModdingMenuAbstract
	{
		protected GameModeID modeID_;
		protected LevelSelectMenuAbstract.DisplayType displayType_;
		protected LevelPlaylist playlist_;
		protected Action onPopBack_;
		protected Action onDeletePlaylist_;


		public override string Name_ => "custommenu.mod." + Mod.modName.ToLower() + "." + nameof(LevelSetOptionsMenu) + "." + LevelSetMenuType.ToString().ToLower() + ((IsMainMenu) ? "_mainmenu" : "");

		public override string Title
		{
			get
			{
				switch (LevelSetMenuType)
				{
				case LevelSetMenuType.Basic:
					return "Level Set Options";

				case LevelSetMenuType.Special:
					return "Special Level Set Options";

				case LevelSetMenuType.Playlist:
					return "Playlist Level Set Options";

				default:
					goto case LevelSetMenuType.Basic;
				}
			}
		}

		public LevelSetMenuType LevelSetMenuType { get; internal set; }

		public bool IsMainMenu { get; internal set; }

		public GameObject TitleLabel => PanelObject_.transform.Find("MenuTitleTemplate/UILabel - Title").gameObject;
		
		public GameObject DescriptionLabel => PanelObject_.transform.Find("MenuTitleTemplate/UILabel - Description").gameObject;


		public void Show(LevelSelectMenuAbstract.DisplayType displayType, GameModeID modeID, LevelPlaylist playlist, Action onDeletePlaylist)
		{
			try
			{
				displayType_ = displayType;
				modeID_ = modeID;
				playlist_ = playlist;
				onPopBack_ = null;
				onDeletePlaylist_ = onDeletePlaylist;

				UpdateMenuTitle();

				Mod.Instance.OptionsMenu.DisplaySubmenu(Name_);
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in RenameFile()");
				Mod.Log.LogError(ex);
				throw;
			}
		}

		// Removes the slide-in/fade-in animation for this Options menu,
		//  since we don't want this for a menu accessed in level select.
		public void RemoveFancyFadeIn()
		{
			if (PanelObject_.HasComponent<UIExFancyFadeInMenu>())
			{
				//Mod.Instance.Logger.Debug("Removed UIExFancyFadeInMenu from " + this.GetType().Name);
				PanelObject_.RemoveComponents<UIExFancyFadeInMenu>();
			}
		}

		// NOTE: The Title property is only used once during Awake for this menu,
		//  so we can't set the real title then before knowing what type of submenu it is.
		protected void UpdateMenuTitle()
		{
			UILabel titleLabelObject = TitleLabel.GetComponent<UILabel>();
			(menu_.menuTitleLabel_ ?? titleLabelObject).text = Title;
		}

		// Displays the playlist name below the menu title.
		protected void UpdateMenuDescription()
		{
			Color baseColor = Color.white;
			var playlistData = playlist_.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData?.PlaylistEntry != null)
			{
				baseColor = playlistData.PlaylistEntry.Color_;
			}

			UILabel titleLabelObject = TitleLabel.GetComponent<UILabel>();
			UILabel descriptionLabelObject = DescriptionLabel.GetComponent<UILabel>();

			DescriptionLabel?.SetActive(true);

			descriptionLabelObject.text = playlist_.Name_;
			descriptionLabelObject.color = baseColor;

			// Use same font as playlist entries in Level Sets menu.
			descriptionLabelObject.fontStyle = titleLabelObject.fontStyle; // FontStyle.Normal;
			descriptionLabelObject.fontSize = titleLabelObject.fontSize; // 30;
			descriptionLabelObject.ambigiousFont = titleLabelObject.ambigiousFont; // Resource.LoadFont("DenseBold");
		}

		// This is a custom menu that's not supposed to show up in Options.
		public override bool DisplayInMenu(bool isPauseMenu) => false;

		public override void OnPanelPop()
		{
			onPopBack_?.Invoke();
		}

		public override void Update()
		{
			if (PanelObject_ != null && PanelObject_.activeInHierarchy)
			{
				UpdateVirtual();
			}
		}

		public override void UpdateVirtual()
		{
			UpdateMenuDescription();
		}

		public override void InitializeVirtual()
		{
			// Apparently we can't just call this multiple times with different settings.
			// Menus have been split into all possible categories to prevent issues.

			if (IsMainMenu)
			{
				const string SetAsMainMenu = "SET AS MAIN MENU";

				TweakAction(SetAsMainMenu, //"SET AS MAIN MENU" + ((isCurrent) ? " (CURRENT)" : ""),
					OnSetAsMainMenuClicked,
					"Use this playlist as the main menu. A level will be chosen from the level set based on your Main Menu playlist selection options.");

				// Change the label text to show whether this is the currently selected main menu level set.
				string relativePathID = playlist_.GetRelativePathID();
				Profile profile = G.Sys.ProfileManager_.CurrentProfile_;
				string currentRelativePathID = Mod.Instance.GetProfileMainMenuRelativePathID(profile.Name_);
				bool isCurrent = relativePathID == currentRelativePathID;

				var actionLabel = menu_.actions_[SetAsMainMenu].gameObject.GetComponentInChildren<UILabel>();
				actionLabel.text = SetAsMainMenu + ((isCurrent) ? " (CURRENT)" : "");
			}

			// TODO OPTIONS: Sorting
			/*
			var sortMethods = SortingMethods.AllSupported;
			if (!Mod.Instance.Config.EnableSortByDiamondMedal)
			{
				sortMethods = sortMethods.Where((m) => m != SortingMethods.Diamond_Medal).ToArray();
			}
			var sortEntries = sortMethods.Select((m) => SuperMenu.KVP<string, SortingMethod>(SortingMethods.GetFriendlyName(m), m)).ToArray();

			this.TweakEnum<SortingMethod>(InternalResources.Strings.Settings.SortingReverse0,
				() => this.SortSettings_.SortingMethod0,
				(value) => this.SortSettings_.SortingMethod0 = value,
				"Choose how this Level Set is sorted.",
				sortEntries);

			this.TweakBool(InternalResources.Strings.Settings.SortingReverse0,
				this.SortSettings_.Reverse0,
				(value) => this.SortSettings_.Reverse0 = value,
				"Reverse the order of the first sorting method.");

			this.TweakEnum<SortingMethod>(InternalResources.Strings.Settings.SortingReverse1,
				() => this.SortSettings_.SortingMethod1,
				(value) => this.SortSettings_.SortingMethod1 = value,
				"Second fallback method for how this Level Set is sorted.",
				sortEntries);

			this.TweakBool(InternalResources.Strings.Settings.SortingReverse1,
				this.SortSettings_.Reverse1,
				(value) => this.SortSettings_.Reverse1 = value,
				"Reverse the order of the second sorting method.");

			this.TweakEnum<SortingMethod>(InternalResources.Strings.Settings.SortingReverse2,
				() => this.SortSettings_.SortingMethod2,
				(value) => this.SortSettings_.SortingMethod2 = value,
				"Third fallback method for how this Level Set is sorted.",
				sortEntries);

			this.TweakBool(InternalResources.Strings.Settings.SortingReverse2,
				this.SortSettings_.Reverse2,
				(value) => this.SortSettings_.Reverse2 = value,
				"Reverse the order of the third sorting method.");
			*/

			if (this.LevelSetMenuType == LevelSetMenuType.Special)
			{
				// TODO OPTIONS: Limit, Exclude levels in playlists
				/*
				var limitValues = Mod.Instance.Config.LimitValues.ToArray();// LevelFilter.ArbitraryLimits;
				if (limitValues.Length == 0)
				{
					limitValues = LevelFilter.ArbitraryLimits;
				}
				var limitEntries = limitValues.Select((m) => SuperMenu.KVP<string, int>(LevelFilter.GetFriendlyLimitName(m), m)).ToArray();


				this.TweakEnum<int>(InternalResources.Strings.Settings.LimitCount,
					() => this.SortSettings_.LimitCount,
					(value) => this.SortSettings_.LimitCount = value,
					"Set the maximum number of levels shown in this Level Set.",
					limitEntries);

				this.TweakBool(InternalResources.Strings.Settings.HideLevelsInPlaylists,
					this.SortSettings_.HideLevelsInPlaylists,
					(value) => this.SortSettings_.HideLevelsInPlaylists = value,
					"Exclude levels from this Level Set that appear in personal playlists.");
				*/
			}

			if (LevelSetMenuType == LevelSetMenuType.Playlist)
			{
				TweakAction("CHANGE DISPLAY NAME",
					OnRenamePlaylistClicked,
					"Change the display name of this playlist.");

				TweakAction("CHANGE DISPLAY COLOR",
					OnRecolorPlaylistClicked,
					"Change the display color of this playlist. Submit an empty input to remove the color.");

				TweakAction("RENAME PLAYLIST FILE",
					OnRenameFileClicked,
					"Change the filename of this playlist, which affects sorting order.");

				TweakAction("DELETE PLAYLIST FILE",
					OnDeleteFileClicked,
					"Delete this playlist from the game.");
			}
		}


		private void OnRenamePlaylistClicked()
		{
			try
			{
				this.playlist_.PromptRename(null, null, true);
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in OnRenamePlaylistClicked()");
				Mod.Log.LogError(ex);
			}
		}

		private void OnRecolorPlaylistClicked()
		{
			try
			{
				playlist_.PromptRecolor(null, null, true);
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in OnRecolorPlaylistClicked()");
				Mod.Log.LogError(ex);
			}
		}

		private void OnRenameFileClicked()
		{
			try
			{
				playlist_.PromptRenameFile(null, null);
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in OnRenameFileClicked()");
				Mod.Log.LogError(ex);
			}
		}

		private void OnDeleteFileClicked()
		{
			try
			{
				playlist_.PromptDeleteFile(OnDeleteFileSubmit, true);
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in OnDeleteFileClicked()");
				Mod.Log.LogError(ex);
			}
		}

		private void OnDeleteFileSubmit(bool changed)
		{
			try
			{
				G.Sys.MenuPanelManager_.Pop();
				onDeletePlaylist_?.Invoke();
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in OnDeleteFileSubmit()");
				Mod.Log.LogError(ex);
			}
		}

		private void OnSetAsMainMenuClicked()
		{
			MainMenuLevelSetHelper.SetMainMenuLevelSet(playlist_);
			//Profile profile = G.Sys.ProfileManager_.CurrentProfile_;
			//Mod.Instance.Config.SetProfileMainMenuRelativePathID(profile.Name_, this.playlist_.GetRelativePathID());
			G.Sys.GameManager_.GoToMainMenu(GameManager.OpenOnMainMenuInit.Garage);
		}
	}
}
