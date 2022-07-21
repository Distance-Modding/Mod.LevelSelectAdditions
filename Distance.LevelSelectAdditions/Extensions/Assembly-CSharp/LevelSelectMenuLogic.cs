using Distance.LevelSelectAdditions.Scripts;
using System;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Extensions
{
	public static class LevelSelectMenuLogicExtensions
	{
		public static void UpdateQuickPlaylistText(this LevelSelectMenuLogic levelSelectMenu)
		{
			if (!levelSelectMenu.tempPlaylist_.Name_.IsEmptyPlaylistName())
			{
				levelSelectMenu.quickPlaylistLabel_.text = levelSelectMenu.tempPlaylist_.Name_; // Update the label showing the playlist name
			}

			// Preserve playlist name color (when not using `[c][/c] tag).
			levelSelectMenu.tempPlaylist_.GetBaseColor(out Color baseColor, false);
			levelSelectMenu.quickPlaylistLabel_.color = baseColor;
		}

		public static void UpdateBottomLeftButtonVisibility(this LevelSelectMenuLogic levelSelectMenu)
		{
			// Make sure to always display these buttons when in Playlist Mode.
			if (Mod.Instance.Config.HideChooseMainMenuUnusedButtons && !levelSelectMenu.showingLevelPlaylist_)
			{
				// Hide unused buttons when in the Choose Main Menu display type.
				bool isMainMenu = levelSelectMenu.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
				levelSelectMenu.createPlaylistButton_.SetActive(!isMainMenu || Mod.Instance.Config.EnableChooseMainMenuQuickPlaylist);
				levelSelectMenu.showLeaderboardsButton_.SetActive(!isMainMenu);
			}
			else
			{
				levelSelectMenu.createPlaylistButton_.SetActive(true);
				levelSelectMenu.showLeaderboardsButton_.SetActive(true);
			}
		}

		public static void ResetTempPlaylistState(this LevelSelectMenuLogic levelSelectMenu)
		{
			levelSelectMenu.tempPlaylist_.Name_ = nameof(LevelPlaylist); // restore default uninitialized name

			var playlistData = levelSelectMenu.tempPlaylist_.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData)
			{
				playlistData.FilePath = null;
			}
		}
	}
}
