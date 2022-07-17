using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to assign the file path and name of a loaded QUICK PLAYLIST.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.OnLevelPlaylistMenuClickLoad))]
	internal static class LevelSelectMenuLogic__OnLevelPlaylistMenuClickLoad
	{
		[HarmonyPrefix]
		internal static bool Prefix(LevelSelectMenuLogic __instance, string absolutePath)
		{
			GameObject gameObject = LevelPlaylist.Load(absolutePath);
			if (gameObject != null)
			{
				LevelPlaylist loadedPlaylist = gameObject.GetComponent<LevelPlaylist>();
				__instance.tempPlaylist_.Clear();
				__instance.tempPlaylist_.CopyFrom(loadedPlaylist);

				var playlistData = __instance.tempPlaylist_.gameObject.GetOrAddComponent<LevelPlaylistCompoundData>();
				playlistData.FilePath = absolutePath;
				playlistData.Playlist = __instance.tempPlaylist_;

				__instance.SetupLevelPlaylistVisuals();

				__instance.tempPlaylist_.Name_ = loadedPlaylist.Name_;
				__instance.quickPlaylistLabel_.text = loadedPlaylist.Name_;

				G.Sys.MenuPanelManager_.Pop(false);
			}

			return false;
		}
	}
}