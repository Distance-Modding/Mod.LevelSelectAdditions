using Distance.LevelSelectAdditions.Extensions;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to stop tempPlaylist_ from preserving its previous name. This can effect using the Rename button,
	/// as well as the initial "QUICK PLAYLIST..." text due to the SetupLevelPlaylistVisuals patch.
	/// <para/>
	/// Also includes patch to hide unused Leaderboards (and optionally Playlist Mode) buttons when in the Choose Main Menu display type.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.ClearTempPlaylist))]
	internal static class LevelSelectMenuLogic__ClearTempPlaylist
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelSelectMenuLogic __instance)
		{
			__instance.ResetTempPlaylistState();

			// We're exiting Playlist Mode, so we need to re-evaluate bottom left button visibility.
			__instance.UpdateBottomLeftButtonVisibility();
		}
	}
}