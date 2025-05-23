using HarmonyLib;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to prevent starting a loaded Quick Playlist when in the Choose Main Menu display type.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.StartPlaylist))]
	internal static class LevelSelectMenuLogic__StartPlaylist
	{
		[HarmonyPrefix]
		internal static bool Prefix(LevelSelectMenuLogic __instance)
		{
			if (__instance.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
			{
				return false; // Prevent starting a playlist in Choose Main Menu level display.
			}

			return true; // Fallthrough to normal method behavior.
		}
	}
}
