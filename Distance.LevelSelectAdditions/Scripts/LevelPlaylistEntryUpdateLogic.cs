#pragma warning disable IDE0051
using Distance.LevelSelectAdditions.Events;
using Distance.LevelSelectAdditions.Extensions;
using System;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Scripts
{
	public class LevelPlaylistEntryUpdateLogic : MonoBehaviour
	{
		private LevelGridMenu menu_;


		private void Awake()
		{
			this.menu_ = this.GetComponentInParent<LevelGridMenu>();
			if (!this.menu_)
			{
				Mod.Instance.Logger.Error(nameof(LevelGridMenu) + " component not found");
			}

			PlaylistNameChanged.Subscribe(OnPlaylistNameChanged);
			PlaylistColorChanged.Subscribe(OnPlaylistColorChanged);
		}
		private void OnDestroy()
		{
			PlaylistNameChanged.Unsubscribe(OnPlaylistNameChanged);
			PlaylistColorChanged.Unsubscribe(OnPlaylistColorChanged);
		}

		private void OnPlaylistNameChanged(PlaylistNameChanged.Data data)
		{
			try
			{
				foreach (LevelGridMenu.PlaylistEntry playlistEntry in this.menu_.ScrollableEntries_)
				{
					if (playlistEntry.Playlist_.GetLevelSetID() == data.levelSetID &&
						playlistEntry.type_ != LevelGridMenu.PlaylistEntry.Type.Error)
					{
						data.playlist.Name_.DecodeNGUIColorTag(out string labelName);
						playlistEntry.labelText_ = labelName;
						LevelGridPlaylistButton playlistButton = this.menu_.buttonList_.EntryToButton(playlistEntry) as LevelGridPlaylistButton;

						// Calling this has small visual side effects where the the button will be un-faded
						//  when coming back to the LevelGridGrid (until popping to the LevelGridMenu).
						// But using this method is a good alternative, if other mods are overwriting the `OnDisplayedVirtual` behavior.
						//playlistButton.OnDisplayedVirtual();

						Color color = playlistEntry.Color_;
						if (!playlistEntry.unlocked_)
						{
							color *= 0.425f;
						}
						playlistButton.name_.color = color;
						UIButton uiButton = playlistButton.GetComponent<UIButton>();
						uiButton.defaultColor = color;
						uiButton.hover = color;
						if (playlistEntry.type_ != LevelGridMenu.PlaylistEntry.Type.Error)
						{
							color.a = 0.25f;
						}
						uiButton.disabledColor = color;
						playlistButton.SetNameLabel(playlistEntry.labelText_);
						
					}
				}
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in OnPlaylistNameChanged()");
				Mod.Instance.Logger.Exception(ex);
			}
		}

		private void OnPlaylistColorChanged(PlaylistColorChanged.Data data)
		{
			try
			{
				foreach (LevelGridMenu.PlaylistEntry playlistEntry in this.menu_.ScrollableEntries_)
				{
					if (playlistEntry.Playlist_.GetLevelSetID() == data.levelSetID &&
						playlistEntry.type_ != LevelGridMenu.PlaylistEntry.Type.Error)
					{
						data.playlist.Name_.DecodeNGUIColorTag(out string labelName);
						playlistEntry.labelText_ = labelName;
						LevelGridPlaylistButton playlistButton = this.menu_.buttonList_.EntryToButton(playlistEntry) as LevelGridPlaylistButton;

						// Calling this has small visual side effects where the the button will be un-faded
						//  when coming back to the LevelGridGrid (until popping to the LevelGridMenu).
						// But using this method is a good alternative, if other mods are overwriting the `OnDisplayedVirtual` behavior.
						//playlistButton.OnDisplayedVirtual();

						Color color = playlistEntry.Color_;
						if (!playlistEntry.unlocked_)
						{
							color *= 0.425f;
						}
						playlistButton.name_.color = color;
						UIButton uiButton = playlistButton.GetComponent<UIButton>();
						uiButton.defaultColor = color;
						uiButton.hover = color;
						if (playlistEntry.type_ != LevelGridMenu.PlaylistEntry.Type.Error)
						{
							color.a = 0.25f;
						}
						uiButton.disabledColor = color;
						playlistButton.SetNameLabel(playlistEntry.labelText_);
					}
				}
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in OnPlaylistColorChanged()");
				Mod.Instance.Logger.Exception(ex);
			}
		}
	}
}
