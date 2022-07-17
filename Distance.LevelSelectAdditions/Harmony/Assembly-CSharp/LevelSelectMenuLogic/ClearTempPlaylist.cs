using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to stop tempPlaylist_ from preserving its previous name. This can effect using the Rename button,
	/// as well as the initial "QUICK PLAYLIST..." text due to the SetupLevelPlaylistVisuals patch.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.ClearTempPlaylist))]
	internal static class LevelSelectMenuLogic__ClearTempPlaylist
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelSelectMenuLogic __instance)
		{
			__instance.tempPlaylist_.Name_ = nameof(LevelPlaylist); // restore default uninitialized name

			var playlistData = __instance.tempPlaylist_.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData)
			{
				playlistData.FilePath = null;
			}
		}
	}
}