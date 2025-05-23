using Distance.LevelSelectAdditions.Extensions;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Scripts
{
	public class LevelPlaylistSelectRenameLogic : MonoBehaviour
	{
		private LevelSelectMenuLogic levelSelectMenu_;


		private void Awake()
		{
			levelSelectMenu_ = GetComponentInParent<LevelSelectMenuLogic>();
			if (!levelSelectMenu_)
			{
				Mod.Log.LogError(nameof(LevelSelectMenuLogic) + " component not found");
				return;
			}

			SetupPlaylistRenameButton();
		}

		//  __________________________________
		// | QUICK PLAYLIST/playlist name     |
		// |__________________________________|
		// | Playlist track                   |
		// | Playlist track                   |
		// | ...                              |
		// | Playlist track                   |
		// |__________________________________|
		//  __________________________________
		// |_______|  Save  ||  Load  |_______|
		//
		//     V  change bottom bar into  V
		//  __________________________________
		// |__|  Save  ||  Load  || Rename |__|
		//
		private void SetupPlaylistRenameButton()
		{
			// Full path: "LevelSelectRoot/Panel - Level Select/Anchor - Center/Left Panel/PlaylistButtonGroup2"
			GameObject playlistButtonGroup2 = transform.Find("Panel - Level Select/Anchor - Center/Left Panel/PlaylistButtonGroup2")?.gameObject;
			if (!playlistButtonGroup2)
			{
				Mod.Log.LogError("\"PlaylistButtonGroup2\" game object not found");
				return;
			}

			GameObject saveButton = null, loadButton = null;
			foreach (var child in playlistButtonGroup2.GetChildren())
			{
				if (child.name == "SaveButton")
				{
					saveButton = child;
				}
				else if (child.name == "LoadButton")
				{
					loadButton = child;
				}
			}

			if (!saveButton || !loadButton)
			{
				Mod.Log.LogError("\"SaveButton\" and/or \"LoadButton\" component not found");
				return;
			}

			// Create a copy of LoadButton to be our new rename button.
			var renameButton = Instantiate(loadButton, loadButton.transform.parent);
			renameButton.name = "PlaylistRenameButton"; // Less ambiguous name.

			// Reposition the buttons (to the left) to make room for our new button.
			//  __________________________________
			// |_______|  Save  ||  Load  |_______|
			//
			//     V  change bottom bar into  V
			//  __________________________________
			// |__|  Save  ||  Load  || Rename |__|

			// Position changes could be done with localPosition or *just* position... maybe(?)
			Vector3 savePos = saveButton.transform.localPosition;
			Vector3 loadPos = loadButton.transform.localPosition;
			Vector3 renamePos = loadPos;// renameButton.transform.localPosition;

			//float diff = loadPos.x; // difference from center of panel (same as below)
			float diff = (loadPos.x - savePos.x) / 2f; // difference from center of buttons
			savePos.x   -= diff; // panel center left  => far left
			loadPos.x   -= diff; // panel center right => center
			renamePos.x += diff; // panel center right => far right

			saveButton.transform.localPosition = savePos;
			loadButton.transform.localPosition = loadPos;
			renameButton.transform.localPosition = renamePos;


			// What does this actually do? Is it necessary?
			//  (This is coppied from Gsl.Centrifuge.Distance's `VersionNumber`)
			renameButton.ForEachChildObjectDepthFirstRecursive((obj) => {
				obj.SetActive(true);
			});

			// Change the label text for our button.
			// This function does the same thing as the 2 lines below it.
			//LevelSelectMenuLogic.SetChildLabelText(renameButton, "Rename");
			UILabel label = renameButton.GetComponentInChildren<UILabel>();
			// Use "Name" over "Rename" to have a stronger distinction between renaming the file vs. the display name.
			label.text = "Name"; // "Rename";


			// Change the `onClick` event for our button.
			// There are two UIButton components in here (for foreground and background).
			// We want to find the one with the `onClick` event assigned.
			foreach (var button in renameButton.GetComponents<UIButton>())
			{
				if (button.onClick != null && button.onClick.Count > 0)
				{
					button.onClick.Clear();
					button.onClick.Add(new EventDelegate(OnPlaylistRenameButtonClicked));

					break;
				}
			}
		}

		private void OnPlaylistRenameButtonClicked()
		{
			levelSelectMenu_.tempPlaylist_.PromptRename(OnPlaylistRenameSubmit, null, false);
		}

		private void OnPlaylistRenameSubmit(bool changed)
		{
			levelSelectMenu_.UpdateQuickPlaylistText();
		}
	}
}
