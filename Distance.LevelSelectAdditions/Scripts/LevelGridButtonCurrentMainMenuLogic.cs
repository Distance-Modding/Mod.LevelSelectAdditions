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

		public bool IsCoveringUnplayedCircle => IsCurrentMainMenu && (IsPlaylist || CellIconLocation == 0);

		public InterpolateUIPanelAlphaLogic panelInterp_;
		public UISprite iconSprite_;
		public UISprite iconSpriteShadow_;


		public void UpdateCurrentMainMenuIcon()
		{
			panelInterp_.Reset(1f);

			bool isMainMenu = LevelGridMenu.DisplayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;
			if (isMainMenu)
			{
				Profile profile = G.Sys.ProfileManager_.CurrentProfile_;
				string relativePath, currentRelativePath;

				if (IsPlaylist)
				{
					LevelGridMenu.PlaylistEntry playlistEntry = PlaylistButton.entry_ as LevelGridMenu.PlaylistEntry;
					relativePath = playlistEntry.Playlist_.GetRelativePathID();
					currentRelativePath = Mod.Instance.GetProfileMainMenuRelativePathID(profile.Name_);
				}
				else
				{
					LevelGridGrid.LevelEntry levelEntry = GridCell.entry_ as LevelGridGrid.LevelEntry;
					relativePath = levelEntry.levelInfo_.relativePath_;
					currentRelativePath = profile.MainMenuLevelRelativePath_;
				}

				IsCurrentMainMenu = currentRelativePath == relativePath;

				if (IsCurrentMainMenu)
				{
					iconSprite_.color = IconColor;
				}
				else if (ShowIconImprint && !IsPlaylist)
				{
					iconSprite_.color = IconImprintColor;
				}

				iconSpriteShadow_?.gameObject.SetActive(ShowIconShadow && !IsPlaylist && IsCurrentMainMenu);
				iconSprite_.gameObject.SetActive((ShowIconImprint && !IsPlaylist) || IsCurrentMainMenu);

				// Ensure the iconSprite ALWAYS draws above iconSpriteShadow.
				if (iconSpriteShadow_)
				{
					iconSprite_.depth = 6;
					iconSpriteShadow_.depth = 5;
				}
			}
			else
			{
				IsCurrentMainMenu = false;
				iconSpriteShadow_?.gameObject.SetActive(false);
				iconSprite_.gameObject.SetActive(false);
			}
		}

		private void Update()
		{
			if (!IsPlaylist && iconSprite_ && IsCurrentMainMenu)
			{
				// Don't waste your time, entry_.isSelected_ is never true.
				if (GridCell.buttonList_.selectedEntry_ == GridCell.entry_ && ShowIconHighlight)
				{
					iconSprite_.color = IconHighlightColor;
				}
				else
				{
					iconSprite_.color = IconColor;
				}
			}
		}

		private bool SetupCurrentMainMenuIcon(GameObject unplayedCircle)
		{
			GameObject newUnplayedCircle = UnityEngine.Object.Instantiate(unplayedCircle, unplayedCircle.transform.parent);
			newUnplayedCircle.name = "CurrentMainMenuIcon";

			panelInterp_ = newUnplayedCircle.GetComponent<InterpolateUIPanelAlphaLogic>();
			GameObject newCircle = newUnplayedCircle.transform.Find("Circle").gameObject;
			if (!newCircle)
			{
				Mod.Log.LogError("\"Circle\" game object not found");
				return false;
			}

			newCircle.name = "IconSprite";

			// Setup our icon.
			iconSprite_ = newCircle.GetComponent<UISprite>();
			iconSprite_.color = IconColor;

			if (IsPlaylist)
			{
				iconSprite_.spriteName = PlaylistIconSpriteName;
				iconSprite_.width  = PlaylistIconSize;
				iconSprite_.height = PlaylistIconSize;

				// Reposition our icon.
				Vector3 mainMenuPos = panelInterp_.gameObject.transform.position;
				// Better alignment for camera icon at new size (we can't go too far left since it'll clip).
				// Old (relative to Button 0003): -1.4475 0.3825 0
				// New (relative to Button 0003): -1.445 0.385 0
				mainMenuPos.x += 0.0025f;
				mainMenuPos.y += 0.0025f;
				panelInterp_.gameObject.transform.position = mainMenuPos;
			}
			else
			{
				iconSprite_.spriteName = CellIconSpriteName;
				iconSprite_.width  = CellIconSize;
				iconSprite_.height = CellIconSize;

				// Reposition our icon.
				// Better alignment for camera icon at new size, and move to the top-left corner (instead of top-right).
				// Old (relative to Button 0012): -0.375 -0.4325 0
				// New (relative to Button 0012): -0.0075 -0.45 0
				// NOTE: If we use the alt position (covers the medal logo), the icon will be hard to see when the cell is highlighted.
				// Alt (relative to Button 0012): -0.0075 -0.705 0

				// 0 = TopLeft (unplayed), 1 = TopRight, 2 = BottomRight (medal)
				Vector3 mainMenuPos = panelInterp_.gameObject.transform.position;
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
				panelInterp_.gameObject.transform.position = mainMenuPos;


				// Create a shadow that sits behind the icon to the bottom right by 1.5 pixels.
				if (ShowIconShadow)
				{

					GameObject newCircleShadow = UnityEngine.Object.Instantiate(newCircle, newCircle.transform.parent);
					newCircleShadow.name = "IconSpriteShadow";

					iconSpriteShadow_ = newCircleShadow.GetComponent<UISprite>();

					iconSpriteShadow_.color = IconShadowColor;
					iconSpriteShadow_.spriteName = CellIconSpriteName;
					iconSpriteShadow_.width = CellIconSize;
					iconSpriteShadow_.height = CellIconSize;

					Vector3 shadowPos = iconSpriteShadow_.transform.localPosition;
					shadowPos.x += 1.5f; // 1f;
					shadowPos.y -= 1.5f; // 1f;
					iconSpriteShadow_.transform.localPosition = shadowPos;
				}
			}

			// What does this actually do? Is it necessary?
			//  (This is coppied from Gsl.Centrifuge.Distance's `VersionNumber`)
			newUnplayedCircle.ForEachChildObjectDepthFirstRecursive((obj) => {
				obj.SetActive(true);
			});

			iconSpriteShadow_?.gameObject.SetActive(false);
			iconSprite_.gameObject.SetActive(false);

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
