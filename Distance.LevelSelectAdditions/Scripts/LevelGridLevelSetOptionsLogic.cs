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
				Mod.Log.LogError(nameof(LevelGridGrid) + " component not found");
			}
		}

		private void Update()
		{
			if (/*this.entries_ == null || this.entries_.Count == 0 ||*/ !grid_.isGridPushed_)
			{
				return;
			}

			if (Mod.EnableLevelSetOptionsMenu.Value)
			{
				bool isMainMenu = grid_.levelGridMenu_.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
				if ((!grid_.playlist_.IsResourcesPlaylist() || isMainMenu) && G.Sys.InputManager_.GetKeyUp(InputAction.MenuStart))
				{
					//string levelSetID = this.grid_.playlist_.GetLevelSetID();
					//LevelGridMenu.PlaylistEntry playlistEntry = this.grid_.levelGridMenu_.ScrollableEntries_.Find((entry) => entry.Playlist_.GetLevelSetID() == levelSetID);
					Mod.Instance.ShowLevelSetOptionsMenu(
						grid_.levelGridMenu_.displayType_,
						grid_.levelGridMenu_.modeID_,
						grid_.playlist_,
						//playlistEntry,
						OnDeletePlaylist);
				}
			}
		}

		private void OnDeletePlaylist()
		{
			G.Sys.MenuPanelManager_.Pop(false);
			grid_.levelGridMenu_.CreateEntries();
		}
	}
}
