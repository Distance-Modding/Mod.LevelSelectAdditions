using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patches to store extra data that was used to create level playlists.
	/// This is also handled for the <see cref="LevelPlaylist.Load"/> function.
	/// </summary>
	/// <remarks>
	/// Required For: QUICK PLAYLIST Rename button, Level Set Options menu.
	/// </remarks>
	internal static class LevelPlaylist__Create
	{
		[HarmonyPatch(typeof(LevelPlaylist), nameof(LevelPlaylist.Create), typeof(bool))]
		internal static class LevelPlaylist__Create_overload0
		{
			[HarmonyPostfix]
			internal static void Postfix(ref LevelPlaylist __result/*, bool isCustom*/)
			{
				var playlistData = __result.gameObject.AddComponent<LevelPlaylistCompoundData>();

				playlistData.Playlist = __result;
			}
		}

		// Normally the below Create functions calls LevelPlaylist.Create(bool) underneath,
		// but still add the component in-case another mod changes that.

		[HarmonyPatch(typeof(LevelPlaylist), nameof(LevelPlaylist.Create), typeof(LevelSet), typeof(string))]
		internal static class LevelPlaylist__Create_overload1
		{
			[HarmonyPostfix]
			internal static void Postfix(ref LevelPlaylist __result/*, LevelSet set, string name*/)
			{
				var playlistData = __result.gameObject.GetOrAddComponent<LevelPlaylistCompoundData>();

				playlistData.Playlist = __result;
			}
		}

		[HarmonyPatch(typeof(LevelPlaylist), nameof(LevelPlaylist.Create), typeof(LevelSet), typeof(string), typeof(GameModeID))]
		internal static class LevelPlaylist__Create_overload2
		{
			[HarmonyPostfix]
			internal static void Postfix(ref LevelPlaylist __result/*, LevelSet set, string name*/, GameModeID customGameModeID)
			{
				var playlistData = __result.gameObject.GetOrAddComponent<LevelPlaylistCompoundData>();

				playlistData.Playlist = __result;
				playlistData.CustomGameModeID = customGameModeID;
			}
		}

		[HarmonyPatch(typeof(LevelPlaylist), nameof(LevelPlaylist.Create), typeof(LevelSet), typeof(string), typeof(LevelGroupFlags))]
		internal static class LevelPlaylist__Create_overload3
		{
			[HarmonyPostfix]
			internal static void Postfix(ref LevelPlaylist __result/*, LevelSet set, string name*/, LevelGroupFlags flags)
			{
				var playlistData = __result.gameObject.GetOrAddComponent<LevelPlaylistCompoundData>();

				playlistData.Playlist = __result;
				playlistData.LevelGroupFlags = flags;
			}
		}
	}
}