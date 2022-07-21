using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to override LevelPlaylist saving to prevent changing the filename to the name of the playlist.
	/// </summary>
	/// <remarks>
	/// Required For: QUICK PLAYLIST Rename button, Level Set Options menu Rename/Recolor buttons.
	/// </remarks>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.OnLevelPlaylistMenuClickSave))]
	internal static class LevelSelectMenuLogic__OnLevelPlaylistMenuClickSave
	{
		[HarmonyPrefix]
		internal static bool Prefix(LevelSelectMenuLogic __instance, string absolutePath)
		{
			string curFile  = Resource.GetFileName(absolutePath);
			string curLabel = __instance.quickPlaylistLabel_.text;
			string curName  = __instance.tempPlaylist_.Name_;
			Mod.Instance.Logger.Debug($"Saving playlist file:  \"{curFile}\"");
			Mod.Instance.Logger.Debug($"Saving playlist label: " + (curLabel != null ? $"\"{curLabel}\"" : "null"));
			Mod.Instance.Logger.Debug($"Saving playlist Name:  " + (curName  != null ? $"\"{curName}\"" : "null"));


			// If our playlist is unnamed, then assign a display name based on the file name.
			if (__instance.tempPlaylist_.Name_.IsEmptyPlaylistName())
			{
				string newName = Resource.GetFileNameWithoutExtension(absolutePath);
				__instance.tempPlaylist_.Name_ = newName;

				__instance.UpdateQuickPlaylistText();

				Mod.Instance.Logger.Debug($"Saving playlist New Name:  " + (newName  != null ? $"\"{newName}\"" : "null"));
			}

			var playlistData = __instance.tempPlaylist_.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData)
			{
				playlistData.FilePath = absolutePath;
				playlistData.Playlist = __instance.tempPlaylist_;
			}

			__instance.tempPlaylist_.Save();
			G.Sys.MenuPanelManager_.Pop(false);

			return false;
		}
	}
}