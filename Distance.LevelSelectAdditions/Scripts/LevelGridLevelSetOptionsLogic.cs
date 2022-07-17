#pragma warning disable IDE0051
using Distance.LevelSelectAdditions.Extensions;
using System;
using UnityEngine;

using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.LevelSelectAdditions.Scripts
{
	public class LevelGridLevelSetOptionsLogic : MonoBehaviour
	{
		private LevelGridGrid grid_;


		private void Awake()
		{
			this.grid_ = this.GetComponentInParent<LevelGridGrid>();
			if (!this.grid_)
			{
				Mod.Instance.Logger.Error(nameof(LevelGridGrid) + " component not found");
			}
		}

		private void Update()
		{
			if (/*this.entries_ == null || this.entries_.Count == 0 ||*/ !this.grid_.isGridPushed_)
			{
				return;
			}

			if (Mod.Instance.Config.EnableLevelSetOptionsMenu)
			{
				if (!this.grid_.playlist_.IsResourcesPlaylist() && G.Sys.InputManager_.GetKeyUp(InputAction.MenuStart))
				{
					//string levelSetID = this.grid_.playlist_.GetLevelSetID();
					//LevelGridMenu.PlaylistEntry playlistEntry = this.grid_.levelGridMenu_.ScrollableEntries_.Find((entry) => entry.Playlist_.GetLevelSetID() == levelSetID);
					//var sortMenu = Mod.Instance.GetLevelSetOptionsMenu(this.grid_.playlist_);
					Mod.Instance.LevelSetOptionsMenu.Show(
						this.grid_.levelGridMenu_.displayType_,
						this.grid_.levelGridMenu_.modeID_,
						this.grid_.playlist_,
						//playlistEntry,
						OnDeletePlaylist);
				}
			}
		}

		private void OnDeletePlaylist()
		{
			G.Sys.MenuPanelManager_.Pop(false);
			this.grid_.levelGridMenu_.CreateEntries();
		}
	}
}
