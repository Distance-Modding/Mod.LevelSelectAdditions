using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to store extra data that was used to create level playlists.
	/// This is also handled for the <see cref="LevelPlaylist.Create"/> functions.
	/// </summary>
	/// <remarks>
	/// Required For: QUICK PLAYLIST Rename button, Level Set Options menu.
	/// </remarks>
	[HarmonyPatch(typeof(LevelPlaylist), nameof(LevelPlaylist.Load))]
	internal static class LevelPlaylist__Load
	{
		[HarmonyPostfix]
		internal static void Postfix(ref GameObject __result, string levelPlaylistPath)
		{
			var playlistData = __result.gameObject.AddComponent<LevelPlaylistCompoundData>();
			playlistData.FilePath = levelPlaylistPath;
			playlistData.Playlist = __result.GetComponent<LevelPlaylist>();
		}
	}
}
