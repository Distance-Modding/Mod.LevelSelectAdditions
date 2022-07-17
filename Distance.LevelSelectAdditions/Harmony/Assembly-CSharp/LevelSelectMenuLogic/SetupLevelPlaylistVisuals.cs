using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to fix "QUICK PLAYLIST..." text overwriting playlist name after adding a level (even in vanilla).
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.SetupLevelPlaylistVisuals))]
	internal static class LevelSelectMenuLogic__SetupLevelPlaylistVisuals
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelSelectMenuLogic __instance)
		{
			// Ensure our compound data component is attached.
			__instance.tempPlaylist_.gameObject.GetOrAddComponent<LevelPlaylistCompoundData>();

			if (!__instance.tempPlaylist_.Name_.IsEmptyPlaylistName())
			{
				__instance.quickPlaylistLabel_.text = __instance.tempPlaylist_.Name_;
			}
		}
	}
}