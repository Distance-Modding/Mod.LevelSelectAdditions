using Centrifuge.Distance.GUI.Menu;
using Distance.LevelSelectAdditions.Extensions;
using System;
using UnityEngine;

using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.LevelSelectAdditions.Scripts.Menus
{
	// Use `CentrifugeMenuAbstract` because it already handles assinging the `menuBlueprint_` field.
	public class LevelSetOptionsMenu : CentrifugeMenuAbstract
	{
		protected GameModeID modeID_;
		protected LevelSelectMenuAbstract.DisplayType displayType_;
		protected LevelPlaylist playlist_;
		protected Action onPopBack_;
		protected Action onDeletePlaylist_;


		public override string Name_ => "custommenu.mod." + Mod.Name.ToLower() + "." + nameof(LevelSetOptionsMenu);

		public override string Title
		{
			get
			{
				switch (this.LevelSetMenuType)
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

		// This can be called during Awake, so have a backup when playlist hasn't been assigned yet.
		public LevelSetMenuType LevelSetMenuType => this.playlist_?.GetLevelSetMenuType() ?? LevelSetMenuType.Basic;

		public GameObject TitleLabel => this.PanelObject_.transform.Find("MenuTitleTemplate/UILabel - Title").gameObject;
		
		public GameObject DescriptionLabel => this.PanelObject_.transform.Find("MenuTitleTemplate/UILabel - Description").gameObject;


		public void Show(LevelSelectMenuAbstract.DisplayType displayType, GameModeID modeID, LevelPlaylist playlist, Action onDeletePlaylist)
		{
			try
			{
				this.displayType_ = displayType;
				this.modeID_ = modeID;
				this.playlist_ = playlist;
				this.onPopBack_ = null;
				this.onDeletePlaylist_ = onDeletePlaylist;

				this.UpdateMenuTitle();

				Mod.Instance.OptionsMenu.DisplaySubmenu(this.Name_);
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in RenameFile()");
				Mod.Instance.Logger.Exception(ex);
				throw;
			}
		}

		// Removes the slide-in/fade-in animation for this Options menu,
		//  since we don't want this for a menu accessed in level select.
		public void RemoveFancyFadeIn()
		{
			if (this.PanelObject_.HasComponent<UIExFancyFadeInMenu>())
			{
				//Mod.Instance.Logger.Debug("Removed UIExFancyFadeInMenu from " + this.GetType().Name);
				this.PanelObject_.RemoveComponents<UIExFancyFadeInMenu>();
			}
		}

		// NOTE: The Title property is only used once during Awake for this menu,
		//  so we can't set the real title then before knowing what type of submenu it is.
		protected void UpdateMenuTitle()
		{
			UILabel titleLabelObject = this.TitleLabel.GetComponent<UILabel>();
			(this.menu_.menuTitleLabel_ ?? titleLabelObject).text = this.Title;
		}

		// Displays the playlist name below the menu title.
		protected void UpdateMenuDescription()
		{
			Color color = Color.white;
			bool colorTag = this.playlist_.Name_.DecodeNGUIColorTag(out _);
			if (!colorTag)
			{
				// No color tag, so we need to use the multiplier color.
				var playlistData = this.playlist_.GetComponent<LevelPlaylistCompoundData>();
				if (playlistData?.PlaylistEntry != null)
				{
					color = playlistData.PlaylistEntry.Color_;
				}
				else
				{
					color = GConstants.myLevelColor_;
				}
			}

			UILabel titleLabelObject = this.TitleLabel.GetComponent<UILabel>();
			UILabel descriptionLabelObject = this.DescriptionLabel.GetComponent<UILabel>();

			this.DescriptionLabel?.SetActive(true);

			descriptionLabelObject.text = this.playlist_.Name_;
			descriptionLabelObject.color = color;

			// Use same font as playlist entries in Level Sets menu.
			descriptionLabelObject.fontStyle = titleLabelObject.fontStyle; // FontStyle.Normal;
			descriptionLabelObject.fontSize = titleLabelObject.fontSize; // 30;
			descriptionLabelObject.ambigiousFont = titleLabelObject.ambigiousFont; // Resource.LoadFont("DenseBold");
		}

		// This is a custom menu that's not supposed to show up in Options.
		public override bool DisplayInMenu(bool isPauseMenu) => false;

		public override void OnPanelPop()
		{
			this.onPopBack_?.Invoke();
		}

		public override void Update()
		{
			if (this.PanelObject_ != null && this.PanelObject_.activeInHierarchy)
			{
				this.UpdateVirtual();
			}
		}

		public override void UpdateVirtual()
		{
			this.UpdateMenuDescription();
		}

		public override void InitializeVirtual()
		{
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

			if (this.LevelSetMenuType == LevelSetMenuType.Playlist)
			{
				this.TweakAction("CHANGE DISPLAY NAME",
					this.OnRenamePlaylistClicked,
					"Change the display name of this playlist.");

				this.TweakAction("CHANGE DISPLAY COLOR",
					this.OnRecolorPlaylistClicked,
					"Change the display color of this playlist. Submit an empty input to remove the color.");

				this.TweakAction("RENAME PLAYLIST FILE",
					this.OnRenameFileClicked,
					"Change the filename of this playlist, which affects sorting order.");

				this.TweakAction("DELETE PLAYLIST FILE",
					this.OnDeleteFileClicked,
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
				Mod.Instance.Logger.Error("Error in OnRenamePlaylistClicked()");
				Mod.Instance.Logger.Exception(ex);
			}
		}

		private void OnRecolorPlaylistClicked()
		{
			try
			{
				this.playlist_.PromptRecolor(null, null, true);
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in OnRecolorPlaylistClicked()");
				Mod.Instance.Logger.Exception(ex);
			}
		}

		private void OnRenameFileClicked()
		{
			try
			{
				this.playlist_.PromptRenameFile(null, null);
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in OnRenameFileClicked()");
				Mod.Instance.Logger.Exception(ex);
			}
		}

		private void OnDeleteFileClicked()
		{
			try
			{
				this.playlist_.PromptDeleteFile(OnDeleteFileSubmit, true);
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in OnDeleteFileClicked()");
				Mod.Instance.Logger.Exception(ex);
			}
		}

		private void OnDeleteFileSubmit(bool changed)
		{
			try
			{
				G.Sys.MenuPanelManager_.Pop();
				this.onDeletePlaylist_?.Invoke();
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in OnDeleteFileSubmit()");
				Mod.Instance.Logger.Exception(ex);
			}
		}
	}
}
