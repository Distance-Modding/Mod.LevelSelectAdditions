using Distance.LevelSelectAdditions.Extensions;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Scripts
{
	public class LevelGridButtonCurrentMainMenuLogic : MonoBehaviour
	{
		public const string PlaylistIconSpriteName = "Play";
		public const string CellIconSpriteName = "CameraIcon";
		private const int PlaylistIconSize = 14; // or 16
		private const int CellIconSize = 28;
		private const int CellIconLocation = 2; // 0 = TopLeft (unplayed), 1 = TopRight, 2 = BottomRight (medal)

		public const bool ShowIconImprint   = false;  // Show a an empty slot for where the camera icon would be.
		public const bool ShowIconHighlight = false; // Color the camera icon black when highlighted.
		public const bool ShowIconShadow    = true;  // Show a bottom-right shadow under the camera icon.

		private static readonly Color IconColor = new Color(0.6235f, 0.9059f, 0.502f); // green, rgb(159,231,128)
		private static readonly Color IconImprintColor = new Color(0.286f, 0.286f, 0.286f, 0.298f); // gray, rgba(73,73,73,76)
		private static readonly Color IconHighlightColor = Color.black;
		private static readonly Color IconShadowColor = new Color(0f, 0f, 0f, 0.502f); // transparent, rgba(0,0,0,128)
		

		public LevelGridCell GridCell { get; internal set; }
		public LevelGridPlaylistButton PlaylistButton { get; internal set; }
		public LevelGridMenu LevelGridMenu { get; internal set; }
		public bool IsPlaylist { get; internal set; }

		public bool IsCurrentMainMenu { get; internal set; }

		public bool IsCoveringUnplayedCircle => this.IsCurrentMainMenu && (this.IsPlaylist || CellIconLocation == 0);

		public InterpolateUIPanelAlphaLogic panelInterp_;
		public UISprite iconSprite_;
		public UISprite iconSpriteShadow_;


		public void UpdateCurrentMainMenuIcon()
		{
			this.panelInterp_.Reset(1f);

			bool isMainMenu = this.LevelGridMenu.DisplayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
			if (isMainMenu)
			{
				Profile profile = G.Sys.ProfileManager_.CurrentProfile_;
				string relativePath, currentRelativePath;

				if (this.IsPlaylist)
				{
					LevelGridMenu.PlaylistEntry playlistEntry = this.PlaylistButton.entry_ as LevelGridMenu.PlaylistEntry;
					relativePath = playlistEntry.Playlist_.GetRelativePathID();
					currentRelativePath = Mod.Instance.Config.GetProfileMainMenuRelativePathID(profile.Name_);
				}
				else
				{
					LevelGridGrid.LevelEntry levelEntry = this.GridCell.entry_ as LevelGridGrid.LevelEntry;
					relativePath = levelEntry.levelInfo_.relativePath_;
					currentRelativePath = profile.MainMenuLevelRelativePath_;
				}

				this.IsCurrentMainMenu = currentRelativePath == relativePath;

				if (this.IsCurrentMainMenu)
				{
					this.iconSprite_.color = IconColor;
				}
				else if (ShowIconImprint && !this.IsPlaylist)
				{
					this.iconSprite_.color = IconImprintColor;
				}

				this.iconSpriteShadow_?.gameObject.SetActive(ShowIconShadow && !this.IsPlaylist && this.IsCurrentMainMenu);
				this.iconSprite_.gameObject.SetActive((ShowIconImprint && !this.IsPlaylist) || this.IsCurrentMainMenu);

				// Ensure the iconSprite ALWAYS draws above iconSpriteShadow.
				if (this.iconSpriteShadow_)
				{
					this.iconSprite_.depth = 6;
					this.iconSpriteShadow_.depth = 5;
				}
			}
			else
			{
				this.IsCurrentMainMenu = false;
				this.iconSpriteShadow_?.gameObject.SetActive(false);
				this.iconSprite_.gameObject.SetActive(false);
			}
		}

		private void Update()
		{
			if (!this.IsPlaylist && this.iconSprite_ && this.IsCurrentMainMenu)
			{
				// Don't waste your time, entry_.isSelected_ is never true.
				if (this.GridCell.buttonList_.selectedEntry_ == this.GridCell.entry_ && ShowIconHighlight)
				{
					this.iconSprite_.color = IconHighlightColor;
				}
				else
				{
					this.iconSprite_.color = IconColor;
				}
			}
		}

		private bool SetupCurrentMainMenuIcon(GameObject unplayedCircle)
		{
			GameObject newUnplayedCircle = UnityEngine.Object.Instantiate(unplayedCircle, unplayedCircle.transform.parent);
			newUnplayedCircle.name = "CurrentMainMenuIcon";

			this.panelInterp_ = newUnplayedCircle.GetComponent<InterpolateUIPanelAlphaLogic>();
			GameObject newCircle = newUnplayedCircle.transform.Find("Circle").gameObject;
			if (!newCircle)
			{
				Mod.Instance.Logger.Error("\"Circle\" game object not found");
				return false;
			}

			newCircle.name = "IconSprite";

			// Setup our icon.
			this.iconSprite_ = newCircle.GetComponent<UISprite>();
			this.iconSprite_.color = IconColor;

			if (this.IsPlaylist)
			{
				this.iconSprite_.spriteName = PlaylistIconSpriteName;
				this.iconSprite_.width  = PlaylistIconSize;
				this.iconSprite_.height = PlaylistIconSize;

				// Reposition our icon.
				Vector3 mainMenuPos = this.panelInterp_.gameObject.transform.position;
				// Better alignment for camera icon at new size (we can't go too far left since it'll clip).
				// Old (relative to Button 0003): -1.4475 0.3825 0
				// New (relative to Button 0003): -1.445 0.385 0
				mainMenuPos.x += 0.0025f;
				mainMenuPos.y += 0.0025f;
				this.panelInterp_.gameObject.transform.position = mainMenuPos;
			}
			else
			{
				this.iconSprite_.spriteName = CellIconSpriteName;
				this.iconSprite_.width  = CellIconSize;
				this.iconSprite_.height = CellIconSize;

				// Reposition our icon.
				// Better alignment for camera icon at new size, and move to the top-left corner (instead of top-right).
				// Old (relative to Button 0012): -0.375 -0.4325 0
				// New (relative to Button 0012): -0.0075 -0.45 0
				// NOTE: If we use the alt position (covers the medal logo), the icon will be hard to see when the cell is highlighted.
				// Alt (relative to Button 0012): -0.0075 -0.705 0

				// 0 = TopLeft (unplayed), 1 = TopRight, 2 = BottomRight (medal)
				Vector3 mainMenuPos = this.panelInterp_.gameObject.transform.position;
				if (CellIconLocation == 0)
				{
					mainMenuPos.x += 0.0150f; // Left
				}
				else
				{
					mainMenuPos.x += 0.3675f; // Right
				}
				if (CellIconLocation == 0 || CellIconLocation == 1)
				{
					mainMenuPos.y -= 0.0175f; // Top
				}
				else
				{
					mainMenuPos.y -= 0.2725f; // Bottom
				}
				this.panelInterp_.gameObject.transform.position = mainMenuPos;


				// Create a shadow that sits behind the icon to the bottom right by 1.5 pixels.
				if (ShowIconShadow)
				{

					GameObject newCircleShadow = UnityEngine.Object.Instantiate(newCircle, newCircle.transform.parent);
					newCircleShadow.name = "IconSpriteShadow";

					this.iconSpriteShadow_ = newCircleShadow.GetComponent<UISprite>();

					this.iconSpriteShadow_.color = IconShadowColor;
					this.iconSpriteShadow_.spriteName = CellIconSpriteName;
					this.iconSpriteShadow_.width = CellIconSize;
					this.iconSpriteShadow_.height = CellIconSize;

					Vector3 shadowPos = this.iconSpriteShadow_.transform.localPosition;
					shadowPos.x += 1.5f; // 1f;
					shadowPos.y -= 1.5f; // 1f;
					this.iconSpriteShadow_.transform.localPosition = shadowPos;
				}
			}

			// What does this actually do? Is it necessary?
			//  (This is coppied from Gsl.Centrifuge.Distance's `VersionNumber`)
			newUnplayedCircle.ForEachChildObjectDepthFirstRecursive((obj) => {
				obj.SetActive(true);
			});

			this.iconSpriteShadow_?.gameObject.SetActive(false);
			this.iconSprite_.gameObject.SetActive(false);

			return true;
		}

		public static LevelGridButtonCurrentMainMenuLogic GetOrCreate(LevelGridCell gridCell)
		{
			var compoundData = gridCell.GetComponent<LevelGridButtonCurrentMainMenuLogic>();
			if (!compoundData)
			{
				// First-time setup
				LevelGridGrid.LevelEntry entry = gridCell.entry_ as LevelGridGrid.LevelEntry;

				compoundData = gridCell.gameObject.AddComponent<LevelGridButtonCurrentMainMenuLogic>();
				compoundData.GridCell = gridCell;
				compoundData.LevelGridMenu = entry.levelGridMenu_;
				compoundData.IsPlaylist = false;

				compoundData.SetupCurrentMainMenuIcon(gridCell.panelInterp_.gameObject);
			}

			return compoundData;
		}


		public static LevelGridButtonCurrentMainMenuLogic GetOrCreate(LevelGridPlaylistButton playlistButton)
		{
			var compoundData = playlistButton.GetComponent<LevelGridButtonCurrentMainMenuLogic>();
			if (!compoundData)
			{
				// First-time setup
				LevelGridMenu.PlaylistEntry entry = playlistButton.entry_ as LevelGridMenu.PlaylistEntry;

				compoundData = playlistButton.gameObject.AddComponent<LevelGridButtonCurrentMainMenuLogic>();
				compoundData.PlaylistButton = playlistButton;
				compoundData.LevelGridMenu = entry.levelGridMenu_;
				compoundData.IsPlaylist = true;

				compoundData.SetupCurrentMainMenuIcon(playlistButton.panelInterp_.gameObject);
			}

			return compoundData;
		}
	}
}
